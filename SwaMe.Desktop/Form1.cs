using LibraryParser;
using MzmlParser;
using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
        private string irtFilePath = "";





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

        private void ChooseSpectralLibraryButton_Click(object sender, EventArgs e)
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

                    irtFilePath = openFileDialog.FileName;
                    SpectralLibraryLabel.Text = openFileDialog.SafeFileName;
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
                //logger.Error(String.Format("Unable to open the file: {0}.", inputFilePath));
                throw;
            }
        }

        private void StartAnalysisButton_Click(object sender, EventArgs e)
        {
            NLog.Windows.Forms.RichTextBoxTarget rtbTarget = new NLog.Windows.Forms.RichTextBoxTarget();
            LogManager.Configuration = new LoggingConfiguration();

            rtbTarget.ControlName = "LogBox"; // your RichTextBox control/variable name
            LogManager.Configuration.AddTarget("LogBox", rtbTarget);


            //LogManager.ReconfigExistingLoggers();


            string dateTime = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
            List<string> inputFiles = new List<string>();
            Logger logger = LogManager.GetCurrentClassLogger();
            bool lastFile = false;//saving whether its the last file or not, so if we need to combine all the files in the end, we know when the end is.
            string fileSpecificDirectory = DirectoryCreator.CreateOutputDirectory(inputFilePath, dateTime);
            
            logger.Info("Loading file: {0}", inputFilePath);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int division;
            if (rtDivisionUpDown.Value < 100 && rtDivisionUpDown.Value > 0)
                division = Decimal.ToInt32(rtDivisionUpDown.Value);
            else
            {
                logger.Error("Number of divisions must be within the range 1 - 100. You have input: {0}", rtDivisionUpDown.Value);
                throw new ArgumentOutOfRangeException();
            }
            bool irt = !String.IsNullOrEmpty(irtFilePath);

            MzmlParser.MzmlReader mzmlParser = new MzmlParser.MzmlReader
            {
                ParseBinaryData = true,
                Threading = true,
                MaxQueueSize = Decimal.ToInt32(MaxQueueUpDown.Value),
                MaxThreads = Decimal.ToInt32(MaxThreadsUpDown.Value)
            };

            CheckFileIsReadableOrComplain(inputFilePath);

            AnalysisSettings analysisSettings = new AnalysisSettings()
            {
                MassTolerance = Decimal.ToInt32(BasePeakMassToleranceUpDown.Value),
                RtTolerance = Decimal.ToInt32(BasePeakRtToleranceUpDown.Value),
                IrtMinIntensity = Decimal.ToInt32(MinIrtIntensityUpDown.Value),
                IrtMinPeptides = Decimal.ToInt32(irtPeptidesUpDown.Value),
                IrtMassTolerance = Decimal.ToInt32(irtToleranceUpDown.Value),
                CacheSpectraToDisk = CacheToDiskCheckBox.Checked,
                MinimumIntensity = Decimal.ToInt32(MinIrtIntensityUpDown.Value),
                RunEndTime = 0
            };

            if (!String.IsNullOrEmpty(irtFilePath))
            {
                irt = true;
                if (irtFilePath.ToLower().EndsWith("traml", StringComparison.InvariantCultureIgnoreCase))
                {
                    TraMLReader traMLReader = new TraMLReader();
                    analysisSettings.IrtLibrary = traMLReader.LoadLibrary(irtFilePath);

                }
                else if (irtFilePath.ToLower().EndsWith("tsv", StringComparison.InvariantCultureIgnoreCase) || irtFilePath.ToLower().EndsWith("csv", StringComparison.InvariantCultureIgnoreCase))
                {
                    SVReader svReader = new SVReader();
                    analysisSettings.IrtLibrary = svReader.LoadLibrary(irtFilePath);
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

            logger.Info("Generating metrics...", Convert.ToInt32(sw.Elapsed.TotalSeconds));
            var swameMetrics = new SwaMe.MetricGenerator().GenerateMetrics(run, division, inputFilePath, irt, false, lastFile, dateTime);
            var progMetrics = new Prognosticator.MetricGenerator().GenerateMetrics(run);

            var metrics = swameMetrics.Union(progMetrics).ToDictionary(k => k.Key, v => v.Value);
            string[] mzQCName = { dateTime, Path.GetFileNameWithoutExtension(inputFilePath), "mzQC.json" };
            Directory.SetCurrentDirectory(fileSpecificDirectory);
            new MzqcGenerator.MzqcWriter().BuildMzqcAndWrite(string.Join("_", mzQCName), run, metrics, inputFilePath);
            logger.Info("Generated metrics in {0} seconds", Convert.ToInt32(sw.Elapsed.TotalSeconds));

            if (analysisSettings.CacheSpectraToDisk)
            {
                logger.Info("Deleting temp files...");
                mzmlParser.DeleteTempFiles(run);
            }
            logger.Info("Done!");


            LogManager.Shutdown();
            
        }

        private static void SetVerboseLogging()
        {
            //logger.Info("Verbose output selected: enabled logging for all levels");
            foreach (var rule in LogManager.Configuration.LoggingRules)
            {
                rule.EnableLoggingForLevels(LogLevel.Trace, LogLevel.Debug);
            }
            LogManager.ReconfigExistingLoggers();
        }
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
