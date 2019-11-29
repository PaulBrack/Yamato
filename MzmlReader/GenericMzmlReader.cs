using NLog;
using System;
using System.Xml;
using System.Linq;
using System.Globalization;
using Ionic.Zlib;
using System.IO;

namespace MzmlParser
{
    public abstract class GenericMzmlReader<TRun, TScan>
        where TRun : IGenericRun<TScan>, new()
        where TScan : IGenericScan, new()
    {
        public bool ParseBinaryData { get; set; } = true;
        public bool Threading { get; set; } = true;

        private int currentCycle = 0;
        bool hasAtLeastOneMS1 = false;
        private double previousTargetMz = 0;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private string SurveyScanReferenceableParamGroupId; //This is the referenceableparamgroupid for the survey scan

        protected abstract void ParseBase64DataFilling(TScan scan, TRun run);

        protected void ReadMzml(string path, TRun run)
        {
            ICouldRunCodeInParallel runner = Threading
                ? (ICouldRunCodeInParallel)new ThreadPoolScheduler()
                : new SingleThreadedScheduler();
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
                                ReadSpectrum(reader, run, runner);
                                break;
                            case "referenceableParamGroup":
                                if (string.IsNullOrEmpty(SurveyScanReferenceableParamGroupId))
                                    SurveyScanReferenceableParamGroupId = reader.GetAttribute("id");
                                break;
                        }
                    }
                }
            }
            runner.WaitForAll();
        }

        private void ReadSourceFileMetaData(XmlReader reader, TRun run)
        {
            bool cvParamsRead = false;
            run.SourceFileName = Path.GetFileNameWithoutExtension(reader.GetAttribute("name"));
            if (run.SourceFileName.ToLower().EndsWith(".wiff")) //in a .wiff.scan file you need to remove both extensions
            {
                run.SourceFileName = Path.GetFileNameWithoutExtension(run.SourceFileName);
            }
            run.SourceFileType = Path.GetExtension(reader.GetAttribute("name"));
            run.SourceFilePath = reader.GetAttribute("location");

            while (reader.Read() && !cvParamsRead)
            {
                if (reader.IsStartElement())
                {
                    if (reader.LocalName == "cvParam")
                    {
                        switch (reader.GetAttribute("accession"))
                        {
                            case "MS:1000569":
                                run.SourceFileChecksum = reader.GetAttribute("value");
                                run.FilePropertiesAccession = "MS:1000569";
                                break;
                            case "MS:1000747"://Optional for the conversion process, but included in mzQC
                                run.CompletionTime = reader.GetAttribute("value");
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

        private void ReadSpectrum(XmlReader reader, TRun run, ICouldRunCodeInParallel runner)
        {
            ScanAndTempProperties scan = new ScanAndTempProperties();

            //The cycle number is within a kvp string in the following format: "sample=1 period=1 cycle=1 experiment=1"
            //
            //This is a bit code-soup but I didn't want to spend more than one line on it and it should be robust enough not just to select on index
            //
            //This has only been tested on Sciex converted data
            //
            //Paul Brack 2019/04/03

            bool CycleInfoInID = false;

            if (run.SourceFileType.EndsWith("wiff", StringComparison.InvariantCultureIgnoreCase) || run.SourceFileType.ToUpper().EndsWith("scan", StringComparison.InvariantCultureIgnoreCase))
            {
                scan.Scan.Cycle = int.Parse(reader.GetAttribute("id").Split(' ').Single(x => x.Contains("cycle")).Split('=').Last());
                if (scan.Scan.Cycle != 0)//Some wiffs don't have that info so let's check
                {
                    CycleInfoInID = true;
                }
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
                                break;
                            case "MS:1000285":
                                scan.Scan.TotalIonCurrent = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                break;
                            case "MS:1000016":
                                scan.Scan.ScanStartTime = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                run.StartTime = Math.Min(run.StartTime, scan.Scan.ScanStartTime);
                                run.LastScanTime = Math.Max(run.LastScanTime, scan.Scan.ScanStartTime);//technically this is the starttime of the last scan not the completion time
                                break;

                            case "MS:1000829":
                                scan.Scan.IsolationWindowUpperOffset = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                break;
                            case "MS:1000828":
                                scan.Scan.IsolationWindowLowerOffset = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                break;
                            case "MS:1000827":
                                scan.Scan.IsolationWindowTargetMz = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                break;

                        }
                    }
                    else if (reader.LocalName == "binaryDataArray")
                    {
                        GetBinaryData(reader, scan);
                    }
                    else if (scan.Scan.MsLevel == null && reader.LocalName == "referenceableParamGroupRef")
                    {
                        if (reader.GetAttribute("ref") == SurveyScanReferenceableParamGroupId)
                            scan.Scan.MsLevel = 1;
                        else
                            scan.Scan.MsLevel = 2;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "spectrum")
                {

                    if (!CycleInfoInID)
                    {
                        if (scan.Scan.MsLevel == 1)
                        {
                            currentCycle++;
                            scan.Scan.Cycle = currentCycle;
                            hasAtLeastOneMS1 = true;
                        }
                        //if there is ScanAndTempProperties ms1:
                        else if (hasAtLeastOneMS1)
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
                                currentCycle++;
                                scan.Scan.Cycle = currentCycle;
                            }
                        }
                    }

                    previousTargetMz = scan.Scan.IsolationWindowTargetMz;

                    if (ParseBinaryData)
                        runner.Submit(state => ParseBase64Data(scan, run));
                    else
                        AddScanToRun(scan.Scan, run);
                    cvParamsRead = true;
                }
            }
        }

        private static void GetBinaryData(XmlReader reader, ScanAndTempProperties scan)
        {
            string base64 = string.Empty;
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

        private void ParseBase64Data(ScanAndTempProperties scan, TRun run)
        {
            float[] intensities = ExtractFloatArray(scan.Base64IntensityArray, scan.IntensityZlibCompressed, scan.IntensityBitLength);
            float[] mzs = ExtractFloatArray(scan.Base64MzArray, scan.MzZlibCompressed, scan.MzBitLength);

            if (intensities.Count() == 0)
            {
                intensities = ZeroArray();
                mzs = ZeroArray();
                logger.Info("Empty binary array for a MS{0} scan in cycle number: {0}. The empty scans have been filled with zero values.", scan.Scan.MsLevel, scan.Scan.Cycle);
                run.MissingScans++;
            }
            var spectrum = intensities.Select((x, i) => new SpectrumPoint() { Intensity = x, Mz = mzs[i], RetentionTime = (float)scan.Scan.ScanStartTime }).ToList();
            scan.Scan.Spectrum = spectrum;
            scan.Scan.IsolationWindowLowerBoundary = scan.Scan.IsolationWindowTargetMz - scan.Scan.IsolationWindowLowerOffset;
            scan.Scan.IsolationWindowUpperBoundary = scan.Scan.IsolationWindowTargetMz + scan.Scan.IsolationWindowUpperOffset;

            scan.Scan.Density = spectrum.Count();
            scan.Scan.BasePeakIntensity = intensities.Max();
            scan.Scan.BasePeakMz = mzs[Array.IndexOf(intensities, intensities.Max())];
            AddScanToRun(scan.Scan, run);

            ParseBase64DataFilling(scan.Scan, run);
        }

        private static float[] ExtractFloatArray(string Base64Array, bool IsZlibCompressed, int bits)
        {
            byte[] bytes = Convert.FromBase64String(Base64Array);

            if (IsZlibCompressed)
                bytes = ZlibStream.UncompressBuffer(bytes);

            if (bits == 32)
                return GetFloats(bytes);
            else if (bits == 64)
                return GetFloatsFromDoubles(bytes);
            else
                throw new ArgumentOutOfRangeException("bits", "Numbers must be 32 or 64 bits");
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

        private static void AddScanToRun(TScan scan, TRun run)
        {
            scan.IsolationWindowLowerBoundary = scan.IsolationWindowTargetMz - scan.IsolationWindowLowerOffset;
            scan.IsolationWindowUpperBoundary = scan.IsolationWindowTargetMz + scan.IsolationWindowUpperOffset;

            if (scan.MsLevel == 1)
                run.Ms1Scans.Add(scan);
            else if (scan.MsLevel == 2)
                run.Ms2Scans.Add(scan);
            else
                throw new ArgumentOutOfRangeException("scan.MsLevel", "MS Level must be 1 or 2");
        }

        private static float[] ZeroArray()
        {
            return new float[5];
        }

        private class ScanAndTempProperties
        {
            public ScanAndTempProperties()
            {
                Scan = new TScan();
            }

            public TScan Scan { get; set; }
            public string Base64IntensityArray { get; set; }
            public string Base64MzArray { get; set; }
            public bool IntensityZlibCompressed { get; set; }
            public bool MzZlibCompressed { get; set; }
            public int IntensityBitLength { get; set; }
            public int MzBitLength { get; set; }
        }
    }
}
