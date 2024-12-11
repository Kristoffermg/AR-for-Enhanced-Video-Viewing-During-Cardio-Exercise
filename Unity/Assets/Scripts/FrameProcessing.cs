using Unity.Barracuda;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;
using NN;
using UnityEngine.UI;
using System.Collections.Generic;

public class FrameProcessing : MonoBehaviour
{
    [Tooltip("File of the YOLO model")]
    public NNModel tinyYoloV2;

    [Tooltip("RawImage component which will be used to draw resuls")]
    public RawImage ImageUI;

    [Tooltip("Camera feed input")]
    public VideoPlayer povInput;
    [Tooltip("Video feed input")]
    public VideoPlayer overlayInput;

    [Range(0.0f, 1f)]
    [Tooltip("The minimum value of box confidence below which boxes won't be drawn")]
    public float MinBoxConfidence = 0.3f;

    NNHandler nn;
    YOLOHandler yolo;
    private long lastFrame;

    string[] classesNames;
    OnGUICanvasRelativeDrawer relativeDrawer;

    Color[] colorArray = new Color[] { Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow };
    
    Texture2D videoFrame;


    void Start()
    {
        videoFrame = new Texture2D(416, 416);

        nn = new NNHandler(tinyYoloV2);
        yolo = new YOLOHandler(nn); // https://github.com/wojciechp6/YOLOv8Unity/blob/master/YOLOv8Unity

        var firstInput = nn.model.inputs[0];
        int height = firstInput.shape[5];
        int width = firstInput.shape[6];


        if (povInput == null)
        {
            povInput = GetComponent<VideoPlayer>();
        }

        YOLOv2Postprocessor.DiscardThreshold = MinBoxConfidence;
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

                RenderTexture renderTexture = povInput.texture as RenderTexture;
                RenderTexture.active = renderTexture;
                videoFrame.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                videoFrame.Apply();
                RenderTexture.active = null;

                videoFrame = Preprocess(videoFrame);

                var results = yolo.Run(videoFrame);

                DrawResults(results, videoFrame);
                ImageUI.texture = videoFrame;
            }
        }
    }

    private Texture2D Preprocess(Texture2D texture)
    {
        Texture2D resizedTexture = ResizeTexture(texture, 416, 416);

        Color[] pixels = resizedTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i].r /= 255f;
            pixels[i].g /= 255f;
            pixels[i].b /= 255f;
            pixels[i].a = 1.0f;
        }

        resizedTexture.SetPixels(pixels);
        resizedTexture.Apply();

        return resizedTexture;
    }


    private Texture2D ResizeTexture(Texture2D texture, int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture, rt);
        Texture2D resizedTexture = new Texture2D(width, height);
        resizedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        resizedTexture.Apply();
        RenderTexture.active = null;
        return resizedTexture;
    }

    private void DrawResults(IEnumerable<ResultBox> results, Texture2D texture)
    {
        relativeDrawer.Clear();
        foreach (ResultBox box in results)
            DrawBox(box, texture);
    }

    private void DrawBox(ResultBox box, Texture2D img)
    {
        if (box.score < MinBoxConfidence)
            return;

        Color boxColor = colorArray[box.bestClassIndex % colorArray.Length];
        int boxWidth = (int)(box.score / MinBoxConfidence);
        TextureTools.DrawRectOutline(img, box.rect, boxColor, boxWidth, rectIsNormalized: false, revertY: true);

        Vector2 textureSize = new(img.width, img.height);
        relativeDrawer.DrawLabel(classesNames[box.bestClassIndex], box.rect.position / textureSize);
    }

}
