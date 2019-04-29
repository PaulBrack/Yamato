
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace SwaMe
{
    class SwathGrouper
    {
        //This method groups all of the swaths of the same number together and generates the metrics that are reported per swath.
        //It generates a tsvfile with the "Perswath" metrics in.
        //It returns the number of swaths present in a full cycle.
        public int GroupBySwath(MzmlParser.Run run)
        {
            int maxswath = 0;
            var result = run.Ms2Scans.GroupBy(s => s.Cycle).Select(g => new { Count = g.Count() });
            foreach (var e in result)
            {
                if (e.Count > maxswath)
                    maxswath = e.Count;
            }

            //Create list of target isolationwindows to serve as swathnumber
            List<double> swathBoundaries = new List<double>();
            swathBoundaries = run.Ms2Scans.Select(x => x.IsolationWindowTargetMz).Distinct().ToList();

            double totalTIC = 0;
            foreach (var scan in run.Ms2Scans)
            {
                totalTIC += scan.TotalIonCurrent;
            }

            //Loop through every swath of a certain swathNumber:

            List<int> numOfSwathPerGroup = new List<int>();
            List<double> AveMzRange = new List<double>();
            List<double> TICs = new List<double>();
            List<double> swDensity = new List<double>();
            List<double> swDensity50 = new List<double>();
            List<double> swDensityIQR = new List<double>();
            List<double> mzrange = new List<double>();
            for (int swathsOfThisNumber = 0; swathsOfThisNumber < maxswath; swathsOfThisNumber++)
            {
                int track = 0;

                double TICthisSwath = 0;

                var result333 = run.Ms2Scans.OrderBy(s => s.ScanStartTime)
                    .Where(x => x.MsLevel == 2 && x.IsolationWindowTargetMz == swathBoundaries[swathsOfThisNumber]);
                foreach (var scan in result333)
                {

                    mzrange.Add(scan.IsolationWindowUpperOffset + scan.IsolationWindowLowerOffset);
                    TICthisSwath = TICthisSwath + scan.TotalIonCurrent;
                    swDensity.Add(scan.Density);
                    track++;
                }

                numOfSwathPerGroup.Add(track);
                AveMzRange.Add(mzrange.Average());
                TICs.Add(TICthisSwath);
                swDensity.Sort();
                swDensity50.Add(swDensity.Average());
                swDensityIQR.Add(InterQuartileRangeCalculator.CalcIQR(swDensity));
            }

            new FileMaker().MakeMetricsPerSwathFile(maxswath, run, numOfSwathPerGroup, AveMzRange, totalTIC, TICs, swDensity50, swDensityIQR);

            return maxswath;
        }
    }
}
