﻿#nullable enable

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using SwaMe.Pipeline;
using MzmlParser;
using System;

namespace SwaMe.Test
{
    /// <summary>
    /// The Metric Generator is the main tree from which the other metric calculatingfunction groups are run. 
    /// Therefore most tests will not be run here, but in the function branches RTGrouper/SWATHGrouper etc. There are however a few
    /// functions that are run here and they are also tested here.
    /// </summary>
    [TestClass]
    public class MetricGeneratorTests
    {
        private Run<Scan> emptyms2scansRun;
        private Run<Scan> contains5ms2ScansRun;

        [TestInitialize]
        public void Initialize()
        {
            bool cacheSpectraToDisk = false;
            string tempPath = Path.GetTempPath();

            Scan ms2scan1 = new Scan(cacheSpectraToDisk, 148, 1, 1, 0, TandemMsLevel.Ms2, 2, 1, 1000, tempPath);
            Scan ms2scan2 = new Scan(cacheSpectraToDisk, 150, 1, 1, 0, TandemMsLevel.Ms2, 2, 1, 5000, tempPath);
            Scan ms2scan3 = new Scan(cacheSpectraToDisk, 153.5, 5, 5, 0, TandemMsLevel.Ms2, 4, 1, 1000, tempPath);
            Scan ms2scan4 = new Scan(cacheSpectraToDisk, 148, 1, 1, 0, TandemMsLevel.Ms2, 4, 2, 51000, tempPath);
            Scan ms2scan5 = new Scan(cacheSpectraToDisk, 150, 1, 1, 0, TandemMsLevel.Ms2, 5, 2, 1000, tempPath);
            Scan ms2scan6 = new Scan(cacheSpectraToDisk, 153.5, default, default, 0, TandemMsLevel.Ms2, 5, tempPath); // No upper and lower offsets

            var spectrumpoint1 = new SpectrumPoint(2000, 150, 2.58F);
            var spectrumpoint2 = new SpectrumPoint(3000, 150.01F, 3.00F);

            var basePeak1 = new BasePeak(150, 2.5, 150, spectrumpoint1, spectrumpoint2);
            contains5ms2ScansRun = new Run<Scan>(new AnalysisSettings { RtTolerance = 2.5 })
            {
                LastScanTime = 70,
                StartTime = 2.5
            };
            contains5ms2ScansRun.Ms2Scans.AddRange(new [] { ms2scan1, ms2scan2, ms2scan3, ms2scan4, ms2scan5 });
            FixupIsolationWindows(contains5ms2ScansRun);

            contains5ms2ScansRun.BasePeaks.Add(basePeak1);
            contains5ms2ScansRun.SourceFileNames.Add(" ");
            contains5ms2ScansRun.SourceFileChecksums.Add(" ");

            emptyms2scansRun = new Run<Scan>(new AnalysisSettings { RtTolerance = 2.5 })
            {
                LastScanTime = 0,
                StartTime = 1000000
            };
            emptyms2scansRun.Ms2Scans.AddRange(new List<Scan>() { ms2scan1, ms2scan2, ms2scan3, ms2scan4, ms2scan6 }); //6 does not have upper and lower offsets
            FixupIsolationWindows(emptyms2scansRun);

            emptyms2scansRun.BasePeaks.Add(basePeak1);
            emptyms2scansRun.SourceFileNames.Add(" ");
            emptyms2scansRun.SourceFileChecksums.Add(" ");
        }

        private void FixupIsolationWindows(Run<Scan> run)
        {
            foreach (Scan scan in run.Ms2Scans)
                if (scan.IsolationWindowTargetMz.HasValue && scan.IsolationWindowLowerOffset.HasValue && scan.IsolationWindowUpperOffset.HasValue)
                    run.IsolationWindows.Add(new IsolationWindow(scan.IsolationWindowLowerBoundary.Value, scan.IsolationWindowTargetMz.Value, scan.IsolationWindowUpperBoundary.Value));
        }

        /// <remarks>
        /// RTDuration is correctly calculated, provided that the scan that was fed contained last and first scantimes.
        /// </remarks>
        [TestMethod]
        public void RTDurationCorrectIfcontainsLastAndFirstScanTimes()
        {
            MetricGenerator metricGenerator = new MetricGenerator();
            var metrics = metricGenerator.GenerateMetrics(contains5ms2ScansRun, 1, false);
            Assert.AreEqual(metrics.RtDuration, 67.5);
        }

        /// <remarks>
        /// In the event that the last or first scan times were not able to be recorded, the RTDuration should be set to zero.
        /// </remarks>
        [TestMethod]
        public void RTDurationZeroIfMissingLastScanTimeOrFirstScanTime()
        {
            MetricGenerator metricGenerator = new MetricGenerator();
            var metrics = metricGenerator.GenerateMetrics(emptyms2scansRun, 1, false);
            Assert.AreEqual(metrics.RtDuration, 0);
        }

        /// <remarks>
        /// The difference between the highest swath mz range (loweroffset + upperoffset) and the lowest swath mz range is calculated correctly.
        /// </remarks>
        [TestMethod]
        public void SwathSizeDifferenceCorrectIfOffsetsNotDefault()
        {
            MetricGenerator metricGenerator = new MetricGenerator();
            var metrics = metricGenerator.GenerateMetrics(contains5ms2ScansRun, 1, false);
            Assert.AreEqual(metrics.SwathSizeDifference, 8);
        }

        /// <remarks>
        /// In the event that there were no swathsizes recorded in the run, the swathsize difference is set to zero.
        /// </remarks>
        [TestMethod]
        public void SwathSizeDifferenceZeroIfOffsetsAreDefault()
        {
            MetricGenerator metricGenerator = new MetricGenerator();
            var metrics = metricGenerator.GenerateMetrics(emptyms2scansRun, 1, false);
            Assert.AreEqual(metrics.SwathSizeDifference, 0);
        }

        /// <remarks>
        /// The list of the densities (total number of ions detected per scan) for ms2scans is calculated correctly.
        /// </remarks>
        [TestMethod]
        public void DensityCorrect()
        {
            MetricGenerator metricGenerator = new MetricGenerator();
            var metrics = metricGenerator.GenerateMetrics(contains5ms2ScansRun, 1, false);
            List<int>? correctDensity = new List<int>() { 2, 2, 4, 4, 5 };
            Assert.IsTrue(Enumerable.SequenceEqual(metrics.Density, correctDensity));
        }
    }
}
