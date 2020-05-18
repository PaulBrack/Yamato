#nullable enable

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CVLibrarian
{
    /// <summary>
    /// Understands how to look up, cache, and handle queries on controlled vocabularies.
    /// </summary>
    /// <remarks>At present, only understands built-in terms. TODO: Extend to at least OBOs, and perhaps wider.</remarks>
    public class CVLibrary
    {
        private readonly IDictionary<string, ControlledVocabulary> controlledVocabulariesByName = new Dictionary<string, ControlledVocabulary>();
        private readonly IList<ControlledVocabulary> controlledVocabularies = new List<ControlledVocabulary>();

        public ControlledVocabulary RegisterControlledVocabulary(ControlledVocabulary controlledVocabulary, params string[] names)
        {
            controlledVocabularies.Add(controlledVocabulary);
            foreach (string name in names)
                controlledVocabulariesByName.Add(name, controlledVocabulary);
            controlledVocabulary.PrimaryNamespace = names.Length > 0
                ? names[0]
                : TearOffPrimaryNamespace();
            return controlledVocabulary;
        }

        public Term GetTermByIds(string cvRef, string id) => controlledVocabulariesByName[cvRef].GetById(id);

        public ControlledVocabulary GetById(string cvRef) => controlledVocabulariesByName[cvRef];

        public ReadOnlyCollection<ControlledVocabulary> ControlledVocabularies => new ReadOnlyCollection<ControlledVocabulary>(controlledVocabularies);

        private static int nextPrimaryNamespaceId = 1;

        private static string TearOffPrimaryNamespace()
        {
            return $"ns{nextPrimaryNamespaceId++}";
        }
    }
}
