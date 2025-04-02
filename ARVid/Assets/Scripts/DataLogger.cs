using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataLogger : MonoBehaviour
{
    private List<string> headPositionData;
    private bool isRecording = false;
    private float recordingStartTime;
    private int fileNumber = 1;
    private uint currentRecordedFrame = 0;

    private static readonly float recordingDuration = 10f; 

    void Start()
    {
        headPositionData = new List<string>();
    }

    public void StartOrResetRecording()
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

    private void StartRecording()
    {
        isRecording = true;
        recordingStartTime = Time.time;
        headPositionData.Clear();
        currentRecordedFrame = 0;
        Debug.Log("Recording started...");
        StartCoroutine(StopAfterDuration());
    }

    private IEnumerator StopAfterDuration()
    {
        yield return new WaitForSeconds(recordingDuration);
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

    private void WriteHeadPositionData()
    {
        if (headPositionData.Count == 0)
        {
            Debug.LogWarning("No head position data recorded.");
            return;
        }

        string filePath = $"C:\\Users\\test\\Documents\\Data\\headPositionData_{fileNumber}.csv";
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
