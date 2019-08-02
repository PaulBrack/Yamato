using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace MzmlParser
{
    public class Run
    {
        public Run()
        {
            Ms1Scans = new List<Scan>();
            Ms2Scans = new ConcurrentBag<Scan>();
            Chromatograms = new Chromatograms();
            BasePeaks = new List<BasePeak>();
        }
        public String SourceFileType { get; set; }
        public String SourceFileName { get; set; }
        public String SourceFilePath { get; set; }
        public String SourceFileChecksum { get; set; }
        public List<Scan> Ms1Scans { get; set; }
        public ConcurrentBag<Scan> Ms2Scans { get; set; }
        public List<BasePeak> BasePeaks { get;set; }
        public Chromatograms Chromatograms { get; set; }
        public List<(double, double)> IsolationWindows { get; set; }
        public int MissingScans { get; set; }
    }

    public class Chromatograms
    {
        public List<(double, double)> Ms1Tic { get; set; }
        public List<(double, double)> Ms2Tic { get; set; }
        public List<(double, double)> Ms1Bpc { get; set; }
        public List<(double, double)> Ms2Bpc { get; set; }
    }
}
