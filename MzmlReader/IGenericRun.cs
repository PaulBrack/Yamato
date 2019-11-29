using System.Collections.Generic;
using System.Collections.Concurrent;

namespace MzmlParser
{
    public interface IGenericRun<TScan>
        where TScan: IGenericScan
    {
        double StartTime { get; set; }
        double LastScanTime { get; set; }
        string SourceFileType { get; set; }
        string SourceFileName { get; set; }
        string SourceFilePath { get; set; }
        string SourceFileChecksum { get; set; }
        string CompletionTime { get; set; }
        IList<TScan> Ms1Scans { get; set; }
        ConcurrentBag<TScan> Ms2Scans { get; set; }
        Chromatograms Chromatograms { get; set; }
        IList<(double, double)> IsolationWindows { get; set; }
        int MissingScans { get; set; }
        string FilePropertiesAccession { get; set; }
        AnalysisSettings AnalysisSettings { get; set; }
    }
}