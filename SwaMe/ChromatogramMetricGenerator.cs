using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MathNet.Numerics.Interpolation;
using MzmlParser;

namespace SwaMe
{
    class ChromatogramMetricGenerator
    {
        public void GenerateChromatogram(Run run)
        {
            //Interpolation and smoothing to acquire chromatogram metrics
            foreach (BasePeak basepeak in run.BasePeaks)
            {
                double[] intensities = basepeak.Spectrum.Select(x => (double)x.Intensity).ToArray();
                double[] starttimes = basepeak.Spectrum.Select(x => (double)x.RetentionTime).ToArray();

                //if there are less than two datapoints we cannot calculate chromatogrammetrics:
                if (starttimes.Count() < 2)
                    continue;

                //if there are not enough datapoints, interpolate:
                if (starttimes.Count() < 100)
                {
                    RTandInt ri = Interpolate(starttimes, intensities);
                    intensities = ri.intensities;
                    starttimes = ri.starttimes;
                }

                double[,] array3 = new double[1, intensities.Length];
                for (int i = 0; i < intensities.Length; i++)
                {
                    array3[0, i] = intensities[i];
                }

                WaveletLibrary.Matrix dataMatrix = new WaveletLibrary.Matrix(array3);

                var transform = new WaveletLibrary.WaveletTransform(new WaveletLibrary.HaarLift(), 1);
                dataMatrix = transform.DoForward(dataMatrix);

                double[] Smoothedms2bpc = new double[intensities.Length];
                for (int i = 0; i < intensities.Length; i++)
                {
                    Smoothedms2bpc[i] = dataMatrix.toArray[0, i];
                }
                //Find the fwhm:
                double maxIntens = Smoothedms2bpc.Max();
                int mIIndex = Array.IndexOf(Smoothedms2bpc, Smoothedms2bpc.Max());
                double baseline = Smoothedms2bpc.Where(i => i > 0).DefaultIfEmpty(int.MinValue).Min();
                basepeak.FWHM = CalculateFWHM(starttimes, Smoothedms2bpc, maxIntens, mIIndex, baseline);
                double peakTime = starttimes[mIIndex];
                double fwfpct = CalculateFpctHM(starttimes, Smoothedms2bpc, maxIntens, mIIndex, baseline);
                double f = Math.Abs(peakTime - fwfpct);
                basepeak.Peaksym = fwfpct / (2 * f);
                basepeak.PeakCapacity = 1 + (peakTime / basepeak.FWHM);
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
                else if (i == mIIndex && starttimes[i] >= halfMax) { halfRT2 = starttimes[intensities.Length - 1]; break; }
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
    }
}
