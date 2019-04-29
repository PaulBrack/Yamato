using System.Collections.Generic;

namespace MzmlParser
{
    public class BasePeak
    {
        public double Mz { get; set; }
        public double intensity { get; set; }
        public double RetentionTime { get; set; }
        public List<SpectrumPoint> Spectrum { get; set; }
        public double RTsegment { get; set; }
        public double FWHM;
        public double peaksym;
        public double peakCapacity;
    }
}
