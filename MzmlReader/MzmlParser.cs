using NLog;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Linq;


namespace MzmlParser
{
    public class MzmlParser
    {
       
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string lastScanRead = String.Empty;

        public Run LoadMzml(string path)
        {
            logger.Info("Loading file: {0}", path);

            Run run = new Run();
            using (XmlReader reader = XmlReader.Create(path))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.LocalName)
                        {
                            case "spectrum":
                                ReadSpectrum(reader, run);
                                break;
                        }
                    }
                }
            }
            return run;
        }

        public void ReadSpectrum(XmlReader reader, Run run)
        {
            if (reader == null)
            {
                throw new System.ArgumentNullException(nameof(reader));
            }

            Scan scan = new Scan();
            bool cvParamsRead = false;
            while(reader.Read() && !cvParamsRead)
            {
                if (reader.IsStartElement())
                {
                    if(reader.LocalName == "cvParam")
                    {
                        switch (reader.GetAttribute("accession"))
                        {
                            case "MS:1000511":
                                scan.MsLevel = int.Parse(reader.GetAttribute("value"));
                                break;
                            case "MS:1000505":
                                scan.BasePeakIntensity = double.Parse(reader.GetAttribute("value"));
                                break;
                            case "MS:1000504":
                                scan.BasePeakMz = double.Parse(reader.GetAttribute("value"));
                                break;
                            case "MS:1000285":
                                scan.TotalIonCurrent = double.Parse(reader.GetAttribute("value"));
                                break;
                            case "MS:1000016":
                                scan.ScanStartTime = double.Parse(reader.GetAttribute("value"));
                                break;
                            case "MS:1000827":
                                scan.IsolationWindowTargetMz = double.Parse(reader.GetAttribute("value"));
                                break;
                            case "MS:1000829":
                                scan.IsolationWindowUpperOffset = double.Parse(reader.GetAttribute("value"));
                                break;
                            case "MS:1000828":
                                scan.IsolationWindowLowerOffset = double.Parse(reader.GetAttribute("value"));
                                break;
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "spectrum") 
                {
                    AddScanToRun(scan, run);
                    cvParamsRead = true;
                }
            }
        }

        public void AddScanToRun(Scan scan, Run run)
        {
            if (scan.MsLevel == 1)
                run.Ms1Scans.Add(scan);
            if (scan.MsLevel == 2)
                run.Ms2Scans.Add(scan);
        }
    }
}
