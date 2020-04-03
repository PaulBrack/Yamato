using System;
using System.Runtime.Serialization;

namespace LibraryParser
{
    /// <summary>
    /// Designed to be thrown when the SV parser detects an unfixable format error in the file it's parsing.
    /// </summary>
    [Serializable]
    internal class SVFileFormatException : Exception
    {
        public string Path { get; set; }
        public SVFileFormatException()
        {
        }

        public SVFileFormatException(string message) : base(message)
        {
        }

        public SVFileFormatException(string message, string path) : base(message)
        {
            Path = path;
        }

        public SVFileFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SVFileFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}