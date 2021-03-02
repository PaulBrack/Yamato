#nullable enable

#define USE_DOUBLE_INTENSITIES
#define USE_DOUBLE_RETENTION_TIMES

using System;
using System.Collections.Generic;
using System.Linq;
using CrawdadSharp;
using MathNet.Numerics.Interpolation;
using SwaMe.Pipeline;

#if USE_DOUBLE_INTENSITIES
using Intensity = System.Double;
#else
using Intensity = System.Single;
#endif
#if USE_DOUBLE_RETENTION_TIMES
using RetentionTime = System.Double;
#else
using RetentionTime = System.Single;
#endif

namespace SwaMe
{
    public class ChromatogramMetricGenerator
    {
        /// <returns>Debugging output of the last pair of interpolated retention time and intensity arrays; this is used by some tests but should not be relied on elsewhere.</returns>
        public RetentionTimesAndIntensities? GenerateChromatogram(Run<Scan> run)
        {
            // Crawdad
            RetentionTimesAndIntensities? testOutput = default;
            foreach (BasePeak basepeak in run.BasePeaks)
            {
                // For each peak within the spectrum 
                foreach (RetentionTime bpkRt in basepeak.BpkRTs)
                {
                    // Change these two arrays to be only the spectra surrounding that basepeak retention time:
                    var pointsInRange = basepeak.Spectrum.Where(x => Math.Abs(x.RetentionTime - bpkRt) < run.AnalysisSettings.RtTolerance).ToList();
                    Intensity[] intensities = pointsInRange.Select(x => (Intensity)x.Intensity).ToArray();
                    RetentionTime[] startTimes = pointsInRange.Select(x => (RetentionTime)x.RetentionTime).ToArray();
                    if (intensities.Length > 1)
                    {
                        RetentionTimesAndIntensities interpolated = Interpolate(startTimes, intensities);
                        startTimes = interpolated.StartTimes;
                        intensities = interpolated.Intensities;
                        testOutput = interpolated;
                    }
                    CrawdadPeakFinder cPF = new CrawdadPeakFinder();
                    cPF.SetChromatogram(startTimes, intensities);
                    IList<CrawdadPeak> crawPeaks = cPF.CalcPeaks();

                    int peakCount = crawPeaks.Count;
                    if (peakCount == 0)
                    {
                        basepeak.FWHMs.Add(0);
                        basepeak.FullWidthBaselines.Add(0);
                        basepeak.Peaksyms.Add(0);
                    }
                    else
                    {
                        // If we get here, there's at least one peak, so do the calculation
                        double totalFwhm = 0;
                        double totalPeakSym = 0;
                        double totalBaseWidth = 0;
                        foreach (CrawdadPeak crawPeak in crawPeaks)
                        {
                            double fwhm = crawPeak.Fwhm;
                            if (fwhm == 0)
                                continue;

                            double fvalue = crawPeak.Fvalue;
                            if (fvalue != 0)
                                totalPeakSym += crawPeak.Fwfpct / (2 * crawPeak.Fvalue); //From USP31: General Chapters <621> Chromatography equation for calculating the tailing factor(Available at: http://www.uspbpep.com/usp31/v31261/usp31nf26s1_c621.asp). A high value means that the peak is highly asymmetrical.
                            totalFwhm += fwhm;
                            if (!float.IsNaN(crawPeak.FwBaseline))
                                totalBaseWidth += crawPeak.FwBaseline;
                            else
                                totalBaseWidth += crawPeak.Fwfpct;
                        }
                        basepeak.FWHMs.Add(totalFwhm / peakCount);
                        basepeak.Peaksyms.Add(totalPeakSym / peakCount);
                        basepeak.FullWidthBaselines.Add(totalBaseWidth / peakCount);
                    }
                }
            }
            return testOutput;
        }

