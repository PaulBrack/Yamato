using MzmlParser;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace SwaMe
{
    public class MetricGenerator
    {
        private static Logger logger = LogManager.GetCurrentClassLogger(); 
        public double RTDuration;
        public double swathSizeDifference;
        public List<int> Density;
        public void GenerateMetrics(Run run, int division,  string inputFilePath, bool irt, bool combine, bool lastFile, string date)
        {
            if (run.LastScanTime != 0 && run.StartTime != 1000000)
            {
                //Acquire RTDuration: last minus first
                RTDuration = run.LastScanTime - run.StartTime;
            }
            else 
            {
                logger.Error("StartTime {0} or lastScanTime {0} for the run is null. RTDuration is therefore zero", run.StartTime, run.LastScanTime);
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
                logger.Error(("IsolationWindowUpperOffset {0} or IsolationWindowLowerOffset {0} for the one of the scans has not been changed from default value. swathSizeDifference is therefore zero", run.Ms2Scans.Max(x => x.IsolationWindowUpperOffset), run.Ms2Scans.Max(x => x.IsolationWindowLowerOffset)));
                swathSizeDifference = 0;
            }
            else
            {
                swathSizeDifference = run.Ms2Scans.Max(x => x.IsolationWindowUpperOffset + x.IsolationWindowLowerOffset) - run.Ms2Scans.Min(x => x.IsolationWindowUpperOffset + x.IsolationWindowLowerOffset);
            }

            // This method will group the scans into swaths of the same number, return the number of swaths in a full cycle (maxswath) and call a FileMaker method to write out the metrics.
            SwathGrouper swathGrouper = new SwathGrouper { };
            SwathGrouper.SwathMetrics swathMetrics = swathGrouper.GroupBySwath(run);

            //Retrieving Density metrics
            Density = run.Ms2Scans.OrderBy(g => g.Density).Select(g => g.Density).ToList();

                    
            RTGrouper rtGrouper = new RTGrouper { };
            RTGrouper.RTMetrics rtMetrics = rtGrouper.DivideByRT(run, division, RTDuration);
            FileMaker fileMaker = new FileMaker(division, inputFilePath, run, swathMetrics, rtMetrics, RTDuration, swathSizeDifference, run.Ms2Scans.Count(), Density.Sum(), Density.ElementAt(Density.Count() / 2), InterQuartileRangeCalculator.CalcIQR(Density), run.Ms1Scans.Count(), date);
            fileMaker.MakeUndividedMetricsFile();
            if (run.IRTPeaks != null && run.IRTPeaks.Count() > 0)
            {
                fileMaker.MakeiRTmetricsFile(run);
            }
            
            fileMaker.MakeMetricsPerRTsegmentFile(rtMetrics);
            fileMaker.MakeMetricsPerSwathFile(swathMetrics);
            fileMaker.CreateAndSaveMzqc();
            if (combine && lastFile)
            {
                if (run.IRTPeaks != null && run.IRTPeaks.Count() > 0)
                {
                    string[] iRTFilename = { "AllIRTMetrics_", date, ".tsv" };
                    fileMaker.CombineMultipleFilesIntoSingleFile(date + "_iRTMetrics_*",string.Join("",iRTFilename));
                }

                string[] swathFilename = { "AllMetricsBySwath_", date, ".tsv" };
                string[] rtFilename = { "AllRTDividedMetrics_", date, ".tsv" };
                string[] undividedFilename = { "AllUndividedMetrics_", date, ".tsv" };
                fileMaker.CheckOutputDirectory(inputFilePath);
                fileMaker.CombineMultipleFilesIntoSingleFile(date + "_MetricsBySwath_*",string.Join("",swathFilename) );
                fileMaker.CombineMultipleFilesIntoSingleFile(date + "_RTDividedMetrics_*", string.Join("", rtFilename));
                fileMaker.CombineMultipleFilesIntoSingleFile(date + "_undividedMetrics_*", string.Join("",undividedFilename));
             }
        }
    }
}
        