#nullable enable

using ProtoBuf;

namespace SwaMe.Pipeline
{
    /// <summary>
    /// A Spectrum is an immutable set of SpectrumPoints, with the set implemented as an Array for convenience.
    /// </summary>
    [ProtoContract]
    public class Spectrum
    {
        public Spectrum(SpectrumPoint[] spectrumPoints)
        {
            SpectrumPoints = spectrumPoints;
        }

        [ProtoMember(1)]
        public virtual SpectrumPoint[] SpectrumPoints { get; }
    }
}
