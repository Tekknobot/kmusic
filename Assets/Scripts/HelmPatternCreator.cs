using System.Collections.Generic;
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
    private int currentSequencerIndex = 0; // Index of the current sequencer playing
    private float[] sequencerStartTimes;  // Array to track start times of each sequencer
    private Dictionary<int, List<AudioHelm.Note>> sequencerNotes = new Dictionary<int, List<AudioHelm.Note>>();


    void Start()
    {
        // Ensure buttons are assigned and add listeners
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
        if (isPlaying && patternsCreated && targetSequencers.Count > 0)
        {
            HelmSequencer currentSequencer = targetSequencers[currentSequencerIndex];

            // Calculate the elapsed time since the sequencer started
            float elapsedTime = Time.time - sequencerStartTimes[currentSequencerIndex];
            float cycleLength = GetCycleLengthInSeconds(currentSequencer);

            // Check if the current sequencer has finished playing its cycle
            if (elapsedTime >= cycleLength)
            {
                // Stop the current sequencer and clear its notes
                currentSequencer.Clear();

                // Move to the next sequencer
                currentSequencerIndex = (currentSequencerIndex + 1) % targetSequencers.Count;

                // Clear notes from all other sequencers
                foreach (var sequencer in targetSequencers)
                {
                    if (sequencer != targetSequencers[currentSequencerIndex])
                    {
                        sequencer.Clear();
                    }
                }

                // Add notes to the next sequencer and start it
                HelmSequencer nextSequencer = targetSequencers[currentSequencerIndex];

                // Reapply notes from the saved notes for the next sequencer
                if (sequencerNotes.TryGetValue(currentSequencerIndex, out List<AudioHelm.Note> notes))
                {
                    foreach (AudioHelm.Note note in notes)
                    {
                        nextSequencer.AddNote(note.note, note.start, note.end, note.velocity);
                    }
                }

                // Start the next sequencer
                nextSequencer.StartOnNextCycle();

                // Update the start time for the next sequencer
                sequencerStartTimes[currentSequencerIndex] = Time.time;
            }
        }
    }

    void CreateAndTransferPattern()
    {
        // Instantiate a new sequencer from the prefab
        GameObject newSequencerObj = Instantiate(sequencerPrefab, transform);
        HelmSequencer newSequencer = newSequencerObj.GetComponent<HelmSequencer>();

        // Store notes for the new sequencer
        List<AudioHelm.Note> notes = new List<AudioHelm.Note>();
        foreach (AudioHelm.Note note in sourceSequencer.GetAllNotes())
        {
            newSequencer.AddNote(note.note, note.start, note.end, note.velocity);
            notes.Add(note);
        }

        // Save the notes for this sequencer
        sequencerNotes[targetSequencers.Count] = notes;

        // Copy other settings from the source sequencer if necessary
        CopySequencerSettings(sourceSequencer, newSequencer);

        // Add the new sequencer to the list
        targetSequencers.Add(newSequencer);

        // Initialize the start times array
        sequencerStartTimes = new float[targetSequencers.Count];

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
            PlayCreatedPatterns();
        }

        isPlaying = !isPlaying;
    }

    void PlayCreatedPatterns()
    {
        if (!patternsCreated)
        {
            Debug.LogError("No patterns created to play.");
            return;
        }

        // Stop the source sequencer from playing notes
        sourceSequencer.AllNotesOff(); // Stop playing all notes
        sourceSequencer.loop = false;  // Ensure it doesn't loop

        // Reset the clock timer
        clock.Reset();

        // Unpause the clock
        clock.pause = false;

        // Clear notes from all sequencers
        foreach (var sequencer in targetSequencers)
        {
            sequencer.AllNotesOff();
        }

        // Add notes to the first sequencer and start it
        if (targetSequencers.Count > 0)
        {
            HelmSequencer firstSequencer = targetSequencers[0];

            // Add notes from the saved notes for the first sequencer
            if (sequencerNotes.TryGetValue(0, out List<AudioHelm.Note> notes))
            {
                foreach (AudioHelm.Note note in notes)
                {
                    firstSequencer.AddNote(note.note, note.start, note.end, note.velocity);
                }
            }

            // Start the first sequencer
            firstSequencer.StartOnNextCycle();

            // Initialize start time for the first sequencer
            sequencerStartTimes[0] = Time.time;
        }
    }

    void StopCreatedPatterns()
    {
        // Stop playback of all sequencers and turn off notes
        foreach (HelmSequencer sequencer in targetSequencers)
        {
            sequencer.AllNotesOff();
        }

        // Pause the clock
        clock.pause = true;

        // Optionally, you can reset the flag to indicate patterns are not playing
        patternsCreated = false;

        // Optionally set the source sequencer to loop again if needed
        sourceSequencer.loop = true;

        // Reset the current sequencer index
        currentSequencerIndex = 0;
    }

    float GetCycleLengthInSeconds(HelmSequencer sequencer)
    {
        // Convert the sequencer division to a float value representing beats per minute
        float divisionMultiplier = DivisionToFloat(sequencer.division);
        return sequencer.length * 60.0f / (clock.bpm * divisionMultiplier);
    }

    float DivisionToFloat(HelmSequencer.Division division)
    {
        switch (division)
        {
            case HelmSequencer.Division.kEighth:
                return 2.0f;
            case HelmSequencer.Division.kSixteenth:
                return 4.0f;
            case HelmSequencer.Division.kThirtySecond:
                return 8.0f;
            default: // Assume kQuarter as default
                return 1.0f;
        }
    }
}
