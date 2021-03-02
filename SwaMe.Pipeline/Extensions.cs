#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace SwaMe.Pipeline
{
    public static class Extensions
    {
        public static T[][] AsVerticalArrays<T>(this IEnumerable<(T, T)> list)
        {
            return new T[2][] { list.Select(x => x.Item1).ToArray(), list.Select(x => x.Item2).ToArray() };
        }

        public static T[][] AsHorizontalArrays<T>(this IEnumerable<(T, T)> list)
        {
            return list.Select(x => new T[2] { x.Item1, x.Item2 }).ToArray();
        }

        public static T[][] AsHorizontalArrays<T>(this IEnumerable<(T, T, T)> list)
        {
            return list.Select(x => new T[3] { x.Item1, x.Item2, x.Item3 }).ToArray();
        }

        public static TSource MaxEvaluatedWith<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, bool> comparator)
        {
            bool first = true;
            TSource maxSoFar = default;
            foreach (TSource s in source)
            {
                if (first)
                {
                    first = false;
                    maxSoFar = s;
                }
                else
                {
                    // If we get here, first is false so maxSoFar has a value. TODO: Some code assertion to that effect that doesn't affect performance.
                    if (comparator(s, maxSoFar))
                        maxSoFar = s;
                }
            }
            return maxSoFar;
        }
    }
}