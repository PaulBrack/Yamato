using System.Collections.Generic;

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
        public int Density { get; set; }
        public int swathNumber { get; set; }//This will be used to divide ms2 scans into mzsegments
        public double lowestmz { get; set; }//This is the lowest mz in all the swaths - will be used for calculating mz range covered by all swaths
        public double highestmz { get; set; }
        public List<SpectrumPoint> Spectrum { get; set; }
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
