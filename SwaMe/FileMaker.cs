
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
            streamWriter.Write("Filename\tswathNumber\ttargetMz\tscansPerSwath\tAvgMzRange\tSwathProportionOfTotalTIC\tswDensityAverage\tswDensityIQR\tswAvgProportionSinglyCharged\n");

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
            string metricsPerRTSegmentFile = dateTime + "_RTDividedMetrics_" + fileName + ".tsv";
            StreamWriter streamWriter = new StreamWriter(metricsPerRTSegmentFile);
            streamWriter.Write("Filename\tRTsegment\tMS2Peakwidths\tTailingFactor\tMS2PeakCapacity\tMS2Peakprecision\tMS1PeakPrecision\tDeltaTICAvgrage\tDeltaTICIQR\tAvgCycleTime\tAvgMS2Density\tAvgMS1Density\tMS2TICTotal\tMS1TICTotal\n");

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
        public void MakeComprehensiveMetricsFile()
        {
            CreateOutputDirectory(inputFileInclPath);
            string ComprehensiveFile = dateTime + "_ComprehensiveMetrics_" + fileName + ".tsv";
            StreamWriter streamWriter = new StreamWriter(ComprehensiveFile);
            streamWriter.Write("Filename \t StartTimeStamp \t MissingScans\t RTDuration \t swathSizeDifference \t  MS2Count \t swathsPerCycle \t totalMS2IonCount \t MS2Density50 \t MS2DensityIQR \t MS1Count \n");

            //write streamWriter
            string[] phraseToWrite = { fileName, run.StartTimeStamp, Convert.ToString(run.MissingScans), Convert.ToString(RTDuration),
                    Convert.ToString(swathSizeDifference), Convert.ToString(MS2Count),
                    Convert.ToString(swathMetrics.swathTargets.Count()), Convert.ToString(totalMS2IonCount),
                    Convert.ToString(MS2Density50), Convert.ToString(MS2DensityIQR),
                    Convert.ToString(MS1Count)};

            streamWriter.Write(string.Join("\t", phraseToWrite));
            streamWriter.Close();
            CheckColumnNumber(ComprehensiveFile, 11);
        }

        public void MakeiRTmetricsFile(Run run)
        {
            CreateOutputDirectory(inputFileInclPath);
            string filename = dateTime + "_iRTMetrics_" + fileName + ".tsv";
            StreamWriter streamWriter = new StreamWriter(filename);
            streamWriter.Write("Filename\tiRTPeptideMz\tRetentionTime\tPeakwidth\tTailingFactor\n");

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
            string[] filePaths = { originalFilePath, "SwaMe_results", Path.GetFileNameWithoutExtension(inputFileInclPath), dateTime };
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


