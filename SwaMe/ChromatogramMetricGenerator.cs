using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Interpolation;
using MzmlParser;
using NLog;

namespace SwaMe
{
    public class ChromatogramMetricGenerator
    {
        public double[] intensities;
        public double[] starttimes;
        CrawdadSharp.CrawdadPeakFinder cPF;
        List<CrawdadSharp.CrawdadPeak> crawPeaks;

        public void GenerateChromatogram(Run run)
        {
            //Crawdad
            foreach (BasePeak basepeak in run.BasePeaks)
            {
                //for each peak within the spectrum 
                for (int yyy = 0; yyy < basepeak.BpkRTs.Count(); yyy++)
                {
                    //change these two arrays to be only the spectra surrounding that bpkrt:
                    intensities = basepeak.Spectrum.Where(x => Math.Abs(x.RetentionTime - basepeak.BpkRTs[yyy]) < run.AnalysisSettings.RtTolerance).Select(x => (double)x.Intensity).ToArray();
                    starttimes = basepeak.Spectrum.Where(x => Math.Abs(x.RetentionTime - basepeak.BpkRTs[yyy]) < run.AnalysisSettings.RtTolerance).Select(x => (double)x.RetentionTime).ToArray();
                    if (intensities.Count() > 1)
                    {
                        RTandInt inter = new RTandInt();
                        inter = Interpolate(starttimes, intensities);
                        starttimes = inter.starttimes;
                        intensities = inter.intensities;
                    }
                    cPF = new CrawdadSharp.CrawdadPeakFinder();
                    cPF.SetChromatogram(starttimes, intensities);
                    crawPeaks = cPF.CalcPeaks();
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

            foreach (IRTPeak irtpeak in run.IRTPeaks)
            {
                float TotalFWHM = 0;
                float Totalpeaksym = 0;
                int count = 0;
                foreach (var transition in irtpeak.AssociatedTransitions)
                {
                    starttimes = irtpeak.Spectrum.Where(x=>Math.Abs(x.Mz-transition.ProductMz)<run.AnalysisSettings.MassTolerance && Math.Abs(x.RetentionTime-irtpeak.RetentionTime)<run.AnalysisSettings.RtTolerance).Select(x => (double)x.RetentionTime).ToArray();
                    intensities = irtpeak.Spectrum.Where(x => Math.Abs(x.Mz - transition.ProductMz) < run.AnalysisSettings.MassTolerance).Select(x => (double)x.Intensity).ToArray();
                    cPF = new CrawdadSharp.CrawdadPeakFinder();
                    cPF.SetChromatogram(starttimes, intensities);
                    crawPeaks = cPF.CalcPeaks();
                    foreach (CrawdadSharp.CrawdadPeak crawPeak in crawPeaks)
                    {
                        TotalFWHM += crawPeak.Fwhm;
                        if (crawPeak.Fvalue > 0)
                        {
                            Totalpeaksym += crawPeak.Fwfpct / (crawPeak.Fvalue * 2);
                        }
                        else { Totalpeaksym += crawPeak.Fwfpct; }
                        count++;
                    }
                }
               
                irtpeak.FWHM = TotalFWHM/count;//average fwhm
                irtpeak.Peaksym = Totalpeaksym/count;//Because we have ordered the possible peaks according to their intensities, this value corresponds to their intensity rank 
                
            }
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
