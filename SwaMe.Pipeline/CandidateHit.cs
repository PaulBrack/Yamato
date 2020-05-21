#nullable enable

using System.Collections.Generic;

namespace SwaMe.Pipeline
{
    public class CandidateHit
    {
        public List<double> ProductTargetMzs { get; set; } = new List<double>();
        public double PrecursorTargetMz { get; set; }
        public string? PeptideSequence { get; set; }
        public List<float> Intensities { get; set; } = new List<float>();
        public List<float>? ActualMzs { get; set; }
        public double RetentionTime { get; set; }
        public double AverageMassError { get; set; }
        public double TotalMassError { get; set; }

        public double TotalMassErrorPpm { get; set; }

        public double AverageMassErrorPpm { get; set; }
    }
}
