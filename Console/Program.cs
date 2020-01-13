using CommandLine;
using System;
using System.Diagnostics;
using NLog;
using LibraryParser;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;
using MzmlParser;
using System.Collections.Concurrent;

namespace Yamato.Console
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {

            Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
            {
                UpdateLoggingLevels(options);
                bool combine = options.Combine;
                string dateTime = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
                List<string> inputFiles = new List<string>();

                if (options.LoadFromDirectory != null && options.LoadFromDirectory == true)//multiple files
                {
                    var directoryPath = Path.GetDirectoryName(options.InputFile);
                    DirectoryInfo di = new DirectoryInfo(directoryPath);
                    logger.Info("Attempting to load files with the extension .mzml in the following directory: {0}", directoryPath);
                    foreach (var file in di.GetFiles("*.mzml", SearchOption.TopDirectoryOnly))
                        inputFiles.Add(file.FullName);

                    if (inputFiles.Count == 0)
                    {
                        logger.Error("Unable to locate any MZML files in {0} directory", directoryPath);
                        throw new FileNotFoundException();
                    }
                }
                else //single file
                    inputFiles.Add(options.InputFile);

                foreach (string inputFilePath in inputFiles)
                {
                    bool lastFile = false;//saving whether its the last file or not, so if we need to combine all the files in the end, we know when the end is.
                    if (inputFilePath == inputFiles.Last()) lastFile = true;
                    logger.Info("Loading file: {0}", inputFilePath);
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    int division;
                    if (options.Division < 100 && options.Division > 0)
                        division = options.Division;
                    else
                        throw new ArgumentOutOfRangeException("Your entry for division is not within the range 1 - 100");

                    bool irt = false;
                    if (!String.IsNullOrEmpty(options.IRTFile))
                        irt = true;

                    MzmlParser.MzmlReader mzmlParser = new MzmlParser.MzmlReader();
                    if (options.ParseBinaryData == false)
                        mzmlParser.ParseBinaryData = false;
                    if (options.Threading == false)
                        mzmlParser.Threading = false;

                    CheckFileIsReadableOrComplain(inputFilePath);

                    AnalysisSettings analysisSettings = new AnalysisSettings()
                    {
                        MassTolerance = options.MassTolerance,
                        RtTolerance = options.RtTolerance,
                        IrtMinIntensity = options.IrtMinIntensity,
                        IrtMinPeptides = options.IrtMinPeptides,
                        IrtMassTolerance = options.IrtMassTolerance,
                        CacheSpectraToDisk = options.Cache ?? false,
                        MinimumIntensity = options.MinimumIntensity
                    };

                    if (!String.IsNullOrEmpty(options.IRTFile))
                    {
                        irt = true;
                        TraMLReader traMLReader = new TraMLReader();
                        analysisSettings.IrtLibrary = traMLReader.LoadLibrary(options.IRTFile);
                    }
                    MzmlParser.Run run = mzmlParser.LoadMzml(inputFilePath, irt, analysisSettings);

                    run = new MzmlParser.ChromatogramGenerator().CreateAllChromatograms(run);
                    new SwaMe.MetricGenerator().GenerateMetrics(run, division, inputFilePath, irt, combine, lastFile, dateTime);
                    logger.Info("Parsed file in {0} seconds", Convert.ToInt32(sw.Elapsed.TotalSeconds));
                    logger.Info("Done!");

                }
            });
            LogManager.Shutdown();
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
                logger.Error(String.Format("Unable to open the file: {0}.", inputFilePath));
                throw;
            }
        }

        private static void UpdateLoggingLevels(Options options)
        {
            logger.Info("Verbose output selected: enabled logging for all levels");
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

        [Option('i', "inputfile", Required = true, HelpText = "Input file path.")]
        public String InputFile { get; set; }

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
        public double IrtMassTolerance { get; set; } = 0.05;

        [Option("irtminintensity", Required = false, HelpText = "iRT min intensity")]
        public double IrtMinIntensity { get; set; } = 500;

        [Option("irtminpeptides", Required = false, HelpText = "iRT min peptides")]
        public int IrtMinPeptides { get; set; } = 3;

        [Option("rttolerance", Required = false, HelpText = "RT tolerance")]
        public double RtTolerance { get; set; } = 2.5;

        [Option('c', "combineFiles", Required = false, HelpText = "Combine files at the end?")]
        public bool Combine { get; set; } = true;

        [Option('z', "cacheSpectraToDisk", Required = false, HelpText = "Cache spectra on read")]
        public bool? Cache { get; set; } = false;

        [Option("minimumIntensity", Required = false, HelpText = "The minimum threshold of an intensity value to process")]
        public int MinimumIntensity { get; set; } = 100;
    }
}

