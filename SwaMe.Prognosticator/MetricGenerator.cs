#nullable enable

using SwaMe.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using static LibraryParser.Library;

namespace Prognosticator
{
    public class MetricGenerator
    {
        public double[]? Ms1QuartileDivisions { get; private set; }
        public double[]? Ms2QuartileDivisions { get; private set; }
        public Run<Scan> Run { get; }

        public MetricGenerator(Run<Scan> run)
        {
            Run = run;
        }

        public IList<IMzqcMetrics> GenerateMetrics()
        {
            ChromatogramGenerator.CreateAllChromatograms(Run);
            Ms1QuartileDivisions = ExtractQuartileDivisionTimes(Run, Run.LastScanTime, 1);
            Ms2QuartileDivisions = ExtractQuartileDivisionTimes(Run, Run.LastScanTime, 2);

            return AssembleMetrics();
        }

        private static double[]? ExtractQuartileDivisionSummedIntensities(Run<Scan> run, double? washTime, int msLevel)
        {
            List<(double, double)> chromatogram = LoadChromatogram(run, msLevel);


            double? runEndTime = washTime ?? run.LastScanTime;

            var total = chromatogram.Sum(x => x.Item2);
            double[] quartileDivisionSummedIntensities = new double[]
            {
                chromatogram.Where(x => x.Item1 < runEndTime * 0.25).Sum(x => x.Item2),
                chromatogram.Where(x => x.Item1 < runEndTime * 0.5 && x.Item1 >= runEndTime * 0.25).Sum(x => x.Item2),
                chromatogram.Where(x => x.Item1 < runEndTime * 0.75 && x.Item1 >= runEndTime * 0.5).Sum(x => x.Item2),
                chromatogram.Where(x => x.Item1 < runEndTime && x.Item1 >= runEndTime * 0.75).Sum(x => x.Item2)
            };

            //express these as fractions of the whole
            return quartileDivisionSummedIntensities.Select(x => x / total).ToArray();
        }

