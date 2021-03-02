#nullable enable

using System.Collections.Generic;

namespace SwaMe
{
    /// <summary>
    /// Immutable
    /// </summary>
    public class SwathMetrics
    {
        public IList<double> SwathTargets { get; }
        public double TotalTIC { get; }
        public IList<int> NumOfSwathPerGroup { get; }
        public IList<double?> MzRange { get; }
        public IList<double> TICs { get; }
        public IList<double> SwDensity50 { get; }
        public IList<double?> SwDensityIQR { get; }
        public IList<double> SwathProportionOfTotalTIC { get; }
        public IList<double> SwathProportionPredictedSingleChargeAvg { get; }

        public SwathMetrics(IList<double> swathTargets, double totalTIC, IList<int> numOfSwathPerGroup, IList<double?> mzRange, IList<double> tics, IList<double> swDensity50, IList<double?> swDensityIQR,
        IList<double> swathProportionOfTotalTIC, IList<double> swathProportionPredictedSingleChargeAvg)
        {
            SwathTargets = swathTargets;
            TotalTIC = totalTIC;
            NumOfSwathPerGroup = numOfSwathPerGroup;
            MzRange = mzRange;
            TICs = tics;
            SwDensity50 = swDensity50;
            SwDensityIQR = swDensityIQR;
            SwathProportionOfTotalTIC = swathProportionOfTotalTIC;
            SwathProportionPredictedSingleChargeAvg = swathProportionPredictedSingleChargeAvg;
        }
    }
}
