#nullable enable

namespace MzmlParser
{
    public interface IRunFactory<TScan, TRun>
        where TScan: IScan
        where TRun: IRun<TScan>
    {
        TRun CreateRun();
    }
}
