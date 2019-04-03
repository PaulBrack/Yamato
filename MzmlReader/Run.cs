using System;
using System.Collections.Generic;

namespace MzmlParser
{
    public class Run
    {
        public Run()
        {
            Ms1Scans = new List<Scan>();
            Ms2Scans = new List<Scan>();
            Ms1Tic = new List<Tuple<float, float>>();
            Ms2Tic = new List<Tuple<float, float>>();
        }
        public List<Scan> Ms1Scans { get; set; }
        public List<Scan> Ms2Scans { get; set; }
        public List<Tuple<float, float>> Ms1Tic { get; set; }
        public List<Tuple<float, float>> Ms2Tic { get; set; }
    }
}
