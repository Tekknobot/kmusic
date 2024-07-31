using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro; // Ensure you have TextMeshPro namespace
using AudioHelm;

public class HelmPatternCreator : MonoBehaviour
{
    public HelmSequencer sourceSequencer;    // Existing sequencer
    public GameObject sequencerPrefab;       // Prefab to instantiate new sequencers
    public Button createPatternButton;       // Button to create and transfer patterns
    public Button playPatternsButton;        // Button to start playing the created patterns
    public Button stopPatternsButton;        // Button to stop all patterns
    public Button removePatternButton;       // Button to remove a pattern
    public TextMeshProUGUI patternDisplayText; // TextMeshProUGUI to display both current playing pattern and total patterns
    public BoardManager boardManager;         // Reference to BoardManager
    public AudioHelmClock clock;              // Reference to the AudioHelm clock

    private List<HelmSequencer> targetSequencers = new List<HelmSequencer>(); // List to hold created sequencers
    private bool patternsCreated = false;     // Flag to check if patterns have been created
    private bool isPlaying = false;           // Flag to check if patterns are currently playing
    private int currentSequencerIndex = 0;    // Index of the currently playing sequencer
    private bool isClockPaused = false;       // Flag to check if the clock is paused
    private float loopDuration = 0f;          // Duration of the loop in seconds

    void Start()
    {
        // Ensure all buttons are assigned
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

        if (stopPatternsButton != null)
        {
            stopPatternsButton.onClick.AddListener(StopCreatedPatterns);
        }
        else
        {
            Debug.LogError("Stop Patterns Button not assigned.");
        }

        if (removePatternButton != null)
        {
            removePatternButton.onClick.AddListener(RemovePattern);
        }
        else
        {
            Debug.LogError("Remove Pattern Button not assigned.");
        }

        UpdatePatternDisplay(); // Initialize the pattern display text
    }

    IEnumerator SmoothTransitionToNextSequencer()
    {
        // Stop all sequencers
        foreach (var sequencer in targetSequencers)
        {
            StopSequencer(sequencer);
            sequencer.loop = false;
        }     

        // Ensure the source sequencer is stopped
        sourceSequencer.AllNotesOff();
        sourceSequencer.loop = false;        

        if (targetSequencers.Count == 0)
        {
            Debug.LogError("No target sequencers available for transition.");
            yield break;
        }

        while (isPlaying)  // Continuous loop to keep transitioning as long as isPlaying is true
        {
            // Stop all sequencers
            foreach (var sequencer in targetSequencers)
            {
                StopSequencer(sequencer);
                sequencer.loop = false;
            }

            // Move to the next sequencer index
            currentSequencerIndex = (currentSequencerIndex + 1) % targetSequencers.Count;
            HelmSequencer nextSequencer = targetSequencers[currentSequencerIndex];

            // Prepare and start the next sequencer
            PrepareSequencerForNextCycle(nextSequencer);

            // Update the BoardManager with the notes of the queued-up sequencer
            if (boardManager != null)
            {
                List<AudioHelm.Note> notes = new List<AudioHelm.Note>(nextSequencer.GetAllNotes());
                boardManager.ResetBoard();
                boardManager.UpdateBoardWithNotes(notes); // Update the board with the new notes
                boardManager.HighlightCellOnStep(nextSequencer.currentIndex); // Highlight the cell

                UpdatePatternDisplay();
            }

            // Calculate the duration of the loop based on BPM and 16-step cycle
            if (clock != null)
            {
                float bpm = clock.bpm;
                loopDuration = (960f / bpm) / 4f; // 16 steps per loop cycle
            }
            else
            {
                Debug.LogError("AudioHelmClock not assigned.");
                yield break;
            }

            // Start the next sequencer
            StartSequencer(nextSequencer);

            // Enable AudioSource component
            nextSequencer.GetComponent<AudioSource>().volume = 1;

            // Set loop to true only for the currently playing sequencer
            nextSequencer.loop = true;

            // Wait until the loop ends
            yield return new WaitUntil(() => boardManager.highlightedCellIndex == 15);

            // Wait a little longer to ensure the sequencer has time to finish the step
            yield return new WaitForSeconds(0.1f);

            // Update the pattern display
            UpdatePatternDisplay();

            Debug.Log("Started next sequencer.");

            // Small delay to ensure highlight update before next loop iteration
            yield return new WaitForSeconds(0.1f);
        }

        // Resume the clock after stopping the loop
        if (isClockPaused)
        {
            ResumeClock();
        }
    }

    void PrepareSequencerForNextCycle(HelmSequencer sequencer)
    {
        if (sequencer != null)
        {
            sequencer.gameObject.SetActive(true); // Ensure the sequencer is active
            Debug.Log($"Prepared sequencer for next cycle: {sequencer.name}");
        }
    }

