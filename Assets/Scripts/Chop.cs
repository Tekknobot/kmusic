using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import the TextMeshPro namespace

public class Chop : MonoBehaviour
{
    public Button chopButton; // Reference to the button that will trigger the chop action
    public Button clearChopsButton; // Reference to the button that will clear the chops
    public TextMeshProUGUI feedbackText; // Reference to the TextMeshProUGUI component for feedback

    private const int MaxChops = 16; // Maximum number of chops
    public List<float> timestamps = new List<float>(); // List to store timestamps

    private AudioSource audioSource; // Reference to the AudioSource

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
        UpdateFeedbackText("Chops cleared.");
    }
}
