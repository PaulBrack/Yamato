#nullable enable

using System;

namespace CVLibrarian
{
    /// <summary>
    /// One term within a controlled vocabulary.
    /// </summary>
    /// <remarks>Immutable.</remarks>
    public class Term
    {
        public ControlledVocabulary ControlledVocabulary { get; }
        public string Id { get; }
        public string Name { get; }

        public Term(ControlledVocabulary controlledVocabulary, string id, string name)
        {
            ControlledVocabulary = controlledVocabulary;
            Id = id;
            Name = name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ControlledVocabulary, Id, Name);
        }

        public override bool Equals(object? obj)
        {
            if (null == this && null == obj)
                return true;
            if (null == obj)
                return false;
            Term rhs = (Term)obj;
            return ControlledVocabulary == rhs.ControlledVocabulary && Id.Equals(rhs.Id) && Name == rhs.Name;
        }
    }
}
