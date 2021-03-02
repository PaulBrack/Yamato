#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace SwaMe.Pipeline
{
    public class BasePeak
    {
        public double Mz { get; }
        public IList<double> Intensities { get; }
        public IList<SpectrumPoint> Spectrum { get; }
        public IList<double> RTsegments { get; } = new List<double>();
        public IList<double> FullWidthHalfMaxes { get; } = new List<double>();
        public IList<double> PeakSymmetries { get; } = new List<double>();
        public IList<double> FullWidthBaselines { get; } = new List<double>();
        public IList<double> BasePeakRetentionTimes { get; }

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
            BasePeakRetentionTimes = new List<double> { scan.ScanStartTime };
        }

        /// <summary>
        /// Test access: fake up a BasePeak with the given values.  TODO: Tests should probably mock to an interface instead.
        /// </summary>
        public BasePeak(double mz, double scanStartTime, double basePeakIntensity, params SpectrumPoint[] spectrumPoints)
        {
            Mz = mz;
            Intensities = new List<double> { basePeakIntensity };
            Spectrum = spectrumPoints.ToList();
            BasePeakRetentionTimes = new List<double> { scanStartTime };
        }
    }
}
