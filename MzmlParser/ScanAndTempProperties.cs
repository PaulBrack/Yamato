#nullable enable

namespace MzmlParser
{
    public class ScanAndTempProperties<TScan>
        where TScan: IScan
    {
        public ScanAndTempProperties(IScanFactory<TScan> scanFactory)
        {
            Scan = scanFactory.CreateScan();
        }

        public TScan Scan { get; set; }
        public Base64StringAndDecodingHints? Intensities { get; set; }
        public Base64StringAndDecodingHints? Mzs { get; set; }
    }
}
