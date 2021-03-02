#nullable enable

using System.Collections.Generic;
using System.Collections.Specialized;

namespace LibraryParser
{
    public class Library
    {
        public Library()
        {
            Proteins = new Dictionary<string, Protein>();
            Peptides = new Dictionary<string, Peptide>();
            TransitionList = new OrderedDictionary();
            ProteinDecoys = new Dictionary<string, Protein>();
            RtList = new OrderedDictionary();
        }

        public class Protein
        {
            public string Id { get; }
            // public string? Accession { get; set; } Unused. TODO: Remove.
            public IList<string> AssociatedPeptideIds { get; }
            public IList<string>? UniprotIds { get; set; }

            public Protein(string id)
                : this(id, new List<string>())
            {
            }

            public Protein(string id, IList<string> associatedPeptideIds)
            {
                Id = id;
                AssociatedPeptideIds = associatedPeptideIds;
            }
        }

        public class Peptide
        {
            public string Id { get; }
            public string? ProteinId { get; set; }
            public string Sequence { get; }
            public string? GroupLabel { get; set; }
            public int ChargeState { get; set; }
            public double RetentionTime { get; set; }
            public double CollisionEnergy { get; set; }
            public List<Transition> AssociatedTransitions { get; } = new List<Transition>();
            public List<string> AssociatedTransitionIds { get; } = new List<string>();

            public Peptide(string id, string sequence) {
                Id = id;
                Sequence = sequence;
            }
        }

        public class Transition
        {
            public string Id { get; }
            public double PrecursorMz { get; set; }
            public double ProductMz { get; set; }
            public int ProductIonChargeState { get; set; }
            public int ProductIonSeriesOrdinal { get; set; }
            public int ProductInterpretationRank { get; set; }
            public double ProductIonIntensity { get; set; }
            public string? IonType { get; set; }
            public string PeptideId { get; set; }

            public Transition(string id, string peptideId)
            {
                Id = id;
                PeptideId = peptideId;
            }
        }

        public IDictionary<string, Protein> Proteins { get; }
        public IDictionary<string, Protein> ProteinDecoys { get; }
        public OrderedDictionary RtList { get; }
        public IDictionary<string, Peptide> Peptides { get; }
        public OrderedDictionary TransitionList { get; }
    }
}
