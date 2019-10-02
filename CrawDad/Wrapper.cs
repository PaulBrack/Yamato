using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace CrawDad
{
    public class Wrapper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        pwiz.Crawdad.CrawdadPeakFinder cpf = new pwiz.Crawdad.CrawdadPeakFinder();

        public void SetChromatogram(double[] intensities, double[] starttimes)
        {
            cpf.SetChromatogram(intensities, starttimes);
        }
        public List<pwiz.Crawdad.CrawdadPeak> CalcPeaks()
        {
        return cpf.CalcPeaks();
        }
    }

  

    public class CrawPeak
    {
        pwiz.Crawdad.CrawdadPeak crawPeak;
    }
    
}
