using UnityEngine;
using UnityEngine.UI;

public class MusicPlayerToggle : MonoBehaviour
{
    public GameObject musicPlayerObject; // Reference to the MusicPlayer GameObject
    public GameObject keyManagerObject; // Reference to the KeyManager GameObject
    public GameObject sampleManagerObject; // Reference to the SampleManager GameObject
    public GameObject mixerGroupObject; // Reference to the MixerGroup GameObject
    public Toggle toggle; // Reference to the Toggle UI element

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
            Debug.LogError("Toggle is not assigned in MusicPlayerToggle.");
        }
    }

    private void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            // Toggle is on: Show MusicPlayer, hide KeyManager, SampleManager, and MixerGroup
            if (musicPlayerObject != null)
            {
                musicPlayerObject.SetActive(true);
            }
            else
            {
                Debug.LogError("MusicPlayerObject is not assigned in MusicPlayerToggle.");
            }

            if (keyManagerObject != null)
            {
                keyManagerObject.SetActive(false);
            }
            else
            {
                Debug.LogError("KeyManagerObject is not assigned in MusicPlayerToggle.");
            }

            if (sampleManagerObject != null)
            {
                sampleManagerObject.SetActive(false);
            }
            else
            {
                Debug.LogError("SampleManagerObject is not assigned in MusicPlayerToggle.");
            }

            if (mixerGroupObject != null)
            {
                mixerGroupObject.SetActive(false);
            }
            else
            {
                Debug.LogError("MixerGroupObject is not assigned in MusicPlayerToggle.");
            }
        }
        else
        {
            // Toggle is off: Hide MusicPlayer, show KeyManager, SampleManager, and MixerGroup
            if (musicPlayerObject != null)
            {
                musicPlayerObject.SetActive(false);
            }
            else
            {
                Debug.LogError("MusicPlayerObject is not assigned in MusicPlayerToggle.");
            }

            if (keyManagerObject != null)
            {
                keyManagerObject.SetActive(true);
            }
            else
            {
                Debug.LogError("KeyManagerObject is not assigned in MusicPlayerToggle.");
            }

            if (sampleManagerObject != null)
            {
                sampleManagerObject.SetActive(true);
            }
            else
            {
                Debug.LogError("SampleManagerObject is not assigned in MusicPlayerToggle.");
            }

            if (mixerGroupObject != null)
            {
                mixerGroupObject.SetActive(true);
            }
            else
            {
                Debug.LogError("MixerGroupObject is not assigned in MusicPlayerToggle.");
            }
        }
    }
}
