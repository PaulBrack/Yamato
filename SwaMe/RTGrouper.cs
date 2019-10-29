using System;
using System.Collections.Generic;
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
            public List<double> CycleTime;
            public List<double> MS1TicTotal;
            public List<double> MS2TicTotal;
            public List<double> TicChange50List;
            public List<double> TicChangeIqrList;

            public RTMetrics(List<double> MS1TICTotal, List<double> MS2TICTotal, List<double> cycleTime, List<double> TICchange50List, List<double> TICchangeIQRList, List<int> MS1Density, List<int> MS2Density, List<double> Peakwidths, List<double> PeakSymmetry, List<double> PeakCapacity, List<double> PeakPrecision, List<double> MS1PeakPrecision)
            {
                this.Peakwidths = Peakwidths;
                this.PeakSymmetry = PeakSymmetry;
                this.PeakCapacity = PeakCapacity;
                this.PeakPrecision = PeakPrecision;
                this.MS1PeakPrecision = MS1PeakPrecision;
                this.MS1Density = MS1Density;
                this.MS2Density = MS2Density;
                this.CycleTime = cycleTime;
                this.MS1TicTotal = MS1TICTotal;
                this.MS2TicTotal = MS2TICTotal;
                this.TicChange50List = TICchange50List;
                this.TicChangeIqrList = TICchangeIQRList;
            }

        }

        public RTMetrics DivideByRT(MzmlParser.Run run, int division, double rtDuration)
        {
            double rtSegment = rtDuration / division;
            double[] rtSegs = new double[division];


            for (int i = 0; i < division; i++)
                rtSegs[i] = runStart + rtSegment * i;

            //dividing basepeaks into segments
            foreach (MzmlParser.BasePeak basepeak in run.BasePeaks)
            {
                //Check to see in which RTsegment this basepeak is:
                foreach (double rt in basepeak.BpkRTs)
                {
                    if (rt > rtSegs.Last())
                        basepeak.RTsegments.Add(rtSegs.Count() - 1);
                    else if (rtSegs.Count() > 1 && rt < rtSegs[1])
                        basepeak.RTsegments.Add(0);
                    else for (int segmentboundary = 2; segmentboundary < rtSegs.Count(); segmentboundary++)
                        {
                            if (rt > rtSegs[segmentboundary - 1] && rt < rtSegs[segmentboundary])
                            {
                                basepeak.RTsegments.Add(segmentboundary - 1);
                                segmentboundary = rtSegs.Count();//move to the next bpkRT
                            }
                        }
                }
            }

            //dividing ms2scans into segments of RT
            foreach (MzmlParser.Scan scan in run.Ms2Scans)
            {
                //if the scan starttime falls into the rtsegment, give it the correct rtsegment number
                //We assign to the segmentdivider below. So if >a and <b, it is assigned to segment a.

                if (scan.ScanStartTime > rtSegs.Last())//If the scan is after the last segment divider, it falls into the last segment
                    scan.RTsegment = rtSegs.Count() - 1;
                else if (rtSegs.Count() > 1 && scan.ScanStartTime < rtSegs[1])//If the scan is before the second segment divider it should fall in the first segment. (assuming that the user has selected to have more than one segment)
                    scan.RTsegment = 0;
                else for (int segmentboundary = 2; segmentboundary < rtSegs.Count(); segmentboundary++)//If the scan is not in the first or last segment
                    {
                        if (scan.ScanStartTime > rtSegs[segmentboundary - 1] && scan.ScanStartTime < rtSegs[segmentboundary])// If the scan is larger than the previous boundary and smaller than this boundary, assign to the previous segment.
                            scan.RTsegment = segmentboundary - 1;
                    }


                for (int segmentboundary = 1; segmentboundary < rtSegs.Count(); segmentboundary++)
                {
                    if (scan.ScanStartTime > rtSegs.Last())
                        scan.RTsegment = rtSegs.Count() - 1;
                    else if (scan.ScanStartTime > rtSegs[segmentboundary - 1] && scan.ScanStartTime < rtSegs[segmentboundary])
                    {
                        scan.RTsegment = segmentboundary - 1;
                        break;
                    }
                    else if (scan.ScanStartTime > rtSegs[segmentboundary] && segmentboundary == rtSegs.Count())
                    {
                        scan.RTsegment = segmentboundary + 1;
                        break;
                    }
                }
            }

            //dividing ms1scans into segments of RT
            foreach (MzmlParser.Scan scan in run.Ms1Scans)
            {
                //Check to see in which RTsegment this basepeak is:
                for (int segmentboundary = 1; segmentboundary < rtSegs.Count(); segmentboundary++)
                {
                    if (scan.ScanStartTime > rtSegs.Last())
                        scan.RTsegment = rtSegs.Count() - 1;
                    else if (scan.ScanStartTime > rtSegs[segmentboundary - 1] && scan.ScanStartTime < rtSegs[segmentboundary])
                    {
                        scan.RTsegment = segmentboundary - 1;
                        break;
                    }
                    else if (scan.ScanStartTime > rtSegs[segmentboundary] && segmentboundary == rtSegs.Count())
                    {
                        scan.RTsegment = segmentboundary + 1;
                        break;
                    }

                }
            }

            //Retrieve TICChange metrics and divide into rtsegs
            List<double> ticChange50List = new List<double>();
            List<double> ticChangeIqrList = new List<double>();
            var tempTic = run.Ms2Scans.GroupBy(x => x.RTsegment).Select(d => d.OrderBy(x => x.ScanStartTime).Select(g => g.TotalIonCurrent).ToList());
            for (int i = 0; i < tempTic.Count(); i++)
            {
                var temp = tempTic.ElementAt(i);
                List<double> tempList = new List<double>();
                for (int j = 1; j < temp.Count(); j++)
                    tempList.Add(Math.Abs(temp.ElementAt(j) - temp.ElementAt(j - 1)));
                tempList.Sort();
                ticChange50List.Add(Math.Truncate(Math.Round(tempList.Average())));
                ticChangeIqrList.Add(Math.Truncate(Math.Round(InterQuartileRangeCalculator.CalcIQR(tempList))));
            }

            //Calculations for peakprecision MS2:

            var meanIntensityOfAllBpks = run.BasePeaks.Select(x => x.Intensities.Sum()).Average();
            var meanMzOfAllBpks = run.BasePeaks.Select(x => x.Mz).Average();

            //Calculations for peakprecision MS1:
            var mIMS1Bpks = run.Ms1Scans.Select(x => x.BasePeakIntensity).Average();
            var mMMS1Bpks = run.Ms1Scans.Select(x => x.BasePeakMz).Average();
            List<double> peakWidths = new List<double>();
            List<double> peakSymmetry = new List<double>();
            List<double> peakCapacity = new List<double>();
            List<double> peakPrecision = new List<double>();
            List<double> ms1PeakPrecision = new List<double>();
            List<int> ms1Density = new List<int>();
            List<int> ms2Density = new List<int>();
            List<double> cycleTime = new List<double>();
            List<double> ms1TicTotal = new List<double>();
            List<double> Ms2TicTotal = new List<double>();

            for (int segment = 0; segment < division; segment++)
            {
                List<double> peakWidthsTemp = new List<double>();
                List<double> peakSymTemp = new List<double>();
                List<double> fullWidthBaselinesTemp = new List<double>();
                List<double> peakPrecisionTemp = new List<double>();
                foreach (MzmlParser.BasePeak basepeak in run.BasePeaks)
                {
                    for (int i = 0; i < basepeak.RTsegments.Count(); i++)
                    {
                        if (basepeak.RTsegments[i] == segment)
                        {
                            peakWidthsTemp.Add(basepeak.FWHMs[i]);
                            peakSymTemp.Add(basepeak.Peaksyms[i]);
                            peakPrecisionTemp.Add(basepeak.Intensities[i] / (meanIntensityOfAllBpks * Math.Pow(2, meanMzOfAllBpks / basepeak.Mz)));
                            fullWidthBaselinesTemp.Add(basepeak.FullWidthBaselines[i]);
                        }
                    }
                }
                double firstScanStartTime = 1000;
                double lastScanStartTime = 0;
                int firstCycle = 1000;
                int lastCycle = 0;
                double ms1TicTotalTemp = 0;
                List<int> ms1DensityTemp = new List<int>();
                List<double> ms1PeakPrecisionTemp = new List<double>();

                foreach (MzmlParser.Scan scan in run.Ms1Scans)
                {
                    if (scan.RTsegment == segment)
                    {

                        ms1PeakPrecisionTemp.Add(scan.BasePeakIntensity / (meanIntensityOfAllBpks * Math.Pow(2, meanMzOfAllBpks / scan.BasePeakMz)));
                        firstScanStartTime = Math.Min(firstScanStartTime, scan.ScanStartTime);
                        lastScanStartTime = Math.Max(lastScanStartTime, scan.ScanStartTime);
                        firstCycle = Math.Min(scan.Cycle, firstCycle);
                        lastCycle = Math.Max(scan.Cycle, lastCycle);
                        ms1DensityTemp.Add(scan.Density);
                        ms1TicTotalTemp += scan.TotalIonCurrent;
                    }
                }

                //To get scan speed for both ms1 and ms2 we have to also scan through ms2:
                List<int> ms2DensityTemp = new List<int>();
                double ms2TicTotalTemp = 0;
                foreach (MzmlParser.Scan scan in run.Ms2Scans)
                {
                    if (scan.RTsegment == segment)
                    {
                        firstCycle = Math.Min(scan.Cycle, firstCycle);
                        lastCycle = Math.Max(scan.Cycle, lastCycle);
                        ms2DensityTemp.Add(scan.Density);
                        ms2TicTotalTemp += scan.TotalIonCurrent;
                    }
                }

                cycleTime.Add((lastScanStartTime - firstScanStartTime) / (lastCycle - firstCycle));
                if (peakWidthsTemp.Count > 0)
                {
                    peakWidths.Add(peakWidthsTemp.Average());
                    peakSymmetry.Add(peakSymTemp.Average());
                    peakCapacity.Add(rtSegment / fullWidthBaselinesTemp.Average());//PeakCapacity is calculated as per Dolan et al.,2009, PubMed 10536823);
                    peakPrecision.Add(peakPrecisionTemp.Average());
                }
                else
                {
                    peakWidths.Add(0);
                    peakSymmetry.Add(0);
                    peakCapacity.Add(0);
                    peakPrecision.Add(0);
                }
                if (ms1PeakPrecisionTemp.Count > 0)
                {
                    ms1PeakPrecision.Add(ms1PeakPrecisionTemp.Average());
                    ms1Density.Add(Convert.ToInt32(Math.Round(ms1DensityTemp.Average(), 0)));
                }
                else
                {
                    ms1PeakPrecision.Add(0);
                    ms1Density.Add(0);
                }
                ms2Density.Add(Convert.ToInt32(Math.Round(ms2DensityTemp.Average(), 0)));
                ms1TicTotal.Add(Math.Truncate(Math.Ceiling(ms1TicTotalTemp)));
                Ms2TicTotal.Add(Math.Truncate(Math.Ceiling(ms2TicTotalTemp)));
            }

            RTMetrics rtMetrics = new RTMetrics(ms1TicTotal, Ms2TicTotal, cycleTime, ticChange50List, ticChangeIqrList, ms1Density, ms2Density, peakWidths, peakSymmetry, peakCapacity, peakPrecision, ms1PeakPrecision);
            return rtMetrics;
        }
    }
}



