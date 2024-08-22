using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Chop : MonoBehaviour
{
    public Button chopButton; // Reference to the button that will trigger the chop action
    public Button clearChopsButton; // Reference to the button that will clear the chops
    public Button trimBackButton; // Button for trimming back by 0.1 seconds
    public Button trimForwardButton; // Button for trimming forward by 0.1 seconds
    public Button microStepBackButton; // Button for micro-stepping back by 0.01 seconds
    public Button microStepForwardButton; // Button for micro-stepping forward by 0.01 seconds
    public TextMeshProUGUI feedbackText; // Reference to the TextMeshProUGUI component for feedback
    public TextMeshProUGUI currentTimestampText; // Reference to display the current timestamp

    private const int MaxChops = 16; // Maximum number of chops
    public int selectedChopIndex = 0; // Index of the currently selected chop
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

        // Ensure the trim buttons are assigned and add listeners
        if (trimBackButton != null)
        {
            trimBackButton.onClick.AddListener(OnTrimBackButtonClick);
        }
        else
        {
            Debug.LogError("Trim Back button is not assigned.");
        }

        if (trimForwardButton != null)
        {
            trimForwardButton.onClick.AddListener(OnTrimForwardButtonClick);
        }
        else
        {
            Debug.LogError("Trim Forward button is not assigned.");
        }

        if (microStepBackButton != null)
        {
            microStepBackButton.onClick.AddListener(OnMicroStepBackButtonClick);
        }
        else
        {
            Debug.LogError("Micro Step Back button is not assigned.");
        }

        if (microStepForwardButton != null)
        {
            microStepForwardButton.onClick.AddListener(OnMicroStepForwardButtonClick);
        }
        else
        {
            Debug.LogError("Micro Step Forward button is not assigned.");
        }

        // Ensure the TextMeshProUGUI components are assigned
        if (feedbackText == null)
        {
            Debug.LogError("Feedback TextMeshProUGUI is not assigned.");
        }
        if (currentTimestampText == null)
        {
            Debug.LogError("Current Timestamp TextMeshProUGUI is not assigned.");
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
        UpdateCurrentTimestampDisplay(); // Display the first timestamp
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

            // Select the newly added chop
            selectedChopIndex = timestamps.Count - 1;

            // Save the updated timestamps
            SaveTimestamps();

            // Update feedback with the added timestamp and current chop count
            UpdateFeedbackText($"Added chop: Timestamp = {timestamp}. Total Chops = {timestamps.Count}");
            UpdateCurrentTimestampDisplay();
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

    private void OnTrimBackButtonClick()
    {
        AdjustSelectedTimestamp(-0.1f);
    }

    private void OnTrimForwardButtonClick()
    {
        AdjustSelectedTimestamp(0.1f);
    }

    private void OnMicroStepBackButtonClick()
    {
        AdjustSelectedTimestamp(-0.01f);
    }

    private void OnMicroStepForwardButtonClick()
    {
        AdjustSelectedTimestamp(0.01f);
    }

    private void AdjustSelectedTimestamp(float adjustment)
    {
        if (timestamps.Count > 0 && selectedChopIndex >= 0 && selectedChopIndex < timestamps.Count)
        {
            timestamps[selectedChopIndex] = Mathf.Max(0, timestamps[selectedChopIndex] + adjustment);
            UpdateCurrentTimestampDisplay();
            SaveTimestamps();
            UpdateFeedbackText($"Adjusted chop {selectedChopIndex + 1} by {adjustment} seconds. New Timestamp = {timestamps[selectedChopIndex]}");
        }
        else
        {
            UpdateFeedbackText("No chops available to adjust.");
        }
    }

    private void UpdateFeedbackText(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }

    public void UpdateCurrentTimestampDisplay()
    {
        if (timestamps.Count > 0 && selectedChopIndex >= 0 && selectedChopIndex < timestamps.Count)
        {
            currentTimestampText.text = $"Current Chop: {selectedChopIndex + 1}/{timestamps.Count} Timestamp: {timestamps[selectedChopIndex]:F2} sec";
        }
        else
        {
            currentTimestampText.text = "No chops available.";
        }
    }

    // Method to clear the chops
    public void ClearChops()
    {
        timestamps.Clear();
        SaveTimestamps(); // Clear saved chops
        selectedChopIndex = 0;
        UpdateCurrentTimestampDisplay();
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

            // Reset the selected chop index
            selectedChopIndex = timestamps.Count > 0 ? 0 : -1;

            UpdateCurrentTimestampDisplay();
            UpdateFeedbackText($"Loaded {timestamps.Count} chops.");
        }
        else
        {
            UpdateFeedbackText("No saved chops found.");
        }
    }
}
