using Microsoft.VisualStudio.TestTools.UnitTesting;
using MzmlParser;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SwaMe.Test
{
    [TestClass]
    public class ChromatogramMetricGeneratorTest
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

            var spectrumpoint1 = new SpectrumPoint(2000, 150, 2.58F);
            var spectrumpoint2 = new SpectrumPoint(3000, 150.01F, 3.00F);

            var basePeak1 = new BasePeak(150,2.5,150)
            {
                BpkRTs = new List<double>() { 2.5 },
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
        [TestMethod]
        public void StartTimesCorrectIfOneBasePeak()
        {
            double[] correctST = {2.5699999237060549,2.5799999237060547,2.5842856387702788,2.5885713538345025,2.5928570688987267,2.5971427839629504,2.6014284990271745,2.6057142140913983,2.6099999291556224,2.6142856442198461,2.6185713592840703,2.622857074348294,2.6271427894125181,2.6314285044767418,2.635714219540966,2.6399999346051897,2.6442856496694138,2.6485713647336375,2.6528570797978617,2.6571427948620854,2.6614285099263095,2.6657142249905332,2.6699999400547574,2.6742856551189811,2.6785713701832052,2.6828570852474289,2.6871428003116531,2.6914285153758768,2.6957142304401009,2.6999999455043246,2.7042856605685488,2.7085713756327725,2.7128570906969967,2.7171428057612204,2.7214285208254445,2.7257142358896682,2.7299999509538924,2.7342856660181161,2.73857138108234,2.7428570961465639,2.7471428112107881,2.7514285262750118,2.7557142413392359,2.7599999564034596,2.7642856714676838,2.7685713865319075,2.7728571015961316,2.7771428166603553,2.7814285317245795,2.7857142467888032,2.7899999618530273,2.7942856769172515,2.7985713919814752,2.8028571070456989,2.8071428221099231,2.8114285371741472,2.8157142522383709,2.8199999673025951,2.8242856823668188,2.8285713974310429,2.8328571124952666,2.8371428275594908,2.8414285426237145,2.8457142576879386,2.8499999727521623,2.8542856878163865,2.85857140288061,2.8628571179448343,2.867142833009058,2.8714285480732822,2.8757142631375059,2.87999997820173,2.8842856932659537,2.8885714083301779,2.8928571233944016,2.8971428384586257,2.9014285535228495,2.9057142685870736,2.9099999836512973,2.9142856987155215,2.9185714137797452,2.9228571288439693,2.927142843908193,2.9314285589724172,2.9357142740366409,2.939999989100865,2.9442857041650887,2.9485714192293129,2.9528571342935366,2.9571428493577607,2.9614285644219844,2.9657142794862086,2.9699999945504323,2.9742857096146564,2.97857142467888,2.9828571397431043,2.987142854807328,2.9914285698715521,2.9957142849357759,3.00000000000000};
                Assert.IsTrue(Enumerable.SequenceEqual(correctST, bpk1CMG.starttimes));
        }

        [TestMethod]
        public void StartTimesCorrectIfBasePeakSpectrumEmpty()
        {
            double[] correctST = { };
            Assert.IsTrue(Enumerable.SequenceEqual(correctST, emptyCMG.starttimes));
        }

        [TestMethod]
        public void  basepeakFWHMsCorrectIfOneBasePeak()
        {
            var correctFWHM = 50.666656494140625; 
            Assert.AreEqual(correctFWHM, basepeak1Run.BasePeaks.ElementAt(0).FWHMs.ElementAt(0));
        }
        [TestMethod]
        public void basepeakFWHMsZeroIfBasePeakSpectrumEmpty()
        {
            var correctFWHM =0;
            Assert.AreEqual(correctFWHM, emptyBasePeakRun.BasePeaks.ElementAt(0).FWHMs.ElementAt(0));
        }
        [TestMethod]
        public void basepeakPeakSymsCorrectIfOneBasePeak()
        {
            var correctPeakSym = 0.50498342514038086;
            Assert.AreEqual(correctPeakSym, basepeak1Run.BasePeaks.ElementAt(0).Peaksyms.ElementAt(0));
        }
        [TestMethod]
        public void basepeakPeaksymsZeroIfBasePeakSpectrumEmpty()
        {
            var correctPeakSym = 0;
            Assert.AreEqual(correctPeakSym, emptyBasePeakRun.BasePeaks.ElementAt(0).Peaksyms.ElementAt(0));
        }
        [TestMethod]
        public void basepeakFullWidthAtBaselineCorrectIfOneBasePeak()
        {
            var correctFullWidthAtBaseline =  96.266693115234375;
            Assert.AreEqual(correctFullWidthAtBaseline, basepeak1Run.BasePeaks.ElementAt(0).FullWidthBaselines.ElementAt(0));
        }
        [TestMethod]
        public void basepeakFullWidthAtBaselineZeroIfBasePeakSpectrumEmpty()
        {
            var correctFullWidthAtBaseline = 0;
            Assert.AreEqual(correctFullWidthAtBaseline, emptyBasePeakRun.BasePeaks.ElementAt(0).FullWidthBaselines.ElementAt(0));
        }
    }
}

