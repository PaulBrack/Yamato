using System.Collections.Generic;

namespace MzmlParser
{
    public class Scan : IGenericScan
    {
        public int Cycle { get; set; }
        public int? MsLevel { get; set; }
        public double BasePeakIntensity { get; set; }
        public double BasePeakMz { get; set; }
        public double TotalIonCurrent { get; set; }
        public double ScanStartTime { get; set; }
        public double IsolationWindowTargetMz { get; set; }
        public double IsolationWindowUpperOffset { get; set; }
        public double IsolationWindowLowerOffset { get; set; }
        public double IsolationWindowUpperBoundary { get; set; }
        public double IsolationWindowLowerBoundary { get; set; }
        public int RTsegment { get; set; }
        public int Density { get; set; }
        public IList<SpectrumPoint> Spectrum { get; set; }
        public double ProportionChargeStateOne { get; set; }
    }
}
