using MzmlParser;
using System.Linq;

namespace Prognosticator
{
    public class MetricGenerator
    {
        public void GenerateMetrics(Run run)
        {
            double[] quartileDivisions = ExtractQuartileDivisionTimes(run, 105);
        }

        private static double[] ExtractQuartileDivisionTimes(Run run, double? washTime)
        {
            double chromatogramTotal = 0;
            if (washTime == null)
                chromatogramTotal = run.Chromatograms.Ms1Tic.Sum(x => x.Item2);
            else
                chromatogramTotal = run.Chromatograms.Ms1Tic.Select(x => x.Item2).Where(x => x < washTime).Sum();
            double[] quartileDivisionTimes = new double[3];
            double cumulativeChromatogramTotal = 0;
            foreach (var timeIntensityPair in run.Chromatograms.Ms1Tic)
            {
                cumulativeChromatogramTotal += timeIntensityPair.Item2;
                if (quartileDivisionTimes[0] != 0 && cumulativeChromatogramTotal >= chromatogramTotal * 0.25)
                    quartileDivisionTimes[0] = timeIntensityPair.Item2;
                else if (quartileDivisionTimes[0] != 0 && cumulativeChromatogramTotal >= chromatogramTotal * 0.5)
                    quartileDivisionTimes[1] = timeIntensityPair.Item2;
                else if (quartileDivisionTimes[0] != 0 && cumulativeChromatogramTotal >= chromatogramTotal * 0.75)
                    quartileDivisionTimes[2] = timeIntensityPair.Item2;
            }
            return quartileDivisionTimes;
        }
    }
}