using UnityEngine;
using UnityEngine.Video;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class FrameProcessing : MonoBehaviour
{
    public VideoPlayer povInput;
    public VideoPlayer overlayInput;
    private long lastFrame;

    void Start()
    {
        if (povInput == null)
        {
            povInput = GetComponent<VideoPlayer>();
        }
    }

    private void Update()
    {
        if (povInput.isPlaying) 
        {
            long currentFrame = povInput.frame;
            if(currentFrame != lastFrame && currentFrame >= 0)
            {
                lastFrame = currentFrame;
                Debug.Log($"Frame {currentFrame}");
            }
        }
    }
}
