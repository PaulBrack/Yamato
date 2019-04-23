using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SwaMe
{
    class UndividedMetrics
    {
        public void UndividedMetrix(MzmlParser.Run run, double RTDuration, double swathSizeDifference, int MS2Count, int NumOfSwaths, double cycleTimes50, double cycleTimesIQR,int MS2Density50,int MS2DensityIQR, int MS1Count )
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
