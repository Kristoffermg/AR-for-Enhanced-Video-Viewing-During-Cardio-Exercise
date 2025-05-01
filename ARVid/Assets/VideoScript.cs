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
using Accord.Statistics.Distributions.Univariate;
using static IntensityManager;
using Oculus.Interaction;

public class VideoScript : MonoBehaviour
{
    public static bool pcLinkMode = true;
    public enum Video
    {
        FamilyGuy,
        TheOffice,
        BrooklynNineNine,
        OnePunchMan
    }

    public static VideoPlayer videoPlayer;
    public VideoPlayer inspectorVideoPlayer;


    public GameObject canvas;
    public GameObject camera;
    public GameObject centerEye;
    private uint currentFrame;
    private static bool videoPaused;
    private UIManager uiManager;
    private DataLogger dataLogger;
    private IntensityManager intensityManager;
    private bool hasVibrated = false;
    private bool lockedIn = false;

    private const float VibrationFrequency = 1.0f;
    private const float VibrationAmplitude = 1.0f;
    private const float VibrationDurationRegular = 0.1f;
    private const float VibrationDurationRecording = 0.3f;
    private const float VibrationPause = 0.1f;

    public static string selectedSeries = "FamilyGuy";

    private static int selectedEpisode = 1;
    public static int SelectedEpisode
    {
        get => selectedEpisode;
        set
        {
            ChangeSelectedSeries(selectedSeries, value, UIManager.CurrentViewingExperience == UIManager.ViewingExperience.Phone);
            selectedEpisode = value;
        }
    }

    public static string CurrentCardioMachine { get; private set; } = "Treadmill";

