using System.Linq;
using System.Collections.Concurrent;

namespace MzmlParser
{
    public static class IrtPeptideMatcher
    {
        public static ConcurrentBag<CandidateHit> ChooseIrtPeptides(Run run)
        {
            var chosenCandidates = new ConcurrentBag<CandidateHit>();
            foreach (string peptideSequence in run.IRTHits.Select(x => x.PeptideSequence).Distinct())
            {
                var a = run.IRTHits.Where(x => x.PeptideSequence == peptideSequence).ToList(); // pick the potential hits for this peptide
                a = a.Where(x => x.Intensities.Count() == a.OrderBy(y => y.Intensities.Count()).Last().Intensities.Count()).ToList(); // pick the hits matching the most transitions
                chosenCandidates.Add(a.OrderBy(x => x.Intensities.Min()).Last()); // pick the hit with the highest minimum intensity value
            }

            return chosenCandidates;
        }
    }
}
