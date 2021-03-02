#nullable enable

using MzmlParser;

namespace SwaMe.Pipeline
{
    public class ScanAndRunFactory
        : IScanFactory<Scan>
        , IRunFactory<Scan, Run<Scan>>
    {
        private AnalysisSettings AnalysisSettings { get; }

        public ScanAndRunFactory(AnalysisSettings analysisSettings)
        {
            AnalysisSettings = analysisSettings;
        }

        Run<Scan> IRunFactory<Scan, Run<Scan>>.CreateRun()
        {
            return new Run<Scan>(AnalysisSettings);
        }

        Scan IScanFactory<Scan>.CreateScan()
        {
            return new Scan(AnalysisSettings.CacheSpectraToDisk, AnalysisSettings.TempFolder);
        }
    }
}
