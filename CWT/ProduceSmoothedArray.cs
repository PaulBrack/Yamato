using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MathNet.Numerics.Interpolation;


namespace CWT
{
    public class ProduceSmoothedArray
    {
        private static readonly Object Lock = new Object();
        /* public double[] ProduceSmoothedChromatogram(double[] y, double[] x)
         {

                     List<double> Smoothedms2bpc = new List<double>();
                     if (x.Count() < 2) Smoothedms2bpc = null;//if there are less than two datapoints we cannot calculate chromatogrammetrics:

                     //if there are not enough datapoints, interpolate:
                     if (x.Count() < 100)
                     {
                         RTandInt ri = Interpolate(x, y);
                         y = ri.intensities;
                         x = ri.starttimes;
                     }

                     double[,] array3 = new double[1, y.Count()];
                     for (int i = 0; i < y.Count(); i++)
                     {
                         array3[0, i] = y[i];
                     }

                     WaveletLibrary.Matrix dataMatrix = new WaveletLibrary.Matrix(array3);

                     var transform = new WaveletLibrary.WaveletTransform(new WaveletLibrary.HaarLift(), 1);
                     dataMatrix = transform.DoForward(dataMatrix);

                     for (int i = 0; i < y.Count() - 1; i++)
                     {
                         Smoothedms2bpc.Add( dataMatrix.toArray[0, i]);
                     }
                     return Smoothedms2bpc.ToArray();

         }*/
        public double[] ProduceSmoothedChromatogramWOInterpolation(double[] y, double[] x)
        {

            List<double> Smoothedms2bpc = new List<double>();
            if (x.Count() < 2)
            {
                Smoothedms2bpc = null;
                return y;
            }//if there are less than two datapoints we cannot smooth
            else
            {
                double[,] array3 = new double[1, y.Count()];
                for (int i = 0; i < y.Count(); i++)
                {
                    array3[0, i] = y[i];
                }

                WaveletLibrary.Matrix dataMatrix = new WaveletLibrary.Matrix(array3);

                var transform = new WaveletLibrary.WaveletTransform(new WaveletLibrary.HaarLift(), 1);
                dataMatrix = transform.DoForward(dataMatrix);

                for (int i = 0; i < y.Count() - 1; i++)
                {
                    Smoothedms2bpc.Add(dataMatrix.toArray[0, i]);
                }
                return Smoothedms2bpc.ToArray();
            }

        }



        public RTandInt Interpolate(double[] starttimes, double[] intensities)
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

        [DllImport("Crawdad.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetChromatogram(double[] times, double[] intensities);
        /* [DllImport("Crawdad.dll", CallingConvention = CallingConvention.Cdecl)]
         public static extern List<CrawPeak> CalcPeaks();
         [DllImport("Crawdad.dll", CallingConvention = CallingConvention.Cdecl)]

         [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
         public struct SlimCrawPeak
         {

             public List<float> intensities;
             public List<float> background_vals;
             public float mean_above_baseline;
             public float max_above_baseline;
             public float stddev_mean_above_baseline;
             public int baseline_p_mean_crossing_cnt;

             CrawPeak()
             {
                 //sup_method = method;
                 start_rt_idx = stop_rt_idx = peak_rt_idx = mz_idx = -1;
                 init();
             }
         }

         class CrawPeak : public SlimCrawPeak {


         ///constructor with peak location. start, stop, and peak index locations


         ///Constructor taking start,stop,peak,mz indices, and a vector of intensities
         CrawPeak(int start_idx, int stop_idx, int peak_idx,

        const std::vector<float> & raw, std::vector<float> & scratch ,int mz_idx = -1 );

         virtual void init();


         //TODO -- split this into a simpler approach
         void calc_baseline_stats();

         ///sharpness == area / len
         float get_area_sharpness() const {
     return peak_area / len;
   }
     ///height / len
     float get_height_sharpness() const {
     return peak_height / len;
   }

 int get_baseline_p_mean_crossing() const {
     return baseline_p_mean_crossing_cnt;
   }



   void calc_CV();


 ///extracts data corresponding to the peak's co-ordinates from a float vector to a target vector
 void extract_chrom_regions( const std::vector<float> & chrom, std::vector<float> & target );

 ///calculates slope of the background level as estimated from peak boundaries


 ///calculates nearness of peak location to peak edges
 float assymmetry_stab() const;


 ///returns peak index as a measure of scans from the leftmost boundary

 ///internal method for calclating peak height

 virtual std::string as_string_header() const;

 std::string as_string_long_header() const;
 std::string as_string_long() const;
 std::string internal_as_string_long() const;

   ///returns peak to background ratio

 };*/



        public void CrawDad(double[] starttimes, double[] intensities)
        {


            SetChromatogram(starttimes, intensities);
        }
        /* List<SpectrumPoint> crawPeaks = CalcPeaks();

         foreach(const CrawdadPeakPtr&crawPeak, crawPeaks)
                     {

             //for (int iii = 0; iii < crawPeaks.size(); iii++)
             //{

             //double startTime = basepeak.ms2chromatogram.MS2RT[crawPeak->getStartIndex()];
             //double endTime = basepeak.ms2chromatogram.MS2RT[crawPeak->getEndIndex()];
             double peakTime = currentbasepeak.ms2chromatogram.MS2RT[crawPeak->getTimeIndex()];
             //|| startTime == peakTime || peakTime == endTime

             // skip degenerate peaks
             fwhm = crawPeak->getFwhm();
             if (fwhm == 0 || boost::math::isnan(fwhm))
             {
                 cout << "FWHM is zero. ";
                 continue;
             }

             // Crawdad Fwhm is in index units; we have to translate it back to time units
             //double sampleRate = (endTime - startTime) / (&crawPeaks[iii]->getEndIndex() - &crawPeaks[iii]->getStartIndex());
             //Peak peak(startTime, endTime, peakTime, fwhm, crawPeak->getHeight());
             //basepeak.ms2chromatogram.peaks.insert(peak);

             f = abs(peakTime - (crawPeak->getfiveprcntLh()));
             //static cast <double deleted from here!!!!!!>

             //Now for peak capacity calculation: p = 1 + tg/w  where tg is the Rt of the peak and w is the width of the peak.
             currentbasepeak.bpkFWHM = fwhm;
             currentbasepeak.peakSymm = crawPeak->getfwfivepercent() / (2 * f);
             currentbasepeak.PeakCapacity = 1 + (peakTime / fwhm);

         }

     }*/
    }
    public struct RTandInt
    {
        public double[] starttimes;
        public double[] intensities;
    }



}
