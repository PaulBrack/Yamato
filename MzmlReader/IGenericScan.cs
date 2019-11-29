using System.Collections.Generic;

namespace MzmlParser
{
    public interface IGenericScan
    {
        int Cycle { get; set; }
        int? MsLevel { get; set; }
        double BasePeakIntensity { get; set; }
        double BasePeakMz { get; set; }
        double TotalIonCurrent { get; set; }
        double ScanStartTime { get; set; }
        double IsolationWindowTargetMz { get; set; }
        double IsolationWindowUpperOffset { get; set; }
        double IsolationWindowLowerOffset { get; set; }
        double IsolationWindowUpperBoundary { get; set; }
        double IsolationWindowLowerBoundary { get; set; }
        int RTsegment { get; set; }
        int Density { get; set; }
        IList<SpectrumPoint> Spectrum { get; set; }
    }
}
