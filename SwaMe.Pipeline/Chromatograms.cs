#nullable enable

using System.Collections.Generic;

namespace SwaMe.Pipeline
{
    public class Chromatograms
    {
        public List<(double, double)>? Ms1Tic { get; set; }
        public List<(double, double)>? Ms2Tic { get; set; }
        public List<(double, double, double)>? CombinedTic { get; set; }
        public List<(double, double)>? Ms1Bpc { get; set; }
        public List<(double, double)>? Ms2Bpc { get; set; }
    }
}
