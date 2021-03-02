#nullable enable

using System;

namespace SwaMe.Pipeline
{
    /// <summary>
    /// Immutable, suitable for use as a key in a Set or Dictionary.
    /// </summary>
    /// <remarks>Due to rounding, should we store the original data (lower and upper offset) instead and compute low and high?</remarks>
    public class IsolationWindow
    {
        public double HighMz { get; }
        public double LowMz { get; }
        public double TargetMz { get; }

        public double LowerOffset => TargetMz - LowMz;
        public double UpperOffset => HighMz - TargetMz;
        public double Width => HighMz - LowMz;

        public IsolationWindow(double lowMz, double targetMz, double highMz)
        {
            LowMz = lowMz;
            HighMz = highMz;
            TargetMz = targetMz;
        }

        public override int GetHashCode() => HashCode.Combine(LowMz, HighMz, TargetMz);

        public override bool Equals(object? obj) => obj is IsolationWindow rhs && LowMz == rhs.LowMz && HighMz == rhs.HighMz && TargetMz == rhs.TargetMz;
    }
}
