#nullable enable

using System.Collections.Generic;
using System.Linq;
using NLog;
using SwaMe.Pipeline;

namespace SwaMe
{
    public class MetricGenerator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Metrics GenerateMetrics(Run<Scan> run, int divisions, bool irt)
        {
            double rtDuration;
            if (run.LastScanTime.HasValue && run.StartTime.HasValue)
            {
                //Acquire RTDuration: last minus first
                rtDuration = run.LastScanTime.Value - run.StartTime.Value;
            }
            else
            {
                Logger.Error("StartTime {0} or lastScanTime {0} for the run is null. RTDuration is therefore zero", run.StartTime, run.LastScanTime);
                rtDuration = 0;
            }

            //Interpolate, Smooth, create chromatogram and generate chromatogram metrics
            ChromatogramMetricGenerator chromatogramMetrics = new ChromatogramMetricGenerator();
            chromatogramMetrics.GenerateChromatogram(run);

            if (irt)
                chromatogramMetrics.GenerateiRTChromatogram(run);

            //Calculating the largestswath
            double swathSizeDifference;
            if (run.Ms2Scans.Any(scan => !scan.IsolationWindowUpperOffset.HasValue) || run.Ms2Scans.Any(scan => !scan.IsolationWindowLowerOffset.HasValue))
            {
                Logger.Error("IsolationWindowUpperOffset or IsolationWindowLowerOffset for one or more of the scans has not been set. SwathSizeDifference is therefore zero");
                swathSizeDifference = 0;
            }
            else if (run.IsolationWindows.Count == 0)
            {
                Logger.Error("No isolation windows have been set on the run. SwathSizeDifference is therefore zero");
                swathSizeDifference = 0;
            }
            else
            {
                swathSizeDifference = run.IsolationWindows.Max(isolationWindow => isolationWindow.HighMz - isolationWindow.LowMz) - run.IsolationWindows.Min(isolationWindow => isolationWindow.HighMz - isolationWindow.LowMz);
            }

            // This method will group the scans into swaths of the same number, return the number of swaths in a full cycle (maxswath) and call a FileMaker method to write out the metrics.
            SwathGrouper swathGrouper = new SwathGrouper();
            SwathMetrics swathMetrics = swathGrouper.GroupBySwath(run);

            //Retrieving Density metrics
            int[] densities = run.Ms2Scans.Select(g => g.Density).OrderBy(density => density).ToArray();

            RTGrouper rtGrouper = new RTGrouper();
            RTGrouper.RTMetrics rtMetrics = rtGrouper.DivideByRT(run, divisions, rtDuration);
            
            return new Metrics
            {
                RtDuration = rtDuration,
                SwathSizeDifference = swathSizeDifference,
                Ms1ScanCount = run.Ms1Scans.Count,
                Ms2ScanCount = run.Ms2ScanCount,
                NumOfSwaths = swathMetrics.SwathTargets.Count,
                TargetMz = swathMetrics.SwathTargets,
                TotalMS2IonCount = densities.Sum(),
                Ms2Density50 = densities.ElementAt(densities.Length / 2),
                Ms2DensityIqr = InterQuartileRangeCalculator.CalcIQR(densities),
                NumOfSwathPerGroup = swathMetrics.NumOfSwathPerGroup,
                MzRange = swathMetrics.MzRange,
                SwathProportionOfTotalTIC = swathMetrics.SwathProportionOfTotalTIC,
                SwDensity50 = swathMetrics.SwDensity50,
                SwDensityIQR = swathMetrics.SwDensityIQR,
                Peakwidths = rtMetrics.Peakwidths,
                PeakCapacity = rtMetrics.PeakCapacity,
                MS1PeakPrecision = rtMetrics.MS1PeakPrecision,
                TicChange50List = rtMetrics.TicChange50List,
                TicChangeIqrList = rtMetrics.TicChangeIqrList,
                CycleTime = rtMetrics.CycleTime,
                MS2Density = rtMetrics.MS2Density,
                MS1Density = rtMetrics.MS1Density,
                MS2TicTotal = rtMetrics.MS2TicTotal,
                MS1TicTotal = rtMetrics.MS1TicTotal,
                TailingFactor = rtMetrics.TailingFactor,
                Density = rtMetrics.Density
            };
        }
    }

    public class Metrics : IMzqcMetrics
    {
        public double RtDuration { get; set; }
        public double SwathSizeDifference { get; set; }
        public double Ms2ScanCount { get; set; }
        public double NumOfSwaths { get; set; }
        public IList<double> TargetMz { get; set; }
        public double TotalMS2IonCount { get; set; }
        public double Ms2Density50 { get; set; }
        public double Ms2DensityIqr { get; set; }
        public double Ms1ScanCount { get; set; }
        public IList<int>? NumOfSwathPerGroup { get; set; }
        public IList<double?>? MzRange { get; set; }
        public IList<double>? SwathProportionOfTotalTIC { get; set; }
        public IList<double>? SwDensity50 { get; set; }
        public IList<double?>? SwDensityIQR { get; set; }
        public IList<double>? Peakwidths { get; set; }
        public IList<double>? PeakCapacity { get; set; }
        public IList<double>? MS1PeakPrecision { get; set; }
        public IList<double>? TicChange50List { get; set; }
        public IList<double>? TicChangeIqrList { get; set; }
        public IList<double>? CycleTime { get; set; }
        public IList<int>? MS2Density { get; set; }
        public IList<int>? MS1Density { get; set; }
        public IList<double>? MS2TicTotal { get; set; }
        public IList<double>? MS1TicTotal { get; set; }
        public IList<double>? TailingFactor { get; set; }

        /// <summary>
        /// Testing: Raw densities of final segment.
        /// </summary>
        public IList<int>? Density { get; set; }

        public IDictionary<string, dynamic> RenderableMetrics =>
            new Dictionary<string, dynamic>
            {
                { "QC:4000053", RtDuration },
                { "QC:02", SwathSizeDifference },
                { "QC:4000059", Ms1ScanCount },
                { "QC:4000060", Ms2ScanCount },
                { "QC:04", NumOfSwaths },
                { "QC:05", TargetMz },
                { "QC:06", TotalMS2IonCount },
                { "QC:07", Ms2Density50 },
                { "QC:08", Ms2DensityIqr },
                { "QC:09", NumOfSwathPerGroup },
                { "QC:10", MzRange },
                { "QC:11", SwathProportionOfTotalTIC },
                { "QC:12", SwDensity50 },
                { "QC:13", SwDensityIQR },
                { "QC:14", Peakwidths },
                { "QC:15", PeakCapacity },
                { "QC:16", MS1PeakPrecision },
                { "QC:17", TicChange50List },
                { "QC:18", TicChangeIqrList },
                { "QC:19", CycleTime },
                { "QC:20", MS2Density },
                { "QC:21", MS1Density },
                { "QC:22", MS2TicTotal },
                { "QC:23", MS1TicTotal },
                { "QC:24", TailingFactor }
            };
    };

}
