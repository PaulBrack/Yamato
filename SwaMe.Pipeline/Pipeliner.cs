#nullable enable

using LibraryParser;
using MzmlParser;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SwaMe.Pipeline
{
    /// <summary>
    /// Orchestrates the guts of the SwaMe pipeline.
    /// </summary>
    /// <remarks>This code mostly came out of MzmlReader when it was refactored.</remarks>
    public class Pipeliner : IScanConsumer<Scan, Run<Scan>>, IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly CountdownEvent cde = new CountdownEvent(1);

        public CancellationToken CancellationToken { get; set; }
        /// <summary>
        /// True iff at least one total ion current for a scan was not available in the input and had to be calculated.
        /// </summary>
        public bool TicNotFound { get; private set; }
        public bool AnyEmptyBinaryArray { get; private set; }

        public bool Threading { get; set; }
        public int MaxQueueSize { get; set; } = 1000;
        public int MaxThreads { get; set; } = 0;

        bool IScanConsumer<Scan, Run<Scan>>.RequiresBinaryData => true;

        /// <param name="path">The path to the input file to be opened, or null to read from stdin</param>
        public Run<Scan> LoadMzmlAndRunPipeline(string path, AnalysisSettings analysisSettings)
        {
            // === PHASE 1: READ MZML, CALCULATE BASEPEAKS, CALCULATE IRT HITS ===
            ScanAndRunFactory factory = new ScanAndRunFactory(analysisSettings);
            Run<Scan> run;
            MzmlReader<Scan, Run<Scan>> parser = new MzmlReader<Scan, Run<Scan>>(factory, factory)
            {
                Threading = Threading,
                MaxThreads = MaxThreads,
                MaxQueueSize = MaxQueueSize
            };
            parser.Register(this);
            run = parser.LoadMzml(path);

            run.MissingScans = 0;

            logger.Debug("Run length {0}", run.LastScanTime - run.StartTime);
            logger.Debug("{0} MS1 total scans read", run.Ms1Scans.Count);
            logger.Debug("{0} MS2 total scans read", run.Ms2ScanCount);
            logger.Debug("{0} candidate IRT hits detected", run.IRTHits.Count);
            logger.Debug("{0} base peaks selected", run.BasePeaks.Count);
            logger.Debug("{0} isolation windows detected: min {1} max {2}", run.IsolationWindows.Count, run.IsolationWindows.Min(x => x.Width), run.IsolationWindows.Max(x => x.Width));

            if (MaxThreads != 0)
                ThreadPool.SetMaxThreads(MaxThreads, MaxThreads);

            ThreadedAddBasePeakSpectra(run);

            cde.Reset(1);

            if (null != analysisSettings.IrtLibrary)
            {
                logger.Info("Selecting best IRT peptide candidates...");
                IrtPeptideMatcher.ChooseIrtPeptides(run);
            }
            foreach (var x in run.IRTHits.OrderBy(x => x.RetentionTime))
            {
                logger.Debug("{0} {1}", x.PeptideSequence, x.RetentionTime);
            }

            if (TicNotFound)
                logger.Warn("Total Ion Current value was not found and had to be calculated from the spectra. This will cause this value to be incorrect for centroided MZML files, peak picked data, or other manipulated spectra.");
            if (AnyEmptyBinaryArray)
                logger.Warn("One or more binary data arrays was empty and was zero filled.");

            return run;
        }

        private void ThreadedAddBasePeakSpectra(Run<Scan> run)
        {
            logger.Info("Finding base peak spectra...");
            cde.Reset(1);
            AddBasePeakSpectra(run);

            cde.Signal();
            while (cde.CurrentCount > 1)
            {
                if (CancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException("Reading MZML was cancelled");

                Thread.Sleep(500);
            }
            cde.Wait();
        }

        private static void FindIrtPeptideCandidates(Scan scan, Run<Scan> run, IList<SpectrumPoint> spectrum)
        {
            if (null == run.AnalysisSettings.IrtLibrary)
                return;

            foreach (Library.Peptide peptide in run.AnalysisSettings.IrtLibrary.PeptideList.Values)
            {
                var irtIntensities = new List<float>();
                var irtMzs = new List<float>();

                // Optimisation: Convert peptideTransitions to an Array at the end; if it's left as an IEnumerable then the whole expression is evaluated twice.
                Library.Transition[] peptideTransitions = run.AnalysisSettings.IrtLibrary.TransitionList.Values.OfType<Library.Transition>().Where(x => x.PeptideId == peptide.Id).ToArray();
                int transitionsLeftToSearch = peptideTransitions.Length;
                foreach (Library.Transition transition in peptideTransitions)
                {
                    if (irtIntensities.Count + transitionsLeftToSearch < run.AnalysisSettings.IrtMinPeptides)
                        break;

                    // Optimisation: Convert spectrumPoints to an Array at the end; if it's left as an IEnumerable then the whole expression is evaluated three times.
                    SpectrumPoint[] spectrumPoints = spectrum.Where(x => x.Intensity > run.AnalysisSettings.IrtMinIntensity && Math.Abs(x.Mz - transition.ProductMz) < run.AnalysisSettings.IrtMassTolerance).ToArray();
                    if (spectrumPoints.Length > 0)
                    {
                        float maxIntensity = spectrumPoints.Max(x => x.Intensity);
                        irtIntensities.Add(maxIntensity);
                        irtMzs.Add(spectrumPoints.Where(x => x.Intensity == maxIntensity).First().Mz);
                    }
                    transitionsLeftToSearch--;
                }
                if (irtIntensities.Count >= run.AnalysisSettings.IrtMinPeptides)
                {
                    run.IRTHits.Add(new CandidateHit()
                    {
                        PeptideSequence = peptide.Sequence,
                        Intensities = irtIntensities,
                        ActualMzs = irtMzs,
                        RetentionTime = scan.ScanStartTime,
                        PrecursorTargetMz = peptide.AssociatedTransitions.First().PrecursorMz,
                        ProductTargetMzs = peptide.AssociatedTransitions.Select(x => x.ProductMz).ToList()
                    });
                }
            }
        }

        private void AddBasePeakSpectra(Run<Scan> run)
        {
            foreach (Scan scan in run.Ms2Scans)
            {
                if (CancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException("Reading MZML was cancelled");

                cde.AddCount();
                ThreadPool.QueueUserWorkItem(state => FindBasePeaks(run, scan));
            }
        }

        private void FindBasePeaks(Run<Scan> run, Scan scan)
        {
            var spectrum = scan.Spectrum;
            if (spectrum != null && spectrum.SpectrumPoints.Length > 0)
            {
                foreach (BasePeak basePeak in run.BasePeaks.Where(x => Math.Abs(x.Mz - scan.BasePeakMz) <= run.AnalysisSettings.MassTolerance))
                {
                    var temp = basePeak.BasePeakRetentionTimes.Where(x => Math.Abs(x - scan.ScanStartTime) < run.AnalysisSettings.RtTolerance);
                    if (temp.Any())
                    {
                        IEnumerable<SpectrumPoint>? matching = spectrum.SpectrumPoints.Where(x => Math.Abs(x.Mz - basePeak.Mz) <= run.AnalysisSettings.MassTolerance);

                        if (matching != null)
                        {
                            lock (basePeak.Spectrum)
                            {
                                basePeak.Spectrum.Add(matching.OrderByDescending(x => x.Intensity).FirstOrDefault());
                            }
                        }
                    }
                }
            }
            cde.Signal();
        }

        private static float[] FillZeroArray(float[]? array)
        {
            Array.Resize(ref array, 5);
            return array;
        }

        void IScanConsumer<Scan, Run<Scan>>.Notify(Scan scan, float[]? mzs, float[]? intensities, Run<Scan> run)
        {
            // Initial setup: isolation window boundaries and throw in some minimal arrays if the mzML has none for this scan.
            if (scan.IsolationWindowTargetMz.HasValue && scan.IsolationWindowLowerOffset.HasValue && scan.IsolationWindowUpperOffset.HasValue)
            {
                lock (run.IsolationWindows)
                {
                    run.IsolationWindows.Add(new IsolationWindow(scan.IsolationWindowLowerBoundary.Value, scan.IsolationWindowTargetMz.Value, scan.IsolationWindowUpperBoundary.Value));
                }
            }

            if (null == intensities || intensities.Length == 0)
            {
                intensities = FillZeroArray(intensities);
                mzs = FillZeroArray(mzs);
                logger.Debug("Empty binary array for a MS{0} scan in cycle number: {1}. The empty scans have been filled with zero values.", scan.MsLevel, scan.Cycle);

                run.MissingScans++;
            }

            // Note our highest-intensity peak and its corresponding m/z as a new base peak.
            (float basepeakIntensity, double basePeakMz) = Utilities.LookUpMaxValue(intensities, mzs);
            scan.BasePeakIntensity = basepeakIntensity;
            scan.BasePeakMz = basePeakMz;
            if (intensities.Length > 0)
                run.AtomicallyRecordBasePeakAndRT(basepeakIntensity, basePeakMz, scan.ScanStartTime);

            // Note our TIC if the mzML doesn't contain it.  This is necessarily approximate if we're already dealing with peak-picked data.
            if (scan.TotalIonCurrent == 0)
            {
                scan.TotalIonCurrent = intensities.Sum();
                TicNotFound = true;
            }

            scan.ProportionChargeStateOne = ProportionChargeStateOneCalculator.CalculateProportionChargeStateOne(mzs);

            // Construct SpectrumPoints for everything that's above our minimum intensity.
            var spectrumPoints = intensities.Select((x, i) => new SpectrumPoint(x, mzs[i], (float)scan.ScanStartTime)).Where(x => x.Intensity >= run.AnalysisSettings.MinimumIntensity).ToArray();
            scan.Spectrum = new Spectrum(spectrumPoints);
            scan.Density = spectrumPoints.Length;

            // Extract info for Basepeak chromatograms
            if (null != run.AnalysisSettings.IrtLibrary)
                FindIrtPeptideCandidates(scan, run, spectrumPoints);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cde.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
