using System.Collections.Generic;
using System.Collections.Specialized;

namespace LibraryParser
{
    public class Library
    {
        public Library()
        {
            this.ProteinList = new OrderedDictionary();
            this.PeptideList = new OrderedDictionary();
            this.TransitionList = new OrderedDictionary();
            this.ProteinDecoyList = new OrderedDictionary();
            this.RtList = new OrderedDictionary();
        }

        public class Protein
        {
            public IList<string>? UniprotIds { get; set; }
        }

        public class Peptide
        {
            public string Id;
            public string ProteinId;
            public string Sequence;
            public string GroupLabel;
            public int ChargeState;
            public double RetentionTime;
            public double CollisionEnergy;
            public List<Transition> AssociatedTransitions;
            public List<string> AssociatedTransitionIds;

            public Peptide() {
                AssociatedTransitions = new List<Transition>();
            }
        }

        public class Transition
        {
            public string Id;
            public double PrecursorMz;
            public double ProductMz;
            public int ProductIonChargeState;
            public int ProductIonSeriesOrdinal;
            public int ProductInterpretationRank;
            public double ProductIonIntensity;
            public string IonType;
            public string PeptideId;
        }

        public OrderedDictionary ProteinList
        {
            get;
            set;
        }

        public OrderedDictionary ProteinDecoyList
        {
            get;
            set;
        }

        public OrderedDictionary RtList
        {
            get;
            set;
        }

        public OrderedDictionary PeptideList
        {
            get;
            set;
        }

        public OrderedDictionary TransitionList
        {
            get;
            set;
        }

        public OrderedDictionary TransitionList { get; }
    }
}
