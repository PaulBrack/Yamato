using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SwaMe
{
    class RTGrouper
    {
        public void DivideByRT(MzmlParser.Run run, int division, double RTDuration)
        {
            double RTsegment = RTDuration / division;
            double[] RTsegs = new double[division];

            for (int uuu = 0; uuu < division; uuu++)
            {
                RTsegs[uuu] = run.BasePeaks[0].RetentionTime + RTsegment * uuu;
            }

            //dividing basepeaks into segments
            foreach (MzmlParser.BasePeak basepeak in run.BasePeaks)
            {
                //Check to see in which RTsegment this basepeak is:
                for (int segmentboundary = 1; segmentboundary < RTsegs.Count(); segmentboundary++)
                {
                    if (basepeak.RetentionTime < RTsegs[0]) basepeak.RTsegment = 0;
                    if (basepeak.RetentionTime > RTsegs[segmentboundary - 1] && basepeak.RetentionTime < RTsegs[segmentboundary])
                    {
                        basepeak.RTsegment = segmentboundary;
                    }
                }
            }

            //dividing ms2scans into segments of RT
            foreach (MzmlParser.Scan scan in run.Ms2Scans)
            {
                //if the scan starttime falls into the rtsegment, give it the correct rtsegment number
                for (int segmentboundary = 1; segmentboundary < RTsegs.Count(); segmentboundary++)
                {
                    if (scan.ScanStartTime < RTsegs[0]) { scan.RTsegment = 0; break; }
                    else if (scan.ScanStartTime > RTsegs[segmentboundary - 1] && scan.ScanStartTime < RTsegs[segmentboundary])
                    {
                        scan.RTsegment = segmentboundary;
                        break;
                    }
                    else if (scan.ScanStartTime > RTsegs[segmentboundary] && segmentboundary == RTsegs.Count()) { scan.RTsegment = segmentboundary + 1; break; }
                }
            }

            //dividing ms1scans into segments of RT
            foreach (MzmlParser.Scan scan in run.Ms1Scans)
            {
                //Check to see in which RTsegment this basepeak is:
                for (int segmentboundary = 1; segmentboundary < RTsegs.Count(); segmentboundary++)
                {
                    if (scan.ScanStartTime < RTsegs[0]) scan.RTsegment = 0;
                    if (scan.ScanStartTime > RTsegs[segmentboundary - 1] && scan.ScanStartTime < RTsegs[segmentboundary])
                    {
                        scan.RTsegment = segmentboundary;
                    }
                }
            }

            //Retrieve TICChange metrics and divide into rtsegs
            List<double> TICchange50List = new List<double>();
            List<double> TICchangeIQRList = new List<double>();
            var TempTIC = run.Ms2Scans.GroupBy(x => x.RTsegment).Select(d => d.OrderBy(x => x.ScanStartTime).Select(g => g.TotalIonCurrent).ToList());
            for (int eee = 0; eee < TempTIC.Count(); eee++)
            {
                var Temp = TempTIC.ElementAt(eee);
                List<double> Templist = new List<double>();
                for (int uuu = 1; uuu < Temp.Count(); uuu++)
                {

                    Templist.Add(Math.Abs(Temp.ElementAt(uuu) - Temp.ElementAt(uuu - 1)));
                }
                Templist.Sort();
                TICchange50List.Add(Templist.Average());
                TICchangeIQRList.Add(InterQuartileRangeCalculator.CalcIQR(Templist));
            }

            //Calculations for peakprecision MS2:

            var meanIntensityOfAllBpks = run.BasePeaks.Select(x=>x.Intensity).Average();
            var meanMzOfAllBpks = run.BasePeaks.Select(x => x.Mz).Average();

            //Calculations for peakprecision MS1:
            var mIMS1Bpks = run.Ms1Scans.Select(x => x.BasePeakIntensity).Average();
            var mMMS1Bpks = run.Ms1Scans.Select(x => x.BasePeakMz).Average();
            List<List<double>> Peakwidths = new List<List<double>>();
            List<List<double>> PeakSymmetry = new List<List<double>>();
            List<List<double>> PeakCapacity = new List<List<double>>();
            List<List<double>> PeakPrecision = new List<List<double>>();
            List<List<double>> MS1PeakPrecision = new List<List<double>>();
            List<List<int>> MS1Density = new List<List<int>>();
            List<List<int>> MS2Density = new List<List<int>>();
            List<double> cycleTime = new List<double>();
            List<double> MS1TICTotal = new List<double>();
            List<double> MS2TICTotal = new List<double>();

            for (int segment = 0; segment < division; segment++)
            {
                List<double> PeakwidthsTemp = new List<double>();
                List<double> PeaksymTemp = new List<double>();
                List<double> PeakCapacityTemp = new List<double>();
                List<double> PeakprecisionTemp = new List<double>();
                foreach (MzmlParser.BasePeak basepeak in run.BasePeaks)
                {
                    
                    if (basepeak.RTsegment == segment)
                    {
                        PeakwidthsTemp.Add(basepeak.FWHM);
                        PeaksymTemp.Add(basepeak.Peaksym);
                        PeakprecisionTemp.Add(basepeak.Intensity / (meanIntensityOfAllBpks * Math.Pow(2,meanMzOfAllBpks / basepeak.Mz)));
                        PeakCapacityTemp.Add(basepeak.PeakCapacity);
                    }
                }

                double firstScanStartTime = 1000;
                double lastScanStartTime = 0;
                int firstCycle = 1000;
                int lastCycle = 0;
                double MS1TICTotalTemp = 0;
                List<int> MS1DensityTemp = new List<int>();
                List<double> MS1PeakprecisionTemp = new List<double>();
                
                foreach (MzmlParser.Scan scan in run.Ms1Scans)
                {
                    if (scan.RTsegment == segment)
                    {

                        MS1PeakprecisionTemp.Add(scan.BasePeakIntensity / (meanIntensityOfAllBpks * Math.Pow(2, meanMzOfAllBpks / scan.BasePeakMz)));
                        firstScanStartTime = Math.Min(firstScanStartTime, scan.ScanStartTime);
                        lastScanStartTime = Math.Max(lastScanStartTime, scan.ScanStartTime);
                        firstCycle = Math.Min(scan.Cycle, firstCycle);
                        lastCycle = Math.Max(scan.Cycle, lastCycle);
                        MS1DensityTemp.Add(scan.Density);
                        MS1TICTotalTemp += scan.TotalIonCurrent;
                    }
                }

                //To get scan speed for both ms1 and ms2 we have to also scan through ms2:
                List<int> MS2DensityTemp = new List<int>();
                double MS2TICTotalTemp = 0;
                foreach (MzmlParser.Scan scan in run.Ms2Scans)
                {
                    if (scan.RTsegment == segment)
                    {
                        firstCycle = Math.Min(scan.Cycle, firstCycle);
                        lastCycle = Math.Max(scan.Cycle, lastCycle);
                        MS2DensityTemp.Add(scan.Density);
                        MS2TICTotalTemp += scan.TotalIonCurrent;
                    }
                }

                cycleTime.Add((lastCycle - firstCycle) / (lastScanStartTime - firstScanStartTime));
                Peakwidths.Add(PeakwidthsTemp);
                PeakSymmetry.Add(PeaksymTemp);
                PeakCapacity.Add(PeakCapacityTemp);
                PeakPrecision.Add(PeakprecisionTemp);
                MS1PeakPrecision.Add(MS1PeakprecisionTemp);
                MS1Density.Add(MS1DensityTemp);
                MS2Density.Add(MS2DensityTemp);
                MS1TICTotal.Add(MS1TICTotalTemp);
                MS2TICTotal.Add(MS2TICTotalTemp);
            }
            FileMaker fm = new FileMaker { };
            fm.MakeMetricsPerRTsegmentFile(run, Peakwidths, PeakSymmetry, PeakCapacity, PeakPrecision, MS1PeakPrecision ,cycleTime, TICchange50List, TICchangeIQRList,MS1Density,MS2Density,MS1TICTotal,MS2TICTotal,division);
        }
    }
}



