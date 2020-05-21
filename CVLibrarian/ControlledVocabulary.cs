#nullable enable

using System.Collections.Generic;

namespace CVLibrarian
{
    public class ControlledVocabulary
    {
        public string Name { get; }
        public string Url { get; }
        public string Version { get; }
        private IDictionary<string, Term> TermsById { get; } = new Dictionary<string, Term>();
        public string? PrimaryNamespace { get; internal set; }

        public ControlledVocabulary(string name, string url, string version)
        {
            Name = name;
            Url = url;
            Version = version;
        }

        internal void AddTerms(IEnumerable<Term> terms)
        {
            foreach (Term term in terms)
                AddTerm(term);
        }

        private void AddTerm(Term term)
        {
            TermsById.Add(term.Id, term);
        }

        public Term GetById(string id) => TermsById[id];
    }
}