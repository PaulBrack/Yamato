#nullable enable

using System.Collections.Generic;
using System.Collections.Concurrent;
using MzmlParser;

namespace SwaMe.Pipeline
{
    public class Run<TScan> : IRun<TScan>
        where TScan: IScan
    {
        public double StartTime { get; set; } = 0;
        public double LastScanTime { get; set; } = 1000000;
        public List<string> SourceFileTypes { get; } = new List<string>();
        public List<string> SourceFileNames { get; } = new List<string>();
        public string? SourceFilePath { get; set; }
        public List<string> SourceFileChecksums { get; } = new List<string>();
        public string? CompletionTime { get; set; }
        public List<TScan> Ms1Scans { get; } = new List<TScan>();
        public List<TScan> Ms2Scans { get; } = new List<TScan>();
        /// <remarks>
        /// BEWARE: BasePeaks has concurrent read-calculate-write access.  If writing, please lock on the BasePeaks object.
        /// It is not a concurrent datatype because read-calculate-write accesses are not amenable to being defended in that way.
        /// </remarks>
        public List<BasePeak> BasePeaks { get; } = new List<BasePeak>();
        public Chromatograms Chromatograms { get; } = new Chromatograms();
        public List<(double, double)>? IsolationWindows { get; set; }
        public int MissingScans { get; set; }
        public string? FilePropertiesAccession { get; set; }
        public ConcurrentBag<IRTPeak> IRTPeaks { get; set; } = new ConcurrentBag<IRTPeak>();
        public ConcurrentBag<CandidateHit> IRTHits { get; set; } = new ConcurrentBag<CandidateHit>();
        public AnalysisSettings? AnalysisSettings { get; set; }
        public string? StartTimeStamp { get; set; }
        public string? ID { get; set; }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (IScan scan in Ms1Scans)
                        scan.Dispose();
                    foreach (IScan scan in Ms2Scans)
                        scan.Dispose();
                    Ms1Scans.Clear();
                    Ms2Scans.Clear();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
