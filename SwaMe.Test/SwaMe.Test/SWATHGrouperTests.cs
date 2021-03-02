using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MzmlParser;
using SwaMe.Pipeline;

namespace SwaMe.Test
{
    [TestClass]
    public class SWATHGrouperTests
    {
        private static Run<Scan> ms2andms1Run;
        private static SwathGrouper swathGrouper;
        public SwathMetrics result;

        [TestInitialize]
        public void Initialize()
        {
            string tempPath = Path.GetTempPath();

            Scan ms2scan1 = new Scan(false, tempPath)
            {
                IsolationWindowLowerOffset = 1,
                IsolationWindowTargetMz = 550,
                IsolationWindowUpperOffset = 1,
                ScanStartTime = 0,
                MsLevel = TandemMsLevel.Ms2,
                Density = 2,
                Cycle = 1,
                TotalIonCurrent = 1000
            };


            Scan ms2scan2 = new Scan(false, tempPath)
            {
                IsolationWindowLowerOffset = 5,
                IsolationWindowTargetMz = 550,
                IsolationWindowUpperOffset = 5,
                ScanStartTime = 30,
                MsLevel = TandemMsLevel.Ms2,
                Density = 4,
                Cycle = 2,
                TotalIonCurrent = 3050
            };


            Scan ms2scan3 = new Scan(false, tempPath)
            {
                IsolationWindowLowerOffset = 1,
                IsolationWindowTargetMz = 550,
                IsolationWindowUpperOffset = 1,
                ScanStartTime = 35,
                MsLevel = TandemMsLevel.Ms2,
                Density = 4,
                Cycle = 3,
                TotalIonCurrent = 4000
            };
            Scan ms2scan4 = new Scan(false, tempPath)
            {
                IsolationWindowLowerOffset = 1,
                IsolationWindowTargetMz = 550,
                IsolationWindowUpperOffset = 1,
                ScanStartTime = 70,
                MsLevel = TandemMsLevel.Ms2,
                Density = 5,
                Cycle = 4,
                TotalIonCurrent = 6000
            };
            Scan ms2scan5 = new Scan(false, tempPath)
            {
                IsolationWindowLowerOffset = 1,
                IsolationWindowTargetMz = 550,
                IsolationWindowUpperOffset = 1,
                ScanStartTime = 70.002,
                MsLevel = TandemMsLevel.Ms2,
                Density = 5,
                Cycle = 5,
                TotalIonCurrent = 6000
            };
            Scan ms2scan6 = new Scan(false, tempPath)
            {
                IsolationWindowLowerOffset = 1,
                IsolationWindowTargetMz = 1050,
                IsolationWindowUpperOffset = 1,
                ScanStartTime = 0,
                MsLevel = TandemMsLevel.Ms2,
                Density = 2,
                Cycle = 1,
                TotalIonCurrent = 1000
            };


            Scan ms2scan7 = new Scan(false, tempPath)
            {
                IsolationWindowLowerOffset = 5,
                IsolationWindowTargetMz = 1050,
                IsolationWindowUpperOffset = 5,
                ScanStartTime = 30,
                MsLevel = TandemMsLevel.Ms2,
                Density = 4,
                Cycle = 2,
                TotalIonCurrent = 3050
            };


            Scan ms2scan8 = new Scan(false, tempPath)
            {
                IsolationWindowLowerOffset = 1,
                IsolationWindowTargetMz = 1050,
                IsolationWindowUpperOffset = 1,
                ScanStartTime = 35,
                MsLevel = TandemMsLevel.Ms2,
                Density = 40,
                Cycle = 3,
                TotalIonCurrent = 4000
            };
            Scan ms2scan9 = new Scan(false, tempPath)
            {
                IsolationWindowLowerOffset = 1,
                IsolationWindowTargetMz = 1050,
                IsolationWindowUpperOffset = 1,
                ScanStartTime = 70,
                MsLevel = TandemMsLevel.Ms2,
                Density = 20,
                Cycle = 4,
                TotalIonCurrent = 20000
            };
            Scan ms2scan10 = new Scan(false, tempPath)
            {
                IsolationWindowLowerOffset = 1,
                IsolationWindowTargetMz = 1050,
                IsolationWindowUpperOffset = 1,
                ScanStartTime = 70.002,
                MsLevel = TandemMsLevel.Ms2,
                Density = 18,
                Cycle = 5,
                TotalIonCurrent = 10000
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
            basePeak1.FullWidthHalfMaxes.Add(1);
            basePeak1.FullWidthHalfMaxes.Add(2);
            basePeak1.PeakSymmetries.Add(1);
            basePeak1.PeakSymmetries.Add(2);
            basePeak1.Intensities.Add(1);
            basePeak1.Intensities.Add(2);
            basePeak1.FullWidthBaselines.Add(1);
            basePeak1.FullWidthBaselines.Add(2);

            var basePeak2 = new BasePeak(300, 60, 150, spectrumpoint3);

            basePeak2.FullWidthHalfMaxes.Add(2);
            basePeak2.PeakSymmetries.Add(1);
            basePeak2.Intensities.Add(2);
            basePeak2.FullWidthBaselines.Add(1);

            //Runs:
            ms2andms1Run = new Run<Scan>(new AnalysisSettings { RtTolerance = 2.5 })
            {
                LastScanTime = 100,
                StartTime = 0
            };
            ms2andms1Run.Ms2Scans.AddRange(new Scan[] { ms2scan1, ms2scan2, ms2scan3, ms2scan4, ms2scan5, ms2scan6, ms2scan7, ms2scan8, ms2scan9, ms2scan10 });
            ms2andms1Run.Ms1Scans.AddRange(new Scan[] { ms1scan1, ms1scan2, ms1scan3 });

            ms2andms1Run.BasePeaks.Add(basePeak1);
            ms2andms1Run.BasePeaks.Add(basePeak2);
            ms2andms1Run.SourceFileNames.Add(" ");
            ms2andms1Run.SourceFileChecksums.Add(" ");
            /*
            emptyms2scansRun = new Run
            {
                AnalysisSettings = new AnalysisSettings
                {
                    RtTolerance = 2.5
                },
                Ms2Scans = new ConcurrentBag<Scan>() { ms2scan1, ms2scan2, ms2scan3, ms2scan4, ms2scan6 },//9 does not have upper and lower offsets
                LastScanTime = 0,
                StartTime = 1000000
            };

            emptyms2scansRun.BasePeaks.Add(basePeak1);
            emptyms2scansRun.SourceFileNames.Add(" ");
            emptyms2scansRun.SourceFileChecksums.Add(" ");*/

            swathGrouper = new SwathGrouper();
            result = swathGrouper.GroupBySwath(ms2andms1Run);
        }

        [TestMethod]
        public void TotalTICCorrect()
        {
            Assert.AreEqual(result.TotalTIC, 58100);
        }

        [TestMethod]
        public void SwathTargetsCorrect()
        {
            // Targets should come out of SwathGrouper sorted in ascending order.
            List<double> correctTargets = new List<double>() { 550, 1050 };
            Assert.IsTrue(Enumerable.SequenceEqual(result.SwathTargets, correctTargets));
        }

        [TestMethod]
        public void NumOfSwathPerGroupCorrect()
        {
            List<int> correctnumOfSwathPerGroup = new List<int>() { 5, 5 };
            Assert.IsTrue(Enumerable.SequenceEqual(result.NumOfSwathPerGroup, correctnumOfSwathPerGroup));
        }

        [TestMethod]
        public void MzTargetRangePerGroupCorrect()
        {
            List<double> correctmzTargetRange = new List<double>() {3.6, 3.6};
            Assert.IsTrue(Enumerable.SequenceEqual(result.MzRange, correctmzTargetRange));
        }

        [TestMethod]
        public void TICsCorrect()
        {
            List<double> correctTICs = new List<double>() { 20050, 38050 };
            Assert.IsTrue(Enumerable.SequenceEqual(result.TICs, correctTICs));
        }

        [TestMethod]
        public void SwDensity50Correct()
        {
            List<double> correctswDensity50 = new List<double>() { 4, 17 };
            Assert.IsTrue(Enumerable.SequenceEqual(result.SwDensity50, correctswDensity50));
        }

        [TestMethod]
        public void SwDensityIQRCorrect()
        {
            List<double?> correctswDensityIQR = new List<double?>() { 1, 16 };
            Assert.IsTrue(Enumerable.SequenceEqual(result.SwDensityIQR, correctswDensityIQR));
        }

        [TestMethod]
        public void SwathProportionOfTotalTICCorrect()
        {
            List<double> correctSwathProportionOfTotalTIC = new List<double>() { 0.34509466437177283, 0.65490533562822717 };
            Assert.IsTrue(Enumerable.SequenceEqual(result.SwathProportionOfTotalTIC, correctSwathProportionOfTotalTIC));
        }
    }
}
