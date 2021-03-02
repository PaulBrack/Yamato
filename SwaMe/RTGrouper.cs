#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using SwaMe.Pipeline;

namespace SwaMe
{
    public class RTGrouper
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public double[] rtSegs;

        /// <remarks>Immutable</remarks>
        public class RTMetrics
        {
            public IList<double> Peakwidths { get; }
            public IList<double> TailingFactor { get; }
            public IList<double> PeakCapacity { get; }
            public IList<double> PeakPrecision { get; }
            public IList<double> MS1PeakPrecision { get; }
            public IList<int> MS1Density { get; }
            public IList<int> MS2Density { get; }
            public IList<double> CycleTime { get; }
            public IList<double> MS1TicTotal { get; }
            public IList<double> MS2TicTotal { get; }
            public IList<double> TicChange50List { get; }
            public IList<double> TicChangeIqrList { get; }
            public IList<string> SegmentBoundaries { get; }

            public RTMetrics(IList<double> ms1TicTotal,
                IList<double> ms2TicTotal,
                IList<double> cycleTime,
                IList<double> ticChange50List,
                IList<double> ticChangeIQRList,
                IList<int> ms1Density,
                IList<int> ms2Density,
                IList<double> peakWidths,
                IList<double> tailingFactor,
                IList<double> peakCapacity,
                IList<double> peakPrecision,
                IList<double> ms1PeakPrecision,
                IList<string> segmentBoundaries)
            {
                Peakwidths = peakWidths;
                TailingFactor = tailingFactor;
                PeakCapacity = peakCapacity;
                PeakPrecision = peakPrecision;
                MS1PeakPrecision = ms1PeakPrecision;
                MS1Density = ms1Density;
                MS2Density = ms2Density;
                CycleTime = cycleTime;
                MS1TicTotal = ms1TicTotal;
                MS2TicTotal = ms2TicTotal;
                TicChange50List = ticChange50List;
                TicChangeIqrList = ticChangeIQRList;
                SegmentBoundaries = segmentBoundaries;
            }
        }

        public RTMetrics DivideByRT(Run<Scan> run, int division, double rtDuration)
        {
            double rtSegment = rtDuration / division;
            rtSegs = new double[division];
            List<string> segmentBoundaries = new List<string>();

            if (run.StartTime.HasValue)
            {
            for (int i = 0; i < division; i++)
            {
                    rtSegs[i] = run.StartTime.Value + rtSegment * i;
                if (i > 0)
                        segmentBoundaries.Add(rtSegs[i - 1] + "_" + rtSegs[i]); // segmentBoundaries is a string denoting the startOfTheRTsegment_endOfTheRTsegment for reference
                else
                    segmentBoundaries.Add(run.StartTime + "_" + rtSegs[i]);
            }
            }

            //dividing basepeaks into segments
            foreach (BasePeak basepeak in run.BasePeaks)
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
            double experimentWideMS2TICSquared = 0;
            //dividing ms2scans into segments of RT
            foreach (Scan scan in run.Ms2Scans)
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
                experimentWideMS2TICSquared += Math.Pow(scan.TotalIonCurrent, 2);
            }

            //dividing ms1scans into segments of RT
            foreach (Scan scan in run.Ms1Scans)
            {
                if (scan.ScanStartTime > rtSegs.Last())//If the scan is after the last segment divider, it falls into the last segment
                    scan.RTsegment = rtSegs.Count() - 1;
                else if (rtSegs.Count() > 1 && scan.ScanStartTime < rtSegs[1])//If the scan is before the second segment divider it should fall in the first segment. (assuming that the user has selected to have more than one segment)
                    scan.RTsegment = 0;
                else for (int segmentboundary = 2; segmentboundary < rtSegs.Count(); segmentboundary++)//If the scan is not in the first or last segment
                    {
                        if (scan.ScanStartTime > rtSegs[segmentboundary - 1] && scan.ScanStartTime < rtSegs[segmentboundary])// If the scan is larger than the previous boundary and smaller than this boundary, assign to the previous segment.
                            scan.RTsegment = segmentboundary - 1;
                    }
            }

