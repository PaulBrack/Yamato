using MzmlParser;
using System;
using System.Collections.Generic;
using System.Linq;
using static LibraryParser.Library;

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

        private static double[] ExtractQuartileDivisionSummedIntensities(Run run, double? washTime, int msLevel)
        {
            List<(double, double)> chromatogram = LoadChromatogram(run, msLevel);

            List<Double> quartileDivisionSummedIntensities = new List<double>();

            double runEndTime = washTime ?? run.LastScanTime;

            quartileDivisionSummedIntensities.Add(chromatogram.Where(x => x.Item1 < runEndTime * 0.25).Sum(x => x.Item2));
            quartileDivisionSummedIntensities.Add(chromatogram.Where(x => x.Item1 < runEndTime * 0.5 && x.Item1 >= runEndTime * 0.25).Sum(x => x.Item2));
            quartileDivisionSummedIntensities.Add(chromatogram.Where(x => x.Item1 < runEndTime * 0.75 && x.Item1 >= runEndTime * 0.5).Sum(x => x.Item2));
            quartileDivisionSummedIntensities.Add(chromatogram.Where(x => x.Item1 < runEndTime && x.Item1 >= runEndTime * 0.75).Sum(x => x.Item2));

            //express these as fractions of the whole
            quartileDivisionSummedIntensities.ForEach(x => x = x / chromatogram.Sum(y => y.Item2));

            return quartileDivisionSummedIntensities.ToArray();
        }

        private static double[] ExtractQuartileDivisionTimes(Run run, double? washTime, int msLevel)
        {
            List<(double, double)> chromatogram = LoadChromatogram(run, msLevel);

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

        private static List<(double, double)> LoadChromatogram(Run run, int msLevel)
        {
            List<(double, double)> chromatogram;

            if (msLevel == 1)
                chromatogram = run.Chromatograms.Ms1Tic;
            else if (msLevel == 2)
                chromatogram = run.Chromatograms.Ms2Tic;
            else
                throw new ArgumentOutOfRangeException("MS Level must be 1 or 2");
            return chromatogram;
        }

        public Dictionary<string, dynamic> AssembleMetrics()
        {
            List<double> smoothedIntensity = new List<double>();
            for (int i = 0; i < Run.Chromatograms.Ms2Tic.Last().Item1; i += 5)
            {
                double y = Run.Chromatograms.Ms2Tic.Where(x => x.Item1 >= i && x.Item1 < i + 5).Average(x => x.Item2);
                smoothedIntensity.Add(y);
                Console.WriteLine(y);
            }

            var metrics = new Dictionary<string, dynamic>
            {
                { "QC:99", Ms1QuartileDivisions },
                { "QC:98", Ms2QuartileDivisions },
                { "QC:97", Run.Chromatograms.Ms1Tic.AsHorizontalArrays() },
                { "QC:96", Run.Chromatograms.Ms2Tic.AsHorizontalArrays() },
                { "QC:95", Run.Chromatograms.Ms1Bpc.AsHorizontalArrays() },
                { "QC:94", Run.Chromatograms.Ms2Bpc.AsHorizontalArrays() },
                { "QC:93", Run.Chromatograms.CombinedTic.AsHorizontalArrays() },
                { "QC:92", Run.Chromatograms.Ms1Tic.Sum(x => x.Item2) / Run.Chromatograms.Ms2Tic.Sum(x => x.Item2) },
                { "QC:91", Run.AnalysisSettings.RunEndTime / 2 - Ms1QuartileDivisions[1] },
                { "QC:90", Run.AnalysisSettings.RunEndTime / 2 - Ms2QuartileDivisions[1] },
                { "QC:83", ExtractQuartileDivisionSummedIntensities(Run, Run.AnalysisSettings.RunEndTime, 1) },
                { "QC:82", ExtractQuartileDivisionSummedIntensities(Run, Run.AnalysisSettings.RunEndTime, 2) }
            };

            if (Run.AnalysisSettings.IrtLibrary != null && Run.IRTHits.Count > 0)
            {
                var libraryPeptides = Run.AnalysisSettings.IrtLibrary.PeptideList.Values.Cast<Peptide>().OrderBy(x => x.RetentionTime);
                var orderedIrtPeptideSequences = libraryPeptides.Select(x => x.Sequence).ToList();
                var orderedIrtHits = Run.IRTHits.OrderBy(x => orderedIrtPeptideSequences.IndexOf(x.PeptideSequence));

                metrics.Add("QC:89", Run.IRTHits.Average(x => x.AverageMassErrorPpm));
                metrics.Add("QC:88", Run.IRTHits.Max(x => x.AverageMassErrorPpm));
                metrics.Add("QC:87", Run.IRTHits.Count() / Run.AnalysisSettings.IrtLibrary.PeptideList.Count);
                metrics.Add("QC:86", orderedIrtHits);
                metrics.Add("QC:85", Convert.ToDouble(Run.IRTHits.Count()) / Convert.ToDouble(Run.AnalysisSettings.IrtLibrary.PeptideList.Count));
                metrics.Add("QC:84", Run.IRTHits.Select(x=> x.RetentionTime).Max() - Run.IRTHits.Select(x => x.RetentionTime).Min());
                metrics.Add("QC:81", GetOrderednessAsPercent(orderedIrtHits.Select(x => x.RetentionTime).ToArray()));
            }
           
            return metrics;
        }

        static double GetOrderednessAsPercent(double[] arr)
        {
            return (1 - (GetInversionCount(arr) / GetTriangularNumber(arr))) * 100;
        }

        //adapted from: https://www.geeksforgeeks.org/csharp-program-for-count-inversions-in-an-array-set-1-using-merge-sort/
        static int GetInversionCount(double[] arr)
        {
            int inv_count = 0;

            for (int i = 0; i < arr.Count() - 1; i++)
                for (int j = i + 1; j < arr.Count(); j++)
                    if (arr[i] > arr[j])
                        inv_count++;

            return inv_count;
        }

        //adapted from: https://www.geeksforgeeks.org/program-print-triangular-number-series-till-n/
        static double GetTriangularNumber(double[] arr)
        {
            int n = arr.Count();
            int i, j = 1, k = 1;
            double result = 0;
            for (i = 1; i <= n; i++)
            {
                j += 1; 
                k += j; 
                if (i == n)
                    result = k;
            }
            return result;
        }
    }
}