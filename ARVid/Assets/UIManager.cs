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

    public static int StudyParticipantNumber { get; private set; } = 1;

    public bool SettingsMenuEnabled { get; private set; } = true;

    public bool SubtitlesEnabled { get; private set; } = true;

    [SerializeField] private GameObject rightHandRayInteractor;

    public static bool changedPlaybackTime = false;

    public enum ViewingExperience
    {
        Adaptive = 1,
        Phone = 2,
        Static = 3
    }

    public static ViewingExperience CurrentViewingExperience { get; private set; } = ViewingExperience.Adaptive;

    private List<Subtitle> subtitles = new List<Subtitle>();
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
        LoadSubtitles(); 
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
        VideoScript.ChangeSelectedSeries(selectedSeries);
        LoadSubtitles(); 
    }

    public void EpisodeSelectionDropdownChanged(int selectedIndex)
    {
        Debug.Log("EpisodeSelectionDropdown called with: " + selectedIndex);
        int selectedEpisode = Convert.ToInt16(episodeDropdown.options[selectedIndex].text.Replace("Episode ", ""));
        VideoScript.SelectedEpisode = selectedEpisode;
        VideoScript.ChangeSelectedSeries(VideoScript.selectedVideo, selectedEpisode);
        LoadSubtitles(); 
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

    public void AdjustAdaptiveVideoFOV()
    {
        if (CurrentViewingExperience == ViewingExperience.Adaptive)
        {
            float currentDistance = Vector3.Distance(canvas.transform.position, centerEye.transform.position);

            float fovMaxScale = 1.575f;
            float fovMinScale = 0.625f;

            float minDistance = 0.3f;
            float maxDistance = 2.0f;
            float maxFOV = (float)IntensityManager.CurrentIntensity * fovMaxScale;
            float minFOV = (float)IntensityManager.CurrentIntensity * fovMinScale;

            float proximity = 1f - Mathf.InverseLerp(minDistance, maxDistance, currentDistance);
            float dynamicFOV = Mathf.Lerp(minFOV, maxFOV, proximity);
            float fovRadians = dynamicFOV * Mathf.Deg2Rad;

            AdjustVideoFOV(fovRadians);
        }
    }

    public void AdjustVideoFOV(float FOV)
    {
        float currentDistance = Vector3.Distance(canvas.transform.position, centerEye.transform.position);

        float desiredWidth = 2 * currentDistance * Mathf.Tan(FOV / 2);

        float baseScale = desiredWidth * 0.003f;

        Vector3 targetScale = new Vector3(baseScale, baseScale, 1);
        canvas.transform.localScale = Vector3.Lerp(canvas.transform.localScale, targetScale, Time.deltaTime * 5f);
    }

    public void MoveVideoPosition()
    {
        Vector3 leftControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        leftControllerPosition.y += 0.2f;
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
                //AdjustVideoFOV((float)IntensityManager.CurrentIntensity);
                Debug.Log("Viewing Experience set to Static");
                break;
            case ViewingExperience.Static:
                CurrentViewingExperience = ViewingExperience.Phone;
                canvas.transform.localScale = phoneScale;
                canvasTransform.sizeDelta = phoneSize;
                Debug.Log($"Viewing Experience set to Phone: {phoneSize.x}, {phoneSize.y}");
                break;
            case ViewingExperience.Phone:
                CurrentViewingExperience = ViewingExperience.Adaptive;
                canvas.transform.localScale = standardScale;
                canvasTransform.sizeDelta = standardSize;
                Debug.Log("Viewing Experience set to Adaptive");
                break;
        }
        canvas.transform.LookAt(centerEye.transform);
        canvas.transform.Rotate(0, 180, 0);
        return (int)CurrentViewingExperience;
    }

    public void ChangeIntensityLevel()
    {
        switch (IntensityManager.CurrentIntensity)
        {
            case IntensityManager.IntensityLevel.Low:
                IntensityManager.Instance.SetIntensity(IntensityManager.IntensityLevel.Medium);
                Debug.Log("Intensity Level set to Medium");
                break;
            case IntensityManager.IntensityLevel.Medium:
                IntensityManager.Instance.SetIntensity(IntensityManager.IntensityLevel.High);
                Debug.Log("Intensity Level set to High");
                break;
            case IntensityManager.IntensityLevel.High:
                IntensityManager.Instance.SetIntensity(IntensityManager.IntensityLevel.Low);
                Debug.Log("Intensity Level set to Low");
                break;
        }
    }

    private void Start()
    {
        Debug.Log($"Series Dropdown: {seriesDropdown}");
        Debug.Log($"Episode Dropdown: {episodeDropdown}");
        episodeDropdown.onValueChanged.AddListener(EpisodeSelectionDropdownChanged);
        seriesDropdown.onValueChanged.AddListener(SeriesSelectionDropdown);

        LoadSubtitles();
    }

    void Update()
    {
        if (SubtitlesEnabled && subtitles != null && VideoScript.videoPlayer.isPlaying)
        {
            float currentTime = (float)VideoScript.videoPlayer.time;

            // if the videplayer time was either skipped forward or backward, adjust subtitle index
            if (changedPlaybackTime)
            {
                if (subtitles[currentSubtitleIndex].startTime < currentTime)
                {
                    for (int i = currentSubtitleIndex; i < subtitles.Count-1; i++)
                    {
                        if (subtitles[i].startTime < currentTime && subtitles[i + 1].startTime > currentTime)
                        {
                            currentSubtitleIndex = i + 1;
                        }
                    }
                }
                else
                {
                    for (int i = currentSubtitleIndex; i > 0; i--)
                    {
                        if (subtitles[i].startTime > currentTime && subtitles[i - 1].startTime < currentTime)
                        {
                            currentSubtitleIndex = i - 1;
                        }
                    }
                }
            }

            //if (changedPlaybackTime)
            //{
            //    int left = 0;
            //    int right = subtitles.Count - 1;
            //    int newIndex = 0;

            //    while (left <= right)
            //    {
            //        int mid = (left + right) / 2;
            //        if (subtitles[mid].startTime == currentTime)
            //        {
            //            newIndex = mid;
            //            break;
            //        }
            //        else if (subtitles[mid].startTime < currentTime)
            //        {
            //            newIndex = mid; 
            //            left = mid + 1;
            //        }
            //        else
            //        {
            //            right = mid - 1;
            //        }
            //    }

            //    currentSubtitleIndex = newIndex;
            //    changedPlaybackTime = false;
            //}

            if (currentSubtitleIndex < subtitles.Count && currentTime >= subtitles[currentSubtitleIndex].startTime)
            {
                subtitleText.text = subtitles[currentSubtitleIndex].text;
                nextSubtitleTime = subtitles[currentSubtitleIndex].endTime;
                currentSubtitleIndex++;
            }
            else if (currentTime >= nextSubtitleTime)
            {
                subtitleText.text = "";
            }
        }
    }

    void LoadSubtitles()
    {
        subtitles.Clear();
        currentSubtitleIndex = 0;
        subtitleText.text = "";

        if(VideoScript.pcLinkMode)
        {
            subtitleFilePath = "Assets/ep1.srt";
        }
        else
        {
            subtitleFilePath = $"/sdcard/Android/data/com.UnityTechnologies.com.unity.template.urpblank/files/Content/{VideoScript.selectedVideo}/srt/ep{VideoScript.SelectedEpisode}.srt";
        }


        if (string.IsNullOrEmpty(subtitleFilePath) || !File.Exists(subtitleFilePath))
        {
            Debug.LogWarning("Subtitle file not found: " + subtitleFilePath);
            return;
        }

        string[] lines = File.ReadAllLines(subtitleFilePath);
        Subtitle currentSubtitle = null;
        Regex timeCodeRegex = new Regex(@"(\d{2}):(\d{2}):(\d{2}),(\d{3}) --> (\d{2}):(\d{2}):(\d{2}),(\d{3})");

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (currentSubtitle != null)
                {
                    subtitles.Add(currentSubtitle);
                    currentSubtitle = null;
                }
                continue;
            }

            if (int.TryParse(line, out int subtitleNumber))
            {
                currentSubtitle = new Subtitle();
            }
            else if (timeCodeRegex.IsMatch(line))
            {
                Match match = timeCodeRegex.Match(line);
                if (currentSubtitle != null)
                {
                    currentSubtitle.startTime = ParseTimeCode(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value);
                    currentSubtitle.endTime = ParseTimeCode(match.Groups[5].Value, match.Groups[6].Value, match.Groups[7].Value, match.Groups[8].Value);
                }
            }
            else if (currentSubtitle != null)
            {
                currentSubtitle.text += line + "\n";
            }
        }

        if (currentSubtitle != null)
        {
            subtitles.Add(currentSubtitle);
        }

        Debug.Log($"Loaded {subtitles.Count} subtitles from {subtitleFilePath}");
    }

    float ParseTimeCode(string hours, string minutes, string seconds, string milliseconds)
    {
        float h = float.Parse(hours);
        float m = float.Parse(minutes);
        float s = float.Parse(seconds);
        float ms = float.Parse(milliseconds) / 1000f;
        return (h * 3600f) + (m * 60f) + s + ms;
    }

    private class Subtitle
    {
        public float startTime;
        public float endTime;
        public string text = "";
    }
}

