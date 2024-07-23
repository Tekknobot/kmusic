using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioHelm;

public class Length : MonoBehaviour
{
    // Reference to the Slider UI component
    public Slider lengthSlider;

    // Reference to the Sample Sequencer
    public AudioHelm.SampleSequencer sampleSequencer;

    // Start is called before the first frame update
    void Start()
    {
        if (lengthSlider != null && sampleSequencer != null)
        {
            // Add listener for slider value changes
            lengthSlider.onValueChanged.AddListener(OnSliderValueChanged);

            // Set the initial length based on the slider's initial value
            OnSliderValueChanged(lengthSlider.value);
        }
        else
        {
            Debug.LogError("Slider or Sample Sequencer is not assigned.");
        }
    }

    // Called when the Slider value changes
    private void OnSliderValueChanged(float value)
    {
        if (sampleSequencer != null)
        {
            // Debug log to check the slider value
            Debug.Log("Slider Value: " + value);

            // Map slider value to sequencer length
            int length = 0;

            // Adjust the comparison to handle floating-point precision issues
            if (Mathf.Approximately(value, 0f))
            {
                length = 16;
            }
            else if (Mathf.Approximately(value, 1f))
            {
                length = 32;
            }
            else if (Mathf.Approximately(value, 2f))
            {
                length = 64;
            }
            else
            {
                Debug.LogWarning("Slider value out of expected range.");
            }

            // Set the sequencer length and log it
            sampleSequencer.length = length;
            Debug.Log("Sample Sequencer Length Set To: " + length);
        }
        else
        {
            Debug.LogError("Sample Sequencer is not assigned.");
        }
    }
}
