using System.Collections.Generic;
using System.Linq;

namespace SwaMe.Pipeline
{
    public static class ProportionChargeStateOneCalculator
    {
        public static double CalculateProportionChargeStateOne(float[] mzs)
        {
            // Predicted singly charged proportion:
            //
            // The theory is that an M and M+1 pair are singly charged so we are very simply just looking for  occurences where two ions are 1 mz apart (+-massTolerance)
            //
            // We therefore create an array cusums that accumulates the difference between ions, so for every ion we calculate the distance between that ion
            // and the previous and add that to each of the previous ions' cusum of differences. If the cusum of an ion overshoots 1 +massTolerance, we stop adding to it, if it reaches our mark we count it and stop adding to it

            List<int> indexes = new List<int>();
            float[] cusums = new float[mzs.Length];
            int movingPoint = 0;
            double minimum = 1 - 0.001;
            double maximum = 1 + 0.001;

            for (int i = 1; i < mzs.Length; i++)
            {
                float distance = mzs[i] - mzs[i - 1];
                bool matchedWithLower = false;
                for (int ii = movingPoint; ii < i; ii++)
                {
                    cusums[ii] += distance;
                    if (cusums[ii] < minimum)
                        continue;
                    if (cusums[ii] > maximum)
                    {
                        movingPoint++;
                        continue;
                    }

                    // If we get here, we're within the range
                    if (!matchedWithLower) //This is to try and minimise false positives where for example if you have an array: 351.14, 351.15, 352.14 all three get chosen.
                    {
                        indexes.Add(i);
                        indexes.Add(movingPoint);
                    }
                    movingPoint++;
                    matchedWithLower = true;
                    // TODO: This looks to Peter like once matchedWithLower is true then one can break out of the loop as there will never be any other effect.  Is this true?
                }
            }
            int distinct = indexes.Distinct().Count();
            int len = mzs.Length;
            return distinct / (double)len;
        }
    }
}
