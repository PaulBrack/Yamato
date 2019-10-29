using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System;

namespace MzmlParser
{
    public static class IrtPeptideMatcher
    {
        private static CountdownEvent cde = new CountdownEvent(1);

        public static void ChooseIrtPeptides(Run run)
        {
            var chosenCandidates = new ConcurrentBag<CandidateHit>();
            foreach (string peptideSequence in run.IRTHits.Select(x => x.PeptideSequence).Distinct())
            {
                var candidate = ChoosePeptideCandidate(run, chosenCandidates, peptideSequence);
                chosenCandidates.Add(candidate);
            }
            run.IRTHits = chosenCandidates;
        }

        private static void AddIrtSpectra(Run run)
        {
            foreach (CandidateHit candidateHit in run.IRTHits)
            {
                cde.AddCount();
                ThreadPool.QueueUserWorkItem(state => FindIrtSpectra(run, candidateHit));
            }
        }

        private static void FindIrtSpectra(Run run, CandidateHit candidateHit)
        {
            var matchingScans = run.Ms2Scans.Where(x => Math.Abs(x.ScanStartTime - candidateHit.RetentionTime) < run.AnalysisSettings.RtTolerance
                               && candidateHit.PrecursorTargetMz > x.IsolationWindowLowerBoundary
                               && candidateHit.PrecursorTargetMz < x.IsolationWindowUpperBoundary);

            foreach (var matchingScan in matchingScans)
            {
                foreach (double targetMz in candidateHit.ProductTargetMzs)
                {
                    var matchingSpectra = matchingScan.Spectrum.Where(x => Math.Abs(x.Mz - targetMz) < run.AnalysisSettings.IrtMassTolerance);
                    if (matchingSpectra.Any())
                    {
                        IRTPeak iRTPeak = new IRTPeak();
                        //TODO - hydrate irtpeak

                        run.IRTPeaks.Add(iRTPeak);
                    }
                }
            }
            cde.Signal();
        }

        private static CandidateHit ChoosePeptideCandidate(Run run, ConcurrentBag<CandidateHit> chosenCandidates, string peptideSequence)
        {
            var hits = run.IRTHits.Where(x => x.PeptideSequence == peptideSequence).ToList(); // pick the potential hits for this peptide
            hits = hits.Where(x => x.Intensities.Count() == hits.OrderBy(y => y.Intensities.Count()).Last().Intensities.Count()).ToList(); // pick the hits matching the most transitions
            return hits.OrderBy(x => x.Intensities.Min()).Last(); // pick the hit with the highest minimum intensity value
        }
    }
}
