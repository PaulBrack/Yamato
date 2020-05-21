#nullable enable

using System;

namespace MzmlParser
{
    /// <summary>
    /// The bare minimum interface that any scan implementation should provide.
    /// </summary>
    public interface IScan : IDisposable
    {
        int Cycle { get; set; }
        double IsolationWindowLowerOffset { get; set; }
        double IsolationWindowTargetMz { get; set; }
        double IsolationWindowUpperOffset { get; set; }
        int? MsLevel { get; set; }
        double ScanStartTime { get; set; }
        double TotalIonCurrent { get; set; }
    }
}