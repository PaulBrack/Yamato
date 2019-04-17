using MzmlParser;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO;

namespace SwaMe
{
    public class MetricGenerator
    {
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
              

                List<double> intensityList = new List<double>();
                List<double> starttimesList = new List<double>();
                //if there are not enough datapoints, interpolate:
                if (starttimes.Count() < 100)
                {
                    //First step is interpolating a spline, then choosing the pointsat which to add values:
                    MathNet.Numerics.Interpolation.NevillePolynomialInterpolation interpolation = MathNet.Numerics.Interpolation.NevillePolynomialInterpolation.InterpolateSorted(starttimes, intensities);
                    //Check to see the original datapoints are still there:
                    FitsAtSamplePoints(intensities, interpolation);

                    double stimesinterval = starttimes.Last() - starttimes[0];
                    int numNeeded = 100 - starttimes.Count();
                    double intervals = stimesinterval / numNeeded;
                    intensityList = intensities.OfType<double>().ToList();
                    starttimesList = starttimes.OfType<double>().ToList();
                    for (int uuu = 0; uuu < numNeeded; uuu++)
                    {
                        double placetobe = starttimes[0] + (intervals * uuu);
                        
                        //insert newIntensity into the correct spot in the array
                        for (int yyy = 0; yyy < 100; yyy++)
                        {
                            if (starttimesList[yyy] < placetobe) { continue; }
                            else
                            {
                                
                                if (yyy > 0)
                                {
                                    if (starttimesList[yyy] == placetobe) { placetobe = placetobe + 0.01; }
                                    double newIntensity = interpolation.Interpolate(placetobe);
                                    intensityList.Insert(yyy, newIntensity);
                                    starttimesList.Insert(yyy , placetobe);
                                }
                                else {
                                    if (starttimesList[yyy] == placetobe) { placetobe = placetobe - 0.01; }
                                    double newIntensity = interpolation.Interpolate(placetobe);
                                    intensityList.Insert(yyy, newIntensity);
                                    starttimesList.Insert(yyy, placetobe);
                                }
                                
                                break;
                                /*double[] newintensities = new double[starttimes.Count()+1];
                                myArrSegMid2.CopyTo(z, yyy+1);*/
                            }
                        }
                    }

                    intensities = intensityList.Select(item => Convert.ToDouble(item)).ToArray();
                    starttimes = starttimesList.Select(item => Convert.ToDouble(item)).ToArray();
                }

                double[,] array3 = new double[1,intensities.Length];
                for (int aaa=0;aaa<intensities.Length; aaa++)
                {
                    array3[0, aaa] = intensities[aaa];
                }
                  
                WaveletLibrary.Matrix dataMatrix = new WaveletLibrary.Matrix(array3);
                
                
                Console.WriteLine("Setup wavelet transform...");
                var transform = new WaveletLibrary.WaveletTransform(new WaveletLibrary.HaarLift(), 1);
                Console.WriteLine("Wavelet transform...");
                dataMatrix = transform.DoForward(dataMatrix);

                // intensities = dataMatrix[0];                //sgSmooth smooth = new sgSmooth { };
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
         

    }
}
