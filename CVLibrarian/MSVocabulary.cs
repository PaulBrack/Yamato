#nullable enable

namespace CVLibrarian
{
    /// <summary>
    /// Interim: Holder to return the known Unit Ontology vocabulary using code.
    /// </summary>
    public static class MSVocabulary
    {
        public static ControlledVocabulary Get()
        {
            ControlledVocabulary vocabulary = new ControlledVocabulary("Proteomics Standards Initiative Mass Spectrometry Ontology", "https://raw.githubusercontent.com/HUPO-PSI/psi-ms-CV/master/psi-ms.obo", "4.1.7");
            vocabulary.AddTerms(new Term[]
            {
                new Term(vocabulary, "XXXXXXXXXXXXXX", "SwaMe"),
                new Term(vocabulary, "MS:1000040", "m/z"),
                new Term(vocabulary, "MS:1000042", "Peak Intensity"),
                new Term(vocabulary, "MS:1000569", "SHA-1"),
                new Term(vocabulary, "MS:1000584", "mzML format"),
                new Term(vocabulary, "MS:1000747", "completion time")
            });
            return vocabulary;
        }
    }
}