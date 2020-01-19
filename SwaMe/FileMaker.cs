
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MzmlParser;
using MzqcGenerator;
using NLog;

namespace SwaMe
{
    public class FileMaker
    {
        private int division;
        private int MS2Density50;
        private int MS2DensityIQR;
        private int MS1Count;
        private int MS2Count;
        private int totalMS2IonCount;
        private string inputFileInclPath;
        private double RTDuration;
        private double swathSizeDifference;
        private Run run;
        private SwathGrouper.SwathMetrics swathMetrics;
        private RTGrouper.RTMetrics rtMetrics;
        private string dateTime;
        private string fileName;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public FileMaker(int division, string inputFileInclPath, Run run, SwathGrouper.SwathMetrics swathMetrics, RTGrouper.RTMetrics rtMetrics, double RTDuration, double swathSizeDifference, int MS2Count, int totalMS2IonCount, int MS2Density50, int MS2DensityIQR, int MS1Count, string dateTime)
        {
            this.swathMetrics = swathMetrics;
            this.division = division;
            this.inputFileInclPath = inputFileInclPath;
            this.run = run;
            this.rtMetrics = rtMetrics;
            this.RTDuration = RTDuration;
            this.swathSizeDifference = swathSizeDifference;
            this.MS2Count = MS2Count;
            this.totalMS2IonCount = totalMS2IonCount;
            this.MS2Density50 = MS2Density50;
            this.MS2DensityIQR = MS2DensityIQR;
            this.MS1Count = MS1Count;
            this.dateTime = dateTime;
            if (run.SourceFileNames[0].Contains("Func", System.StringComparison.CurrentCultureIgnoreCase))
            {
                this.fileName = run.ID;
            }
            else { this.fileName = run.SourceFileNames[0]; }
        }

        public void MakeMetricsPerSwathFile(SwathGrouper.SwathMetrics swathMetrics)
        {
            //tsv
            CreateOutputDirectory(inputFileInclPath);
            string swathFileName = dateTime + "_MetricsBySwath_" + fileName + ".tsv";
            StreamWriter streamWriter = new StreamWriter(swathFileName);
            streamWriter.Write("Filename \t swathNumber \t targetMz \t scansPerSwath \t AvgMzRange \t SwathProportionOfTotalTIC \t swDensityAverage \t swDensityIQR \t swAvgProportionSinglyCharged \n");

            for (int i = 0; i < swathMetrics.swathTargets.Count(); i++)
            {
                string[] swathNumber = { "swath", Convert.ToString(i + 1) };
                string[] phraseToWrite = { run.SourceFileNames[0], string.Join("_",swathNumber), Convert.ToString(swathMetrics.swathTargets[i]), Convert.ToString(swathMetrics.numOfSwathPerGroup.ElementAt(i)),
                    Convert.ToString(swathMetrics.mzRange.ElementAt(i)), Convert.ToString(swathMetrics.SwathProportionOfTotalTIC.ElementAt(i)),
                    Convert.ToString(swathMetrics.swDensity50[i]), Convert.ToString(swathMetrics.swDensityIQR[i]),
                    Convert.ToString(swathMetrics.SwathProportionPredictedSingleChargeAvg.ElementAt(i)) };

                streamWriter.Write(string.Join("\t", phraseToWrite));
                streamWriter.Write("\n");
            }
            streamWriter.Close();
            CheckColumnNumber(swathFileName, 9);
        }
        public void MakeMetricsPerRTsegmentFile(RTGrouper.RTMetrics rtMetrics)
        {
            CreateOutputDirectory(inputFileInclPath);
            string metricsPerRTSegmentFile = dateTime+ "_RTDividedMetrics_" + fileName + ".tsv";
            StreamWriter streamWriter = new StreamWriter(metricsPerRTSegmentFile);
            streamWriter.Write("Filename\t RTsegment \t MS2Peakwidths \t TailingFactor \t MS2PeakCapacity \t MS2Peakprecision \t MS1PeakPrecision \t DeltaTICAvgrage \t DeltaTICIQR \t AvgCycleTime \t AvgMS2Density \t AvgMS1Density \t MS2TICTotal \t MS1TICTotal \n");

            for (int segment = 0; segment < division; segment++)
            {
                //write streamWriter
                string[] RTSegment = { "RTsegment", Convert.ToString(segment + 1) };
                string[] phraseToWrite = { fileName, string.Join("_", RTSegment), Convert.ToString(rtMetrics.Peakwidths.ElementAt(segment)),
                    Convert.ToString(rtMetrics.TailingFactor.ElementAt(segment)), Convert.ToString(rtMetrics.PeakCapacity.ElementAt(segment)),
                    Convert.ToString(rtMetrics.PeakPrecision.ElementAt(segment)), Convert.ToString(rtMetrics.MS1PeakPrecision.ElementAt(segment)),
                    Convert.ToString(rtMetrics.TicChange50List.ElementAt(segment)), Convert.ToString(rtMetrics.TicChangeIqrList.ElementAt(segment)),
                    Convert.ToString(rtMetrics.CycleTime.ElementAt(segment)), Convert.ToString(rtMetrics.MS2Density.ElementAt(segment)),
                    Convert.ToString(rtMetrics.MS1Density.ElementAt(segment)),Convert.ToString(rtMetrics.MS2TicTotal.ElementAt(segment)),
                    Convert.ToString(rtMetrics.MS1TicTotal.ElementAt(segment))};

                streamWriter.Write(string.Join("\t", phraseToWrite)); 
                streamWriter.Write("\n");

            }
            streamWriter.Close();
            CheckColumnNumber(metricsPerRTSegmentFile, 14);
        }
        public void MakeUndividedMetricsFile()
        {
            CreateOutputDirectory(inputFileInclPath);
            string undividedFile = dateTime + "_undividedMetrics_" + fileName + ".tsv";
            StreamWriter streamWriter = new StreamWriter(undividedFile);
            streamWriter.Write("Filename \t StartTimeStamp \t MissingScans\t RTDuration \t swathSizeDifference \t  MS2Count \t swathsPerCycle \t totalMS2IonCount \t MS2Density50 \t MS2DensityIQR \t MS1Count \n");

            //write streamWriter
            string[] phraseToWrite = { fileName, run.StartTimeStamp, Convert.ToString(run.MissingScans), Convert.ToString(RTDuration),
                    Convert.ToString(swathSizeDifference), Convert.ToString(MS2Count),
                    Convert.ToString(swathMetrics.swathTargets.Count()), Convert.ToString(totalMS2IonCount),
                    Convert.ToString(MS2Density50), Convert.ToString(MS2DensityIQR),
                    Convert.ToString(MS1Count)};

            streamWriter.Write(string.Join("\t", phraseToWrite));
            streamWriter.Close();
            CheckColumnNumber(undividedFile, 11);
        }

