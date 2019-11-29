using System.Collections.Generic;

namespace MzmlParser
{
    public interface IChromatograms
    {
        IList<(double, double)> Ms1Tic { get; set; }
        IList<(double, double)> Ms2Tic { get; set; }
        IList<(double, double)> Ms1Bpc { get; set; }
        IList<(double, double)> Ms2Bpc { get; set; }
    }
}
