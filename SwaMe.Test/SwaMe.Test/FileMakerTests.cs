using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MzmlParser;
using static SwaMe.RTGrouper;
using Yamato.Console;

namespace SwaMe.Test
{
    [TestClass]
    public class FileMakerTests
    {
        /// <summary>
        /// FileMaker is where all the tsv and json files get written. This file writes into the temp folder and checks that the process went smoothly.
        /// </summary>
        /// 
        //IRTPeaks
        private static IRTPeak iRTPeak1 = new IRTPeak()
        {
            Mz = 550,
            RetentionTime = 20,
            FWHM = 18,
            Peaksym = 0.5
        };
        private static IRTPeak iRTPeak2 = new IRTPeak()
        {
            Mz = 825,
            RetentionTime = 30,
            FWHM = 30,
            Peaksym = 0.9
        };
        //Run
        Run iRTrun = new Run()
        {
            IRTPeaks = {iRTPeak1, iRTPeak2 },
            SourceFileNames = { "File1", "File2"}
        };


        //SwathMetrics
        private static List<double> SwathTargets = new List<double>() { 550,1050 };
        private static List<int> NumOfSwathPerGroup = new List<int>() { 5,5 };
        private static List<double> MzRange = new List<double>() { 5,18 };
        private static List<double> TICs = new List<double>() { 20500, 40000 };
        private static List<double> SwDensity50 = new List<double>() { 2, 8 };
        private static List<double> SwDensityIQR = new List<double>() { 1, 2 };
        private static List<double> SwathProportionOfTotalTIC = new List<double>() { 0.20,0.80 };
        private static List<double> SwathProportionPredictedSingleChargeAvg = new List<double>() { 0.30, 0.70 };

        private static SwathGrouper.SwathMetrics swathMetrics = new SwathGrouper.SwathMetrics(SwathTargets, 1000, NumOfSwathPerGroup, MzRange, TICs, SwDensity50, SwDensityIQR,
            SwathProportionOfTotalTIC, SwathProportionPredictedSingleChargeAvg)
        { };

        //run
        private static Run RunWithoutIRT = new Run()
        { 
        SourceFileNames = { "File1", "File2"},
        SourceFileChecksums = {"aaa","bbb" },
        StartTimeStamp = "2017-02-26T13:07:31Z"
        };

        //RTMetrics
        private static List<double> MS1TICTotal = new List<double>() { 1000,3000 };
        private static List<double> MS2TICTotal = new List<double>() { 2000, 4000 };
        private static List<double> CycleTime = new List<double>() { 2, 4 };
        private static List<double> TICchange50List = new List<double>() { 450,650 };
        private static List<double> TICchangeIQRList = new List<double>() {21, 51 }; 
        private static List<int> MS1Density = new List<int>() { 5, 5 };
        private static List<int> MS2Density = new List<int>() { 6, 5 };
        private static List<double> Peakwidths = new List<double>() { 20, 40 };
        private static List<double> TailingFactor = new List<double>() { 30, 60 };
        private static List<double> PeakPrecision = new List<double>() { 33,66 };
        private static List<double> MS1PeakPrecision = new List<double>() { 36, 68 };
        private static List<double> PeakCapacity = new List<double>() { 44, 120 };
        private static List<string> segmentBoundaries = new List<string>() { "2.5_3.3", "3.3_4.0" };//segmentBoundaries is a string denoting the startOfTheRTsegment_endOfTheRTsegment for reference

