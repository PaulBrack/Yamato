#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace SwaMe.Pipeline
{
    public class BasePeak
    {
        public IList<SpectrumPoint> Spectrum { get; }

        public BasePeak (Scan scan, double massTolerance, IEnumerable<SpectrumPoint>spectrum)
        {
            Mz = scan.BasePeakMz;
            Intensities = new List<double> { scan.BasePeakIntensity };
            Spectrum = new List<SpectrumPoint> { spectrum.Where(x => Math.Abs(x.Mz - scan.BasePeakMz) <= massTolerance).MaxEvaluatedWith((lhs, rhs) => lhs.Intensity > rhs.Intensity ? 1 : 0) };
            BpkRTs = new List<double> { scan.ScanStartTime };
        }

        public BasePeak(double mz, double scanStartTime, double basepeakintensity)
        {
            Mz = mz;
            Intensities = new List<double> { basepeakintensity };
            Spectrum = new List<SpectrumPoint>();
            BpkRTs = new List<double> { scanStartTime };
        }
    }
}
