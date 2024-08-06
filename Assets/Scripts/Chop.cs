using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import the TextMeshPro namespace

public class Chop : MonoBehaviour
{
    public Button chopButton; // Reference to the button that will trigger the chop action
    public TextMeshProUGUI feedbackText; // Reference to the TextMeshProUGUI component for feedback

    private const int MaxChops = 16; // Maximum number of chops
    public List<float> timestamps = new List<float>(); // List to store timestamps

    private AudioSource audioSource; // Reference to the AudioSource

    private void Start()
    {
        // Ensure the button is assigned and add a listener
        if (chopButton != null)
        {
            chopButton.onClick.AddListener(OnChopButtonClick);
        }
        else
        {
            Debug.LogError("Chop button is not assigned.");
        }

        // Ensure the TextMeshProUGUI component is assigned
        if (feedbackText == null)
        {
            Debug.LogError("Feedback TextMeshProUGUI is not assigned.");
        }

        // Get the AudioSource component from the same GameObject
        audioSource = GameObject.Find("MusicPlayer").GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is not assigned or found.");
        }
    }

    private void OnChopButtonClick()
    {
        if (timestamps.Count >= MaxChops)
        {
            UpdateFeedbackText("Maximum number of chops reached.");
            return;
        }

        if (audioSource != null)
        {
            // Record the current timestamp of the audio source
            float timestamp = audioSource.time;
            timestamps.Add(timestamp);

            // Update feedback with the added timestamp
            UpdateFeedbackText($"Added chop: Timestamp={timestamp}");
        }
    }

    private void UpdateFeedbackText(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
        else
        {
            Debug.LogError("Feedback TextMeshProUGUI is not assigned.");
        }
    }
}
