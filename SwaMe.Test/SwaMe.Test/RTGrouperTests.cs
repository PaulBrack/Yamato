using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MzmlParser;
using SwaMe.Pipeline;

namespace SwaMe.Test
{
    [TestClass]
    public class RTGrouperTests
    {
        private Run<Scan> Ms2andms1Run;
        private Run<Scan> Emptyms2scansRun;
        private RTGrouper RTGrouper;
        public RTGrouper.RTMetrics Result;

        [TestInitialize]
        public void Initialize()
        {
            string tempPath = Path.GetTempPath();
            Scan ms2scan1 = new Scan(false, 148, 1, 1, 0, TandemMsLevel.Ms2, 2, 1, 100.00, tempPath);
            Scan ms2scan2 = new Scan(false, 150, 1, 1, 20, TandemMsLevel.Ms2, 2, 1, 100000.02, tempPath);
            Scan ms2scan3 = new Scan(false, 145, 5, 5, 30, TandemMsLevel.Ms2, 4, 2, 30, tempPath);
            Scan ms2scan4 = new Scan(false, 150, 5, 5, 32, TandemMsLevel.Ms2, 4, 2, 20010.33, tempPath);
            Scan ms2scan5 = new Scan(false, 155, 5, 5, 34, TandemMsLevel.Ms2, 4, 2, 50.33, tempPath);
            Scan ms2scan6 = new Scan(false, 158, 1, 1, 35, TandemMsLevel.Ms2, 4, 2, 4000, tempPath);
            Scan ms2scan7 = new Scan(false, 148, 1, 1, 70, TandemMsLevel.Ms2, 5, 3, 60000, tempPath);
            Scan ms2scan8 = new Scan(false, 150, 1, 1, 70, TandemMsLevel.Ms2, 5, 3, 6000, tempPath);
            Scan ms2scan9 = new Scan(false, tempPath)
            {
                ScanStartTime = 0,
                MsLevel = TandemMsLevel.Ms2,
                Density = 5,
                TotalIonCurrent = 8000
            };
            //MS1scans
            Scan ms1scan1 = new Scan(false, tempPath)
            {
                IsolationWindowLowerOffset = 1,
                IsolationWindowUpperOffset = 1,
                ScanStartTime = 0,
                MsLevel = TandemMsLevel.Ms1,
                Density = 2,
                Cycle = 1,
                TotalIonCurrent = 1000,
                BasePeakIntensity = 1000,
                BasePeakMz = 1058
            };
            Scan ms1scan2 = new Scan(false, tempPath)
            {
                IsolationWindowLowerOffset = 5,
                IsolationWindowUpperOffset = 5,
                ScanStartTime = 30,
                MsLevel = TandemMsLevel.Ms1,
                Density = 4,
                Cycle = 2,
                TotalIonCurrent = 3050,
                BasePeakIntensity = 1500,
                BasePeakMz = 459
            };
            Scan ms1scan3 = new Scan(false, tempPath)
            {
                IsolationWindowLowerOffset = 5,
                IsolationWindowUpperOffset = 5,
                ScanStartTime = 70,
                MsLevel = TandemMsLevel.Ms1,
                Density = 4,
                Cycle = 3,
                TotalIonCurrent = 3050,
                BasePeakIntensity = 5000,
                BasePeakMz = 150
            };

            //BasePeaks:
            var spectrumpoint1 = new SpectrumPoint(2000, 150, 2.58F);
            var spectrumpoint2 = new SpectrumPoint(3000, 150.01F, 3.00F);
            var spectrumpoint3 = new SpectrumPoint(3000, 150.01F, 60F);
            var basePeak1 = new BasePeak(150, 2.5, 150, spectrumpoint1, spectrumpoint2);
            basePeak1.RTsegments.Add(2);
            basePeak1.FullWidthHalfMaxes.Add(1);
            basePeak1.FullWidthHalfMaxes.Add(2);
            basePeak1.PeakSymmetries.Add(1);
            basePeak1.PeakSymmetries.Add(2);
            basePeak1.Intensities.Add(2);
            basePeak1.FullWidthBaselines.Add(1);
            basePeak1.FullWidthBaselines.Add(2);

            var basePeak2 = new BasePeak(300, 60, 150, spectrumpoint3);
            basePeak2.FullWidthHalfMaxes.Add(2);
            basePeak2.PeakSymmetries.Add(1);
            basePeak2.FullWidthBaselines.Add(1);

            //Runs:
            Ms2andms1Run = new Run<Scan>(new AnalysisSettings { RtTolerance = 2.5 })
            {
                LastScanTime = 100,
                StartTime = 0
            };
            Ms2andms1Run.Ms2Scans.AddRange(new List<Scan>() { ms2scan1, ms2scan2, ms2scan3, ms2scan4, ms2scan5, ms2scan6, ms2scan7, ms2scan8 });
            Ms2andms1Run.Ms1Scans.AddRange(new List<Scan>() { ms1scan1, ms1scan2, ms1scan3 });


            Ms2andms1Run.BasePeaks.Add(basePeak1);
            Ms2andms1Run.BasePeaks.Add(basePeak2);
            Ms2andms1Run.SourceFileNames.Add(" ");
            Ms2andms1Run.SourceFileChecksums.Add(" ");

            Emptyms2scansRun = new Run<Scan>(new AnalysisSettings { RtTolerance = 2.5 })
            {
                LastScanTime = 0,
                StartTime = 1000000
            };
            Emptyms2scansRun.Ms2Scans.AddRange(new List<Scan>() { ms2scan1, ms2scan2, ms2scan3, ms2scan4, ms2scan9 }); //9 does not have upper and lower offsets

            Emptyms2scansRun.BasePeaks.Add(basePeak1);
            Emptyms2scansRun.SourceFileNames.Add(" ");
            Emptyms2scansRun.SourceFileChecksums.Add(" ");

            RTGrouper = new RTGrouper();
            Result = RTGrouper.DivideByRT(Ms2andms1Run, 2, 100);
        }

