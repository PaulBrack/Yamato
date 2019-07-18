namespace MzqcGenerator
{
    public class JsonClasses
    {
        public class MzQC
        {
            public string version = "0.0.11";
            public RunQuality runQuality { get; set; }
            public CV cv { get; set; }
        }

        public class RunQuality
        {
            public MetaData metaData { get; set; }
            public QualityParameters[] qualityParameters { get; set; }
        } 

        public class MetaData
        {
            public InputFiles inputFiles { get; set; }
            public AnalysisSoftware analysisSoftware = new AnalysisSoftware();
        }
       
        public class InputFiles
        {
            public string location { get; set; }
            public string name { get; set; }
            public FileFormat fileFormat { get; set; }
            public FileProperties[] fileProperties { get; set; }
        }

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
        public class AnalysisSoftware
        {
            public string cvRef = "MS";
            public string accession = "MS XXXXXXXXXX";
            public string name = "SwaMe";
            public string version = "0.0.1";
            public string URI = "XXXXXXXXX";
           
        }
        public class QualityParameters
        {
            public string cvRef { get; set; }
            public string accession { get; set; }
            public string name { get; set; }
            public Unit unit { get; set; }
            public dynamic value { get; set; }
        }

        public class Unit
        {
            public string cvRef { get; set; }
            public string accession { get; set; }
            public string name { get; set; }
        }

        public class CV
        {
            public NUV qc { get; set; }
            public NUV ms { get; set; }
            public NUV uo { get; set; }
        }

        public class NUV //Name,URI,Version
        {
            public string name { get; set; }
            public string URI { get; set; }
            public string version { get; set; }
        }
    }
}
