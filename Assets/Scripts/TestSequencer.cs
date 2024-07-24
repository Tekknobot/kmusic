using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;

public class TestSequencer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
         GetComponent<SampleSequencer>().AddNote(51, 1, 1 + 1, 1.0f);
    }
}
