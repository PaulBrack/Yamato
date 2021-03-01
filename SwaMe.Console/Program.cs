using CommandLine;
using System;
using System.Diagnostics;
using NLog;
using LibraryParser;
using System.Linq;
using System.IO;
using SwaMe.Pipeline;
using System.Collections.Generic;

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
                // An inputfile of "-" on the command line denotes stdin, for which we use null internally.
                if ("-".Equals(options.InputFile, StringComparison.Ordinal))
                    options.InputFile = null;
                // An outputfile of "-" on the command line denotes stdout, for which we use null internally.
                if ("-".Equals(options.OutputFile, StringComparison.Ordinal))
                    options.OutputFile = null;

                if (options.Verbose)
                    SetVerboseLogging();

                if (null == options.InputFile)
                    Logger.Info("Loading file: {0}", options.InputFile);
                else
                    Logger.Info("Reading mzML from standard input");

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
                bool irt = !string.IsNullOrEmpty(options.IRTFile);

                // stdin (denoted by null) is always considered readable; anything else needs a check (#99)
                if (null != options.InputFile)
                    CheckFileIsReadableOrComplain(options.InputFile);

                AnalysisSettings analysisSettings = new AnalysisSettings()
                {
                    MassTolerance = options.MassTolerance,
                    RtTolerance = options.RtTolerance,
                    IrtMinIntensity = options.IrtMinIntensity,
                    IrtMinPeptides = options.IrtMinTransitions,
                    IrtMassTolerance = options.IrtMassTolerance,
                    CacheSpectraToDisk = options.Cache,
                    MinimumIntensity = options.MinimumIntensity,
                    TempFolder = Path.Combine(options.TempFolder, Guid.NewGuid().ToString())
                };

                if (analysisSettings.CacheSpectraToDisk && !Directory.Exists(analysisSettings.TempFolder))
                    Directory.CreateDirectory(analysisSettings.TempFolder);

                using Pipeliner pipeliner = new Pipeliner()
                {
                    Threading = options.Threading ?? true,
                    MaxQueueSize = options.MaxQueueSize,
                    MaxThreads = options.MaxThreads
                };

                if (!string.IsNullOrEmpty(options.IRTFile))
                {
                    irt = true;
                    if (options.IRTFile.ToLower().EndsWith("traml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        TraMLReader traMLReader = new TraMLReader();
                        analysisSettings.IrtLibrary = traMLReader.LoadLibrary(options.IRTFile);

                    }
                    else if (options.IRTFile.ToLower().EndsWith("tsv", StringComparison.InvariantCultureIgnoreCase) || options.IRTFile.ToLower().EndsWith("csv", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SVReader svReader = new SVReader();
                        analysisSettings.IrtLibrary = svReader.LoadLibrary(options.IRTFile);
                    }
                }
                using Run<Scan> run = pipeliner.LoadMzmlAndRunPipeline(options.InputFile, analysisSettings);

                Logger.Info("Generating metrics...", Convert.ToInt32(sw.Elapsed.TotalSeconds));
                    IDictionary<string, dynamic> mergedRenderedMetrics = new Dictionary<string, dynamic>();
                    Utilities.AddRenderedMzqcMetricsTo(mergedRenderedMetrics, new SwaMe.MetricGenerator().GenerateMetrics(run, division, irt));
                    Utilities.AddRenderedMzqcMetricsTo(mergedRenderedMetrics, new Prognosticator.MetricGenerator().GenerateMetrics(run));

                    new MzqcGenerator.MzqcWriter().BuildMzqcAndWrite(options.OutputFile, run, mergedRenderedMetrics, options.InputFile, analysisSettings);
                Logger.Info("Generated metrics in {0} seconds", Convert.ToInt32(sw.Elapsed.TotalSeconds));

                if (analysisSettings.CacheSpectraToDisk)
                {
                    Logger.Trace("Deleting temp files...");
                    Directory.Delete(analysisSettings.TempFolder);
                }
                Logger.Trace("Done!");

            });
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, "An unexpected error occured");
                Logger.Fatal(ex.Message);
                Logger.Fatal(ex.StackTrace);
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
                Stream stream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);
                stream.Close();
            }
            catch (IOException)
            {
                Logger.Error(string.Format("Unable to open the file: {0}.", inputFilePath));
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



    public class Options
    {
        [Option("dir", Required = false, HelpText = "Load from directory: if true, reads the directory path of the input file path and runs on all .mzml files in that directory")]
        public bool? LoadFromDirectory { get; set; }

        [Option('i', "inputfile", Required = true, HelpText = "Input file path, or - to use standard input.")]
        public string InputFile { get; set; }

        [Option('o', "outputfile", Required = true, HelpText = "Output file path, or - to use standard output (NOTE: Use production logging or your log output will also go to stdout).")]
        public string OutputFile { get; set; }

        [Option('v', "verbose", Required = false, HelpText = "Enable verbose logging.")]
        public bool Verbose { get; set; } = false;

        [Option('d', "division", Required = false, HelpText = "Number of units the user would like to divide certain SwaMe metrics into.")]
        public int Division { get; set; } = 1;

        [Option('m', "masstolerance", Required = false, HelpText = "mass tolerance in daltons. This will be used to distinguish which peaks are part of the same chromatogram. Similarly with iRT peptide searching, this is the tolerance that will allow two values to be considered the same peak.")]
        public float MassTolerance { get; set; } = 0.05F;

        [Option('t', "threading", Required = false, HelpText = "whether threading is used")]
        public bool? Threading { get; set; } = true;

        [Option('r', "irtFile", Required = false, HelpText = "iRT file path")]
        public string IRTFile { get; set; } = null;

        [Option("irttolerance", Required = false, HelpText = "iRT mass tolerance")]
        public double IrtMassTolerance { get; set; } = 0.005;

        [Option("irtminintensity", Required = false, HelpText = "iRT min intensity")]
        public double IrtMinIntensity { get; set; } = 200;

        [Option("irtmintransitions", Required = false, HelpText = "iRT min peptides")]
        public int IrtMinTransitions { get; set; } = 3;

        [Option("rttolerance", Required = false, HelpText = "RT tolerance")]
        public double RtTolerance { get; set; } = 2.5;

        [Option('c', "combineFiles", Required = false, HelpText = "Combine files at the end?")]
        public bool Combine { get; set; } = true;

        [Option('z', "cacheSpectraToDisk", Required = false, HelpText = "Cache spectra on read")]
        public bool Cache { get; set; } = false;

        [Option("minimumIntensity", Required = false, HelpText = "The minimum threshold of an intensity value to process")]
        public int MinimumIntensity { get; set; } = 100;

        [Option("maxQueueSize", Required = false, HelpText = "The maximum number of threads to queue. When the number is met, the parser will pause")]
        public int MaxQueueSize { get; set; } = 100;

        [Option("maxThreads", Required = false, HelpText = "The maximum number of worker threads. Set as zero for default system max.")]
        public int MaxThreads { get; set; } = 0;

        [Option("tempFolder", Required = false, HelpText = "The temp folder SwaMe will use. Defaults to the temp path defined by TMP or TEMP on Windows, or TMPPTH on Linux. This folder must already exist and SwaMe must be able to read and write to it.")]
        public string TempFolder { get; set; } = Path.GetTempPath();
    }
}

