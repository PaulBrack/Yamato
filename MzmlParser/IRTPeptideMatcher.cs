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

        public static void ChooseIrtPeptides(Run run)
        {
            var chosenCandidates = new ConcurrentBag<CandidateHit>();
            foreach (string peptideSequence in run.IRTHits.Select(x => x.PeptideSequence).Distinct())
            {
                var candidate = ChoosePeptideCandidate(run, chosenCandidates, peptideSequence);
                chosenCandidates.Add(candidate);
            }
            run.IRTHits = chosenCandidates;
            AddIrtSpectra(run);
        }

        private static void AddIrtSpectra(Run run)
        {
            foreach (CandidateHit candidateHit in run.IRTHits)
            {
                cde.AddCount();
                ThreadPool.QueueUserWorkItem(state => FindIrtSpectra(run, candidateHit));
            }
            cde.Signal();
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
                    var match = matchingScan.Spectrum.Where(x => Math.Abs(x.Mz - targetMz) < run.AnalysisSettings.IrtMassTolerance).ToList();
                    if(match.Any())
                    matchingSpectrum.Add(match.First());//In a scan only one point is taken for each transition
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

        private static CandidateHit ChoosePeptideCandidate(Run run, ConcurrentBag<CandidateHit> chosenCandidates, string peptideSequence)
        {
            var hits = run.IRTHits.Where(x => x.PeptideSequence == peptideSequence).ToList(); // pick the potential hits for this peptide
            hits = hits.Where(x => x.Intensities.Count() == hits.OrderBy(y => y.Intensities.Count()).Last().Intensities.Count()).ToList(); // pick the hits matching the most transitions
            return hits.OrderBy(x => x.Intensities.Min()).Last(); // pick the hit with the highest minimum intensity value
        }
    }
}
