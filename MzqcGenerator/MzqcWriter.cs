using MzmlParser;
using Newtonsoft.Json;
using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MzqcGenerator
{
    public class MzqcWriter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IDictionary<string, JsonClasses.QualityParameters> QualityParametersByAccession { get; } = new Dictionary<string, JsonClasses.QualityParameters>();

        public List<JsonClasses.Unit> Units { get; }

        public MzqcWriter()
        {
            JsonClasses.Unit Count = new JsonClasses.Unit("UO", "UO:0000189", "count");
            JsonClasses.Unit Second = new JsonClasses.Unit("UO", "UO:0000010", "second");
            JsonClasses.Unit Mz = new JsonClasses.Unit("MS", "MS:1000040", "m/z");
            JsonClasses.Unit Ratio = new JsonClasses.Unit("UO", "UO:0010006", "ratio"); // TODO: What's the difference between this and UO:0000190?
            JsonClasses.Unit Intensity = new JsonClasses.Unit("MS", "MS:1000042", "Peak Intensity");
            Units = new List<JsonClasses.Unit>() { Count, Second, Mz, Ratio, Intensity };

            List<JsonClasses.QualityParameters> qualityParameters = new List<JsonClasses.QualityParameters>
            {
                new JsonClasses.QualityParameters("QC", "QC:4000053", "Quameter metric: RT-Duration", Second, null),
                new JsonClasses.QualityParameters("QC", "QC:02", "SwaMe metric: swathSizeDifference", Mz, null),
                new JsonClasses.QualityParameters("QC", "QC:4000060", "Quameter metric: MS2-Count", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:04", "SwaMe metric: NumOfSwaths", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:05", "SwaMe metric: Target mz", Mz, null),
                new JsonClasses.QualityParameters("QC", "QC:06", "SwaMe metric: TotalMS2IonCount", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:07", "SwaMe metric: MS2Density50", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:08", "SwaMe metric: MS2DensityIQR", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:4000059", "Quameter metric: MS1-Count", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:09", "SwaMe metric: scansPerSwathGroup", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:10", "SwaMe metric: AvgMzRange", Mz, null),
                new JsonClasses.QualityParameters("QC", "QC:11", "SwaMe metric: SwathProportionOfTotalTIC", Ratio, null),
                new JsonClasses.QualityParameters("QC", "QC:12", "SwaMe metric: swDensity50", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:13", "SwaMe metric: swDensityIQR", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:14", "SwaMe metric: Peakwidths", Second, null),
                new JsonClasses.QualityParameters("QC", "QC:15", "SwaMe metric: PeakCapacity", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:24", "SwaMe metric: TailingFactor", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:XXXXXXXX", "SwaMe metric: MS2PeakPrecision", Mz, null),
                new JsonClasses.QualityParameters("QC", "QC:16", "SwaMe metric: MS1PeakPrecision", Mz, null),
                new JsonClasses.QualityParameters("QC", "QC:17", "SwaMe metric: DeltaTICAverage", Intensity, null),
                new JsonClasses.QualityParameters("QC", "QC:18", "SwaMe metric: DeltaTICIQR", Intensity, null),
                new JsonClasses.QualityParameters("QC", "QC:19", "SwaMe metric: AvgScanTime", Second, null),
                new JsonClasses.QualityParameters("QC", "QC:20", "SwaMe metric: MS2Density", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:21", "SwaMe metric: MS1Density", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:22", "SwaMe metric: MS2TICTotal", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:23", "SwaMe metric: MS1TICTotal", Count, null),

                new JsonClasses.QualityParameters("QC", "QC:99", "Prognosticator Metric: MS1TICQuartiles", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:98", "Prognosticator Metric: MS2TICQuartiles", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:97", "Prognosticator Metric: MS1TIC", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:96", "Prognosticator Metric: MS2TIC", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:95", "Prognosticator Metric: MS1BPC", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:94", "Prognosticator Metric: MS2BPC", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:93", "Prognosticator Metric: CombinedTIC", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:92", "Prognosticator Metric: MS1:MS2 ratio", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:91", "Prognosticator Metric: MS1 weighted median skew", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:90", "Prognosticator Metric: MS2 weighted median skew", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:89", "Prognosticator Metric: MeanIrtMassError", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:88", "Prognosticator Metric: MaxIrtMassError", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:87", "Prognosticator Metric: IrtPeptideFoundProportion", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:86", "Prognosticator Metric: IrtPeptides", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:85", "Prognosticator Metric: IrtPeptidesFound", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:84", "Prognosticator Metric: IrtSpread", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:83", "Prognosticator Metric: MS1TICQuartilesByRT", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:82", "Prognosticator Metric: MS2TICQuartilesByRT", Count, null),
                new JsonClasses.QualityParameters("QC", "QC:81", "Prognosticator Metric: IrtOrderedness", Count, null)
            };
            foreach (var qp in qualityParameters)
                QualityParametersByAccession.Add(qp.accession, qp);
        }

        public void BuildMzqcAndWrite(string outputFileName, Run run, Dictionary<string, dynamic> qcParams, string inputFileInclPath, object analysisSettings)
        {
            List<JsonClasses.QualityParameters> qualityParameters = new List<JsonClasses.QualityParameters>();
            foreach (var metric in qcParams)
            {
                // Note that this code smashes a value into the given QualityParameters; this will fail if the same one is used more than once, or the initialisation of those parameters is made static.
                if (QualityParametersByAccession.TryGetValue(metric.Key, out var matchingMetric))
                {
                    matchingMetric.value = metric.Value;
                    qualityParameters.Add(matchingMetric);
                }
                else
                    Logger.Warn("Term \"{0}\" was not found in the MZQC definition when attempting to write output. This term was ignored.", metric.Key);
            }
            //Now for the other stuff
            JsonClasses.FileFormat fileFormat = new JsonClasses.FileFormat() { };
            List<JsonClasses.FileProperties> fileProperties = new List<JsonClasses.FileProperties>();

            JsonClasses.FileProperties fileProperty = new JsonClasses.FileProperties("MS", run.FilePropertiesAccession, "SHA-1", run.SourceFileChecksums.First());
            JsonClasses.FileProperties completionTime = new JsonClasses.FileProperties("MS", "MS:1000747", "completion time", run.CompletionTime);
            fileProperties.Add(fileProperty);

            List<JsonClasses.InputFiles> inputFiles = new List<JsonClasses.InputFiles>();
            JsonClasses.InputFiles inputFile = new JsonClasses.InputFiles("file://" + inputFileInclPath, run.SourceFileNames.First(), fileFormat, fileProperties);
            inputFiles.Add(inputFile);

            List<JsonClasses.AnalysisSoftware> analysisSoftwarelist = new List<JsonClasses.AnalysisSoftware>();
            JsonClasses.AnalysisSoftware analysisSoftware = new JsonClasses.AnalysisSoftware() { 
                cvRef = "MS", 
                accession = "XXXXXXXXXXXXXX", 
                name = "SwaMe", 
                uri = "https://github.com/PaulBrack/Yamato/tree/master/Console", 
                version = typeof(MzqcWriter).Assembly.GetName().Version.ToString(), 
                analysisSettings = analysisSettings 
            };
            analysisSoftwarelist.Add(analysisSoftware);
            JsonClasses.MetaData metadata = new JsonClasses.MetaData() { inputFiles = inputFiles, analysisSoftware = analysisSoftwarelist };
            JsonClasses.RunQuality runQualitySingle = new JsonClasses.RunQuality() { metadata = metadata, qualityParameters = qualityParameters.ToArray() };
            List<JsonClasses.RunQuality> runQuality = new List<JsonClasses.RunQuality> { runQualitySingle };
            JsonClasses.NUV qualityControl = new JsonClasses.NUV() { name = "Proteomics Standards Initiative Quality Control Ontology", uri = "https://github.com/HUPO-PSI/mzQC/raw/master/cv/qc-cv.obo", version = "0.1.0" };
            JsonClasses.NUV massSpectrometry = new JsonClasses.NUV() { name = "Proteomics Standards Initiative Mass Spectrometry Ontology", uri = "https://raw.githubusercontent.com/HUPO-PSI/psi-ms-CV/master/psi-ms.obo", version = "4.1.7" };
            JsonClasses.NUV UnitOntology = new JsonClasses.NUV() { name = "Unit Ontology", uri = "http://ontologies.berkeleybop.org/uo.obo", version = "releases/2020-03-10" };
            JsonClasses.CV cV = new JsonClasses.CV() { QC = qualityControl, MS = massSpectrometry, UO = UnitOntology };
            JsonClasses.MzQC metrics = new JsonClasses.MzQC() { runQuality = runQuality, cv = cV };

            //Then save:
            WriteMzqc(outputFileName, metrics);
        }

        public void WriteMzqc(string path, JsonClasses.MzQC metrics)
        {
            using (StreamWriter file = File.CreateText(path))
            {
                file.Write("{ \"mzQC\":");
                JsonSerializer serializer = new JsonSerializer()
                {
                    NullValueHandling = NullValueHandling.Ignore
                };
                serializer.Serialize(file, metrics);
                file.Write("}");
            }
        }
    }
}
