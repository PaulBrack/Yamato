using System;
using System.Collections.Generic;
using System.Text;

namespace SwaMe
{
    public class JsonClasses
    {
        public class MzQC
        {
            public string version = "0.0.11";
            public RunQuality runQuality;
            public CV cv;

            public MzQC(RunQuality runQuality, CV cv) {
                this.runQuality = runQuality;
                this.cv = cv;
            }
        }

        public class RunQuality
        {
            public MetaData metaData;
            public QualityParameters[] qcparameters;

            public RunQuality(MetaData metaData, QualityParameters[] qcparameters)
            {
                this.metaData = metaData;
                this.qcparameters = qcparameters;
            }
        }
        

        public class MetaData
        {
            public InputFiles inputFiles;
            public AnalysisSoftware analysisSoftware;

            public MetaData(InputFiles inputFiles)
            {
                this.inputFiles = inputFiles;
                AnalysisSoftware aS = new AnalysisSoftware();
                this.analysisSoftware = aS;
            }
        }
       
        public class InputFiles
        {
            public string location;
            public string name;
            public FileFormat fileFormat;
            public FileProperties[] fileProperties;

            public InputFiles(string location, string name)
            {
                this.location = location;
                this.name = name;
            }


            public InputFiles(string location, string name, FileProperties[] fileProperties)
            {
                this.location = location;
                this.name = name;
                FileFormat fF = new FileFormat();
                this.fileProperties = fileProperties;
                this.fileFormat = fF;
            }

            
        }
        public class FileFormat
        {
            public string cvRef ;
            public string accession ;
            public string name;

            public FileFormat()
            {
                this.cvRef = "MS";
                this.accession = "MS:1000584";
                this.name = "mzML format";
            }
        }
        public class FileProperties
        {
            public string cvRef;
            public string accession;
            public string name;
            public string value;

            public FileProperties(string cvRef, string accession, string name, string value)
            {
                this.cvRef = cvRef;
                this.accession = accession;
                this.name = name;
                this.value = value;
            }
        }
        public class AnalysisSoftware
        {
            public string cvRef;
            public string accession;
            public string name;
            public string version;
            public string URI ;
            public AnalysisSoftware()
            {
                this.cvRef =  "MS";
                this.accession = "MS XXXXXXXXXX";
                this.name =  "SwaMe";
                this.version = "0.0.1";
                this.URI = "XXXXXXXXX";

            }
        }
        public class QualityParameters
        {
            public string cvRef;
            public string accession;
            public string name;
            public Unit unit;
            public dynamic value;

            public QualityParameters(string cvRef,string accession, string name, Unit unit, int value)
            {
                this.cvRef = cvRef;
                this.accession = accession;
                this.name = name;
                this.unit = unit;
                this.value = value;
            }

            public QualityParameters(string cvRef, string accession, string name, Unit unit, double value)
            {
                this.cvRef = cvRef;
                this.accession = accession;
                this.name = name;
                this.unit = unit;
                this.value = value;
            }
            public QualityParameters(string cvRef, string accession, string name, Unit unit, List<double> value)
            {
                this.cvRef = cvRef;
                this.accession = accession;
                this.name = name;
                this.unit = unit;
                this.value = value;
            }
            public QualityParameters(string cvRef, string accession, string name, Unit unit, List<int> value)
            {
                this.cvRef = cvRef;
                this.accession = accession;
                this.name = name;
                this.unit = unit;
                this.value = value;
            }
        }
        
        
        public class Unit
        {
            public string cvRef;
            public string accession;
            public string name;
            public Unit(string cvRef, string accession, string name)
            {
                this.cvRef = cvRef;
                this.accession = accession;
                this.name = name;
            }
        }

        public class CV
        {
            public NUV QC;
            public NUV MS;
            public NUV OU;

            public CV(NUV QC, NUV MS,NUV OU)
            {
                this.QC = QC;
                this.MS = MS;
                this.OU = OU;
            }
        }

        public class NUV //Name,URI,Version
        {
            public string name;
            public string URI;
            public string version;

            public NUV(string name, string URI, string version)
            {
                this.name = name;
                this.URI = URI;
                this.version = version;
            }

        }
       

    }
   
}
