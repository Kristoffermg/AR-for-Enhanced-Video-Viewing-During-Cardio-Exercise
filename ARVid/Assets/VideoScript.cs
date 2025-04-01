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
using UnityEngine.XR;
using TMPro;
using Accord.Statistics.Distributions.Univariate;
using static IntensityManager;
using Oculus.Interaction;


public class VideoScript : MonoBehaviour
{
    public AudioSource audio;
    public VideoPlayer video;

    public GameObject canvas;
    public GameObject camera;

    public GameObject centerEye;

    private readonly double startTime = 5; // How many seconds into the video it should start

    private bool videoPaused;

    UIManager uiManager;
    DataLogger dataLogger;
    IntensityManager intensityManager;


    void Start()
    {
        video.time = startTime;
        audio.time = (float)startTime;

        Debug.Log($"System display: {OVRPlugin.systemDisplayFrequency} | target: {Application.targetFrameRate}");
        OVRPlugin.systemDisplayFrequency = 120.0f; 
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
        QualitySettings.antiAliasing = 0;;

        string videoPath = Path.Combine(Application.streamingAssetsPath, "output.mp4");

        if (!File.Exists(videoPath))
        {
            Debug.LogError($"Video file not found at {videoPath}");
            return;
        }

        video.source = VideoSource.Url; 
        video.url = "file://" + videoPath; 

        video.Prepare();

        video.prepareCompleted += (VideoPlayer vp) =>
        {
            if (vp.isPrepared)
            {
                vp.Play();
            }
            else
            {
                Debug.LogError("Video preparation failed.");
            }
        };

        uiManager = GetComponent<UIManager>();
        dataLogger = GetComponent<DataLogger>();
        intensityManager = GetComponent<IntensityManager>();
        videoPaused = false;

        canvas.transform.LookAt(centerEye.transform);
        canvas.transform.Rotate(0, 180, 0);
    }

    void Update()
    {
        Vector3 headPosition = centerEye.transform.position;

        dataLogger.StoreFrameData(headPosition);
        uiManager.AdjustCanvasFOV();
        HandleControllerInput(headPosition);
        uiManager.MoveVideoPosition();
    }


    private void HandleControllerInput(Vector3 centerEyePosition)
    {
        if (OVRInput.Get(OVRInput.RawButton.LHandTrigger))
        {
            video.time = 0; // video.Stop() can cause flickering
            audio.Stop();
            video.Play();
            audio.Play();
        }

        if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger))
        {
            dataLogger.EnqueueData(centerEyePosition);
        }

        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            TogglePause();
        }

        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            uiManager.ChangeViewingExperience();
        }

        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            intensityManager.ChangeIntensityLevel();
        }
    }

    private void TogglePause()
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
}



