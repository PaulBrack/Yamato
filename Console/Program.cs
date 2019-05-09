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
                
                string inputFilePath = options.InputFile;
                logger.Info("Loading file: {0}", inputFilePath);

                Stopwatch sw = new Stopwatch();
                sw.Start();

                int division = options.Division;
                string iRTpath = "none";
                if (options.iRTFile != null)
                { iRTpath = options.iRTFile; }
                
                MzmlParser.MzmlReader mzmlParser = new MzmlParser.MzmlReader();
                if (options.ParseBinaryData == false)
                    mzmlParser.ParseBinaryData = false;
                if (options.Threading == false)
                    mzmlParser.Threading = false;

                MzmlParser.Run run = mzmlParser.LoadMzml(inputFilePath);
                run = new MzmlParser.ChromatogramGenerator().CreateAllChromatograms(run);
                new SwaMe.MetricGenerator().GenerateMetrics(run, division,iRTpath, inputFilePath);
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

    public class Options
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

        [Option('t', "threading", Required = false, HelpText = "whether threading is used")]
        public bool? Threading { get; set; }

        [Option('r', "iRT filepath", Required = false, HelpText = "iRT file path")]
        public String iRTFile { get; set; }
    }
}