    void StopSequencer(HelmSequencer sequencer)
    {
        if (sequencer != null)
        {
            sequencer.loop = false; // Stop the sequencer
            Debug.Log($"Stopped sequencer: {sequencer.name}");
        }
    }

    void StartSequencer(HelmSequencer sequencer)
    {
        if (sequencer != null)
        {
            sequencer.loop = true; // Start with loop set to true
            Debug.Log($"Started sequencer: {sequencer.name}");
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

        // Set loop to false when creating
        newSequencer.loop = false;
        newSequencer.GetComponent<AudioSource>().volume = 0;

        // Transfer notes from the source sequencer to the new sequencer
        TransferNotes(sourceSequencer, newSequencer);

        // Add the new sequencer to the list
        targetSequencers.Add(newSequencer);

        // Set the flag to indicate patterns have been created
        patternsCreated = true;

        // Update pattern display
        UpdatePatternDisplay();

        Debug.Log("Pattern created and transferred.");
    }

    void TransferNotes(HelmSequencer source, HelmSequencer target)
    {
        if (source != null && target != null)
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
    }

    void StartPlayingPatterns()
    {
        if (clock == null)
        {
            Debug.LogError("AudioHelmClock not assigned.");
            return;
        }

        clock.Reset();

        if (!patternsCreated)
        {
            Debug.LogError("No patterns created to play.");
            return;
        }   

        // Ensure the source sequencer is stopped
        sourceSequencer.AllNotesOff();
        sourceSequencer.loop = false;

        isPlaying = true;
        currentSequencerIndex = -1;

        // Pause the clock before starting playback
        ResumeClock();

        // Start playing the first sequencer
        StartCoroutine(SmoothTransitionToNextSequencer());

        UpdatePatternDisplay();
        
        Debug.Log("Started playing patterns.");
    }

    void StopCreatedPatterns()
    {
        if (!patternsCreated)
        {
            Debug.LogWarning("No patterns created to stop.");
            return;
        }

        // Stop playback of all sequencers
        foreach (var sequencer in targetSequencers)
        {
            StopSequencer(sequencer);
        }

        // Resume the clock when stopping playback
        PauseClock();

        Debug.Log("Stopped all patterns.");
    }

    void RemovePattern()
    {
        if (!patternsCreated || targetSequencers.Count == 0)
        {
            Debug.LogWarning("No patterns created to remove.");
            return;
        }

        // Remove the last sequencer in the list
        int indexToRemove = targetSequencers.Count - 1;
        HelmSequencer sequencerToRemove = targetSequencers[indexToRemove];

        if (sequencerToRemove != null)
        {
            // Stop the sequencer if it is playing
            StopSequencer(sequencerToRemove);

            // Remove the sequencer from the list and destroy its GameObject
            targetSequencers.RemoveAt(indexToRemove);
            Destroy(sequencerToRemove.gameObject);

            // Update BoardManager if necessary
            if (boardManager != null)
            {
                boardManager.ResetBoard();
                if (targetSequencers.Count > 0)
                {
                    HelmSequencer remainingSequencer = targetSequencers[Mathf.Clamp(currentSequencerIndex, 0, targetSequencers.Count - 1)];
                    List<AudioHelm.Note> notes = new List<AudioHelm.Note>(remainingSequencer.GetAllNotes());
                    boardManager.UpdateBoardWithNotes(notes);
                }
            }

            // Update pattern display
            // Adjust currentSequencerIndex if necessary
            if (targetSequencers.Count == 0)
            {
                currentSequencerIndex = 0;
            }
            else if (currentSequencerIndex >= targetSequencers.Count)
            {
                currentSequencerIndex = targetSequencers.Count - 1;
            }

            UpdatePatternDisplay();

            Debug.Log("Pattern removed.");
        }
    }

    void UpdatePatternDisplay()
    {
        if (patternDisplayText != null)
        {
            int totalPatterns = targetSequencers.Count;
            int displayIndex = isPlaying && totalPatterns > 0 ? (currentSequencerIndex + 1) : 0; // Display index should be 1-based

            patternDisplayText.text = $"{displayIndex}/{totalPatterns}";
        }
    }



    // Dummy methods for clock management
    void PauseClock()
    {
        if (clock != null)
        {
            isClockPaused = true;
            Debug.Log("Clock paused.");
            clock.pause = true;
        }
    }

    void ResumeClock()
    {
        if (clock != null)
        {
            isClockPaused = false;
            Debug.Log("Clock resumed.");
            clock.pause = false;
        }
    }
}
