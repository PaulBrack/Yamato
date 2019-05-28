using NLog;
using System;
using System.Xml;
using System.Linq;
using System.Globalization;
using System.Threading;
using Ionic.Zlib;

namespace MzmlParser
{
    public class MzmlReader
    {
        public MzmlReader()
        {
            ParseBinaryData = true;
            ExtractBasePeaks = true;
            Threading = true;
        }

        public bool ExtractBasePeaks { get; set; }
        public bool ParseBinaryData { get; set; }
        public bool Threading { get; set; }

        private const double BasePeakMinimumIntensity = 100;
        private const double massTolerance = 0.05;
        private const double rtTolerance = 2.5; //2.5 mins on either side
        public int currentCycle = 0;
        bool MS1 = false;
        double previousTargetMz = 0;

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static CountdownEvent cde = new CountdownEvent(1);
        private static readonly Object Lock = new Object();
        private string SurveyScanReferenceableParamGroupId; //This is the referenceableparamgroupid for the survey scan

        public Run LoadMzml(string path)
        {
            Run run = new Run();
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
                                ReadSpectrum(reader, run);
                                break;
                            case "referenceableParamGroup":
                                if (String.IsNullOrEmpty(SurveyScanReferenceableParamGroupId))
                                    SurveyScanReferenceableParamGroupId = reader.GetAttribute("id");
                                break;
                        }
                    }
                }
            }
            cde.Signal();
            cde.Wait();
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

        public void ReadSpectrum(XmlReader reader, Run run)
        {
            ScanAndTempProperties scan = new ScanAndTempProperties();

            //The cycle number is within a kvp string in the following format: "sample=1 period=1 cycle=1 experiment=1"
            //
            //This is a bit code-soup but I didn't want to spend more than one line on it and it should be robust enough not just to select on index
            //
            //This has only been tested on Sciex converted data
            //
            //Paul Brack 2019/04/03
            if (run.SourceFileName.ToUpper().EndsWith("WIFF"))
            {
                scan.Scan.Cycle = int.Parse(reader.GetAttribute("id").Split(' ').Single(x => x.Contains("cycle")).Split('=').Last());
            }

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
                                    if (run.SourceFileName.ToUpper().EndsWith("RAW"))
                                    {
                                        if (scan.Scan.MsLevel == 1)
                                        {
                                            currentCycle++;
                                            scan.Scan.Cycle = currentCycle;
                                            MS1 = true;
                                        }
                                        //if there is ScanAndTempProperties ms1:
                                        else if (MS1)
                                        {
                                            scan.Scan.Cycle = currentCycle;
                                        }
                                        //if there is no ms1:
                                        else
                                        {
                                            if (previousTargetMz < scan.Scan.IsolationWindowTargetMz)
                                            {
                                                scan.Scan.Cycle = currentCycle;
                                            }
                                            else
                                            {
                                                currentCycle += currentCycle;
                                                scan.Scan.Cycle = currentCycle;
                                            }
                                        }
                                    }
                                    break;
                                case "MS:1000285":
                                    scan.Scan.TotalIonCurrent = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                    break;
                                case "MS:1000016":
                                    scan.Scan.ScanStartTime = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                    break;
                                case "MS:1000827":
                                    scan.Scan.IsolationWindowTargetMz = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                    break;
                                case "MS:1000829":
                                    scan.Scan.IsolationWindowUpperOffset = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                    break;
                                case "MS:1000828":
                                    scan.Scan.IsolationWindowLowerOffset = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                    break;
                            }
                        }
                        else if (reader.LocalName == "binaryDataArray")
                        {
                            GetBinaryData(reader, scan);
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
                        if (ParseBinaryData)
                        {
                            if (Threading)
                            {
                                cde.AddCount();
                                ThreadPool.QueueUserWorkItem(state => ParseBase64Data(scan, run, ExtractBasePeaks, Threading));
                            }
                            else
                                ParseBase64Data(scan, run, ExtractBasePeaks, Threading);
                        }
                        else
                        {
                            AddScanToRun(scan.Scan, run);
                        }
                        cvParamsRead = true;
                    }
                }
        }

        private static void GetBinaryData(XmlReader reader, ScanAndTempProperties scan)
        {
            string base64 = String.Empty;
            int bits = 0;
            bool intensityArray = false;
            bool mzArray = false;
            bool binaryDataArrayRead = false;
            bool IsZlibCompressed = false;

            while (reader.Read() && binaryDataArrayRead == false)
            {
                if (reader.IsStartElement())
                {
                    if (reader.LocalName == "cvParam")
                    {
                        switch (reader.GetAttribute("accession"))
                        {
                            case "MS:1000574":
                                if (reader.GetAttribute("name") == "zlib compression")
                                    IsZlibCompressed = true;
                                break;
                            case "MS:1000521":
                                //32 bit float
                                bits = 32;
                                break;
                            case "MS:1000523":
                                //64 bit float
                                bits = 64;
                                break;
                            case "MS:1000515":
                                //intensity array
                                intensityArray = true;
                                break;
                            case "MS:1000514":
                                //mz array
                                mzArray = true;
                                break;
                        }
                    }
                    else if (reader.LocalName == "binary")
                    {
                        base64 = reader.ReadElementContentAsString();
                    }

                }
                if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "binaryDataArray")
                {
                    binaryDataArrayRead = true;
                    if (intensityArray)
                    {
                        scan.Base64IntensityArray = base64;
                        scan.IntensityBitLength = bits;
                        scan.IntensityZlibCompressed = IsZlibCompressed;
                    }
                    else if (mzArray)
                    {
                        scan.Base64MzArray = base64;
                        scan.MzBitLength = bits;
                        scan.MzZlibCompressed = IsZlibCompressed;
                    }
                }
            }
        }

        private static void ParseBase64Data(ScanAndTempProperties scan, Run run, bool extractBasePeaks, bool threading)
        {
            float[] intensities = ExtractFloatArray(scan.Base64IntensityArray, scan.IntensityZlibCompressed, scan.IntensityBitLength);
            float[] mzs = ExtractFloatArray(scan.Base64MzArray, scan.MzZlibCompressed, scan.MzBitLength);
            var spectrum = intensities.Select((x, i) => new SpectrumPoint() { Intensity = x, Mz = mzs[i], RetentionTime = (float)scan.Scan.ScanStartTime }).ToList();

            //Want to potentially chuck 30GB of scan data into RAM? This is how you do it...
            //scan.Scan.Spectrum = spectrum;
            scan.Scan.Density = spectrum.Count();
            scan.Scan.BasePeakIntensity = intensities.Max();
            scan.Scan.BasePeakMz = mzs[Array.IndexOf(intensities, (int)scan.Scan.BasePeakIntensity)];
            AddScanToRun(scan.Scan, run);

            if (extractBasePeaks && scan.Scan.MsLevel == 2)
            {
                lock (Lock)
                {
                    //Create a new basepeak if no matching one exists
                    if (!run.BasePeaks.Exists(x => Math.Abs(x.Mz - scan.Scan.BasePeakMz) <= massTolerance) && scan.Scan.BasePeakIntensity >= BasePeakMinimumIntensity)
                    {
                        spectrum.Select(x => Math.Abs(x.Mz - scan.Scan.BasePeakMz) <= massTolerance);
                        BasePeak basePeak = new BasePeak()
                        {
                            Mz = scan.Scan.BasePeakMz,
                            RetentionTime = scan.Scan.ScanStartTime,
                            Intensity = scan.Scan.BasePeakIntensity,
                            Spectrum = spectrum.Where(x => Math.Abs(x.Mz - scan.Scan.BasePeakMz) <= massTolerance).OrderByDescending(x => x.Intensity).Take(1).ToList()
                        };
                        run.BasePeaks.Add(basePeak);
                    }
                    //Check to see if we have a basepeak we can add points to
                    else
                    {
                        foreach (BasePeak bp in run.BasePeaks.Where(x => Math.Abs(x.RetentionTime - scan.Scan.ScanStartTime) <= rtTolerance && Math.Abs(x.Mz - scan.Scan.BasePeakMz) <= massTolerance))
                        {
                            bp.Spectrum.Add(spectrum.Where(x => Math.Abs(x.Mz - bp.Mz) <= massTolerance).OrderByDescending(x => x.Intensity).First());
                        }
                    }
                }
            }

            if (threading)
                cde.Signal();
        }

        private static float[] ExtractFloatArray(string Base64Array, bool IsZlibCompressed, int bits)
        {
            float[] floats = new float[0];
            byte[] bytes = Convert.FromBase64String(Base64Array);

            if (IsZlibCompressed)
                bytes = ZlibStream.UncompressBuffer(bytes);

            if (bits == 32)
                floats = GetFloats(bytes);
            else if(bits == 64)
                floats = GetFloatsFromDoubles(bytes);
            else
                throw new ArgumentOutOfRangeException("bits", "Numbers must be 32 or 64 bits");

            return floats;
        }

        private static float[] GetFloats(byte[] bytes)
        {
            float[] floats = new float[bytes.Length / 4];
            for (int i = 0; i < floats.Length; i++)
                floats[i] = BitConverter.ToSingle(bytes, i * 4);
            return floats;
        }

        private static float[] GetFloatsFromDoubles(byte[] bytes)
        {
            float[] floats = new float[bytes.Length / 8];
            for (int i = 0; i < floats.Length; i++)
                floats[i] = (float)BitConverter.ToDouble(bytes, i * 8);
            return floats;
        }

        private static void AddScanToRun(Scan scan, Run run)
        {
            if (scan.MsLevel == 1)
                run.Ms1Scans.Add(scan);
            else if (scan.MsLevel == 2)
                run.Ms2Scans.Add(scan);
            else {
                throw new ArgumentOutOfRangeException("scan.MsLevel", "MS Level must be 1 or 2");
            }
        }

        private void FindMs2IsolationWindows(Run run)
        {
            run.IsolationWindows = run.Ms2Scans.Select(x => (x.IsolationWindowTargetMz - x.IsolationWindowLowerOffset, x.IsolationWindowTargetMz + x.IsolationWindowUpperOffset)).Distinct().ToList();
        }
    }
}
