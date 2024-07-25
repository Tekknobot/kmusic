using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;

public class Mixer : MonoBehaviour
{
    public Slider[] sliders = new Slider[8]; // Array to hold 8 sliders
    public Toggle[] muteButtons = new Toggle[8]; // Array to hold 8 mute buttons
    public Toggle[] soloButtons = new Toggle[8]; // Array to hold 8 solo buttons
    public AudioMixer mixer;

    private string[] groupNames = new string[8]; // To store the names of the audio mixer groups
    private Dictionary<string, float> originalVolumes = new Dictionary<string, float>(); // To store original volumes
    private string soloGroupName = null; // The currently soloed group

    void Start()
    {
        // Check if all sliders, mute buttons, and solo buttons are assigned
        if (sliders.Length != 8 || muteButtons.Length != 8 || soloButtons.Length != 8)
        {
            Debug.LogError("Ensure all sliders, mute buttons, and solo buttons are assigned in the inspector.");
            return;
        }

        // Initialize or configure sliders here
        for (int i = 0; i < sliders.Length; i++)
        {
            Slider slider = sliders[i];
            if (slider != null)
            {
                slider.onValueChanged.AddListener(delegate { OnSliderValueChanged(slider); });
            }
            else
            {
                Debug.LogError($"Slider at index {i} is not assigned.");
            }
        }

        // Initialize mute and solo buttons
        for (int i = 0; i < muteButtons.Length; i++)
        {
            Toggle muteButton = muteButtons[i];
            if (muteButton != null)
            {
                int index = i; // Capture the index for use in the lambda
                muteButton.onValueChanged.AddListener(delegate { OnMuteButtonValueChanged(muteButton, index); });
            }
            else
            {
                Debug.LogError($"Mute button at index {i} is not assigned.");
            }

            Toggle soloButton = soloButtons[i];
            if (soloButton != null)
            {
                int index = i; // Capture the index for use in the lambda
                soloButton.onValueChanged.AddListener(delegate { OnSoloButtonValueChanged(soloButton, index); });
            }
            else
            {
                Debug.LogError($"Solo button at index {i} is not assigned.");
            }

            // Store the name of each group
            groupNames[i] = sliders[i].name; // Assuming slider names match group names
        }

        // Store original volumes of each group
        foreach (string groupName in groupNames)
        {
            float currentVolume;
            mixer.GetFloat(groupName, out currentVolume);
            originalVolumes[groupName] = currentVolume;
        }
    }

    void OnSliderValueChanged(Slider changedSlider)
    {
        Debug.Log("Slider value changed: " + changedSlider.value);

        // Assuming the slider's name matches the exposed parameter name in the AudioMixer
        string parameterName = changedSlider.name;

        // Use the slider value directly
        float sliderValue = changedSlider.value;

        if (mixer != null)
        {
            bool result = mixer.SetFloat(parameterName, sliderValue);
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

    void OnMuteButtonValueChanged(Toggle changedToggle, int index)
    {
        string groupName = groupNames[index];
        bool isMuted = changedToggle.isOn;

        if (mixer != null)
        {
            float value = isMuted ? 0f : originalVolumes[groupName]; // Mute to 0, unmute to original volume
            mixer.SetFloat(groupName, value);
        }
        else
        {
            Debug.LogError("AudioMixer is not assigned.");
        }
    }

    void OnSoloButtonValueChanged(Toggle changedToggle, int index)
    {
        string soloGroupName = groupNames[index];
        bool isSolo = changedToggle.isOn;

        if (mixer != null)
        {
            if (isSolo)
            {
                // Save the previously soloed group, if any
                if (this.soloGroupName != null)
                {
                    // Reset the previous soloed group
                    mixer.SetFloat(this.soloGroupName, originalVolumes[this.soloGroupName]);
                }

                // Mute all groups except the soloed one
                foreach (string groupName in groupNames)
                {
                    if (groupName != soloGroupName)
                    {
                        mixer.SetFloat(groupName, 0f); // Mute all other groups
                    }
                }

                // Unmute the solo group
                mixer.SetFloat(soloGroupName, originalVolumes[soloGroupName]); // Restore original volume of the soloed group

                // Set the current soloed group
                this.soloGroupName = soloGroupName;
            }
            else
            {
                // Unmute the soloed group if it was muted before
                if (soloGroupName == this.soloGroupName)
                {
                    mixer.SetFloat(soloGroupName, originalVolumes[soloGroupName]);
                    this.soloGroupName = null; // Reset the solo group
                }
            }
        }
        else
        {
            Debug.LogError("AudioMixer is not assigned.");
        }
    }
}