    void Awake()
    {
        inspectorVideoPlayer = GetComponent<VideoPlayer>();
        videoPlayer = inspectorVideoPlayer;
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer component not found");
        }
    }

    void Start()
    {
        InitializeComponents();
        SetupVideoPlayer();
        videoPaused = false;
        currentFrame = 0;
        canvas.transform.LookAt(centerEye.transform);
        canvas.transform.Rotate(0, 180, 0);
    }

    void Update()
    {
        currentFrame++;
        Vector3 headPosition = centerEye.transform.position;

        if(UIManager.CurrentViewingExperience == UIManager.ViewingExperience.Adaptive)
            uiManager.AdjustAdaptiveVideoFOV();

        if (OVRInput.GetControllerPositionTracked(OVRInput.Controller.LTouch) && !lockedIn)
            uiManager.MoveVideoPosition();

        HandleControllerInput(headPosition);
        dataLogger.EnqueueRecentData(headPosition);
        if (currentFrame >= intensityManager.intensityUpdateRate)
        {
            currentFrame = 0;
            //intensityManager.ComputeIntensity(dataLogger.recentHeadPositionData);
        }

        dataLogger.EnqueueData(headPosition);
    }

    private void InitializeComponents()
    {
        uiManager = GetComponent<UIManager>();
        dataLogger = GetComponent<DataLogger>();
        intensityManager = GetComponent<IntensityManager>();

        if (uiManager == null) { Debug.LogError("UIManager not found in scene"); }
        if (dataLogger == null) { Debug.LogError("DataLogger not found in scene"); }
        if (intensityManager == null) { Debug.LogError("IntensityManager not found in scene"); }
    }

    private void SetupVideoPlayer()
    {
        string videoPath = "";
        if (pcLinkMode)
        {
            videoPath = "Assets/ep1.mp4";
        }
        else
        {
            videoPath = Path.Combine(Application.persistentDataPath, "/sdcard/Android/data/com.UnityTechnologies.com.unity.template.urpblank/files/Content", selectedSeries, "mp4", $"ep{selectedEpisode}.mp4");
        }

        if (!File.Exists(videoPath))
        {
            Debug.LogError($"Video file not found at {videoPath}");
            return;
        }

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = "file://" + videoPath;
        videoPlayer.Prepare();
    }

    private void HandleControllerInput(Vector3 centerEyePosition)
    {
        if (OVRInput.GetControllerPositionTracked(OVRInput.Controller.LTouch))
        {
            HandleLeftControllerInput();
        }

        if (OVRInput.GetControllerPositionTracked(OVRInput.Controller.RTouch))
        {
            HandleRightControllerInput(centerEyePosition);
        }
    }

    private void HandleRightControllerInput(Vector3 centerEyePosition)
    {
        float verticalInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
        float horizontalInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

        if (OVRInput.Get(OVRInput.RawButton.RHandTrigger))
        {
            HandleHandTriggerInput(horizontalInput, verticalInput);
        }
        else
        {
            if (!uiManager.SettingsMenuEnabled)
                HandleThumbstickInput(verticalInput);
        }
    }

    private void HandleLeftControllerInput()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            lockedIn = !lockedIn;
            StartCoroutine(VibrateXTimes(1, VibrationDurationRegular, false));
        }

        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            switch (CurrentCardioMachine)
            {
                case "Treadmill":
                    StartCoroutine(VibrateXTimes(1, VibrationDurationRegular, false));
                    CurrentCardioMachine = "Elliptical";
                    break;
                case "Elliptical":
                    StartCoroutine(VibrateXTimes(2, VibrationDurationRegular, false));
                    CurrentCardioMachine = "Row";
                    break;
                case "Row":
                    StartCoroutine(VibrateXTimes(3, VibrationDurationRegular, false));
                    CurrentCardioMachine = "Treadmill";
                    break;
            }
        }
    }

    private void HandleHandTriggerInput(float horizontalInput, float verticalInput)
    {
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)) // Toggle settings menu
        {
            uiManager.ToggleSettingsMenu();
        }

        if (uiManager.SettingsMenuEnabled)
            return;

        if (OVRInput.GetDown(OVRInput.RawButton.A)) // Restart video
        {
            videoPlayer.time = 0;
            videoPlayer.Stop();
        }

        if (OVRInput.GetDown(OVRInput.RawButton.B)) // Start video and record head movement for X minutes
        {
            dataLogger.StartOrStopRecording();
            StartCoroutine(VibrateXTimes(1, VibrationDurationRecording, true));
        }

        if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickDown)) // Cancel recording
        {
            dataLogger.CancelRecording();
        }

        if (videoPlayer.isPrepared && videoPlayer.isPlaying)
        {
            HandleVideoPlayback(horizontalInput, verticalInput);
        }
    }

    private void HandleThumbstickInput(float verticalInput)
    {
        if (OVRInput.GetDown(OVRInput.RawButton.A)) // Start/Pause
        {
            TogglePause();
        }

        if (OVRInput.GetDown(OVRInput.RawButton.B)) // Change method/viewing experience
        {
            int currentViewingExperience = uiManager.ChangeViewingExperience();
            ChangeSelectedSeries(selectedSeries, UIManager.CurrentViewingExperience == UIManager.ViewingExperience.Phone);
            StartCoroutine(VibrateXTimes(currentViewingExperience, VibrationDurationRegular, true));
        }

        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)) // Change intensity (Low -> Medium -> High)
        {
            intensityManager.ChangeIntensityLevel();
            uiManager.SetIntensityScaleAdjustment(true);
            switch (CurrentIntensity)
            {
                case IntensityLevel.Low:
                    StartCoroutine(VibrateXTimes(1, VibrationDurationRegular, true));
                    uiManager.AdjustVideoFOV((float)IntensityLevel.Low, 0.5f);
                    break;
                case IntensityLevel.Medium:
                    StartCoroutine(VibrateXTimes(2, VibrationDurationRegular, true));
                    uiManager.AdjustVideoFOV((float)IntensityLevel.Medium, 0.5f);
                    break;
                case IntensityLevel.High:
                    StartCoroutine(VibrateXTimes(3, VibrationDurationRegular, true));
                    uiManager.AdjustVideoFOV((float)IntensityLevel.High, 0.5f);
                    break;
            }
        }

        HandleRecordingDurationAdjustment(verticalInput);
    }

    public static void DecrementEpisode()
    {
        if (selectedEpisode > 1)
        {
            SelectedEpisode--;
        }
        Debug.Log($"Decrement episode called: {selectedEpisode}");
    }

    public static void IncrementEpisode()
    {
        if (selectedEpisode < 3)
        {
            selectedEpisode++;
        }
        Debug.Log($"Increment episode called: {selectedEpisode}");
    }

    public static void ChangeSelectedSeries(string series, bool phone)
    {
        selectedSeries = series;
        ChangeSelectedSeries(selectedSeries, selectedEpisode, phone);
    }

    public static void ChangeSelectedSeries(string series, int episode, bool phone)
    {
        selectedSeries = series;
        selectedEpisode = episode;

        string fileName = $"ep{selectedEpisode}{(phone ? "_phone" : "")}.mp4";
        string videoPath;

        if (pcLinkMode)
        {
            videoPath = Path.Combine("Assets", fileName);
        }
        else
        {
            videoPath = Path.Combine(Application.persistentDataPath, "Content", series, "mp4", fileName);
        }

        if (!File.Exists(videoPath))
        {
            Debug.LogError($"Video file not found at {videoPath}");
            return;
        }

        string normalizedPath = videoPath.Replace("\\", "/");
        string finalUrl = "file://" + normalizedPath;

        if (videoPlayer.url == finalUrl)
        {
            Debug.Log("Same video url encountered, wont switch video source");
            return;
        }

        Debug.Log($"Selected video: {selectedSeries} episode {selectedEpisode} {(phone ? "phone" : "")}");

        bool switchResolution = (videoPlayer.url.Contains("_phone") && !finalUrl.Contains("_phone")) ||
                                (!videoPlayer.url.Contains("_phone") && finalUrl.Contains("_phone"));

        double videoTime = videoPlayer.time;

        videoPlayer.url = finalUrl;

        if (switchResolution)
        {
            Debug.Log($"Old time: {videoPlayer.time}, New time: {videoTime}");
            videoPlayer.time = videoTime;
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.Prepare(); 
        }
        else
        {
            videoPlayer.Play();
        }

        videoPlayer.loopPointReached -= OnVideoEnded; // to make sure the video is only "subscribed" once
        videoPlayer.loopPointReached += OnVideoEnded;
    }

    static void OnVideoPrepared(VideoPlayer source)
    {
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.Play();
    }

    static void OnVideoEnded(VideoPlayer source)
    {
        videoPlayer.loopPointReached -= OnVideoEnded;
        ChangeSelectedSeries(selectedSeries, selectedEpisode + 1, UIManager.CurrentViewingExperience == UIManager.ViewingExperience.Phone);
    }



    private void HandleVideoPlayback(float horizontalInput, float verticalInput)
    {
        if (horizontalInput > 0.5f) // Fast forward
        {
            videoPlayer.playbackSpeed = 4;
            UIManager.changedPlaybackTime = true;
        }
        else if (horizontalInput < -0.5f) // Skip backwards a little
        {
            videoPlayer.time -= 10;
            UIManager.changedPlaybackTime = true;
        }
        else if (verticalInput > 0.5f) // Go forward 1 minute
        {
            videoPlayer.time +=  60;
            UIManager.changedPlaybackTime = true;
        }
        else if (verticalInput < -0.5f) // Go backwards 1 minute
        {
            videoPlayer.time -= 60;
            UIManager.changedPlaybackTime = true;
        }
        else // Set to normal playback
        {
            videoPlayer.playbackSpeed = 1;
        }
    }

    private void HandleRecordingDurationAdjustment(float verticalInput)
    {
        if (verticalInput > 0.5f && !hasVibrated) // Recording length + 1min
        {
            if (dataLogger.RecordingDuration < 5)
            {
                dataLogger.RecordingDuration += 1;
                hasVibrated = true;
            }
            StartCoroutine(VibrateXTimes(dataLogger.RecordingDuration, VibrationDurationRegular, true));
        }
        else if (verticalInput < -0.5f && !hasVibrated) // Recording length - 1min
        {
            if (dataLogger.RecordingDuration > 1)
            {
                dataLogger.RecordingDuration -= 1;
                hasVibrated = true;
            }
            StartCoroutine(VibrateXTimes(dataLogger.RecordingDuration, VibrationDurationRegular, true));
        }
        else if (Mathf.Abs(verticalInput) < 0.1f && hasVibrated)
        {
            hasVibrated = false;
        }
    }

    private IEnumerator VibrateXTimes(int vibrations, float vibrationDuration, bool rightController)
    {
        for (int i = 0; i < vibrations; i++)
        {
            if(rightController)
            {
                OVRInput.SetControllerVibration(VibrationFrequency, VibrationAmplitude, OVRInput.Controller.RTouch);
                yield return new WaitForSeconds(vibrationDuration);
                OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
                yield return new WaitForSeconds(VibrationPause);
            }
            else
            {
                OVRInput.SetControllerVibration(VibrationFrequency, VibrationAmplitude, OVRInput.Controller.LTouch);
                yield return new WaitForSeconds(vibrationDuration);
                OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
                yield return new WaitForSeconds(VibrationPause);
            }
        }
    }
    private void TogglePause()
    {
        if (videoPaused)
        {
            videoPlayer.Play();
        }
        else
        {
            videoPlayer.Pause();
            Debug.Log($"Paused at time: {videoPlayer.time} seconds");
        }
        videoPaused = !videoPaused;
    }



}