using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using static LibraryParser.Library;

namespace LibraryParser.Test
{
    [TestClass]
    public class TramlReaderTest
    {
        private static Library library;

        [ClassInitialize]
        public static void Initialize(TestContext t)
        {
            library = new TraMLReader().LoadLibrary(@"libraries\metformin_assayLibrary050.traML");
        }

        [TestMethod]
        public void ProteinCountIsCorrect()
        {
            Assert.AreEqual(251, library.ProteinList.Count);
        }

        [TestMethod]
        public void ProteinDecoyCountIsCorrect()
        {
            Assert.AreEqual(251, library.ProteinDecoyList.Count);
        }

        [TestMethod]
        public void RtCountIsCorrect()
        {
            Assert.AreEqual(18, library.RtList.Count);
        }

        [TestMethod]
        public void PeptideCountIsCorrect()
        {
            Assert.AreEqual(9632, library.PeptideList.Count);
        }

        [TestMethod]
        public void TransitionCountIsCorrect()
        {
            Assert.AreEqual(57738, library.TransitionList.Count);
        }

        [TestMethod]
        public void ProteinIdIsFilled()
        {
            Assert.AreEqual("1/sp|O75882|ATRN_HUMAN", ((Protein)(library.ProteinList[0])).Id);
            Assert.AreEqual("1/sp|P01719|LV501_HUMAN", ((Protein)(library.ProteinList[library.ProteinList.Count -1])).Id);
        }

        [TestMethod]
        public void ProteinAccessionIsFilled()
        {
            Assert.AreEqual("1/sp|O75882|ATRN_HUMAN", ((Protein)(library.ProteinList[0])).Accession);
        }

        [TestMethod]
        public void PeptideProteinIdIsFilled()
        {
            Assert.AreEqual("1/sp|O75882|ATRN_HUMAN", ((Peptide)(library.PeptideList[0])).ProteinId);
            Assert.AreEqual("DECOY_1/sp|P25311|ZA2G_HUMAN", ((Peptide)(library.PeptideList[library.ProteinList.Count - 1])).ProteinId);
        }

        [TestMethod]
        public void PeptideIdIsFilled()
        {
            Assert.AreEqual("1_AAAAAAVSGSAAAEAK_2", ((Peptide)(library.PeptideList[0])).Id);
        }

        [TestMethod]
        public void PeptideSequenceIsFilled()
        {
            Assert.AreEqual("AAAAAAVSGSAAAEAK", ((Peptide)(library.PeptideList[0])).Sequence);
        }

        [TestMethod]
        public void PeptideChargeStateIsFilled()
        {
            Assert.AreEqual(2, ((Peptide)(library.PeptideList[0])).ChargeState);
        }

        [TestMethod]
        public void PeptideRetentionTimeIsFilled()
        {
            Assert.AreEqual(-12.8, ((Peptide)(library.PeptideList[0])).RetentionTime);
        }

        [TestMethod]
        public void TransitionIdIsFilled()
        {
            Assert.AreEqual("11_y9_1_AAAAAAVSGSAAAEAK_2", ((Transition)(library.TransitionList[0])).Id);
        }

        [TestMethod]
        public void TransitionPeptideRefIsFilled()
        {
            Assert.AreEqual("1_AAAAAAVSGSAAAEAK_2", ((Transition)(library.TransitionList[0])).PeptideId);
        }

        [TestMethod]
        public void TransitionPrecursorMzIsFilled()
        {
            Assert.AreEqual(658.843873083, ((Transition)(library.TransitionList[0])).PrecursorMz );
        }

        [TestMethod]
        public void TransitionProductMzIsFilled()
        {
            Assert.AreEqual(791.389373008, ((Transition)(library.TransitionList[0])).ProductMz);
        }

        [TestMethod]
        public void TransitionProductIonSeriesOrdinalIsFilled()
        {
            Assert.AreEqual(9, ((Transition)(library.TransitionList[0])).ProductIonSeriesOrdinal);
        }

        [TestMethod]
        public void TransitionProductInterpretationRankIsFilled()
        {
            Assert.AreEqual(1, ((Transition)(library.TransitionList[0])).ProductInterpretationRank);
        }

        [TestMethod]
        public void TransitionIonTypeIsFilled()
        {
            Assert.AreEqual("frag: y ion", ((Transition)(library.TransitionList[0])).IonType);
        }

        [TestMethod]
        public void TransitionProductIonIntensityIsFilled()
        {
            Assert.AreEqual(10000, ((Transition)(library.TransitionList[0])).ProductIonIntensity);
        }
    }
}
