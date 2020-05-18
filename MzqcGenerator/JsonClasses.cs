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
            public string cvRef;
            public string accession;
            public string name;

            public FileFormat(string cvRef, string accession, string name)
            {
                this.cvRef = cvRef;
                this.accession = accession;
                this.name = name;
            }
        }


        public class FileProperties
        {
            public FileProperties(string cvRef, string accession, string name, string value)
            {
                this.cvRef = cvRef;
                this.accession = accession;
                this.name = name;
                this.value = value;
            }
            public string cvRef { get; set; }
            public string accession { get; set; }
            public string name { get; set; }
            public string value { get; set; }
        }
        public class InputFiles
        {
            public InputFiles(string location, string name, FileFormat fileFormat, List<FileProperties> fileProperties)
            {
                this.location = location;
                this.name = name;
                this.fileFormat = fileFormat;
                this.fileProperties = fileProperties;
            }
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
            public object analysisSettings { get; set; }
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

            public Unit(string cvRef, string accession, string name)
            {
                this.cvRef = cvRef;
                this.accession = accession;
                this.name = name;
            }
        }

        public class QualityParameters
        {
            public QualityParameters() { }

            public QualityParameters(string cvRef, string accession, string name, Unit unit, dynamic value)
            {
                this.cvRef = cvRef;
                this.accession = accession;
                this.name = name;
                this.unit = unit;
                this.value = value;
            }

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

        public class MzQC
        {
            public string version { get; set; }
            public List<RunQuality> runQuality { get; set; }
            public IDictionary<string, NUV> cv { get; set; }
        }

#pragma warning restore IDE1006 // Naming Styles
    }
}
