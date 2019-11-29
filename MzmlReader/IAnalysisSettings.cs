using LibraryParser;

namespace MzmlParser
{
    public interface IAnalysisSettings
    {
        double MassTolerance { get; set; }
        double RtTolerance { get; set; }
        Library IrtLibrary { get; set; }
        double IrtMassTolerance { get; set; }
        double IrtMinIntensity { get; set; }
        int IrtMinPeptides { get; set; }
        void SetGlobalMassTolerance(int tolerance);
    }
}
