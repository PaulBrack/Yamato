#nullable enable

using System;
using System.Collections.Generic;

namespace MzmlParser
{
    public interface IRun<TScan> : IDisposable
        where TScan: IScan
    {
        string? CompletionTime { get; set; }
        string? FilePropertiesAccession { get; set; }
        string? ID { get; set; }
        double LastScanTime { get; set; }
        IList<TScan> Ms1Scans { get; }
        int Ms2ScanCount { get; }
        IList<TScan> Ms2Scans { get; }
        IList<string> SourceFileChecksums { get; }
        IList<string> SourceFileNames { get; }
        string? SourceFilePath { get; set; }
        IList<string> SourceFileTypes { get; }
        double StartTime { get; set; }
        string? StartTimeStamp { get; set; }

        /// <returns>The number of scans in Ms2Scans at the instant this scan was added.  Use for logging.</returns>
        int SafelyAddMs2Scan(TScan scan);
    }
}