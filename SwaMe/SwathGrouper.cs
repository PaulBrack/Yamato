
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
            public List<double> mzRange;
            public List<double> TICs;
            public List<double> swDensity50;
            public List<double> swDensityIQR;
            public List<double> SwathProportionOfTotalTIC;
            public List<double> SwathProportionPredictedSingleChargeAvg;

            public SwathMetrics(int maxswath, double totalTIC, List<int> numOfSwathPerGroup, List<double> mzRange, List<double> TICs, List<double> swDensity50, List<double> swDensityIQR,
            List<double> SwathProportionOfTotalTIC, List<double> SwathProportionPredictedSingleChargeAvg)
            {
                this.maxswath = maxswath;
                this.totalTIC = totalTIC;
                this.numOfSwathPerGroup = numOfSwathPerGroup;
                this.mzRange = mzRange;
                this.TICs = TICs;
                this.swDensity50 = swDensity50;
                this.swDensityIQR = swDensityIQR;
                this.SwathProportionOfTotalTIC = SwathProportionOfTotalTIC;
                this.SwathProportionPredictedSingleChargeAvg = SwathProportionPredictedSingleChargeAvg;
            }
        }
        public SwathMetrics GroupBySwath(MzmlParser.Run run)
        {
            
            //Create list of target isolationwindows to serve as swathnumber
            List<double> swathBoundaries = new List<double>();
            swathBoundaries = run.Ms2Scans.OrderBy(y=>y.ScanStartTime).Select(x => x.IsolationWindowTargetMz).Distinct().ToList();
            double totalTIC = 0;
            foreach (var scan in run.Ms2Scans)
            {
                totalTIC += scan.TotalIonCurrent;
            }

            //Loop through every swath of a certain swathNumber:

            List<int> numOfSwathPerGroup = new List<int>();
            List<double> TICs = new List<double>();
            List<double> swDensity = new List<double>();
            List<double> swDensity50 = new List<double>();
            List<double> swDensityIQR = new List<double>();
            List<double> mzTargetRange = new List<double>();
            List<double> medianMzTargetRange = new List<double>();
            List<double> SwathProportionOfTotalTIC = new List<double>();
            List<double> TotalSwathProportionPredictedSingleCharge = new List<double>();
            List<double> SwathProportionPredictedSingleChargeAvg = new List<double>();

//Loop through all the swaths of the same number and add to
            for (int swathNumber = 0; swathNumber < swathBoundaries.Count(); swathNumber++)
            {
                int track = 0;

                double TICthisSwath = 0;

                var orderedMS2Scans = run.Ms2Scans.OrderBy(s => s.ScanStartTime)
                    .Where(x => x.MsLevel == 2 && x.IsolationWindowTargetMz == swathBoundaries[swathNumber]);
                foreach (var scan in orderedMS2Scans)
                {
                    mzTargetRange.Add(scan.IsolationWindowUpperOffset + scan.IsolationWindowLowerOffset);
                    TICthisSwath = TICthisSwath + scan.TotalIonCurrent;
                    swDensity.Add(scan.Density);
                    TotalSwathProportionPredictedSingleCharge.Add(scan.proportionChargeStateOne); //The chargestate one's we pick up is where there is a match for M+1. Therefore we need to double it to add the M.
                    track++;
                }
                mzTargetRange.Sort();
                medianMzTargetRange.Add(mzTargetRange.ElementAt(mzTargetRange.Count/2));
                numOfSwathPerGroup.Add(track);
                TICs.Add(TICthisSwath);
                TICthisSwath = 0;
                SwathProportionPredictedSingleChargeAvg.Add(TotalSwathProportionPredictedSingleCharge.Average());
                TotalSwathProportionPredictedSingleCharge.Clear();
                swDensity.Sort();
                swDensity50.Add(Math.Truncate(Math.Ceiling(swDensity.Average())));
                swDensityIQR.Add(Math.Truncate(Math.Ceiling(InterQuartileRangeCalculator.CalcIQR(swDensity))));
                swDensity.Clear();
            }

            for (int num = 0; num < swathBoundaries.Count(); num++)
            {
                SwathProportionOfTotalTIC.Add((TICs[num] / totalTIC));
            }

            SwathMetrics swathMetrics = new SwathMetrics(swathBoundaries.Count(), totalTIC, numOfSwathPerGroup, mzTargetRange, TICs, swDensity50, swDensityIQR, SwathProportionOfTotalTIC, SwathProportionPredictedSingleChargeAvg);
            return swathMetrics;
        }

    }
   
}
