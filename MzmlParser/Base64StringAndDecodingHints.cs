#nullable enable

using Ionic.Zlib;
using System;

namespace MzmlParser
{
    /// <summary>
    /// A useful container for some base64-encoded data and the information on how to decode it.
    /// </summary>
    /// <remarks>Immutable</remarks>
    public sealed class Base64StringAndDecodingHints
    {
        public Base64StringAndDecodingHints(string base64Data, Compression compression, Bitness bitness)
        {
            Base64Data = base64Data;
            Compression = compression;
            Bitness = bitness;
        }

        public string Base64Data { get; }
        public Compression Compression { get; }
        public Bitness Bitness { get; }

        public float[] ExtractFloatArray()
        {
            if (string.IsNullOrEmpty(Base64Data))
                return Array.Empty<float>();

            byte[] bytes = Convert.FromBase64String(Base64Data);
            bytes = Compression switch
            {
                Compression.Uncompressed => bytes,
                Compression.Zlib => ZlibStream.UncompressBuffer(bytes),
                _ => throw new ArgumentOutOfRangeException("Compression", "Unknown compression")
            };

            return Bitness switch
            {
                Bitness.IEEE754FloatLittleEndian => GetFloats(bytes),
                Bitness.IEEE754DoubleLittleEndian => GetFloatsFromDoubles(bytes),
                _ => throw new ArgumentOutOfRangeException("Bitness", "Unknown bitness")
            };
        }

        private static float[] GetFloats(byte[] bytes)
        {
            float[] floats = new float[bytes.Length / 4];
            for (int i = 0; i < floats.Length; i++)
                floats[i] = BitConverter.ToSingle(bytes, i * 4);
            return floats;
        }

        private static float[] GetFloatsFromDoubles(byte[] bytes)
        {
            float[] floats = new float[bytes.Length / 8];
            for (int i = 0; i < floats.Length; i++)
                floats[i] = (float)BitConverter.ToDouble(bytes, i * 8);
            return floats;
        }
    }

    public enum Bitness
    {
        NotSet,
        IEEE754FloatLittleEndian,
        IEEE754DoubleLittleEndian
    }

    public enum Compression
    {
        Uncompressed,
        Zlib
    }
}
