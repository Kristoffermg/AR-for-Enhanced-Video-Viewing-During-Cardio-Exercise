using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Video;
using MathNet.Numerics.Statistics;
using System;
using System.Linq;
using Unity.Properties;
using Accord.Statistics.Running;
using Oculus.Interaction.OVR.Input;

public class VideoScript : MonoBehaviour
{
    public AudioSource audio;
    public VideoPlayer video;

    public GameObject canvas;
    public GameObject camera;

    public GameObject centerEye;

    public GameObject intensitySphere;

    private Queue<double> xValues;
    private Queue<double> yValues;
    private Queue<double> zValues;

    private Queue<double> historicalXValues;
    private Queue<double> historicalYValues;
    private Queue<double> historicalZValues;

    private List<string> headPositionData;

    private Vector3 baseCanvasScale;

    private readonly double startTime = 5; // How many seconds into the video it should start

    void Start()
    {
        headPositionData = new List<string>();

        xValues = new Queue<double>();
        yValues = new Queue<double>();
        zValues = new Queue<double>();

        historicalXValues = new Queue<double>();
        historicalYValues = new Queue<double>();
        historicalZValues = new Queue<double>();

        baseCanvasScale = canvas.transform.localScale;

        video.time = startTime;
        audio.time = (float)startTime;

        //video.Play();
    }

    Vector3 previousVideoPosition = Vector3.zero;
    private float previousHeadsetHeight = 0.0f;

    private int currentRecordedFrame = 0;

    private readonly int perceivedIntensityUpdateInterval = 10; // how often the intensity should be recomputed/updated based on the number of frames passed
    private int currentFrame = 0;
    private static readonly int historicalDataSizeThreshold = 100; // the size of the sliding window

    private readonly int scale = historicalDataSizeThreshold / 100;

    private readonly float lerpScale = 3.0f; // how quickly the linear interpolation of the canvas scale happens (smooth size change)
    private readonly float canvasDownscaleAmount = 0.0002f; // how much the size of the canvas should be changed based on intensity (higher number = greater size reduction)
    private readonly float minimumCanvasSize = 0.00005f;

    bool videoPaused = false;

    void Update()
    {
        //OVRInput.Update();
        //OVRInput.FixedUpdate();

        canvas.transform.LookAt(centerEye.transform);
        canvas.transform.Rotate(0, 180, 0);


        Vector3 cameraPosition = camera.transform.position;
        Vector3 centerEyePosition = centerEye.transform.position;

        currentFrame++;

        Vector3 newVideoPosition = previousVideoPosition;

        if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger))
        {
            Vector3 leftControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand);
            leftControllerPosition.y += 0.2f;
            newVideoPosition = leftControllerPosition;
        }

        if (OVRInput.Get(OVRInput.RawButton.LHandTrigger))
        {
            video.Stop();
            audio.Stop();
            video.Play();
            audio.Play();
        }

        float heightDifference = previousHeadsetHeight - centerEyePosition.y;
        newVideoPosition.y -= heightDifference;

        canvas.transform.position = newVideoPosition;
        intensitySphere.transform.position = newVideoPosition;

        previousVideoPosition = newVideoPosition;
        previousHeadsetHeight = centerEyePosition.y;

        StoreFrameData(centerEyePosition);

        if (currentFrame == perceivedIntensityUpdateInterval)
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

            canvas.transform.localScale = Vector3.Lerp(canvas.transform.localScale, newScale, Time.deltaTime * lerpScale);

            //Debug.Log("intensity: " + intensity);
        }

        if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
        {
            currentRecordedFrame++;
            headPositionData.Add($"{currentRecordedFrame},{centerEyePosition.x},{centerEyePosition.y},{centerEyePosition.z}");
            xValues.Enqueue(centerEyePosition.x);
            yValues.Enqueue(centerEyePosition.y);
            zValues.Enqueue(centerEyePosition.z);
        }

        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            if (videoPaused)
            {
                video.Play();
                audio.Play();
            }
            else
            {
                video.Pause();
                audio.Pause();
                Debug.Log($"Paused at time: {video.time} seconds");
            }
            videoPaused = !videoPaused;
        }

        if (OVRInput.Get(OVRInput.RawButton.B))
        {
            List<double> x = new List<double>(xValues);
            List<double> y = new List<double>(yValues);
            List<double> z = new List<double>(zValues);

            x = ApplyMedianFilter(x, 3);
            x = GaussianSmooth(x, 11);
            x = ApplySavitzkyGolay(x);

            y = ApplyMedianFilter(y, 3);
            y = GaussianSmooth(y, 11);
            y = ApplySavitzkyGolay(y);

            z = ApplyMedianFilter(z, 3);
            z = GaussianSmooth(z, 11);
            z = ApplySavitzkyGolay(z);

            using (StreamWriter writer = new StreamWriter("C:\\Users\\test\\Documents\\Data\\headPositionData.csv"))
            {
                writer.WriteLine("frame,x,y,z");

                for (int i = 0; i < x.Count; i++)
                {
                    writer.WriteLine($"{i + 1},{x[i]},{y[i]},{z[i]}");
                }

                //foreach(string row in headPositionData)
                //{
                //    writer.WriteLine(row);
                //}
            }
        }

        //Vector3 cameraAngle = canvas.transform.rotation.eulerAngles;
        //cameraAngle[2] = canvas.transform.rotation.eulerAngles[2];

        //Quaternion canvasRotation = canvas.transform.rotation;
        //canvasRotation.eulerAngles = cameraAngle;

        //canvas.transform.rotation = canvasRotation;
    }

    // Maintains a sliding window of the past 400 (x,y,z) values
    void StoreFrameData(Vector3 centerEyePosition)
    {
        historicalXValues.Enqueue(centerEyePosition.x);
        historicalYValues.Enqueue(centerEyePosition.y);
        historicalZValues.Enqueue(centerEyePosition.z);

        if (historicalXValues.Count >= historicalDataSizeThreshold)
        {
            historicalXValues.Dequeue();
            historicalYValues.Dequeue();
            historicalZValues.Dequeue();
        }
    }

    int xWeight = 5;
    int yWeight = 5;
    int zWeight = 5;

    int CalculatePerceivedIntensity(List<double> xValues, List<double> yValues, List<double> zValues)
    {
        xValues = ApplyMedianFilter(xValues, 3);
        xValues = GaussianSmooth(xValues, 11);
        xValues = ApplySavitzkyGolay(xValues);

        yValues = ApplyMedianFilter(yValues, 3);
        yValues = GaussianSmooth(yValues, 11);
        yValues = ApplySavitzkyGolay(yValues);

        zValues = ApplyMedianFilter(zValues, 3);
        zValues = GaussianSmooth(zValues, 11);
        zValues = ApplySavitzkyGolay(zValues);

        int[] xPeaks = FindSignificantPeaks(xValues);
        int[] yPeaks = FindSignificantPeaks(yValues);
        int[] zPeaks = FindSignificantPeaks(zValues);

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
    static int[] FindPeaks(List<double> data)
    {
        List<int> peaks = new List<int>();
        for (int i = 1; i < data.Count - 1; i++)
        {
            if (data[i] > data[i - 1] && data[i] > data[i + 1])
            {
                //Debug.Log($"Adding peak: {data[i]}");
                peaks.Add(i);
            }
        }
        return peaks.ToArray();
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



