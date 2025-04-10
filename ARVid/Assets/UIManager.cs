using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject centerEye;
    public GameObject canvas;

    Vector3 standardScale = new Vector3(0.003f, 0.003f, 0.003f);
    Vector2 standardSize = new Vector2(192f, 108f);
    
    public Vector2 phoneSize = new Vector2(95f, 45f); 
    Vector3 phoneScale = new Vector3(0.0008f, 0.0007f, 0.0008f);

    Vector3 previousVideoPosition = Vector3.zero;

    public enum ViewingExperience
    {
        Adaptive = 1,
        Phone = 2,
        Static = 3
    }

    private ViewingExperience currentViewingExperience = ViewingExperience.Adaptive;

    public void UIChange()
    {
        Vector3 centerEyePosition = centerEye.transform.position;
        Vector3 newVideoPosition = previousVideoPosition;
    }

    public void AdjustAdaptiveVideoFOV()
    {
        if (currentViewingExperience == ViewingExperience.Adaptive)
        {
            float currentDistance = Vector3.Distance(canvas.transform.position, centerEye.transform.position);

            float fovMaxScale = 1.575f;
            float fovMinScale = 0.625f;

            float minDistance = 0.3f;
            float maxDistance = 2.0f;
            float maxFOV = (float)IntensityManager.Instance.CurrentIntensity * fovMaxScale;
            float minFOV = (float)IntensityManager.Instance.CurrentIntensity * fovMinScale; 

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
        switch (currentViewingExperience)
        {
            case ViewingExperience.Adaptive:
                currentViewingExperience = ViewingExperience.Static;
                AdjustVideoFOV((float)IntensityManager.Instance.CurrentIntensity);
                Debug.Log("Viewing Experience set to Static");
                break;
            case ViewingExperience.Static:
                currentViewingExperience = ViewingExperience.Phone;
                canvas.transform.localScale = phoneScale;
                canvasTransform.sizeDelta = phoneSize;
                Debug.Log($"Viewing Experience set to Phone: {phoneSize.x}, {phoneSize.y}");
                break;
            case ViewingExperience.Phone:
                currentViewingExperience = ViewingExperience.Adaptive;
                canvas.transform.localScale = standardScale;
                canvasTransform.sizeDelta = standardSize;
                Debug.Log("Viewing Experience set to Adaptive");
                break;
        }
        return (int)currentViewingExperience;
    }

    public void ChangeIntensityLevel()
    {
        switch(IntensityManager.Instance.CurrentIntensity)
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