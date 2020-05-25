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
        List<TScan> Ms1Scans { get; }
        List<TScan> Ms2Scans { get; }
        List<string> SourceFileChecksums { get; }
        List<string> SourceFileNames { get; }
        string? SourceFilePath { get; set; }
        List<string> SourceFileTypes { get; }
        double StartTime { get; set; }
        string? StartTimeStamp { get; set; }
    }
}