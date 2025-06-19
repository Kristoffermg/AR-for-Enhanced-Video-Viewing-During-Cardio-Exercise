using UnityEngine;

public class IntensityManager : MonoBehaviour
{
    public GameObject intensitySphere;
    public GameObject canvas;
    private InferenceManager inferenceManager;
    [SerializeField] private UIManager uiManager;

    public static IntensityManager Instance { get; private set; }

    public static IntensityLevel CurrentIntensity { get; private set; } = IntensityLevel.Low;

    private int currentFrame = 0;
    private static readonly int historicalDataSizeThreshold = 300; // the size of the sliding window
    private readonly int scale = historicalDataSizeThreshold / 100;
    private readonly float canvasDownscaleAmount = 0.0004f; // how much the size of the canvas should be changed based on intensity (higher number = greater size reduction)
    private readonly float minimumCanvasSize = 0.00005f;
    public readonly uint intensityUpdateRate = 200;
    private Vector3 baseCanvasScale;
    private Vector3 velocity = Vector3.zero;

    public enum IntensityLevel
    {
        Low = 60,
        Medium = 45,
        High = 30
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // prevent duplicate
        }
    }

    private void Start()
    {
        inferenceManager = new InferenceManager();
        uiManager = new UIManager();
    }

    private void Update()
    {
        currentFrame++;
    }

    public void ChangeIntensityLevel()
    {
        switch (CurrentIntensity)
        {
            case IntensityLevel.Low:
                SetIntensity(IntensityLevel.Medium);
                Debug.Log("Intensity Level set to Medium");
                break;
            case IntensityLevel.Medium:
                SetIntensity(IntensityLevel.High);
                Debug.Log("Intensity Level set to High");
                break;
            case IntensityLevel.High:
                SetIntensity(IntensityLevel.Low);
                Debug.Log("Intensity Level set to Low");
                break;
        }
    }

    public void SetIntensity(IntensityLevel newIntensity)
    {
        CurrentIntensity = newIntensity;
    }
}