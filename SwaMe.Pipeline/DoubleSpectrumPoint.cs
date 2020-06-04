#nullable enable

namespace SwaMe.Pipeline
{
    public class DoubleSpectrumPoint : GenericSpectrumPoint<double>
    {
        public DoubleSpectrumPoint()
        {
        }

        public DoubleSpectrumPoint(double intensity, double mz, double retentionTime)
            : base(intensity, mz, retentionTime)
        {
        }
    }
}
