using UnityEngine;
using UnityEngine.UI;

public class MusicPlayerToggle : MonoBehaviour
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
            Debug.LogError("MusicPlayerObject is not assigned in MusicPlayerToggle.");
        }

        if (keyManagerObject != null)
        {
            initialPositionKeyManager = keyManagerObject.transform.position;
        }
        else
        {
            Debug.LogError("KeyManagerObject is not assigned in MusicPlayerToggle.");
        }

        if (sampleManagerObject != null)
        {
            initialPositionSampleManager = sampleManagerObject.transform.position;
        }
        else
        {
            Debug.LogError("SampleManagerObject is not assigned in MusicPlayerToggle.");
        }

        if (mixerGroupObject != null)
        {
            initialPositionMixerGroup = mixerGroupObject.transform.position;
        }
        else
        {
            Debug.LogError("MixerGroupObject is not assigned in MusicPlayerToggle.");
        }

        // Initialize the toggle and set up the listener
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggleChanged);
            // Set initial visibility based on toggle state
            OnToggleChanged(!toggle.isOn);
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
            // Toggle is on: Move objects to their initial positions
            MoveObjectsToInitialPositions();
        }
        else
        {
            // Toggle is off: Move objects offscreen
            MoveObjectsToOffscreenPositions();
        }
    }

    private void MoveObjectsToInitialPositions()
    {
        if (sampleManagerObject != null)
        {
            sampleManagerObject.transform.position = offscreenPosition;
        }

        if (mixerGroupObject != null)
        {
            mixerGroupObject.transform.position = offscreenPosition;
        }

        if (musicPlayerObject != null)
        {
            musicPlayerObject.transform.position = offscreenPosition;
        }

        if (keyManagerObject != null)
        {
            keyManagerObject.transform.position = offscreenPosition;
        }
    }

    private void MoveObjectsToOffscreenPositions()
    {
        if (sampleManagerObject != null)
        {
            sampleManagerObject.transform.position = offscreenPosition;
        }

        if (mixerGroupObject != null)
        {
            mixerGroupObject.transform.position = offscreenPosition;
        }

        if (sampleManagerObject != null)
        {
            sampleManagerObject.transform.position = offscreenPosition;
        }

        if (keyManagerObject != null)
        {
            keyManagerObject.transform.position = offscreenPosition;
        }
    }
}
