using System.Linq;
using System.Collections.Concurrent;

namespace MzmlParser
{
    public static class IrtPeptideMatcher
    {
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

        private static CandidateHit ChoosePeptideCandidate(Run run, ConcurrentBag<CandidateHit> chosenCandidates, string peptideSequence)
        {
            var a = run.IRTHits.Where(x => x.PeptideSequence == peptideSequence).ToList(); // pick the potential hits for this peptide
            a = a.Where(x => x.Intensities.Count() == a.OrderBy(y => y.Intensities.Count()).Last().Intensities.Count()).ToList(); // pick the hits matching the most transitions
            return a.OrderBy(x => x.Intensities.Min()).Last(); // pick the hit with the highest minimum intensity value
        }
    }
}
