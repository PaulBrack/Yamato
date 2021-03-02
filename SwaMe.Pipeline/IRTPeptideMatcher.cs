#nullable enable

using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System;
using LibraryParser;
using System.Collections.Generic;
using static LibraryParser.Library;

namespace SwaMe.Pipeline
{
    public static class IrtPeptideMatcher
    {
        private static readonly CountdownEvent cde = new CountdownEvent(1);
        public static CancellationToken _cancellationToken { get; set; }

        public static void ChooseIrtPeptides(Run<Scan> run)
        {
            ConcurrentBag<CandidateHit>? chosenCandidates = new ConcurrentBag<CandidateHit>();
            foreach (string peptideSequence in run.IRTHits.Select(x => x.PeptideSequence).Distinct())
            {
                var candidate = ChoosePeptideCandidate(run, peptideSequence);
                chosenCandidates.Add(candidate);
            }
            run.IRTHits = chosenCandidates;
            AddIrtSpectra(run);
        }

        private static void AddIrtSpectra(Run<Scan> run)
        {
            foreach (CandidateHit candidateHit in run.IRTHits)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException("AddIrtSpectra was cancelled");
                }
                cde.AddCount();
                ThreadPool.QueueUserWorkItem(state => FindIrtSpectra(run, candidateHit));
            }
            cde.Signal();
            while (cde.CurrentCount > 1)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException("AddIrtSpectra was cancelled");
                }
                Thread.Sleep(500);
            }
            cde.Wait();
            cde.Reset(1);
        }

        private static void FindIrtSpectra(Run<Scan> run, CandidateHit candidateHit)
        {
            IEnumerable<Scan>? matchingScans = run.Ms2Scans.Where(x => Math.Abs(x.ScanStartTime - candidateHit.RetentionTime) < run.AnalysisSettings.RtTolerance);

            var libraryPeptides = run.AnalysisSettings.IrtLibrary.Peptides.Values.ToList(); // TODO: Get this ONCE!
            List<SpectrumPoint> matchingSpectrum = new List<SpectrumPoint>();
            Peptide? peptide = default;

            foreach (var matchingScan in matchingScans)
            {
                foreach (double targetMz in candidateHit.ProductTargetMzs)
                {
                    var spectrum = matchingScan.Spectrum;
                    if (spectrum != null && spectrum.SpectrumPoints != null && spectrum.SpectrumPoints.Length > 0)
                    {
                        List<SpectrumPoint>? match = matchingScan.Spectrum.SpectrumPoints.Where(x => Math.Abs(x.Mz - targetMz) < run.AnalysisSettings.IrtMassTolerance).ToList();
                        if (match.Any())
                            matchingSpectrum.Add(match.First()); // In a scan only one point is taken for each transition
                    }
                }
            }
            if (matchingSpectrum.Any())
            {
                peptide = libraryPeptides.Where(x => x.Sequence == candidateHit.PeptideSequence).Single();
            }

            IRTPeak iRTPeak = new IRTPeak
            {
                RetentionTime = candidateHit.RetentionTime,
                Mz = candidateHit.PrecursorTargetMz,
                AssociatedTransitions = (null == peptide)
                    ? (IList<Transition>)Array.Empty<Transition>()
                    : peptide.AssociatedTransitions,
                Spectrum = matchingSpectrum
            };
            run.IRTPeaks.Add(iRTPeak);
            cde.Signal();
        }

        private static CandidateHit ChoosePeptideCandidate(Run<Scan> run, string peptideSequence)
        {
            List<CandidateHit> hits = run.IRTHits.Where(x => x.PeptideSequence == peptideSequence).ToList(); // pick the potential hits for this peptide
            hits = hits.Where(x => x.Intensities.Count() == hits.OrderBy(y => y.Intensities.Count()).Last().Intensities.Count()).ToList(); // pick the hits matching the most transitions
            CandidateHit bestHit = hits.OrderBy(x => x.Intensities.Min()).Last(); // pick the hit with the highest minimum intensity value

            //Let's pull out the reference peptide
            IOrderedEnumerable<double>? refPeptideTransitionMzs = run.AnalysisSettings.IrtLibrary.Peptides.Values.ToList().Where(x => x.Sequence == peptideSequence).Single().AssociatedTransitions.Select(x => x.ProductMz).OrderBy(x => x);
            IOrderedEnumerable<float>? bestHitTransitionMzs = bestHit.ActualMzs.OrderBy(x => x);

            foreach (var mz in bestHitTransitionMzs)
            {
                IEnumerable<double>? possHit = refPeptideTransitionMzs.Where(x => Math.Abs(x - mz) <= run.AnalysisSettings.IrtMassTolerance);
                if (possHit.Count() == 1)
                {
                    bestHit.TotalMassError += Math.Abs(possHit.First() - mz);
                    bestHit.TotalMassErrorPpm += bestHit.TotalMassError / possHit.First() * 1e6;
                }
            }

            bestHit.AverageMassError = bestHit.TotalMassError / bestHit.ActualMzs.Count;
            bestHit.AverageMassErrorPpm = bestHit.TotalMassErrorPpm / bestHit.ActualMzs.Count;

            return bestHit;
        }
    }
}
