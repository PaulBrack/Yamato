using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System;
using LibraryParser;
using System.Collections.Generic;

namespace MzmlParser
{
    public static class IrtPeptideMatcher
    {
        private static CountdownEvent cde = new CountdownEvent(1);
        public static CancellationToken _cancellationToken { get; set; }


        public static void ChooseIrtPeptides(Run run)
        {
            var chosenCandidates = new ConcurrentBag<CandidateHit>();
            foreach (string peptideSequence in run.IRTHits.Select(x => x.PeptideSequence).Distinct())
            {
                var candidate = ChoosePeptideCandidate(run, peptideSequence);
                chosenCandidates.Add(candidate);
            }
            run.IRTHits = chosenCandidates;
            AddIrtSpectra(run);
        }

        private static void AddIrtSpectra(Run run)
        {
            foreach (CandidateHit candidateHit in run.IRTHits)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException("Reading MZML was cancelled");
                }
                cde.AddCount();
                ThreadPool.QueueUserWorkItem(state => FindIrtSpectra(run, candidateHit));
            }
            cde.Signal();
            while (cde.CurrentCount > 1)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException("Reading MZML was cancelled");
                }
                Thread.Sleep(500);
            }
            cde.Wait();
            cde.Reset(1);

        }

        private static void FindIrtSpectra(Run run, CandidateHit candidateHit)
        {
            var matchingScans = run.Ms2Scans.Where(x => Math.Abs(x.ScanStartTime - candidateHit.RetentionTime) < run.AnalysisSettings.RtTolerance/*
                               && candidateHit.PrecursorTargetMz > x.IsolationWindowLowerBoundary
                               && candidateHit.PrecursorTargetMz < x.IsolationWindowUpperBoundary*/);

            var libraryPeptides = run.AnalysisSettings.IrtLibrary.PeptideList.Values.Cast<Library.Peptide>().ToList();
            List<SpectrumPoint> matchingSpectrum = new List<SpectrumPoint>();
            Library.Peptide peptide = new Library.Peptide();

            foreach (var matchingScan in matchingScans)
            {
                foreach (double targetMz in candidateHit.ProductTargetMzs)
                {
                    var spectrum = matchingScan.Spectrum;
                    if (spectrum != null && spectrum.SpectrumPoints != null && spectrum.SpectrumPoints.Count > 0)
                    {
                        var match = matchingScan.Spectrum.SpectrumPoints.Where(x => Math.Abs(x.Mz - targetMz) < run.AnalysisSettings.IrtMassTolerance).ToList();
                        if (match.Any())
                            matchingSpectrum.Add(match.First());//In a scan only one point is taken for each transition
                    }
                }
            }
            if (matchingSpectrum.Any())
            {
                peptide = libraryPeptides.Where(x => x.Sequence == candidateHit.PeptideSequence).Single();

            }

            IRTPeak iRTPeak = new IRTPeak();
            iRTPeak.RetentionTime = candidateHit.RetentionTime;
            iRTPeak.Mz = candidateHit.PrecursorTargetMz;
            iRTPeak.AssociatedTransitions = peptide.AssociatedTransitions;
            iRTPeak.Spectrum = matchingSpectrum;
            run.IRTPeaks.Add(iRTPeak);
            cde.Signal();
        }

        private static CandidateHit ChoosePeptideCandidate(Run run, string peptideSequence)
        {


            var hits = run.IRTHits.Where(x => x.PeptideSequence == peptideSequence).ToList(); // pick the potential hits for this peptide
            hits = hits.Where(x => x.Intensities.Count() == hits.OrderBy(y => y.Intensities.Count()).Last().Intensities.Count()).ToList(); // pick the hits matching the most transitions
            var bestHit = hits.OrderBy(x => x.Intensities.Min()).Last(); // pick the hit with the highest minimum intensity value




            //Let's pull out the reference peptide
            var refPeptideTransitionMzs = run.AnalysisSettings.IrtLibrary.PeptideList.Values.Cast<Library.Peptide>().ToList().Where(x => x.Sequence == peptideSequence).Single().AssociatedTransitions.Select(x => x.ProductMz).OrderBy(x => x);
            var bestHitTransitionMzs = bestHit.ActualMzs.OrderBy(x => x);

            foreach (var mz in bestHitTransitionMzs)
            {
                var possHit = refPeptideTransitionMzs.Where(x => Math.Abs(x - mz) <= run.AnalysisSettings.IrtMassTolerance);
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
