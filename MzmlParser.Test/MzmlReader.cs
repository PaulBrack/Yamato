using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace MzmlParser.Test
{



    [TestClass]
    public class MzmlReaderTest
    {
        private static Run run;

        private static AnalysisSettings analysisSettings = new AnalysisSettings()
        {
            MassTolerance = 0.05,
            RtTolerance = 5,
            IrtMinIntensity = 250,
            IrtMinPeptides = 3,
            IrtMassTolerance = 0.05
        };

        [ClassInitialize]
        public static void Initialize(TestContext t)
        {
            run = new MzmlReader().LoadMzml(Path.Combine("mzmls", "test.mzml"), true, analysisSettings);
        }

        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual(1, 1);
        }
    }
}
