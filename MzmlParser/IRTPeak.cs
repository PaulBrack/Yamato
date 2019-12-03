using System.Collections.Generic;
using LibraryParser;

namespace MzmlParser
{
    public class IRTPeak
    {
        public double Mz { get; set; }
        public double Intensity { get; set; }
        public List<Library.Transition> AssociatedTransitions;
        public double RetentionTime { get; set; }
        public double ExpectedRetentionTime { get; set; }
        public List<SpectrumPoint> Spectrum { get; set; }
        public double FWHM;
        public double Peaksym;
        public List<double> TransitionRTs { get; set; }
        public SpectrumPoint BasePeak;
        public List<PossiblePeak> PossPeaks;
        public double DotProduct;

        public IRTPeak()
        {
            AssociatedTransitions = new List<Library.Transition>();
            Spectrum = new List<SpectrumPoint>();
            TransitionRTs = new List<double>();
            PossPeaks = new List<PossiblePeak>();
        }
    }

    public class PossiblePeak
    {
        public double DotProduct;
        public SpectrumPoint BasePeak;
        public List<List<SpectrumPoint>> Alltransitions;
        public int MatchingTransitions = 0;
    }

    public class SmoothedPeak
    {
        public double DotProduct;
        public double RT;
        public double peakArea;
        public double FWHMAllTransitions;
        public double PSAllTransitions;
    }
}
