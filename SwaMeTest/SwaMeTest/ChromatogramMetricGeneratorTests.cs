using Microsoft.VisualStudio.TestTools.UnitTesting;
using MzmlParser;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SwaMe.Test
{
    [TestClass]
    public class MetricGeneratorTest
    {
        private static Run emptyBasePeakRun;
        private static Run basepeak1Run;
        private static ChromatogramMetricGenerator bpk1CMG;
        private static ChromatogramMetricGenerator emptyCMG;

        [TestInitialize]
        public void Initialize()
        {

            emptyBasePeakRun = new Run
            {
                AnalysisSettings = new AnalysisSettings
                {
                    RtTolerance = 2.5
                }
            };
            basepeak1Run = new Run
            {
                AnalysisSettings = new AnalysisSettings
                {
                    RtTolerance = 2.5
                }
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
            var basePeak1 = new BasePeak(150,2.5,150)
            {
                BpkRTs = new List<double>() { 2.5, 2.58 },
                Spectrum = new List<SpectrumPoint>() { spectrumpoint1, spectrumpoint2 }
            };
            var emptyBasePeak = new BasePeak(1, 1, 1)
            {
                Spectrum = new List<SpectrumPoint>() { }
            };

            basepeak1Run.BasePeaks = new ConcurrentBag<BasePeak>() { basePeak1 };
            bpk1CMG = new ChromatogramMetricGenerator();
            bpk1CMG.GenerateChromatogram(basepeak1Run);

            emptyBasePeakRun.BasePeaks = new ConcurrentBag<BasePeak>() {emptyBasePeak};
            emptyCMG = new ChromatogramMetricGenerator();
            emptyCMG.GenerateChromatogram(emptyBasePeakRun);

        }
               

        [TestMethod]
        public void IntensitiesCorrectIfOneBasePeak()
        {
            double[] correctInt = { 1976.1904805155295, 2000.00000000000, 2010.2040816326535, 2020.408163265306, 2030.6122448979597, 2040.8163265306123, 2051.020408163266, 2061.2244897959181, 2071.428571428572, 2081.6326530612246, 2091.836734693878, 2102.0408163265306, 2112.2448979591841, 2122.4489795918366, 2132.65306122449, 2142.8571428571427, 2153.0612244897961, 2163.2653061224487, 2173.4693877551026, 2183.6734693877552, 2193.8775510204086, 2204.0816326530612, 2214.2857142857147, 2224.4897959183672, 2234.6938775510207, 2244.8979591836733, 2255.1020408163267, 2265.3061224489793, 2275.5102040816328, 2285.7142857142853, 2295.9183673469388, 2306.1224489795914, 2316.3265306122453, 2326.5306122448974, 2336.7346938775513, 2346.9387755102039, 2357.1428571428573, 2367.34693877551, 2377.5510204081634, 2387.7551020408159, 2397.9591836734694, 2408.163265306122, 2418.3673469387754, 2428.571428571428, 2438.7755102040819, 2448.9795918367345, 2459.1836734693879, 2469.3877551020405, 2479.591836734694, 2489.7959183673465, 2500.00000000000, 2510.2040816326535, 2520.408163265306, 2530.6122448979586, 2540.8163265306121, 2551.0204081632655, 2561.2244897959181, 2571.428571428572, 2581.6326530612246, 2591.836734693878, 2602.0408163265306, 2612.2448979591841, 2622.4489795918366, 2632.65306122449, 2642.8571428571427, 2653.0612244897966, 2663.2653061224487, 2673.4693877551026, 2683.6734693877552, 2693.8775510204086, 2704.0816326530612, 2714.2857142857147, 2724.4897959183672, 2734.6938775510207, 2744.8979591836733, 2755.1020408163267, 2765.3061224489793, 2775.5102040816328, 2785.7142857142853, 2795.9183673469388, 2806.1224489795914, 2816.3265306122448, 2826.5306122448974, 2836.7346938775509, 2846.9387755102034, 2857.1428571428569, 2867.3469387755094, 2877.5510204081634, 2887.7551020408159, 2897.9591836734694, 2908.163265306122, 2918.3673469387754, 2928.571428571428, 2938.7755102040815, 2948.979591836734, 2959.1836734693875, 2969.38775510204, 2979.591836734694, 2989.7959183673465, 3000.00000000000 };
            Assert.IsTrue(Enumerable.SequenceEqual(correctInt, bpk1CMG.intensities));
        }

        [TestMethod]
        public void IntensitiesCorrectIfBasePeakSpectrumEmpty()
        {
            double[] correctInt = { };
            Assert.IsTrue(Enumerable.SequenceEqual(correctInt, emptyCMG.intensities));
        }
    }
}

