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
            Assert.AreEqual(1, run.Ms1Scans.First().Cycle);
            Assert.AreEqual(1, run.Ms2Scans.First().Cycle);
            Assert.AreEqual(1, run.Ms2Scans.ToList()[50].Cycle);
            //TODO: Test a larger file
        }

        [TestMethod]
        public void MsLevelReadCorrectly()
        {
            Assert.AreEqual(false, run.Ms1Scans.Any(x => x.MsLevel != 1));
            Assert.AreEqual(false, run.Ms2Scans.Any(x => x.MsLevel != 2));
        }

        [TestMethod]
        public void IsolationWindowTargetMzReadCorrectly()
        {
            Assert.AreEqual(403, run.Ms2Scans.First().IsolationWindowTargetMz);
            Assert.AreEqual(747, run.Ms2Scans.Last().IsolationWindowTargetMz);
        }

        [TestMethod]
        public void IsolationWindowUpperOffestReadCorrectly()
        {
            Assert.AreEqual(3.5, run.Ms2Scans.First().IsolationWindowUpperOffset);
            Assert.AreEqual(3.5, run.Ms2Scans.Last().IsolationWindowUpperOffset);
        }

        [TestMethod]
        public void IsolationWindowLowerOffestReadCorrectly()
        {
            Assert.AreEqual(3.5, run.Ms2Scans.First().IsolationWindowLowerOffset);
            Assert.AreEqual(3.5, run.Ms2Scans.Last().IsolationWindowLowerOffset);
        }

        [TestMethod]
        public void IsolationWindowUpperBoundaryReadCorrectly()
        {
            Assert.AreEqual(406.5, run.Ms2Scans.First().IsolationWindowUpperBoundary);
            Assert.AreEqual(750.5, run.Ms2Scans.Last().IsolationWindowUpperBoundary);
           
        }

        [TestMethod]
        public void IsolationWindowLowerBoundaryReadCorrectly()
        {
            Assert.AreEqual(399.5, run.Ms2Scans.First().IsolationWindowLowerBoundary);
            Assert.AreEqual(743.5, run.Ms2Scans.Last().IsolationWindowLowerBoundary);
        }


    }
}
