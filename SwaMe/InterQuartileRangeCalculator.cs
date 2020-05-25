using System;
using System.Collections.Generic;

namespace SwaMe
{
    public static class InterQuartileRangeCalculator
    {
        public static double CalcIQR(List<double> list)
        {
            if (list.Count < 5) 
            {
            throw new ArgumentException(String.Format("{0} is too few list items to calculate IQR", list.Count),
                                      nameof(list));
            }
            // Note list must already be sorted.
            int length = list.Count;
            double Q1 = list[length / 4];
            double Q3 = list[length / 4 * 3];
            return Q3 - Q1;
        }

        public static int CalcIQR(List<int> list)
        {
            // Note list must already be sorted.
            if (list.Count < 5)
            {
                throw new ArgumentException(String.Format("{0} is too few list items to calculate IQR", list.Count),
                                          nameof(list));
            }
            int length = list.Count;
            int Q1 = list[length / 4];
            int Q3 = list[length / 4 * 3];
            return Q3 - Q1;
        }
    }
}