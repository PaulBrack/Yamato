using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Interpolation;
using MzmlParser;
using NLog;

namespace SwaMe
{
    class ChromatogramMetricGenerator
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void GenerateChromatogram(Run run)
        {
            //Crawdad
            foreach (BasePeak basepeak in run.BasePeaks)
            {
                //for each peak within the spectrum 
                for (int yyy = 0; yyy < basepeak.BpkRTs.Count(); yyy++)
                {
                    //change these two arrays to be only the spectra surrounding that bpkrt:
                    double[] intensities = basepeak.Spectrum.Where(x => Math.Abs(x.RetentionTime - basepeak.BpkRTs[yyy]) < run.AnalysisSettings.RtTolerance).Select(x => (double)x.Intensity).ToArray();
                    double[] starttimes = basepeak.Spectrum.Where(x => Math.Abs(x.RetentionTime - basepeak.BpkRTs[yyy]) < run.AnalysisSettings.RtTolerance).Select(x => (double)x.RetentionTime).ToArray();
                    if (intensities.Count() > 1)
                    {
                        RTandInt inter = new RTandInt();
                        inter = Interpolate(starttimes, intensities);
                        starttimes = inter.starttimes;
                        intensities = inter.intensities;
                    }
                    CrawdadSharp.CrawdadPeakFinder cPF = new CrawdadSharp.CrawdadPeakFinder();
                    cPF.SetChromatogram(starttimes, intensities);
                    List<CrawdadSharp.CrawdadPeak> crawPeaks = cPF.CalcPeaks();
                    double totalFwhm = 0;
                    double totalPeakSym = 0;
                    double totalBaseWidth = 0;
                    foreach (CrawdadSharp.CrawdadPeak crawPeak in crawPeaks)
                    {
                        double peakTime = starttimes[crawPeak.TimeIndex];
                        double fwhm = crawPeak.Fwhm;
                        double fvalue = crawPeak.Fvalue;
                        if (fwhm == 0)
                        {
                            continue;
                        }
                        else if (fvalue != 0)
                        {
                            totalPeakSym += crawPeak.Fwfpct / (2 * crawPeak.Fvalue); //From USP31: General Chapters <621> Chromatography equation for calculating the tailing factor(Available at: http://www.uspbpep.com/usp31/v31261/usp31nf26s1_c621.asp). A high value means that the peak is highly asymmetrical.
                        }
                        totalFwhm += fwhm;
                        if (!float.IsNaN(crawPeak.FwBaseline)) totalBaseWidth += crawPeak.FwBaseline; 
                        else
                            totalBaseWidth += crawPeak.Fwfpct;
                        

                    }
                    if (crawPeaks.Count() > 0)
                    {
                        basepeak.FWHMs.Add(totalFwhm / crawPeaks.Count());
                        basepeak.Peaksyms.Add(totalPeakSym / crawPeaks.Count());
                        basepeak.FullWidthBaselines.Add(totalBaseWidth / crawPeaks.Count());
                    }
                    else
                    {
                        basepeak.FWHMs.Add(0);
                        basepeak.FullWidthBaselines.Add(0);
                        basepeak.Peaksyms.Add(0);
                    }
                }
            }
        }

