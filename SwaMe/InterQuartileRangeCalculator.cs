using System.Collections.Generic;

namespace SwaMe
{
    static class InterQuartileRangeCalculator
    {
        public static double CalcIQR(List<double> list)
        {
            // Note list must already be sorted.
            int length = list.Count;
            double median = list[length / 2];
            double Q1 = list[length / 4];
            double Q3 = list[length / 4 * 3];
            return Q3 - Q1;
        }

        public static int CalcIQR(List<int> list)
        {
            // Note list must already be sorted.
            int length = list.Count;
            int median = list[length / 2];
            int Q1 = list[length / 4];
            int Q3 = list[length / 4 * 3];
            return Q3 - Q1;
        }
    }
}