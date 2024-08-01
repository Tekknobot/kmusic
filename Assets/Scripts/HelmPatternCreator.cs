using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using AudioHelm;

public class HelmPatternCreator : MonoBehaviour
{
    public HelmSequencer sourceSequencer;
    public GameObject sequencerPrefab;
    public Button createPatternButton;
    public Button playPatternsButton;
    public Button stopPatternsButton;
    public Button removePatternButton;
    public TextMeshProUGUI patternDisplayText;
    public BoardManager boardManager;
    public AudioHelmClock clock;

    public List<HelmSequencer> targetSequencers = new List<HelmSequencer>();
    private bool patternsCreated = false;
    private bool isPlaying = false;
    private int currentSequencerIndex = -1;
    private bool isClockPaused = false;
    private bool hasStartedPlayback = false; // Flag to check if playback has started
    private bool initialDelayApplied = false; // Flag for initial delay

    void Start()
    {
        if (createPatternButton != null) createPatternButton.onClick.AddListener(CreateAndTransferPattern);
        else Debug.LogError("Create Pattern Button not assigned.");

        if (playPatternsButton != null) playPatternsButton.onClick.AddListener(StartPlayingPatterns);
        else Debug.LogError("Play Patterns Button not assigned.");

        if (stopPatternsButton != null) stopPatternsButton.onClick.AddListener(StopCreatedPatterns);
        else Debug.LogError("Stop Patterns Button not assigned.");

        if (removePatternButton != null) removePatternButton.onClick.AddListener(RemovePattern);
        else Debug.LogError("Remove Pattern Button not assigned.");

        UpdatePatternDisplay();
    }

    IEnumerator SmoothTransitionToNextSequencer()
    {
        if (targetSequencers.Count == 0)
        {
            Debug.LogError("No target sequencers available for transition.");
            yield break;
        }

        // Ensure the clock is running
        if (clock != null)
        {
            clock.Reset();
            ResumeClock();
        }
        else
        {
            Debug.LogError("AudioHelmClock not assigned.");
            yield break;
        }

        while (isPlaying)
        {
            // Stop and mute all sequencers
            foreach (var sequencer in targetSequencers)
            {
                if (sequencer != null && sequencer.gameObject.activeSelf)
                {
                    StopSequencer(sequencer);
                }
            }

            // Move to the next sequencer index
            currentSequencerIndex = (currentSequencerIndex + 1) % targetSequencers.Count;
            HelmSequencer nextSequencer = targetSequencers[currentSequencerIndex];

            // Prepare and start the next sequencer
            PrepareSequencerForNextCycle(nextSequencer);

            // Update the BoardManager with the notes of the next sequencer
            if (boardManager != null)
            {
                List<AudioHelm.Note> notes = new List<AudioHelm.Note>(nextSequencer.GetAllNotes());
                boardManager.ResetBoard();
                boardManager.UpdateBoardWithNotes(notes);
                boardManager.HighlightCellOnStep(nextSequencer.currentIndex);
            }

            // Calculate the duration of one bar based on BPM
            float secondsPerBeat = 60f / clock.bpm;
            float oneBarDuration = secondsPerBeat * 4; // Assuming 4 beats per bar

            // Start the next sequencer and set its volume to 1
            StartSequencer(nextSequencer);

            if (initialDelayApplied)
            {
                // Wait for the duration of one loop cycle (4 beats) after initial delay
                yield return new WaitForSeconds(oneBarDuration);
            }
            else
            {
                // Wait for one bar before starting playback
                yield return new WaitForSeconds(oneBarDuration);

                // Set flag to indicate initial delay has been applied
                initialDelayApplied = true;
            }

            // Stop the current sequencer after the loop duration
            StopSequencer(nextSequencer);

            // Update the pattern display
            UpdatePatternDisplay();

            Debug.Log("Started next sequencer.");

            // Small delay to ensure highlight update before next loop iteration
            yield return new WaitForSeconds(0.1f);
        }

        // Resume the clock after stopping the loop
        if (isClockPaused) ResumeClock();
    }

    void PrepareSequencerForNextCycle(HelmSequencer sequencer)
    {
        if (sequencer != null)
        {
            sequencer.gameObject.SetActive(true);
            Debug.Log($"Prepared sequencer for next cycle: {sequencer.name}");
        }
    }

