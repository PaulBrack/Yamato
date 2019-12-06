using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using MzmlParser;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Linq;

namespace SwaMe.Test
{
    [TestClass]
    public class MetricGeneratorTest
    {
        private static Run run;

        [TestInitialize]
        public void Initialize()
        {

            run = new Run();
            run.AnalysisSettings = new AnalysisSettings
            {
                RtTolerance = 2.5
            };


            var basePeak1 = new BasePeak(150, 2.5, 150);
            basePeak1.BpkRTs = new List<double>() { 2.58 };
            var spectrumpoint1 = new SpectrumPoint();
            spectrumpoint1.Intensity = 2000;
            spectrumpoint1.Mz = 150;
            spectrumpoint1.RetentionTime = 2.58F;
            var spectrumpoint2 = new SpectrumPoint();
            spectrumpoint2.Intensity = 3000;
            spectrumpoint2.Mz = 150.01F;
            spectrumpoint2.RetentionTime = 3.00F;

            basePeak1.Spectrum = new List<SpectrumPoint>() { spectrumpoint1, spectrumpoint2 };

            run.BasePeaks = new ConcurrentBag<BasePeak>() { basePeak1};
        }
               

        [TestMethod]
        public void CheckIntensitiesAndStartTimesIfOneBasePeak()
        {
            var sC = new ChromatogramMetricGenerator();
            sC.GenerateChromatogram(run);
            var correctIntFile = File.ReadAllText("..\\..\\..\\..\\correctIntensities.csv");
            double[] correctInt = correctIntFile.Split(',').Select(s => Convert.ToDouble(s)).ToArray();
            Assert.IsTrue(Enumerable.SequenceEqual(correctInt,sC.intensities));
        }

        [TestMethod]
        public void CheckIntensitiesAndStartTimesIfBasePeakSpectrumEmpty()
        {
            run.BasePeaks.ElementAt(0).Spectrum.Clear();
            var sC = new ChromatogramMetricGenerator();
            sC.GenerateChromatogram(run);
            double[] correctInt = { 2000, 300 };
            Assert.IsFalse(Enumerable.SequenceEqual(correctInt, sC.intensities));
        }
    }
}

