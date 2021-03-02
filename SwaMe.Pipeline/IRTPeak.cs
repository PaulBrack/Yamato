#nullable enable

using System.Collections.Generic;
using LibraryParser;

namespace SwaMe.Pipeline
{
    public class IRTPeak
    {
        public double Mz { get; set; }
        public double Intensity { get; set; }
        public IList<Library.Transition> AssociatedTransitions { get; set; } = new List<Library.Transition>();
        public double RetentionTime { get; set; }
        public double ExpectedRetentionTime { get; set; }
        public List<SpectrumPoint> Spectrum { get; set; } = new List<SpectrumPoint>();
        public double FullWidthHalfMax { get; set; }
        public double PeakSymmetry { get; set; }
        public List<double> TransitionRTs { get; set; } = new List<double>();
        public SpectrumPoint? BasePeak { get; set; }
        public List<PossiblePeak> PossPeaks { get; set; } = new List<PossiblePeak>();
        public double DotProduct { get; set; }
    }

    public class PossiblePeak
    {
        public double DotProduct { get; set; }
        public SpectrumPoint? BasePeak { get; set; }
        public List<List<SpectrumPoint>>? AllTransitions { get; set; }
        public int MatchingTransitions { get; set; } = 0;
    }

    public class SmoothedPeak
    {
        public double DotProduct { get; set; }
        public double RT { get; set; }
        public double PeakArea { get; set; }
        public double FWHMAllTransitions { get; set; }
        public double PSAllTransitions { get; set; }
    }
}
