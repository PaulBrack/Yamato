using System;
using System.Collections.Generic;
using System.Text;

namespace MzmlParser
{
    public class QuickScan
    {
        public int mslevel { get; set; }
        public double scanStartTime { get; set; }
        public string base64IntensityArray { get; set; }
        public int intensityBitLength { get; set; }
        public bool intensityZlibCompressed { get; set; }
        public string base64MzArray { get; set; }
        public int mzBitLength { get; set; }
        public bool mzZlibCompressed { get; set; }
    }
}

