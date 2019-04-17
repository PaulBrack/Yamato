using MzmlParser;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using MathNet.Numerics.Interpolation;

namespace SwaMe
{
    public class MetricGenerator
    {
        public struct RTandInt
        {
            public double[] starttimes;
            public double[] intensities;
        }

        public void GenerateMetrics(Run run)
        {
            
            foreach (BasePeak basepeak in run.BasePeaks)
            {
                double[] intensities = new double[basepeak.Spectrum.Count()];
                double[] starttimes = new double[basepeak.Spectrum.Count()];
                for (int iii = 0; iii< basepeak.Spectrum.Count(); iii++)
                {
                    intensities[iii] = basepeak.Spectrum[iii].Intensity;
                    starttimes[iii] = basepeak.Spectrum[iii].RetentionTime;
                }

                //if there are less than two datapoints we cannot calculate chromatogrammetrics:
                if (starttimes.Count() < 2) { continue; }
               
                //if there are not enough datapoints, interpolate:
                if (starttimes.Count() < 100)
                {
                    RTandInt ri = Interpolate(starttimes, intensities);
                    intensities = ri.intensities;
                    starttimes = ri.starttimes;
                }

                double[,] array3 = new double[1,intensities.Length];
                for (int aaa=0;aaa<intensities.Length; aaa++)
                {
                    array3[0, aaa] = intensities[aaa];
                }
                  
                WaveletLibrary.Matrix dataMatrix = new WaveletLibrary.Matrix(array3);
                
                
                var transform = new WaveletLibrary.WaveletTransform(new WaveletLibrary.HaarLift(), 1);
                dataMatrix = transform.DoForward(dataMatrix);

                double[] Smoothedms2bpc = new double[intensities.Length];
                for (int aaa = 0; aaa < intensities.Length; aaa++)
                {
                    Smoothedms2bpc[aaa] = dataMatrix.toArray[0,aaa];
                }
                //Find the fwhm:
                double maxIntens = Smoothedms2bpc.Max();
                int mIIndex = Array.IndexOf(Smoothedms2bpc, Smoothedms2bpc.Max());
                double baseline = Smoothedms2bpc.Where(i => i > 0).DefaultIfEmpty(int.MinValue).Min();
                ChromatogramMetrics cm = new ChromatogramMetrics { };
                double FWHM = cm.CalculateFWHM( starttimes, Smoothedms2bpc, maxIntens, mIIndex, baseline);
                double FpctHM = cm.CalculateFpctHM( starttimes, Smoothedms2bpc, maxIntens, mIIndex, baseline);
            }
        }

        private RTandInt Interpolate(double[] starttimes,double [] intensities)
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
            for (int uuu = 0; uuu < numNeeded; uuu++)
            {
                double placetobe = starttimes[0] + (intervals * uuu);

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

    }
}