        public void MakeiRTmetricsFile(Run run)
        {
            CreateOutputDirectory(inputFileInclPath);
            string filename = dateTime + "_iRTMetrics_" + fileName + ".tsv";
            StreamWriter streamWriter = new StreamWriter(filename);
            streamWriter.Write("Filename\t iRTPeptideMz \t RetentionTime\t Peakwidth \t TailingFactor \n");

            

            foreach (IRTPeak peak in run.IRTPeaks)
            {

                //write streamWriter
                string[] phraseToWrite = { fileName, Convert.ToString(peak.Mz), Convert.ToString(peak.RetentionTime),
                    Convert.ToString(peak.FWHM), Convert.ToString(peak.Peaksym)};

                streamWriter.Write(string.Join("\t", phraseToWrite));
                streamWriter.Write("\n");
            }
            streamWriter.Close();
            CheckColumnNumber(filename, 5);
        }

        public void CreateAndSaveMzqc()
        {
            //Declare units:
            JsonClasses.Unit Count = new JsonClasses.Unit() { cvRef = "UO", accession = "UO:0000189", name = "count" };
            JsonClasses.Unit Second = new JsonClasses.Unit() { cvRef = "UO", accession = "UO:0000010", name = "second" };
            JsonClasses.Unit Mz = new JsonClasses.Unit() { cvRef = "MS", accession = "MS:1000040", name = "m/z" };
            JsonClasses.Unit Ratio = new JsonClasses.Unit() { cvRef = "UO", accession = "UO:0010006", name = "ratio" };//Also doesn't exist, need to re-evaluate...
            JsonClasses.Unit Intensity = new JsonClasses.Unit() { cvRef = "MS", accession = "MS:1000042", name = "Peak Intensity" };

            //Start with the long part: adding all the metrics
            JsonClasses.QualityParameters[] qualityParameters = new JsonClasses.QualityParameters[25];
            qualityParameters[0] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:4000053", name = "Quameter metric: RT-Duration", unit = Second, value = RTDuration };
            qualityParameters[1] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXX", name = "SwaMe metric: swathSizeDifference", unit = Mz, value = swathSizeDifference };
            qualityParameters[2] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:4000060", name = "Quameter metric: MS2-Count", unit = Count, value = MS2Count };
            qualityParameters[3] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXX", name = "SwaMe metric: NumOfSwaths", unit = Count, value = swathMetrics.swathTargets.Count() };
            qualityParameters[4] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXX", name = "SwaMe metric: Target mz", unit = Mz, value = swathMetrics.swathTargets };
            qualityParameters[5] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXX", name = "SwaMe metric: TotalMS2IonCount", unit = Count, value = totalMS2IonCount };
            qualityParameters[6] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXX", name = "SwaMe metric: MS2Density50", unit = Count, value = MS2Density50 };
            qualityParameters[7] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXX", name = "SwaMe metric: MS2DensityIQR", unit = Count, value = MS2DensityIQR };
            qualityParameters[8] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:4000059", name = "Quameter metric: MS1-Count", unit = Count, value = MS1Count };
            qualityParameters[9] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: scansPerSwathGroup", unit = Count, value = swathMetrics.numOfSwathPerGroup };
            qualityParameters[10] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: AvgMzRange", unit = Mz, value = swathMetrics.mzRange };
            qualityParameters[11] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: SwathProportionOfTotalTIC", unit = Ratio, value = swathMetrics.SwathProportionOfTotalTIC };
            qualityParameters[12] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: swDensity50", unit = Count, value = swathMetrics.swDensity50 };
            qualityParameters[13] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: swDensityIQR", unit = Count, value = swathMetrics.swDensityIQR };
            qualityParameters[14] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: Peakwidths", unit = Second, value = rtMetrics.Peakwidths };
            qualityParameters[15] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: PeakCapacity", unit = Count, value = rtMetrics.PeakCapacity };
            qualityParameters[16] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: TailingFactor", unit = Count, value = rtMetrics.TailingFactor };
            qualityParameters[17] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: MS2PeakPrecision", unit = Mz, value = rtMetrics.PeakPrecision };
            qualityParameters[17] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: MS1PeakPrecision", unit = Mz, value = rtMetrics.MS1PeakPrecision };
            qualityParameters[18] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: DeltaTICAverage", unit = Intensity, value = rtMetrics.TicChange50List };
            qualityParameters[19] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: DeltaTICIQR", unit = Intensity, value = rtMetrics.TicChangeIqrList };
            qualityParameters[20] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: AvgScanTime", unit = Second, value = rtMetrics.CycleTime };
            qualityParameters[21] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: MS2Density", unit = Count, value = rtMetrics.MS2Density };
            qualityParameters[22] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: MS1Density", unit = Count, value = rtMetrics.MS1Density };
            qualityParameters[23] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: MS2TICTotal", unit = Count, value = rtMetrics.MS2TicTotal };
            qualityParameters[24] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: MS1TICTotal", unit = Count, value = rtMetrics.MS1TicTotal };

            //Now for the other stuff
            JsonClasses.FileFormat fileFormat = new JsonClasses.FileFormat() { };
            List<JsonClasses.FileProperties> fileProperties = new List<JsonClasses.FileProperties>() { };
            JsonClasses.FileProperties fileProperty = new JsonClasses.FileProperties() {cvRef="MS", accession = run.FilePropertiesAccession, name = "SHA-1", value = run.SourceFileChecksums[0] };
            JsonClasses.FileProperties completionTime = new JsonClasses.FileProperties() { cvRef = "MS", accession = "MS:1000747", name = "completion time", value = run.CompletionTime };
            fileProperties.Add(fileProperty);
            List<JsonClasses.InputFiles> inputFiles = new List<JsonClasses.InputFiles>();
            JsonClasses.InputFiles inputFile = new JsonClasses.InputFiles() { location = "file://" + inputFileInclPath, name = fileName, fileFormat = fileFormat, fileProperties = fileProperties };
            inputFiles.Add(inputFile);
            List<JsonClasses.AnalysisSoftware> analysisSoftwarelist = new List<JsonClasses.AnalysisSoftware>();
            JsonClasses.AnalysisSoftware analysisSoftware = new JsonClasses.AnalysisSoftware() { cvRef = "MS", accession = "XXXXXXXXXXXXXX", name = "SwaMe", uri = "https://github.com/PaulBrack/Yamato/tree/master/Console", version = "1.0" };
            analysisSoftwarelist.Add(analysisSoftware);
            JsonClasses.MetaData metadata = new JsonClasses.MetaData() { inputFiles = inputFiles, analysisSoftware = analysisSoftwarelist };
            JsonClasses.RunQuality runQualitySingle = new JsonClasses.RunQuality() { metadata = metadata, qualityParameters = qualityParameters };
            List<JsonClasses.RunQuality> runQuality = new List<JsonClasses.RunQuality>();
            runQuality.Add(runQualitySingle);
            JsonClasses.NUV qualityControl = new JsonClasses.NUV() { name = "Proteomics Standards Initiative Quality Control Ontology", uri = "https://raw.githubusercontent.com/HUPO-PSI/mzqc/master/cv/v0_0_11/qc-cv.obo", version = "0.1.0" };
            JsonClasses.NUV massSpectrometry = new JsonClasses.NUV() { name = "Proteomics Standards Initiative Mass Spectrometry Ontology", uri = "https://raw.githubusercontent.com/HUPO-PSI/psi-ms-CV/master/psi-ms.obo", version = "4.1.7" };
            JsonClasses.NUV UnitOntology = new JsonClasses.NUV() { name = "Unit Ontology", uri = "https://raw.githubusercontent.com/bio-ontology-research-group/unit-ontology/master/unit.obo", version = "09:04:2014 13:37" };
            JsonClasses.CV cV = new JsonClasses.CV() { QC = qualityControl, MS = massSpectrometry, UO = UnitOntology };
            JsonClasses.MzQC metrics = new JsonClasses.MzQC() { runQuality = runQuality, cv = cV };

            //Then save:
            string mzQCFile = @"metrics_" + fileName+ ".json";
            new MzqcGenerator.MzqcWriter().WriteMzqc(mzQCFile, metrics);

        }

