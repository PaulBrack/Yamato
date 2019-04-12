using CommandLine;
using System;
using System.Diagnostics;
using NLog;

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

                string path = options.InputFile;
                logger.Info("Loading file: {0}", path);

                Stopwatch sw = new Stopwatch();
                sw.Start();

                MzmlParser.MzmlParser mzmlParser = new MzmlParser.MzmlParser();
                if (options.ParseBinaryData == false)
                    mzmlParser.ParseBinaryData = false;

                MzmlParser.Run run = mzmlParser.LoadMzml(path);
                run = new MzmlParser.ChromatogramGenerator().CreateAllChromatograms(run);
                new SwaMe.MetricGenerator().GenerateMetrics(run);
                logger.Info("Parsed file in {0} seconds", Convert.ToInt32(sw.Elapsed.TotalSeconds));
                logger.Info("Done!");
            });
            LogManager.Shutdown();
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

    class Options
    {
        [Option('i', "inputfile", Required = true, HelpText = "Input file path.")]
        public String InputFile { get; set; }

        [Option('d', "division", Required = false, HelpText = "Number of units the user would like to divide certain SwaMe metrics into.")]
        public int Division { get; set; }

        [Option('u', "upperoffset", Required = false, HelpText = "m/z tolerance upper offset. The closest m/z value to the m/z of the basepeak that is still within the upper and lower offest from the basepeak m/z are part of the same chromatogram.")]
        public float UpperOffset { get; set; }

        [Option('l', "loweroffset", Required = false, HelpText = "m/z tolerance lower offset. The closest m/z value to the m/z of the basepeak that is still within the upper and lower offest from the basepeak m/z are part of the same chromatogram.")]
        public float LowerOffset { get; set; }

        [Option('p', "parsebinarydata", Required = false, HelpText = "whether binary data will be parsed")]
        public bool? ParseBinaryData { get; set; }
    }
}

