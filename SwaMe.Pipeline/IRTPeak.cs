#nullable enable

using System.Collections.Generic;
using LibraryParser;

namespace SwaMe.Pipeline
{
    public class IRTPeak
    {
        public double Mz { get; set; }
        public double Intensity { get; set; }
        public IList<Library.Transition> AssociatedTransitions = new List<Library.Transition>();
        public double RetentionTime { get; set; }
        public double ExpectedRetentionTime { get; set; }
        public List<SpectrumPoint> Spectrum { get; set; } = new List<SpectrumPoint>();
        public double FWHM;
        public double Peaksym;
        public List<double> TransitionRTs { get; set; } = new List<double>();
        public SpectrumPoint? BasePeak;
        public List<PossiblePeak> PossPeaks = new List<PossiblePeak>();
        public double DotProduct;
    }

    public class PossiblePeak
    {
        public double DotProduct;
        public SpectrumPoint? BasePeak;
        public List<List<SpectrumPoint>>? AllTransitions;
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
