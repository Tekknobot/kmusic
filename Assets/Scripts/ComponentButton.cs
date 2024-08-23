using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComponentButton : MonoBehaviour
{
    public GameObject musicPlayerObject; // Reference to the MusicPlayer GameObject
    public GameObject keyManagerObject; // Reference to the KeyManager GameObject
    public GameObject sampleManagerObject; // Reference to the SampleManager GameObject
    public GameObject mixerGroupObject; // Reference to the MixerGroup GameObject
    public Button button; // Reference to the UI Button

    private Vector3 initialPositionMusicPlayer; // Stores the initial position for MusicPlayer
    private Vector3 initialPositionKeyManager; // Stores the initial position for KeyManager
    private Vector3 initialPositionSampleManager; // Stores the initial position for SampleManager
    private Vector3 initialPositionMixerGroup; // Stores the initial position for MixerGroup

    private Vector3 offscreenPosition = new Vector3(-1000, 0, 0); // Define an offscreen position
    public int currentObjectIndex = 3; // Index to keep track of which object to move on-screen

    public int currentPatternGroup = 1;

    public GameObject TrimmerUI;

    // Start is called before the first frame update
    void Start()
    {
        // Store the initial positions of all GameObjects
        if (musicPlayerObject != null)
        {
            musicPlayerObject.SetActive(false);
            initialPositionMusicPlayer = musicPlayerObject.transform.position;
        }
        else
        {
            Debug.LogError("MusicPlayerObject is not assigned in ComponentButton.");
        }

        if (keyManagerObject != null)
        {
            initialPositionKeyManager = keyManagerObject.transform.position;
        }
        else
        {
            Debug.LogError("KeyManagerObject is not assigned in ComponentButton.");
        }

        if (sampleManagerObject != null)
        {
            initialPositionSampleManager = sampleManagerObject.transform.position;
        }
        else
        {
            Debug.LogError("SampleManagerObject is not assigned in ComponentButton.");
        }

        if (mixerGroupObject != null)
        {
            mixerGroupObject.SetActive(false);
            initialPositionMixerGroup = mixerGroupObject.transform.position;
        }
        else
        {
            Debug.LogError("MixerGroupObject is not assigned in ComponentButton.");
        }

        // Add listener to the button
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("Button is not assigned in ComponentButton.");
        }

        // Start the coroutine to wait for initialization
        StartCoroutine(WaitForInitialization());
    }

    // Coroutine to wait for initialization before calling OnButtonClick
    private IEnumerator WaitForInitialization()
    {
        yield return new WaitForEndOfFrame(); // Wait until the end of the frame
        // Optionally, you can add a longer wait time:
        // yield return new WaitForSeconds(0.1f); // Wait for 0.1 seconds

        OnButtonClick(); // Call OnButtonClick after initialization
    }

    // Method to be called when the button is clicked
    public void OnButtonClick()
    {
        MoveCurrentObjectOnScreen();
        MoveOtherObjectsOffscreen();
        currentObjectIndex = (currentObjectIndex + 1) % 4; // Cycle through the objects
    }

    private void MoveCurrentObjectOnScreen()
    {
        switch (currentObjectIndex)
        {
            case 0:
                if (musicPlayerObject != null)
                {
                    musicPlayerObject.SetActive(true);
                    musicPlayerObject.transform.position = initialPositionMusicPlayer;
                    currentPatternGroup = 0;
                    TrimmerUI.SetActive(false);
                }
                break;
            case 1:
                if (keyManagerObject != null)
                {
                    keyManagerObject.transform.position = initialPositionKeyManager;
                    currentPatternGroup = 1;
                    TrimmerUI.SetActive(false);
                }
                break;
            case 2:
                if (sampleManagerObject != null)
                {
                    sampleManagerObject.transform.position = initialPositionSampleManager;
                    currentPatternGroup = 2;
                    TrimmerUI.SetActive(true);
                    GameObject.Find("AudioVisualizer").GetComponent<AudioVisualizer>().StartRender();
                }
                break;
            case 3:
                if (mixerGroupObject != null)
                {
                    mixerGroupObject.SetActive(true);
                    mixerGroupObject.transform.position = initialPositionMixerGroup;
                    currentPatternGroup = 4;
                    TrimmerUI.SetActive(false);
                }
                break;
        }
    }

    private void MoveOtherObjectsOffscreen()
    {
        if (currentObjectIndex != 0 && musicPlayerObject != null)
        {
            musicPlayerObject.transform.position = offscreenPosition;
        }

        if (currentObjectIndex != 1 && keyManagerObject != null)
        {
            keyManagerObject.transform.position = offscreenPosition;
        }

        if (currentObjectIndex != 2 && sampleManagerObject != null)
        {
            sampleManagerObject.transform.position = offscreenPosition;
        }

        if (currentObjectIndex != 3 && mixerGroupObject != null)
        {
            mixerGroupObject.transform.position = offscreenPosition;
        }
    }
}
