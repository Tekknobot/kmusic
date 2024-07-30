using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using AudioHelm;

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

    private int currentSequencerIndex = 0; // Index of the currently playing sequencer
    private float beatsPerBar = 1f; // Number of beats in one bar
    private double nextSequencerSwitchTime = 0f; // Time to switch to the next sequencer in global beat time

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
            playPatternsButton.onClick.AddListener(StartPlayingPatterns);
        }
        else
        {
            Debug.LogError("Play Patterns Button not assigned.");
        }
    }

    void Update()
    {
        if (isPlaying)
        {
            double globalBeatTime = AudioHelmClock.GetGlobalBeatTime();
            Debug.Log($"Global Beat Time: {globalBeatTime}");

            // Check if it's time to switch to the next sequencer
            if (globalBeatTime >= nextSequencerSwitchTime)
            {
                StartCoroutine(SmoothTransitionToNextSequencer());
            }
        }
    }

    void CreateAndTransferPattern()
    {
        if (sequencerPrefab == null || sourceSequencer == null)
        {
            Debug.LogError("Sequencer Prefab or Source Sequencer not assigned.");
            return;
        }

        // Instantiate a new sequencer from the prefab
        GameObject newSequencerObj = Instantiate(sequencerPrefab, transform);
        HelmSequencer newSequencer = newSequencerObj.GetComponent<HelmSequencer>();

        if (newSequencer == null)
        {
            Debug.LogError("New sequencer prefab does not have a HelmSequencer component.");
            return;
        }

        // Name the new sequencer
        newSequencer.name = "Helm Pattern " + (targetSequencers.Count + 1);

        newSequencer.loop = false;

        // Transfer notes from the source sequencer to the new sequencer
        TransferNotes(sourceSequencer, newSequencer);

        // Copy other settings from the source sequencer if necessary
        CopySequencerSettings(sourceSequencer, newSequencer);

        // Add the new sequencer to the list
        targetSequencers.Add(newSequencer);

        // Set the flag to indicate patterns have been created
        patternsCreated = true;

        Debug.Log("Pattern created and transferred.");
    }

    void TransferNotes(HelmSequencer source, HelmSequencer target)
    {
        // Clear existing notes in the target sequencer
        target.Clear();

        // Transfer notes from the source sequencer to the target sequencer
        foreach (AudioHelm.Note note in source.GetAllNotes())
        {
            target.AddNote(note.note, note.start, note.end, note.velocity);
            Debug.Log($"Transferred note {note.note} to new sequencer.");
        }
    }

    void CopySequencerSettings(HelmSequencer source, HelmSequencer target)
    {
        // Copy any relevant settings from source to target
        target.length = source.length;
        target.division = source.division;
    }

    void StartPlayingPatterns()
    {
        if (!patternsCreated)
        {
            Debug.LogError("No patterns created to play.");
            return;
        }

        isPlaying = true;
        currentSequencerIndex = 0;

        // Ensure the clock is not paused
        clock.pause = false;
        sourceSequencer.loop = false;

        // Immediately start playing the first sequencer
        StartCoroutine(SmoothTransitionToNextSequencer());

        Debug.Log("Started playing patterns.");
    }

    IEnumerator SmoothTransitionToNextSequencer()
    {
        if (targetSequencers.Count > 0)
        {
            HelmSequencer currentSequencer = targetSequencers[currentSequencerIndex];
            currentSequencer.loop = false;
            currentSequencer.AllNotesOff();
        }

        // Move to the next sequencer index
        currentSequencerIndex = (currentSequencerIndex + 1) % targetSequencers.Count;
        HelmSequencer nextSequencer = targetSequencers[currentSequencerIndex];
        
        // Ensure the next sequencer is ready to play
        nextSequencer.loop = true;
        nextSequencer.StartOnNextCycle();

        // Calculate the time for the next sequencer switch
        double bpm = clock.bpm; // Get the BPM from the AudioHelmClock
        double secondsPerBeat = 60f / bpm; // Duration of one beat in seconds
        nextSequencerSwitchTime = AudioHelmClock.GetGlobalBeatTime() + beatsPerBar * secondsPerBeat;

        Debug.Log($"Switched to sequencer: {nextSequencer.name}");

        // Wait for the transition period to avoid overlap
        yield return new WaitForSeconds(0.1f);

        // Ensure a smooth transition
        yield return null;
    }

    void StopCreatedPatterns()
    {
        isPlaying = false;

        // Stop playback of all sequencers
        foreach (HelmSequencer sequencer in targetSequencers)
        {
            sequencer.loop = false;  // Ensure no sequencer is looping
            sequencer.AllNotesOff(); // Stop all notes
            sequencer.Clear();       // Clear the sequencer
            Debug.Log($"Stopped and cleared sequencer: {sequencer.name}");
        }

        // Pause the clock
        clock.pause = true;
        Debug.Log("Paused the clock.");

        // Optionally set the source sequencer to loop again if needed
        sourceSequencer.loop = true;
        Debug.Log("Set the source sequencer to loop.");
    }
}