            //Retrieve TICChange metrics and divide into rtsegs
            List<double> ticChange50List = new List<double>();
            List<double> ticChangeIqrList = new List<double>();
            var tempTic = run.Ms2Scans.OrderBy(x => x.ScanStartTime).GroupBy(x => x.RTsegment).Select(d => d.Select(g => g.TotalIonCurrent).ToList());
            double magnitude = Math.Pow(experimentWideMS2TICSquared, 0.5);
            for (int i = 0; i < tempTic.Count(); i++)
            {
                var temp = tempTic.ElementAt(i);
                List<double> tempList = new List<double>();
                for (int j = 1; j < temp.Count(); j++)
                    tempList.Add(Math.Abs(temp.ElementAt(j) - temp.ElementAt(j - 1)) / magnitude);//Normalised to TIC magnitude.
                tempList.Sort();
                ticChange50List.Add(tempList.Average());
                if (tempList.Count() > 4)
                {
                    ticChangeIqrList.Add(InterQuartileRangeCalculator.CalcIQR(tempList));
                }
                else
                {
                    logger.Error("There are only {0} MS2Scans in this segment, which is too few to calculate the IQR of the TIC Change. This value has been set to zero.", tempTic.Count());
                    ticChangeIqrList.Add(0);
                }
            }

            //Calculations for peakprecision MS2:

            var meanIntensityOfAllBpks = run.BasePeaks.Select(x => x.Intensities.Sum()).Average();
            var meanMzOfAllBpks = run.BasePeaks.Select(x => x.Mz).Average();

            //Calculations for peakprecision MS1:
            //double mIMS1Bpks;
            //if (run.Ms1Scans.Count()>0) mIMS1Bpks = run.Ms1Scans.Select(x => x.BasePeakIntensity).Average();
            //var mMMS1Bpks = run.Ms1Scans.Select(x => x.BasePeakMz).Average();
            List<double> peakWidths = new List<double>();
            List<double> tailingFactor = new List<double>();
            List<double> peakCapacity = new List<double>();
            List<double> peakPrecision = new List<double>();
            List<double> ms1PeakPrecision = new List<double>();
            List<int> ms1Density = new List<int>();
            List<int> ms2Density = new List<int>();
            List<double> cycleTime = new List<double>();
            List<double> ms1TicTotal = new List<double>();
            List<double> ms2TicTotal = new List<double>();

