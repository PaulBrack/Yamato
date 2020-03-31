using System;
using System.Collections.Generic;
using System.IO;
using MzmlParser;

namespace SwaMe.Desktop
{
    public class AnalysisSettingsFileWriter
    {
        public void WriteASFile(Run run, string dateTime, List<string> fileNames) //Multiple files
        {
            string originalDirectory = Directory.GetCurrentDirectory().ToString();
            Directory.SetCurrentDirectory(Directory.GetParent(Directory.GetParent(originalDirectory).ToString()).ToString());
            string ASFile;
            ASFile = dateTime + "_AnalysisSettings_" + ".txt";
            StreamWriter streamWriter = new StreamWriter(ASFile);
            streamWriter.Write("Analysis Settings:");
            streamWriter.Write(Environment.NewLine);
            streamWriter.Write("InputFiles:");
            streamWriter.Write(Environment.NewLine);

            foreach (string file in fileNames) 
            {
                streamWriter.Write(Path.GetFileNameWithoutExtension(file));
                streamWriter.Write(Environment.NewLine);
            }

            //write streamWriter
            string[] phraseToWrite = { "MassTolerance", Convert.ToString(run.AnalysisSettings.MassTolerance), "RtTolerance", Convert.ToString(run.AnalysisSettings.RtTolerance),
                                        "IrtLibrary",string.IsNullOrEmpty(Convert.ToString(run.AnalysisSettings.IrtLibrary)) ? "None" : Convert.ToString(run.AnalysisSettings.IrtLibrary), "IrtMassTolerance",Convert.ToString(run.AnalysisSettings.IrtMassTolerance),
                                        "IrtMinIntensity",Convert.ToString(run.AnalysisSettings.IrtMinIntensity),"IrtMinPeptides",Convert.ToString(run.AnalysisSettings.IrtMinPeptides),
                                        "CacheSpectraToDisk",Convert.ToString(run.AnalysisSettings.CacheSpectraToDisk),"MinimumIntensity",Convert.ToString(run.AnalysisSettings.MinimumIntensity),
                                        "RunEndTime",Convert.ToString(run.AnalysisSettings.RunEndTime)};

            streamWriter.Write(string.Join(Environment.NewLine, phraseToWrite));
            streamWriter.Close();
            Directory.SetCurrentDirectory(originalDirectory);
        }

        public void WriteASFile(Run run, string dateTime, string fileName)//Only one file
        {
            string ASFile;
            ASFile = dateTime + "_AnalysisSettings_" + Path.GetFileNameWithoutExtension(fileName) + ".txt";
            StreamWriter streamWriter = new StreamWriter(ASFile);
            streamWriter.Write("Analysis Settings:");
            streamWriter.Write(Environment.NewLine);
            streamWriter.Write(Environment.NewLine);
            //write streamWriter
            string[] phraseToWrite = { "MassTolerance", Convert.ToString(run.AnalysisSettings.MassTolerance), "RtTolerance", Convert.ToString(run.AnalysisSettings.RtTolerance),
                                        "IrtLibrary",string.IsNullOrEmpty(Convert.ToString(run.AnalysisSettings.IrtLibrary)) ? "None" : Convert.ToString(run.AnalysisSettings.IrtLibrary), "IrtMassTolerance",Convert.ToString(run.AnalysisSettings.IrtMassTolerance),
                                        "IrtMinIntensity",Convert.ToString(run.AnalysisSettings.IrtMinIntensity),"IrtMinPeptides",Convert.ToString(run.AnalysisSettings.IrtMinPeptides),
                                        "CacheSpectraToDisk",Convert.ToString(run.AnalysisSettings.CacheSpectraToDisk),"MinimumIntensity",Convert.ToString(run.AnalysisSettings.MinimumIntensity),
                                        "RunEndTime",Convert.ToString(run.AnalysisSettings.RunEndTime)};

            streamWriter.Write(string.Join(Environment.NewLine, phraseToWrite));
            streamWriter.Close();
        }
    }
}

