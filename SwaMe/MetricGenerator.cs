using System;
using MzmlParser;
using System.Collections.Generic;

namespace SwaMe
{
    public class MetricGenerator
    {
        const double massTolerance = 5;

        public Run CalculateSwameMetrics(Run run)
        {           
            RemoveDuplicateScans(run.Ms1Scans);
            RemoveDuplicateScans(run.Ms2Scans);
            return run;
        }

        private static void RemoveDuplicateScans(List<Scan> ms1Scans)
        {
            for (int i = 1; i < ms1Scans.Count; ++i)
            {
                var scan = ms1Scans[i];
                var prevscan = ms1Scans[i - 1];

                double mzDiff = Math.Abs(scan.BasePeakMz - prevscan.BasePeakMz);

                if (mzDiff <= massTolerance)
                {
                    ms1Scans.Remove(scan);
                }
            }
        }
    }
}
