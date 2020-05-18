#nullable enable

namespace CVLibrarian
{
    /// <summary>
    /// Interim: Holder to return the known Unit Ontology vocabulary using code.
    /// </summary>
    public static class UnitOntology
    {
        public static ControlledVocabulary Get()
        {
            ControlledVocabulary vocabulary = new ControlledVocabulary("Unit Ontology", "http://ontologies.berkeleybop.org/uo.obo", "releases/2020-03-10");
            vocabulary.AddTerms(new Term[]
            {
                new Term(vocabulary, "UO:0000189", "count"),
                new Term(vocabulary, "UO:0000010", "second"),
                new Term(vocabulary, "UO:0010006", "ratio") // TODO: What's the difference between this and UO:0000190?
            });
            return vocabulary;
        }
    }
}
