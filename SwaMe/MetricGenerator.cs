using MzmlParser;
using System.Collections.Generic;
using System.Linq;

namespace SwaMe
{
    public class MetricGenerator
    {
        public void GenerateMetrics(Run run, int division, string iRTpath, string path)
        {
            if (iRTpath != "none")
            {
                LibraryParser.Library irtLibrary = new LibraryParser.Library();
                if (iRTpath.Contains("traml"))
                {
                    LibraryParser.TraMLReader lp = new LibraryParser.TraMLReader();
                    irtLibrary = lp.LoadLibrary(iRTpath);
                }
                else if (iRTpath.Contains("sky"))
                {
                    LibraryParser.SkyReader sp = new LibraryParser.SkyReader();
                    irtLibrary = sp.LoadLibrary(iRTpath);
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
            ChromatogramMetrics cm = new ChromatogramMetrics();
            cm.CreateChromatogram(run);

            //Calculating the largestswath
            double swathSizeDifference = CalcSwathSizeDiff(run);

            // This method will group the scans into swaths of the same number, return the number of swaths in a full cycle (maxswath) and call a FileMaker method to write out the metrics.
            SwathGrouper Sd = new SwathGrouper { };
            int maxswath = Sd.GroupBySwath(run);

            //Retrieving cycletimesmetrics
            List<double> CycleTimes = CalcCycleTime(run);

            //Retrieving Density metrics
            var Density = run.Ms2Scans.OrderBy(g => g.Density).Select(g => g.Density).ToList();

            //Create IQR so you can calculate IQR:            
            RTGrouper Rd = new RTGrouper { };
            Rd.DivideByRT(run, division, RTDuration);
            FileMaker Um = new FileMaker { };
            Um.MakeUndividedMetricsFile(run, RTDuration, swathSizeDifference, run.Ms2Scans.Count(), maxswath, CycleTimes.ElementAt(CycleTimes.Count() / 2), InterQuartileRangeCalculator.CalcIQR(CycleTimes), Density[Density.Count() / 2], InterQuartileRangeCalculator.CalcIQR(Density), run.Ms1Scans.Count());
            Um.MakeJSON(path, run, RTDuration,swathSizeDifference, run.Ms2Scans.Count(),maxswath, CycleTimes.ElementAt(CycleTimes.Count() / 2), InterQuartileRangeCalculator.CalcIQR(CycleTimes), Density[Density.Count() / 2], InterQuartileRangeCalculator.CalcIQR(Density), run.Ms1Scans.Count());
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
        