using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;

public class DrumSamples : MonoBehaviour
{
    // Define 8 public arrays of 8 AudioClip elements each
    public AudioHelm.Keyzone[] Rock = new AudioHelm.Keyzone[8];
    public AudioHelm.Keyzone[] Hiphop = new AudioHelm.Keyzone[8];
    public AudioHelm.Keyzone[] House = new AudioHelm.Keyzone[8];
    public AudioHelm.Keyzone[] Boombap = new AudioHelm.Keyzone[8];
    public AudioHelm.Keyzone[] Lofi = new AudioHelm.Keyzone[8];
    public AudioHelm.Keyzone[] Techno = new AudioHelm.Keyzone[8];
    public AudioHelm.Keyzone[] Trip = new AudioHelm.Keyzone[8];

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
