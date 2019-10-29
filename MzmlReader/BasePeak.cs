using System;
using System.Collections.Generic;
using System.Linq;

namespace MzmlParser
{
    public class BasePeak
    {
        public double Mz { get; set; }
        public List<double> Intensities { get; set; }
        public List<SpectrumPoint> Spectrum { get; set; }
        public List<double> RTsegments { get; set; }
        public List<double> FWHMs;
        public List<double> Peaksyms;
        public List<double> FWBaselines;
        public List<double> BpkRTs;

        public BasePeak (Scan scan, double massTolerance, List<SpectrumPoint>spectrum)
        {
            Mz = scan.BasePeakMz;
            Intensities = new List<double>();
            Intensities.Add(scan.BasePeakIntensity);
            Spectrum = spectrum.Where(x => Math.Abs(x.Mz - scan.BasePeakMz) <= massTolerance).OrderByDescending(x => x.Intensity).Take(1).ToList();
            BpkRTs = new List<double>();
            BpkRTs.Add(scan.ScanStartTime);
            RTsegments = new List<double>();
            FWHMs = new List<double>();
            Peaksyms = new List<double>();
            FWBaselines = new List<double>();
        }
        public BasePeak(double mz, double scanStartTime, double basepeakintensity)
        {
            Mz = mz;
            Intensities = new List<double>();
            Intensities.Add(basepeakintensity);
            Spectrum = new List<SpectrumPoint>();
            BpkRTs = new List<double>();
            BpkRTs.Add(scanStartTime);
            RTsegments = new List<double>();
            FWHMs = new List<double>();
            Peaksyms = new List<double>();
            FWBaselines = new List<double>();
        }
    }
}
