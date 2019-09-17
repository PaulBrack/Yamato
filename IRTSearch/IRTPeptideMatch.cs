using System;
using System.Collections.Generic;
using MzmlParser;
using NLog;
using LibraryParser;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Threading;

namespace IRTSearcher
{
    public class IRTPeptideMatch
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly Object Lock = new Object();
        const double irtTolerance = 0.5;


        private static CountdownEvent cde = new CountdownEvent(1);


        public Run ParseLibrary(Run run, string iRTpath, double massTolerance)
        {
            CheckIrtPathAccessible(iRTpath);
            run.iRTpath = iRTpath;
            run.IRTPeaks = new List<IRTPeak>();
            Library irtLibrary = new Library();
            if (run.iRTpath.EndsWith("traml", StringComparison.InvariantCultureIgnoreCase))
            {
                TraMLReader traMLReader = new TraMLReader();
                irtLibrary = traMLReader.LoadLibrary(run.iRTpath);
                {
                    run.IRTPeaks = new List<IRTPeak>();
                    for (int i = 0; i < irtLibrary.PeptideList.Count; i++)
                    {
                        IRTPeak peak = new IRTPeak();
                        Library.Peptide irtLibPeptide = (Library.Peptide)irtLibrary.PeptideList[i];
                        peak.ExpectedRetentionTime = irtLibPeptide.RetentionTime;
                        peak.Mz = GetTheoreticalMz(irtLibPeptide.Sequence, irtLibPeptide.ChargeState);

                        for (int transition = 0; transition < irtLibrary.TransitionList.Count; transition++)
                        {
                            if (Math.Abs(((Library.Transition)irtLibrary.TransitionList[transition]).PrecursorMz - peak.Mz) < 0.02)//chose this value as the smallest difference between two biognosis peptides is this
                            {
                                peak.AssociatedTransitions.Add((Library.Transition)irtLibrary.TransitionList[transition]);
                            }
                        }

                        run.IRTPeaks.Add(peak);
                        run.IRTPeaks = run.IRTPeaks.OrderBy(x => x.ExpectedRetentionTime).ToList();
                    }
                }
            }
            else if (run.iRTpath.EndsWith("csv", StringComparison.InvariantCultureIgnoreCase) || run.iRTpath.EndsWith("tsv", StringComparison.InvariantCultureIgnoreCase) || run.iRTpath.EndsWith("txt", StringComparison.InvariantCultureIgnoreCase))
            {
                SVReader svReader = new SVReader();
                irtLibrary = svReader.LoadLibrary(run.iRTpath);
                run.IRTPeaks = new List<IRTPeak>();
                for (int i = 0; i < irtLibrary.PeptideList.Count; i++)
                {
                    IRTPeak peak = new IRTPeak();
                    Library.Peptide irtLibPeptide = (Library.Peptide)irtLibrary.PeptideList[i];
                    peak.Mz = double.Parse(irtLibPeptide.Id.Replace(",", "."), CultureInfo.InvariantCulture);
                    for (int transition = 0; transition < irtLibrary.TransitionList.Count; transition++)
                    {
                        if (Math.Abs(((Library.Transition)irtLibrary.TransitionList[transition]).PrecursorMz - peak.Mz) < 0.02)//chose this value as the smallest difference between two biognosis peptides is this
                        {
                            peak.AssociatedTransitions.Add((Library.Transition)irtLibrary.TransitionList[transition]);
                        }
                    }
                    peak.AssociatedTransitions = irtLibPeptide.AssociatedTransitions;
                    run.IRTPeaks.Add(peak);
                }
            }
            foreach (IRTPeak peak in run.IRTPeaks.Where(x => x.PossPeaks.Count() > 0))
            {
                peak.PossPeaks = peak.PossPeaks.OrderByDescending(x => x.BasePeak.Intensity).ToList();
            }

            ReadSpectrum(run, massTolerance);
            irtSearch(run, massTolerance);
            return run;
        }

        private static double GetTheoreticalMz(string Sequence, int chargeState)
        {
            return (Sequence.Count(x => x == 'A') * 71.04 + Sequence.Count(x => x == 'H') * 137.06 + Sequence.Count(x => x == 'R') * 156.10 +
                Sequence.Count(x => x == 'K') * 128.09 + Sequence.Count(x => x == 'I') * 113.08 + Sequence.Count(x => x == 'F') * 147.07 +
                Sequence.Count(x => x == 'L') * 113.08 + Sequence.Count(x => x == 'W') * 186.08 + Sequence.Count(x => x == 'M') * 131.04 +
                Sequence.Count(x => x == 'P') * 97.05 + Sequence.Count(x => x == 'C') * 103.01 + Sequence.Count(x => x == 'N') * 114.04 +
                Sequence.Count(x => x == 'V') * 99.07 + Sequence.Count(x => x == 'G') * 57.02 + Sequence.Count(x => x == 'S') * 87.03 +
                Sequence.Count(x => x == 'Q') * 128.06 + Sequence.Count(x => x == 'Y') * 163.06 + Sequence.Count(x => x == 'D') * 115.03 +
                Sequence.Count(x => x == 'E') * 129.04 + Sequence.Count(x => x == 'T') * 101.05 + 18.02 + 2.017) / chargeState;
        }

