using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AndroidAudioVisualizer : MonoBehaviour
{
    public int spectrumSize = 64; // Number of spectrum samples
    public float heightMultiplier = 10f; // Scale for visualizer height
    public float reactionSpeed = 10f; // Speed of reaction (higher = faster)
    public AnimationCurve frequencyCurve; // Custom scaling curve
    public int curveResolution = 10; // Number of points per segment for smooth curves

    private LineRenderer lineRenderer;
    private Vector3[] positions;
    private float[] spectrumData;
    private float[] smoothedSpectrumData; // Holds smoothed values

    void Start()
    {
        #if UNITY_ANDROID
        // Initialize LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = spectrumSize * curveResolution; // Adjust for curve resolution

        positions = new Vector3[spectrumSize];
        spectrumData = new float[spectrumSize];
        smoothedSpectrumData = new float[spectrumSize]; // Initialize smoothing array
        #else
        Debug.Log("Audio Visualizer is disabled because it's not running on Android.");
        enabled = false; // Disable the script on non-Android platforms
        #endif
    }

    void Update()
    {
        #if UNITY_ANDROID
        UpdateSpectrumData();
        SmoothSpectrumData();
        UpdateVisualizer();
        #endif
    }

    private void UpdateSpectrumData()
    {
        // Get spectrum data from the AudioListener (captures all audio in the scene)
        AudioListener.GetSpectrumData(spectrumData, 0, FFTWindow.Blackman);
    }

    private void SmoothSpectrumData()
    {
        // Smooth the spectrum data to control reaction speed
        for (int i = 0; i < spectrumSize; i++)
        {
            smoothedSpectrumData[i] = Mathf.Lerp(smoothedSpectrumData[i], spectrumData[i], Time.deltaTime * reactionSpeed);
        }
    }

    private void UpdateVisualizer()
    {
        // Get screen dimensions in world space
        Camera mainCamera = Camera.main;
        float screenWidth = mainCamera.aspect * mainCamera.orthographicSize * 2f; // Full width in world space
        float screenHeight = mainCamera.orthographicSize * 2f; // Full height in world space

        float screenLeft = mainCamera.transform.position.x - screenWidth / 2f; // Left edge
        float screenRight = mainCamera.transform.position.x + screenWidth / 2f; // Right edge
        float screenCenterY = mainCamera.transform.position.y; // Vertical center in world space

        // Update initial positions to span the full screen width and remain centered
        for (int i = 0; i < spectrumSize; i++)
        {
            // Evenly distribute x positions across the full width of the screen
            float x = Mathf.Lerp(screenLeft, screenRight, (float)i / (spectrumSize - 1));

            // Use smoothed spectrum data for visualizer
            float y = screenCenterY + smoothedSpectrumData[i] * frequencyCurve.Evaluate((float)i / spectrumSize) * heightMultiplier;

            positions[i] = new Vector3(x, y, 0);
        }

        // Generate curved points using Catmull-Rom interpolation
        Vector3[] curvedPoints = GenerateCurvedLine(positions);
        lineRenderer.positionCount = curvedPoints.Length;
        lineRenderer.SetPositions(curvedPoints);
    }

    private Vector3[] GenerateCurvedLine(Vector3[] controlPoints)
    {
        // Calculate the number of points for the curve
        int curvedLength = (controlPoints.Length - 1) * curveResolution + 1;
        Vector3[] curvedPoints = new Vector3[curvedLength];

        int index = 0;

        for (int i = 0; i < controlPoints.Length - 1; i++)
        {
            Vector3 p0 = i > 0 ? controlPoints[i - 1] : controlPoints[i];
            Vector3 p1 = controlPoints[i];
            Vector3 p2 = controlPoints[i + 1];
            Vector3 p3 = i < controlPoints.Length - 2 ? controlPoints[i + 2] : controlPoints[i + 1];

            // Generate points between p1 and p2
            for (int j = 0; j < curveResolution; j++)
            {
                float t = j / (float)curveResolution;
                curvedPoints[index++] = CatmullRom(t, p0, p1, p2, p3);
            }
        }

        // Add the last point
        curvedPoints[index] = controlPoints[controlPoints.Length - 1];

        return curvedPoints;
    }

    private Vector3 CatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Catmull-Rom spline formula
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }
}
