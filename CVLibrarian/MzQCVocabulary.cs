#nullable enable

namespace CVLibrarian
{
    /// <summary>
    /// Interim: Holder to return the known mzQC vocabulary using code.
    /// </summary>
    public static class MzQCVocabulary
    {
        public static ControlledVocabulary Get()
        {
            ControlledVocabulary vocabulary = new ControlledVocabulary("Proteomics Standards Initiative Quality Control Ontology", "https://github.com/HUPO-PSI/mzQC/raw/master/cv/qc-cv.obo", "0.1.0");
            vocabulary.AddTerms(new Term[]
            {
                new Term(vocabulary, "QC:4000053", "Quameter metric: RT-Duration"),
                new Term(vocabulary, "QC:02", "SwaMe metric: swathSizeDifference"),
                new Term(vocabulary, "QC:4000060", "Quameter metric: MS2-Count"),
                new Term(vocabulary, "QC:04", "SwaMe metric: NumOfSwaths"),
                new Term(vocabulary, "QC:05", "SwaMe metric: Target mz"),
                new Term(vocabulary, "QC:06", "SwaMe metric: TotalMS2IonCount"),
                new Term(vocabulary, "QC:07", "SwaMe metric: MS2Density50"),
                new Term(vocabulary, "QC:08", "SwaMe metric: MS2DensityIQR"),
                new Term(vocabulary, "QC:4000059", "Quameter metric: MS1-Count"),
                new Term(vocabulary, "QC:09", "SwaMe metric: scansPerSwathGroup"),
                new Term(vocabulary, "QC:10", "SwaMe metric: AvgMzRange"),
                new Term(vocabulary, "QC:11", "SwaMe metric: SwathProportionOfTotalTIC"),
                new Term(vocabulary, "QC:12", "SwaMe metric: swDensity50"),
                new Term(vocabulary, "QC:13", "SwaMe metric: swDensityIQR"),
                new Term(vocabulary, "QC:14", "SwaMe metric: Peakwidths"),
                new Term(vocabulary, "QC:15", "SwaMe metric: PeakCapacity"),
                new Term(vocabulary, "QC:24", "SwaMe metric: TailingFactor"),
                new Term(vocabulary, "QC:XXXXXXXX", "SwaMe metric: MS2PeakPrecision"),
                new Term(vocabulary, "QC:16", "SwaMe metric: MS1PeakPrecision"),
                new Term(vocabulary, "QC:17", "SwaMe metric: DeltaTICAverage"),
                new Term(vocabulary, "QC:18", "SwaMe metric: DeltaTICIQR"),
                new Term(vocabulary, "QC:19", "SwaMe metric: AvgScanTime"),
                new Term(vocabulary, "QC:20", "SwaMe metric: MS2Density"),
                new Term(vocabulary, "QC:21", "SwaMe metric: MS1Density"),
                new Term(vocabulary, "QC:22", "SwaMe metric: MS2TICTotal"),
                new Term(vocabulary, "QC:23", "SwaMe metric: MS1TICTotal"),

                new Term(vocabulary, "QC:99", "Prognosticator Metric: MS1TICQuartiles"),
                new Term(vocabulary, "QC:98", "Prognosticator Metric: MS2TICQuartiles"),
                new Term(vocabulary, "QC:97", "Prognosticator Metric: MS1TIC"),
                new Term(vocabulary, "QC:96", "Prognosticator Metric: MS2TIC"),
                new Term(vocabulary, "QC:95", "Prognosticator Metric: MS1BPC"),
                new Term(vocabulary, "QC:94", "Prognosticator Metric: MS2BPC"),
                new Term(vocabulary, "QC:93", "Prognosticator Metric: CombinedTIC"),
                new Term(vocabulary, "QC:92", "Prognosticator Metric: MS1:MS2 ratio"),
                new Term(vocabulary, "QC:91", "Prognosticator Metric: MS1 weighted median skew"),
                new Term(vocabulary, "QC:90", "Prognosticator Metric: MS2 weighted median skew"),
                new Term(vocabulary, "QC:89", "Prognosticator Metric: MeanIrtMassError"),
                new Term(vocabulary, "QC:88", "Prognosticator Metric: MaxIrtMassError"),
                new Term(vocabulary, "QC:87", "Prognosticator Metric: IrtPeptideFoundProportion"),
                new Term(vocabulary, "QC:86", "Prognosticator Metric: IrtPeptides"),
                new Term(vocabulary, "QC:85", "Prognosticator Metric: IrtPeptidesFound"),
                new Term(vocabulary, "QC:84", "Prognosticator Metric: IrtSpread"),
                new Term(vocabulary, "QC:83", "Prognosticator Metric: MS1TICQuartilesByRT"),
                new Term(vocabulary, "QC:82", "Prognosticator Metric: MS2TICQuartilesByRT"),
                new Term(vocabulary, "QC:81", "Prognosticator Metric: IrtOrderedness")
            });
            return vocabulary;
        }
    }
}
