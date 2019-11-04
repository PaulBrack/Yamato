
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MzmlParser;
using MzqcGenerator;

namespace SwaMe
{
    class FileMaker
    {
        private int division;
        private int MS2Density50;
        private int MS2DensityIQR;
        private int MS1Count;
        private int MS2Count;
        private double cycleTimes50;
        private double cycleTimesIQR;
        private int totalMS2IonCount;
        private string inputFilePath;
        private double RTDuration;
        private double swathSizeDifference;
        private MzmlParser.Run run;
        private SwathGrouper.SwathMetrics swathMetrics;
        private RTGrouper.RTMetrics rtMetrics;

        public FileMaker(int division, string inputFilePath, MzmlParser.Run run, SwathGrouper.SwathMetrics swathMetrics, RTGrouper.RTMetrics rtMetrics, double RTDuration, double swathSizeDifference, int MS2Count, double cycleTimes50, double cycleTimesIQR, int totalMS2IonCount, int MS2Density50, int MS2DensityIQR, int MS1Count)
        {
            this.swathMetrics = swathMetrics;
            this.division = division;
            this.inputFilePath = inputFilePath;
            this.run = run;
            this.rtMetrics = rtMetrics;
            this.RTDuration = RTDuration;
            this.swathSizeDifference = swathSizeDifference;
            this.MS2Count = MS2Count;
            this.cycleTimes50 = cycleTimes50;
            this.cycleTimesIQR = cycleTimesIQR;
            this.totalMS2IonCount = totalMS2IonCount;
            this.MS2Density50 = MS2Density50;
            this.MS2DensityIQR = MS2DensityIQR;
            this.MS1Count = MS1Count;
        }

        public void MakeMetricsPerSwathFile(SwathGrouper.SwathMetrics swathMetrics,string inputFile)
        {
            //tsv
            StreamWriter streamWriter = new StreamWriter("MetricsBySwath_"+ run.SourceFileName + ".tsv");
            streamWriter.Write("Filename \t swathNumber \t scansPerSwath \t AveMzRange \t TICRatioOfSwath \t swDensityAverage \t swDensityIQR  \n");

            for (int i = 0; i < (swathMetrics.numOfSwathPerGroup.Count()-1); i++)
            {
                streamWriter.Write(run.SourceFileName);
                streamWriter.Write("\t");
                streamWriter.Write("Swathnumber");
                streamWriter.Write(i + 1);
                streamWriter.Write("\t");
                streamWriter.Write(swathMetrics.numOfSwathPerGroup.ElementAt(i));
                streamWriter.Write("\t");
                streamWriter.Write(swathMetrics.AveMzRange.ElementAt(i));
                streamWriter.Write("\t");
                streamWriter.Write(swathMetrics.TICRatio.ElementAt(i));
                streamWriter.Write("\t");
                streamWriter.Write(swathMetrics.swDensity50[i]);
                streamWriter.Write("\t");
                streamWriter.Write(swathMetrics.swDensityIQR[i]);
                streamWriter.Write("\n");
            }
            streamWriter.Close();

        }
        public void MakeMetricsPerRTsegmentFile(RTGrouper.RTMetrics rtMetrics)
        {
            string metricsPerRTSegmentFile = "RTDividedMetrics_"+ run.SourceFileName+ ".tsv";
            StreamWriter streamWriter = new StreamWriter(metricsPerRTSegmentFile);
            streamWriter.Write("Filename\t RTsegment \t MS2Peakwidths \t PeakSymmetry \t MS2PeakCapacity \t MS2Peakprecision \t MS1PeakPrecision \t DeltaTICAverage \t DeltaTICIQR \t AveCycleTime \t AveMS2Density \t AveMS1Density \t MS2TICTotal \t MS1TICTotal");

            for (int segment = 0; segment < division; segment++)
            {
                //write streamWriter
                streamWriter.Write("\n");
                streamWriter.Write(run.SourceFileName);
                streamWriter.Write("\t");
                streamWriter.Write("RTsegment");
                streamWriter.Write(segment);
                streamWriter.Write(" \t ");
                streamWriter.Write(rtMetrics.Peakwidths.ElementAt(segment).ToString());
                streamWriter.Write(" \t ");
                streamWriter.Write(rtMetrics.PeakSymmetry.ElementAt(segment).ToString());
                streamWriter.Write(" \t ");
                streamWriter.Write(rtMetrics.PeakCapacity.ElementAt(segment).ToString());
                streamWriter.Write(" \t ");
                streamWriter.Write(rtMetrics.PeakPrecision.ElementAt(segment).ToString());
                streamWriter.Write("\t");
                streamWriter.Write(rtMetrics.MS1PeakPrecision.ElementAt(segment).ToString());
                streamWriter.Write("\t");
                streamWriter.Write(rtMetrics.TicChange50List.ElementAt(segment));
                streamWriter.Write(" \t ");
                streamWriter.Write(rtMetrics.TicChangeIqrList.ElementAt(segment));
                streamWriter.Write(" \t ");
                streamWriter.Write(rtMetrics.CycleTime.ElementAt(segment));
                streamWriter.Write(" \t ");
                streamWriter.Write(rtMetrics.MS2Density.ElementAt(segment));
                streamWriter.Write(" \t ");
                streamWriter.Write(rtMetrics.MS1Density.ElementAt(segment));
                streamWriter.Write(" \t ");
                streamWriter.Write(rtMetrics.MS2TicTotal.ElementAt(segment));
                streamWriter.Write(" \t ");
                streamWriter.Write(rtMetrics.MS1TicTotal.ElementAt(segment));
                streamWriter.Write(" \t ");
            }
            streamWriter.Close();
        }
        public void MakeUndividedMetricsFile()
        {
            string undividedFile = "undividedMetrics_" + run.SourceFileName + ".tsv";
            StreamWriter streamWriter = new StreamWriter(undividedFile);
            streamWriter.Write("Filename \t MissingScans\t RTDuration \t swathSizeDifference \t  MS2Count \t swathsPerCycle \t CycleTimes50 \t CycleTimesIQR \t totalMS2IonCount \t MS2Density50 \t MS2DensityIQR \t MS1Count");
            streamWriter.Write("\n");
            streamWriter.Write(run.SourceFileName);
            streamWriter.Write("\t");
            streamWriter.Write(run.MissingScans);
            streamWriter.Write("\t");
            streamWriter.Write(RTDuration);
            streamWriter.Write("\t");
            streamWriter.Write(swathSizeDifference);
            streamWriter.Write("\t");
            streamWriter.Write(MS2Count);
            streamWriter.Write("\t");
            streamWriter.Write(swathMetrics.maxswath);
            streamWriter.Write("\t");
            streamWriter.Write(cycleTimes50);
            streamWriter.Write("\t");
            streamWriter.Write(cycleTimesIQR);
            streamWriter.Write("\t");
            streamWriter.Write(totalMS2IonCount);
            streamWriter.Write("\t");
            streamWriter.Write(MS2Density50);
            streamWriter.Write("\t");
            streamWriter.Write(MS2DensityIQR);
            streamWriter.Write("\t");
            streamWriter.Write(MS1Count);
            streamWriter.Close();
        }

