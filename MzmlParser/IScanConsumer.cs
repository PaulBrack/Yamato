#nullable enable

namespace MzmlParser
{
    /// <summary>
    /// To consume scans as they are issued, implement this interface and Register() yourself on an MzmlParser instance before calling LoadMzml().
    /// </summary>
    /// <remarks>
    /// Calls to this interface may be made concurrently on many different threads from MzmlParser. Implementors should ensure that their implementation is thread-safe.
    /// The only guarantee is that a scan later in the file will not be issued to the same thread before a scan earlier in the file.
    /// </remarks>
    public interface IScanConsumer<TScan, TRun>
        where TScan : IScan
        where TRun : IRun<TScan>
    {
        /// <summary>
        /// One of the scans in the input is scan, within the context of run.
        /// The consumer should treat the scan as read-only; while the parser will not hold any reference to the scan once this call returns, there may be other consumers.
        /// </summary>
        void Notify(TScan scan, float[]? mzs, float[]? intensities, TRun run);

        /// <summary>
        /// If true, this consumer requires the binary data of m/z and intensity from the reader.
        /// The reader contracts to supply these fields if RequiresBinaryData is true.  If false, it might not.
        /// </summary>
        bool RequiresBinaryData { get; }
    }
}
