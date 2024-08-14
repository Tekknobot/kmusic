using UnityEngine;

public class CameraSizeAdjuster : MonoBehaviour
{
    public Camera mainCamera;

    // Define desired height or width for the camera view
    public float targetHeight = 10f;
    public float targetWidth = 10f;

    void Start()
    {
        AdjustCameraSize();
    }

    void AdjustCameraSize()
    {
        // Calculate the size needed to fit the target dimensions
        float screenAspect = (float)Screen.width / Screen.height;
        float targetAspect = targetWidth / targetHeight;

        if (screenAspect >= targetAspect)
        {
            // Widescreen or equal aspect ratio: fit to width
            mainCamera.orthographicSize = targetWidth / 2f;
        }
        else
        {
            // Tall or equal aspect ratio: fit to height
            mainCamera.orthographicSize = targetHeight / (2f * screenAspect);
        }

        Debug.Log("Adjusted camera orthographic size based on screen dimensions.");
    }
}
