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
            // Set the clock to unpause (playing) on start
            helmClock.pause = false;

            // Initialize the Toggle's state based on the clock's paused state
            pauseToggle.isOn = helmClock.pause;

            // Add listener for toggle value changes
            pauseToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        pauseToggle.isOn = true;

        helmClock.Reset();
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
                helmClock.Reset();
            }
        }
    }
}
