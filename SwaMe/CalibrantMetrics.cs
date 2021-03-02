#nullable enable

using System.Collections.Generic;

namespace SwaMe
{
    public class CalibrantMetrics
    {
        public List<double> PeakWidths { get; }
        public List<double> TailingFactor { get; }
        public List<double> PeakPrecision { get; }

        public CalibrantMetrics(List<double> peakWidths, List<double> tailingFactor, List<double> peakPrecision)
        {
            PeakWidths = peakWidths;
            TailingFactor = tailingFactor;
            PeakPrecision = peakPrecision;
        }
    }
}
