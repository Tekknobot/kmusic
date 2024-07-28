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

    // Key for storing BPM in PlayerPrefs
    private const string BPM_PREF_KEY = "BPM";

    // Start is called before the first frame update
    void Start()
    {
        if (helmClock != null && bpmSlider != null)
        {
            // Load the saved BPM value or default to helmClock's BPM if not found
            float savedBPM = PlayerPrefs.GetFloat(BPM_PREF_KEY, helmClock.bpm);

            // Initialize the BPM value and set the slider's value
            helmClock.bpm = savedBPM;
            bpmSlider.value = savedBPM;
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
            SaveBPM();
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

    // Save the BPM value to PlayerPrefs
    void SaveBPM()
    {
        PlayerPrefs.SetFloat(BPM_PREF_KEY, helmClock.bpm);
        PlayerPrefs.Save();
    }
}