            for (int segment = 0; segment < division; segment++)
            {
                List<double> peakWidthsTemp = new List<double>();
                List<double> peakSymTemp = new List<double>();
                List<double> fullWidthBaselinesTemp = new List<double>();
                List<double> peakPrecisionTemp = new List<double>();
                foreach (BasePeak basePeak in run.BasePeaks)
                {
                    for (int i = 0; i < basePeak.RTsegments.Count; i++)
                    {
                        if (basePeak.RTsegments[i] == segment)
                        {
                            //Each rtsegment should have all these values, but due to some of the peaks being decreased after release candidate 1, some basepeaks have rtsegments, but no datapoints. If min intensity is set high enough, this should never happen.
                            if (basePeak.RTsegments.Count == basePeak.FullWidthHalfMaxes.Count && basePeak.FullWidthHalfMaxes.Count == basePeak.PeakSymmetries.Count && basePeak.FullWidthHalfMaxes.Count == basePeak.PeakSymmetries.Count && basePeak.FullWidthHalfMaxes.Count == basePeak.Intensities.Count && basePeak.FullWidthHalfMaxes.Count == basePeak.FullWidthBaselines.Count)
                            {
                                peakWidthsTemp.Add(basePeak.FullWidthHalfMaxes[i]);
                                peakSymTemp.Add(basePeak.PeakSymmetries[i]);
                                peakPrecisionTemp.Add(basePeak.Intensities[i] / (meanIntensityOfAllBpks * Math.Pow(2, meanMzOfAllBpks / basePeak.Mz)));
                                fullWidthBaselinesTemp.Add(basePeak.FullWidthBaselines[i]);
                            }
                        }
                    }
                }

                List<double> firstScansOfCycle = new List<double>();
                List<double> lastScansOfCycle = new List<double>();
                List<double> difference = new List<double>();
                double ms1TicTotalTemp = 0;
                List<int> ms1DensityTemp = new List<int>();
                List<double> ms1PeakPrecisionTemp = new List<double>();

                foreach (Scan scan in run.Ms1Scans.OrderBy(x => x.ScanStartTime))
                {
                    if (scan.RTsegment == segment)
                    {

                        ms1PeakPrecisionTemp.Add(scan.BasePeakIntensity / (meanIntensityOfAllBpks * Math.Pow(2, meanMzOfAllBpks / scan.BasePeakMz)));
                        ms1DensityTemp.Add(scan.Density);
                        ms1TicTotalTemp += scan.TotalIonCurrent;
                    }
                }
                int minNumberMS1Or2 = 0;
                //To get scan speed for both ms1 and ms2 we have to also scan through ms2:
                if (run.Ms1Scans.Count() > 1)
                {
                    lastScansOfCycle = run.Ms2Scans.Where(x => x.RTsegment == segment).GroupBy(g => g.Cycle).Select(x => x.Max(y => y.ScanStartTime)).ToList();
                    firstScansOfCycle = run.Ms1Scans.Where(x => x.RTsegment == segment).Select(y => y.ScanStartTime).ToList();

                }
                else
                {
                    lastScansOfCycle = run.Ms2Scans.Where(x => x.RTsegment == segment).GroupBy(g => g.Cycle).Select(x => x.Max(y => y.ScanStartTime)).ToList();
                    firstScansOfCycle = run.Ms2Scans.Where(x => x.RTsegment == segment).GroupBy(g => g.Cycle).Select(x => x.Min(y => y.ScanStartTime)).ToList();
                }
                minNumberMS1Or2 = Math.Min(lastScansOfCycle.Count(), firstScansOfCycle.Count());
                for (int i = 0; i < minNumberMS1Or2; i++)
                {
                    difference.Add(lastScansOfCycle[i] - firstScansOfCycle[i]);
                }
                cycleTime.Add(difference.Average() * 60);
                List<int> ms2DensityTemp = run.Ms2Scans.Where(x => x.RTsegment == segment).Select(x => x.Density).ToList();
                double ms2TicTotalTemp = run.Ms2Scans.Where(x => x.RTsegment == segment).Select(x => x.TotalIonCurrent).Sum();

                if (peakWidthsTemp.Count > 0)
                {
                    peakWidths.Add(peakWidthsTemp.Average());
                    tailingFactor.Add(peakSymTemp.Average());
                    peakCapacity.Add(rtSegment / fullWidthBaselinesTemp.Average());//PeakCapacity is calculated as per Dolan et al.,2009, PubMed 10536823);
                    peakPrecision.Add(peakPrecisionTemp.Average());
                }
                else
                {
                    peakWidths.Add(0);
                    tailingFactor.Add(0);
                    peakCapacity.Add(0);
                    peakPrecision.Add(0);
                }
                if (ms1PeakPrecisionTemp.Count > 0)
                {
                    ms1PeakPrecision.Add(ms1PeakPrecisionTemp.Average());
                    ms1Density.Add(Convert.ToInt32(Math.Ceiling(ms1DensityTemp.Average())));
                }
                else
                {
                    ms1PeakPrecision.Add(0);
                    ms1Density.Add(0);
                }
                ms2Density.Add(Convert.ToInt32(Math.Round(ms2DensityTemp.Average(), 0)));
                ms1TicTotal.Add(ms1TicTotalTemp);
                ms2TicTotal.Add(ms2TicTotalTemp);
            }

            RTMetrics rtMetrics = new RTMetrics(ms1TicTotal, ms2TicTotal, cycleTime, ticChange50List, ticChangeIqrList, ms1Density, ms2Density, peakWidths, tailingFactor, peakCapacity, peakPrecision, ms1PeakPrecision, segmentBoundaries, lastMs2DensityTemp);
            return rtMetrics;
        }
    }
}