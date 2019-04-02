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
               throw new System.ArgumentNullException(nameof(reader));
            
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
                            case "MS:1000514":
                                scan.MzArray = GetSucceedingBinaryDataArray(reader);
                                break;
                            case "MS:1000515":
                                scan.IntensityArray = GetSucceedingBinaryDataArray(reader);
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

        private static float[] GetSucceedingBinaryDataArray(XmlReader reader)
        {
            float[] floats = new float[] { };
            //const int bufferSize = 1024;
            //byte[] temp = new byte[] { };
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    if (reader.LocalName == "binary")
                    {
                        //Keep this streaming code for later 
                        //int readBytes = 0;
                        //byte[] buffer = new byte[bufferSize];

                        //while ((readBytes = reader.ReadElementContentAsBase64(buffer, 0, bufferSize)) > 0)
                        //{
                        //    byte[] ret = new byte[temp.Length + bufferSize];
                        //    Buffer.BlockCopy(temp, 0, ret, 0, temp.Length);
                        //    Buffer.BlockCopy(buffer, 0, ret, temp.Length, buffer.Length);
                        //    temp = ret;
                        //}

                        byte[] bytes = Convert.FromBase64String(reader.ReadElementContentAsString());

                        floats = new float[bytes.Length / 4];

                        for (int i = 0; i < floats.Length; i++)
                            floats[i] = BitConverter.ToSingle(bytes, i * 4);

                        float[] f = new float[] { floats.Max() };
                        return f;
                        
                    }
                }
            }
            return floats;
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
