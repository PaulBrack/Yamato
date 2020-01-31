using MzmlParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Prognosticator
{
    public class MetricGenerator
    {
        public double[] Ms1QuartileDivisions { get; private set; }
        public double[] Ms2QuartileDivisions { get; private set; }
        public Run Run { get; private set; }

        public Dictionary<string, dynamic> GenerateMetrics(Run run)
        {
            Run = Prognosticator.ChromatogramGenerator.CreateAllChromatograms(run);
            Ms1QuartileDivisions = ExtractQuartileDivisionTimes(run, run.AnalysisSettings.RunEndTime, 1);
            Ms2QuartileDivisions = ExtractQuartileDivisionTimes(run, run.AnalysisSettings.RunEndTime, 2);

            return AssembleMetrics();
        }

        private static double[] ExtractQuartileDivisionTimes(Run run, double? washTime, int msLevel)
        {
            List<(double, double)> chromatogram;

            if (msLevel == 1)
                chromatogram = run.Chromatograms.Ms1Tic;
            else if (msLevel == 2)
                chromatogram = run.Chromatograms.Ms2Tic;
            else
                throw new ArgumentOutOfRangeException("MS Level must be 1 or 2");


            double chromatogramTotal = 0;
            if (washTime == 0)
                chromatogramTotal = chromatogram.Sum(x => x.Item2);
            else
                chromatogramTotal = chromatogram.Where(x => x.Item1 < washTime).Select(x => x.Item2).Sum();
            double[] quartileDivisionTimes = new double[3] { 0, 0, 0 };
            double cumulativeChromatogramTotal = 0;
            foreach (var timeIntensityPair in chromatogram)
            {

                cumulativeChromatogramTotal += timeIntensityPair.Item2;
                if (!(quartileDivisionTimes[0] > 0) && cumulativeChromatogramTotal >= chromatogramTotal * 0.25)
                    quartileDivisionTimes[0] = timeIntensityPair.Item1;
                else if (!(quartileDivisionTimes[1] > 0) && cumulativeChromatogramTotal >= chromatogramTotal * 0.5)
                    quartileDivisionTimes[1] = timeIntensityPair.Item1;
                else if (!(quartileDivisionTimes[2] > 0) && cumulativeChromatogramTotal >= chromatogramTotal * 0.75)
                    quartileDivisionTimes[2] = timeIntensityPair.Item1;
            }
            return quartileDivisionTimes;
        }

        public Dictionary<string, dynamic> AssembleMetrics()
        {
            List<double> smoothedIntensity = new List<double>();
            for(int i = 0; i < Run.Chromatograms.Ms2Tic.Last().Item1; i += 5)
            {
                double y = Run.Chromatograms.Ms2Tic.Where(x => x.Item1 >= i && x.Item1 < i + 5).Average(x => x.Item2);
                smoothedIntensity.Add(y);



                Console.WriteLine(y);

            }


            return new Dictionary<string, dynamic>
            {
                { "QC:99", Ms1QuartileDivisions },
                { "QC:98", Ms2QuartileDivisions },
                { "QC:97", Run.Chromatograms.Ms1Tic.AsHorizontalArrays() },
                { "QC:96", Run.Chromatograms.Ms2Tic.AsHorizontalArrays() },
                { "QC:95", Run.Chromatograms.Ms1Bpc.AsHorizontalArrays() },
                { "QC:94", Run.Chromatograms.Ms2Bpc.AsHorizontalArrays() },
                { "QC:93", Run.Chromatograms.CombinedTic.AsHorizontalArrays() },
                { "QC:92", Run.Chromatograms.Ms2Tic.Sum(x => x.Item2) / Run.Chromatograms.Ms2Tic.Sum(x => x.Item2) },
                { "QC:91", Run.AnalysisSettings.RunEndTime / 2 - Ms1QuartileDivisions[1] },
                { "QC:90", Run.AnalysisSettings.RunEndTime / 2 - Ms2QuartileDivisions[1] },
                { "QC:89", Run.IRTHits.Average(x => x.AverageMassErrorPpm) },
                { "QC:88", Run.IRTHits.Max(x => x.AverageMassErrorPpm) },
                { "QC:87", Run.IRTHits.Count() / Run.AnalysisSettings.IrtLibrary.PeptideList.Count },
                { "QC:86", Run.IRTHits }

            };
        }
    }
}