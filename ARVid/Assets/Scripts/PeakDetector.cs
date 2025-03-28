using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PeakDetector : MonoBehaviour
{
    public int[] FindSignificantPeaks(List<double> values, double multiplier = 0.35, int windowSize = 5)
    {
        if (values.Count < 3) return Array.Empty<int>();

        List<int> peaks = new List<int>();

        double globalNoiseThreshold = ComputeNoiseThreshold(values, multiplier);

        for (int i = 1; i < values.Count - 1; i++)
        {
            if (values[i] > values[i - 1] && values[i] > values[i + 1])
            {
                double localThreshold = ComputeLocalNoiseThreshold(values, i, windowSize, multiplier);

                double leftMin = values.Take(i).DefaultIfEmpty(values[i]).Min();
                double rightMin = values.Skip(i + 1).DefaultIfEmpty(values[i]).Min();
                double prominence = values[i] - Math.Max(leftMin, rightMin);

                if (prominence > localThreshold && values[i] > globalNoiseThreshold)
                {
                    peaks.Add(i);
                }
            }
        }

        return peaks.ToArray();
    }

    private double ComputeNoiseThreshold(List<double> values, double multiplier)
    {
        double mean = values.Average();
        double stdDev = Math.Sqrt(values.Select(v => Math.Pow(v - mean, 2)).Average());
        return mean + (stdDev * multiplier);
    }

    private double ComputeLocalNoiseThreshold(List<double> values, int index, int windowSize, double multiplier)
    {
        int start = Math.Max(0, index - windowSize);
        int end = Math.Min(values.Count - 1, index + windowSize);
        List<double> localWindow = values.GetRange(start, end - start + 1);

        return ComputeNoiseThreshold(localWindow, multiplier);
    }
}