        /// <summary>
        /// TODO: There are no tests on this method.
        /// </summary>
        public void GenerateiRTChromatogram(Run<Scan> run)
        {
            foreach (IRTPeak irtpeak in run.IRTPeaks)
            {
                float totalFWHM = 0;
                float totalPeakSymmetry = 0;
                int totalPeakCount = 0;
                foreach (LibraryParser.Library.Transition? transition in irtpeak.AssociatedTransitions)
                {
                    // TODO: Why are these criteria different?  PJC 2021-03-01
                    RetentionTime[] startTimes = irtpeak.Spectrum.Where(x=>Math.Abs(x.Mz-transition.ProductMz)<run.AnalysisSettings.MassTolerance && Math.Abs(x.RetentionTime-irtpeak.RetentionTime)<run.AnalysisSettings.RtTolerance).Select(x => (double)x.RetentionTime).ToArray();
                    Intensity[] intensities = irtpeak.Spectrum.Where(x => Math.Abs(x.Mz - transition.ProductMz) < run.AnalysisSettings.MassTolerance).Select(x => (double)x.Intensity).ToArray();
                    CrawdadPeakFinder crawdadPeakFinder = new CrawdadPeakFinder();
                    crawdadPeakFinder.SetChromatogram(startTimes, intensities);
                    IList<CrawdadPeak> crawdadPeaks = crawdadPeakFinder.CalcPeaks();
                    foreach (CrawdadPeak crawdadPeak in crawdadPeaks)
                    {
                        totalFWHM += crawdadPeak.Fwhm;
                        if (crawdadPeak.Fvalue > 0)
                            totalPeakSymmetry += crawdadPeak.Fwfpct / (crawdadPeak.Fvalue * 2);
                        else
                            totalPeakSymmetry += crawdadPeak.Fwfpct;
                        totalPeakCount++;
                    }
                }
               
                irtpeak.FullWidthHalfMax = totalFWHM / totalPeakCount; //average fwhm
                irtpeak.PeakSymmetry = totalPeakSymmetry / totalPeakCount; // Because we have ordered the possible peaks according to their intensities, this value corresponds to their intensity rank 
            }
        }

        /// <summary>
        /// TODO: This always returns a *copy* of the inputs, even when they contain enough points.  Is that necessary, or can we elide the copy if there are enough points?
        /// </summary>
        private RetentionTimesAndIntensities Interpolate(RetentionTime[] startTimes, Intensity[] intensities)
        {
            // First step is interpolating a spline, then choosing the points at which to add values:
            CubicSpline interpolation = CubicSpline.InterpolateNatural(startTimes, intensities);

            // Now we work out how many to add so we reach at least 100 datapoints and add them:
            RetentionTime stimesinterval = startTimes.Last() - startTimes[0];
            int numNeeded = 100 - startTimes.Length;
            RetentionTime interval = stimesinterval / numNeeded;
            List<Intensity> intensityList = intensities.ToList();
            List<RetentionTime> startTimesList = startTimes.ToList();
            for (int i = 0; i < numNeeded; i++)
            {
                RetentionTime insertionRetentionTime = startTimes[0] + (interval * i);

                // Insert newIntensity into the correct spot in the array
                for (int offset = 0; offset < 100; offset++)
                {
                    if (startTimesList[offset] < insertionRetentionTime)
                        continue;
                    if (offset > 0)
                    {
                        // Inserting at somewhere other than the start of the array - if we've hit the element, go later.
                        // TODO: BUG: This puts the values in the wrong order, as the interpolation is for a LATER time but still inserts EARLIER than the current element.
                        if (startTimesList[offset] == insertionRetentionTime)
                            insertionRetentionTime += 0.01;
                    }
                    else
                    {
                        // Inserting at the start of the array - if we've hit the first element, go earlier
                        if (startTimesList[offset] == insertionRetentionTime)
                            insertionRetentionTime -= 0.01;
                    }
                    double newIntensity = interpolation.Interpolate(insertionRetentionTime);
                    intensityList.Insert(offset, newIntensity);
                    startTimesList.Insert(offset, insertionRetentionTime);
                    break;
                }
            }

            return new RetentionTimesAndIntensities(
                startTimesList.ToArray(),
                intensityList.ToArray()
            );
        }

        /// <summary>
        /// Immutable.
        /// </summary>
        public class RetentionTimesAndIntensities
        {
            public RetentionTime[] StartTimes { get; }
            public Intensity[] Intensities { get; }

            public RetentionTimesAndIntensities(RetentionTime[] startTimes, Intensity[] intensities)
            {
                StartTimes = startTimes;
                Intensities = intensities;
            }
        }
    }
}
