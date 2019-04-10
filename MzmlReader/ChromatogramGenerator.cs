using System.Collections.Generic;
using System.Linq;

namespace MzmlParser
{
    public class ChromatogramGenerator
    {
        public Run CreateAllChromatograms(Run run)
        {
            run.Chromatograms.Ms1Tic = ExtractMs1TotalIonChromatogram(run);
            run.Chromatograms.Ms2Tic = ExtractMs2TotalIonChromatogram(run);
            run.Chromatograms.Ms1Bpc = ExtractMs1BasePeakChromatogram(run);
            run.Chromatograms.Ms2Bpc = ExtractMs2BasePeakChromatogram(run);
            return run;
        }

        public List<(double, double)> ExtractMs1TotalIonChromatogram(Run run)
        {
            List<(double, double)> chromatogram = run.Ms1Scans.Select(x => (x.ScanStartTime, x.TotalIonCurrent)).ToList();
            return chromatogram;
        }

        public List<(double, double)> ExtractMs2TotalIonChromatogram(Run run)
        {
            List<(double, double)> chromatogram = new List<(double, double)>();
            foreach (int cycle in run.Ms2Scans.Select(x => x.Cycle).Distinct().ToList())
            {
                var selectedScans = run.Ms2Scans.Where(x => x.Cycle == cycle);
                double startTime = selectedScans.First().ScanStartTime;
                double tic = selectedScans.Select(x => x.TotalIonCurrent).Sum();
                chromatogram.Add((startTime, tic));
            }
            return chromatogram;
        }

        public List<(double, double)> ExtractMs1BasePeakChromatogram(Run run)
        {
            List<(double, double)> chromatogram = run.Ms1Scans.Select(x => (x.ScanStartTime, x.BasePeakIntensity)).ToList();
            return chromatogram;
        }

        public List<(double, double)> ExtractMs2BasePeakChromatogram(Run run)
        {
            List<(double, double)> chromatogram = new List<(double, double)>();
            foreach (int cycle in run.Ms2Scans.Select(x => x.Cycle).Distinct().ToList())
            {
                var selectedScans = run.Ms2Scans.Where(x => x.Cycle == cycle);
                double startTime = selectedScans.First().ScanStartTime;
                double bp = selectedScans.Select(x => x.BasePeakIntensity).Max();
                chromatogram.Add((startTime, bp));
            }
            return chromatogram;
        }
    }
}