        public void GenerateiRTChromatogram(Run run)
        {
            int Count = 0;
            foreach (IRTPeak irtpeak in run.IRTPeaks)
            {
                int length = 0;
                double highestScore = 0;
                int rank = 1;

                irtpeak.PossPeaks = irtpeak.PossPeaks.OrderByDescending(x => x.BasePeak.Intensity).ToList();
                foreach (PossiblePeak pPeak in irtpeak.PossPeaks)
                {

                    Dotproduct dp = new Dotproduct();
                    SmoothedPeak sp = new SmoothedPeak();
                    List<double[]> transSmoothInt = new List<double[]>();
                    int peakPosition = 0;
                    for (int i = 0; i < pPeak.Alltransitions.Count() - 1; i++)
                    {
                        if (pPeak.Alltransitions[i].Count() != 0)
                        {
                            var transitionSpectrum = pPeak.Alltransitions[i];
                            double[] intensities = transitionSpectrum.OrderBy(x => x.RetentionTime).Select(x => (double)x.Intensity).ToArray();
                            double[] starttimes = transitionSpectrum.OrderBy(x => x.RetentionTime).Select(x => (double)x.RetentionTime).ToArray();
                            length = intensities.Count();

                            //if there are less than two datapoints we cannot calculate chromatogrammetrics:
                            if (starttimes.Count() < 2)
                                continue;

                            double[,] intensitiesArray = new double[1, intensities.Length];

                            for (int j = 0; j < intensities.Length; j++)
                                intensitiesArray[0, j] = intensities[j];

                            WaveletLibrary.Matrix dataMatrix = new WaveletLibrary.Matrix(intensitiesArray);

                            var transform = new WaveletLibrary.WaveletTransform(new WaveletLibrary.HaarLift(), 1);
                            dataMatrix = transform.DoForward(dataMatrix);

                            double[] Smoothed = new double[intensities.Length];
                            for (int j = 0; j < intensities.Length; j++)
                                Smoothed[j] = dataMatrix.toArray[0, j];

                            transSmoothInt.Add(Smoothed);

                            if (i == 0)
                                peakPosition = Array.IndexOf(intensities, intensities.Max());

                            //Calculate all the parameters for dotproducts that need to be summed accross transitions:
                            dp = CalcDotProductParameters(i, irtpeak, dp, intensities, peakPosition);
                            //Collect the peak metrics for each transition, which will then be averaged out for all transitions accross a possible peak
                            sp = ProducePeakMetrics(starttimes, intensities, irtpeak, sp);
                        }
                        else
                        {
                            transSmoothInt.Add(null);//just to maintain the order in future analyses
                        }

                    }

                    sp.DotProduct = Math.Pow(dp.TiriPair, 2) / (dp.Tsquared * dp.Rsquared);
                    double RTscore = CalcRTScore(Count, run, sp);
                    double score = 0.4 * sp.DotProduct + 0.01 * pPeak.Alltransitions[0].Count() / (rank / 10) + 0.4 * RTscore; //ALgorithm changed so score penalises less for a lower rough peak area estimation (rank here is used as a substitute for intensity) and puts a higher value on dotproduct
                    if (highestScore < score)
                    {
                        highestScore = score;
                        irtpeak.RetentionTime = sp.RT;
                        irtpeak.FWHM = sp.FWHMAllTransitions / irtpeak.AssociatedTransitions.Count();//average fwhm
                        irtpeak.Peaksym = sp.PSAllTransitions / irtpeak.AssociatedTransitions.Count();
                    }

                    rank++;//Because we have ordered the possible peaks according to their intensities, this value corresponds to their intensity rank 
                }
                Count++;
            }
        }

        public double CalculateFWHM(double[] starttimes, double[] intensities, double maxIntens, int mIIndex, double baseline)
        {
            double halfMax = (maxIntens - baseline) / 2 + baseline;
            double halfRT1 = 0;
            double halfRT2 = 0;
            for (int i = mIIndex; i > 0; i--)
            {
                if (intensities[i] < halfMax)
                {
                    halfRT1 = starttimes[i];
                    break;
                }
                else if (i == 0 && starttimes[i] >= halfMax) { halfRT2 = starttimes[0]; break; }
            }
            for (int i = mIIndex; i < intensities.Length; i++)
            {
                if (intensities[i] < halfMax)
                {
                    halfRT2 = starttimes[i];
                    break;
                }
                else if (i == mIIndex && starttimes[i] >= halfMax)
                {
                    halfRT2 = starttimes[intensities.Length - 1];
                    break;
                }
            }
            return halfRT2 - halfRT1;
        }

        public double CalculateFpctHM(double[] starttimes, double[] intensities, double maxIntens, int mIIndex, double baseline)
        {
            double fiveMax = (maxIntens - baseline) / 20 + baseline;
            double fiveRT1 = 0;
            double fiveRT2 = 0;
            for (int i = mIIndex; i > 0; i--)
            {
                if (intensities[i] < fiveMax)
                {
                    fiveRT1 = starttimes[i];
                    break;
                }
                else if (i == 0 && starttimes[i] >= fiveMax) { fiveRT2 = starttimes[0]; }
            }

            for (int i = mIIndex; i < intensities.Length; i++)
            {
                if (intensities[i] < fiveMax)
                {
                    fiveRT2 = starttimes[i];
                    break;
                }
                else if (i == mIIndex && starttimes[i] >= fiveMax) { fiveRT2 = starttimes[intensities.Length - 1]; }
            }
            return fiveRT2 - fiveRT1;
        }

