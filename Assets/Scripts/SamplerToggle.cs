using UnityEngine;
using UnityEngine.UI;

public class SamplerToggle : MonoBehaviour
{
    public GameObject mixerGroupObject; // Reference to the MixerGroup GameObject
    public GameObject keyManagerObject; // Reference to the KeyManager GameObject
    public GameObject sampleManagerObject; // Reference to the SampleManager GameObject
    public Toggle toggle; // Reference to the Toggle UI element

    public Toggle musicPlayerToggle;

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
            // Toggle is on: Show SampleManager and hide MixerGroup and KeyManager
            if (sampleManagerObject != null)
            {
                sampleManagerObject.SetActive(true);
            }
            else
            {
                Debug.LogError("SampleManagerObject is not assigned in SamplerToggle.");
            }

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
            // Toggle is off: Show MixerGroup and KeyManager, hide SampleManager
            if (sampleManagerObject != null)
            {
                sampleManagerObject.SetActive(false);
            }
            else
            {
                Debug.LogError("SampleManagerObject is not assigned in SamplerToggle.");
            }

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
}
