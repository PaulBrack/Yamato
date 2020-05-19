#nullable enable

using ProtoBuf;
using System.Collections.Generic;
using System.IO;

namespace MzmlParser
{
    public class Scan
    {

        public Scan(bool cacheSpectraToDisk, string tempDirectory) 
        {
            CacheSpectraToDisk = cacheSpectraToDisk;
            TempDirectory = tempDirectory;
        }

        public Scan(bool cacheSpectraToDisk, int isolationWindowLowerOffset, int isolationWindowUpperOffset, double scanStartTime, int msLevel, int density, string tempDirectory)
        {
            CacheSpectraToDisk = cacheSpectraToDisk;
            IsolationWindowLowerOffset = isolationWindowLowerOffset;
            IsolationWindowUpperOffset = isolationWindowUpperOffset;
            ScanStartTime = scanStartTime;
            MsLevel = msLevel;
            Density = density;
            TempDirectory = tempDirectory;
        }

        public Scan(bool cacheSpectraToDisk, int isolationWindowLowerOffset, int isolationWindowUpperOffset, double scanStartTime, int msLevel, int density, int cycle, double totalIonCurrent, string tempDirectory)
        {
            CacheSpectraToDisk = cacheSpectraToDisk;
            IsolationWindowLowerOffset = isolationWindowLowerOffset;
            IsolationWindowUpperOffset = isolationWindowUpperOffset;
            ScanStartTime = scanStartTime;
            MsLevel = msLevel;
            Density = density;
            TotalIonCurrent = totalIonCurrent;
            Cycle = cycle;
            TempDirectory = tempDirectory;
        }

        public bool CacheSpectraToDisk { get; set; }
        public string? Base64IntensityArray { get; set; }
        public int Cycle { get; set; }
        public int? MsLevel { get; set; }
        public double BasePeakIntensity { get; set; }
        public double BasePeakMz { get; set; }
        public double TotalIonCurrent { get; set; }
        public double ScanStartTime { get; set; }
        public double IsolationWindowTargetMz { get; set; }
        public double IsolationWindowUpperOffset { get; set; } = 100000;
        public double IsolationWindowLowerOffset { get; set; } = 100000;
        public double IsolationWindowUpperBoundary { get; set; }
        public double IsolationWindowLowerBoundary { get; set; }
        public int RTsegment { get; set; }
        public int Density { get; set; }
        public double ProportionChargeStateOne { get; set; }
        public string TempDirectory { get; set;  }

        private Spectrum? m_Spectrum;

        public string ScanId
        {
            get
            {
                return string.Format("{0}_{1}_{2}", MsLevel, Cycle, IsolationWindowTargetMz);
            }
        }

        private string TempFileName
        {
            get
            {
                return Path.Combine(TempDirectory, string.Format("Yamato_{0}.tempscan", ScanId));
            }
        }
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

        public int SpectrumXmlBase64Line { get; set; }

        public int SpectrumXmlBase64LinePos { get; set; }

        public int SpectrumXmlBase64Length { get; set; }

        public int SpectrumXmlBase64BinaryOffset { get; set; }

        public void DeleteTempFile()
        {
            File.Delete(TempFileName);
        }
    }

    [ProtoContract]
    public class Spectrum
    {
        [ProtoMember(1)]
        public virtual IList<SpectrumPoint>? SpectrumPoints { get; set; }
    }

    public class ScanAndTempProperties
    {
        public ScanAndTempProperties(bool cacheSpectra, string tempDirectory)
        {
            Scan = new Scan(cacheSpectra, tempDirectory);
        }

        public Scan Scan { get; set; }
        public string? Base64IntensityArray { get; set; }
        public string? Base64MzArray { get; set; }
        public bool IntensityZlibCompressed { get; set; }
        public bool MzZlibCompressed { get; set; }
        public int IntensityBitLength { get; set; }
        public int MzBitLength { get; set; }
    }


}
