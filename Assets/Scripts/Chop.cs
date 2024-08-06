using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Ensure this namespace is included for UI elements
using TMPro; // Import TextMeshPro namespace

public class Chop : MonoBehaviour
{
    // Reference to the button that will trigger the chop action
    public Button chopButton;

    // Reference to the TextMeshProUGUI component for feedback
    public TextMeshProUGUI feedbackText;

    // Maximum number of chops
    private const int MaxChops = 16;

    // List to store data for each chop
    private List<SampleData> chops = new List<SampleData>();

    // Variables for storing chop data
    private int songIndex = 0;        // Example data - adjust as needed
    private float timestamp = 0.0f;   // Example data - adjust as needed
    private int padNumber = 0;        // Example data - adjust as needed

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

    // You might want to include additional methods to update songIndex, timestamp, and padNumber
    public void UpdateChopData(int songIndex, float timestamp, int padNumber)
    {
        this.songIndex = songIndex;
        this.timestamp = timestamp;
        this.padNumber = padNumber;
    }
}
