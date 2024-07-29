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
    private int currentSequencerIndex = 0; // Index of the current sequencer playing
    private float[] sequencerStartBeats;  // Array to track start beats of each sequencer
    private Dictionary<int, List<AudioHelm.Note>> sequencerNotes = new Dictionary<int, List<AudioHelm.Note>>();
    private int patternCounter = 1; // Counter for naming patterns

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
            float currentBeat = (float)AudioHelmClock.GetGlobalBeatTime(); // Cast to float
            float cycleLengthBeats = GetCycleLengthInBeats(currentSequencer);

            // Calculate the elapsed beats since the sequencer started
            float elapsedBeats = currentBeat - sequencerStartBeats[currentSequencerIndex];

            // Check if the current sequencer has finished playing its cycle
            if (elapsedBeats >= cycleLengthBeats)
            {
                // Move to the next sequencer
                currentSequencerIndex = (currentSequencerIndex + 1) % targetSequencers.Count;

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

                // Ensure correct timing for the next sequencer
                sequencerStartBeats[currentSequencerIndex] = currentBeat;

                // Start the next sequencer in the middle of the current sequencer's cycle
                StartNextSequencerInMiddle(nextSequencer);

                // Clear the current sequencer's notes after the cycle has ended
                StartCoroutine(ClearSequencerAfterCycle(currentSequencer, cycleLengthBeats));
            }
        }
    }

    void CreateAndTransferPattern()
    {
        // Instantiate a new sequencer from the prefab
        GameObject newSequencerObj = Instantiate(sequencerPrefab, transform);
        HelmSequencer newSequencer = newSequencerObj.GetComponent<HelmSequencer>();

        // Name the new sequencer
        newSequencer.name = "Helm Pattern " + patternCounter;

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

        // Initialize the start beats array
        sequencerStartBeats = new float[targetSequencers.Count];

        // Increment the pattern counter
        patternCounter++;

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
        clock.pause = false; // Ensure the clock is not paused

        // Clear notes from all sequencers
        foreach (var sequencer in targetSequencers)
        {
            sequencer.AllNotesOff();
            sequencer.Clear();
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

            // Initialize start time for the first sequencer
            sequencerStartBeats[0] = (float)AudioHelmClock.GetGlobalBeatTime(); // Cast to float

            // Start the first sequencer
            firstSequencer.StartOnNextCycle();
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

        // Reset the current sequencer index
        currentSequencerIndex = 0;
    }

    void StartNextSequencerInMiddle(HelmSequencer nextSequencer)
    {
        // Calculate the remaining beats in the current cycle
        float currentBeat = (float)AudioHelmClock.GetGlobalBeatTime(); // Cast to float
        float elapsedBeats = currentBeat - sequencerStartBeats[currentSequencerIndex];
        float cycleLengthBeats = GetCycleLengthInBeats(targetSequencers[currentSequencerIndex]);
        float remainingBeats = cycleLengthBeats - elapsedBeats;

        // Adjust the start beats for the next sequencer
        sequencerStartBeats[currentSequencerIndex] = currentBeat - (cycleLengthBeats / 2.0f);

        // Start the next sequencer on its next cycle, adjusting for the half-cycle offset
        nextSequencer.StartOnNextCycle();
    }

    IEnumerator ClearSequencerAfterCycle(HelmSequencer sequencer, float cycleLengthBeats)
    {
        // Wait until the cycle has fully completed
        float startBeat = (float)AudioHelmClock.GetGlobalBeatTime(); // Cast to float
        float waitForBeats = cycleLengthBeats;
        while (waitForBeats > 0)
        {
            float currentBeat = (float)AudioHelmClock.GetGlobalBeatTime(); // Cast to float
            yield return null; // Wait for the next frame
            float newBeat = (float)AudioHelmClock.GetGlobalBeatTime(); // Cast to float
            waitForBeats -= (newBeat - currentBeat);
        }

        // Clear the sequencer's notes after the cycle ends
        sequencer.AllNotesOff();
        sequencer.Clear();
    }

    float GetCycleLengthInBeats(HelmSequencer sequencer)
    {
        // Convert the sequencer length and division to beats
        float divisionMultiplier = DivisionToFloat(sequencer.division);
        return sequencer.length / divisionMultiplier;
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
