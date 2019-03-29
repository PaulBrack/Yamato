using CommandLine;
using System;
using System.Diagnostics;
using NLog;
using System.Collections.Generic;
using System.IO;


namespace Console
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var options = new Options();
            UpdateLoggingLevels(options);

            string path = options.InputFile;
            logger.Info("Loading file: {0}", path);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            MzmlParser.Run run = new MzmlParser.MzmlParser().LoadMzml(path);

            logger.Info("Parsed file in {0} seconds", Convert.ToInt32(sw.Elapsed.TotalSeconds));
            logger.Info("Done!");

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
