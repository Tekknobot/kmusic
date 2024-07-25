using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Mixer : MonoBehaviour
{
    public Slider[] sliders = new Slider[8]; // Array to hold 8 sliders
    public AudioMixer mixer;

    void Start()
    {
        // Check if all sliders are assigned
        if (sliders.Length != 8)
        {
            Debug.LogError("Not all sliders are assigned. Ensure there are 8 sliders assigned in the inspector.");
        }

        // Initialize or configure sliders here
        foreach (Slider slider in sliders)
        {
            if (slider != null)
            {
                slider.onValueChanged.AddListener(delegate { OnSliderValueChanged(slider); });
            }
            else
            {
                Debug.LogError("Slider is not assigned in the array.");
            }
        }
    }

    void OnSliderValueChanged(Slider changedSlider)
    {
        Debug.Log("Slider value changed: " + changedSlider.value);

        // Assuming the slider's name matches the exposed parameter name in the AudioMixer
        string parameterName = changedSlider.name;

        // Directly use the slider value as dB value
        float dBValue = changedSlider.value;

        if (mixer != null)
        {
            bool result = mixer.SetFloat(parameterName, dBValue);
            if (!result)
            {
                Debug.LogError($"Failed to set AudioMixer parameter '{parameterName}'. Ensure the parameter is exposed and the name matches.");
            }
        }
        else
        {
            Debug.LogError("AudioMixer is not assigned.");
        }
    }
}
