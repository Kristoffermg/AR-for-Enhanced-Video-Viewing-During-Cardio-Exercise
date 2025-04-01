using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


public class IntensityManager : MonoBehaviour
{
    public GameObject intensitySphere;
    public GameObject canvas;

    private PeakDetector peakDetector;

    public void Start()
    {
        peakDetector = new PeakDetector();

    }

    public static IntensityManager Instance { get; private set; }

    public IntensityLevel CurrentIntensity { get; private set; } = IntensityLevel.Low;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    private int currentFrame = 0;
    private static readonly int historicalDataSizeThreshold = 300; // the size of the sliding window
    private readonly int scale = historicalDataSizeThreshold / 100;

    //private readonly float lerpScale = 3.0f; // how quickly the linear interpolation of the canvas scale happens (smooth size change)
    private readonly float canvasDownscaleAmount = 0.0004f; // how much the size of the canvas should be changed based on intensity (higher number = greater size reduction)
    private readonly float minimumCanvasSize = 0.00005f;

    private Vector3 baseCanvasScale;
    private Vector3 velocity = Vector3.zero;

    public enum IntensityLevel
    {
        Low = 40,
        Medium = 30,
        High = 20
    }

    public void ChangeIntensityLevel()
    {
        switch (CurrentIntensity)
        {
            case IntensityLevel.Low:
                SetIntensity(IntensityLevel.Medium);
                Debug.Log("Intensity Level set to Medium");
                break;
            case IntensityLevel.Medium:
                SetIntensity(IntensityLevel.High);
                Debug.Log("Intensity Level set to High");
                break;
            case IntensityLevel.High:
                SetIntensity(IntensityLevel.Low);
                Debug.Log("Intensity Level set to Low");
                break;
        }
    }

    public void SetIntensity(IntensityLevel newIntensity)
    {
        CurrentIntensity = newIntensity;
    }

    public void Update()
    {
        currentFrame++;
    }

    void RunAdaptiveScaling(Queue<double> historicalXValues, Queue<double> historicalYValues, Queue<double> historicalZValues)
    {
        currentFrame = 0;

        List<double> xValues = new List<double>(historicalXValues);
        List<double> yValues = new List<double>(historicalYValues);
        List<double> zValues = new List<double>(historicalZValues);

        int intensity = CalculatePerceivedIntensity(xValues, yValues, zValues);

        if (intensity <= 1 * scale)
        {
            intensitySphere.GetComponent<Renderer>().material.color = Color.green;
        }
        else if (intensity <= 2 * scale)
        {
            intensitySphere.GetComponent<Renderer>().material.color = Color.yellow;
        }
        else if (intensity <= 3 * scale)
        {
            intensitySphere.GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            intensitySphere.GetComponent<Renderer>().material.color = Color.black;
        }
        Vector3 canvasScaleChange = new Vector3(canvasDownscaleAmount * intensity, canvasDownscaleAmount * intensity, canvasDownscaleAmount * intensity);

        Vector3 newScale = baseCanvasScale - canvasScaleChange;

        newScale = Vector3.Max(newScale, new Vector3(minimumCanvasSize, minimumCanvasSize, minimumCanvasSize));

        canvas.transform.localScale = Vector3.SmoothDamp(
            canvas.transform.localScale,
            newScale,
            ref velocity,
            0.6f
        );

        Debug.Log("intensity: " + intensity);
    }

    int xWeight = 5;
    int yWeight = 5;
    int zWeight = 5;

