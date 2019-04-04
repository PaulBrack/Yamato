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
                MzmlParser.Run run = mzmlParser.LoadMzml(path);
                run = new MzmlParser.ChromatogramGenerator().CreateAllChromatograms(run);
                new SwaMe.MetricGenerator().GenerateMetrics(run);
                logger.Info("Parsed file in {0} seconds", Convert.ToInt32(sw.Elapsed.TotalSeconds));
                logger.Info("Done!");
            });
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
    }
}
