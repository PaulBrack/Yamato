using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace SwaMe
{
    class FileMaker
    {
        public void MakeMetricsPerSwathFile(int numOfSwaths, MzmlParser.Run run, List<int> numOfSwathPerGroup, List<double> AveMzRange, double totalTIC, List<double> TICs, List<double> swDensity50, List<double> swDensityIQR)
        {

            StreamWriter sm = new StreamWriter("MetricsBySwath.tsv");
            sm.Write("Filename \t swathNumber \t scansPerSwath \t AveMzRange \t TICpercentageOfSwath \t swDensityAverage \t swDensityIQR  \n");
            for (int num = 0; num < numOfSwaths; num++)
            {
                sm.Write(run.SourceFileName);
                sm.Write("\t");
                sm.Write("Swathnumber");
                sm.Write(num + 1);
                sm.Write("\t");
                sm.Write(numOfSwathPerGroup.ElementAt(num));
                sm.Write("\t");
                sm.Write(AveMzRange.ElementAt(num));
                sm.Write("\t");
                sm.Write((TICs[num] / totalTIC) * 100);
                sm.Write("\t");
                sm.Write(swDensity50[num]);
                sm.Write("\t");
                sm.Write(swDensityIQR[num]);
                sm.Write("\n");
            }
            sm.Close();
        }
        public void MakeMetricsPerRTsegmentFile(MzmlParser.Run run, List<List<double>> Peakwidths, List<List<double>> PeakSymmetry, List<List<double>> PeakCapacity, List<List<double>> PeakPrecision, List<List<double>> MS1Peakprecision,
            List<double> cycleTime, List<double> TICchange50List, List<double> TICchangeIQRList, List<List<int>> MS1Density, List<List<int>> MS2Density, List<double> MS1TICTotal, List<double> MS2TICTotal, int division)
        {

            StreamWriter rM = new StreamWriter("RTDividedMetrics.tsv");
            rM.Write("Filename\t RTsegment \t MS2Peakwidths \t PeakSymmetry \t MS2PeakCapacity \t MS2Peakprecision \t MS1PeakPrecision \t DeltaTICAverage \t DeltaTICIQR \t AveScanTime \t AveMS2Density \t AveMS1Density \t MS2TICTotal \t MS1TICTotal");

            for (int segment = 0; segment < division; segment++)
            {
                //write rM
                rM.Write("\n");
                rM.Write(run.SourceFileName);
                rM.Write("\t");
                rM.Write("RTsegment");
                rM.Write(segment);
                rM.Write(" \t ");
                rM.Write(Peakwidths.ElementAt(segment).Average().ToString());
                rM.Write(" \t ");
                rM.Write(PeakSymmetry.ElementAt(segment).Average().ToString());
                rM.Write(" \t ");
                rM.Write(PeakCapacity.ElementAt(segment).Average().ToString());
                rM.Write(" \t ");
                rM.Write(PeakPrecision.ElementAt(segment).Average().ToString());
                rM.Write("\t");
                rM.Write(MS1Peakprecision.ElementAt(segment).Average().ToString());
                rM.Write("\t");
                rM.Write(TICchange50List.ElementAt(segment));
                rM.Write(" \t ");
                rM.Write(TICchangeIQRList.ElementAt(segment));
                rM.Write(" \t ");
                rM.Write(cycleTime.ElementAt(segment));
                rM.Write(" \t ");
                rM.Write(MS2Density.ElementAt(segment).Average());
                rM.Write(" \t ");
                rM.Write(MS1Density.ElementAt(segment).Average());
                rM.Write(" \t ");
                rM.Write(MS1TICTotal.ElementAt(segment));
                rM.Write(" \t ");
                rM.Write(MS2TICTotal.ElementAt(segment));
                rM.Write(" \t ");
            }
            rM.Close();
        }
        public void MakeUndividedMetricsFile(MzmlParser.Run run, double RTDuration, double swathSizeDifference, int MS2Count, int NumOfSwaths, double cycleTimes50, double cycleTimesIQR, int MS2Density50, int MS2DensityIQR, int MS1Count)
        {
            StreamWriter um = new StreamWriter("undividedMetrics.tsv");
            um.Write("Filename \t RTDuration \t swathSizeDifference \t  MS2Count \t swathsPerCycle \t CycleTimes50 \t CycleTimesIQR \t MS2Density50 \t MS2DensityIQR \t MS1Count");
            um.Write("\n");
            um.Write(run.SourceFileName);
            um.Write("\t");
            um.Write(RTDuration);
            um.Write("\t");
            um.Write(swathSizeDifference);
            um.Write("\t");
            um.Write(MS2Count);
            um.Write("\t");
            um.Write(NumOfSwaths);
            um.Write("\t");
            um.Write(cycleTimes50);
            um.Write("\t");
            um.Write(cycleTimesIQR);
            um.Write("\t");
            um.Write(MS2Density50);
            um.Write("\t");
            um.Write(MS2DensityIQR);
            um.Write("\t");
            um.Write(MS1Count);
            um.Close();
        }
    }
}