        private RTandInt Interpolate(double[] starttimes, double[] intensities)
        {
            List<double> intensityList = new List<double>();
            List<double> starttimesList = new List<double>();

            //First step is interpolating a spline, then choosing the pointsat which to add values:
            CubicSpline interpolation = CubicSpline.InterpolateNatural(starttimes, intensities);

            //Now we work out how many to add so we reach at least 100 datapoints and add them:
            double stimesinterval = starttimes.Last() - starttimes[0];
            int numNeeded = 100 - starttimes.Count();
            double intervals = stimesinterval / numNeeded;
            intensityList = intensities.OfType<double>().ToList();
            starttimesList = starttimes.OfType<double>().ToList();
            for (int i = 0; i < numNeeded; i++)
            {
                double placetobe = starttimes[0] + (intervals * i);

                //insert newIntensity into the correct spot in the array
                for (int currentintensity = 0; currentintensity < 100; currentintensity++)
                {
                    if (starttimesList[currentintensity] < placetobe) { continue; }
                    else
                    {
                        if (currentintensity > 0)
                        {
                            if (starttimesList[currentintensity] == placetobe) { placetobe = placetobe + 0.01; }
                            double newIntensity = interpolation.Interpolate(placetobe);
                            intensityList.Insert(currentintensity, newIntensity);
                            starttimesList.Insert(currentintensity, placetobe);
                        }
                        else
                        {
                            if (starttimesList[currentintensity] == placetobe) { placetobe = placetobe - 0.01; }
                            double newIntensity = interpolation.Interpolate(placetobe);
                            intensityList.Insert(currentintensity, newIntensity);
                            starttimesList.Insert(currentintensity, placetobe);
                        }
                        break;
                    }
                }
            }
            RTandInt ri = new RTandInt();
            ri.intensities = intensityList.Select(item => Convert.ToDouble(item)).ToArray();
            ri.starttimes = starttimesList.Select(item => Convert.ToDouble(item)).ToArray();
            return ri;
        }

        public struct RTandInt
        {
            public double[] starttimes;
            public double[] intensities;
        }
        public SmoothedPeak ProducePeakMetrics(double[] starttimes, double[] Smoothed, IRTPeak irtpeak, SmoothedPeak sp)
        {
            int mIIndex = Array.IndexOf(Smoothed, Smoothed.Max());
            double height = Smoothed.Max();
            double width = starttimes.Max() - starttimes.Min();
            sp.peakArea = height * width / 2;
            sp.RT = starttimes[mIIndex];
            double baseline = Smoothed.Where(i => i > 0).DefaultIfEmpty(int.MinValue).Min();
            sp.FWHMAllTransitions += CalculateFWHM(starttimes, Smoothed, Smoothed.Max(), mIIndex, baseline);
            double tempFWfpt = CalculateFpctHM(starttimes, Smoothed, Smoothed.Max(), mIIndex, baseline);
            double f = Math.Abs(irtpeak.RetentionTime - tempFWfpt);
            sp.PSAllTransitions += tempFWfpt / (2 * f);
            return sp;
        }
        public double CalcRTScore(int Count, Run run, SmoothedPeak sp)
        {
            double RTscore = 0;
            if (Count > 0 && Count < run.IRTPeaks.Count() - 1 && run.IRTPeaks.Max(x => x.RetentionTime) < sp.RT)//if this is not the peaks for the first iRT peptide, we give the peak an RTscore based on whether it is sequential to the last iRT peptide or not
                RTscore = 1;
            else if (Count == 0 && sp.RT != 0)//if this is the first irtpeptide, we want to score its RT based on its proximity to the beginning of the run
                RTscore = 1 / Math.Pow(sp.RT - 0.227, 2);//The power and times ten calculation was added to penalize a peptide greatly for occurring later in the RT. Due to our not wanting to hardcode any peptide standard RTs, we would like to keep the order that the peptides are presented. Therefore, the first peptide should rather occur too early than too late.
            return RTscore;
        }
        public Dotproduct CalcDotProductParameters(int i, IRTPeak irtpeak, Dotproduct dp, double[] intensities, int peakPosition)
        {
            double Ri = irtpeak.AssociatedTransitions[i].ProductIonIntensity;
            double Ti = 0;
            if (intensities.Count() > i)
            {
                if (intensities.Count() > peakPosition)
                {
                    Ti = intensities[peakPosition];
                }
                else
                {
                    Ti = 0;
                }
            }
            dp.TiriPair += Ti * Ri;
            dp.Tsquared += Ti * Ti;
            dp.Rsquared += Ri * Ri;
            dp.Tisum += Ti;
            return dp;
        }
    }
}
