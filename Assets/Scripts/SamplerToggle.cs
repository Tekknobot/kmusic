using UnityEngine;
using UnityEngine.UI;

public class SamplerToggle : MonoBehaviour
{
    public GameObject mixerGroupObject; // Reference to the MixerGroup GameObject
    public GameObject keyManagerObject; // Reference to the KeyManager GameObject
    public GameObject sampleManagerObject; // Reference to the SampleManager GameObject
    public Toggle toggle; // Reference to the Toggle UI element

    public Toggle musicPlayerToggle;

    private Vector3 offscreenPosition = new Vector3(-1000, 0, 0); // Define an offscreen position
    private Vector3 onscreenPosition = new Vector3(6.810134f, -1.697827f, 1.511794f); // Define the onscreen position (default)

    private void Start()
    {
        // Initialize the toggle and set up the listener
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggleChanged);
            // Set initial visibility based on toggle state
            OnToggleChanged(toggle.isOn);
        }
        else
        {
            Debug.LogError("Toggle is not assigned in SamplerToggle.");
        }
    }

    private void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            // Toggle is on: Show SampleManager offscreen and hide MixerGroup and KeyManager
            MoveSampleManagerToOnscreen();

            if (mixerGroupObject != null)
            {
                mixerGroupObject.SetActive(true);
            }
            else
            {
                Debug.LogError("MixerGroupObject is not assigned in SamplerToggle.");
            }

            if (keyManagerObject != null)
            {
                keyManagerObject.SetActive(false);
            }
            else
            {
                Debug.LogError("KeyManagerObject is not assigned in SamplerToggle.");
            }

            musicPlayerToggle.enabled = false;
        }
        else
        {
            // Toggle is off: Show SampleManager on screen and hide MixerGroup and KeyManager
            MoveSampleManagerToOffscreen();

            if (mixerGroupObject != null)
            {
                mixerGroupObject.SetActive(false);
            }
            else
            {
                Debug.LogError("MixerGroupObject is not assigned in SamplerToggle.");
            }

            if (keyManagerObject != null)
            {
                keyManagerObject.SetActive(true);
            }
            else
            {
                Debug.LogError("KeyManagerObject is not assigned in SamplerToggle.");
            }

            musicPlayerToggle.enabled = true;
        }
    }

    private void MoveSampleManagerToOffscreen()
    {
        if (sampleManagerObject != null)
        {
            sampleManagerObject.transform.position = offscreenPosition;
            sampleManagerObject.SetActive(true); // Make sure it's active so it can be moved
        }
        else
        {
            Debug.LogError("SampleManagerObject is not assigned in SamplerToggle.");
        }
    }

    private void MoveSampleManagerToOnscreen()
    {
        if (sampleManagerObject != null)
        {
            sampleManagerObject.transform.position = onscreenPosition;
            sampleManagerObject.SetActive(true); // Make sure it's active when moved back on screen
        }
        else
        {
            Debug.LogError("SampleManagerObject is not assigned in SamplerToggle.");
        }
    }
}
