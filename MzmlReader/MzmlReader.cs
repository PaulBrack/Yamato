using NLog;
using System;
using System.Xml;
using System.Linq;
using System.Globalization;
using System.Threading;
using Ionic.Zlib;
using System.IO;
using System.Collections.Generic;
using LibraryParser;
using System.Diagnostics;

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
        public int currentCycle = 0;
        bool MS1 = false;
        public double previousTargetMz = 0;

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static CountdownEvent cde = new CountdownEvent(1);
        private static readonly Object Lock = new Object();
        private string SurveyScanReferenceableParamGroupId; //This is the referenceableparamgroupid for the survey scan

        public Run LoadMzml(string path, double massTolerance, bool storeScansInMemory, string irtPath)
        {
            Run run = new Run();
            run.AnalysisSettings.MassTolerance = massTolerance;
            run.AnalysisSettings.RtTolerance = 2.5; //2.5 mins on either side
            bool irt = false;
            if (ExtractBasePeaks)
            {
                Stopwatch sw2 = new Stopwatch();
                sw2.Start();
                run = GetBasePeaks(run, path);
                logger.Info("Collected Basepeaks in {0} seconds", Convert.ToInt32(sw2.Elapsed.TotalSeconds));
                sw2.Stop();
            }
            run.MissingScans = 0;
            if (!String.IsNullOrEmpty(irtPath))
            {
                irt = true;
                TraMLReader traMLReader = new TraMLReader();
                run.IrtLibrary = traMLReader.LoadLibrary(irtPath);
                
            }
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
                                ReadSpectrum(reader, run, storeScansInMemory, irt);
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

        public Run GetBasePeaks(Run run, string path)
        {
            using (XmlReader reader = XmlReader.Create(path))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        if (reader.LocalName == "spectrum")
                        {
                            QuickScan summaryScan = new QuickScan();
                            while (reader.Read())
                            {
                                if (reader.IsStartElement())
                                {
                                    if (reader.LocalName == "cvParam")
                                    {
                                        switch (reader.GetAttribute("accession"))
                                        {
                                            case "MS:1000511":
                                                summaryScan.Mslevel = int.Parse(reader.GetAttribute("value"));
                                                break;
                                            case "MS:1000016":
                                                summaryScan.ScanStartTime = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                                break;
                                        }

                                    }

                                    if (reader.LocalName == "binaryDataArray")
                                    {
                                        if (summaryScan.Mslevel == 2)
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
                                                        summaryScan.Base64IntensityArray = base64;
                                                        summaryScan.IntensityBitLength = bits;
                                                        summaryScan.IntensityZlibCompressed = IsZlibCompressed;
                                                    }
                                                    else if (mzArray)
                                                    {
                                                        summaryScan.Base64MzArray = base64;
                                                        summaryScan.MzBitLength = bits;
                                                        summaryScan.MzZlibCompressed = IsZlibCompressed;
                                                    }
                                                }
                                            }
                                        }

                                    }

                                }
                                if (summaryScan.Mslevel == 2 && reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "spectrum")
                                {
                                    AddInfoToBasePeaks(run, summaryScan);
                                }

                            }
                        }

                    }
                }
            }
            logger.Info("BasePeaks have been parsed.");
            return run;
        }

        public void AddInfoToBasePeaks(Run run, QuickScan qs)
        {
            float[] intensities = ExtractFloatArray(qs.Base64IntensityArray, qs.IntensityZlibCompressed, qs.IntensityBitLength);
            float[] mzs = ExtractFloatArray(qs.Base64MzArray, qs.MzZlibCompressed, qs.MzBitLength);
            float basepeakIntensity;
            if (intensities.Count() > 0)
            {
                basepeakIntensity = intensities.Max();
                int maxIndex = intensities.ToList().IndexOf(basepeakIntensity);
                double mz = mzs[maxIndex];

                if (run.BasePeaks.Count(x => Math.Abs(x.Mz - mz) <run.AnalysisSettings.MassTolerance) < 1)//If a basepeak with this mz doesn't exist yet add it
                {
                    BasePeak bp = new BasePeak(mz, qs.ScanStartTime, basepeakIntensity);
                    run.BasePeaks.Add(bp);
                }
                else //we do have a match, now lets figure out if they fall within the rtTolerance
                {
                    //find out which basepeak
                    foreach (BasePeak thisbp in run.BasePeaks.Where(x => Math.Abs(x.Mz - mz) < run.AnalysisSettings.MassTolerance))
                    {
                        bool found = false;
                        for (int rt = 0; rt < thisbp.BpkRTs.Count(); rt++)
                        {
                            if (Math.Abs(thisbp.BpkRTs[rt] - qs.ScanStartTime) < run.AnalysisSettings.RtTolerance)//this is part of a previous basepeak, or at least considered to be 
                            {
                                found = true;
                                break;
                            }

                        }
                        if (found == false)//This is considered to be a new instance
                        {
                            thisbp.BpkRTs.Add(qs.ScanStartTime);
                            thisbp.Intensities.Add(basepeakIntensity);
                        }
                    }
                }
            }
            else { basepeakIntensity = 0; }
            
        }

        public void ReadSourceFileMetaData(XmlReader reader, Run run)
        {
            bool cvParamsRead = false;
            run.SourceFileName = Path.GetFileNameWithoutExtension(reader.GetAttribute("name"));
            if (run.SourceFileName.ToLower().EndsWith(".wiff"))//in a .wiff.scan file you need to remove both extensions
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

        public void ReadSpectrum(XmlReader reader, Run run, bool storeScansInMemory, bool irt)
        {
            ScanAndTempProperties scan = new ScanAndTempProperties();

            //The cycle number is within a kvp string in the following format: "sample=1 period=1 cycle=1 experiment=1"
            //
            //This is a bit code-soup but I didn't want to spend more than one line on it and it should be robust enough not just to select on index
            //
            //This has only been tested on Sciex converted data
            //
            //Paul Brack 2019/04/03
            if (run.SourceFileType.EndsWith("wiff", StringComparison.InvariantCultureIgnoreCase) || run.SourceFileType.ToUpper().EndsWith("scan", StringComparison.InvariantCultureIgnoreCase))
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
                                if (run.SourceFileType.ToUpper().EndsWith("RAW"))
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
                            ThreadPool.QueueUserWorkItem(state => ParseBase64Data(scan, run, ExtractBasePeaks, Threading, storeScansInMemory, irt));
                        }
                        else
                        {
                            ParseBase64Data(scan, run, ExtractBasePeaks, Threading,  storeScansInMemory, irt);
                        }
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

        private static void ParseBase64Data(ScanAndTempProperties scan, Run run, bool extractBasePeaks, bool threading, bool storeScansInMemory, bool irt)
        {

            float[] intensities = ExtractFloatArray(scan.Base64IntensityArray, scan.IntensityZlibCompressed, scan.IntensityBitLength);
            float[] mzs = ExtractFloatArray(scan.Base64MzArray, scan.MzZlibCompressed, scan.MzBitLength);

            if (intensities.Count() == 0)
            {
                intensities = FillZeroArray(intensities);
                mzs = FillZeroArray(mzs);
                logger.Info("Empty binary array for a MS{0} scan in cycle number: {0}. The empty scans have been filled with zero values.", scan.Scan.MsLevel, scan.Scan.Cycle);
                run.MissingScans++;
            }
            var spectrum = intensities.Select((x, i) => new SpectrumPoint() { Intensity = x, Mz = mzs[i], RetentionTime = (float)scan.Scan.ScanStartTime }).ToList();
            scan.Scan.IsolationWindowLowerBoundary = scan.Scan.IsolationWindowTargetMz - scan.Scan.IsolationWindowLowerOffset;
            scan.Scan.IsolationWindowUpperBoundary = scan.Scan.IsolationWindowTargetMz + scan.Scan.IsolationWindowUpperOffset;
            //if (storeScansInMemory)
            //    scan.Scan.Spectrum = spectrum;


            scan.Scan.Density = spectrum.Count();
            scan.Scan.BasePeakIntensity = intensities.Max();
            scan.Scan.BasePeakMz = mzs[Array.IndexOf(intensities, intensities.Max())];
            AddScanToRun(scan.Scan, run);

            //Extract info for Basepeak chromatograms
            if (extractBasePeaks && scan.Scan.MsLevel == 2)
            {
                foreach (BasePeak bp in run.BasePeaks.Where(x => Math.Abs(x.Mz - scan.Scan.BasePeakMz) <= run.AnalysisSettings.MassTolerance))
                {
                    var temp = bp.BpkRTs.Where(x => Math.Abs(x - scan.Scan.ScanStartTime) < run.AnalysisSettings.RtTolerance);
                    if (temp.Count() >= 1)
                        bp.Spectrum.Add(spectrum.Where(x => Math.Abs(x.Mz - bp.Mz) <= run.AnalysisSettings.MassTolerance).OrderByDescending(x => x.Intensity).First());
                }
            }
            if (irt)
            {
                foreach (Library.Peptide peptide in run.IrtLibrary.PeptideList.Values)
                {
                    var irtIntensities = new List<float>();
                    bool foundAllTransitions = true;
                    foreach (Library.Transition t in run.IrtLibrary.TransitionList.Values.OfType<Library.Transition>().Where(x => x.PeptideId == peptide.Id))
                    {
                        var spectrumPoints = spectrum.Where(x => Math.Abs(x.Mz - t.ProductMz) < 0.02 && x.Intensity > 200);
                        if (spectrumPoints.Any())
                            irtIntensities.Add(spectrumPoints.Max(x => x.Intensity));
                        else
                        {
                            foundAllTransitions = false;
                            break;
                        }
                    }
                    if (foundAllTransitions)
                        run.IRTHits.Add(new CandidateHit() { PeptideSequence = peptide.Sequence, Intensities = irtIntensities, RetentionTime = scan.Scan.ScanStartTime });
                }
            }
            if (threading)
            {
                cde.Signal();
            }
        }

        private static float[] ExtractFloatArray(string Base64Array, bool IsZlibCompressed, int bits)
        {
            float[] floats = new float[0];
            byte[] bytes = Convert.FromBase64String(Base64Array);

            if (IsZlibCompressed)
                bytes = ZlibStream.UncompressBuffer(bytes);

            if (bits == 32)
                floats = GetFloats(bytes);
            else if (bits == 64)
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
            scan.IsolationWindowLowerBoundary = scan.IsolationWindowTargetMz - scan.IsolationWindowLowerOffset;
            scan.IsolationWindowUpperBoundary = scan.IsolationWindowTargetMz + scan.IsolationWindowUpperOffset;

            if (scan.MsLevel == 1)
                run.Ms1Scans.Add(scan);
            else if (scan.MsLevel == 2)
                run.Ms2Scans.Add(scan);
            else
            {
                throw new ArgumentOutOfRangeException("scan.MsLevel", "MS Level must be 1 or 2");
            }
        }

        private static float[] FillZeroArray(float[] array)
        {
            System.Array.Resize(ref array, 5);
            return array;
        }

        private void FindMs2IsolationWindows(Run run)
        {
            run.IsolationWindows = run.Ms2Scans.Select(x => (x.IsolationWindowTargetMz - x.IsolationWindowLowerOffset, x.IsolationWindowTargetMz + x.IsolationWindowUpperOffset)).Distinct().ToList();
        }
    }
}
