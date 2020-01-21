﻿using System;
using System.Collections.Generic;
using System.Linq;
using MzmlParser;

namespace Prognosticator
{
    public static class ChromatogramGenerator
    {
        public static Run CreateAllChromatograms(Run run)
        {
            run.Chromatograms.Ms1Tic = ExtractMs1TotalIonChromatogram(run);
            run.Chromatograms.Ms2Tic = ExtractMs2TotalIonChromatogram(run);
            run.Chromatograms.Ms1Bpc = ExtractMs1BasePeakChromatogram(run);
            run.Chromatograms.Ms2Bpc = ExtractMs2BasePeakChromatogram(run);
            return run;
        }

        public static List<(double, double)> ExtractMs1TotalIonChromatogram(Run run)
        {
            return run.Ms1Scans.Select(x => (x.ScanStartTime, x.TotalIonCurrent)).ToList();
        }

        public static List<(double, double)> ExtractMs2TotalIonChromatogram(Run run)
        {
            return run.Ms2Scans.GroupBy(x => x.Cycle).Select(x => new ValueTuple<double, double>(x.First().ScanStartTime, x.Select(y => y.TotalIonCurrent).Sum())).ToList();
        }

        public static List<(double, double)> ExtractMs1BasePeakChromatogram(Run run)
        {
            return run.Ms1Scans.Select(x => (x.ScanStartTime, x.BasePeakIntensity)).ToList();
        }

        public static List<(double, double)> ExtractMs2BasePeakChromatogram(Run run)
        {
            return run.Ms2Scans.GroupBy(x => x.Cycle).Select(x => new ValueTuple<double, double>(x.First().ScanStartTime, x.Select(y => y.BasePeakIntensity).Max())).ToList();
        }
    }
}
