﻿using System;
using System.Collections.Generic;
using MzmlParser;
using NLog;
using LibraryParser;
using System.IO;
using System.Linq;
using System.Globalization;

namespace IRTSearcher
{
    public class IRTPeptideMatch
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly Object Lock = new Object();
        const double irtTolerance = 0.5;

        public Run ParseLibrary(Run run, string iRTpath, double massTolerance)
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
                logger.Error(ex, "The iRT file {0} is in use. Please close the application using it and try again.", iRTpath);
            }
            run.iRTpath = iRTpath;
            run.IRTPeaks = new List<IRTPeak>();
            lock (Lock)
            {
                Library irtLibrary = new Library();
                if (run.iRTpath.ToLower().Contains("traml"))
                {
                    TraMLReader traMLReader = new TraMLReader();
                    irtLibrary = traMLReader.LoadLibrary(run.iRTpath);
                    {
                        run.IRTPeaks = new List<IRTPeak>();
                        for (int iii = 0; iii < irtLibrary.PeptideList.Count; iii++)
                        {
                            IRTPeak peak = new IRTPeak();
                            peak.AssociatedTransitions = new List<Library.Transition>();
                            peak.Spectrum = new List<SpectrumPoint>();
                            peak.TransitionRTs = new List<double>();
                            peak.PossPeaks = new List<PossiblePeak>();

                            var temp = irtLibrary.PeptideList[iii];
                            peak.ExpectedRetentionTime = ((Library.Peptide)temp).RetentionTime;
                            string Sequence = ((Library.Peptide)temp).Sequence;
                            peak.Mz = (Sequence.Count(x => x == 'A') * 71.04 + Sequence.Count(x => x == 'H') * 137.06 + Sequence.Count(x => x == 'R') * 156.10 +
                                Sequence.Count(x => x == 'K') * 128.09 + Sequence.Count(x => x == 'I') * 113.08 + Sequence.Count(x => x == 'F') * 147.07 +
                                Sequence.Count(x => x == 'L') * 113.08 + Sequence.Count(x => x == 'W') * 186.08 + Sequence.Count(x => x == 'M') * 131.04 +
                                Sequence.Count(x => x == 'P') * 97.05 + Sequence.Count(x => x == 'C') * 103.01 + Sequence.Count(x => x == 'N') * 114.04 +
                                Sequence.Count(x => x == 'V') * 99.07 + Sequence.Count(x => x == 'G') * 57.02 + Sequence.Count(x => x == 'S') * 87.03 +
                                Sequence.Count(x => x == 'Q') * 128.06 + Sequence.Count(x => x == 'Y') * 163.06 + Sequence.Count(x => x == 'D') * 115.03 +
                                Sequence.Count(x => x == 'E') * 129.04 + Sequence.Count(x => x == 'T') * 101.05 + 18.02 + 2.017) / ((Library.Peptide)temp).ChargeState;

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
                else if (run.iRTpath.Contains("csv") || run.iRTpath.Contains("tsv") || run.iRTpath.Contains("txt"))
                {
                    SVReader svReader = new SVReader();
                    irtLibrary = svReader.LoadLibrary(run.iRTpath);
                    run.IRTPeaks = new List<IRTPeak>();
                    for (int iii = 0; iii < irtLibrary.PeptideList.Count; iii++)
                    {
                        IRTPeak peak = new IRTPeak();
                        peak.Spectrum = new List<SpectrumPoint>();
                        peak.AssociatedTransitions = new List<Library.Transition>();
                        peak.TransitionRTs = new List<double>();
                        peak.PossPeaks = new List<PossiblePeak>();
                        var temp = irtLibrary.PeptideList[iii];
                        peak.Mz = double.Parse(((Library.Peptide)temp).Id.Replace(",", "."), CultureInfo.InvariantCulture);
                        for (int transition = 0; transition < irtLibrary.TransitionList.Count; transition++)
                        {
                            if (Math.Abs(((Library.Transition)irtLibrary.TransitionList[transition]).PrecursorMz - peak.Mz) < 0.02)//chose this value as the smallest difference between two biognosis peptides is this
                            {
                                peak.AssociatedTransitions.Add((Library.Transition)irtLibrary.TransitionList[transition]);
                            }
                        }
                        peak.AssociatedTransitions = ((Library.Peptide)temp).AssociatedTransitions;
                        run.IRTPeaks.Add(peak);
                    }
                }
            }

            ReadSpectrum(run, massTolerance);
            irtSearch(run, massTolerance);
            return run;
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
                            for (int ttt = 0; ttt < ip.PossPeaks.Count() - 1; ttt++)
                            {

                                if (Math.Abs(ip.PossPeaks[ttt].BasePeak.RetentionTime - temp[0].RetentionTime) < irtTolerance)// if there is already a possPeak that it can fit into then add
                                {
                                    found = true;

                                    if (ip.PossPeaks[ttt].BasePeak.Intensity < temp[0].Intensity)
                                    {
                                        //This peak is more intense and should be the basepeak of this peak
                                        ip.PossPeaks[ttt].BasePeak = temp[0];
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
            foreach (Scan scan in run.Ms2Scans)
            {
                if (scan.Spectrum !=null)
                {
                    foreach (IRTPeak peak in run.IRTPeaks)
                    {

                        if (peak.PossPeaks.Count() > 0)
                        {
                            peak.PossPeaks = peak.PossPeaks.OrderByDescending(x => x.BasePeak.Intensity).ToList();
                            for (int iii = 0; iii < peak.PossPeaks.Count() - 1; iii++)
                            {

                                if (Math.Abs(scan.ScanStartTime - peak.PossPeaks[iii].BasePeak.RetentionTime) < irtTolerance)
                                {
                                    //find if transitions are present

                                    int TransitionsMatched = 0;
                                    for (int iterator = 0; iterator < peak.AssociatedTransitions.Count(); iterator++)
                                    {
                                        int temp = scan.Spectrum.Count(x => Math.Abs(x.Mz - peak.AssociatedTransitions[iterator].ProductMz) <= massTolerance);
                                        if (temp > 0)
                                        {
                                            TransitionsMatched++;
                                        }

                                    }

                                    if (TransitionsMatched == peak.AssociatedTransitions.Count())
                                    {
                                        //Add the spectrumpoints of transitions to the transitionSpectrum of that possible peak
                                        for (int iterator = 0; iterator < TransitionsMatched; iterator++)
                                        {
                                            peak.PossPeaks[iii].Alltransitions[iterator].Add(scan.Spectrum.Where(x => Math.Abs(x.Mz - peak.AssociatedTransitions[iterator].ProductMz) <= massTolerance).First());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void irtSearch(Run run, double massTolerance)
        {
            foreach (Scan scan in run.Ms1Scans)
            {
                foreach (IRTPeak ip in run.IRTPeaks)
                {
                    if (Math.Abs(ip.RetentionTime - scan.ScanStartTime) <= irtTolerance)
                    {
                        List<SpectrumPoint> temp = scan.Spectrum.Where(x => Math.Abs(ip.Mz - x.Mz) <= massTolerance).OrderByDescending(x => x.Intensity).Take(1).ToList();
                        if (temp.Count > 0) ip.Spectrum.Add(temp[0]);
                    }
                }
            }
        }
    }
}