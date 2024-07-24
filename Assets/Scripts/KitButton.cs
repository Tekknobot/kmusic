using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;

public class KitButton : MonoBehaviour
{
    public AudioHelm.Sampler sampler;

    // Start is called before the first frame update
    void Start()
    {
        sampler = GameObject.Find("Sequencer").GetComponent<Sampler>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
