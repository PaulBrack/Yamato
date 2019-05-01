using System.Collections.Generic;
using System.Collections.Specialized;

namespace TramlParser
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
            this.UniprotIdList = new OrderedDictionary();
        }

        public class Protein
        {
            public string Id;
            public string Accession;
            public List<string> AssociatedPeptideIds;
        }

        public class Peptide
        {
            public string Id;
            public string ProteinId;
            public string Sequence;
            public string GroupLabel;
            public int ChargeState;
            public double RetentionTime;
            public List<string> AssociatedTransitionIds;
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

        public OrderedDictionary UniprotIdList
        {
            get;
            set;
        }
    }
}
