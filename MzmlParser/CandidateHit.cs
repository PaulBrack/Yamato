using System.Collections.Generic;

namespace MzmlParser
{
    public class CandidateHit
    {
        public List<double> ProductTargetMzs { get; set; }
        public double PrecursorTargetMz { get; set; }
        public string PeptideSequence { get; set; }
        public List<float> Intensities { get; set; }
        public double RetentionTime { get; set; }
        public CandidateHit()
        {
            Intensities = new List<float>();
            ProductTargetMzs = new List<double>();
        }
    }
}
