using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

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
        public void CorrectNumberOfMs1ScansAreLoaded()
        {
            Assert.AreEqual(1, run.Ms1Scans.Count);
        }

        [TestMethod]
        public void CorrectNumberOfMs2ScansAreLoaded()
        {
            Assert.AreEqual(65, run.Ms2Scans.Count);
        }

        [TestMethod]
        public void StartTimeReadCorrectly()
        {
            Assert.AreEqual(0.0046, run.StartTime);
        }

        [TestMethod]
        public void EndTimeReadCorrectly()
        {
            Assert.AreEqual(0.033183333333, run.LastScanTime);
        }

        [TestMethod]
        public void SourceFileTypeReadCorrectly()
        {
            Assert.AreEqual(".wiff", run.SourceFileType);
        }

        [TestMethod]
        public void SourceFileNameReadCorrectly()
        {
            Assert.AreEqual("SWATH_OC_244_1.wiff", run.SourceFileName);
        }

        [TestMethod]
        public void SourceFileChecksumReadCorrectly()
        {
            Assert.AreEqual("4cd26cdeb97116d5d4f62a575642383e8d6709d4", run.SourceFileChecksum);
        }

        [TestMethod]
        public void SourceFileLocationReadCorrectly()
        {
            Assert.AreEqual(@"file://C:\wiffs", run.SourceFilePath);
        }

        [TestMethod]
        public void CycleNumberReadCorrectly()
        {
            Assert.AreEqual(run.Ms1Scans[0].Cycle, 1);
            Assert.AreEqual(run.Ms2Scans.First().Cycle, 1);
            Assert.AreEqual(run.Ms2Scans.ToList()[99].Cycle, 1);
            Assert.AreEqual(run.Ms2Scans.ToList()[100].Cycle, 2);
        }


    }
}
