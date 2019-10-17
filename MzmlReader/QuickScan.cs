namespace MzmlParser
{
    public class QuickScan 
    {
        public int Mslevel { get; set; }
        public double ScanStartTime { get; set; }
        public string Base64IntensityArray { get; set; }
        public int IntensityBitLength { get; set; }
        public bool IntensityZlibCompressed { get; set; }
        public string Base64MzArray { get; set; }
        public int MzBitLength { get; set; }
        public bool MzZlibCompressed { get; set; }
    }


}

