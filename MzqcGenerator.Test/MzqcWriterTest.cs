using Microsoft.VisualStudio.TestTools.UnitTesting;
using SwaMe;
using SwaMe.Pipeline;
using System.Collections.Generic;
using static SwaMe.RTGrouper;

namespace MzqcGenerator.Test
{
    [TestClass]
    public class MzqcWriterTest
    {
        /// <summary>
        /// FileMaker is where all the tsv and json files get written. This file writes into the temp folder and checks that the process went smoothly.
        /// </summary>
        /// 
        //IRTPeaks
        private static readonly IRTPeak iRTPeak1 = new IRTPeak()
        {
            Mz = 550,
            RetentionTime = 20,
            FWHM = 18,
            Peaksym = 0.5
        };
        private static readonly IRTPeak iRTPeak2 = new IRTPeak()
        {
            Mz = 825,
            RetentionTime = 30,
            FWHM = 30,
            Peaksym = 0.9
        };

        //Run
        private static readonly Run<Scan> iRTrun = new Run<Scan>()
        {
            IRTPeaks = { iRTPeak1, iRTPeak2 },
            SourceFileNames = { "File1", "File2" }
        };


        //SwathMetrics
        private static readonly List<double> SwathTargets = new List<double>() { 550, 1050 };
        private static readonly List<int> NumOfSwathPerGroup = new List<int>() { 5, 5 };
        private static readonly List<double> MzRange = new List<double>() { 5, 18 };
        private static readonly List<double> TICs = new List<double>() { 20500, 40000 };
        private static readonly List<double> SwDensity50 = new List<double>() { 2, 8 };
        private static readonly List<double?> SwDensityIQR = new List<double?>() { 1, 2 };
        private static readonly List<double> SwathProportionOfTotalTIC = new List<double>() { 0.20, 0.80 };
        private static readonly List<double> SwathProportionPredictedSingleChargeAvg = new List<double>() { 0.30, 0.70 };

        private static readonly SwathGrouper.SwathMetrics swathMetrics = new SwathGrouper.SwathMetrics(SwathTargets, 1000, NumOfSwathPerGroup, MzRange, TICs, SwDensity50, SwDensityIQR,
            SwathProportionOfTotalTIC, SwathProportionPredictedSingleChargeAvg)
        { };

        //run
        private static readonly Run<Scan> RunWithoutIRT = new Run<Scan>()
        {
            SourceFileNames = { "File1", "File2" },
            SourceFileChecksums = { "aaa", "bbb" },
            StartTimeStamp = "2017-02-26T13:07:31Z"
        };

        //RTMetrics
        private static readonly List<double> MS1TICTotal = new List<double>() { 1000, 3000 };
        private static readonly List<double> MS2TICTotal = new List<double>() { 2000, 4000 };
        private static readonly List<double> CycleTime = new List<double>() { 2, 4 };
        private static readonly List<double> TICchange50List = new List<double>() { 450, 650 };
        private static readonly List<double> TICchangeIQRList = new List<double>() { 21, 51 };
        private static readonly List<int> MS1Density = new List<int>() { 5, 5 };
        private static readonly List<int> MS2Density = new List<int>() { 6, 5 };
        private static readonly List<double> Peakwidths = new List<double>() { 20, 40 };
        private static readonly List<double> TailingFactor = new List<double>() { 30, 60 };
        private static readonly List<double> PeakPrecision = new List<double>() { 33, 66 };
        private static readonly List<double> MS1PeakPrecision = new List<double>() { 36, 68 };
        private static readonly List<double> PeakCapacity = new List<double>() { 44, 120 };
        private static readonly List<string> segmentBoundaries = new List<string>() { "2.5_3.3", "3.3_4.0" };


        private static readonly RTMetrics RTMetrics = new RTMetrics(MS1TICTotal, MS2TICTotal, CycleTime, TICchange50List, TICchangeIQRList, MS1Density, MS2Density, Peakwidths, TailingFactor,
            PeakCapacity, PeakPrecision, MS1PeakPrecision, segmentBoundaries)
        { };