        private static double[] ExtractQuartileDivisionTimes(Run<Scan> run, double? washTime, int msLevel)
        {
            List<(double, double)> chromatogram = LoadChromatogram(run, msLevel);

            double chromatogramTotal = 0;
            if (washTime == 0 || washTime == null)
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

        private static List<(double, double)> LoadChromatogram(Run<Scan> run, int msLevel)
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

        private IList<IMzqcMetrics> AssembleMetrics()
        {
            IMzqcMetrics baseMetrics = new BaseMetrics()
            {
                MS1TICQuartiles = Ms1QuartileDivisions,
                MS2TICQuartiles = Ms2QuartileDivisions,
                MS1TIC = Run.Chromatograms.Ms1Tic.AsHorizontalArrays(),
                MS2TIC = Run.Chromatograms.Ms2Tic.AsHorizontalArrays(),
                MS1BPC = Run.Chromatograms.Ms1Bpc.AsHorizontalArrays(),
                MS2BPC = Run.Chromatograms.Ms2Bpc.AsHorizontalArrays(),
                CombinedTIC = Run.Chromatograms.CombinedTic.AsHorizontalArrays(),
                MS1MS2Ratio = Run.Chromatograms.Ms1Tic.Sum(x => x.Item2) / Run.Chromatograms.Ms2Tic.Sum(x => x.Item2),
                MS1WeightedMedianSkew = Run.LastScanTime / 2 - Ms1QuartileDivisions[1],
                MS2WeightedMedianSkew = Run.LastScanTime / 2 - Ms2QuartileDivisions[1],
                MS1TICQuartilesByRT = ExtractQuartileDivisionSummedIntensities(Run,Run.LastScanTime, 1),
                MS2TICQuartilesByRT = ExtractQuartileDivisionSummedIntensities(Run, Run.LastScanTime, 2)
            };

            IList<IMzqcMetrics> metrics = new List<IMzqcMetrics> { baseMetrics };

            if (Run.AnalysisSettings.IrtLibrary != null && Run.IRTHits.Count > 0)
            {
                var libraryPeptides = Run.AnalysisSettings.IrtLibrary.PeptideList.Values.Cast<Peptide>().OrderBy(x => x.RetentionTime);
                var orderedIrtPeptideSequences = libraryPeptides.Select(x => x.Sequence).ToList();
                var orderedIrtHits = Run.IRTHits.OrderBy(x => orderedIrtPeptideSequences.IndexOf(x.PeptideSequence));

                IMzqcMetrics irtMetrics = new IrtMetrics()
                {
                    MeanIrtMassError = Run.IRTHits.Average(x => x.AverageMassErrorPpm),
                    MaxIrtMassError = Run.IRTHits.Max(x => x.AverageMassErrorPpm),
                    IrtPeptideFoundProportion = Run.IRTHits.Count() / Run.AnalysisSettings.IrtLibrary.PeptideList.Count,
                    IrtPeptides = orderedIrtHits.ToList(),
                    IrtPeptidesFound = Run.IRTHits.Count / (double)Run.AnalysisSettings.IrtLibrary.PeptideList.Count,
                    IrtSpread = Run.IRTHits.Select(x => x.RetentionTime).Max() - Run.IRTHits.Select(x => x.RetentionTime).Min(),
                    IrtOrderedness = GetOrderednessAsPercent(orderedIrtHits.Select(x => x.RetentionTime).ToArray())
                };
                metrics.Add(irtMetrics);
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
            int j = 1, k = 1;
            double result = 0;
            for (int i = 1; i <= n; i++)
            {
                j += 1; 
                k += j; 
                if (i == n)
                    result = k;
            }
            return result;
        }
    }

    public class BaseMetrics : IMzqcMetrics
    {
        public Run<Scan> Run { get; set; }

        public double[] MS1TICQuartiles { get; set; }
        public double[] MS2TICQuartiles { get; set; }
        public double[][] MS1TIC { get; set; }
        public double[][] MS2TIC { get; set; }
        public double[][] MS1BPC { get; set; }
        public double[][] MS2BPC { get; set; }
        public double[][] CombinedTIC { get; set; }
        public double MS1MS2Ratio { get; set; }
        public double? MS1WeightedMedianSkew { get; set; }
        public double? MS2WeightedMedianSkew { get; set; }
        public double[] MS1TICQuartilesByRT { get; set; }
        public double[] MS2TICQuartilesByRT { get; set; }

        public IDictionary<string, dynamic> RenderableMetrics =>
            new Dictionary<string, dynamic>()
            {
                { "QC:99", MS1TICQuartiles },
                { "QC:98", MS2TICQuartiles },
                { "QC:97", MS1TIC },
                { "QC:96", MS2TIC },
                { "QC:95", MS1BPC },
                { "QC:94", MS2BPC },
                { "QC:93", CombinedTIC },
                { "QC:92", MS1MS2Ratio },
                { "QC:91", MS1WeightedMedianSkew },
                { "QC:90", MS2WeightedMedianSkew },
                { "QC:83", MS1TICQuartilesByRT },
                { "QC:82", MS2TICQuartilesByRT }
            };
    }

    public class IrtMetrics : IMzqcMetrics
    {
        public double MeanIrtMassError { get; set; }
        public double MaxIrtMassError { get; set; }
        public double IrtPeptideFoundProportion { get; set; }
        public IList<CandidateHit> IrtPeptides { get; set; }
        public double IrtPeptidesFound { get; set; }
        public double IrtSpread { get; set; }
        public double IrtOrderedness { get; set; }


        public IDictionary<string, dynamic> RenderableMetrics
        {
            get
            {
                return new Dictionary<string, dynamic>()
                {
                    { "QC:89", MeanIrtMassError },
                    { "QC:88", MaxIrtMassError },
                    { "QC:87", IrtPeptideFoundProportion },
                    { "QC:86", IrtPeptides },
                    { "QC:85", IrtPeptidesFound },
                    { "QC:84", IrtSpread },
                    { "QC:81", IrtOrderedness }
                };
            }
        }
    }
}