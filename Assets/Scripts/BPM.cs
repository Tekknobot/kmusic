using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AudioHelm;

public class BPMController : MonoBehaviour
{
    // Reference to the Audio Helm Clock
    public AudioHelmClock helmClock;

    // Reference to the TextMeshPro object to display the BPM
    public TextMeshProUGUI bpmLabel;

    // Reference to the Slider UI component
    public Slider bpmSlider;

    // Start is called before the first frame update
    void Start()
    {
        if (helmClock != null && bpmSlider != null)
        {
            // Initialize the BPM value and set the slider's value
            bpmSlider.value = helmClock.bpm;
            UpdateBPMLabel();

            // Add listener for slider value changes
            bpmSlider.onValueChanged.AddListener(delegate { OnSliderValueChanged(); });
        }
    }

    // Called when the slider value changes
    public void OnSliderValueChanged()
    {
        if (helmClock != null && bpmSlider != null)
        {
            helmClock.bpm = bpmSlider.value;
            UpdateBPMLabel();
        }
    }

    // Update the BPM label text
    void UpdateBPMLabel()
    {
        if (bpmLabel != null)
        {
            bpmLabel.text = $"{helmClock.bpm}";
        }
    }
}
