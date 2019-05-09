using MzmlParser;
using System.Collections.Generic;
using System.Linq;

namespace SwaMe
{
    public class MetricGenerator
    {
        public void GenerateMetrics(Run run, int division, string iRTpath, string inputFilePath)
        {
            if (iRTpath != "none")
            {
                LibraryParser.Library irtLibrary = new LibraryParser.Library();
                if (iRTpath.Contains("traml"))
                {
                    LibraryParser.TraMLReader traMLReader = new LibraryParser.TraMLReader();
                    irtLibrary = traMLReader.LoadLibrary(iRTpath);
                }
                else if (iRTpath.Contains("sky"))
                {
                    LibraryParser.SkyReader skyReader = new LibraryParser.SkyReader();
                    irtLibrary = skyReader.LoadLibrary(iRTpath);
                }

                for (int iterator = 0; iterator < irtLibrary.PeptideList.Count; iterator++)
                {
                    //Now search in the MS1 binary arrays for peptide m/z

                    //Amongst all the matches, search the next couple of ms2 scans for the correct m/z windows for matches to find out which ms1 matches are the correct ones.
                }
            }


            //Acquire RTDuration:
            double RTDuration = run.BasePeaks[run.BasePeaks.Count() - 1].RetentionTime - run.BasePeaks[0].RetentionTime;

            //Interpolate, Smooth, create chromatogram and generate chromatogram metrics
            ChromatogramMetricGenerator chromatogramMetrics = new ChromatogramMetricGenerator();
            chromatogramMetrics.GenerateChromatogram(run);

            //Calculating the largestswath
            double swathSizeDifference = CalcSwathSizeDiff(run);

            // This method will group the scans into swaths of the same number, return the number of swaths in a full cycle (maxswath) and call a FileMaker method to write out the metrics.
            SwathGrouper swathGrouper = new SwathGrouper { };
            SwathGrouper.SwathMetrics swathMetrics = swathGrouper.GroupBySwath(run);

            //Retrieving cycletimesmetrics
            List<double> CycleTimes = CalcCycleTime(run);
            CycleTimes.Sort();
            //Retrieving Density metrics
            var Density = run.Ms2Scans.OrderBy(g => g.Density).Select(g => g.Density).ToList();

            //Create IQR so you can calculate IQR:            
            RTGrouper rtGrouper = new RTGrouper { };
            RTGrouper.RTMetrics rtMetrics = rtGrouper.DivideByRT(run, division, RTDuration);
            FileMaker fileMaker = new FileMaker(division, inputFilePath, run, swathMetrics, rtMetrics, RTDuration, swathSizeDifference, run.Ms2Scans.Count(), CycleTimes.ElementAt(CycleTimes.Count() / 2), InterQuartileRangeCalculator.CalcIQR(CycleTimes), Density.ElementAt(Density.Count() / 2), InterQuartileRangeCalculator.CalcIQR(Density), run.Ms1Scans.Count());
            fileMaker.MakeUndividedMetricsFile();
            fileMaker.MakeMetricsPerRTsegmentFile(rtMetrics);
            fileMaker.MakeMetricsPerSwathFile(swathMetrics);
            fileMaker.MakeJSON();
        }

        private double CalcSwathSizeDiff(Run run)
        {
            return run.Ms2Scans.Select(s => s.IsolationWindowUpperOffset + s.IsolationWindowLowerOffset).OrderBy(x => x).Last();
        }

        private List<double> CalcCycleTime(Run run)
        {
            return run.Ms2Scans.GroupBy(s => s.Cycle).Select(g => g.OrderByDescending(d => d.ScanStartTime)).Select(e => e.First().ScanStartTime - e.Last().ScanStartTime).ToList();
        }
    }
}
        