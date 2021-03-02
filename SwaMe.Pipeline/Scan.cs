#nullable enable

using MzmlParser;
using ProtoBuf;
using System;
using System.IO;

namespace SwaMe.Pipeline
{
    public class Scan : IScan
    {
        public Scan(bool cacheSpectraToDisk, string? tempDirectory)
        {
            CacheSpectraToDisk = cacheSpectraToDisk;
            TempDirectory = tempDirectory;
        }

        public Scan(bool cacheSpectraToDisk, double? isolationWindowTargetMz, double? isolationWindowLowerOffset, double? isolationWindowUpperOffset, double scanStartTime, TandemMsLevel msLevel, int density, string? tempDirectory = default)
        {
            CacheSpectraToDisk = cacheSpectraToDisk;
            IsolationWindowTargetMz = isolationWindowTargetMz;
            IsolationWindowLowerOffset = isolationWindowLowerOffset;
            IsolationWindowUpperOffset = isolationWindowUpperOffset;
            ScanStartTime = scanStartTime;
            MsLevel = msLevel;
            Density = density;
            TempDirectory = tempDirectory;
        }

        public Scan(bool cacheSpectraToDisk, double? isolationWindowTargetMz, double? isolationWindowLowerOffset, double? isolationWindowUpperOffset, double scanStartTime, TandemMsLevel msLevel, int density, int cycle, double totalIonCurrent, string? tempDirectory = default)
        {
            CacheSpectraToDisk = cacheSpectraToDisk;
            IsolationWindowTargetMz = isolationWindowTargetMz;
            IsolationWindowLowerOffset = isolationWindowLowerOffset;
            IsolationWindowUpperOffset = isolationWindowUpperOffset;
            ScanStartTime = scanStartTime;
            MsLevel = msLevel;
            Density = density;
            TotalIonCurrent = totalIonCurrent;
            Cycle = cycle;
            TempDirectory = tempDirectory;
        }

        public bool CacheSpectraToDisk { get; }
        public int Cycle { get; set; }
        public TandemMsLevel MsLevel { get; set; }
        public double BasePeakIntensity { get; set; }
        public double BasePeakMz { get; set; }
        public double TotalIonCurrent { get; set; }
        public double ScanStartTime { get; set; }
        public double? IsolationWindowTargetMz { get; set; }
        public double? IsolationWindowUpperOffset { get; set; }
        public double? IsolationWindowLowerOffset { get; set; }
        public double? IsolationWindowUpperBoundary => IsolationWindowTargetMz + IsolationWindowUpperOffset;
        public double? IsolationWindowLowerBoundary => IsolationWindowTargetMz - IsolationWindowLowerOffset;
        public int RTsegment { get; set; }
        public int Density { get; set; }
        public double ProportionChargeStateOne { get; set; }
        public string? TempDirectory { get; }

        private Spectrum? m_Spectrum;

        public string ScanId => string.Format("{0}_{1}_{2}", MsLevel, Cycle, IsolationWindowTargetMz);

        private string TempFileName => Path.Combine(TempDirectory, string.Format("Yamato_{0}.tempscan", ScanId));
        public Spectrum? Spectrum
        {
            get
            {
                if (CacheSpectraToDisk)
                {
                    using var file = File.OpenRead(TempFileName);
                    return (Spectrum)Serializer.Deserialize(typeof(Spectrum), file);
                }
                else
                {
                    return m_Spectrum;
                }
            }
            set
            {
                if (CacheSpectraToDisk)
                {
                    using var file = File.Create(TempFileName);
                    Serializer.Serialize(file, value);
                }
                else
                {
                    m_Spectrum = value;
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                if (CacheSpectraToDisk)
                    File.Delete(TempFileName);
                m_Spectrum = null;

                disposedValue = true;
            }
        }

        ~Scan()
        {
          // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
          Dispose(false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
