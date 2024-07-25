using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;

public class KitButton : MonoBehaviour
{
    public AudioHelm.Sampler sampler;
    public DrumSamples drumSamples;
    private string currentKitName = "None"; // Variable to keep track of the current kit name

    // Start is called before the first frame update
    void Start()
    {
        sampler = GameObject.Find("Sequencer").GetComponent<Sampler>();
        drumSamples = GameObject.Find("DrumSamples").GetComponent<DrumSamples>(); // Get the DrumSamples component

        // Load the saved kit
        LoadKit();
    }

    // Method to handle button click event
    public void OnButtonClick()
    {
        switch (gameObject.name)
        {
            case "707":
                SwapSamples(drumSamples.Rock);
                SaveKit("707");
                break;
            case "808":
                SwapSamples(drumSamples.Hiphop);
                SaveKit("808");
                break;
            case "909":
                SwapSamples(drumSamples.House);
                SaveKit("909");
                break;
            case "Boombap":
                SwapSamples(drumSamples.Boombap);
                SaveKit("Boombap");
                break;
            case "Lofi":
                SwapSamples(drumSamples.Lofi);
                SaveKit("Lofi");
                break;
            case "Techno":
                SwapSamples(drumSamples.Techno);
                SaveKit("Techno");
                break;
            case "Trip":
                SwapSamples(drumSamples.Trip);
                SaveKit("Trip");
                break;
            default:
                Debug.LogError("Unknown kit name: " + gameObject.name);
                break;
        }
    }

    // Generic method to swap the keyzones with the given array
    private void SwapSamples(AudioHelm.Keyzone[] keyzones)
    {
        if (sampler == null || drumSamples == null)
        {
            Debug.LogError("Sampler or DrumSamples component not found.");
            return;
        }

        // Clear the current keyzones in the sampler
        sampler.keyzones.Clear();

        // Add the keyzones from the provided array to the sampler
        for (int i = 0; i < keyzones.Length; i++)
        {
            sampler.keyzones.Add(keyzones[i]);
        }
    }

    // Method to save the currently active kit name
    private void SaveKit(string kitName)
    {
        PlayerPrefs.SetString("CurrentDrumKit", kitName);
        PlayerPrefs.Save();
        currentKitName = kitName;
    }

    // Method to load the saved kit name
    private void LoadKit()
    {
        currentKitName = PlayerPrefs.GetString("CurrentDrumKit", "707"); // Default to "707" if no kit is saved
        switch (currentKitName)
        {
            case "707":
                SwapSamples(drumSamples.Rock);
                break;
            case "808":
                SwapSamples(drumSamples.Hiphop);
                break;
            case "909":
                SwapSamples(drumSamples.House);
                break;
            case "Boombap":
                SwapSamples(drumSamples.Boombap);
                break;
            case "Lofi":
                SwapSamples(drumSamples.Lofi);
                break;
            case "Techno":
                SwapSamples(drumSamples.Techno);
                break;
            case "Trip":
                SwapSamples(drumSamples.Trip);
                break;
            default:
                Debug.LogError("Unknown kit name: " + currentKitName);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Optionally, implement any logic needed for per-frame updates.
    }
}