        /// <remarks>
        /// RTSegments is correctly calculated.
        /// </remarks>
        [TestMethod]
        public void RtSegsCorrect()
        {
            double[] correctsegments = { 0, 50 };
            Assert.IsTrue(Enumerable.SequenceEqual(RTGrouper.rtSegs, correctsegments));
        }

        /// <remarks>
        /// RTSegs is correctly allocated to basepeaks - first.
        /// </remarks>
        [TestMethod]
        public void RtsegmentsAllocatedToFirstBasePeakCorrect()
        {
            Assert.AreEqual(Ms2andms1Run.BasePeaks[0].RTsegments[0], 2);
        }

        /// <remarks>
        /// RTSegs is correctly allocated to basepeaks - second.
        /// </remarks>
        [TestMethod]
        public void RtsegmentsAllocatedToSecondBasePeakCorrect()
        {
            Assert.AreEqual(Ms2andms1Run.BasePeaks[1].RTsegments[0], 1);
        }

        /// <remarks>
        /// RTSegs is correctly allocated to scans - first.
        /// </remarks>
        [TestMethod]
        public void RtsegmentsAllocatedToFirstScanCorrect()
        {
            Assert.AreEqual(Ms2andms1Run.Ms2Scans.OrderBy(x => x.ScanStartTime).ElementAt(0).RTsegment, 0);
        }

        /// <remarks>
        /// RTSegs is correctly allocated to scans - fifth (This scan is part of the second RTsegment).
        /// </remarks>
        [TestMethod]
        public void RtsegmentsAllocatedToFifthScanCorrect()
        {
            Assert.AreEqual(Ms2andms1Run.Ms2Scans.OrderBy(x => x.ScanStartTime).ElementAt(7).RTsegment, 1);
        }

        /// <remarks>
        /// TIC Change from one scan to the next chronological scan is correctly calculated.
        /// </remarks>
        [TestMethod]
        public void TICChangeCorrect()
        {
            List<double> correctTICChangeList = new List<double>() { 0.4112602937765108, 0.45553110066628605 };
            Assert.IsTrue(Enumerable.SequenceEqual(Result.TicChange50List, correctTICChangeList));
        }