        [TestInitialize]
        public void Initialize()
        {
            var mg = new SwaMe.MetricGenerator();
            mg.GenerateMetrics(RunWithoutIRT, 100, false);
            var metrics = mg.AssembleMetrics();

            new MzqcWriter().BuildMzqcAndWrite("test.json", RunWithoutIRT, metrics, "", null);
        }
        /*
        /// <remarks>
        ///Writes a .json file for the iRT metrics with a run that includes iRTpeaks with all the necessary info and then reads it back in to verify it.
        /// </remarks>
        [TestMethod]
        public void JSONFileCorrect()
        {
            List<string> fileText = File.ReadLines("test.json").ToList();
            string temppath = Path.GetTempPath();
            temppath = temppath.Replace("\\", "\\\\");
            string correctLine = string.Concat("{ \"mzQC\":{\"version\":\"0.0.11\",\"runQuality\":[{\"metadata\":{\"inputFiles\":[{\"location\":\"file://", temppath, "\",\"name\":\"File1\",\"fileFormat\":{\"cvRef\":\"MS\",\"accession\":\"MS:1000584\",\"name\":\"mzML format\"},\"fileProperties\":[{\"cvRef\":\"MS\",\"name\":\"SHA-1\",\"value\":\"aaa\"}]}],\"analysisSoftware\":[{\"cvRef\":\"MS\",\"accession\":\"XXXXXXXXXXXXXX\",\"name\":\"SwaMe\",\"version\":\"1.0\",\"uri\":\"https://github.com/PaulBrack/Yamato/tree/master/Console\"}]},\"qualityParameters\":[{\"cvRef\":\"QC\",\"accession\":\"QC:4000053\",\"name\":\"Quameter metric: RT-Duration\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000010\",\"name\":\"second\"},\"value\":70.0},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXX\",\"name\":\"SwaMe metric: swathSizeDifference\",\"unit\":{\"cvRef\":\"MS\",\"accession\":\"MS:1000040\",\"name\":\"m/z\"},\"value\":2.0},{\"cvRef\":\"QC\",\"accession\":\"QC:4000060\",\"name\":\"Quameter metric: MS2-Count\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":10},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXX\",\"name\":\"SwaMe metric: NumOfSwaths\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":2},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXX\",\"name\":\"SwaMe metric: Target mz\",\"unit\":{\"cvRef\":\"MS\",\"accession\":\"MS:1000040\",\"name\":\"m/z\"},\"value\":[550.0,1050.0]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXX\",\"name\":\"SwaMe metric: TotalMS2IonCount\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":1000},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXX\",\"name\":\"SwaMe metric: MS2Density50\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":50},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXX\",\"name\":\"SwaMe metric: MS2DensityIQR\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":20},{\"cvRef\":\"QC\",\"accession\":\"QC:4000059\",\"name\":\"Quameter metric: MS1-Count\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":5000},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: scansPerSwathGroup\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":[5,5]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: AvgMzRange\",\"unit\":{\"cvRef\":\"MS\",\"accession\":\"MS:1000040\",\"name\":\"m/z\"},\"value\":[5.0,18.0]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: SwathProportionOfTotalTIC\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0010006\",\"name\":\"ratio\"},\"value\":[0.2,0.8]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: swDensity50\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":[2.0,8.0]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: swDensityIQR\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":[1.0,2.0]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: Peakwidths\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000010\",\"name\":\"second\"},\"value\":[20.0,40.0]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: PeakCapacity\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":[44.0,120.0]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: TailingFactor\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":[30.0,60.0]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: MS1PeakPrecision\",\"unit\":{\"cvRef\":\"MS\",\"accession\":\"MS:1000040\",\"name\":\"m/z\"},\"value\":[36.0,68.0]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: DeltaTICAverage\",\"unit\":{\"cvRef\":\"MS\",\"accession\":\"MS:1000042\",\"name\":\"Peak Intensity\"},\"value\":[450.0,650.0]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: DeltaTICIQR\",\"unit\":{\"cvRef\":\"MS\",\"accession\":\"MS:1000042\",\"name\":\"Peak Intensity\"},\"value\":[21.0,51.0]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: AvgScanTime\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000010\",\"name\":\"second\"},\"value\":[2.0,4.0]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: MS2Density\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":[6,5]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: MS1Density\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":[5,5]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: MS2TICTotal\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":[2000.0,4000.0]},{\"cvRef\":\"QC\",\"accession\":\"QC:XXXXXXXX\",\"name\":\"SwaMe metric: MS1TICTotal\",\"unit\":{\"cvRef\":\"UO\",\"accession\":\"UO:0000189\",\"name\":\"count\"},\"value\":[1000.0,3000.0]}]}],\"cv\":{\"QC\":{\"name\":\"Proteomics Standards Initiative Quality Control Ontology\",\"uri\":\"https://raw.githubusercontent.com/HUPO-PSI/mzqc/master/cv/v0_0_11/qc-cv.obo\",\"version\":\"0.1.0\"},\"MS\":{\"name\":\"Proteomics Standards Initiative Mass Spectrometry Ontology\",\"uri\":\"https://raw.githubusercontent.com/HUPO-PSI/psi-ms-CV/master/psi-ms.obo\",\"version\":\"4.1.7\"},\"UO\":{\"name\":\"Unit Ontology\",\"uri\":\"https://raw.githubusercontent.com/bio-ontology-research-group/unit-ontology/master/unit.obo\",\"version\":\"09:04:2014 13:37\"}}}}");
            List<string> correctText = new List<string>();
            correctText.Add(correctLine);
            Assert.IsTrue(Enumerable.SequenceEqual(fileText, correctText));

        }

    */
    }
}
