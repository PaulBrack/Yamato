namespace MzmlParser
{
    /// <summary>
    /// A point in the 2-dimensional LC-MS space of retention time and m/z, recording intensity.
    /// </summary>
    /// <remarks>
    /// Arguably, this should be a struct, for ease of memory management and pointer dereferencing within the VM.
    /// It's arguable because the class reduces copying overhead of arrays, so the optimisation depends on the exact use case.
    /// </remarks>
    public class SpectrumPoint
    {
        /// <remarks>Depending on the use of this class, this may be raw retention time or iRT. Within MzmlParser, it's raw and in TODO: seconds.</remarks>
        public float RetentionTime { get; set; }
        /// <summary>
        /// Mass over charge, in Daltons.
        /// </summary>
        public float Mz { get; set; }
        public float Intensity { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}(RetentionTime={RetentionTime}, Mz={Mz}, Intensity={Intensity})";
        }
    }
}
