
using System.Collections.Generic;
using System.Linq;
using System;

namespace SwaMe
{
    public class SwathGrouper
    {
        //This method groups all of the swaths of the same number together and generates the metrics that are reported per swath.
        //It generates a tsvfile with the "Perswath" metrics in.
        //It returns the number of swaths present in a full cycle.
        public class SwathMetrics
        {
            public int maxswath;
            public double totalTIC;
            public List<int> numOfSwathPerGroup;
            public List<double> AveMzRange;
            public List<double> TICs;
            public List<double> swDensity50;
            public List<double> swDensityIQR;
            public List<double> TICRatio;

            public SwathMetrics(int maxswath, double totalTIC, List<int> numOfSwathPerGroup, List<double> AveMzRange, List<double> TICs, List<double> swDensity50, List<double> swDensityIQR,
            List<double> TICRatio)
            {
                this.maxswath = maxswath;
                this.totalTIC = totalTIC;
                this.numOfSwathPerGroup = numOfSwathPerGroup;
                this.AveMzRange = AveMzRange;
                this.TICs = TICs;
                this.swDensity50 = swDensity50;
                this.swDensityIQR = swDensityIQR;
                this.TICRatio = TICRatio;
            }
        }
        public SwathMetrics GroupBySwath(MzmlParser.Run run)
        {
            int maxswath = 0;
            var result = run.Ms2Scans.GroupBy(s => s.Cycle).Select(g => new { Count = g.Count() });
            foreach (var e in result)
            {
                if (e.Count > maxswath)
                    maxswath = e.Count-1;
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
            List<double> TICRatio = new List<double>();
//Loop through all the swaths of the same number and add to
            for (int swathNumber = 0; swathNumber < swathBoundaries.Count(); swathNumber++)
            {
                int track = 0;

                double TICthisSwath = 0;

                var result333 = run.Ms2Scans.OrderBy(s => s.ScanStartTime)
                    .Where(x => x.MsLevel == 2 && x.IsolationWindowTargetMz == swathBoundaries[swathNumber]);
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
                TICthisSwath = 0;
                swDensity.Sort();
                swDensity50.Add(Math.Ceiling(swDensity.Average()));
                swDensityIQR.Add(Math.Ceiling(InterQuartileRangeCalculator.CalcIQR(swDensity)));
                swDensity.Clear();
            }

            for (int num = 0; num < swathBoundaries.Count(); num++)
            {
                TICRatio.Add((TICs[num] / totalTIC));
            }

            SwathMetrics swathMetrics = new SwathMetrics(maxswath, totalTIC, numOfSwathPerGroup, AveMzRange, TICs, swDensity50, swDensityIQR, TICRatio);
            return swathMetrics;
        }

    }
   
}
