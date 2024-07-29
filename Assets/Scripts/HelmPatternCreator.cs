using System.Collections;
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
    private Coroutine playCoroutine;      // Coroutine for sequential playback

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

    void CreateAndTransferPattern()
    {
        // Instantiate a new sequencer from the prefab
        GameObject newSequencerObj = Instantiate(sequencerPrefab, transform);
        HelmSequencer newSequencer = newSequencerObj.GetComponent<HelmSequencer>();

        // Copy the notes from the source sequencer to the new sequencer
        foreach (AudioHelm.Note note in sourceSequencer.GetAllNotes())
        {
            newSequencer.AddNote(note.note, note.start, note.end, note.velocity);
        }

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
        target.loop = false; // Ensure individual sequencers do not loop
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

        // Set the first target sequencer to loop initially
        if (targetSequencers.Count > 0)
        {
            targetSequencers[0].loop = true;
        }

        // Start the coroutine for sequential playback
        playCoroutine = StartCoroutine(PlaySequentialPatterns());
    }



    void StopCreatedPatterns()
    {
        // Stop the playback coroutine
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
        }

        // Pause the clock to stop all sequencers
        clock.pause = true;

        // Reset the position of all target sequencers
        foreach (HelmSequencer sequencer in targetSequencers)
        {
            sequencer.AllNotesOff();
        }

        // Optionally, you can reset the flag to indicate patterns are not playing
        patternsCreated = false;

        sourceSequencer.loop = true;
    }

    IEnumerator PlaySequentialPatterns()
    {
        bool firstSequencerPlayed = false;
        
        while (true)
        {
            foreach (HelmSequencer sequencer in targetSequencers)
            {
                if (!firstSequencerPlayed && sequencer == targetSequencers[0])
                {
                    // Set the loop to false after the first cycle
                    sequencer.StartOnNextCycle();
                    yield return new WaitForSeconds(GetCycleLengthInSeconds(sequencer));
                    sequencer.loop = false;
                    sequencer.AllNotesOff();
                    firstSequencerPlayed = true;
                }
                else
                {
                    // Start subsequent sequencers
                    sequencer.StartOnNextCycle();
                    yield return new WaitForSeconds(GetCycleLengthInSeconds(sequencer));
                    sequencer.AllNotesOff();
                }
            }
        }
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
