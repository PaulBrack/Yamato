#nullable enable

using System.Collections.Generic;
using System.Collections.Concurrent;
using MzmlParser;
using System.Linq;
using System;

namespace SwaMe.Pipeline
{
    public class Run<TScan> : IRun<TScan>
        where TScan: IScan
    {
        public double? StartTime { get; set; }
        public double? LastScanTime { get; set; }
        public IList<string> SourceFileTypes { get; } = new List<string>();
        public IList<string> SourceFileNames { get; } = new List<string>();
        public string? SourceFilePath { get; set; }
        public IList<string> SourceFileChecksums { get; } = new List<string>();
        public string? CompletionTime { get; set; }
        public IList<TScan> Ms1Scans { get; } = new List<TScan>();
        public int Ms2ScanCount { get; private set; }
        public IList<TScan> Ms2Scans { get; } = new List<TScan>();
        /// <remarks>
        /// BEWARE: BasePeaks has concurrent read-calculate-write access.  If writing, please lock on the BasePeaks object, for example using AtomicallyRecordBasePeakAndRT.
        /// It is not a concurrent datatype because read-calculate-write accesses are not amenable to being defended in that way.
        /// </remarks>
        public List<BasePeak> BasePeaks { get; } = new List<BasePeak>();
        public Chromatograms Chromatograms { get; } = new Chromatograms();
        /// <remarks>
        /// BEWARE: IsolationWindows has concurrent Add access.  If writing, please lock on the IsolationWindows object.
        /// It is not a concurrent datatype because there is no ConcurrentSet.
        /// </remarks>
        public ISet<IsolationWindow> IsolationWindows { get; } = new HashSet<IsolationWindow>();
        public int MissingScans { get; set; }
        public string? FilePropertiesAccession { get; set; }
        public ConcurrentBag<IRTPeak> IRTPeaks { get; set; } = new ConcurrentBag<IRTPeak>();
        public ConcurrentBag<CandidateHit> IRTHits { get; set; } = new ConcurrentBag<CandidateHit>();
        public AnalysisSettings AnalysisSettings { get; }
        public string? StartTimeStamp { get; set; }
        public string? ID { get; set; }

        public Run(AnalysisSettings analysisSettings)
        {
            AnalysisSettings = analysisSettings;
        }

        /// <summary>
        /// Atomically test-and-set the BasePeak(s) and BpkRT(s) that we're going to use for this point.
        /// This may involve creating a new BasePeak, or a new BpkRt inside one or more existing BasePeaks.
        /// </summary>
        public void AtomicallyRecordBasePeakAndRT(float intensity, double mz, double retentionTime)
        {
            BasePeak[] candidates;
            lock (BasePeaks)
            {
                candidates = BasePeaks.Where(x => Math.Abs(x.Mz - mz) < AnalysisSettings.MassTolerance).ToArray();
                if (0 == candidates.Length)
                {
                    // No basepeak with this mz exists yet, so add it
                    BasePeaks.Add(new BasePeak(mz, retentionTime, intensity));
                    return;
                }
            }

            // If we get here, we have one or more matches within our mass tolerance.  Ensure there's a BpkRt in each match that's within rtTolerance of this scan.
            // Because we're doing this in each BasePeak match, rather than picking a "best" candidate, we don't need to do this inside the lock on all BasePeaks;
            // we just need to ensure that no other thread can be checking BpkRTs for the same candidate at the same time.
            foreach (BasePeak candidate in candidates)
            {
                bool found = false;
                lock (candidate)
                {
                    foreach (double rt in candidate.BpkRTs)
                    {
                        if (Math.Abs(rt - retentionTime) < AnalysisSettings.RtTolerance) // This is considered to be part of a previous basepeak
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) // No matching BpkRt in this BasePeak, so add one.
                    {
                        candidate.BpkRTs.Add(retentionTime);
                        candidate.Intensities.Add(intensity);
                    }
                }
            }
        }

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

        public int SafelyAddMs2Scan(TScan scan)
        {
            lock (Ms2Scans)
            {
                Ms2Scans.Add(scan);
                return ++Ms2ScanCount;
            }
        }
        #endregion
    }
}
