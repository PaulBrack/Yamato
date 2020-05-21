#nullable enable

namespace MzmlParser
{
    public interface IScanFactory<TScan>
        where TScan: IScan
    {
        TScan CreateScan();
    }
}
