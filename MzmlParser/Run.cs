using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using LibraryParser;

namespace MzmlParser
{
    public class Run
    {
        public Run()
        {
            Ms1Scans = new List<Scan>();
            Ms2Scans = new ConcurrentBag<Scan>();
            Chromatograms = new Chromatograms();
            BasePeaks = new ConcurrentBag<BasePeak>();
            IRTHits = new ConcurrentBag<CandidateHit>();
            IRTPeaks = new ConcurrentBag<IRTPeak>();
            SourceFileTypes = new List<String>();
            SourceFileNames = new List<String>();
            SourceFileChecksums = new List<String>();
        }
        public double StartTime { get; set; } = 0;
        public double LastScanTime { get; set; } = 1000000;
        public List<String> SourceFileTypes { get; set; }
        public List<String> SourceFileNames { get; set; }
        public String SourceFilePath { get; set; }
        public List<String> SourceFileChecksums { get; set; }
        public String CompletionTime { get; set; }
        public List<Scan> Ms1Scans { get; set; }
        public ConcurrentBag<Scan> Ms2Scans { get; set; }
        public ConcurrentBag<BasePeak> BasePeaks { get; set; }
        public Chromatograms Chromatograms { get; set; }
        public List<(double, double)> IsolationWindows { get; set; }
        public int MissingScans { get; set; }
        public String FilePropertiesAccession;
        public ConcurrentBag<IRTPeak> IRTPeaks { get; set; }
        public ConcurrentBag<CandidateHit> IRTHits { get; set; }
        public AnalysisSettings AnalysisSettings { get; set; }
    }

    public class Chromatograms
    {
        public List<(double, double)> Ms1Tic { get; set; }
        public List<(double, double)> Ms2Tic { get; set; }
        public List<(double, double)> Ms1Bpc { get; set; }
        public List<(double, double)> Ms2Bpc { get; set; }
    }
    public class AnalysisSettings
    {
        public double MassTolerance { get; set; }
        public double RtTolerance { get; set; }
        public Library IrtLibrary { get; set; }
        public double IrtMassTolerance { get; set; }
        public double IrtMinIntensity { get; set; }
        public int IrtMinPeptides { get; set; }
        public void SetGlobalMassTolerance (int tolerance)
        {
            MassTolerance = tolerance;
            IrtMassTolerance = tolerance;
        }
        public bool CacheSpectraToDisk { get; set; }
    }
}
