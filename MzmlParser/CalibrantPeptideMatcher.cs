using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using System;
using LibraryParser;
using System.Collections.Generic;

namespace MzmlParser
{
    public static class CalibrantPeptideMatcher
    {
        private static CountdownEvent cde = new CountdownEvent(1);

        public static void ChooseCalibrantPeptides(Run run)
        {
            var chosenCandidates = new ConcurrentBag<CandidateHit>();
            foreach (string peptideSequence in run.CalibrantHits.Select(x => x.PeptideSequence).Distinct())
            {
                var candidate = ChoosePeptideCandidate(run, chosenCandidates, peptideSequence);
                chosenCandidates.Add(candidate);
            }
            run.CalibrantHits = chosenCandidates;
            AddCalibrantSpectra(run);
        }

        private static void AddCalibrantSpectra(Run run)
        {
            foreach (CandidateHit candidateHit in run.CalibrantHits)
            {
                cde.AddCount();
                ThreadPool.QueueUserWorkItem(state => FindCalibrantSpectra(run, candidateHit));
            }
            cde.Signal();
            cde.Wait();
            cde.Reset(1);

        }

        private static void FindCalibrantSpectra(Run run, CandidateHit candidateHit)
        {
            var matchingScans = run.Ms2Scans.Where(x => Math.Abs(x.ScanStartTime - candidateHit.RetentionTime) < run.AnalysisSettings.RtTolerance/*
                               && candidateHit.PrecursorTargetMz > x.IsolationWindowLowerBoundary
                               && candidateHit.PrecursorTargetMz < x.IsolationWindowUpperBoundary*/);

            var libraryPeptides = run.AnalysisSettings.CalibrantLibrary.PeptideList.Values.Cast<Library.Peptide>().ToList();
            List<SpectrumPoint> matchingSpectrum = new List<SpectrumPoint>();
            Library.Peptide peptide = new Library.Peptide();

            foreach (var matchingScan in matchingScans)
            {
                foreach (double targetMz in candidateHit.ProductTargetMzs)
                {
                    var spectrum = matchingScan.Spectrum;
                    if (spectrum != null && spectrum.SpectrumPoints != null && spectrum.SpectrumPoints.Count > 0)
                    {
                        var match = matchingScan.Spectrum.SpectrumPoints.Where(x => Math.Abs(x.Mz - targetMz) < run.AnalysisSettings.CalibrantMassTolerance).ToList();
                        if (match.Any())
                            matchingSpectrum.Add(match.First());//In a scan only one point is taken for each transition
                    }
                }
            }
            if (matchingSpectrum.Any())
            {
                peptide = libraryPeptides.Where(x => x.Sequence == candidateHit.PeptideSequence).Single();

            }

            CalibrantPeak CalibrantPeak = new CalibrantPeak();
            CalibrantPeak.RetentionTime = candidateHit.RetentionTime;
            CalibrantPeak.Mz = candidateHit.PrecursorTargetMz;
            CalibrantPeak.AssociatedTransitions = peptide.AssociatedTransitions;
            CalibrantPeak.Spectrum = matchingSpectrum;
            run.CalibrantPeaks.Add(CalibrantPeak);
            cde.Signal();
        }

        private static CandidateHit ChoosePeptideCandidate(Run run, ConcurrentBag<CandidateHit> chosenCandidates, string peptideSequence)
        {
            var hits = run.CalibrantHits.Where(x => x.PeptideSequence == peptideSequence).ToList(); // pick the potential hits for this peptide
            hits = hits.Where(x => x.Intensities.Count() == hits.OrderBy(y => y.Intensities.Count()).Last().Intensities.Count()).ToList(); // pick the hits matching the most transitions
            return hits.OrderBy(x => x.Intensities.Min()).Last(); // pick the hit with the highest minimum intensity value
        }
    }
}
