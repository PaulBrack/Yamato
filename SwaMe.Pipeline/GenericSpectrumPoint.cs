﻿#nullable enable

using ProtoBuf;

namespace SwaMe.Pipeline
{
    /// <summary>
    /// A point in the 2-dimensional LC-MS space of retention time and m/z, recording intensity at this coordinate.
    /// </summary>
    /// <remarks>
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
    public class GenericSpectrumPoint<T> where T: struct
    {
        public GenericSpectrumPoint()
        {
        }

        public GenericSpectrumPoint(T intensity, T mz, T retentionTime)
        {
            Intensity = intensity;
            Mz = mz;
            RetentionTime = retentionTime;
        }

        /// <remarks>Depending on the use of this class, this may be raw retention time or iRT. Within MzmlParser, it's raw and in minutes.</remarks>
        [ProtoMember(1)]
        public T RetentionTime { get; set; }

        /// <summary>
        /// Mass over charge, in Daltons.
        /// </summary>
        [ProtoMember(2)]
        public T Mz { get; set; }

        [ProtoMember(3)]
        public T Intensity { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name}(RetentionTime={RetentionTime}, Mz={Mz}, Intensity={Intensity})";
        }
    }
}
