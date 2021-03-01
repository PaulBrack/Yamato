#nullable enable

using ProtoBuf;
using System.Collections.Generic;

namespace SwaMe.Pipeline
{
    [ProtoContract]
    public class Spectrum
    {
        [ProtoMember(1)]
        public virtual IList<SpectrumPoint>? SpectrumPoints { get; set; }
    }
}
