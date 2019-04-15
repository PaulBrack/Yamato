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
                double maxIntens = intensities.Max();
                int mIIndex = System.Array.BinarySearch(intensities, maxIntens);
                double baseline = intensities.Min();

                List<double> intensityList = new List<double>();
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
                    for (int uuu = 0; uuu < numNeeded; uuu++)
                    {
                        double placetobe = intensityList[0] + (intervals * uuu);
                        double newIntensity = interpolation.Interpolate(placetobe);
                        //insert newIntensity into the correct spot in the array
                        for (int yyy = 0; yyy < intensityList.Count(); yyy++)
                        {
                            if (starttimes[yyy] < placetobe) { continue; }
                            else
                            {

                                intensityList.Insert(yyy, newIntensity);

                                /*double[] newintensities = new double[starttimes.Count()+1];
                                myArrSegMid2.CopyTo(z, yyy+1);*/
                            }
                        }
                    }

                    intensities = intensityList.Select(item => Convert.ToDouble(item)).ToArray();

                }
                sgSmooth smooth = new sgSmooth { };
                double[] Smoothedms2bpc = smooth.sg_smooth(intensities, 3, 2);
                //Find the fwhm:
                 ChromatogramMetrics cm = new ChromatogramMetrics { };
                 double FWHM = cm.CalculateFWHM(Smoothedms2bpc, starttimes, maxIntens,mIIndex, baseline);
                 double FpctHM = cm.CalculateFpctHM(Smoothedms2bpc, starttimes, maxIntens, mIIndex, baseline);
            }
        }
    public void FitsAtSamplePoints(double[] intensities, MathNet.Numerics.Interpolation.NevillePolynomialInterpolation interpolation)
        {
            for (int i = 0; i < intensities.Length; i++)
            {
                //Debug.Assert(intensities[i] == interpolation.Interpolate(intensities[i]));
            }
        }
    }
}
