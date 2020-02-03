using System.Collections.Generic;

namespace SwaMe
{
    public class CalibrantMetrics
    {
        public List<double> Peakwidths;
        public List<double> TailingFactor;
        public List<double> PeakPrecision;
        public CalibrantMetrics(List<double> Peakwidths, List<double> TailingFactor, List<double>PeakPrecision)
        {
            this.Peakwidths = Peakwidths;
            this.TailingFactor = TailingFactor;
            this.PeakPrecision = PeakPrecision;
        }
    }
}





