using UnityEngine;
using UnityEngine.UI;

public class SamplerToggle : MonoBehaviour
{
    public GameObject musicPlayerObject; // Reference to the MusicPlayer GameObject
    public GameObject keyManagerObject; // Reference to the KeyManager GameObject
    public GameObject sampleManagerObject; // Reference to the SampleManager GameObject
    public GameObject mixerGroupObject; // Reference to the MixerGroup GameObject
    public Toggle toggle; // Reference to the Toggle UI element

    private Vector3 initialPositionMusicPlayer; // Stores the initial position for MusicPlayer
    private Vector3 initialPositionKeyManager; // Stores the initial position for KeyManager
    private Vector3 initialPositionSampleManager; // Stores the initial position for SampleManager
    private Vector3 initialPositionMixerGroup; // Stores the initial position for MixerGroup

    private Vector3 offscreenPosition = new Vector3(-1000, 0, 0); // Define an offscreen position

    private void Start()
    {
        // Store the initial positions of all GameObjects
        if (musicPlayerObject != null)
        {
            initialPositionMusicPlayer = musicPlayerObject.transform.position;
        }
        else
        {
            Debug.LogError("MusicPlayerObject is not assigned in SamplerToggle.");
        }

        if (keyManagerObject != null)
        {
            initialPositionKeyManager = keyManagerObject.transform.position;
        }
        else
        {
            Debug.LogError("KeyManagerObject is not assigned in SamplerToggle.");
        }

        if (sampleManagerObject != null)
        {
            initialPositionSampleManager = sampleManagerObject.transform.position;
        }
        else
        {
            Debug.LogError("SampleManagerObject is not assigned in SamplerToggle.");
        }

        if (mixerGroupObject != null)
        {
            initialPositionMixerGroup = mixerGroupObject.transform.position;
        }
        else
        {
            Debug.LogError("MixerGroupObject is not assigned in SamplerToggle.");
        }

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
            // Toggle is on: Move only the KeyManager on-screen
            MoveKeyManagerOnScreen();
            MoveOthersOffscreen();
        }
        else
        {
            // Toggle is off: Move all objects back to their initial positions
            MoveObjectsToInitialPositions();
        }
    }

    private void MoveKeyManagerOnScreen()
    {
        if (keyManagerObject != null)
        {
            keyManagerObject.transform.position = initialPositionKeyManager;
        }
    }

    private void MoveOthersOffscreen()
    {
        if (musicPlayerObject != null)
        {
            musicPlayerObject.transform.position = offscreenPosition;
        }

        if (sampleManagerObject != null)
        {
            sampleManagerObject.transform.position = offscreenPosition;
        }

        if (mixerGroupObject != null)
        {
            mixerGroupObject.transform.position = offscreenPosition;
        }
    }

    private void MoveObjectsToInitialPositions()
    {
        if (musicPlayerObject != null)
        {
            musicPlayerObject.transform.position = initialPositionMusicPlayer;
        }

        if (keyManagerObject != null)
        {
            keyManagerObject.transform.position = initialPositionKeyManager;
        }

        if (sampleManagerObject != null)
        {
            sampleManagerObject.transform.position = initialPositionSampleManager;
        }

        if (mixerGroupObject != null)
        {
            mixerGroupObject.transform.position = initialPositionMixerGroup;
        }
    }
}