        public void MakeiRTmetricsFile(Run run)
        {

            StreamWriter streamWriter = new StreamWriter("iRTMetrics_"  +run.SourceFileName + ".tsv");
            streamWriter.Write("Filename\t iRTPeptideMz \t RetentionTime\t Peakwidth \t PeakSymmetry");

            foreach (IRTPeak peak in run.IRTPeaks)
            {
                //write streamWriter
                streamWriter.Write("\n");
                streamWriter.Write(run.SourceFileName);
                streamWriter.Write("\t");
                streamWriter.Write(peak.Mz.ToString());
                streamWriter.Write(" \t ");
                streamWriter.Write(peak.RetentionTime.ToString());
                streamWriter.Write(" \t ");
                streamWriter.Write(peak.FWHM.ToString());
                streamWriter.Write(" \t ");
                streamWriter.Write(peak.Peaksym.ToString());
                streamWriter.Write(" \t ");
            }
            streamWriter.Close();
        }

        public void CreateAndSaveMzqc()
        {
            //Declare units:
            JsonClasses.Unit Count = new JsonClasses.Unit() { cvRef = "UO", accession = "UO:0000189", name = "count" };
            JsonClasses.Unit Second = new JsonClasses.Unit() { cvRef = "UO", accession = "UO:0000010", name = "second" };
            JsonClasses.Unit mZ = new JsonClasses.Unit() { cvRef = "MS", accession = "MS:1000040", name = "m/z" };
            JsonClasses.Unit Hertz = new JsonClasses.Unit() { cvRef = "UO", accession = "UO:0000106", name = "Hertz" };
            JsonClasses.Unit Ratio = new JsonClasses.Unit() { cvRef = "UO", accession = "UO:0010006", name = "ratio" };//Also doesn't exist, need to re-evaluate...
            JsonClasses.Unit Intensity = new JsonClasses.Unit() { cvRef = "MS", accession = "MS:1000042", name = "Peak Intensity" };

            //Start with the long part: adding all the metrics
            JsonClasses.QualityParameters[] qualityParameters = new JsonClasses.QualityParameters[25];
            qualityParameters[0] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:4000053", name = "Quameter metric: RT-Duration", unit = Second, value = RTDuration };
            qualityParameters[1] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXX", name = "SwaMe metric: swathSizeDifference", unit = mZ, value = swathSizeDifference };
            qualityParameters[2] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:4000060", name = "Quameter metric: MS2-Count", unit = Count, value = MS2Count };
            qualityParameters[3] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXX", name = "SwaMe metric: NumOfSwaths", unit = Count, value = swathMetrics.maxswath };
            qualityParameters[4] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXX", name = "SwaMe metric: CycleTimes50", unit = Hertz, value = cycleTimes50 };
            qualityParameters[5] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXX", name = "SwaMe metric: CycleTimesIQR", unit = Hertz, value = cycleTimesIQR };
            qualityParameters[6] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXX", name = "SwaMe metric: TotalMS2IonCount", unit = Count, value = totalMS2IonCount };
            qualityParameters[6] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXX", name = "SwaMe metric: MS2Density50", unit = Count, value = MS2Density50 };
            qualityParameters[7] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXX", name = "SwaMe metric: MS2DensityIQR", unit = Count, value = MS2DensityIQR };
            qualityParameters[8] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:4000059", name = "Quameter metric: MS1-Count", unit = Count, value = MS1Count };
            qualityParameters[9] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: scansPerSwathGroup", unit = Count, value = swathMetrics.numOfSwathPerGroup };
            qualityParameters[10] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: AveMzRange", unit = mZ, value = swathMetrics.AveMzRange };
            qualityParameters[11] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: TICRatioOfSwath", unit = Ratio, value = swathMetrics.TICRatio };
            qualityParameters[12] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: swDensity50", unit = Count, value = swathMetrics.swDensity50 };
            qualityParameters[13] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: swDensityIQR", unit = Count, value = swathMetrics.swDensityIQR };
            qualityParameters[14] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: Peakwidths", unit = Second, value = rtMetrics.Peakwidths };
            qualityParameters[15] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: PeakCapacity", unit = Count, value = rtMetrics.PeakCapacity };
            qualityParameters[16] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: PeakSymmetry", unit = Count, value = rtMetrics.PeakSymmetry };
            qualityParameters[17] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: MS2PeakPrecision", unit = mZ, value = rtMetrics.PeakPrecision };
            qualityParameters[17] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: MS1PeakPrecision", unit = mZ, value = rtMetrics.MS1PeakPrecision };
            qualityParameters[18] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: DeltaTICAverage", unit = Intensity, value = rtMetrics.TicChange50List };
            qualityParameters[19] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: DeltaTICIQR", unit = Intensity, value = rtMetrics.TicChangeIqrList };
            qualityParameters[20] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: AveScanTime", unit = Second, value = rtMetrics.CycleTime };
            qualityParameters[21] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: MS2Density", unit = Count, value = rtMetrics.MS2Density };
            qualityParameters[22] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: MS1Density", unit = Count, value = rtMetrics.MS1Density };
            qualityParameters[23] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: MS2TICTotal", unit = Count, value = rtMetrics.MS2TicTotal };
            qualityParameters[24] = new JsonClasses.QualityParameters() { cvRef = "QC", accession = "QC:XXXXXXXX", name = "SwaMe metric: MS1TICTotal", unit = Count, value = rtMetrics.MS1TicTotal };

            //Now for the other stuff
            JsonClasses.FileFormat fileFormat = new JsonClasses.FileFormat() { };
            List<JsonClasses.FileProperties> fileProperties = new List<JsonClasses.FileProperties>() { };
            JsonClasses.FileProperties fileProperty = new JsonClasses.FileProperties() {cvRef="MS", accession = run.FilePropertiesAccession, name = "SHA-1", value = run.SourceFileChecksum };
            JsonClasses.FileProperties completionTime = new JsonClasses.FileProperties() { cvRef = "MS", accession = "MS:1000747", name = "completion time", value = run.CompletionTime };
            fileProperties.Add(fileProperty);
            List<JsonClasses.InputFiles> inputFiles = new List<JsonClasses.InputFiles>();
            JsonClasses.InputFiles inputFile = new JsonClasses.InputFiles() { location = "file://" + inputFilePath, name = run.SourceFileName, fileFormat = fileFormat, fileProperties = fileProperties };
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
            string mzQCFile = @"metrics_" + run.SourceFileName+ ".json";
            new MzqcGenerator.MzqcWriter().WriteMzqc(mzQCFile, metrics);

          

            
        }
    }
}


            