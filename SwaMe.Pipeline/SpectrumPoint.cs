#nullable enable

using ProtoBuf;
using System;

namespace SwaMe.Pipeline
{
    /// <summary>
    /// A point in the 2-dimensional LC-MS space of retention time and m/z, recording intensity at this coordinate.
    /// </summary>
    /// <remarks>
    /// Immutable value type.
    /// 
    /// Arguably, this should be a struct, for ease of memory management and pointer dereferencing within the VM.
    /// It's arguable because the class reduces copying overhead of arrays, so the optimisation depends on the exact use case.
    /// 
    /// Paul Brack 2019/04/16
    /// 
    /// I've done some performance testing trying to load a SWATH map into memory. RAM use (and execution time) halves with
    /// this as a struct - this is our current bottleneck and on a 64GB RAM machine I can now load a 249K scan SWATH file 
    /// directly into memory now this has been changed. 
    /// </remarks>
    [ProtoContract]
    public class SpectrumPoint
    {
        public SpectrumPoint(float intensity, float mz, float retentionTime)
        {
            Intensity = intensity;
            Mz = mz;
            RetentionTime = retentionTime;
        }

        [ProtoMember(1)]
        public float Intensity { get; }

        /// <summary>
        /// Mass over charge, in Daltons.
        /// </summary>
        [ProtoMember(2)]
        public float Mz { get; }

        /// <remarks>Depending on the use of this class, this may be raw retention time or iRT. Within MzmlParser, it's raw and in minutes.</remarks>
        [ProtoMember(3)]
        public float RetentionTime { get; }

        public override string ToString()
        {
            return $"{GetType().Name}(RetentionTime={RetentionTime}, Mz={Mz}, Intensity={Intensity})";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Intensity, Mz, RetentionTime);
        }

        public override bool Equals(object? obj)
        {
            return obj is SpectrumPoint rhs && Equals(rhs);
        }

        public static bool operator ==(SpectrumPoint a, SpectrumPoint b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SpectrumPoint a, SpectrumPoint b)
        {
            return !(a == b);
        }

        public bool Equals(SpectrumPoint rhs)
        {
            return Intensity == rhs.Intensity
                && Mz == rhs.Mz
                && RetentionTime == rhs.RetentionTime;
        }
    }
}
