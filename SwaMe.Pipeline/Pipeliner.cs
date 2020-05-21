using LibraryParser;
using MzmlParser;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SwaMe.Pipeline
{
    /// <summary>
    /// Orchestrates the guts of the SwaMe pipeline.
    /// </summary>
    /// <remarks>This code mostly came out of MzmlReader when it was refactored.</remarks>
    public class Pipeliner : IScanConsumer<Scan, Run<Scan>>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly CountdownEvent cde = new CountdownEvent(1);

        public CancellationToken CancellationToken { get; set; }
        public bool TicNotFound { get; private set; }
        public bool AnyEmptyBinaryArray { get; private set; }

        public bool Threading { get; set; }
        public int MaxQueueSize { get; set; } = 1000;
        public int MaxThreads { get; set; } = 0;

        bool IScanConsumer<Scan, Run<Scan>>.RequiresBinaryData => true;

        /// <param name="path">The path to the input file to be opened, or null to read from stdin</param>
        public Run<Scan> LoadMzml(string path, AnalysisSettings analysisSettings)
        {
            ScanAndRunFactory factory = new ScanAndRunFactory(analysisSettings);
            MzmlReader<Scan, Run<Scan>> parser = new MzmlReader<Scan, Run<Scan>>(factory, factory)
            {
                Threading = Threading,
                MaxThreads = MaxThreads,
                MaxQueueSize = MaxQueueSize
            };
            parser.Register(this);
            Run<Scan> run = parser.LoadMzml(path);
            run.MissingScans = 0;

            logger.Debug("Run length {0}", run.LastScanTime - run.StartTime);
            logger.Debug("{0} MS1 total scans read", run.Ms1Scans.Count);
            logger.Debug("{0} MS2 total scans read", run.Ms2Scans.Count);
            logger.Debug("{0} candidate IRT hits detected", run.IRTHits.Count);
            logger.Debug("{0} base peaks selected", run.BasePeaks.Count);

            if (MaxThreads != 0)
                ThreadPool.SetMaxThreads(MaxThreads, MaxThreads);

            FindMs2IsolationWindows(run);

            ThreadedAddBasePeakSpectra(run);

            cde.Reset(1);

            bool irt = analysisSettings.IrtLibrary != null;
            if (irt)
            {
                logger.Info("Selecting best IRT peptide candidates...");
                IrtPeptideMatcher.ChooseIrtPeptides(run);
            }
            foreach (var x in run.IRTHits.OrderBy(x => x.RetentionTime))
            {
                logger.Debug("{0} {1}", x.PeptideSequence, x.RetentionTime);
            }

            if (TicNotFound)
                logger.Warn("Total Ion Current value was not found and had to be calculated from the spectra. This will cause this value to be incorrect for centroided MZML files, peak picked data, or other manipulated spectra.");
            if (AnyEmptyBinaryArray)
                logger.Warn("One or more binary data arrays was empty and was zero filled.");

            return run;
        }

        private void ThreadedAddBasePeakSpectra(Run<Scan> run)
        {
            logger.Info("Finding base peak spectra...");
            cde.Reset(1);
            AddBasePeakSpectra(run);

            cde.Signal();
            while (cde.CurrentCount > 1)
            {
                if (CancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException("Reading MZML was cancelled");

                Thread.Sleep(500);
            }
            cde.Wait();
        }

        private static void FindIrtPeptideCandidates(Scan scan, Run<Scan> run, List<SpectrumPoint> spectrum)
        {
            foreach (Library.Peptide peptide in run.AnalysisSettings.IrtLibrary.PeptideList.Values)
            {
                var irtIntensities = new List<float>();
                var irtMzs = new List<float>();
                // Optimisation: Convert peptideTransitions to an Array at the end; if it's left as an IEnumerable then the whole expression is evaluated twice.
                Library.Transition[] peptideTransitions = run.AnalysisSettings.IrtLibrary.TransitionList.Values.OfType<Library.Transition>().Where(x => x.PeptideId == peptide.Id).ToArray();
                int transitionsLeftToSearch = peptideTransitions.Length;
                foreach (Library.Transition t in peptideTransitions)
                {
                    if (irtIntensities.Count() + transitionsLeftToSearch < run.AnalysisSettings.IrtMinPeptides)
                        break;

                    // Optimisation: Convert spectrumPoints to an Array at the end; if it's left as an IEnumerable then the whole expression is evaluated three times.
                    SpectrumPoint[] spectrumPoints = spectrum.Where(x => x.Intensity > run.AnalysisSettings.IrtMinIntensity && Math.Abs(x.Mz - t.ProductMz) < run.AnalysisSettings.IrtMassTolerance).ToArray();
                    if (spectrumPoints.Length > 0)
                    {
                        float maxIntensity = spectrumPoints.Max(x => x.Intensity);
                        irtIntensities.Add(maxIntensity);
                        irtMzs.Add(spectrumPoints.Where(x => x.Intensity == maxIntensity).First().Mz);
                    }
                    transitionsLeftToSearch--;
                }
                if (irtIntensities.Count >= run.AnalysisSettings.IrtMinPeptides)
                {
                    run.IRTHits.Add(new CandidateHit()
                    {
                        PeptideSequence = peptide.Sequence,
                        Intensities = irtIntensities,
                        ActualMzs = irtMzs,
                        RetentionTime = scan.ScanStartTime,
                        PrecursorTargetMz = peptide.AssociatedTransitions.First().PrecursorMz,
                        ProductTargetMzs = peptide.AssociatedTransitions.Select(x => x.ProductMz).ToList()
                    });
                }
            }
        }

        private void AddBasePeakSpectra(Run<Scan> run)
        {
            foreach (Scan scan in run.Ms2Scans)
            {
                if (CancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException("Reading MZML was cancelled");

                cde.AddCount();
                ThreadPool.QueueUserWorkItem(state => FindBasePeaks(run, scan));
            }
        }

        private void FindBasePeaks(Run<Scan> run, Scan scan)
        {
            foreach (BasePeak bp in run.BasePeaks.Where(x => Math.Abs(x.Mz - scan.BasePeakMz) <= run.AnalysisSettings.MassTolerance))
            {
                var temp = bp.BpkRTs.Where(x => Math.Abs(x - scan.ScanStartTime) < run.AnalysisSettings.RtTolerance);
                var spectrum = scan.Spectrum;
                if (temp.Any() && spectrum != null && spectrum.SpectrumPoints != null && spectrum.SpectrumPoints.Count > 0)
                {
                    var matching = spectrum.SpectrumPoints.Where(x => Math.Abs(x.Mz - bp.Mz) <= run.AnalysisSettings.MassTolerance);

                    if (matching != null)
                        bp.Spectrum.Add(matching.OrderByDescending(x => x.Intensity).FirstOrDefault());
                }
            }
            cde.Signal();
        }

        private static float[] FillZeroArray(float[] array)
        {
            Array.Resize(ref array, 5);
            return array;
        }

        private void FindMs2IsolationWindows(Run<Scan> run)
        {
            run.IsolationWindows = run.Ms2Scans.Select(x => (x.IsolationWindowTargetMz - x.IsolationWindowLowerOffset, x.IsolationWindowTargetMz + x.IsolationWindowUpperOffset)).Distinct().ToList();
            logger.Debug("{0} isolation windows detected: min {1} max {2}", run.IsolationWindows.Count, run.IsolationWindows.Min(x => x.Item2 - x.Item1), run.IsolationWindows.Max(x => x.Item2 - x.Item1));
        }

        void IScanConsumer<Scan, Run<Scan>>.Notify(Scan scan, float[] mzs, float[] intensities, Run<Scan> run)
        {
            if (intensities.Count() == 0)
            {
                intensities = FillZeroArray(intensities);
                mzs = FillZeroArray(mzs);
                logger.Debug("Empty binary array for a MS{0} scan in cycle number: {1}. The empty scans have been filled with zero values.", scan.MsLevel, scan.Cycle);

                run.MissingScans++;
            }
            var spectrum = intensities.Select((x, i) => new SpectrumPoint(x, mzs[i], (float)scan.ScanStartTime)).Where(x => x.Intensity >= run.AnalysisSettings.MinimumIntensity).ToList();

            //Predicted singly charged proportion:

            //The theory is that an M and M+1 pair are singly charged so we are very simply just looking for  occurences where two ions are 1 mz apart (+-massTolerance)

            //We therefore create an array cusums that accumulates the difference between ions, so for every ion we calculate the distance between that ion
            //and the previous and add that to each of the previous ions' cusum of differences. If the cusum of an ion overshoots 1 +massTolerance, we stop adding to it, if it reaches our mark we count it and stop adding to it

            List<int> indexes = new List<int>();
            float[] cusums = new float[mzs.Length];
            int movingPoint = 0;
            double minimum = 1 - 0.001;
            double maximum = 1 + 0.001;

            for (int i = 1; i < mzs.Length; i++)
            {
                float distance = mzs[i] - mzs[i - 1];
                bool matchedWithLower = false;
                for (int ii = movingPoint; ii < i; ii++)
                {
                    cusums[ii] += distance;
                    if (cusums[ii] < minimum) continue;
                    else if (cusums[ii] > minimum && cusums[ii] < maximum)
                    {
                        if (!matchedWithLower)//This is to try and minimise false positives where for example if you have an array: 351.14, 351.15, 352.14 all three get chosen.
                        {
                            indexes.Add(i);
                            indexes.Add(movingPoint);
                        }
                        movingPoint += 1;
                        matchedWithLower = true;
                        continue;
                    }
                    else if (cusums[ii] > maximum) { movingPoint += 1; }
                }
            }
            int distinct = indexes.Distinct().Count();
            int len = mzs.Length;
            scan.ProportionChargeStateOne = distinct / (double)len;

            if (scan.TotalIonCurrent == 0)
            {
                scan.TotalIonCurrent = intensities.Sum();
                TicNotFound = true;
            }
            scan.Spectrum = new Spectrum() { SpectrumPoints = spectrum };
            scan.IsolationWindowLowerBoundary = scan.IsolationWindowTargetMz - scan.IsolationWindowLowerOffset;
            scan.IsolationWindowUpperBoundary = scan.IsolationWindowTargetMz + scan.IsolationWindowUpperOffset;

            scan.Density = spectrum.Count();
            scan.BasePeakIntensity = intensities.Max();
            scan.BasePeakMz = mzs[Array.IndexOf(intensities, intensities.Max())];
            float basepeakIntensity;
            if (intensities.Count() > 0)
            {
                basepeakIntensity = intensities.Max();
                int maxIndex = intensities.ToList().IndexOf(basepeakIntensity);
                double mz = mzs[maxIndex];

                if (run.BasePeaks.Count(x => Math.Abs(x.Mz - mz) < run.AnalysisSettings.MassTolerance) < 1)//If a basepeak with this mz doesn't exist yet add it
                {
                    BasePeak bp = new BasePeak(mz, scan.ScanStartTime, basepeakIntensity);
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
                            thisbp.Intensities.Add(basepeakIntensity);
                        }
                    }
                }
            }
            else { basepeakIntensity = 0; }
            //Extract info for Basepeak chromatograms

            bool irt = run.AnalysisSettings.IrtLibrary != null;
            if (irt)
                FindIrtPeptideCandidates(scan, run, spectrum);
        }
    }
}
