#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace SwaMe.Pipeline
{
    public class BasePeak
    {
        public double Mz { get; }
        public List<double> Intensities { get; }
        public IList<SpectrumPoint> Spectrum { get; }
        public List<double> RTsegments { get; } = new List<double>();
        public List<double> FWHMs { get; } = new List<double>();
        public List<double> Peaksyms { get; } = new List<double>();
        public List<double> FullWidthBaselines { get; } = new List<double>();
        public List<double> BpkRTs { get; }

        public BasePeak (Scan scan, double massTolerance, IEnumerable<SpectrumPoint> spectrum)
        {
            Mz = scan.BasePeakMz;
            Intensities = new List<double> { scan.BasePeakIntensity };
            Spectrum = new List<SpectrumPoint>
            {
                spectrum
                    .Where(x => Math.Abs(x.Mz - scan.BasePeakMz) <= massTolerance)
                    .MaxEvaluatedWith((lhs, rhs) => lhs.Intensity > rhs.Intensity)
            };
            BpkRTs = new List<double> { scan.ScanStartTime };
        }

        /// <summary>
        /// Test access: fake up a BasePeak with the given values.  TODO: Tests should probably mock to an interface instead.
        /// </summary>
        public BasePeak(double mz, double scanStartTime, double basepeakintensity, params SpectrumPoint[] spectrumPoints)
        {
            Mz = mz;
            Intensities = new List<double> { basepeakintensity };
            Spectrum = spectrumPoints.ToList();
            BpkRTs = new List<double> { scanStartTime };
        }
    }
}
