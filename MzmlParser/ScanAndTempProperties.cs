#nullable enable

namespace MzmlParser
{
    internal class ScanAndTempProperties<TScan>
        where TScan: IScan
    {
        public ScanAndTempProperties(TScan scan)
        {
            Scan = scan;
        }

        public TScan Scan { get; }
        public Base64StringAndDecodingHints? Intensities { get; set; }
        public Base64StringAndDecodingHints? Mzs { get; set; }
    }
}