    int CalculatePerceivedIntensity(List<double> xValues, List<double> yValues, List<double> zValues)
    {
        //xValues = ApplyMedianFilter(xValues, 3);
        //xValues = GaussianSmooth(xValues, 11);
        //xValues = ApplySavitzkyGolay(xValues);

        //yValues = ApplyMedianFilter(yValues, 3);
        //yValues = GaussianSmooth(yValues, 11);
        //yValues = ApplySavitzkyGolay(yValues);

        //zValues = ApplyMedianFilter(zValues, 3);
        //zValues = GaussianSmooth(zValues, 11);
        //zValues = ApplySavitzkyGolay(zValues);

        int[] xPeaks = peakDetector.FindSignificantPeaks(xValues);
        int[] yPeaks = peakDetector.FindSignificantPeaks(yValues);
        int[] zPeaks = peakDetector.FindSignificantPeaks(zValues);

        int numberOfXPeaks = xPeaks.Length;
        int numberOfYPeaks = yPeaks.Length;
        int numberOfZPeaks = zPeaks.Length;

        int weightedXPeaks = numberOfXPeaks * xWeight;
        int weightedYPeaks = numberOfYPeaks * yWeight;
        int weightedZPeaks = numberOfZPeaks * zWeight;

        int weightedAveragePeaks = (weightedXPeaks + weightedYPeaks + weightedZPeaks) / (xWeight + yWeight + zWeight);

        //Debug.Log($"Peaks: {numberOfXPeaks},{numberOfYPeaks},{numberOfZPeaks} INTENSITY: {weightedAveragePeaks}");

        return weightedAveragePeaks;
    }

    List<double> ApplySavitzkyGolay(List<double> data, int windowSize = 5, int polyOrder = 2)
    {
        int halfWindow = windowSize / 2;
        List<double> smoothed = new List<double>(data);

        for (int i = halfWindow; i < data.Count - halfWindow; i++)
        {
            var x = Enumerable.Range(-halfWindow, windowSize).Select(v => (double)v).ToArray();
            var y = data.Skip(i - halfWindow).Take(windowSize).ToArray();
            var p = MathNet.Numerics.Fit.Polynomial(x, y, polyOrder);
            smoothed[i] = p[0];
        }

        return smoothed;
    }
    static List<double> ApplyMedianFilter(List<double> data, int kernelSize)
    {
        int halfSize = kernelSize / 2;
        List<double> filtered = new List<double>();

        for (int i = 0; i < data.Count; i++)
        {
            List<double> window = new List<double>();

            for (int j = -halfSize; j <= halfSize; j++)
            {
                int index = Math.Max(0, Math.Min(i + j, data.Count - 1));
                window.Add(data[index]);
            }

            filtered.Add(window.Median());
        }
        return filtered;
    }

    static List<double> GaussianSmooth(List<double> data, int kernelSize)
    {
        double sigma = kernelSize / 2.0;
        int halfSize = kernelSize / 2;
        double[] kernel = new double[kernelSize];
        double sum = 0;

        for (int i = 0; i < kernelSize; i++)
        {
            double x = i - halfSize;
            kernel[i] = Math.Exp(-0.5 * (x * x) / (sigma * sigma));
            sum += kernel[i];
        }

        for (int i = 0; i < kernelSize; i++)
            kernel[i] /= sum;

        List<double> smoothed = new List<double>();

        for (int i = 0; i < data.Count; i++)
        {
            double value = 0;
            for (int j = 0; j < kernelSize; j++)
            {
                int index = i + j - halfSize;
                if (index >= 0 && index < data.Count)
                    value += data[index] * kernel[j];
            }
            smoothed.Add(value);
        }

        return smoothed;
    }

    int[] FindSignificantPeaks(List<double> values)
    {
        if (values.Count < 3) return new int[0];

        List<int> peaks = new List<int>();

        double noiseThreshold = ComputeNoiseThreshold(values);

        for (int i = 1; i < values.Count - 1; i++)
        {
            if (values[i] > values[i - 1] && values[i] > values[i + 1])
            {
                if (values[i] > noiseThreshold)
                {
                    peaks.Add(i);
                }
            }
        }

        return peaks.ToArray();
    }

    double ComputeNoiseThreshold(List<double> values, double multiplier = 0.35)
    {
        double mean = values.Average();
        double stdDev = Math.Sqrt(values.Select(v => Math.Pow(v - mean, 2)).Average());
        return mean + (stdDev * multiplier);
    }
}