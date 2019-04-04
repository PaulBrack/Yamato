using System.Collections.Generic;

namespace MzmlParser
{
    public class BasePeak
    {
        public double Mz { get; set; }
        public double retentionTime { get; set; }
        public List<(float, float, float)> Spectrum { get; set; } //intensity, m/z, RT
    }
}
