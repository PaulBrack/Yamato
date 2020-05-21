#nullable enable

using NLog;
using System;
using System.Xml;
using System.Linq;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Collections.Generic;

namespace MzmlParser
{
    public class MzmlReader<TScan, TRun>
        where TScan: IScan
        where TRun: IRun<TScan>
    {
        public MzmlReader(IScanFactory<TScan> scanFactory, IRunFactory<TScan, TRun> runFactory)
        {
            ScanFactory = scanFactory;
            RunFactory = runFactory;
            Threading = true;
        }

        private IScanFactory<TScan> ScanFactory { get; }
        private IRunFactory<TScan, TRun> RunFactory { get; }

        public bool Threading { get; set; }
        public int MaxQueueSize { get; set; } = 1000;
        public int MaxThreads { get; set; } = 0;

        private readonly IList<IScanConsumer<TScan, TRun>> scanConsumers = new List<IScanConsumer<TScan, TRun>>();

        public CancellationToken CancellationToken { get; set; }

        private bool parseBinaryData;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly CountdownEvent cde = new CountdownEvent(1);
        private string? SurveyScanReferenceableParamGroupId; //This is the referenceableparamgroupid for the survey scan

        public void Register(IScanConsumer<TScan, TRun> scanConsumer)
        {
            scanConsumers.Add(scanConsumer);
        }

        /// <param name="path">The path to the input file to be opened, or null to read from stdin</param>
        public TRun LoadMzml(string path)
        {
            if (MaxThreads != 0)
                ThreadPool.SetMaxThreads(MaxThreads, MaxThreads);
            TRun run = RunFactory.CreateRun();
            run.StartTime = 100;
            run.LastScanTime = 0;
            parseBinaryData = scanConsumers.Any(scanConsumer => scanConsumer.RequiresBinaryData);

            ReadMzml(path, run);

            cde.Signal();
            while(cde.CurrentCount > 1)
            {
                if (CancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException("Reading MZML was cancelled");

                Thread.Sleep(500);
            }
            cde.Wait();
            return run;
        }

        /// <param name="path">The path to the input file to be opened, or null to read from stdin</param>
        private void ReadMzml(string path, TRun run)
        {
            using XmlReader reader = null == path
                ? XmlReader.Create(Console.In)
                : XmlReader.Create(path);
            while (reader.Read())
            {
                if (CancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException("Reading MZML was cancelled");

                if (reader.IsStartElement())
                {
                    switch (reader.LocalName)
                    {
                        case "run":
                            run.StartTimeStamp = reader.GetAttribute("startTimeStamp");
                            break;
                        case "sourceFile":
                            ReadSourceFileMetaData(reader, run);
                            break;
                        case "spectrum":
                            ReadSpectrum(reader, run);
                            break;
                        case "referenceableParamGroup":
                            if (string.IsNullOrEmpty(SurveyScanReferenceableParamGroupId))
                                SurveyScanReferenceableParamGroupId = reader.GetAttribute("id");
                            break;
                    }
                }
            }
        }

        public void ReadSourceFileMetaData(XmlReader reader, TRun run)
        {
            bool cvParamsRead = false;
            string rawFilename = Path.GetFileName(reader.GetAttribute("name"));
            string[] filenameWithoutExtention = rawFilename.Split(".", 2, StringSplitOptions.None);
            run.SourceFileNames.Add(filenameWithoutExtention[0]);
            run.SourceFileTypes.Add(Path.GetExtension(reader.GetAttribute("name")));
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
                                run.SourceFileChecksums.Add(reader.GetAttribute("value"));
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

        public void ReadSpectrum(XmlReader reader, TRun run)
        {
            ScanAndTempProperties<TScan> scanAndTempProperties = new ScanAndTempProperties<TScan>(ScanFactory);

            //The cycle number is within a kvp string in the following format: "sample=1 period=1 cycle=1 experiment=1"
            //
            //This is a bit code-soup but I didn't want to spend more than one line on it and it should be robust enough not just to select on index
            //
            //This has only been tested on Sciex converted data
            //
            //Paul Brack 2019/04/03

            bool CycleInfoInID = false;

            if (run.SourceFileTypes[0].EndsWith("wiff", StringComparison.InvariantCultureIgnoreCase) || run.SourceFileTypes[0].ToUpper().EndsWith("scan", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!string.IsNullOrEmpty(reader.GetAttribute("id")) && !string.IsNullOrEmpty(reader.GetAttribute("id").Split(' ').DefaultIfEmpty("0").Single(x => x.Contains("cycle"))))
                {
                    scanAndTempProperties.Scan.Cycle = int.Parse(reader.GetAttribute("id").Split(' ').DefaultIfEmpty("0").Single(x => x.Contains("cycle")).Split('=').Last());
                    if (scanAndTempProperties.Scan.Cycle != 0)//Some wiffs don't have that info so let's check
                        CycleInfoInID = true;
                }
            }

            bool cvParamsRead = false;
            double previousTargetMz = 0;
            int currentCycle = 0;
            bool hasAtLeastOneMS1 = false;
            while (reader.Read() && !cvParamsRead)
            {
                if (reader.IsStartElement())
                {
                    if (reader.LocalName == "cvParam")
                    {
                        switch (reader.GetAttribute("accession"))
                        {
                            case "MS:1000511":
                                scanAndTempProperties.Scan.MsLevel = int.Parse(reader.GetAttribute("value"));
                                break;
                            case "MS:1000285":
                                scanAndTempProperties.Scan.TotalIonCurrent = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                break;
                            case "MS:1000016":
                                scanAndTempProperties.Scan.ScanStartTime = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                run.StartTime = Math.Min(run.StartTime, scanAndTempProperties.Scan.ScanStartTime);
                                run.LastScanTime = Math.Max(run.LastScanTime, scanAndTempProperties.Scan.ScanStartTime);//technically this is the starttime of the last scan not the completion time
                                break;
                            case "MS:1000829":
                                scanAndTempProperties.Scan.IsolationWindowUpperOffset = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                break;
                            case "MS:1000828":
                                scanAndTempProperties.Scan.IsolationWindowLowerOffset = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                break;
                            case "MS:1000827":
                                scanAndTempProperties.Scan.IsolationWindowTargetMz = double.Parse(reader.GetAttribute("value"), CultureInfo.InvariantCulture);
                                break;
                        }
                    }
                    else if (reader.LocalName == "binaryDataArray")
                    {
                        GetBinaryData(reader, scanAndTempProperties);
                    }
                    if (scanAndTempProperties.Scan.MsLevel == null && reader.LocalName == "referenceableParamGroupRef")
                    {
                        scanAndTempProperties.Scan.MsLevel = reader.GetAttribute("ref") == SurveyScanReferenceableParamGroupId ? 1 : 2;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "spectrum")
                {
                    if (!CycleInfoInID)
                    {
                        if (scanAndTempProperties.Scan.MsLevel == 1)
                        {
                            currentCycle++;
                            scanAndTempProperties.Scan.Cycle = currentCycle;
                            hasAtLeastOneMS1 = true;
                        }
                        //if there is ScanAndTempProperties ms1:
                        else if (hasAtLeastOneMS1)
                        {
                            scanAndTempProperties.Scan.Cycle = currentCycle;
                        }
                        //if there is no ms1:
                        else
                        {
                            if (previousTargetMz >= scanAndTempProperties.Scan.IsolationWindowTargetMz)
                                currentCycle++;
                            scanAndTempProperties.Scan.Cycle = currentCycle;
                        }
                    }

                    previousTargetMz = scanAndTempProperties.Scan.IsolationWindowTargetMz;

                    if (parseBinaryData)
                    {
                        if (Threading)
                        {
                            cde.AddCount();
                            while (cde.CurrentCount > MaxQueueSize)
                                Thread.Sleep(1000);

                            ThreadPool.QueueUserWorkItem(state => ParseBase64Data(scanAndTempProperties, run));
                        }
                        else
                        {
                            ParseBase64Data(scanAndTempProperties, run);
                        }
                    }
                    else
                    {
                        AddScanToRun(scanAndTempProperties.Scan, run);
                    }
                    cvParamsRead = true;
                }
            }
        }

        private static void GetBinaryData(XmlReader reader, ScanAndTempProperties<TScan> scan)
        {
            string base64 = string.Empty;
            Bitness bitness = Bitness.NotSet;
            bool isIntensityArray = false;
            bool isMzArray = false;
            bool binaryDataArrayRead = false;
            Compression compression = Compression.Uncompressed;

            while (reader.Read() && !binaryDataArrayRead)
            {
                if (reader.IsStartElement())
                {
                    if ("cvParam".Equals(reader.LocalName))
                    {
                        switch (reader.GetAttribute("accession"))
                        {
                            case "MS:1000574":
                                if ("zlib compression".Equals(reader.GetAttribute("name")))
                                    compression = Compression.Zlib;
                                break;
                            case "MS:1000521":
                                //32 bit float
                                bitness = Bitness.IEEE754FloatLittleEndian;
                                break;
                            case "MS:1000523":
                                //64 bit float
                                bitness = Bitness.IEEE754DoubleLittleEndian;
                                break;
                            case "MS:1000515":
                                //intensity array
                                isIntensityArray = true;
                                break;
                            case "MS:1000514":
                                //mz array
                                isMzArray = true;
                                break;
                        }
                    }
                    else if ("binary".Equals(reader.LocalName))
                    {
                        reader.ReadStartElement();
                        base64 = reader.ReadContentAsString();
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && "binaryDataArray".Equals(reader.LocalName))
                {
                    binaryDataArrayRead = true;
                    Base64StringAndDecodingHints base64StringAndDecodingHints = new Base64StringAndDecodingHints(base64, compression, bitness);
                    if (isIntensityArray)
                        scan.Intensities = base64StringAndDecodingHints;
                    else if (isMzArray)
                        scan.Mzs = base64StringAndDecodingHints;
                }
            }
        }

        private void ParseBase64Data(ScanAndTempProperties<TScan> scanAndTempProperties, TRun run)
        {
            float[]? intensities = parseBinaryData ? scanAndTempProperties.Intensities?.ExtractFloatArray() : null;
            float[]? mzs = parseBinaryData ? scanAndTempProperties.Mzs?.ExtractFloatArray() : null;
            foreach (var scanConsumer in scanConsumers)
                scanConsumer.Notify(scanAndTempProperties.Scan, mzs, intensities, run);
            AddScanToRun(scanAndTempProperties.Scan, run);

            if (Threading)
                cde.Signal();
        }

        private void AddScanToRun(TScan scan, TRun run)
        {
            if (scan.MsLevel == 1)
            {
                lock (run.Ms1Scans)
                    run.Ms1Scans.Add(scan);
            }
            else if (scan.MsLevel == 2)
            {
                lock (run.Ms2Scans)
                {
                    run.Ms2Scans.Add(scan);
                    // Check made thread-safe Peter Crowther 2020-05-21.
                    if (run.Ms2Scans.Count % 10000 == 0)
                        logger.Info("{0} MS2 scans read", run.Ms2Scans.Count);
                }
            }
            else
                throw new ArgumentOutOfRangeException("scan.MsLevel", "MS Level must be 1 or 2");
        }
    }
}