        public void CombineMultipleFilesIntoSingleFile(string inputFileNamePattern, string outputFileName)
        {
            string[] inputFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), inputFileNamePattern, SearchOption.AllDirectories);
            CheckOutputDirectory(inputFileInclPath);
            StreamWriter combinedwriter = new StreamWriter(outputFileName);
            int counter = 0;
            foreach (var inputFile in inputFiles)
            {
                 var inputStream = File.ReadAllLines(inputFile);
                 if (counter != 0) inputStream = inputStream.Skip(1).ToArray();
                 foreach (string line in inputStream)
                 {
                     combinedwriter.WriteLine(line);
                 }
                 counter++;
            }
            combinedwriter.Close();
        }

        public void CheckColumnNumber(string inputFile, int desiredColumnNumbers)
        {
            string[] lines = File.ReadAllLines(inputFile);
            string[] items = lines[0].Split('\t');
            if (items.Count() != desiredColumnNumbers)
            {
                logger.Error(inputFile + "does not appear to contain all the desired columns.");
            }
        }
        public void CheckOutputDirectory(string inputFileInclPath) 
        {
            string filePath = Path.Join(GetFilePathWithoutExtension(inputFileInclPath), "SwaMe_results");

            if (Directory.GetCurrentDirectory() != filePath && !string.IsNullOrEmpty(filePath)) Directory.SetCurrentDirectory(filePath);
        }
        public void CreateOutputDirectory(string inputFileInclPath)
        {
            string originalFilePath = GetFilePathWithoutExtension(inputFileInclPath);
            string[] filePaths = { originalFilePath, "SwaMe_results",Path.GetFileNameWithoutExtension(inputFileInclPath), dateTime };
            string filePath = Path.Combine(filePaths);
            DirectoryInfo di = Directory.CreateDirectory(filePath);
            Directory.SetCurrentDirectory(filePath);
        }
        public string GetFilePathWithoutExtension(string inputFileInclPath)
        {
            string filePath;
            if (inputFileInclPath.Contains(".mzML", StringComparison.InvariantCultureIgnoreCase))
            {
                filePath = Path.GetDirectoryName(inputFileInclPath);
            }
            else { filePath = inputFileInclPath; }
            return filePath;
        }

    }
   
}


            