using System;
using System.Collections.Generic;
using System.Text;

namespace MzmlParser
{
    class ChromatogramGenerator
    {
        public Run CreateAllChromatograms(Run run)
        {

            return run;
        }

        public List<Tuple<float, float>> ExtractMs1Chromatogram(Run run)
        {
            List<Tuple<float, float>> chromatogram = new List<Tuple<float, float>>();
            return chromatogram;
        }
    }
}
