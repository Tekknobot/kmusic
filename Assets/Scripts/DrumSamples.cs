using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumSamples : MonoBehaviour
{
    // Define 8 public arrays of 8 AudioClip elements each
    public AudioClip[] Rock = new AudioClip[8];
    public AudioClip[] Hiphop = new AudioClip[8];
    public AudioClip[] House = new AudioClip[8];
    public AudioClip[] Boombap = new AudioClip[8];
    public AudioClip[] Lofi = new AudioClip[8];
    public AudioClip[] Techno = new AudioClip[8];
    public AudioClip[] Trip = new AudioClip[8];

    // Start is called before the first frame update
    void Start()
    {
        // Optionally, initialize your arrays with specific AudioClip references here
        // Example: kickSamples[0] = Resources.Load<AudioClip>("Path/To/Your/AudioClip");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
