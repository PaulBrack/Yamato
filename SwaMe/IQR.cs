using System;
using System.Collections.Generic;

namespace SwaMe
{
    class IQR
    {
        public double calcIQR(List<double> list, int length)
        {

            // Note list must already be sorted.
            double median = list[length / 2];
            double Q1 = list[length / 4];
            double Q3 = list[length / 4 * 3];
            return Q3 - Q1;

        }

        public int calcIQR(List<int> list, int length)
        {

            // Note list must already be sorted.
            int median = list[length / 2];
            int Q1 = list[length / 4];
            int Q3 = list[length / 4 * 3];
            return Q3 - Q1;

        }
    }
}
