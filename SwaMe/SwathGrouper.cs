
using System.Collections.Generic;
using System.Linq;
using System;
using SwaMe.Pipeline;

namespace SwaMe
{
    public class SwathGrouper
    {
        public SwathMetrics GroupBySwath(Run<Scan> run)
        {
            
            //Create list of target isolationwindows to serve as swathnumber
            List<double> swathTargets = new List<double>();
            // Force ordering of swathTargets so that our tests don't depend on the order that Distinct() happens to impose.
            swathTargets = run.Ms2Scans.Select(x => x.IsolationWindowTargetMz).Distinct().OrderBy(x => x).ToList();
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
            List<double?> swDensityIQR = new List<double?>();
            List<double> mzTargetRange = new List<double>();
            List<double> averageMzTargetRange = new List<double>();
            List<double> SwathProportionOfTotalTIC = new List<double>();
            List<double> TotalSwathProportionPredictedSingleCharge = new List<double>();
            List<double> SwathProportionPredictedSingleChargeAvg = new List<double>();

//Loop through all the swaths of the same number and add to
            for (int swathNumber = 0; swathNumber < swathTargets.Count(); swathNumber++)
            {
                int track = 0;

                double TICthisSwath = 0;

                var orderedMS2Scans = run.Ms2Scans
                    .Where(x => x.IsolationWindowTargetMz == swathTargets[swathNumber]); // Turns out ordering is not required; users of this either don't care about order or sort their own results.
                foreach (var scan in orderedMS2Scans)
                {
                    mzTargetRange.Add(scan.IsolationWindowUpperOffset + scan.IsolationWindowLowerOffset);
                    TICthisSwath += scan.TotalIonCurrent;
                    swDensity.Add(scan.Density);
                    TotalSwathProportionPredictedSingleCharge.Add(scan.ProportionChargeStateOne); //The chargestate one's we pick up is where there is a match for M+1. Therefore we need to double it to add the M.
                    track++;
                }
                averageMzTargetRange.Add(mzTargetRange.Average());
                numOfSwathPerGroup.Add(track);
                TICs.Add(TICthisSwath);
                TICthisSwath = 0;
                SwathProportionPredictedSingleChargeAvg.Add(TotalSwathProportionPredictedSingleCharge.Average());
                TotalSwathProportionPredictedSingleCharge.Clear();
                swDensity.Sort();
                swDensity50.Add(Math.Truncate(Math.Ceiling(swDensity.Average())));
                if (swDensity.Count > 4)
                    swDensityIQR.Add(Math.Ceiling(InterQuartileRangeCalculator.CalcIQR(swDensity)));
                else
                    swDensityIQR.Add(default);
                swDensity.Clear();
                mzTargetRange.Clear();
            }

            for (int num = 0; num < swathTargets.Count(); num++)
            {
                SwathProportionOfTotalTIC.Add(TICs[num] / totalTIC);
            }

            SwathMetrics swathMetrics = new SwathMetrics(swathTargets, totalTIC, numOfSwathPerGroup, averageMzTargetRange, TICs, swDensity50, swDensityIQR, SwathProportionOfTotalTIC, SwathProportionPredictedSingleChargeAvg);
            return swathMetrics;
        }

    }
   
}
