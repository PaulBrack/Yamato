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