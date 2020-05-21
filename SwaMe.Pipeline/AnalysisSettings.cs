#nullable enable


using LibraryParser;

namespace SwaMe.Pipeline
{
    public class AnalysisSettings
    {
        public double MassTolerance { get; set; }
        public double RtTolerance { get; set; }
        public Library? IrtLibrary { get; set; }
        public double IrtMassTolerance { get; set; }
        public double IrtMinIntensity { get; set; }
        public int IrtMinPeptides { get; set; }
        public void SetGlobalMassTolerance (int tolerance)
        {
            MassTolerance = tolerance;
            IrtMassTolerance = tolerance;
        }
        public bool CacheSpectraToDisk { get; set; }
        public int MinimumIntensity { get; set; }
        public string? TempFolder { get; set; }
    }
}
