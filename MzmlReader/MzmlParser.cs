using NLog;
using System;
using System.Xml;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace MzmlParser
{
    public class MzmlParser
    {
        private const float BasePeakMinimumIntensity = 100;
        private const float BasePeakMaximumDeltaRt = 2;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string SurveyScanReferenceableParamGroupId; //This is the referenceableparamgroupid for the survey scan
        public int numberOfTasks = 0;
        ManualResetEvent signal = new ManualResetEvent(false);
        

        public Run LoadMzml(string path)
        {
            Run run = new Run();
            using (CountdownEvent e = new CountdownEvent(1))
            {
                logger.Info("Loading file: {0}", path);
                using (XmlReader reader = XmlReader.Create(path))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.LocalName)
                            {
                                case "sourceFile":
                                    ReadSourceFileMetaData(reader, run);
                                    break;
                                case "spectrum":
                                    ReadSpectrum(reader, run, e);
                                    break;
                                case "referenceableParamGroup":
                                    if (String.IsNullOrEmpty(SurveyScanReferenceableParamGroupId))
                                        SurveyScanReferenceableParamGroupId = reader.GetAttribute("id");
                                    break;
                            }
                        }
                    }
                }
                e.Signal();
                e.Wait();
            }
            FindMs2IsolationWindows(run);
            
            return run;
        }

        public void ReadSourceFileMetaData(XmlReader reader, Run run)
        {
            bool cvParamsRead = false;
            run.SourceFileName = reader.GetAttribute("name");
            run.SourceFilePath = reader.GetAttribute("location");

            while (reader.Read() && !cvParamsRead)
            {
                if (reader.IsStartElement())
                {
                    if (reader.LocalName == "cvParam")
                    {
                        switch (reader.GetAttribute("accession"))
                        {
                            case "MS:1000562":
                                run.SourceFileType = reader.GetAttribute("name");
                                break;
                            case "MS:1000569":
                                run.SourceFileChecksum = reader.GetAttribute("value");
                                break;
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "sourceFile")
                {
                    cvParamsRead = true;
                }
            }
        }

        public void ReadSpectrum(XmlReader reader, Run run, CountdownEvent e)
        {
            ScanAndTempProperties scan = new ScanAndTempProperties();

            //The cycle number is within a kvp string in the following format: "sample=1 period=1 cycle=1 experiment=1"
            //
            //This is a bit code-soup but I didn't want to spend more than one line on it and it should be robust enough not just to select on index
            //
            //This has only been tested on Sciex converted data
            //
            //Paul Brack 2019/04/03
            scan.Scan.Cycle = int.Parse(reader.GetAttribute("id").Split(' ').Single(x => x.Contains("cycle")).Split('=').Last());

            bool cvParamsRead = false;
            while (reader.Read() && !cvParamsRead)
            {
                if (reader.IsStartElement())
                {
                    if (reader.LocalName == "cvParam")
                    {
                        switch (reader.GetAttribute("accession"))
                        {
                            case "MS:1000511":
                                scan.Scan.MsLevel = int.Parse(reader.GetAttribute("value"));
                                break;
                            case "MS:1000285":
                                scan.Scan.TotalIonCurrent = double.Parse(reader.GetAttribute("value"));
                                break;
                            case "MS:1000016":
                                scan.Scan.ScanStartTime = double.Parse(reader.GetAttribute("value"));
                                break;
                            case "MS:1000827":
                                scan.Scan.IsolationWindowTargetMz = double.Parse(reader.GetAttribute("value"));
                                break;
                            case "MS:1000829":
                                scan.Scan.IsolationWindowUpperOffset = double.Parse(reader.GetAttribute("value"));
                                break;
                            case "MS:1000828":
                                scan.Scan.IsolationWindowLowerOffset = double.Parse(reader.GetAttribute("value"));
                                break;
                            case "MS:1000514":
                                scan.Base64MzArray = GetSucceedingBinaryDataArray(reader);
                                break;
                            case "MS:1000515":
                                scan.Base64IntensityArray = GetSucceedingBinaryDataArray(reader);
                                break;
                        }
                    }
                    if (scan.Scan.MsLevel == null && reader.LocalName == "referenceableParamGroupRef")
                    {
                        if (reader.GetAttribute("ref") == SurveyScanReferenceableParamGroupId)
                            scan.Scan.MsLevel = 1;
                        else
                            scan.Scan.MsLevel = 2;
                    }
                    
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "spectrum")
                {
                    e.AddCount();
                    
                    // Put processor intensive work in a thread so we can get on with I/O
                    ThreadPool.QueueUserWorkItem(state => ParseBase64Data(scan, run, ref e));
                    cvParamsRead = true;
                }
            }
           
        }

        private static string GetSucceedingBinaryDataArray(XmlReader reader)
        {
            string base64 = String.Empty;
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    if (reader.LocalName == "binary")
                    {
                        return reader.ReadElementContentAsString();
                    }
                }
            }
            return base64;
        }

        private static void ParseBase64Data(ScanAndTempProperties scan, Run run, ref CountdownEvent e)
        {
  
            float[] intensities = ExtractFloatArray(scan.Base64IntensityArray);
            float[] mzs = ExtractFloatArray(scan.Base64MzArray);

            scan.Scan.BasePeakIntensity = intensities.Max();
            scan.Scan.BasePeakMz = mzs[Array.IndexOf(intensities, (int)scan.Scan.BasePeakIntensity)];
            AddScanToRun(scan.Scan, run);
            e.Signal();
        }

        private static float[] ExtractFloatArray(string Base64Array)
        {
            byte[] bytes = Convert.FromBase64String(Base64Array);
            float[] floats = new float[bytes.Length / 4];

            for (int i = 0; i < floats.Length; i++)
                floats[i] = BitConverter.ToSingle(bytes, i * 4);
            return floats;
        }

        private static void AddScanToRun(Scan scan, Run run)
        {
            if (scan.MsLevel == 1)
                run.Ms1Scans.Add(scan);
            else if (scan.MsLevel == 2)
                run.Ms2Scans.Add(scan);
            else { }
        }

        private void FindMs2IsolationWindows(Run run)
        {
            run.IsolationWindows = run.Ms2Scans.Select(x => (x.IsolationWindowTargetMz - x.IsolationWindowLowerOffset, x.IsolationWindowTargetMz + x.IsolationWindowUpperOffset)).Distinct().ToList();
        }
    }
}
