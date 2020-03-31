using MzmlParser;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace SwaMe
{
    public class MetricGenerator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public double RTDuration;
        public double swathSizeDifference;
        public List<int> Density;
        public SwathGrouper.SwathMetrics SwathMetrics { get; private set; }
        public RTGrouper.RTMetrics RtMetrics { get; private set; }
        public Run Run { get; private set; }

        public Dictionary<string, dynamic> GenerateMetrics(Run run, int division, string inputFilePath, bool irt, bool combine, bool lastFile, string date)
        {
            Run = run;
            if (run.LastScanTime != 0 && run.StartTime != 1000000)
            {
                //Acquire RTDuration: last minus first
                RTDuration = run.LastScanTime - run.StartTime;
            }
            else
            {
                Logger.Error("StartTime {0} or lastScanTime {0} for the run is null. RTDuration is therefore zero", run.StartTime, run.LastScanTime);
                RTDuration = 0;
            }


            //Interpolate, Smooth, create chromatogram and generate chromatogram metrics
            ChromatogramMetricGenerator chromatogramMetrics = new ChromatogramMetricGenerator();
            chromatogramMetrics.GenerateChromatogram(run);

            if (irt)
                chromatogramMetrics.GenerateiRTChromatogram(run);

            //Calculating the largestswath
            if (run.Ms2Scans.Max(x => x.IsolationWindowUpperOffset) == 100000 || run.Ms2Scans.Max(x => x.IsolationWindowLowerOffset) == 100000)
            {
                Logger.Error(("IsolationWindowUpperOffset {0} or IsolationWindowLowerOffset {0} for the one of the scans has not been changed from default value. swathSizeDifference is therefore zero", run.Ms2Scans.Max(x => x.IsolationWindowUpperOffset), run.Ms2Scans.Max(x => x.IsolationWindowLowerOffset)));
                swathSizeDifference = 0;
            }
            else
            {
                swathSizeDifference = run.Ms2Scans.Max(x => x.IsolationWindowUpperOffset + x.IsolationWindowLowerOffset) - run.Ms2Scans.Min(x => x.IsolationWindowUpperOffset + x.IsolationWindowLowerOffset);
            }

            // This method will group the scans into swaths of the same number, return the number of swaths in a full cycle (maxswath) and call a FileMaker method to write out the metrics.
            SwathGrouper swathGrouper = new SwathGrouper { };
            SwathMetrics = swathGrouper.GroupBySwath(run);

            //Retrieving Density metrics
            Density = run.Ms2Scans.OrderBy(g => g.Density).Select(g => g.Density).ToList();


            RTGrouper rtGrouper = new RTGrouper { };
            RtMetrics = rtGrouper.DivideByRT(run, division, RTDuration);
            
            

            return AssembleMetrics();
        }

        public Dictionary<string, dynamic> AssembleMetrics()
        {
            return new Dictionary<string, dynamic>
            {
                { "QC:4000053", RTDuration },
                { "QC:02", swathSizeDifference },
                { "QC:4000059", Run.Ms1Scans.Count() },
                { "QC:4000060", Run.Ms2Scans.Count() },
                { "QC:04", SwathMetrics.swathTargets.Count() },
                { "QC:05", SwathMetrics.swathTargets },
                { "QC:06", Density.Sum() },
                { "QC:07", Density.ElementAt(Density.Count() / 2) },
                { "QC:08", InterQuartileRangeCalculator.CalcIQR(Density) },
                { "QC:09", SwathMetrics.numOfSwathPerGroup },
                { "QC:10", SwathMetrics.mzRange },
                { "QC:11", SwathMetrics.SwathProportionOfTotalTIC },
                { "QC:12", SwathMetrics.swDensity50 },
                { "QC:13", SwathMetrics.swDensityIQR },
                { "QC:14", RtMetrics.Peakwidths },
                { "QC:15", RtMetrics.PeakCapacity },
                { "QC:16", RtMetrics.MS1PeakPrecision },
                { "QC:17", RtMetrics.TicChange50List },
                { "QC:18", RtMetrics.TicChangeIqrList },
                { "QC:19", RtMetrics.CycleTime },
                { "QC:20", RtMetrics.MS2Density },
                { "QC:21", RtMetrics.MS1Density },
                { "QC:22", RtMetrics.MS2TicTotal },
                { "QC:23", RtMetrics.MS1TicTotal },
                { "QC:24", RtMetrics.TailingFactor }
            };
        }
    }
}
