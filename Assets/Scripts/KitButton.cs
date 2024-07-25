using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;

public class KitButton : MonoBehaviour
{
    public AudioHelm.Sampler sampler;
    public DrumSamples drumSamples;

    // Start is called before the first frame update
    void Start()
    {
        sampler = GameObject.Find("Sequencer").GetComponent<Sampler>();
        drumSamples = GameObject.Find("DrumSamples").GetComponent<DrumSamples>(); // Get the DrumSamples component
    }

    // Method to handle button click event
    public void OnButtonClick()
    {
        switch (gameObject.name)
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