        /// <remarks>
        /// TICChangeIQR from one scan to the next is correctly calculated.
        /// </remarks>
        [TestMethod]
        public void TICChangeIQRCorrect()
        {
            List<double> correctTICChangeIQRList = new List<double>() { 0.6743549129237949, 0 };
            Assert.IsTrue(Enumerable.SequenceEqual(Result.TicChangeIqrList, correctTICChangeIQRList));
        }

        /// <remarks>
        /// MS2TICTotal for each segment is correctly calculated.
        /// </remarks>
        [TestMethod]
        public void MS2TICTotalCorrect()
        {
            List<double> correctMS2TICTotalList = new List<double>() { 124190.68000000001, 66000 };
            Assert.IsTrue(Enumerable.SequenceEqual(Result.MS2TicTotal, correctMS2TICTotalList));
        }

        /// <remarks>
        /// CycleTime for each segment is correctly calculated.
        /// </remarks>
        [TestMethod]
        public void CycleTimeCorrect()
        {
            List<double> correctCycleTime = new List<double>() { 750, 0 };
            Assert.IsTrue(Enumerable.SequenceEqual(Result.CycleTime, correctCycleTime));
        }

        /// <remarks>
        /// MS2Density for each segment is correctly calculated.
        /// </remarks>
        [TestMethod]
        public void MS2DensityCorrect()
        {
            List<int> correctMS2Density = new List<int>() { 3, 5 };
            Assert.IsTrue(Enumerable.SequenceEqual(Result.MS2Density, correctMS2Density));
        }

        /// <remarks>
        ///Average Peakwidths for each segment is correctly calculated.
        /// </remarks>
        [TestMethod]
        public void PeakWidthsCorrect()
        {
            List<double> correctPeakWidths = new List<double>() { 2, 2 };
            Assert.IsTrue(Enumerable.SequenceEqual(Result.PeakWidths, correctPeakWidths));
        }

        /// <remarks>
        /// Average tailing factor for each segment is correctly calculated.
        /// </remarks>
        [TestMethod]
        public void TailingFactorCorrect()
        {
            List<double> correctTailingFactor = new List<double>() { 2, 1 };
            Assert.IsTrue(Enumerable.SequenceEqual(Result.TailingFactor, correctTailingFactor));
        }

        /// <remarks>
        /// PeakCapacity for each segment is correctly calculated.
        /// </remarks>
        [TestMethod]
        public void PeakCapacityCorrect()
        {
            List<double> correctPeakCapacity = new List<double>() { 25, 50 };
            Assert.IsTrue(Enumerable.SequenceEqual(Result.PeakCapacity, correctPeakCapacity));
        }

        /// <remarks>
        /// Peakprecision for each segment is correctly calculated.
        /// </remarks>
        [TestMethod]
        public void PeakPrecisionCorrect()
        {
            List<double> correctPeakPrecision = new List<double>() { 0.0046828263654738241, 0.590665785597378 };
            Assert.IsTrue(Enumerable.SequenceEqual(Result.PeakPrecision, correctPeakPrecision));
        }

        /// <remarks>
        /// MS1TIC total for each segment is correctly calculated.
        /// </remarks>
        [TestMethod]
        public void MS1TICTotalCorrect()
        {
            List<double> correctMS1TICTotalList = new List<double>() { 4050, 3050 };
            Assert.IsTrue(Enumerable.SequenceEqual(Result.MS1TicTotal, correctMS1TICTotalList));
        }

        /// <remarks>
        /// Density(number of ions) in MS1 scans total for each segment is correctly calculated.
        /// </remarks>
        [TestMethod]
        public void MS1Density()
        {
            List<int> correctMS1DensityList = new List<int>() { 3, 4 };
            Assert.IsTrue(Enumerable.SequenceEqual(Result.MS1Density, correctMS1DensityList));
        }

        /// <remarks>
        /// peak precision between MS1 scans total for each segment is correctly calculated.
        /// </remarks>
        [TestMethod]
        public void MS1PeakPrecision()
        {
            List<double> correctMS1PeakPrecisionList = new List<double>() { 6.3934899179680507, 11.707065913684561 };
            Assert.IsTrue(Enumerable.SequenceEqual(Result.MS1PeakPrecision, correctMS1PeakPrecisionList));
        }
    }
}
