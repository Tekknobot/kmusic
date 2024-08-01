using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioHelm;

public class Pause : MonoBehaviour
{
    // Reference to the Audio Helm Clock
    public AudioHelmClock helmClock;

    // Reference to the Toggle UI component
    public Toggle pauseToggle;

    // Start is called before the first frame update
    void Start()
    {
        if (helmClock != null && pauseToggle != null)
        {
            // Set the clock to pause (playing) on start
            helmClock.pause = true;

            // Initialize the Toggle's state based on the clock's paused state
            pauseToggle.isOn = false;

            // Add listener for toggle value changes
            pauseToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
        else
        {
            Debug.LogWarning("helmClock or pauseToggle is not assigned in the inspector.");
        }
    }

    // Called when the Toggle value changes
    private void OnToggleValueChanged(bool isOn)
    {
        if (helmClock != null)
        {
            // Reverse the logic for setting pause state
            helmClock.pause = !isOn;

            // Reset the clock if the toggle is turned on
            if (isOn)
            {
                // Make sure the HelmPatternCreator exists and is properly assigned
                var patternCreator = GameObject.Find("HelmPatternCreator")?.GetComponent<HelmPatternCreator>(); 
                if (patternCreator.targetSequencers.Count > 0) {
                    GameObject.Find("HelmSequencer").GetComponent<AudioSource>().volume = 0;
                }               
                patternCreator.StartPlayingPatterns();
            }
            else 
            {
                // Make sure the HelmPatternCreator exists and is properly assigned
                var patternCreator = GameObject.Find("HelmPatternCreator")?.GetComponent<HelmPatternCreator>();
                if (patternCreator != null)
                {
                    patternCreator.StopCreatedPatterns();
                }
                else
                {
                    Debug.LogWarning("HelmPatternCreator not found or missing HelmPatternCreator component.");
                }
            }
        }
        else
        {
            Debug.LogWarning("helmClock is not assigned.");
        }
    }
}
