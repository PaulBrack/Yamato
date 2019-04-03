using System;
using System.Collections.Generic;
using System.Text;

namespace MzmlParser
{
    public class BasePeak
    {
        public float Mz { get; set; }
        public float retentionTime { get; set; }
        public List<(float, float)> Xic { get; set; }
    }
}
