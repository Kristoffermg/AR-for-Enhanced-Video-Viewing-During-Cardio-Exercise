using Accord.Statistics.Distributions.Univariate;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using System;
using System.Collections.Generic;
using TMPro;
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

    Vector3 standardScale = new Vector3(0.003f, 0.003f, 0.003f);
    Vector2 standardSize = new Vector2(192f, 108f);
    
    public Vector2 phoneSize = new Vector2(95f, 45f); 
    Vector3 phoneScale = new Vector3(0.0008f, 0.0007f, 0.0008f);

    Vector3 previousVideoPosition = Vector3.zero;

    public static int StudyParticipantNumber { get; private set; } = 1;

    public bool SettingsMenuEnabled { get; private set; } = true;

    [SerializeField] private GameObject rightHandRayInteractor;

    public enum ViewingExperience
    {
        Adaptive = 1,
        Phone = 2,
        Static = 3
    }

    public static ViewingExperience CurrentViewingExperience { get; private set; } = ViewingExperience.Adaptive;

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

    public void SeriesSelectionDropdown(Dropdown change)
    {
        string selectedSeries = change.value.ToString();
        VideoScript.ChangeSelectedSeries(selectedSeries);
    }

    public void EpisodeSelectionDropdownChanged(Dropdown change)
    {
        int selectedEpisode = Convert.ToInt16(change.value.ToString().Split(" ")[1]);
        VideoScript.SelectedEpisode = selectedEpisode;
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

        canvas.transform.LookAt(centerEye.transform);
        canvas.transform.Rotate(0, 180, 0);
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
                AdjustVideoFOV((float)IntensityManager.CurrentIntensity);
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
        return (int)CurrentViewingExperience;
    }

    public void ChangeIntensityLevel()
    {
        switch(IntensityManager.CurrentIntensity)
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

}