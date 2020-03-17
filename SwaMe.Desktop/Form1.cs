using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SwaMe.Desktop
{
    public partial class Form1 : Form
    {

        private string inputFilePath = "";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void choose_file_click(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "mzml files (*.mzml)|*.mzml|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    inputFilePath = openFileDialog.FileName;
                    fileNameLabel.Text = openFileDialog.SafeFileName;
                }
            }


        }



        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void maskedTextBox5_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {


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

        private void StartAnalysisButton_Click(object sender, EventArgs e)
        {
            try
            {

                string dateTime = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
                List<string> inputFiles = new List<string>();

                bool lastFile = false;//saving whether its the last file or not, so if we need to combine all the files in the end, we know when the end is.
                string fileSpecificDirectory = DirectoryCreator.CreateOutputDirectory(inputFilePath, dateTime);
                
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
                    IrtMinPeptides = options.IrtMinTransitions,
                    IrtMassTolerance = options.IrtMassTolerance,
                    CacheSpectraToDisk = options.Cache,
                    MinimumIntensity = options.MinimumIntensity,
                    RunEndTime = options.RunEndTime
                };

                if (!String.IsNullOrEmpty(options.IRTFile))
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
