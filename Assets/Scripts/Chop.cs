using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Chop : MonoBehaviour
{
    public Button chopButton; // Reference to the button that will trigger the chop action
    public Button clearChopsButton; // Reference to the button that will clear the chops
    public TextMeshProUGUI feedbackText; // Reference to the TextMeshProUGUI component for feedback

    private const int MaxChops = 16; // Maximum number of chops
    public List<float> timestamps = new List<float>(); // List to store timestamps

    private AudioSource audioSource; // Reference to the AudioSource

    private const string TimestampsKey = "ChopTimestamps"; // Key for saving/loading timestamps

    private void Awake()
    {
        // Ensure the chop button is assigned and add a listener
        if (chopButton != null)
        {
            chopButton.onClick.AddListener(OnChopButtonClick);
        }
        else
        {
            Debug.LogError("Chop button is not assigned.");
        }

        // Ensure the clear chops button is assigned and add a listener
        if (clearChopsButton != null)
        {
            clearChopsButton.onClick.AddListener(OnClearChopsButtonClick);
        }
        else
        {
            Debug.LogError("Clear Chops button is not assigned.");
        }

        // Ensure the TextMeshProUGUI component is assigned
        if (feedbackText == null)
        {
            Debug.LogError("Feedback TextMeshProUGUI is not assigned.");
        }

        // Get the AudioSource component from the MusicPlayer GameObject
        GameObject musicPlayer = GameObject.Find("MusicPlayer");
        if (musicPlayer != null)
        {
            audioSource = musicPlayer.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("AudioSource component is not found on MusicPlayer.");
            }
        }
        else
        {
            Debug.LogError("MusicPlayer GameObject is not found.");
        }

        // Load the saved timestamps
        LoadTimestamps();
    }

    private void OnChopButtonClick()
    {
        if (timestamps.Count >= MaxChops)
        {
            UpdateFeedbackText("Maximum number of chops reached.");
            return;
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            // Record the current timestamp of the audio source
            float timestamp = audioSource.time;
            timestamps.Add(timestamp);

            // Save the updated timestamps
            SaveTimestamps();

            // Update feedback with the added timestamp and current chop count
            UpdateFeedbackText($"Added chop: Timestamp = {timestamp}. Total Chops = {timestamps.Count}");
        }
        else
        {
            UpdateFeedbackText("AudioSource is not playing or not assigned.");
        }
    }

    private void OnClearChopsButtonClick()
    {
        ClearChops();
    }

    private void UpdateFeedbackText(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    // Method to clear the chops
    public void ClearChops()
    {
        timestamps.Clear();
        SaveTimestamps(); // Clear saved chops
        UpdateFeedbackText("Chops cleared.");
    }

    // Method to save the timestamps to PlayerPrefs
    private void SaveTimestamps()
    {
        string timestampsString = string.Join(",", timestamps); // Convert timestamps to a comma-separated string
        PlayerPrefs.SetString(TimestampsKey, timestampsString); // Save the string in PlayerPrefs
        PlayerPrefs.Save(); // Force save
    }

    // Method to load the timestamps from PlayerPrefs
    private void LoadTimestamps()
    {
        timestamps.Clear(); // Clear any existing timestamps
        if (PlayerPrefs.HasKey(TimestampsKey))
        {
            string timestampsString = PlayerPrefs.GetString(TimestampsKey); // Load the saved string
            if (!string.IsNullOrEmpty(timestampsString))
            {
                string[] timestampArray = timestampsString.Split(','); // Split the string into an array
                foreach (string timestamp in timestampArray)
                {
                    if (float.TryParse(timestamp, out float result))
                    {
                        timestamps.Add(result); // Convert each string to a float and add to the list
                    }
                }
            }

            UpdateFeedbackText($"Loaded {timestamps.Count} chops.");
        }
        else
        {
            UpdateFeedbackText("No saved chops found.");
        }
    }
}