        private static RTMetrics RTMetrics = new RTMetrics(MS1TICTotal, MS2TICTotal, CycleTime, TICchange50List, TICchangeIQRList, MS1Density, MS2Density, Peakwidths, TailingFactor,
            PeakCapacity, PeakPrecision, MS1PeakPrecision, segmentBoundaries) { };

        
        [TestInitialize]
        public void Initialize() 
        {
            Yamato.Console.DirectoryCreator.CreateOutputDirectory(Path.GetTempPath(), "Today");
            FileMaker fileMaker = new FileMaker(2, Path.GetTempPath(), RunWithoutIRT, swathMetrics, RTMetrics, 70, 2, 10, 1000, 50, 20, 5000, "Today") { };

            fileMaker.MakeMetricsPerSwathFile(swathMetrics);
            fileMaker.MakeMetricsPerRTsegmentFile(RTMetrics);
            fileMaker.MakeComprehensiveMetricsFile();
            fileMaker.MakeiRTmetricsFile(iRTrun);
            //fileMaker.AssembleMetrics();
        }
        /// <remarks>
        ///Writes a tsv swathmetrics file and then reads it back in to verify it.
        /// </remarks>
        [TestMethod]
        public void SwathMetricsFileCorrect()
        {
            string correctLine1 = "Filename\tswathNumber\ttargetMz\tscansPerSwath\tAvgMzRange\tSwathProportionOfTotalTIC\tswDensityAverage\tswDensityIQR\tswAvgProportionSinglyCharged";
            string correctLine2 = "File1\tswath_1\t550\t5\t5\t0.2\t2\t1\t0.3";
            string correctLine3 = "File1\tswath_2\t1050\t5\t18\t0.8\t8\t2\t0.7";
            List<string> correctText = new List<string>() { correctLine1, correctLine2, correctLine3 };

            var fileText = File.ReadLines(Path.Combine(Path.GetTempPath(),"QC_Results", "Today", "Today_MetricsBySwath_File1.tsv"));

            if (fileText.ElementAt(1).Contains(","))//Stupid South Africa and its commas for decimals rules. The theory is that if they are there, they should be in both the second and third line, so we only need to check if its in the second line.
            {
                List<string> newfileText = new List<string>();
                for (int line = 0; line < fileText.Count(); line++)
                {
                    newfileText.Add(fileText.ElementAt(line).Replace(",", "."));
                }
                Assert.IsTrue(Enumerable.SequenceEqual(newfileText, correctText));
            }
            else
            {
                Assert.IsTrue(Enumerable.SequenceEqual(fileText, correctText));

            }
        }
        /// <remarks>
        ///Writes a tsv RTMetrics file and then reads it back in to verify it.
        /// </remarks>
        [TestMethod]
        public void RTMetricsFileCorrect()
        {
            

            var fileText = File.ReadLines(Path.Combine(Path.GetTempPath(), "QC_Results", "Today", "Today_RTDividedMetrics_File1.tsv"));
            string correctLine1 = "Filename\tRTsegment\tsegmentBoundaries\tMS2Peakwidths\tTailingFactor\tMS2PeakCapacity\tMS2Peakprecision\tMS1PeakPrecision\tDeltaTICAvgrage\tDeltaTICIQR\tAvgCycleTime\tAvgMS2Density\tAvgMS1Density\tMS2TICTotal\tMS1TICTotal";
            string correctLine2 = "File1\tRTsegment_1\t2.5_3.3\t20\t30\t44\t33\t36\t450\t21\t2\t6\t5\t2000\t1000";
            string correctLine3 = "File1\tRTsegment_2\t3.3_4.0\t40\t60\t120\t66\t68\t650\t51\t4\t5\t5\t4000\t3000";
            List<string> correctText = new List<string>() { correctLine1, correctLine2, correctLine3 };

            if (fileText.ElementAt(1).Contains(","))//Stupid South Africa and its commas for decimals rules. The theory is that if they are there, they should be in both the second and third line, so we only need to check if its in the second line.
            {
                List<string> newfileText = new List<string>();
                for (int line = 0; line < fileText.Count(); line++)
                {
                    newfileText.Add(fileText.ElementAt(line).Replace(",", "."));
                }
                Assert.IsTrue(Enumerable.SequenceEqual(newfileText, correctText));
            }
            else
            {
                Assert.IsTrue(Enumerable.SequenceEqual(fileText, correctText));

            }
        }
        /// <remarks>
        ///Writes a tsv file for the Comprehensive metrics and then reads it back in to verify it.
        /// </remarks>
        [TestMethod]
        public void ComprehensiveFileCorrect()
        {


            var fileText = File.ReadLines(Path.Combine(Path.GetTempPath(), "QC_Results", "Today", "Today_ComprehensiveMetrics_File1.tsv"));
            string correctLine1 = "Filename \t StartTimeStamp \t MissingScans\t RTDuration \t swathSizeDifference \t  MS2Count \t swathsPerCycle \t totalMS2IonCount \t MS2Density50 \t MS2DensityIQR \t MS1Count ";
            string correctLine2 = "File1\t2017-02-26T13:07:31Z\t0\t70\t2\t10\t2\t1000\t50\t20\t5000";
            List<string> correctText = new List<string>() { correctLine1, correctLine2 };

            if (fileText.ElementAt(1).Contains(","))//Stupid South Africa and its commas for decimals rules. The theory is that if they are there, they should be in both the second and third line, so we only need to check if its in the second line.
            {
                List<string> newfileText = new List<string>();
                for (int line = 0; line < fileText.Count(); line++)
                {
                    newfileText.Add(fileText.ElementAt(line).Replace(",", "."));
                }
                Assert.IsTrue(Enumerable.SequenceEqual(newfileText, correctText));
            }
            else
            {
                Assert.IsTrue(Enumerable.SequenceEqual(fileText, correctText));

            }
        }
        /// <remarks>
        ///Writes a tsv file for the iRT metrics with a run that includes iRTpeaks with all the necessary info and then reads it back in to verify it.
        /// </remarks>
        [TestMethod]
        public void IRTFileCorrect()
        {
            var fileText = File.ReadLines(Path.Combine(Path.GetTempPath(), "QC_Results", "Today", "Today_iRTMetrics_File1.tsv"));
            string correctLine1 = "Filename\tiRTPeptideMz\tRetentionTime\tPeakwidth\tTailingFactor";
            string correctLine2 = "File1\t825\t30\t30\t0.9";
            string correctLine3 = "File1\t550\t20\t18\t0.5";
            List<string> correctText = new List<string>() { correctLine1, correctLine2, correctLine3 };

            if (fileText.ElementAt(1).Contains(","))//Stupid South Africa and its commas for decimals rules. The theory is that if they are there, they should be in both the second and third line, so we only need to check if its in the second line.
            {
                List<string> newfileText = new List<string>();
                for (int line = 0; line < fileText.Count(); line++)
                {
                    newfileText.Add(fileText.ElementAt(line).Replace(",", "."));
                }
                Assert.IsTrue(Enumerable.SequenceEqual(newfileText, correctText));
            }
            else
            {
                Assert.IsTrue(Enumerable.SequenceEqual(fileText, correctText));
            }
        }

    }
}
