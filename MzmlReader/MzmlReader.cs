using System;
using System.Linq;
using System.Collections.Generic;
using LibraryParser;

namespace MzmlParser
{
    public class MzmlReader : GenericMzmlReader<Run, Scan>
    {
        public bool ExtractBasePeaks { get; set; } = true;

        public Run LoadMzml(string path, AnalysisSettings analysisSettings)
        {
            Run run = new Run()
            {
                AnalysisSettings = analysisSettings,
                MissingScans = 0,
                StartTime = 100,
                LastScanTime = 0
            };

            ReadMzml(path, run);

            AddBasePeakSpectra(run);

            IrtPeptideMatcher.ChooseIrtPeptides(run);

            return run;
        }

        protected override void ParseBase64DataFilling(Scan scan, Run run)
        {
            PredictSinglyChargedProportion(scan);
            CheckBasePeakForScan(scan, run);
            if (null != run.AnalysisSettings.IrtLibrary)
                FindIrtPeptideCandidates(scan, run);
        }

        private static void CheckBasePeakForScan(Scan scan, Run run)
        {
            if (scan.Spectrum.Count() > 0)
            {
                double mz = scan.BasePeakMz;

                // TODO: BEWARE: There is a race condition here, as the check-and-add is not atomic.
                if (run.BasePeaks.Count(x => Math.Abs(x.Mz - mz) < run.AnalysisSettings.MassTolerance) < 1)//If a basepeak with this mz doesn't exist yet add it
                {
                    BasePeak bp = new BasePeak(mz, scan.ScanStartTime, scan.BasePeakIntensity);
                    run.BasePeaks.Add(bp);
                }
                else //we do have a match, now lets figure out if they fall within the rtTolerance
                {
                    //find out which basepeak
                    foreach (BasePeak thisbp in run.BasePeaks.Where(x => Math.Abs(x.Mz - mz) < run.AnalysisSettings.MassTolerance))
                    {
                        bool found = false;
                        for (int rt = 0; rt < thisbp.BpkRTs.Count(); rt++)
                        {
                            if (Math.Abs(thisbp.BpkRTs[rt] - scan.ScanStartTime) < run.AnalysisSettings.RtTolerance)//this is part of a previous basepeak, or at least considered to be 
                            {
                                found = true;
                                break;
                            }

                        }
                        if (!found)//This is considered to be a new instance
                        {
                            thisbp.BpkRTs.Add(scan.ScanStartTime);
                            thisbp.Intensities.Add(scan.BasePeakIntensity);
                        }
                    }
                }
            }
            //Extract info for Basepeak chromatograms
        }

        private static void PredictSinglyChargedProportion(Scan scan)
        {
            //Predicted singly charged proportion:

            //The theory is that an M and M+1 pair are singly charged so we are very simply just looking for  occurences where two ions are 1 mz apart (+-massTolerance)

            //We therefore create an array cusums that accumulates the difference between ions, so for every ion we calculate the distance between that ion
            //and the previous and add that to each of the previous ions' cusum of differences. If the cusum of an ion overshoots 1 +massTolerance, we stop adding to it, if it reaches our mark we count it and stop adding to it

            List<int> indexes = new List<int>();
            float[] cusums = new float[scan.Spectrum.Count];
            int movingPoint = 0;
            double minimum = 1 - 0.001;
            double maximum = 1 + 0.001;

            for (int i = 1; i < cusums.Length; i++)
            {
                float distance = scan.Spectrum[i].Mz - scan.Spectrum[i - 1].Mz;
                bool matchedWithLower = false;
                for (int ii = movingPoint; ii < i; ii++)
                {
                    cusums[ii] += distance;
                    if (cusums[ii] < minimum)
                        continue;
                    else if (cusums[ii] > minimum && cusums[ii] < maximum)
                    {
                        if (!matchedWithLower) //This is to try and minimise false positives where for example if you have an array: 351.14, 351.15, 352.14 all three get chosen.
                        {
                            indexes.Add(i);
                            indexes.Add(movingPoint);
                        }
                        movingPoint += 1;
                        matchedWithLower = true;
                        continue;
                    }
                    else if (cusums[ii] > maximum)
                    {
                        movingPoint += 1;
                    }
                }
            }
            int distinct = indexes.Distinct().Count();
            int len = cusums.Length;
            scan.ProportionChargeStateOne = distinct / (double)len;
        }

        private static void FindIrtPeptideCandidates(Scan scan, Run run)
        {
            foreach (Library.Peptide peptide in run.AnalysisSettings.IrtLibrary.PeptideList.Values)
            {
                var irtIntensities = new List<float>();
                var peptideTransitions = run.AnalysisSettings.IrtLibrary.TransitionList.Values.OfType<Library.Transition>().Where(x => x.PeptideId == peptide.Id);
                int transitionsLeftToSearch = peptideTransitions.Count();
                foreach (Library.Transition t in peptideTransitions)
                {
                    if (irtIntensities.Count() + transitionsLeftToSearch < run.AnalysisSettings.IrtMinPeptides)
                    {
                        break;
                    }
                    var spectrumPoints = scan.Spectrum.Where(x => x.Intensity > run.AnalysisSettings.IrtMinIntensity && Math.Abs(x.Mz - t.ProductMz) < run.AnalysisSettings.IrtMassTolerance);
                    if (spectrumPoints.Any())
                        irtIntensities.Add(spectrumPoints.Max(x => x.Intensity));
                    transitionsLeftToSearch--;

                }
                if (irtIntensities.Count >= run.AnalysisSettings.IrtMinPeptides)
                {
                    run.IRTHits.Add(new CandidateHit()
                    {
                        PeptideSequence = peptide.Sequence,
                        Intensities = irtIntensities,
                        RetentionTime = scan.ScanStartTime,
                        PrecursorTargetMz = peptide.AssociatedTransitions.First().PrecursorMz,
                        ProductTargetMzs = peptide.AssociatedTransitions.Select(x => x.ProductMz).ToList()
                    });
                }
            }
        }

        private void AddBasePeakSpectra(Run run)
        {
            ICouldRunCodeInParallel runner = Threading
                ? (ICouldRunCodeInParallel)new ThreadPoolScheduler()
                : new SingleThreadedScheduler();

            foreach (Scan scan in run.Ms2Scans)
                runner.Submit(state => FindBasePeaks(run, scan));

            runner.WaitForAll();
        }

        private static void FindBasePeaks(Run run, Scan scan)
        {
            foreach (BasePeak bp in run.BasePeaks.Where(x => Math.Abs(x.Mz - scan.BasePeakMz) <= run.AnalysisSettings.MassTolerance))
            {
                var temp = bp.BpkRTs.Where(x => Math.Abs(x - scan.ScanStartTime) < run.AnalysisSettings.RtTolerance);
                if (temp.Any())
                    bp.Spectrum.Add(scan.Spectrum.Where(x => Math.Abs(x.Mz - bp.Mz) <= run.AnalysisSettings.MassTolerance).OrderByDescending(x => x.Intensity).First());
            }
        }
    }
}
