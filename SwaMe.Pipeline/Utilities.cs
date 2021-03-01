#nullable enable

using System.Collections.Generic;

namespace SwaMe.Pipeline
{
    public static class Utilities
    {
        /// <summary>
        /// Return the largest value in lookup, and the value at the same index at alongForTheRide. If two or more values in lookup are equal, the lowest index is used.
        /// </summary>
        /// <remarks>Preconditions: alongForTheRide is at least as long as lookup; lookup is of length > 0</remarks>
        public static (float maxValue, float otherValueAtIndexOfMaxValue) LookUpMaxValue(float[] lookup, float[] alongForTheRide)
        {
            float maxValueSoFar = lookup[0];
            int indexOfMaxValueSoFar = 0;
            for (int i = 1; i < lookup.Length; i++)
                if (lookup[i] > maxValueSoFar)
                {
                    maxValueSoFar = lookup[i];
                    indexOfMaxValueSoFar = i;
                }
            return (maxValueSoFar, alongForTheRide[indexOfMaxValueSoFar]);
        }

        /// <summary>
        /// A simple extension of AddRange for IList.
        /// </summary>
        public static void AddRange<T>(this IList<T> victim, IEnumerable<T> toAdd)
        {
            foreach (T toTransfer in toAdd)
                victim.Add(toTransfer);
        }

        public static void AddRenderedMzqcMetricsTo(IDictionary<string, dynamic> mergedRenderedMetrics, IMzqcMetrics rawMetrics)
        {
            foreach (KeyValuePair<string, dynamic> pair in rawMetrics.RenderableMetrics)
                mergedRenderedMetrics.Add(pair);
        }

        public static void AddRenderedMzqcMetricsTo(IDictionary<string, dynamic> mergedRenderedMetrics, IList<IMzqcMetrics> rawMetricList)
        {
            foreach (IMzqcMetrics rawMetrics in rawMetricList)
                foreach (KeyValuePair<string, dynamic> pair in rawMetrics.RenderableMetrics)
                    mergedRenderedMetrics.Add(pair);
        }

    }
}
