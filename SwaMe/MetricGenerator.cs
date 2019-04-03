using System;
using MzmlParser;
using System.Collections.Generic;
using System.Linq;

namespace SwaMe
{
    public class MetricGenerator
    {
        const double massTolerance = 0.05;

        public Run CalculateSwameMetrics(Run run)
        {
            List<Scan> filteredBasePeaks = GetFilteredBasePeaks(run.Ms2Scans);
            return run;
        }

        private static List<Scan> GetFilteredBasePeaks(List<Scan>Scans)
        {
            List<Scan> filteredScans = new List<Scan>();
            Scans = Scans.OrderBy(x => x.BasePeakMz).ToList();
            for (int i = 1; i < Scans.Count; ++i)
            {
                var scan = Scans[i];
                var prevscan = Scans[i - 1];

                double mzDiff = Math.Abs(scan.BasePeakMz - prevscan.BasePeakMz);

                if (mzDiff >= massTolerance)
                {
                    filteredScans.Add(scan);
                }
            }
            filteredScans = filteredScans.OrderBy(x => x.ScanStartTime).ToList();
            return filteredScans;
        }
    }
}
