using System;
using System.Collections.Generic;
using System.Text;

namespace MzmlParser
{
    public class Scan
    {
        public int ExperimentIndex { get; set; }
        public int MsLevel { get; set; }
        public double BasePeakIntensity { get; set; }
        public double BasePeakMz { get; set; }
        public double TotalIonCurrent { get; set; }  
        public double ScanStartTime { get; set; }
        public double IsolationWindowTargetMz { get; set; }
        public double IsolationWindowUpperOffset { get; set; }
        public double IsolationWindowLowerOffset { get; set; }
    }
}
