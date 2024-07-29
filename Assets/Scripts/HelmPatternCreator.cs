using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioHelm;
using System.Collections;

public class PatternCreator : MonoBehaviour
{
    public HelmSequencer sourceSequencer; // Existing sequencer
    public GameObject sequencerPrefab;    // Prefab to instantiate new sequencers
    public Button createPatternButton;    // Button to create and transfer patterns
    public Button playPatternsButton;     // Button to play the created patterns
    public AudioHelmClock clock;

    private List<HelmSequencer> targetSequencers = new List<HelmSequencer>(); // List to hold created sequencers
    private bool patternsCreated = false; // Flag to check if patterns have been created
    private bool isPlaying = false;       // Flag to check if patterns are currently playing

    private float sequencerStartDelay;    // Delay between starting each sequencer

    void Start()
    {
        if (createPatternButton != null)
        {
            createPatternButton.onClick.AddListener(CreateAndTransferPattern);
        }
        else
        {
            Debug.LogError("Create Pattern Button not assigned.");
        }

        if (playPatternsButton != null)
        {
            playPatternsButton.onClick.AddListener(TogglePlayPatterns);
        }
        else
        {
            Debug.LogError("Play Patterns Button not assigned.");
        }
    }

    void Update()
    {
        // No pattern synchronization logic needed here
    }

    void CreateAndTransferPattern()
    {
        // Instantiate a new sequencer from the prefab
        GameObject newSequencerObj = Instantiate(sequencerPrefab, transform);
        HelmSequencer newSequencer = newSequencerObj.GetComponent<HelmSequencer>();

        // Name the new sequencer
        newSequencer.name = "Helm Pattern " + (targetSequencers.Count + 1);

        // Transfer notes from the source sequencer to the new sequencer
        List<AudioHelm.Note> notes = new List<AudioHelm.Note>();
        foreach (AudioHelm.Note note in sourceSequencer.GetAllNotes())
        {
            newSequencer.AddNote(note.note, note.start, note.end, note.velocity);
            notes.Add(note);
        }

        // Save the notes for this sequencer (optional if you need to reapply them later)
        // You can store notes in a dictionary or list if needed

        // Copy other settings from the source sequencer if necessary
        CopySequencerSettings(sourceSequencer, newSequencer);

        // Add the new sequencer to the list
        targetSequencers.Add(newSequencer);

        // Set the flag to indicate patterns have been created
        patternsCreated = true;
    }

    void CopySequencerSettings(HelmSequencer source, HelmSequencer target)
    {
        // Copy any relevant settings from source to target
        target.length = source.length;
        target.division = source.division;
    }

    void TogglePlayPatterns()
    {
        if (isPlaying)
        {
            StopCreatedPatterns();
        }
        else
        {
            CalculateSequencerStartDelay(); // Calculate delay based on BPM
            StartCoroutine(PlayCreatedPatterns());
        }

        isPlaying = !isPlaying;
    }

    void CalculateSequencerStartDelay()
    {
        // Calculate the duration of one beat in seconds
        float bpm = clock.bpm; // Get BPM from the clock
        float beatsPerBar = 4.0f; // Assuming 4 beats per bar
        float beatsPerMinute = bpm;
        float beatsPerSecond = beatsPerMinute / 60.0f;
        float secondsPerBeat = 1.0f / beatsPerSecond;
        
        // Calculate delay in seconds (1 bar = beatsPerBar beats)
        sequencerStartDelay = secondsPerBeat * beatsPerBar;
    }

    IEnumerator PlayCreatedPatterns()
    {
        if (!patternsCreated)
        {
            Debug.LogError("No patterns created to play.");
            yield break;
        }

        // Stop the source sequencer from playing notes
        sourceSequencer.AllNotesOff(); // Stop playing all notes
        sourceSequencer.loop = false;  // Ensure it doesn't loop

        // Reset the clock timer
        clock.Reset();
        clock.pause = false; // Ensure the clock is not paused

        while (isPlaying)
        {
            for (int i = 0; i < targetSequencers.Count; i++)
            {
                HelmSequencer currentSequencer = targetSequencers[i];
                
                // Set the current sequencer to loop
                currentSequencer.loop = true;
                currentSequencer.StartOnNextCycle(); // Start the current sequencer

                // Set all other sequencers to not loop
                for (int j = 0; j < targetSequencers.Count; j++)
                {
                    if (j != i)
                    {
                        targetSequencers[j].loop = false;
                    }
                }

                // Wait for the calculated delay before starting the next sequencer
                yield return new WaitForSeconds(sequencerStartDelay);
            }
        }
    }

    void StopCreatedPatterns()
    {
        // Stop playback of all sequencers and turn off notes
        foreach (HelmSequencer sequencer in targetSequencers)
        {
            sequencer.AllNotesOff();
            sequencer.Clear();
        }

        // Pause the clock
        clock.pause = true;

        // Optionally, you can reset the flag to indicate patterns are not playing
        patternsCreated = false;

        // Optionally set the source sequencer to loop again if needed
        sourceSequencer.loop = true;
    }
}
