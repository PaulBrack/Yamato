using Newtonsoft.Json;
using System.IO;

namespace MzqcGenerator
{
    public class MzqcWriter
    {
        public void WriteMzqc (string path, JsonClasses.MzQC metrics)
        {
            using (StreamWriter file = File.CreateText(path))
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
                JsonClasses.FileProperties fileProperty = new JsonClasses.FileProperties() { cvRef = "MS", accession = run.FilePropertiesAccession, name = "SHA-1", value = run.SourceFileChecksums[0] };
                JsonClasses.FileProperties completionTime = new JsonClasses.FileProperties() { cvRef = "MS", accession = "MS:1000747", name = "completion time", value = run.CompletionTime };
                fileProperties.Add(fileProperty);
                List<JsonClasses.InputFiles> inputFiles = new List<JsonClasses.InputFiles>();
                JsonClasses.InputFiles inputFile = new JsonClasses.InputFiles() { location = "file://" + inputFileInclPath, name = run.SourceFileNames[0], fileFormat = fileFormat, fileProperties = fileProperties };
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
                string mzQCFile = @"metrics_" + run.SourceFileNames[0] + ".json";
                new MzqcGenerator.MzqcWriter().WriteMzqc(mzQCFile, metrics);
            }
        }

    }

}