    void StopSequencer(HelmSequencer sequencer)
    {
        if (sequencer != null)
        {
            sequencer.loop = false;
            sequencer.AllNotesOff();

            // Set volume to 0 when stopping
            AudioSource audioSource = sequencer.gameObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.volume = 0;
            }

            Debug.Log($"Stopped sequencer: {sequencer.name}");
        }
    }

    void StartSequencer(HelmSequencer sequencer)
    {
        if (sequencer != null)
        {
            sequencer.loop = true;

            // Set volume to 1 when starting
            AudioSource audioSource = sequencer.gameObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.volume = 1;
            }

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

        GameObject newSequencerObj = Instantiate(sequencerPrefab, transform);
        HelmSequencer newSequencer = newSequencerObj.GetComponent<HelmSequencer>();

        if (newSequencer == null)
        {
            Debug.LogError("New sequencer prefab does not have a HelmSequencer component.");
            return;
        }

        newSequencer.name = "Helm Pattern " + (targetSequencers.Count + 1);
        newSequencer.loop = false;
        newSequencer.gameObject.GetComponent<AudioSource>().volume = 0;

        TransferNotes(sourceSequencer, newSequencer);

        targetSequencers.Add(newSequencer);

        patternsCreated = true;
        currentSequencerIndex = 0;

        UpdatePatternDisplay();

        Debug.Log("Pattern created and added to the list.");
    }

    void TransferNotes(HelmSequencer source, HelmSequencer target)
    {
        if (source != null && target != null)
        {
            target.Clear();

            foreach (AudioHelm.Note note in source.GetAllNotes())
            {
                target.AddNote(note.note, note.start, note.end, note.velocity);
                Debug.Log($"Transferred note {note.note} to new sequencer.");
            }
        }
    }

    public void StartPlayingPatterns()
    {
        if (clock == null)
        {
            Debug.LogError("AudioHelmClock not assigned.");
            return;
        }

        if (!patternsCreated)
        {
            Debug.LogError("No patterns created to play.");
            return;
        }

        if (hasStartedPlayback)
        {
            Debug.LogWarning("Playback has already started.");
            return;
        }

        // Stop and mute the source sequencer
        sourceSequencer.AllNotesOff();
        sourceSequencer.loop = false;
        AudioSource sourceAudioSource = sourceSequencer.gameObject.GetComponent<AudioSource>();
        if (sourceAudioSource != null)
        {
            sourceAudioSource.volume = 0;
        }

        // Initialize playback state
        isPlaying = true;
        hasStartedPlayback = true; // Set flag to true
        initialDelayApplied = false; // Reset the initial delay flag

        // Reset and resume the clock
        clock.Reset();
        ResumeClock();

        // Stop all sequencers initially and set their volumes to 0
        foreach (var sequencer in targetSequencers)
        {
            StopSequencer(sequencer);
        }

        // Start a coroutine to handle the initial delay and playback
        StartCoroutine(StartPlaybackCoroutine());

        Debug.Log("Started playing patterns.");

        // Ensure the PAUSE toggle is on
        GameObject.Find("PAUSE").GetComponent<Toggle>().isOn = true;
    }

    IEnumerator StartPlaybackCoroutine()
    {
        boardManager.ResetBoard();

        // Calculate the duration of one bar based on BPM
        float secondsPerBeat = 60f / clock.bpm;
        float oneBarDuration = secondsPerBeat * 4; // Assuming 4 beats per bar

        // Wait for one bar before starting playback
        yield return new WaitForSeconds(oneBarDuration);

        // Prepare and start the first sequencer
        if (targetSequencers.Count > 0)
        {
            currentSequencerIndex = 0;
            HelmSequencer firstSequencer = targetSequencers[currentSequencerIndex];
            PrepareSequencerForNextCycle(firstSequencer);
            StartSequencer(firstSequencer); // Start only the first sequencer

            // Update the BoardManager with the notes of the first sequencer
            if (boardManager != null)
            {
                List<AudioHelm.Note> notes = new List<AudioHelm.Note>(firstSequencer.GetAllNotes());
                boardManager.ResetBoard();
                boardManager.UpdateBoardWithNotes(notes);
                boardManager.HighlightCellOnStep(firstSequencer.currentIndex);
            }

            // Update the pattern display to show the first sequencer
            UpdatePatternDisplay();
        }
        else
        {
            Debug.LogError("No target sequencers available.");
            isPlaying = false;
            yield break;
        }

        // Start the coroutine to transition between sequencers
        StartCoroutine(SmoothTransitionToNextSequencer());
    }

    public void StopCreatedPatterns()
    {
        if (clock != null)
        {
            clock.pause = true;
            isClockPaused = true;
        }
        isPlaying = false;
        hasStartedPlayback = false; // Reset flag when stopping

        // Stop and mute all sequencers
        foreach (var sequencer in targetSequencers)
        {
            StopSequencer(sequencer);
        }

        Debug.Log("Stopped all patterns.");
    }

    void RemovePattern()
    {
        if (targetSequencers.Count > 0)
        {
            HelmSequencer lastSequencer = targetSequencers[targetSequencers.Count - 1];
            Destroy(lastSequencer.gameObject);
            targetSequencers.RemoveAt(targetSequencers.Count - 1);

            if (targetSequencers.Count > 0)
            {
                currentSequencerIndex = Mathf.Min(currentSequencerIndex, targetSequencers.Count - 1);
                HelmSequencer activeSequencer = targetSequencers[currentSequencerIndex];
                PrepareSequencerForNextCycle(activeSequencer);
                StartSequencer(activeSequencer);
            }
            else
            {
                currentSequencerIndex = -1;
                isPlaying = false;
            }

            // Update pattern display
            UpdatePatternDisplay();

            Debug.Log("Removed last pattern.");
        }
        else
        {
            Debug.LogWarning("No patterns to remove.");
        }
    }

    void UpdatePatternDisplay()
    {
        if (patternDisplayText != null)
        {
            int totalPatterns = targetSequencers.Count;
            int displayIndex = (totalPatterns > 0) ? (currentSequencerIndex + 1) : 0; // Display index should be 1-based
            patternDisplayText.text = $"{displayIndex}/{totalPatterns}";
        }
    }

    void ResumeClock()
    {
        if (clock != null)
        {
            clock.pause = false;
            isClockPaused = false;
            Debug.Log("Clock resumed.");
        }
        else
        {
            Debug.LogError("AudioHelmClock not assigned.");
        }
    }
}
