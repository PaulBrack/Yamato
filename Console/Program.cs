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

                int division;
                division = options.Division;

                double massTolerance;
                massTolerance = options.MassTolerance;

                bool irt = false;
                string iRTpath = "none";
                if (String.IsNullOrEmpty(options.IRTFile))
                {
                    iRTpath = options.IRTFile;
                    irt = true;
                }
                
                MzmlParser.MzmlReader mzmlParser = new MzmlParser.MzmlReader();
                if (options.ParseBinaryData == false)
                    mzmlParser.ParseBinaryData = false;
                if (options.Threading == false)
                    mzmlParser.Threading = false;

                MzmlParser.Run run = mzmlParser.LoadMzml(inputFilePath, massTolerance,irt);
                if (irt == true)
                {
                    IRTSearcher.IRTPeptideMatch irtSearcher = new IRTSearcher.IRTPeptideMatch();
                    run = irtSearcher.ParseLibrary(run, iRTpath,massTolerance);
                }
                
                run = new MzmlParser.ChromatogramGenerator().CreateAllChromatograms(run);
                new SwaMe.MetricGenerator().GenerateMetrics(run, division, inputFilePath,massTolerance);
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
        public int Division { get; set; } = 1;

        [Option('m', "masstolerance", Required = false, HelpText = "m/z tolerance. The closest m/z value to the m/z of the basepeak that is still within this value from the basepeak m/z are part of the same chromatogram. Similarly with iRT peptide searching, this is the tolerance that will allow two values to be considered the same peak.")]
        public float MassTolerance { get; set; } = 0.05F;

        [Option('p', "parsebinarydata", Required = false, HelpText = "whether binary data will be parsed")]
        public bool? ParseBinaryData { get; set; } = true;

        [Option('t', "threading", Required = false, HelpText = "whether threading is used")]
        public bool? Threading { get; set; } = true;

        [Option('r', "iRT filepath", Required = false, HelpText = "iRT file path")]
        public String IRTFile { get; set; } = null;
    }
}

