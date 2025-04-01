
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class DataLogger : MonoBehaviour
{
    private Queue<double> xValues;
    private Queue<double> yValues;
    private Queue<double> zValues;

    private Queue<double> historicalXValues;
    private Queue<double> historicalYValues;
    private Queue<double> historicalZValues;

    private List<string> headPositionData;

    private uint currentRecordedFrame = 0;


    void Start()
    {
        xValues = new Queue<double>();
        yValues = new Queue<double>();
        zValues = new Queue<double>();

        historicalXValues = new Queue<double>();
        historicalYValues = new Queue<double>();
        historicalZValues = new Queue<double>();

        headPositionData = new List<string>();
    }

    private static readonly int historicalDataSizeThreshold = 300; // the size of the sliding window

    public void EnqueueData(Vector3 centerEyePosition)
    {
        currentRecordedFrame++;
        headPositionData.Add($"{currentRecordedFrame},{centerEyePosition.x},{centerEyePosition.y},{centerEyePosition.z}");

        xValues.Enqueue(centerEyePosition.x);
        yValues.Enqueue(centerEyePosition.y);
        zValues.Enqueue(centerEyePosition.z);
    }

    // Maintains a sliding window of the (x,y,z) values
    public void StoreFrameData(Vector3 centerEyePosition)
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

    private int fileNumber = 1;

    public void WriteHeadPositionData(int intensity = 0)
    {
        //List<double> x = new List<double>(xValues);
        //List<double> y = new List<double>(yValues);
        //List<double> z = new List<double>(zValues);

        List<double> x = new List<double>(xValues);
        List<double> y = new List<double>(yValues);
        List<double> z = new List<double>(zValues);

        //x = ApplyMedianFilter(x, 3);
        //x = GaussianSmooth(x, 11);
        //x = ApplySavitzkyGolay(x);

        //y = ApplyMedianFilter(y, 3);
        //y = GaussianSmooth(y, 11);
        //y = ApplySavitzkyGolay(y);

        //z = ApplyMedianFilter(z, 3);
        //z = GaussianSmooth(z, 11);
        //z = ApplySavitzkyGolay(z);

        if (x.Count == 0 || y.Count == 0 || z.Count == 0)
        {
            Debug.LogWarning("No head position data to write, skipping...");
            return;
        }

        using (StreamWriter writer = new StreamWriter($"C:\\Users\\test\\Documents\\Data\\headPositionData{fileNumber}_{intensity}.csv"))
        {
            writer.WriteLine("frame,x,y,z");

            for (int i = 0; i < x.Count; i++)
            {
                writer.WriteLine($"{i + 1},{x[i]},{y[i]},{z[i]}");
            }
        }
        fileNumber++;
    }
}