using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject centerEye;
    public GameObject canvas;

    Vector3 standardScale = new Vector3(0.003f, 0.003f, 0.003f);
    Vector2 standardSize = new Vector2(192f, 108f);

    Vector3 phoneScale = new Vector3(0.001f, 0.001f, 0.001f);
    Vector2 phoneSize = new Vector2(148f, 72f);

    Vector3 previousVideoPosition = Vector3.zero;

    private enum ViewingExperience
    {
        Adaptive,
        Phone,
        Static
    }

    private ViewingExperience currentViewingExperience = ViewingExperience.Adaptive;

    public void UIChange()
    {
        Vector3 centerEyePosition = centerEye.transform.position;
        Vector3 newVideoPosition = previousVideoPosition;
    }

    public void AdjustCanvasFOV()
    {
        if(currentViewingExperience == ViewingExperience.Adaptive)
        {
            float currentDistance = Vector3.Distance(canvas.transform.position, centerEye.transform.position);
            float desiredWidth = 2 * currentDistance * Mathf.Tan((float)IntensityManager.Instance.CurrentIntensity * Mathf.Deg2Rad / 2);
            Vector3 targetScale =  new Vector3(desiredWidth * 0.003f, desiredWidth * 0.003f, 1);
            canvas.transform.localScale = Vector3.Lerp(canvas.transform.localScale, targetScale, Time.deltaTime * 5f);

            canvas.transform.LookAt(centerEye.transform);
            canvas.transform.Rotate(0, 180, 0);
        }
    }

    public void MoveVideoPosition()
    {
        Vector3 leftControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand);
        leftControllerPosition.y += 0.2f;
        canvas.transform.position = Vector3.Lerp(canvas.transform.position, leftControllerPosition, 0.1f);
    }

    public void ChangeViewingExperience()
    {
        RectTransform canvasTransform = canvas.GetComponent<RectTransform>();
        switch (currentViewingExperience)
        {
            case ViewingExperience.Adaptive:
                currentViewingExperience = ViewingExperience.Static;
                Debug.Log("Viewing Experience set to Static");
                break;
            case ViewingExperience.Static:
                currentViewingExperience = ViewingExperience.Phone;
                canvas.transform.localScale = phoneScale;
                canvasTransform.sizeDelta = phoneSize;
                Debug.Log("Viewing Experience set to Phone");
                break;
            case ViewingExperience.Phone:
                currentViewingExperience = ViewingExperience.Adaptive;
                canvas.transform.localScale = standardScale;
                canvasTransform.sizeDelta = standardSize;
                Debug.Log("Viewing Experience set to Adaptive");
                break;
        }
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