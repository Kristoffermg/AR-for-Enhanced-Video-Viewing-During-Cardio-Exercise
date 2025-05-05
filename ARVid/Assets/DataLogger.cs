using Meta.XR.MRUtilityKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataLogger : MonoBehaviour
{
    public List<(double x, double y, double z)> recentHeadPositionData;
    private List<string> headPositionData;
    private bool isRecording = false;
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

        string folderName = $"StudyParticipant_{UIManager.StudyParticipantNumber}";
        string path = Path.Combine(Application.persistentDataPath, folderName);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Debug.Log("Directory created at: " + path);
        }
        else
        {
            Debug.Log("Directory already exists at: " + path);
        }

        string viewingExperience = Enum.GetName(typeof(UIManager.ViewingExperience), UIManager.CurrentViewingExperience);

        string fileName = $"{folderName}/{VideoScript.CurrentCardioMachine}_{viewingExperience}.csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        // This is to avoid overwriting the old file in case an accidental recording was started
        int fileNameIteration = 1;
        string directory = Path.GetDirectoryName(filePath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);
        string newFilePath = filePath;

        while (File.Exists(newFilePath))
        {
            newFilePath = Path.Combine(directory, $"{fileNameWithoutExtension}_{fileNameIteration}{extension}");
            fileNameIteration++;
        }

        filePath = newFilePath;

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("frame,x,y,z");
            foreach (string line in headPositionData)
            {
                writer.WriteLine(line);
            }
        }

        Debug.Log("CSV written to: " + filePath);
    }
}
