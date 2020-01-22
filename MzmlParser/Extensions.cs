using System.Collections.Generic;
using System.Linq;

namespace MzmlParser
{
    public static class Extensions
    {
        public static double[][] AsVerticalArrays(this List<(double,double)> list)
        {
            return new double[2][] { list.Select(x => x.Item1).ToArray(), list.Select(x => x.Item2).ToArray() };
        }

        public static double[][] AsHorizontalArrays(this List<(double, double)> list)
        {
            return list.Select(x => new double[2] { x.Item1, x.Item2 }).ToArray();
        }

        public static double[][] AsHorizontalArrays(this List<(double, double, double)> list)
        {
            return list.Select(x => new double[3] { x.Item1, x.Item2, x.Item3 }).ToArray();
        }
    }
}