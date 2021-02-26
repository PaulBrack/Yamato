using System;
using System.Collections.Generic;

namespace SwaMe
{
    public static class InterQuartileRangeCalculator
    {
        /// <summary>
        /// Precondition: list must be sorted ascending.
        /// </summary>
        public static double CalcIQR(IList<double> list)
        {
            if (list.Count < 5)
                throw new ArgumentException($"At least 5 list items required to calculate IQR, got {list.Count}", nameof(list));

            int length = list.Count;
            double Q1 = list[length / 4];
            double Q3 = list[length / 4 * 3];
            return Q3 - Q1;
        }

        /// <summary>
        /// Precondition: list must be sorted ascending.
        /// </summary>
        public static int CalcIQR(IList<int> list)
        {
            if (list.Count < 5)
                throw new ArgumentException($"At least 5 list items required to calculate IQR, got {list.Count}", nameof(list));
            int length = list.Count;
            int Q1 = list[length / 4];
            int Q3 = list[length / 4 * 3];
            return Q3 - Q1;
        }
    }
}