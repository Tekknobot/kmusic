using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import the TextMeshPro namespace

public class Chop : MonoBehaviour
{
    public Button chopButton; // Reference to the button that will trigger the chop action
    public TextMeshProUGUI feedbackText; // Reference to the TextMeshProUGUI component for feedback

    private const int MaxChops = 16; // Maximum number of chops
    public List<SampleData> chops = new List<SampleData>(); // List to store data for each chop

    // Variables for storing chop data
    private int songIndex = 0; // Example data - adjust as needed
    private float timestamp = 0.0f; // Example data - adjust as needed
    private int padNumber = 0; // Example data - adjust as needed

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
    }

    private void OnChopButtonClick()
    {
        if (chops.Count >= MaxChops)
        {
            UpdateFeedbackText("Maximum number of chops reached.");
            return;
        }

        // Create a new SampleData instance with the current data
        SampleData newChop = new SampleData(songIndex, timestamp, padNumber);

        // Add the new chop data to the list
        chops.Add(newChop);

        // Update feedback with the added chop data
        UpdateFeedbackText($"Added chop: SongIndex={newChop.songIndex}, Timestamp={newChop.timestamp}, PadNumber={newChop.padNumber}");
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

    // Method to update chop data manually
    public void UpdateChopData(int songIndex, float timestamp, int padNumber)
    {
        this.songIndex = songIndex;
        this.timestamp = timestamp;
        this.padNumber = padNumber;
    }
}

// SampleData class definition
[System.Serializable]
public class SampleData
{
    public int songIndex;
    public float timestamp;
    public int padNumber;

    public SampleData(int songIndex, float timestamp, int padNumber)
    {
        this.songIndex = songIndex;
        this.timestamp = timestamp;
        this.padNumber = padNumber;
    }
}
