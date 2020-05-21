using System;

namespace MzmlParser
{
    public interface IScan : IDisposable
    {
        string Base64IntensityArray { get; set; }
        double BasePeakIntensity { get; set; }
        double BasePeakMz { get; set; }
        bool CacheSpectraToDisk { get; set; }
        int Cycle { get; set; }
        int Density { get; set; }
        double IsolationWindowLowerBoundary { get; set; }
        double IsolationWindowLowerOffset { get; set; }
        double IsolationWindowTargetMz { get; set; }
        double IsolationWindowUpperBoundary { get; set; }
        double IsolationWindowUpperOffset { get; set; }
        int? MsLevel { get; set; }
        double ProportionChargeStateOne { get; set; }
        int RTsegment { get; set; }
        string ScanId { get; }
        double ScanStartTime { get; set; }
        string TempDirectory { get; set; }
        double TotalIonCurrent { get; set; }
    }
}