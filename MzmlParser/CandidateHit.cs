using System.Collections.Generic;

namespace MzmlParser
{
    public class CandidateHit
    {
        public List<double> ProductTargetMzs { get; set; }
        public double PrecursorTargetMz { get; set; }
        public string PeptideSequence { get; set; }
        public List<float> Intensities { get; set; }
        public List<float> ActualMzs { get; set; }
        public double RetentionTime { get; set; }
        public double AverageMassError { get; set; }
        public double TotalMassError { get; set; }

        public double TotalMassErrorPpm { get; set; }

        public double AverageMassErrorPpm { get; set; }
        public CandidateHit()
        {
            Intensities = new List<float>();
            ProductTargetMzs = new List<double>();
        }
    }
}
