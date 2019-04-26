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
           
            for (int divider = 0; divider < division; divider++)
            {
                sw.Write("RTsegment");
                sw.Write(division);
                sw.Write(" \t ");
                
                sym.Write("RTsegment");
                sym.Write(division);
                sym.Write(" \t ");
            }

            sw.Write("\n");
            sw.Write(run.SourceFileName);
            sym.Write("\n");
            sym.Write(run.SourceFileName);
            
            //Calculations for peakprecision MS2:

            var meanIntensityOfAllBpks = run.BasePeaks.Select(x=>x.intensity).Average();
            var meanMzOfAllBpks = run.BasePeaks.Select(x => x.Mz).Average();

            //Calculations for peakprecision MS1:
            var mIMS1Bpks = run.Ms1Scans.Select(x => x.BasePeakIntensity).Average();
            var mMMS1Bpks = run.Ms1Scans.Select(x => x.BasePeakMz).Average();

            

            StreamWriter rM = new StreamWriter("RTDividedMetrics.tsv");
            rM.Write("Filename\t RTsegment \t MS2PeakCapacity \t MS2Peakprecision \t MS1PeakPrecision \t DeltaTICAverage \t DeltaTICIQR \t AveScanTime \t AveMS2Density \t AveMS1Density \t MS2TICTotal \t MS1TICTotal");

            

            for (int segment = 0; segment < division; segment++)
            {
                List<double> PeakwidthsTemp = new List<double>();
                List<double> PeaksymTemp = new List<double>();
                List<double> PeakCapacity = new List<double>();
                List<double> PeakprecisionTemp = new List<double>();
                foreach (MzmlParser.BasePeak basepeak in run.BasePeaks)
                {
                    
                    if (basepeak.RTsegment == segment)
                    {
                        PeakwidthsTemp.Add(basepeak.FWHM);
                        PeaksymTemp.Add(basepeak.peaksym);
                        PeakprecisionTemp.Add(basepeak.intensity / (meanIntensityOfAllBpks * Math.Pow(2,meanMzOfAllBpks / basepeak.Mz)));
                        PeakCapacity.Add(basepeak.peakCapacity);
                    }
                }
                double firstScanStartTime = 1000;
                double lastScanStartTime = 0;
                int firstCycle = 1000;
                int lastCycle = 0;
                double MS1TICTotal = 0;
                List<double> MS1Density = new List<double>();
                List<double> MS1PeakprecisionTemp = new List<double>();
                
                foreach (MzmlParser.Scan scan in run.Ms1Scans)
                {
                    
                    if (scan.RTsegment == segment)
                    {

                        MS1PeakprecisionTemp.Add(scan.BasePeakIntensity / (meanIntensityOfAllBpks * Math.Pow(2, meanMzOfAllBpks / scan.BasePeakMz)));
                        firstScanStartTime = Math.Min(firstScanStartTime, scan.ScanStartTime);
                        lastScanStartTime = Math.Max(lastScanStartTime, scan.ScanStartTime);
                        firstCycle = Math.Min(scan.Cycle, firstCycle);
                        lastCycle = Math.Max(scan.Cycle, lastCycle);
                        MS1Density.Add(scan.Density);
                        MS1TICTotal += scan.TotalIonCurrent;
                    }
                }

                //To get scan speed for both ms1 and ms2 we have to also scan through ms2:
                List<int> MS2Density = new List<int>();
                double MS2TICTotal = 0;
                foreach (MzmlParser.Scan scan in run.Ms2Scans)
                {
                    if (scan.RTsegment == segment)
                    {
                        firstCycle = Math.Min(scan.Cycle, firstCycle);
                        lastCycle = Math.Max(scan.Cycle, lastCycle);
                        MS2Density.Add(scan.Density);
                        MS2TICTotal += scan.TotalIonCurrent;
                    }
                }
                

                //write rM
                rM.Write("\n");
                rM.Write(run.SourceFileName);
                rM.Write("\t");
                rM.Write("RTsegment");
                rM.Write(segment);
                rM.Write(" \t ");
                rM.Write(PeakCapacity.Average().ToString());
                rM.Write(" \t ");
                rM.Write(PeakprecisionTemp.Average().ToString());
                rM.Write("\t");
                rM.Write(MS1PeakprecisionTemp.Average().ToString());
                rM.Write("\t");
                rM.Write(TICchange50List.ElementAt(segment));
                rM.Write(" \t ");
                rM.Write(TICchangeIQRList.ElementAt(segment));
                rM.Write(" \t ");
                rM.Write((lastCycle - firstCycle)/(lastScanStartTime - firstScanStartTime));
                rM.Write(" \t ");
                rM.Write(MS2Density.Average());
                rM.Write(" \t ");
                rM.Write(MS1Density.Average());
                rM.Write(" \t ");
                rM.Write(MS2TICTotal);
                rM.Write(" \t ");
                rM.Write(MS1TICTotal);
                rM.Write(" \t ");

                //write sym
                sym.Write(" \t ");
                sym.Write(PeaksymTemp.Average().ToString());
                
                //write sw
                sw.Write(" \t ");
                sw.Write(PeakwidthsTemp.Average().ToString());
                
            }
            sw.Close();
            sym.Close();
            rM.Close();

        }
    }
}



