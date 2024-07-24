using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudioHelm;

public class SequencerTest : MonoBehaviour
{
    public GameObject sequencer;

    void Start()
    {
        var sampleSequencer = sequencer.GetComponent<AudioHelm.SampleSequencer>();
        if (sampleSequencer != null)
        {
            sampleSequencer.AddNote(63, 1, 2, 1.0f);
            //sampleSequencer.RemoveNotesInRange(63, 1, 2);
        }
        else
        {
            Debug.LogError("SampleSequencer component not found.");
        }
    }
}
