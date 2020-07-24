using CVLibrarian;
using Newtonsoft.Json;
using NLog;
using SwaMe.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MzqcGenerator
{
    public class MzqcWriter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private IDictionary<string, JsonClasses.QualityParameters> QualityParametersByAccession { get; }

        private readonly CVLibrary cvLibrary = new CVLibrary();
        private readonly ControlledVocabulary uo;
        private readonly ControlledVocabulary ms;
        private readonly ControlledVocabulary qc;

        public MzqcWriter()
        {
            uo = cvLibrary.RegisterControlledVocabulary(UnitOntology.Get(), "UO");
            ms = cvLibrary.RegisterControlledVocabulary(MSVocabulary.Get(), "MS");
            qc = cvLibrary.RegisterControlledVocabulary(MzQCVocabulary.Get(), "QC");

            JsonClasses.Unit Count = ToUnit(uo, "UO:0000189");
            JsonClasses.Unit Intensity = ToUnit(ms, "MS:1000042");
            JsonClasses.Unit Mz = ToUnit(ms, "MS:1000040");
            JsonClasses.Unit Ratio = ToUnit(uo, "UO:0010006"); // TODO: What's the difference between this and UO:0000190?
            JsonClasses.Unit Second = ToUnit(uo, "UO:0000010");
            JsonClasses.Unit DateTime = ToUnit(ms, "XXXXXXXXXXXXXX"); // Completely incorrect, but for want of a better unit in the meantime.

            QualityParametersByAccession = new JsonClasses.QualityParameters[]
            {
                ToQualityParameters(qc, "QC:4000053", Second),
                ToQualityParameters(qc, "QC:02", Mz),
                ToQualityParameters(qc, "QC:4000060", Count),
                ToQualityParameters(qc, "QC:04", Count),
                ToQualityParameters(qc, "QC:05", Mz),
                ToQualityParameters(qc, "QC:06", Count),
                ToQualityParameters(qc, "QC:07", Count),
                ToQualityParameters(qc, "QC:08", Count),
                ToQualityParameters(qc, "QC:4000059", Count),
                ToQualityParameters(qc, "QC:09", Count),
                ToQualityParameters(qc, "QC:10", Mz),
                ToQualityParameters(qc, "QC:11", Ratio),
                ToQualityParameters(qc, "QC:12", Count),
                ToQualityParameters(qc, "QC:13", Count),
                ToQualityParameters(qc, "QC:14", Second),
                ToQualityParameters(qc, "QC:15", Count),
                ToQualityParameters(qc, "QC:24", Count),
                ToQualityParameters(qc, "QC:XXXXXXXX", Mz),
                ToQualityParameters(qc, "QC:16", Mz),
                ToQualityParameters(qc, "QC:17", Intensity),
                ToQualityParameters(qc, "QC:18", Intensity),
                ToQualityParameters(qc, "QC:19", Second),
                ToQualityParameters(qc, "QC:20", Count),
                ToQualityParameters(qc, "QC:21", Count),
                ToQualityParameters(qc, "QC:22", Count),
                ToQualityParameters(qc, "QC:23", Count),
                ToQualityParameters(qc, "QC:25", DateTime), //So starttimestamp is one of the QuaMeter metrics that don't have a ref in mzQC dev working group yet. I am still completely unsure of what the unit will become, since neither UO nor MS has a datetime unit. It is the starttimestamp stated in the mzml.
                ToQualityParameters(qc, "QC:26", Count),

                ToQualityParameters(qc, "QC:99", Count),
                ToQualityParameters(qc, "QC:98", Count),
                ToQualityParameters(qc, "QC:97", Count),
                ToQualityParameters(qc, "QC:96", Count),
                ToQualityParameters(qc, "QC:95", Count),
                ToQualityParameters(qc, "QC:94", Count),
                ToQualityParameters(qc, "QC:93", Count),
                ToQualityParameters(qc, "QC:92", Count),
                ToQualityParameters(qc, "QC:91", Count),
                ToQualityParameters(qc, "QC:90", Count),
                ToQualityParameters(qc, "QC:89", Count),
                ToQualityParameters(qc, "QC:88", Count),
                ToQualityParameters(qc, "QC:87", Count),
                ToQualityParameters(qc, "QC:86", Count),
                ToQualityParameters(qc, "QC:85", Count),
                ToQualityParameters(qc, "QC:84", Count),
                ToQualityParameters(qc, "QC:83", Count),
                ToQualityParameters(qc, "QC:82", Count),
                ToQualityParameters(qc, "QC:81", Count)
            }
            .ToDictionary(qp => qp.accession);
        }

        /// <param name="outputFileName">The path to the output file to be opened, or null to send to stdout</param>
        public void BuildMzqcAndWrite(string outputFileName, Run<Scan> run, Dictionary<string, dynamic> qcParams, string inputFileInclPath, object analysisSettings)
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

            JsonClasses.InputFiles inputFile = new JsonClasses.InputFiles(
                "file://" + inputFileInclPath,
                run.SourceFileNames.First(),
                ToFileFormat(ms, "MS:1000584"),
                new List<JsonClasses.FileProperties>()
                {
                    // ToFileProperties("MS", "MS:1000747", run.CompletionTime), // TODO: Include this?
                    ToFileProperties(ms, run.FilePropertiesAccession, run.SourceFileChecksums.First())
                }
            );

            Term swaMe = ms.GetById("XXXXXXXXXXXXXX");
            JsonClasses.AnalysisSoftware analysisSoftware = new JsonClasses.AnalysisSoftware() { 
                cvRef = ms.PrimaryNamespace, 
                accession = swaMe.Id, 
                name = swaMe.Name, 
                uri = "https://github.com/PaulBrack/Yamato/tree/master/Console", 
                version = typeof(MzqcWriter).Assembly.GetName().Version.ToString(), 
                analysisSettings = analysisSettings 
            };

            JsonClasses.MzQC metrics = new JsonClasses.MzQC()
            {
                version = "0.0.11",
                runQuality = new List<JsonClasses.RunQuality>
                {
                    new JsonClasses.RunQuality()
                    {
                        metadata = new JsonClasses.MetaData()
                        {
                            inputFiles = new List<JsonClasses.InputFiles>() {
                                inputFile
                            },
                            analysisSoftware = new List<JsonClasses.AnalysisSoftware>()
                            {
                                analysisSoftware
                            }
                        },
                        qualityParameters = qualityParameters.ToArray()
                    }
                },
                cv = cvLibrary.ControlledVocabularies.ToDictionary(cv => cv.PrimaryNamespace, ToNUV)
            };

            //Then save:
            WriteMzqc(outputFileName, metrics);
        }

        /// <param name="path">The path to the output file to be opened, or null to send to stdout</param>
        public void WriteMzqc(string path, JsonClasses.MzQC metrics)
        {
            using TextWriter file = null == path ? Console.Out : File.CreateText(path);
            file.Write("{ \"mzQC\":");
            JsonSerializer serializer = new JsonSerializer()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            serializer.Serialize(file, metrics);
            file.Write("}");
        }

        private JsonClasses.Unit ToUnit(string cvRef, string id) => ToUnit(cvLibrary.GetById(cvRef), id);
        private JsonClasses.Unit ToUnit(string cvRef, Term term) => ToUnit(cvLibrary.GetById(cvRef), term);
        private JsonClasses.Unit ToUnit(ControlledVocabulary cv, string id) => ToUnit(cv, cv.GetById(id));
        private JsonClasses.Unit ToUnit(ControlledVocabulary cv, Term term) => new JsonClasses.Unit(cv.PrimaryNamespace, term.Id, term.Name);

        private JsonClasses.FileFormat ToFileFormat(string cvRef, string id) => ToFileFormat(cvLibrary.GetById(cvRef), id);
        private JsonClasses.FileFormat ToFileFormat(string cvRef, Term term) => ToFileFormat(cvLibrary.GetById(cvRef), term);
        private JsonClasses.FileFormat ToFileFormat(ControlledVocabulary cv, string id) => ToFileFormat(cv, cv.GetById(id));
        private JsonClasses.FileFormat ToFileFormat(ControlledVocabulary cv, Term term) => new JsonClasses.FileFormat(cv.PrimaryNamespace, term.Id, term.Name);

        private JsonClasses.FileProperties ToFileProperties(string cvRef, string id, string value) => ToFileProperties(cvLibrary.GetById(cvRef), id, value);
        private JsonClasses.FileProperties ToFileProperties(string cvRef, Term term, string value) => ToFileProperties(cvLibrary.GetById(cvRef), term, value);
        private JsonClasses.FileProperties ToFileProperties(ControlledVocabulary cv, string id, string value) => ToFileProperties(cv, cv.GetById(id), value);
        private JsonClasses.FileProperties ToFileProperties(ControlledVocabulary cv, Term term, string value) => new JsonClasses.FileProperties(cv.PrimaryNamespace, term.Id, term.Name, value);

        private JsonClasses.QualityParameters ToQualityParameters(string cvRef, string id, JsonClasses.Unit unit) => ToQualityParameters(cvLibrary.GetById(cvRef), id, unit);
        private JsonClasses.QualityParameters ToQualityParameters(string cvRef, Term term, JsonClasses.Unit unit) => ToQualityParameters(cvLibrary.GetById(cvRef), term, unit);
        private JsonClasses.QualityParameters ToQualityParameters(ControlledVocabulary cv, string id, JsonClasses.Unit unit) => ToQualityParameters(cv, cv.GetById(id), unit);
        private JsonClasses.QualityParameters ToQualityParameters(ControlledVocabulary cv, Term term, JsonClasses.Unit unit) => new JsonClasses.QualityParameters(cv.PrimaryNamespace, term.Id, term.Name, unit, null);

        private JsonClasses.NUV ToNUV(string cvRef) => ToNUV(cvLibrary.GetById(cvRef));
        private JsonClasses.NUV ToNUV(ControlledVocabulary controlledVocabulary) => new JsonClasses.NUV() { name = controlledVocabulary.Name, uri = controlledVocabulary.Url, version = controlledVocabulary.Version };
    }
}
