using Accord.Statistics.Distributions.Univariate;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIManager : MonoBehaviour
{
    public GameObject centerEye;
    public GameObject canvas;
    public GameObject settings;
    public GameObject video;

    public VideoPlayer videoPlayer;

    public TextMeshProUGUI participantNumberText;
    public TextMeshProUGUI subtitleText;
    public TextMeshProUGUI episodeText;

    public TMP_Dropdown seriesDropdown;
    public TMP_Dropdown episodeDropdown;

    //Vector3 standardScale = new Vector3(0.003f, 0.003f, 0.003f);
    Vector3 standardScale = new Vector3(0.003114f, 0.003326f, 0.003114f);
    Vector2 standardSize = new Vector2(192f, 108f);

    public Vector2 phoneSize = new Vector2(95f, 45f);
    Vector3 phoneScale = new Vector3(0.0008f, 0.0007f, 0.0008f);

    Vector3 previousVideoPosition = Vector3.zero;

    private float intensityScaleSpeed = 0.4f; // How quickly the video size should adjust after changing intensity level (only applicable to adaptive mode)
    private float adaptiveScaleSpeed = 5f;  // How quickly the video size should adjust during the adaptive mode 
    private float intensityAdjustmentDuration = 7f; // Duration to use intensityScaleSpeed (seconds)
    private float speedTransitionDuration = 3f; // Duration of the speed transition (seconds)

    private float intensityAdjustmentStartTime;
    private float currentScaleSpeed;
    private float speedTransitionStartTime;
    private bool transitioningSpeed = false;

    public bool currentlyAdjustingIntensityScale = false;

    public static int StudyParticipantNumber { get; private set; } = 1;

    public bool SettingsMenuEnabled { get; private set; } = true;

    public bool SubtitlesEnabled { get; private set; } = true;

    [SerializeField] private GameObject rightHandRayInteractor;
    [SerializeField] private GameObject rightHandAnchor;

    public static bool changedPlaybackTime = false;

    public enum ViewingExperience
    {
        Adaptive = 1,
        Phone = 3,
        Static = 2
    }

    public static ViewingExperience CurrentViewingExperience { get; private set; } = ViewingExperience.Adaptive;

    //private List<Subtitle> subtitles = new List<Subtitle>();
    private int currentSubtitleIndex = 0;
    private float nextSubtitleTime = 0f;
    private string subtitleFilePath;

    [SerializeField] private UnityEngine.UI.Toggle subtitleToggle;


    public void UIChange()
    {
        Vector3 centerEyePosition = centerEye.transform.position;
        Vector3 newVideoPosition = previousVideoPosition;
    }

    public void ToggleSettingsMenu()
    {
        if (SettingsMenuEnabled)
        {
            settings.SetActive(false);
            video.SetActive(true);
            rightHandRayInteractor.SetActive(false);
            rightHandAnchor.SetActive(false);

            if (!VideoScript.videoPlayer.isPrepared)
            {
                VideoScript.videoPlayer.Prepare();
                VideoScript.videoPlayer.prepareCompleted += OnVideoPrepared;
            }
            else
            {
                VideoScript.videoPlayer.Play();
            }
        }
        else
        {
            settings.SetActive(true);
            video.SetActive(false);
            rightHandRayInteractor.SetActive(true);
            rightHandAnchor.SetActive(true);

            if (VideoScript.videoPlayer.isPlaying)
            {
                VideoScript.videoPlayer.Pause();
            }

        }

        SettingsMenuEnabled = !SettingsMenuEnabled;
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        VideoScript.videoPlayer.prepareCompleted -= OnVideoPrepared;
        VideoScript.videoPlayer.Play();
    }

    public void IncrementButtonClick()
    {
        StudyParticipantNumber++;
        StudyParticipantChange();
    }

    public void DecrementButtonClick()
    {
        StudyParticipantNumber--;
        StudyParticipantChange();
    }

    private void StudyParticipantChange()
    {
        participantNumberText.text = $"Participant {StudyParticipantNumber}";
    }

    public void DecrementEpisodeButton()
    {
        VideoScript.DecrementEpisode();
    }

    public void IncrementEpisodeButton()
    {
        VideoScript.IncrementEpisode();
    }

    public void SeriesSelectionDropdown(int selectedIndex)
    {
        Debug.Log("SeriesSelectionDropdown called with: " + selectedIndex);
        string selectedSeries = seriesDropdown.options[selectedIndex].text;
        VideoScript.ChangeSelectedSeries(selectedSeries, CurrentViewingExperience == ViewingExperience.Phone);
    }

    public void EpisodeSelectionDropdownChanged(int selectedIndex)
    {
        Debug.Log("EpisodeSelectionDropdown called with: " + selectedIndex);
        int selectedEpisode = Convert.ToInt16(episodeDropdown.options[selectedIndex].text.Replace("Episode ", ""));
        VideoScript.SelectedEpisode = selectedEpisode;
        VideoScript.ChangeSelectedSeries(VideoScript.selectedSeries, selectedEpisode, CurrentViewingExperience == ViewingExperience.Phone);
        currentSubtitleIndex = 0;
        subtitleText.text = "";
    }

    public void SubtitleStatusChanged(UnityEngine.UI.Toggle toggle)
    {
        if (toggle != null) 
        {
            SubtitlesEnabled = toggle.isOn;
            subtitleText.enabled = SubtitlesEnabled;
        }
        else
        {
            Debug.LogError("SubtitleStatusChanged was called with a null toggle!");
        }

    }

    public void SetIntensityScaleAdjustment(bool adjusting)
    {
        currentlyAdjustingIntensityScale = adjusting;
        if (adjusting)
        {
            intensityAdjustmentStartTime = Time.time;
            currentScaleSpeed = intensityScaleSpeed; 
            transitioningSpeed = false;
        }
        else if (transitioningSpeed == false)
        {
            speedTransitionStartTime = Time.time;
            transitioningSpeed = true;
        }
    }

    public void AdjustAdaptiveVideoFOV()
    {
        if (CurrentViewingExperience != ViewingExperience.Adaptive)
        {
            Debug.LogError("AdjustAdaptiveVideoFOV called while not in adaptive mode");
            return;
        }

        if (currentlyAdjustingIntensityScale)
        {
            if (Time.time - intensityAdjustmentStartTime >= intensityAdjustmentDuration && !transitioningSpeed)
            {
                speedTransitionStartTime = Time.time;
                transitioningSpeed = true;
            }
        }

        if (transitioningSpeed)
        {
            float timeElapsed = Time.time - speedTransitionStartTime;
            if (timeElapsed < speedTransitionDuration)
            {
                float t = Mathf.Clamp01(timeElapsed / speedTransitionDuration);
                currentScaleSpeed = Mathf.Lerp(intensityScaleSpeed, adaptiveScaleSpeed, t);
            }
            else
            {
                currentScaleSpeed = adaptiveScaleSpeed;
                transitioningSpeed = false;
                currentlyAdjustingIntensityScale = false; 
            }
        }
        else if (!currentlyAdjustingIntensityScale)
        {
            currentScaleSpeed = adaptiveScaleSpeed; 
        }

        float currentDistance = Vector3.Distance(canvas.transform.position, centerEye.transform.position);
        float fovMaxScale = 1.575f;
        float fovMinScale = 0.625f;
        float minDistance = 0.3f;
        float maxDistance = 2.0f;
        float maxFOV = (float)IntensityManager.CurrentIntensity * fovMaxScale;
        float minFOV = (float)IntensityManager.CurrentIntensity * fovMinScale;

        float proximity = 1f - Mathf.InverseLerp(minDistance, maxDistance, currentDistance);
        float FOV = Mathf.Lerp(minFOV, maxFOV, proximity);

        AdjustVideoFOV(FOV, currentScaleSpeed);
    }

    public void AdjustVideoFOV(float FOV, float scalingSpeed)
    {
        float fovRadians = FOV * Mathf.Deg2Rad;
        float currentDistance = Vector3.Distance(canvas.transform.position, centerEye.transform.position);
        float desiredWidth = 2 * currentDistance * Mathf.Tan(fovRadians / 2);
        float baseScale = desiredWidth * 0.003f;
        Vector3 targetScale = new Vector3(baseScale, baseScale, 1);

        canvas.transform.localScale = Vector3.Lerp(canvas.transform.localScale, targetScale, Time.deltaTime * scalingSpeed);
    }



    public void MoveVideoPosition()
    {
        Vector3 leftControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        //leftControllerPosition.y += 0.2f;
        canvas.transform.position = Vector3.Lerp(canvas.transform.position, leftControllerPosition, 0.1f);
    }

    public int ChangeViewingExperience()
    {
        RectTransform canvasTransform = canvas.GetComponent<RectTransform>();
        switch (CurrentViewingExperience)
        {
            case ViewingExperience.Adaptive:
                CurrentViewingExperience = ViewingExperience.Static;
                canvas.transform.localScale = standardScale;
                Debug.Log("Viewing Experience set to Static");
                break;
            case ViewingExperience.Static:
                CurrentViewingExperience = ViewingExperience.Phone;
                canvas.transform.localScale = phoneScale;
                canvasTransform.sizeDelta = phoneSize;
                Debug.Log($"Viewing Experience set to Phone");
                break;
            case ViewingExperience.Phone:
                CurrentViewingExperience = ViewingExperience.Adaptive;
                canvas.transform.localScale = standardScale;
                canvasTransform.sizeDelta = standardSize;
                Debug.Log("Viewing Experience set to Adaptive");
                break;
        }
        LookAtCamera();
        return (int)CurrentViewingExperience;
    }

    public void LookAtCamera()
    {
        // Store the current anchor and pivot settings of the canvas RectTransform
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 originalAnchor = canvasRect.anchorMin;
        Vector2 originalPivot = canvasRect.pivot;

        // First just do a basic horizontal look-at
        canvas.transform.LookAt(new Vector3(centerEye.transform.position.x, canvas.transform.position.y, centerEye.transform.position.z));
        canvas.transform.Rotate(0, 180, 0); // Flip to face camera

        // Calculate a more precise direction for vertical adjustment
        // Use a ray from center eye to a point at the same height as the canvas
        Vector3 eyeForward = centerEye.transform.forward;
        Vector3 horizontalDirection = new Vector3(eyeForward.x, 0, eyeForward.z).normalized;

        // Get height difference between eye and canvas
        float heightDifference = centerEye.transform.position.y - canvas.transform.position.y;

        // Calculate angle based on height and pivot
        float verticalAngle = 0;

        // If canvas is below eye level, tilt up
        if (heightDifference > 0)
        {
            // The pivot being at bottom (0.5, 0) means we need to tilt differently
            verticalAngle = Mathf.Clamp(Mathf.Atan2(heightDifference, 1.5f) * Mathf.Rad2Deg, 0, 60);
        }
        // If canvas is above eye level, tilt down
        else if (heightDifference < 0)
        {
            verticalAngle = Mathf.Clamp(Mathf.Atan2(heightDifference, 1.5f) * Mathf.Rad2Deg, -60, 0);
        }

        // Apply the vertical rotation separately, keeping the y rotation we already set
        canvas.transform.localEulerAngles = new Vector3(verticalAngle, canvas.transform.localEulerAngles.y, 0f);

        Debug.Log($"Canvas pointing at center eye. Height difference: {heightDifference}, Vertical angle: {verticalAngle}");
    }

    private void Start()
    {
        episodeDropdown.onValueChanged.AddListener(EpisodeSelectionDropdownChanged);
        seriesDropdown.onValueChanged.AddListener(SeriesSelectionDropdown);
    }
}

