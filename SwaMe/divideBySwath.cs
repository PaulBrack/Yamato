
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace SwaMe
{
    class divideBySwath
    {
        public int SwathDivider(MzmlParser.Run run)
        {
            int maxswath = 0;
            var result = run.Ms2Scans.GroupBy(s => s.Cycle).Select(g => new { Count = g.Count() });
            foreach (var e in result)
                if (e.Count > maxswath) { maxswath = e.Count; }

            //group by cycle to allocate swathnumber


            var swathsPerNum = run.Ms2Scans.OrderBy(s => s.ScanStartTime).GroupBy(s => s.Cycle);
            foreach (var group in swathsPerNum)
            {
                int track = 0;
                var groupKey = group.Key;
                foreach (var groupedItem in group)
                {
                    groupedItem.swathNumber = track;
                    track++;
                }
                    

            }


            double totalTIC = 0;
            foreach (var scan in run.Ms2Scans)
            {
                totalTIC += scan.TotalIonCurrent;
            }
            
            //Loop through every swath of a certain swathNumber:

            List<int> numOfSwathPerGroup = new List<int>();
            List<double> mzRanges = new List <double>();
            List<double> TICs = new List<double>();
            List<double> swDensity = new List<double>();
            List<double> swDensity50 = new List<double>();
            List<double> swDensityIQR = new List<double>();
            double lowestmz = 1000;
            IQR iqr = new IQR { };
            for (int swathsOfThisNumber = 0; swathsOfThisNumber < maxswath; swathsOfThisNumber++)
            {
                int track =0 ;
                double highestmz = 0;
                double TICthisSwath = 0;

                var result333 = run.Ms2Scans.OrderBy(s => s.ScanStartTime)
                    .Where(x => x.swathNumber == swathsOfThisNumber);
                    foreach (var scan in result333)
                {
                    highestmz = Math.Max(highestmz, scan.highestmz);
                    lowestmz = Math.Min(lowestmz, scan.lowestmz);
                    TICthisSwath = TICthisSwath + scan.TotalIonCurrent;
                    swDensity.Add(scan.Density);
                    track++;
                }
                    
                 
                numOfSwathPerGroup.Add(track);
                mzRanges.Add(highestmz-lowestmz);
                TICs.Add(TICthisSwath);
                swDensity.Sort();
                swDensity50.Add(swDensity.Average());
                swDensityIQR.Add(iqr.calcIQR(swDensity, swDensity.Count()));
            }
            


            StreamWriter sm = new StreamWriter("MetricsBySwath.tsv");
            sm.Write("Filename \t swathNumber \t scansPerSwath \t mzRangePerSwath \t TICpercentageOfSwath \t swDensityAverage \t swDensityIQR  \n");
            for (int num = 0; num < maxswath; num++)
            {
                sm.Write(run.SourceFileName);
                sm.Write("\t");
                sm.Write("Swathnumber");
                sm.Write(num+1);
                sm.Write("\t");
                sm.Write(numOfSwathPerGroup.ElementAt(num));
                sm.Write("\t");
                sm.Write(mzRanges.ElementAt(num));
                sm.Write("\t");
                sm.Write((TICs[num]/totalTIC)*100);
                sm.Write("\t");
                sm.Write(swDensity50[num]);
                sm.Write("\t");
                sm.Write(swDensityIQR[num]);
                sm.Write("\n");
            }



















            sm.Close();
            return maxswath;
        }

    }
}
