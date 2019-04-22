using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SwaMe
{
    class RTDivider
    {
        public void DivideByRT(MzmlParser.Run run, int division)
        {
            StreamWriter sw = new StreamWriter("PeakWidths.csv");
            sw.Write("Filename , ");
            StreamWriter sym = new StreamWriter("Symmetry.csv");
            sym.Write("Filename, ");
            for (int divider = 0; divider < division; divider++)
            {
                sw.Write("RTsegment");
                sw.Write(division);
                sw.Write(" , ");
                sym.Write("RTsegment");
                sym.Write(division);
                sym.Write(" , ");
            }
            sw.Write("\n");
            sw.Write(run.SourceFileName);
            sym.Write("\n");
            sym.Write(run.SourceFileName);

            for (int segment = 0; segment < division; segment++)
            {
                List<double> PeakwidthsTemp = new List<double>();
                List<double> PeaksymTemp = new List<double>();
                foreach (MzmlParser.BasePeak basepeak in run.BasePeaks)
                {
                    if (basepeak.RTsegment == segment)
                    {
                        PeakwidthsTemp.Add(basepeak.FWHM);

                        PeaksymTemp.Add(basepeak.peaksym);
                    }
                }
                double pwMean = PeakwidthsTemp.Average();
                sym.Write(" , ");
                sym.Write(pwMean.ToString());
                sw.Write(" , ");
                sw.Write(pwMean.ToString());
            }
            sw.Close();
            sym.Close();

        }
    }
}
