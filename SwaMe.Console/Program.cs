using CommandLine;
using System;
using System.Diagnostics;
using NLog;
using LibraryParser;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MzmlParser;
using NLog.Fluent;

namespace Yamato.Console
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
            {
                if(options.Verbose)
                    SetVerboseLogging();
                bool combine = options.Combine;
                string dateTime = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
                List<string> inputFiles = new List<string>();

                if (options.LoadFromDirectory != null && options.LoadFromDirectory == true)//multiple files
                {
                    var directoryPath = Path.GetDirectoryName(options.InputFile);
                    DirectoryInfo di = new DirectoryInfo(directoryPath);
                    Logger.Info("Attempting to load files with the extension .mzml in the following directory: {0}", directoryPath);
                    foreach (var file in di.GetFiles("*.mzml", SearchOption.TopDirectoryOnly))
                        inputFiles.Add(file.FullName);

                    if (inputFiles.Count == 0)
                    {
                        Logger.Error("Unable to locate any MZML files in {0} directory", directoryPath);
                        throw new FileNotFoundException();
                    }
                }
                else //single file
                    inputFiles.Add(options.InputFile);

                foreach (string inputFilePath in inputFiles)
                {
                    bool lastFile = false;//saving whether its the last file or not, so if we need to combine all the files in the end, we know when the end is.
                    string fileSpecificDirectory = DirectoryCreator.CreateOutputDirectory(inputFilePath, dateTime);
                    if (inputFilePath == inputFiles.Last()) lastFile = true;
                    Logger.Info("Loading file: {0}", inputFilePath);
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    int division;
                    if (options.Division < 100 && options.Division > 0)
                        division = options.Division;
                    else
                    {
                        Logger.Error("Number of divisions must be within the range 1 - 100. You have input: {0}", options.Division);
                        throw new ArgumentOutOfRangeException();
                    }
                    bool irt = !String.IsNullOrEmpty(options.IRTFile);

                    MzmlParser.MzmlReader mzmlParser = new MzmlParser.MzmlReader
                    {
                        ParseBinaryData = options.ParseBinaryData ?? true,
                        Threading = options.Threading ?? true,
                        MaxQueueSize = options.MaxQueueSize,
                        MaxThreads = options.MaxThreads
                    };

                    CheckFileIsReadableOrComplain(inputFilePath);

                    AnalysisSettings analysisSettings = new AnalysisSettings()
                    {
                        MassTolerance = options.MassTolerance,
                        RtTolerance = options.RtTolerance,
                        IrtMinIntensity = options.IrtMinIntensity,
                        IrtMinPeptides = options.IrtMinPeptides,
                        IrtMassTolerance = options.IrtMassTolerance,
                        CacheSpectraToDisk = options.Cache,
                        MinimumIntensity = options.MinimumIntensity,
                        RunEndTime = options.RunEndTime
                    };

                    if (!String.IsNullOrEmpty(options.IRTFile))
                    {
                        irt = true;
                        TraMLReader traMLReader = new TraMLReader();
                        analysisSettings.IrtLibrary = traMLReader.LoadLibrary(options.IRTFile);
                    }
                    MzmlParser.Run run = mzmlParser.LoadMzml(inputFilePath, analysisSettings);
                    AnalysisSettingsFileWriter Aw = new AnalysisSettingsFileWriter();
                    if (inputFiles.Count() > 1 && lastFile)//multiple files and this is the last
                    {
                        Aw.WriteASFile(run, dateTime, inputFiles);
                    }
                    else //only one file
                    {
                        Aw.WriteASFile(run, dateTime, inputFilePath);
                    }

                    Logger.Info("Generating metrics...", Convert.ToInt32(sw.Elapsed.TotalSeconds));
                    var swameMetrics = new SwaMe.MetricGenerator().GenerateMetrics(run, division, inputFilePath, irt, combine, lastFile, dateTime);
                    var progMetrics = new Prognosticator.MetricGenerator().GenerateMetrics(run);

                    var metrics = swameMetrics.Union(progMetrics).ToDictionary(k => k.Key, v => v.Value);
                    string[] mzQCName = { dateTime, Path.GetFileNameWithoutExtension(inputFilePath), "mzQC.json" };
                    Directory.SetCurrentDirectory(fileSpecificDirectory);
                    new MzqcGenerator.MzqcWriter().BuildMzqcAndWrite(string.Join("_", mzQCName), run, metrics, inputFilePath);
                    Logger.Info("Generated metrics in {0} seconds", Convert.ToInt32(sw.Elapsed.TotalSeconds));

                    if (analysisSettings.CacheSpectraToDisk)
                    {
                        Logger.Info("Deleting temp files...");
                        mzmlParser.DeleteTempFiles(run);
                    }
                    Logger.Info("Done!");

                }
            });
            }
            catch (Exception ex)
            {
                Logger.Error("An unexpected error occured:");
                Logger.Error(ex.Message);
                Logger.Error(ex.StackTrace);
                LogManager.Shutdown();
                Environment.Exit(1);
            }
            LogManager.Shutdown();
            Environment.Exit(0);
            
        }

        private static void CheckFileIsReadableOrComplain(string inputFilePath)
        {
            try
            {
                Stream stream = new FileStream(inputFilePath, FileMode.Open);
                stream.Close();
            }
            catch (IOException)
            {
                Logger.Error(String.Format("Unable to open the file: {0}.", inputFilePath));
                throw;
            }
        }

        private static void SetVerboseLogging()
        {
            Logger.Info("Verbose output selected: enabled logging for all levels");
            foreach (var rule in LogManager.Configuration.LoggingRules)
            {
                rule.EnableLoggingForLevels(LogLevel.Trace, LogLevel.Debug);
            }
            LogManager.ReconfigExistingLoggers();
        }


    }

    public class DirectoryCreator
    {
        public static string CreateOutputDirectory(string inputFileInclPath, string dateTime)
        {
            string originalFilePath = Path.GetDirectoryName(inputFileInclPath);
            string[] filePaths = { originalFilePath, "QC_Results", Path.GetFileNameWithoutExtension(inputFileInclPath), dateTime };
            string filePath = Path.Combine(filePaths);
            DirectoryInfo di = Directory.CreateDirectory(filePath);
            Directory.SetCurrentDirectory(filePath);
            return filePath;
        }
    }
        

    public class Options
    {
        [Option("dir", Required = false, HelpText = "Load from directory: if true, reads the directory path of the input file path and runs on all .mzml files in that directory")]
        public bool? LoadFromDirectory { get; set; }

        [Option('i', "inputfile", Required = true, HelpText = "Input file path.")]
        public string InputFile { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Enable verbose logging.")]
        public bool Verbose { get; set; } = false;

        [Option('d', "division", Required = false, HelpText = "Number of units the user would like to divide certain SwaMe metrics into.")]
        public int Division { get; set; } = 1;

        [Option('m', "masstolerance", Required = false, HelpText = "mass tolerance in daltons. This will be used to distinguish which peaks are part of the same chromatogram. Similarly with iRT peptide searching, this is the tolerance that will allow two values to be considered the same peak.")]
        public float MassTolerance { get; set; } = 0.05F;

        [Option('p', "parsebinarydata", Required = false, HelpText = "whether binary data will be parsed")]
        public bool? ParseBinaryData { get; set; } = true;

        [Option('t', "threading", Required = false, HelpText = "whether threading is used")]
        public bool? Threading { get; set; } = true;

        [Option('r', "irtFile", Required = false, HelpText = "iRT file path")]
        public String IRTFile { get; set; } = null;

        [Option("irttolerance", Required = false, HelpText = "iRT mass tolerance")]
        public double IrtMassTolerance { get; set; } = 0.005;

        [Option("irtminintensity", Required = false, HelpText = "iRT min intensity")]
        public double IrtMinIntensity { get; set; } = 0;

        [Option("irtminpeptides", Required = false, HelpText = "iRT min peptides")]
        public int IrtMinPeptides { get; set; } = 3;

        [Option("rttolerance", Required = false, HelpText = "RT tolerance")]
        public double RtTolerance { get; set; } = 2.5;

        [Option('c', "combineFiles", Required = false, HelpText = "Combine files at the end?")]
        public bool Combine { get; set; } = true;

        [Option('z', "cacheSpectraToDisk", Required = false, HelpText = "Cache spectra on read")]
        public bool Cache { get; set; } = false;

        [Option("minimumIntensity", Required = false, HelpText = "The minimum threshold of an intensity value to process")]
        public int MinimumIntensity { get; set; } = 100;

        [Option("maxQueueSize", Required = false, HelpText = "The maximum number of threads to queue. When the number is met, the parser will pause")]
        public int MaxQueueSize { get; set; } = 1000;

        [Option("maxThreads", Required = false, HelpText = "The maximum number of worker threads. Set as zero for default system max.")]
        public int MaxThreads { get; set; } = 0;

        [Option("runEndTime", Required = false, HelpText = "The time during the run to stop calculating metrics (e.g. when the wash begins)")]
        public int? RunEndTime { get; set; } = null;
    }
}

