using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MzmlParser;

namespace SwaMe.Test
{
    [TestClass]
    public class MetricGeneratorTests
    {
        private static Run emptyms2scansRun;
        private static Run contains5ms2scansRun;
        private static MetricGenerator mG;

        [TestInitialize]
        public void Initialize()
        {
            ///<summary>The Metric Generator is the main tree from which the other metric calculatingfunction groups are run. 
            ///Therefore most tests will not be run here, but in the function branches RTGrouper/SWATHGrouper etc. There are however a few
            ///functions that are run here and they are also tested here.</summary>

            Scan ms2scan1 = new Scan(false)
            {
                IsolationWindowLowerOffset = 1,
                IsolationWindowUpperOffset = 1,
                ScanStartTime = 0,
                MsLevel = 2,
                Density = 2
            };

            Scan ms2scan2 = new Scan(false)
            {
                IsolationWindowLowerOffset = 1,
                IsolationWindowUpperOffset = 1,
                ScanStartTime = 0,
                RTsegment = 1,
                MsLevel = 2,
                Density = 2
            };
            Scan ms2scan3 = new Scan(false)
            {
                IsolationWindowLowerOffset = 5,
                IsolationWindowUpperOffset = 5,
                ScanStartTime = 0,
                RTsegment = 1,
                MsLevel = 2,
                Density = 4
            };
            Scan ms2scan4 = new Scan(false)
            {
                IsolationWindowLowerOffset = 1,
                IsolationWindowUpperOffset = 1,
                ScanStartTime = 0,
                MsLevel = 2,
                Density = 4
            };
            Scan ms2scan5 = new Scan(false)
            {
                IsolationWindowLowerOffset = 1,
                IsolationWindowUpperOffset = 1,
                ScanStartTime = 0,
                MsLevel = 2,
                Density = 5
            };
            Scan ms2scan6 = new Scan(false)
            {
                ScanStartTime = 0,
                MsLevel = 2,
                Density = 5
            };

            var spectrumpoint1 = new SpectrumPoint()
            {
                Intensity = 2000,
                Mz = 150,
                RetentionTime = 2.58F
            };
            var spectrumpoint2 = new SpectrumPoint()
            {
                Intensity = 3000,
                Mz = 150.01F,
                RetentionTime = 3.00F
            };
            var basePeak1 = new BasePeak(150, 2.5, 150)
            {
                BpkRTs = new List<double>() { 2.5 },
                Spectrum = new List<SpectrumPoint>() { spectrumpoint1, spectrumpoint2 }
            };
            contains5ms2scansRun = new Run
            {
                AnalysisSettings = new AnalysisSettings
                {
                    RtTolerance = 2.5
                },
                Ms2Scans = new ConcurrentBag<Scan>() { ms2scan1, ms2scan2, ms2scan3, ms2scan4, ms2scan5 },
                LastScanTime = 70,
                StartTime = 2.5
            };

            
            contains5ms2scansRun.BasePeaks.Add(basePeak1);
            contains5ms2scansRun.SourceFileNames.Add( " ");
            contains5ms2scansRun.SourceFileChecksums.Add(" ");

            emptyms2scansRun = new Run
            {
                AnalysisSettings = new AnalysisSettings
                {
                    RtTolerance = 2.5
                },
                Ms2Scans = new ConcurrentBag<Scan>() { ms2scan1, ms2scan2, ms2scan3, ms2scan4, ms2scan6 },//6 does not have upper and lower offsets
                LastScanTime = 0,
                StartTime = 1000000
            };
            
            emptyms2scansRun.BasePeaks.Add(basePeak1);
            emptyms2scansRun.SourceFileNames.Add(" ");
            emptyms2scansRun.SourceFileChecksums.Add(" ");

            mG = new MetricGenerator();

        }
        /// <remarks>
        /// This test tests if the RTDuration is correctly calculated, provided that the scan that was fed contained the information necessary to calculate RTDuration, namely last and first scantimes.
        /// </remarks>
        [TestMethod]
        public void RTDurationCorrectIfcontainsLastAndFirstScanTimes() 
        {
            mG.GenerateMetrics(contains5ms2scansRun, 1, "", false, false, false, "");
            Assert.AreEqual(mG.RTDuration, 67.5);
        }
        /// <remarks>
        /// Here we test the scenario that, in the event that the last and first scan times were not able to be recorded, the RTDuration is set to zero.
        /// </remarks>
        [TestMethod]
        public void RTDurationZeroIfMissingLastScanTimeOrFirstScanTime()
        {
            mG.GenerateMetrics(emptyms2scansRun, 1, "", false, false, false, "");
            Assert.AreEqual(mG.RTDuration, 0);
        }
        /// <remarks>
        /// Here we test that the difference between the highest swath mz range (loweroffset + upperoffset) and the lowest swath mz range is calculated correctly.
        /// </remarks>
        [TestMethod]
        public void swathSizeDifferenceCorrectIfOffsetsNotDefault()
        {
            mG.GenerateMetrics(contains5ms2scansRun, 1, "", false, false, false, "");
            Assert.AreEqual(mG.swathSizeDifference, 8);
        }
        /// <remarks>
        /// Here we test the scenario that, in the event that there were no swathsizes recorded in the run, the swathsize difference is set to zero.
        /// </remarks>
        [TestMethod]
        public void swathSizeDifferenceZeroIfOffsetsAreDefault()
        {
            mG.GenerateMetrics(emptyms2scansRun, 1, "", false, false, false, "");
            Assert.AreEqual(mG.swathSizeDifference, 0);
        }
        /// <remarks>
        /// Here we test that the list of the densities (total number of ions detected per scan) for ms2scans is calculated correctly.
        /// </remarks>
        [TestMethod]
        public void DensityCorrect()
        {
            mG.GenerateMetrics(contains5ms2scansRun, 1, "", false, false, false, "");
            List<int>correctDensity = new List<int>(){2,2,4,4,5 };
            Assert.IsTrue(Enumerable.SequenceEqual(mG.Density, correctDensity));
        }

    }
}
