using Meta.XR.MRUtilityKit;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataLogger : MonoBehaviour
{
    public List<(double x, double y, double z)> recentHeadPositionData;
    private List<string> headPositionData;
    private bool isRecording = false;
    private int fileNumber = 1;
    private uint currentRecordedFrame = 0;
    private int recordingDuration = 1;
    public int RecordingDuration
    {
        get { return recordingDuration; }
        set { recordingDuration = value; }
    }

    void Start()
    {
        headPositionData = new List<string>();
        recentHeadPositionData = new List<(double x, double y, double z)>();
    }

    public void StartOrStopRecording()
    {
        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    public void CancelRecording()
    {
        if (!isRecording) return;
        isRecording = false;
        Debug.Log("Recording cancelled.");
    }

    private void StartRecording()
    {
        isRecording = true;
        headPositionData.Clear();
        currentRecordedFrame = 0;
        Debug.Log("Recording started...");
        StartCoroutine(StopAfterDuration());
    }

    private IEnumerator StopAfterDuration()
    {
        yield return new WaitForSeconds(recordingDuration * 60);
        StopRecording();
    }

    private void StopRecording()
    {
        if (!isRecording) return;
        isRecording = false;
        WriteHeadPositionData();
        Debug.Log("Recording stopped and data saved.");
    }

    public void EnqueueData(Vector3 centerEyePosition)
    {
        if (!isRecording) return;
        currentRecordedFrame++;
        headPositionData.Add($"{currentRecordedFrame},{centerEyePosition.x},{centerEyePosition.y},{centerEyePosition.z}");
    }

    public void EnqueueRecentData(Vector3 centerEyePosition, uint dataSizeCap = 1000)
    {
        if (recentHeadPositionData.Count >= dataSizeCap)
            recentHeadPositionData.RemoveAt(0);
        recentHeadPositionData.Add((centerEyePosition.x, centerEyePosition.y, centerEyePosition.z));
    }

    private void WriteHeadPositionData()
    {
        if (headPositionData.Count == 0)
        {
            Debug.LogWarning("No head position data recorded.");
            return;
        }
        string filePath = Path.Combine(Application.persistentDataPath, $"{fileNumber}.csv");
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("frame,x,y,z");
            foreach (string line in headPositionData)
            {
                writer.WriteLine(line);
            }
        }
        fileNumber++;
        Debug.Log($"Data saved to {filePath}");
    }
}
