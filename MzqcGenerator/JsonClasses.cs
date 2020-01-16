using System.Collections.Generic;

namespace MzqcGenerator
{
    public class JsonClasses
    {
        // Naming Styles - these names are reflected into the JSON so are
        // exempted from normal style guidelines
        #pragma warning disable IDE1006

        public class FileFormat
        {
            public string cvRef = "MS";
            public string accession = "MS:1000584";
            public string name = "mzML format";
        }
        public class FileProperties
        {

            public string cvRef { get; set; }
            public string accession { get; set; }
            public string name { get; set; }
            public string value { get; set; }
        }
        public class InputFiles
        {
            public string location { get; set; }
            public string name { get; set; }
            public FileFormat fileFormat { get; set; }
            public List<FileProperties> fileProperties { get; set; }
        }
        public class AnalysisSoftware
        {
            public string cvRef { get; set; }
            public string accession { get; set; }
            public string name { get; set; }
            public string version { get; set; }
            public string uri { get; set; }

        }
        public class MetaData
        {
            public List<InputFiles> inputFiles { get; set; }
            public List<AnalysisSoftware> analysisSoftware { get; set; }
        }
        public class Unit
        {
            public string cvRef { get; set; }
            public string accession { get; set; }
            public string name { get; set; }
        }
        public class QualityParameters
        {
            public string cvRef { get; set; }
            public string accession { get; set; }
            public string name { get; set; }
            public Unit unit { get; set; }
            public dynamic value { get; set; }
        }
        public class RunQuality
        {
            public MetaData metadata { get; set; }
            public QualityParameters[] qualityParameters { get; set; }
        }

        public class NUV //Name,URI,Version
        {
            public string name { get; set; }
            public string uri { get; set; }
            public string version { get; set; }
        }

        public class CV
        {
            public NUV QC { get; set; }
            public NUV MS { get; set; }
            public NUV UO { get; set; }
        }


        public class MzQC
        {
            public string version = "0.0.11";
            public List<RunQuality> runQuality { get; set; }
            public CV cv { get; set; }
        }

        #pragma warning restore IDE1006 // Naming Styles
    }
}
