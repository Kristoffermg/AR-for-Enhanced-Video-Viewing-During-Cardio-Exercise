using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using static IntensityManager;

public class VideoScript : MonoBehaviour
{
    public static bool pcLinkMode = false;
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
    private const float VibrationDurationRecording = 0.6f;
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

        videoPlayer.loopPointReached -= OnVideoEnded;
        videoPlayer.loopPointReached += OnVideoEnded;
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

        if (OVRInput.GetDown(OVRInput.RawButton.LHandTrigger))
        {
            uiManager.LookAtCamera();
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
            StartCoroutine(VibrateXTimes(1, VibrationDurationRegular, true));
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
            if (UIManager.CurrentViewingExperience != UIManager.ViewingExperience.Adaptive)
                return;

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

    private static double lastRequestedTime = 0;
    private static bool changingVideo = false;
    private static string pendingSeries = null;
    private static int pendingEpisode = -1;
    private static bool pendingPhone = false;
    private static bool hasPendingChange = false;

    public static void ChangeSelectedSeries(string series, int episode, bool phone)
    {
        if (changingVideo)
        {
            Debug.Log($"Video change already in progress. Queuing change to {series} ep{episode} {(phone ? "phone" : "")}");
            pendingSeries = series;
            pendingEpisode = episode;
            pendingPhone = phone;
            hasPendingChange = true;
            return;
        }

        changingVideo = true;

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
            changingVideo = false;
            shouldPlayAfterPrepare = false;
            forceStartFromBeginning = false;

            ProcessPendingChange();
            return;
        }
        string normalizedPath = videoPath.Replace("\\", "/");
        string finalUrl = "file://" + normalizedPath;
        if (videoPlayer.url == finalUrl)
        {
            Debug.Log("Same video url encountered, wont switch video source");
            changingVideo = false;
            shouldPlayAfterPrepare = false;
            forceStartFromBeginning = false;

            ProcessPendingChange();
            return;
        }
        Debug.Log($"Selected video: {selectedSeries} episode {selectedEpisode} {(phone ? "phone" : "")}");
        bool switchResolution = (videoPlayer.url.Contains("_phone") && !finalUrl.Contains("_phone")) ||
                                (!videoPlayer.url.Contains("_phone") && finalUrl.Contains("_phone"));

        if (!forceStartFromBeginning)
        {
            double currentTime = videoPlayer.time;
            if (currentTime > 0)
            {
                lastRequestedTime = currentTime;
            }
        }

        void OnPrepareCompleted(VideoPlayer vp)
        {
            if (forceStartFromBeginning)
            {
                Debug.Log("Forcing video to start from beginning (time = 0)");
                vp.time = 0;
                forceStartFromBeginning = false;
            }
            else
            {
                Debug.Log($"Video prepared. Setting time from {vp.time} to {lastRequestedTime}");
                vp.time = lastRequestedTime;
            }

            if (shouldPlayAfterPrepare)
            {
                Debug.Log("Auto-playing video after preparation");
                vp.Play();
                videoPaused = false;
            }
            else
            {
                vp.Pause();
                videoPaused = true;
            }

            vp.prepareCompleted -= OnPrepareCompleted; 
            changingVideo = false;

            shouldPlayAfterPrepare = false;

            ProcessPendingChange();
        }

        videoPlayer.Pause();

        videoPlayer.url = finalUrl;

        videoPlayer.prepareCompleted += OnPrepareCompleted;
        videoPlayer.Prepare();

        if (switchResolution)
        {
            Debug.Log($"Resolution switch initiated. Saved time: {lastRequestedTime}");
        }

        videoPlayer.loopPointReached -= OnVideoEnded;
        videoPlayer.loopPointReached += OnVideoEnded;
    }

    private static void ProcessPendingChange()
    {
        if (hasPendingChange)
        {
            Debug.Log($"Processing pending change to {pendingSeries} ep{pendingEpisode} {(pendingPhone ? "phone" : "")}");
            string tempSeries = pendingSeries;
            int tempEpisode = pendingEpisode;
            bool tempPhone = pendingPhone;

            hasPendingChange = false;
            pendingSeries = null;
            pendingEpisode = -1;

            ChangeSelectedSeries(tempSeries, tempEpisode, tempPhone);
        }
    }

    static void OnVideoPrepared(VideoPlayer source)
    {
        videoPlayer.prepareCompleted -= OnVideoPrepared;
    }

    private static bool shouldPlayAfterPrepare = false;
    private static bool forceStartFromBeginning = false;

    static void OnVideoEnded(VideoPlayer source)
    {
        Debug.Log($"Video ended. Current episode: {selectedSeries} ep{selectedEpisode}");

        videoPlayer.loopPointReached -= OnVideoEnded;

        bool isPhoneMode = UIManager.CurrentViewingExperience == UIManager.ViewingExperience.Phone;

        int nextEpisode = selectedEpisode + 1;
        string nextFileName = $"ep{nextEpisode}{(isPhoneMode ? "_phone" : "")}.mp4";
        string nextVideoPath;

        if (pcLinkMode)
        {
            nextVideoPath = Path.Combine("Assets", nextFileName);
        }
        else
        {
            nextVideoPath = Path.Combine(Application.persistentDataPath, "Content", selectedSeries, "mp4", nextFileName);
        }

        if (File.Exists(nextVideoPath))
        {
            Debug.Log($"Switching to next episode: {selectedSeries} ep{nextEpisode}");

            lastRequestedTime = 0;
            forceStartFromBeginning = true;

            shouldPlayAfterPrepare = true;

            ChangeSelectedSeries(selectedSeries, nextEpisode, isPhoneMode);
        }
        else
        {
            Debug.LogWarning($"Next episode not found at {nextVideoPath}. Restarting current episode.");

            videoPlayer.loopPointReached += OnVideoEnded;

            videoPlayer.time = 0;
            videoPlayer.Play();
            videoPaused = false;
        }
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