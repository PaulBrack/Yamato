using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SwaMe
{
    class ChromatogramMetrics
    {
        public double CalculateFWHM(double[] starttimes, double[] intensities,double maxIntens,int mIIndex,double baseline)
        {
            
            double halfMax = (maxIntens - baseline) / 2 + baseline;
            double halfRT1 = 0;
            double halfRT2 = 0;
            for (int it = 0; it < mIIndex; it++)
            {
                if (intensities[it] > halfMax)
                {
                    halfRT1 = starttimes[it];
                }
            }

            for (int it = mIIndex; it > 0; it--)
            {
                if (intensities[it] < halfMax)
                {
                    halfRT2 = starttimes[it];
                }
            }

            return halfRT2 - halfRT1;
        }

        public double CalculateFpctHM(double[] starttimes, double[] intensities, double maxIntens, int mIIndex, double baseline)
        {
            
            double fiveMax = (maxIntens - baseline) / 20 + baseline;
            double fiveRT1 = 0;
            double fiveRT2 = 0;
            for (int it = 0; it < mIIndex; it++)
            {
                if (intensities[it] > fiveMax)
                {
                    fiveRT1 = starttimes[it];
                }
            }

            for (int it = mIIndex; it > 0; it--)
            {
                if (intensities[it] < fiveMax)
                {
                    fiveRT2 = starttimes[it];
                }
            }

            return fiveRT2 - fiveRT1;
        }


    }
}
