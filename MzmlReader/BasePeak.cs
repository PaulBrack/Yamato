using System;
using System.Collections.Generic;
using System.Text;

namespace MzmlParser
{
    public class BasePeak
    {
        public double Mz { get; set; }
        public double retentionTime { get; set; }
        public List<(float, float)> Spectrum { get; set; } //intensity, m/z
    }
}
