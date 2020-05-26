#nullable enable

namespace MzmlParser
{
    internal class ScanAndTempProperties<TScan, TRun>
        where TScan: IScan
        where TRun: IRun<TScan>
    {
        public ScanAndTempProperties(TScan scan, TRun run)
        {
            Scan = scan;
            Run = run;
        }

        public TRun Run { get; }
        public TScan Scan { get; }
        public Base64StringAndDecodingHints? Intensities { get; set; }
        public Base64StringAndDecodingHints? Mzs { get; set; }
    }
}
