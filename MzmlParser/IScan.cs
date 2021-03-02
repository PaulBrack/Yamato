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
        /// <summary>
        /// Note that the lower offset is *always positive*, so to get to the lower boundary, use targetMz - lowerOffset.
        /// </summary>
        double? IsolationWindowLowerOffset { get; set; }
        double? IsolationWindowTargetMz { get; set; }
        double? IsolationWindowUpperOffset { get; set; }
        TandemMsLevel MsLevel { get; set; }
        double ScanStartTime { get; set; }
        double TotalIonCurrent { get; set; }
    }

    public enum TandemMsLevel
    {
        NotSet = 0,
        Ms1 = 1,
        Ms2 = 2
    }
}