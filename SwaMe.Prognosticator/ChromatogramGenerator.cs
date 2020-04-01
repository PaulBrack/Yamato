using System;
using System.Collections.Generic;
using System.Linq;
using MzmlParser;
using NLog;

namespace Prognosticator
{
    public static class ChromatogramGenerator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static Run CreateAllChromatograms(Run run)
        {
            run.Chromatograms.Ms1Tic = ExtractMs1TotalIonChromatogram(run);
            run.Chromatograms.Ms2Tic = ExtractMs2TotalIonChromatogram(run);
            run.Chromatograms.Ms1Bpc = ExtractMs1BasePeakChromatogram(run);
            run.Chromatograms.Ms2Bpc = ExtractMs2BasePeakChromatogram(run);
            run.Chromatograms.CombinedTic = CreateCombinedChromatogram(run);
            return run;
        }

        public static List<(double, double)> ExtractMs1TotalIonChromatogram(Run run)
        {
            return run.Ms1Scans.Select(x => (x.ScanStartTime, x.TotalIonCurrent)).ToList();
        }

        public static List<(double, double)> ExtractMs2TotalIonChromatogram(Run run)
        {
            return run.Ms2Scans.GroupBy(x => x.Cycle).Select(x => new ValueTuple<double, double>(x.First().ScanStartTime, x.Select(y => y.TotalIonCurrent).Sum())).OrderBy(x => x.Item1).ToList();
        }

        public static List<(double, double)> ExtractMs1BasePeakChromatogram(Run run)
        {
            return run.Ms1Scans.Select(x => (x.ScanStartTime, x.BasePeakIntensity)).ToList();
        }

        public static List<(double, double)> ExtractMs2BasePeakChromatogram(Run run)
        {
            return run.Ms2Scans.GroupBy(x => x.Cycle).Select(x => new ValueTuple<double, double>(x.First().ScanStartTime, x.Select(y => y.BasePeakIntensity).Max())).OrderBy(x => x.Item1).ToList();
        }

        public static List<(double, double, double)> CreateCombinedChromatogram(Run run)
        {
            if (run.Chromatograms.Ms1Tic.Count == run.Chromatograms.Ms2Tic.Count)
                return run.Chromatograms.Ms1Tic.Select((x, i) => new ValueTuple<double, double, double>(x.Item1, x.Item2, run.Chromatograms.Ms2Tic[i].Item2)).ToList();
            else
            {
                Logger.Warn("Unable to produce combined chromatogram as MS1 and MS2 chromatograms have different nummbers of elements ({0} vs {1})", run.Ms1Scans.Count, run.Ms2Scans.Count);
                return new List<(double, double, double)>();
            }
        }
    }
}
