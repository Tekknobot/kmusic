using UnityEngine;
using System.Collections;

public class CameraAspectRatioAdjuster : MonoBehaviour
{
    private Camera _camera;
    private bool _hasAdjusted = false;

    void Start()
    {
        // Get the camera component attached to this GameObject
        _camera = GetComponent<Camera>();

        if (_camera == null)
        {
            Debug.LogError("No Camera component found on this GameObject.");
            return;
        }

        StartCoroutine(AdjustProjectionWithDelay()); // Delay adjustment to allow proper initialization
    }

    private IEnumerator AdjustProjectionWithDelay()
    {
        yield return new WaitForSeconds(0.1f); // Wait for 0.1 seconds to allow the screen to initialize
        AdjustProjection();
    }

    void Update()
    {
        if (_hasAdjusted || _camera == null)
        {
            return; // Exit if already adjusted or camera is not available
        }

        AdjustProjection(); // Adjust dynamically if not yet adjusted
    }

    private void AdjustProjection()
    {
        // Calculate the current aspect ratio
        float aspectRatio = Mathf.Round((float)Screen.width / Screen.height * 100f) / 100f; // Rounded to 2 decimal places

        // Check the aspect ratio and adjust the orthographic size accordingly
        if (Mathf.Abs(aspectRatio - 9f / 16f) < 0.01f)
        {
            Debug.Log("Aspect ratio is 9:16. Setting orthographic size to 9.");
            if (_camera.orthographic)
            {
                _camera.orthographicSize = 9.5f;
            }
        }
        else if (Mathf.Abs(aspectRatio - 9f / 19.5f) < 0.01f)
        {
            Debug.Log("Aspect ratio is 9:19. Setting orthographic size to 9.");
            if (_camera.orthographic)
            {
                _camera.orthographicSize = 9f;
            }
        }
        else if (Mathf.Abs(aspectRatio - 9f / 22f) < 0.01f)
        {
            Debug.Log("Aspect ratio is 9:22. Setting orthographic size to 10.");
            if (_camera.orthographic)
            {
                _camera.orthographicSize = 10f;
            }
        }
        else
        {
            Debug.Log("Aspect ratio does not match predefined values. No changes made.");
        }

        _hasAdjusted = true; // Ensure this runs only once
    }
}