        private static void CheckIrtPathAccessible(string iRTpath)
        {
            try
            {
                using (Stream stream = new FileStream(iRTpath, FileMode.Open))
                {
                    logger.Info("Starting the incorporation of iRT file: {0}. Please be patient.", iRTpath);
                }
            }
            catch (IOException ex)
            {
                logger.Error(ex, "The iRT file {0} was not able to be read - this can happen because it is in use by another program. Please close the application using it and try again.", iRTpath);
                throw ex;
            }
        }

        public static void ReadSpectrum(Run run, double massTolerance)
        {
            foreach (Scan scan in run.Ms1Scans)
            {
                //Extract info for iRT chromatograms
                foreach (IRTPeak ip in run.IRTPeaks)
                {
                    List<SpectrumPoint> temp = scan.Spectrum.Where(x => Math.Abs(ip.Mz - x.Mz) <= irtTolerance).OrderByDescending(x => x.Intensity).Take(1).ToList();
                    if (temp.Count > 0)
                    {
                        if (ip.PossPeaks.Count() > 0)
                        {
                            bool found = false;
                            for (int i = 0; i < ip.PossPeaks.Count() - 1; i++)
                            {
                                if (Math.Abs(ip.PossPeaks[i].BasePeak.RetentionTime - temp[0].RetentionTime) < irtTolerance)// if there is already a possPeak that it can fit into then add
                                {
                                    found = true;

                                    if (ip.PossPeaks[i].BasePeak.Intensity < temp[0].Intensity)
                                    {
                                        //This peak is more intense and should be the basepeak of this peak
                                        ip.PossPeaks[i].BasePeak = temp[0];
                                    }
                                }
                            }
                            if (!found)
                            {
                                PossiblePeak possPeak = new PossiblePeak();
                                possPeak.Alltransitions = new List<List<SpectrumPoint>>();
                                foreach (var at in ip.AssociatedTransitions)
                                {
                                    List<SpectrumPoint> tempList = new List<SpectrumPoint>();
                                    possPeak.Alltransitions.Add(tempList);
                                }
                                possPeak.BasePeak = temp[0];
                                ip.PossPeaks.Add(possPeak);
                            }
                        }
                        else
                        {

                            PossiblePeak possPeak = new PossiblePeak();
                            possPeak.Alltransitions = new List<List<SpectrumPoint>>();
                            foreach (var at in ip.AssociatedTransitions)
                            {
                                List<SpectrumPoint> tempList = new List<SpectrumPoint>();
                                possPeak.Alltransitions.Add(tempList);
                            }
                            possPeak.BasePeak = temp[0];
                            ip.PossPeaks.Add(possPeak);
                        }
                    }
                }
            }
            //lets try to find all the spectra where at least two transitions occur and add their RT's to a list.We can then later compare this list to the iRTPeak.spectrum.RT's
            foreach (IRTPeak peak in run.IRTPeaks.Where(x => x.PossPeaks.Count() > 0))
            {

                cde.AddCount();
                ThreadPool.QueueUserWorkItem(state => FindMatchingTransitions(run, massTolerance, peak));
                FindMatchingTransitions(run, massTolerance, peak);
            }
            cde.Signal();
            cde.Wait();

        }

        private static void FindMatchingTransitions(Run run, double massTolerance, IRTPeak peak)
        {

            foreach (PossiblePeak pp in peak.PossPeaks.OrderByDescending(x => x.BasePeak.Intensity).ToList())
            {
                var matchingscans = run.Ms2Scans.Where(x => x.Spectrum != null && x.IsolationWindowLowerBoundary <= (pp.BasePeak.Mz - massTolerance) & x.IsolationWindowUpperBoundary >= (pp.BasePeak.Mz + massTolerance));

                foreach (Scan scan in matchingscans)
                {



                }
            }
            cde.Signal();
        }

        private static void irtSearch(Run run, double massTolerance)
        {
            foreach (Scan scan in run.Ms1Scans)
            {
                foreach (IRTPeak ip in run.IRTPeaks)
                {
                    if (Math.Abs(ip.RetentionTime - scan.ScanStartTime) <= massTolerance)
                    {
                        List<SpectrumPoint> temp = scan.Spectrum.Where(x => Math.Abs(ip.Mz - x.Mz) <= massTolerance).OrderByDescending(x => x.Intensity).Take(1).ToList();
                        if (temp.Count > 0) ip.Spectrum.Add(temp[0]);
                    }
                }
            }

        }
    }
}
