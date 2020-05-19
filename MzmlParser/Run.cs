#nullable enable

using System.Collections.Generic;
using System.Collections.Concurrent;
using LibraryParser;

namespace MzmlParser
{
    public class Run
    {
        public double StartTime { get; set; } = 0;
        public double LastScanTime { get; set; } = 1000000;
        public List<string> SourceFileTypes { get; set; } = new List<string>();
        public List<string> SourceFileNames { get; set; } = new List<string>();
        public string? SourceFilePath { get; set; }
        public List<string> SourceFileChecksums { get; set; } = new List<string>();
        public string? CompletionTime { get; set; }
        public List<Scan> Ms1Scans { get; set; } = new List<Scan>();
        public ConcurrentBag<Scan> Ms2Scans { get; set; } = new ConcurrentBag<Scan>();
        public ConcurrentBag<BasePeak> BasePeaks { get; set; } = new ConcurrentBag<BasePeak>();
        public Chromatograms Chromatograms { get; set; } = new Chromatograms();
        public List<(double, double)>? IsolationWindows { get; set; }
        public int MissingScans { get; set; }
        public string? FilePropertiesAccession;
        public ConcurrentBag<IRTPeak> IRTPeaks { get; set; } = new ConcurrentBag<IRTPeak>();
        public ConcurrentBag<CandidateHit> IRTHits { get; set; } = new ConcurrentBag<CandidateHit>();
        public AnalysisSettings? AnalysisSettings { get; set; }
        public string? StartTimeStamp { get; set; }
        public string? ID { get; set; }
    }

    public class Chromatograms
    {
        public List<(double, double)>? Ms1Tic { get; set; }
        public List<(double, double)>? Ms2Tic { get; set; }
        public List<(double, double, double)>? CombinedTic { get; set; }
        public List<(double, double)>? Ms1Bpc { get; set; }
        public List<(double, double)>? Ms2Bpc { get; set; }
    }
    public class AnalysisSettings
    {
        public double MassTolerance { get; set; }
        public double RtTolerance { get; set; }
        public Library? IrtLibrary { get; set; }
        public double IrtMassTolerance { get; set; }
        public double IrtMinIntensity { get; set; }
        public int IrtMinPeptides { get; set; }
        public void SetGlobalMassTolerance (int tolerance)
        {
            MassTolerance = tolerance;
            IrtMassTolerance = tolerance;
        }
        public bool CacheSpectraToDisk { get; set; }
        public int MinimumIntensity { get; set; }
        public string? TempFolder { get; set; }
    }
}
