using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SwaMe
{
    class RTGrouper
    {
        public class RTMetrics
        {
            public List<double> Peakwidths;
            public List<double> PeakSymmetry;
            public List<double> PeakCapacity;
            public List<double> PeakPrecision;
            public List<double> MS1PeakPrecision;
            public List<int> MS1Density;
            public List<int> MS2Density;
            public List<double> cycleTime;
            public List<double> MS1TICTotal;
            public List<double> MS2TICTotal;
            public List<double> TICchange50List;
            public List<double> TICchangeIQRList;

            public RTMetrics(List<double> MS1TICTotal, List<double> MS2TICTotal, List<double> cycleTime, List<double> TICchange50List, List<double> TICchangeIQRList, List<int> MS1Density, List<int> MS2Density, List<double> Peakwidths, List<double> PeakSymmetry, List<double> PeakCapacity, List<double> PeakPrecision, List<double> MS1PeakPrecision)
            {
                this.Peakwidths = Peakwidths;
                this.PeakSymmetry = PeakSymmetry;
                this.PeakCapacity = PeakCapacity;
                this.PeakPrecision = PeakPrecision;
                this.MS1PeakPrecision = MS1PeakPrecision;
                this.MS1Density = MS1Density;
                this.MS2Density = MS2Density;
                this.cycleTime = cycleTime;
                this.MS1TICTotal = MS1TICTotal;
                this.MS2TICTotal = MS2TICTotal;
                this.TICchange50List = TICchange50List;
                this.TICchangeIQRList = TICchangeIQRList;
            }

        }

        public RTMetrics DivideByRT(MzmlParser.Run run, int division, double RTDuration)
        {
            double RTsegment = RTDuration / division;
            double[] RTsegs = new double[division];

            for (int i = 0; i < division; i++)
            {
                RTsegs[i] = run.BasePeaks[0].RetentionTime + RTsegment * i;
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
            for (int i = 0; i < TempTIC.Count(); i++)
            {
                var Temp = TempTIC.ElementAt(i);
                List<double> Templist = new List<double>();
                for (int j = 1; j < Temp.Count(); j++)
                {

                    Templist.Add(Math.Abs(Temp.ElementAt(j) - Temp.ElementAt(j - 1)));
                }
                Templist.Sort();
                TICchange50List.Add(Templist.Average());
                TICchangeIQRList.Add(InterQuartileRangeCalculator.CalcIQR(Templist));
            }

            //Calculations for peakprecision MS2:

            var meanIntensityOfAllBpks = run.BasePeaks.Select(x => x.Intensity).Average();
            var meanMzOfAllBpks = run.BasePeaks.Select(x => x.Mz).Average();

            //Calculations for peakprecision MS1:
            var mIMS1Bpks = run.Ms1Scans.Select(x => x.BasePeakIntensity).Average();
            var mMMS1Bpks = run.Ms1Scans.Select(x => x.BasePeakMz).Average();
            List<double> Peakwidths = new List<double>();
            List<double> PeakSymmetry = new List<double>();
            List<double> PeakCapacity = new List<double>();
            List<double> PeakPrecision = new List<double>();
            List<double> MS1PeakPrecision = new List<double>();
            List<int> MS1Density = new List<int>();
            List<int> MS2Density = new List<int>();
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
                        PeakprecisionTemp.Add(basepeak.Intensity / (meanIntensityOfAllBpks * Math.Pow(2, meanMzOfAllBpks / basepeak.Mz)));
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
                Peakwidths.Add(PeakwidthsTemp.Average());
                PeakSymmetry.Add(Math.Ceiling(PeaksymTemp.Average()));
                PeakCapacity.Add(Math.Ceiling(PeakCapacityTemp.Average()));
                PeakPrecision.Add(Math.Ceiling(PeakprecisionTemp.Average()));
                MS1PeakPrecision.Add(Math.Ceiling(MS1PeakprecisionTemp.Average()));
                MS1Density.Add(Convert.ToInt32(Math.Round(MS1DensityTemp.Average(),0)));
                MS2Density.Add(Convert.ToInt32(Math.Round(MS2DensityTemp.Average(), 0)));
                MS1TICTotal.Add(Math.Ceiling(MS1TICTotalTemp));
                MS2TICTotal.Add(Math.Ceiling(MS2TICTotalTemp));
            }

            RTMetrics rtMetrics = new RTMetrics(MS1TICTotal, MS2TICTotal, cycleTime, TICchange50List, TICchangeIQRList, MS1Density, MS2Density, Peakwidths, PeakSymmetry, PeakCapacity, PeakPrecision, MS1PeakPrecision);
            return rtMetrics;
        }
    }
}



