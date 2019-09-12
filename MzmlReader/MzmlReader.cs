using NLog;
using System;
using System.Xml;
using System.Linq;
using System.Globalization;
using System.Threading;
using Ionic.Zlib;
using LibraryParser;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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
        private const double rtTolerance = 2.5; //2.5 mins on either side
        const double irtTolerance = 0.5;
        public int currentCycle = 0;
        bool MS1 = false;
        double previousTargetMz = 0;

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static CountdownEvent cde = new CountdownEvent(1);
        private static readonly Object Lock = new Object();
        private string SurveyScanReferenceableParamGroupId; //This is the referenceableparamgroupid for the survey scan




        public Run LoadMzml(string path, string iRTpath, double massTolerance)
        {
            Run run = new Run();
            logger.Info("Loading file: {0}", path);
            run.iRTpath = iRTpath;
            run.IRTPeaks = new List<IRTPeak>();
            if (run.iRTpath != null)
            {
                lock (Lock)
                {
                    Library irtLibrary = new Library();
                    if (run.iRTpath.ToLower().Contains("traml"))
                    {
                       TraMLReader traMLReader = new TraMLReader();
                        irtLibrary = traMLReader.LoadLibrary(run.iRTpath);
                        {
                            run.IRTPeaks = new List<IRTPeak>();
                            for (int iii = 0; iii < irtLibrary.PeptideList.Count; iii++)
                            {
                                IRTPeak peak = new IRTPeak();
                                peak.AssociatedTransitions = new List<Library.Transition>();
                                peak.Spectrum = new List<SpectrumPoint>();
                                peak.TransitionRTs = new List<double>();
                                peak.PossPeaks = new List<PossiblePeak>();

                                var temp = irtLibrary.PeptideList[iii];
                                peak.ExpectedRetentionTime = ((Library.Peptide)temp).RetentionTime;
                                string Sequence = ((Library.Peptide)temp).Sequence;
                                peak.Mz = (Sequence.Count(x => x == 'A') * 71.04 + Sequence.Count(x => x == 'H') * 137.06 + Sequence.Count(x => x == 'R') * 156.10 +
                                    Sequence.Count(x => x == 'K') * 128.09 + Sequence.Count(x => x == 'I') * 113.08 + Sequence.Count(x => x == 'F') * 147.07 +
                                    Sequence.Count(x => x == 'L') * 113.08 + Sequence.Count(x => x == 'W') * 186.08 + Sequence.Count(x => x == 'M') * 131.04 +
                                    Sequence.Count(x => x == 'P') * 97.05 + Sequence.Count(x => x == 'C') * 103.01 + Sequence.Count(x => x == 'N') * 114.04 +
                                    Sequence.Count(x => x == 'V') * 99.07 + Sequence.Count(x => x == 'G') * 57.02 + Sequence.Count(x => x == 'S') * 87.03 +
                                    Sequence.Count(x => x == 'Q') * 128.06 + Sequence.Count(x => x == 'Y') * 163.06 + Sequence.Count(x => x == 'D') * 115.03 +
                                    Sequence.Count(x => x == 'E') * 129.04 + Sequence.Count(x => x == 'T') * 101.05 + 18.02 + 2.017) / ((Library.Peptide)temp).ChargeState;

                                for (int transition = 0; transition < irtLibrary.TransitionList.Count; transition++)
                                {
                                    if (Math.Abs(((Library.Transition)irtLibrary.TransitionList[transition]).PrecursorMz - peak.Mz) < 0.02)//chose this value as the smallest difference between two biognosis peptides is this
                                    {
                                        peak.AssociatedTransitions.Add((Library.Transition)irtLibrary.TransitionList[transition]);
                                    }
                                }

                                run.IRTPeaks.Add(peak);
                                run.IRTPeaks = run.IRTPeaks.OrderBy(x => x.ExpectedRetentionTime).ToList();
                            }

                        }
                    }
                    else if (run.iRTpath.Contains("csv") || run.iRTpath.Contains("tsv"))
                    {
                        SVReader svReader = new SVReader();
                        irtLibrary = svReader.LoadLibrary(run.iRTpath);
                        run.IRTPeaks = new List<IRTPeak>();
                        for (int iii = 0; iii < irtLibrary.PeptideList.Count; iii++)
                        {
                            IRTPeak peak = new IRTPeak();
                            peak.Spectrum = new List<SpectrumPoint>();
                            peak.AssociatedTransitions = new List<Library.Transition>();
                            peak.TransitionRTs = new List<double>();
                            peak.PossPeaks = new List<PossiblePeak>();
                            var temp = irtLibrary.PeptideList[iii];
                            peak.Mz = Convert.ToDouble(((Library.Peptide)temp).Id);
                            for (int transition = 0; transition < irtLibrary.TransitionList.Count; transition++)
                            {
                                if (Math.Abs(((Library.Transition)irtLibrary.TransitionList[transition]).PrecursorMz - peak.Mz) < 0.02)//chose this value as the smallest difference between two biognosis peptides is this
                                {
                                    peak.AssociatedTransitions.Add((Library.Transition)irtLibrary.TransitionList[transition]);
                                }
                            }
                            peak.AssociatedTransitions = ((Library.Peptide)temp).AssociatedTransitions;
                            run.IRTPeaks.Add(peak);
                        }
                    }


                }
            }
            run.MissingScans = 0;

            try
            {
                using (Stream stream = new FileStream("MyFilename.txt", FileMode.Open))
                {
                    logger.Info("");
                }
            }
            catch (IOException)
            {
                logger.Info("This file is in use. Please close the application using it and try again.");
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
                                ReadSourceFileMetaData(reader, run, iRTpath);
                                break;
                            case "spectrum":
                                ReadSpectrum(reader, run, massTolerance);
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
    

        public void ReadSourceFileMetaData(XmlReader reader, Run run, string iRTpath)
        {
            bool cvParamsRead = false;
            run.SourceFileName = reader.GetAttribute("name");
            run.SourceFilePath = reader.GetAttribute("location");
            run.iRTpath = iRTpath;

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

        public void ReadSpectrum(XmlReader reader, Run run, double massTolerance)
        {
            ScanAndTempProperties scan = new ScanAndTempProperties();

            //The cycle number is within a kvp string in the following format: "sample=1 period=1 cycle=1 experiment=1"
            //
            //This is a bit code-soup but I didn't want to spend more than one line on it and it should be robust enough not just to select on index
            //
            //This has only been tested on Sciex converted data
            //
            //Paul Brack 2019/04/03
            if (run.SourceFileName.ToUpper().EndsWith("WIFF")|| run.SourceFileName.ToUpper().EndsWith("SCAN"))
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
                            ThreadPool.QueueUserWorkItem(state => ParseBase64Data(scan, run, ExtractBasePeaks, Threading, massTolerance));
                            if (run.iRTpath != null)
                            {
                                cde.AddCount();
                                ThreadPool.QueueUserWorkItem(state => Base64iRTSearch(scan, run, Threading, massTolerance));
                               
                            }
                        }
                        else
                        {
                            ParseBase64Data(scan, run, ExtractBasePeaks, Threading, massTolerance);
                            if (run.iRTpath != "none")
                            {
                                Base64iRTSearch(scan, run, Threading, massTolerance);
                            }
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

        private static void ParseBase64Data(ScanAndTempProperties scan, Run run, bool extractBasePeaks, bool threading, double massTolerance)
        {

            float[] intensities = ExtractFloatArray(scan.Base64IntensityArray, scan.IntensityZlibCompressed, scan.IntensityBitLength);

            float[] mzs = ExtractFloatArray(scan.Base64MzArray, scan.MzZlibCompressed, scan.MzBitLength);
            if (intensities.Count() == 0)
            {
                intensities = FillZeroArray(intensities);
                mzs = FillZeroArray(mzs);
                logger.Info("Empty binary array for a MS{0} scan in cycle number: {0}", scan.Scan.MsLevel, scan.Scan.Cycle);
                run.MissingScans++;
            }
            var spectrum = intensities.Select((x, i) => new SpectrumPoint() { Intensity = x, Mz = mzs[i], RetentionTime = (float)scan.Scan.ScanStartTime }).ToList();

            //Want to potentially chuck 30GB of scan data into RAM? This is how you do it...

            //scan.Scan.Spectrum = spectrum;

            scan.Scan.Density = spectrum.Count();
            scan.Scan.BasePeakIntensity = intensities.Max();
            scan.Scan.BasePeakMz = mzs[Array.IndexOf(intensities, intensities.Max())];
            AddScanToRun(scan.Scan, run);


            //Extract info for Basepeak chromatograms
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
                        foreach (BasePeak bp in run.BasePeaks.Where(x => Math.Abs(x.RetentionTime - scan.Scan.ScanStartTime) <= irtTolerance && Math.Abs(x.Mz - scan.Scan.BasePeakMz) <= massTolerance))
                        {
                            bp.Spectrum.Add(spectrum.Where(x => Math.Abs(x.Mz - bp.Mz) <= massTolerance).OrderByDescending(x => x.Intensity).First());
                        }
                    }
                }
            }

            //Extract info for iRT chromatograms
            if (run.iRTpath != null && scan.Scan.MsLevel == 1)
            {
                lock (Lock)
                {
                    foreach (IRTPeak ip in run.IRTPeaks)
                    {
                        List<SpectrumPoint> temp = spectrum.Where(x => Math.Abs(ip.Mz - x.Mz) <= irtTolerance).OrderByDescending(x => x.Intensity).Take(1).ToList();
                        if (temp.Count > 0)
                        {
                            if (ip.PossPeaks.Count() > 0)
                            {
                                bool found = false;
                                for (int ttt = 0; ttt < ip.PossPeaks.Count() - 1; ttt++)
                                {

                                    if (Math.Abs(ip.PossPeaks[ttt].BasePeak.RetentionTime - temp[0].RetentionTime) < irtTolerance)// if there is already a possPeak that it can fit into then add
                                    {
                                        found = true;

                                        if (ip.PossPeaks[ttt].BasePeak.Intensity < temp[0].Intensity)
                                        {
                                            //This peak is more intense and should be the basepeak of this peak
                                            ip.PossPeaks[ttt].BasePeak = temp[0];
                                        }
                                    }

                                }

                                if (!found)
                                { 
                                    PossiblePeak possPeak = new PossiblePeak();
                                    possPeak.Alltransitions = new List<List<SpectrumPoint>>();
                                    foreach (var at in ip.AssociatedTransitions)
                                    {
                                        List<SpectrumPoint> tempList = new List<SpectrumPoint>();
                                        possPeak.Alltransitions.Add(tempList);
                                    }
                                    possPeak.BasePeak = temp[0];
                                    ip.PossPeaks.Add(possPeak);
                                }
                            }
                            else {

                                PossiblePeak possPeak = new PossiblePeak();
                                possPeak.Alltransitions = new List<List<SpectrumPoint>>();
                                foreach (var at in ip.AssociatedTransitions)
                                {
                                    List<SpectrumPoint> tempList = new List<SpectrumPoint>();
                                    possPeak.Alltransitions.Add(tempList);
                                }
                                possPeak.BasePeak = temp[0];
                                ip.PossPeaks.Add(possPeak);
                            }
                           

                            
                        }

                    }

                }

            }

            //lets try to find all the spectra where at least two transitions occur and add their RT's to a list.We can then later compare this list to the iRTPeak.spectrum.RT's
            if (run.IRTPeaks != null && scan.Scan.MsLevel == 2)
            {

                lock (Lock)
                {
                    foreach (IRTPeak peak in run.IRTPeaks)
                    {
                        
                            if (peak.PossPeaks.Count() > 0)
                            {
                                 peak.PossPeaks = peak.PossPeaks.OrderByDescending(x => x.BasePeak.Intensity).ToList();
                                for (int iii = 0; iii < peak.PossPeaks.Count() - 1; iii++)
                                {

                                    if (Math.Abs(scan.Scan.ScanStartTime - peak.PossPeaks[iii].BasePeak.RetentionTime) < irtTolerance)
                                    {
                                    //find if transitions are present
                                    
                                        int TransitionsMatched = 0;
                                        for (int iterator = 0; iterator < peak.AssociatedTransitions.Count(); iterator++)
                                        {
                                            int temp = spectrum.Count(x => Math.Abs(x.Mz - peak.AssociatedTransitions[iterator].ProductMz) <= massTolerance);
                                            if (temp > 0)
                                            {
                                                TransitionsMatched++;
                                            }

                                        }

                                        if (TransitionsMatched == peak.AssociatedTransitions.Count())
                                        {
                                            //Add the spectrumpoints of transitions to the transitionSpectrum of that possible peak
                                            for (int iterator = 0; iterator < TransitionsMatched; iterator++)
                                            {
                                                peak.PossPeaks[iii].Alltransitions[iterator].Add(spectrum.Where(x => Math.Abs(x.Mz - peak.AssociatedTransitions[iterator].ProductMz) <= massTolerance).First());
                                            }
                                        }
                                    }
                                }
                            }
                        
                    }
                }
            }

            if (threading)
            {
                cde.Signal();
            }
        }


        private static void Base64iRTSearch(ScanAndTempProperties scan, Run run, bool threading, double massTolerance)
        {
           
            float[] intensities = ExtractFloatArray(scan.Base64IntensityArray, scan.IntensityZlibCompressed, scan.IntensityBitLength);

            float[] mzs = ExtractFloatArray(scan.Base64MzArray, scan.MzZlibCompressed, scan.MzBitLength);
            if (intensities.Count() == 0)
            {
                intensities = FillZeroArray(intensities);
                mzs = FillZeroArray(mzs);
            }
            var spectrum = intensities.Select((x, i) => new SpectrumPoint() { Intensity = x, Mz = mzs[i], RetentionTime = (float)scan.Scan.ScanStartTime }).ToList();

           

                //Now we loop through to add to the chromatogram
                if (run.IRTPeaks != null && scan.Scan.MsLevel == 1 && run.IRTPeaks.Count()>0)
            {
                    lock (Lock)
                    {
                    
                        foreach (IRTPeak ip in run.IRTPeaks)
                        {
                            if (Math.Abs(ip.RetentionTime - scan.Scan.ScanStartTime) <= irtTolerance)
                            {
                               List<SpectrumPoint> temp = spectrum.Where(x=> Math.Abs(ip.Mz - x.Mz) <= massTolerance).OrderByDescending(x => x.Intensity).Take(1).ToList();
                                if(temp.Count>0)ip.Spectrum.Add(temp[0]);
                            }
                        }



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
