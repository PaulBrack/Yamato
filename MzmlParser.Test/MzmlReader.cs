using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace MzmlParser.Test
{



    [TestClass]
    public class MzmlReaderTest
    {
        private static Run run;

        private static readonly AnalysisSettings analysisSettings = new AnalysisSettings()
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
            run = new MzmlReader().LoadMzml(Path.Combine("mzmls", "test.mzml"), analysisSettings);
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
        public void StartTimeStampReadCorrectly()
        {
            Assert.AreEqual("2017-08-10T11:23:54Z", run.StartTimeStamp);
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
            Assert.IsTrue(Enumerable.Contains(run.SourceFileTypes, ".scan"));
        }

        [TestMethod]
        public void SourceFileNameReadCorrectly()
        {
            Assert.IsTrue(Enumerable.Contains(run.SourceFileNames, "SWATH_OC_244_1"));
        }

        [TestMethod]
        public void IDReadCorrectly()
        {
            Assert.AreEqual(run.ID, "SWATH_OC_244_1");
        }

        [TestMethod]
        public void SourceFileChecksumReadCorrectly()
        {
            Assert.IsTrue(Enumerable.Contains(run.SourceFileChecksums, "b0e7fb43b8c828c51d715b21934d0e925844199e")&& Enumerable.Contains(run.SourceFileChecksums, "4cd26cdeb97116d5d4f62a575642383e8d6709d4"));

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
            Assert.AreEqual(403, run.Ms2Scans.OrderBy(x => x.ScanStartTime).First().IsolationWindowTargetMz);
            Assert.AreEqual(747, run.Ms2Scans.OrderBy(x => x.ScanStartTime).Last().IsolationWindowTargetMz);
        }

        [TestMethod]
        public void IsolationWindowUpperOffestReadCorrectly()
        {
            Assert.AreEqual(3.5, run.Ms2Scans.OrderBy(x => x.ScanStartTime).First().IsolationWindowUpperOffset);
            Assert.AreEqual(3.5, run.Ms2Scans.OrderBy(x => x.ScanStartTime).Last().IsolationWindowUpperOffset);
        }

        [TestMethod]
        public void IsolationWindowLowerOffestReadCorrectly()
        {
            Assert.AreEqual(3.5, run.Ms2Scans.OrderBy(x => x.ScanStartTime).First().IsolationWindowLowerOffset);
            Assert.AreEqual(3.5, run.Ms2Scans.OrderBy(x => x.ScanStartTime).Last().IsolationWindowLowerOffset);
        }

        [TestMethod]
        public void IsolationWindowUpperBoundaryReadCorrectly()
        {
            Assert.AreEqual(406.5, run.Ms2Scans.OrderBy(x => x.ScanStartTime).First().IsolationWindowUpperBoundary);
            Assert.AreEqual(750.5, run.Ms2Scans.OrderBy(x => x.ScanStartTime).Last().IsolationWindowUpperBoundary);
           
        }

        [TestMethod]
        public void IsolationWindowLowerBoundaryReadCorrectly()
        {
            Assert.AreEqual(399.5, run.Ms2Scans.OrderBy(x => x.ScanStartTime).First().IsolationWindowLowerBoundary);
            Assert.AreEqual(743.5, run.Ms2Scans.OrderBy(x => x.ScanStartTime).Last().IsolationWindowLowerBoundary);
        }


    }
}
