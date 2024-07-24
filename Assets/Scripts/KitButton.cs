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
        if (gameObject.name == "707")
        {
            SwapSamplesWithRockArray();
        }
    }

    // Method to swap the keyzones with the Rock array
    private void SwapSamplesWithRockArray()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
