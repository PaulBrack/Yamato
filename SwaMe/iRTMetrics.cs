using System.Collections.Generic;

namespace SwaMe
{
    public class iRTMetrics
    {
        public List<double> Peakwidths;
        public List<double> TailingFactor;
        public iRTMetrics(List<double> Peakwidths, List<double> TailingFactor)
        {
            this.Peakwidths = Peakwidths;
            this.TailingFactor = TailingFactor;
        }
    }
}





