using System.Collections.Generic;
using System.IO;
using MessagePack;

namespace MzmlParser
{
    public class Scan
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

        public string ScanId
        {
            get
            {
                return string.Format("MS{0}_{1}_{2}", MsLevel, IsolationWindowTargetMz, ScanStartTime);
            }
        }
    }

    [MessagePackObject]
    public class Spectrum
    {
        [Key(0)]
        public List<SpectrumPoint> SpectrumPoints
        {
            get;
            set;
        }
    }

    public class ScanAndTempProperties
    {
        public ScanAndTempProperties()
        {
            Scan = new Scan();
        }

        public Scan Scan { get; set; }
        public string Base64IntensityArray { get; set; }
        public string Base64MzArray { get; set; }
        public bool IntensityZlibCompressed { get; set; }
        public bool MzZlibCompressed { get; set; }
        public int IntensityBitLength { get; set; }
        public int MzBitLength { get; set; }
    }
}
