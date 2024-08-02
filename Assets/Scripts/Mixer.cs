using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using TMPro;
using AudioHelm;
using UnityEngine.UI;

public class Mixer : MonoBehaviour
{
    public Slider[] sliders = new Slider[8]; // Array to hold 8 sliders
    public Toggle[] muteButtons = new Toggle[8]; // Array to hold 8 mute buttons
    public Toggle[] soloButtons = new Toggle[8]; // Array to hold 8 solo buttons
    public AudioMixer mixer;

    public Slider helmSlider; // Additional slider for Helm mixer group

    private string[] groupNames = new string[8]; // To store the names of the audio mixer groups
    private Dictionary<string, float> originalVolumes = new Dictionary<string, float>(); // To store original volumes
    private Dictionary<string, float> currentVolumes = new Dictionary<string, float>(); // To store current volumes

    private const string PLAYER_PREFS_PREFIX = "Mixer_"; // Prefix for PlayerPrefs keys

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
                int index = i; // Capture the index for use in the lambda
                slider.onValueChanged.AddListener(delegate { OnSliderValueChanged(slider, index); });
            }
            else
            {
                Debug.LogError($"Slider at index {i} is not assigned.");
            }

            // Store the name of each group
            groupNames[i] = slider.name; // Assuming slider names match group names
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
        }

        // Initialize Helm slider
        if (helmSlider != null)
        {
            helmSlider.onValueChanged.AddListener(OnHelmSliderValueChanged);
            string helmGroupName = "Helm";
            groupNames = AddGroupName(groupNames, helmGroupName); // Add helm group name to the groupNames array

            // Load the saved Helm slider value
            LoadHelmSliderValue();
        }
        else
        {
            Debug.LogError("Helm slider is not assigned.");
        }

        // Load saved values
        LoadSliderValues();

        // Store original volumes of each group
        foreach (string groupName in groupNames)
        {
            float currentVolume;
            mixer.GetFloat(groupName, out currentVolume);
            originalVolumes[groupName] = currentVolume;
        }
    }

    void OnSliderValueChanged(Slider changedSlider, int index)
    {
        // Check if the corresponding mute button is on
        if (muteButtons[index].isOn)
        {
            return; // Ignore slider changes when muted
        }

        Debug.Log("Slider value changed: " + changedSlider.value);

        // Assuming the slider's name matches the exposed parameter name in the AudioMixer
        string parameterName = changedSlider.name;

        // Directly use slider value
        float sliderValue = changedSlider.value;

        if (mixer != null)
        {
            // Set the value directly as the volume
            bool result = mixer.SetFloat(parameterName, sliderValue);
            if (!result)
            {
                Debug.LogError($"Failed to set AudioMixer parameter '{parameterName}'. Ensure the parameter is exposed and the name matches.");
            }

            // Save the current volume
            currentVolumes[parameterName] = sliderValue;

            // Save the slider value
            SaveSliderValue(parameterName, sliderValue);
        }
        else
        {
            Debug.LogError("AudioMixer is not assigned.");
        }
    }

    void OnHelmSliderValueChanged(float value)
    {
        Debug.Log("Helm slider value changed: " + value);

        // Assuming the helmSlider's name matches the exposed parameter name in the AudioMixer
        string parameterName = "Helm";

        if (mixer != null)
        {
            // Set the value directly as the volume
            bool result = mixer.SetFloat(parameterName, value);
            if (!result)
            {
                Debug.LogError($"Failed to set AudioMixer parameter '{parameterName}'. Ensure the parameter is exposed and the name matches.");
            }

            // Save the current volume
            currentVolumes[parameterName] = value;

            // Save the slider value
            SaveSliderValue(parameterName, value);
            SaveHelmSliderValue(value); // Save Helm slider value to PlayerPrefs
        }
        else
        {
            Debug.LogError("AudioMixer is not assigned.");
        }
    }

    void OnMuteButtonValueChanged(Toggle changedToggle, int index)
    {
        // Get the group name corresponding to the button
        string groupName = groupNames[index];
        bool isMuted = changedToggle.isOn;

        if (mixer != null)
        {
            if (isMuted)
            {
                // Store the current volume before muting
                float currentVolume;
                mixer.GetFloat(groupName, out currentVolume);
                currentVolumes[groupName] = currentVolume;

                // Set volume to -80dB if muted
                mixer.SetFloat(groupName, -80f);
            }
            else
            {
                // Restore the current slider value if unmuted
                float sliderValue = sliders[index].value;
                mixer.SetFloat(groupName, sliderValue);

                // Update the currentVolumes dictionary
                currentVolumes[groupName] = sliderValue;
            }
        }
        else
        {
            Debug.LogError("AudioMixer is not assigned.");
        }
    }

    void SaveSliderValue(string parameterName, float value)
    {
        PlayerPrefs.SetFloat(PLAYER_PREFS_PREFIX + parameterName, value);
        PlayerPrefs.Save();
    }

    void SaveHelmSliderValue(float value)
    {
        string helmGroupName = helmSlider.name;
        PlayerPrefs.SetFloat(PLAYER_PREFS_PREFIX + helmGroupName, value);
        PlayerPrefs.Save();
    }

    void LoadSliderValues()
    {
        foreach (string groupName in groupNames)
        {
            string key = PLAYER_PREFS_PREFIX + groupName;
            if (PlayerPrefs.HasKey(key))
            {
                float savedValue = PlayerPrefs.GetFloat(key);
                foreach (Slider slider in sliders)
                {
                    if (slider.name == groupName)
                    {
                        slider.value = savedValue;
                        OnSliderValueChanged(slider, System.Array.IndexOf(sliders, slider)); // Update AudioMixer
                        break;
                    }
                }
            }
        }
    }

    void LoadHelmSliderValue()
    {
        string helmGroupName = helmSlider.name;
        string key = PLAYER_PREFS_PREFIX + helmGroupName;
        if (PlayerPrefs.HasKey(key))
        {
            float savedValue = PlayerPrefs.GetFloat(key);
            helmSlider.value = savedValue;
            OnHelmSliderValueChanged(savedValue); // Update AudioMixer
        }
    }

    private string[] AddGroupName(string[] groupNames, string newGroupName)
    {
        List<string> groupNameList = new List<string>(groupNames);
        groupNameList.Add(newGroupName);
        return groupNameList.ToArray();
    }
}
