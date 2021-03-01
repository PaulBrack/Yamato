using LibraryParser;
using NLog;
using NLog.Config;
using SwaMe.Pipeline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SwaMe.Desktop
{
    public partial class Form1 : Form
    {

        private string inputFilePath = @"c:\wiffs\collinsb_I180316_005_SW-A.mzML";
        private string irtFilePath = @"c:\wiffs\hroest_DIA_iRT.TraML";
        private CancellationTokenSource _cts;

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

        private async void StartAnalysisButton_Click(object sender, EventArgs e)
        {
            CancelButton.Enabled = true;
            StartAnalysisButton.Enabled = false;


            NLog.Windows.Forms.RichTextBoxTarget rtbTarget = new NLog.Windows.Forms.RichTextBoxTarget();
            LogManager.Configuration = new LoggingConfiguration();

            rtbTarget.ControlName = "LogBox";
            LogManager.Configuration.AddTarget("LogBox", rtbTarget);

            //LogManager.ReconfigExistingLoggers();

            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            try
            {
                await Task.Run(() =>
                {
                    AnalyseSample();
                });
            }
            catch (OperationCanceledException)
            {

                StartAnalysisButton.Enabled = true;
                CancelButton.Enabled = false;
            }
        }


        private void AnalyseSample()
        {
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            List<string> inputFiles = new List<string>();
            Logger logger = LogManager.GetCurrentClassLogger();
            bool lastFile = false;//saving whether its the last file or not, so if we need to combine all the files in the end, we know when the end is.


            logger.Info("Loading file: {0}", inputFilePath);
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int division;
            if (rtDivisionUpDown.Value < 100 && rtDivisionUpDown.Value > 0)
                division = decimal.ToInt32(rtDivisionUpDown.Value);
            else
            {
                logger.Error("Number of divisions must be within the range 1 - 100. You have input: {0}", rtDivisionUpDown.Value);
                throw new ArgumentOutOfRangeException();
            }
            bool irt = !string.IsNullOrEmpty(irtFilePath);

            Pipeliner pipeliner = new Pipeliner()
            {
                Threading = true,
                MaxQueueSize = decimal.ToInt32(MaxQueueUpDown.Value),
                MaxThreads = decimal.ToInt32(MaxThreadsUpDown.Value),
                CancellationToken = _cts.Token
            };

            CheckFileIsReadableOrComplain(inputFilePath);

            AnalysisSettings analysisSettings = new AnalysisSettings()
            {
                MassTolerance = decimal.ToDouble(BasePeakMassToleranceUpDown.Value),
                RtTolerance = decimal.ToDouble(BasePeakRtToleranceUpDown.Value),
                IrtMinIntensity = decimal.ToDouble(MinIrtIntensityUpDown.Value),
                IrtMinPeptides = decimal.ToInt32(irtPeptidesUpDown.Value),
                IrtMassTolerance = decimal.ToDouble(irtToleranceUpDown.Value),
                CacheSpectraToDisk = CacheToDiskCheckBox.Checked,
                MinimumIntensity = decimal.ToInt32(MinIrtIntensityUpDown.Value),
                TempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            };

            if (analysisSettings.CacheSpectraToDisk && !Directory.Exists(analysisSettings.TempFolder))
                Directory.CreateDirectory(analysisSettings.TempFolder);

            if (!string.IsNullOrEmpty(irtFilePath))
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
            using (Run<Scan> run = pipeliner.LoadMzml(inputFilePath, analysisSettings))
            {
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
                IDictionary<string, dynamic> mergedRenderedMetrics = new Dictionary<string, dynamic>();
                Utilities.AddRenderedMzqcMetricsTo(mergedRenderedMetrics, new SwaMe.MetricGenerator().GenerateMetrics(run, division, irt));
                Utilities.AddRenderedMzqcMetricsTo(mergedRenderedMetrics, new Prognosticator.MetricGenerator().GenerateMetrics(run));

                new MzqcGenerator.MzqcWriter().BuildMzqcAndWrite("", run, mergedRenderedMetrics, inputFilePath, analysisSettings);
                logger.Info("Generated metrics in {0} seconds", Convert.ToInt32(sw.Elapsed.TotalSeconds));
            }
            if (analysisSettings.CacheSpectraToDisk)
                Directory.Delete(analysisSettings.TempFolder);
            logger.Trace("Done!");
            LogManager.Shutdown();
        }

        private static void SetVerboseLogging()
        {
            foreach (var rule in LogManager.Configuration.LoggingRules)
            {
                rule.EnableLoggingForLevels(LogLevel.Trace, LogLevel.Debug);
            }
            LogManager.ReconfigExistingLoggers();
        }

        private void CancelAnalysisButton_Click(object sender, EventArgs e)
        {
            if (_cts != null)
                _cts.Cancel();
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
