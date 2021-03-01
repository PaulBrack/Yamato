using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using SwaMe.Pipeline;

namespace SwaMe.Test
{
    [TestClass]
    public class MetricGeneratorTests
    {
        private static Run<Scan> Emptyms2scansRun;
        private static Run<Scan> Contains5ms2ScansRun;
        private static MetricGenerator MetricGenerator;

        [TestInitialize]
        public void Initialize()
        {
            ///<summary>The Metric Generator is the main tree from which the other metric calculatingfunction groups are run. 
            ///Therefore most tests will not be run here, but in the function branches RTGrouper/SWATHGrouper etc. There are however a few
            ///functions that are run here and they are also tested here.</summary>

            string tempPath = Path.GetTempPath();

            Scan ms2scan1 = new Scan(false, 1, 1, 0, TandemMsLevel.Ms2, 2, 1, 1000, tempPath);
            Scan ms2scan2 = new Scan(false, 1, 1, 0, TandemMsLevel.Ms2, 2, 1, 5000, tempPath);
            Scan ms2scan3 = new Scan(false, 5, 5, 0, TandemMsLevel.Ms2, 4, 1, 1000, tempPath);
            Scan ms2scan4 = new Scan(false, 1, 1, 0, TandemMsLevel.Ms2, 4, 2, 51000, tempPath);
            Scan ms2scan5 = new Scan(false, 1, 1, 0, TandemMsLevel.Ms2, 5, 2, 1000, tempPath);
            Scan ms2scan6 = new Scan(false, tempPath)
            {
                ScanStartTime = 0,
                MsLevel = TandemMsLevel.Ms2,
                Density = 5
            };

            var spectrumpoint1 = new SpectrumPoint(2000, 150, 2.58F);
            var spectrumpoint2 = new SpectrumPoint(3000, 150.01F, 3.00F);

            var basePeak1 = new BasePeak(150, 2.5, 150)
            {
                BpkRTs = new List<double>() { 2.5 },
                Spectrum = new List<SpectrumPoint>() { spectrumpoint1, spectrumpoint2 }
            };
            Contains5ms2ScansRun = new Run<Scan>
            {
                AnalysisSettings = new AnalysisSettings
                {
                    RtTolerance = 2.5
                },
                LastScanTime = 70,
                StartTime = 2.5
            };
            Contains5ms2ScansRun.Ms2Scans.AddRange(new List<Scan>() { ms2scan1, ms2scan2, ms2scan3, ms2scan4, ms2scan5 });


            Contains5ms2ScansRun.BasePeaks.Add(basePeak1);
            Contains5ms2ScansRun.SourceFileNames.Add(" ");
            Contains5ms2ScansRun.SourceFileChecksums.Add(" ");

            Emptyms2scansRun = new Run<Scan>
            {
                AnalysisSettings = new AnalysisSettings
                {
                    RtTolerance = 2.5
                },
                LastScanTime = 0,
                StartTime = 1000000
            };
            Emptyms2scansRun.Ms2Scans.AddRange(new List<Scan>() { ms2scan1, ms2scan2, ms2scan3, ms2scan4, ms2scan6 }); //6 does not have upper and lower offsets

            Emptyms2scansRun.BasePeaks.Add(basePeak1);
            Emptyms2scansRun.SourceFileNames.Add(" ");
            Emptyms2scansRun.SourceFileChecksums.Add(" ");

            MetricGenerator = new MetricGenerator();

        }
        /// <remarks>
        /// RTDuration is correctly calculated, provided that the scan that was fed contained last and first scantimes.
        /// </remarks>
        [TestMethod]
        public void RTDurationCorrectIfcontainsLastAndFirstScanTimes()
        {
            MetricGenerator.GenerateMetrics(Contains5ms2ScansRun, 1, false);
            Assert.AreEqual(MetricGenerator.RTDuration, 67.5);
        }
        /// <remarks>
        /// In the event that the last and first scan times were not able to be recorded, the RTDuration should be set to zero.
        /// </remarks>
        [TestMethod]
        public void RTDurationZeroIfMissingLastScanTimeOrFirstScanTime()
        {
            MetricGenerator.GenerateMetrics(Emptyms2scansRun, 1, false);
            Assert.AreEqual(MetricGenerator.RTDuration, 0);
        }
        /// <remarks>
        /// The difference between the highest swath mz range (loweroffset + upperoffset) and the lowest swath mz range is calculated correctly.
        /// </remarks>
        [TestMethod]
        public void swathSizeDifferenceCorrectIfOffsetsNotDefault()
        {
            MetricGenerator.GenerateMetrics(Contains5ms2ScansRun, 1, false);
            Assert.AreEqual(MetricGenerator.swathSizeDifference, 8);
        }
        /// <remarks>
        /// In the event that there were no swathsizes recorded in the run, the swathsize difference is set to zero.
        /// </remarks>
        [TestMethod]
        public void swathSizeDifferenceZeroIfOffsetsAreDefault()
        {
            MetricGenerator.GenerateMetrics(Emptyms2scansRun, 1, false);
            Assert.AreEqual(MetricGenerator.swathSizeDifference, 0);
        }
        /// <remarks>
        /// The list of the densities (total number of ions detected per scan) for ms2scans is calculated correctly.
        /// </remarks>
        [TestMethod]
        public void DensityCorrect()
        {
            MetricGenerator.GenerateMetrics(Contains5ms2ScansRun, 1, false);
            List<int> correctDensity = new List<int>() { 2, 2, 4, 4, 5 };
            Assert.IsTrue(Enumerable.SequenceEqual(MetricGenerator.Density, correctDensity));
        }

    }
}
