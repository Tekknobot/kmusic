using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;

public class KitButton : MonoBehaviour
{
    public AudioHelm.Sampler sampler;
    public DrumSamples drumSamples;
    private string currentKitName = "None";

    void Start()
    {
        sampler = GameObject.Find("Sequencer").GetComponent<Sampler>();
        drumSamples = GameObject.Find("DrumSamples").GetComponent<DrumSamples>();
    }

    void Update()
    {
        sampler = PatternManager.Instance.GetActiveDrumSequencer().gameObject.GetComponent<Sampler>();
    }

    public void OnButtonClick()
    {
        LoadKit(gameObject.name);
    }

    public void LoadKit(string kitName)
    {
        currentKitName = kitName;
        PlayerPrefs.SetString("CurrentDrumKit", kitName);
        PlayerPrefs.Save();

        switch (kitName)
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
                Debug.LogError("Unknown kit name: " + kitName);
                break;
        }
    }

    public void LoadKitIntoSampler(Sampler targetSampler)
    {
        if (drumSamples == null)
        {
            Debug.LogError("DrumSamples component not found.");
            return;
        }

        switch (currentKitName)
        {
            case "707":
                SwapSamplesToTargetSampler(targetSampler, drumSamples.Rock);
                break;
            case "808":
                SwapSamplesToTargetSampler(targetSampler, drumSamples.Hiphop);
                break;
            case "909":
                SwapSamplesToTargetSampler(targetSampler, drumSamples.House);
                break;
            case "Boombap":
                SwapSamplesToTargetSampler(targetSampler, drumSamples.Boombap);
                break;
            case "Lofi":
                SwapSamplesToTargetSampler(targetSampler, drumSamples.Lofi);
                break;
            case "Techno":
                SwapSamplesToTargetSampler(targetSampler, drumSamples.Techno);
                break;
            case "Trip":
                SwapSamplesToTargetSampler(targetSampler, drumSamples.Trip);
                break;
            default:
                Debug.LogError("Unknown kit name: " + currentKitName);
                break;
        }
    }

    private void SwapSamples(AudioHelm.Keyzone[] keyzones)
    {
        if (sampler == null)
        {
            Debug.LogError("Sampler component not found.");
            return;
        }
        SwapSamplesToAllDrumSamplers(keyzones);
    }

    private void SwapSamplesToAllDrumSamplers(AudioHelm.Keyzone[] keyzones)
    {
        // Find all drum samplers in the scene
        var drumSamplers = FindObjectsOfType<Sampler>();

        foreach (var drumSampler in drumSamplers)
        {
            // Check if the drum sampler's game object name starts with "Sequencer"
            if (drumSampler != null && drumSampler.gameObject.name.StartsWith("Sequencer"))
            {
                drumSampler.keyzones.Clear();

                // Add the keyzones from the provided array to the drum sampler
                for (int i = 0; i < keyzones.Length; i++)
                {
                    drumSampler.keyzones.Add(keyzones[i]);
                }
            }
        }
    }


    private void SwapSamplesToTargetSampler(Sampler targetSampler, AudioHelm.Keyzone[] keyzones)
    {
        targetSampler.keyzones.Clear();
        for (int i = 0; i < keyzones.Length; i++)
        {
            targetSampler.keyzones.Add(keyzones[i]);
        }
    }
}
