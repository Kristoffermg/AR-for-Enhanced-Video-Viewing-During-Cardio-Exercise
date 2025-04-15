using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class IntensityManager : MonoBehaviour
{
    public GameObject intensitySphere;
    public GameObject canvas;
    private InferenceManager inferenceManager;
    [SerializeField] private UIManager uiManager;

    public static IntensityManager Instance { get; private set; }

    public static IntensityLevel CurrentIntensity { get; private set; } = IntensityLevel.Low;

    private int currentFrame = 0;
    private static readonly int historicalDataSizeThreshold = 300; // the size of the sliding window
    private readonly int scale = historicalDataSizeThreshold / 100;
    private readonly float canvasDownscaleAmount = 0.0004f; // how much the size of the canvas should be changed based on intensity (higher number = greater size reduction)
    private readonly float minimumCanvasSize = 0.00005f;
    public readonly uint intensityUpdateRate = 200;
    private Vector3 baseCanvasScale;
    private Vector3 velocity = Vector3.zero;

    public enum IntensityLevel
    {
        Low = 40,
        Medium = 30,
        High = 20
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // prevent duplicate
        }
    }

    private void Start()
    {
        inferenceManager = new InferenceManager();
        uiManager = new UIManager();
    }

    private void Update()
    {
        currentFrame++;
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
        uiManager.AdjustVideoFOV((float)CurrentIntensity);
    }

    /// <summary>
    /// Computes model features from recent head position data.
    /// </summary>
    /// <param name="recentHeadPositionData">List of recent head position data.</param>
    /// <returns>Array of computed features.</returns>
    private float[] ComputeModelFeatures(List<(double X, double Y, double Z)> recentHeadPositionData)
    {
        var xVals = recentHeadPositionData.Select(p => p.X).ToArray();
        var yVals = recentHeadPositionData.Select(p => p.Y).ToArray();
        var zVals = recentHeadPositionData.Select(p => p.Z).ToArray();
        var meanX = xVals.Mean();
        var meanY = yVals.Mean();
        var meanZ = zVals.Mean();
        var stdX = xVals.StandardDeviation();
        var stdY = yVals.StandardDeviation();
        var stdZ = zVals.StandardDeviation();
        var energyX = xVals.Sum(v => v * v);
        var energyY = yVals.Sum(v => v * v);
        var energyZ = zVals.Sum(v => v * v);
        var sadX = xVals.Skip(1).Zip(xVals, (curr, prev) => Math.Abs(curr - prev)).Sum();
        var sadY = yVals.Skip(1).Zip(yVals, (curr, prev) => Math.Abs(curr - prev)).Sum();
        var sadZ = zVals.Skip(1).Zip(zVals, (curr, prev) => Math.Abs(curr - prev)).Sum();
        return new float[]
        {
            (float)meanX, (float)meanY, (float)meanZ,
            (float)stdX, (float)stdY, (float)stdZ,
            (float)energyX, (float)energyY, (float)energyZ,
            (float)sadX, (float)sadY, (float)sadZ
        };
    }

}