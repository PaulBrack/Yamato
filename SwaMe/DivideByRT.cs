using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SwaMe
{
    class RTDivider
    {
        public void DivideByRT(MzmlParser.Run run, int division, List<double> TICchange50List, List<double> TICchangeIQRList)
        {
            StreamWriter sw = new StreamWriter("PeakWidths.tsv");
            sw.Write("Filename \t ");
            StreamWriter sym = new StreamWriter("Symmetry.tsv");
            sym.Write("Filename\t ");
            StreamWriter RT = new StreamWriter("MetricsDividedByRT.tsv");
            RT.Write("Filename\t");
            for (int divider = 0; divider < division; divider++)
            {
                sw.Write("RTsegment");
                sw.Write(division);
                sw.Write(" \t ");
                RT.Write("RTsegment");
                RT.Write(division);
                RT.Write(" \t ");
                sym.Write("RTsegment");
                sym.Write(division);
                sym.Write(" \t ");
            }

            RT.Write("\n");
            RT.Write(run.SourceFileName);
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
                double psMean = PeaksymTemp.Average();
                sym.Write(" \t ");
                sym.Write(psMean.ToString());
                sw.Write(" \t ");
                sw.Write(pwMean.ToString());
                RT.Write(TICchange50List.ElementAt(segment -1));
                RT.Write(" \t ");
                RT.Write(TICchangeIQRList.ElementAt(segment-1));
                RT.Write(" \t ");
            }
            sw.Close();
            sym.Close();
            RT.Close();
        }
    }
}
