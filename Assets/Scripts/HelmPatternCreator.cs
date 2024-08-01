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

    private List<HelmSequencer> targetSequencers = new List<HelmSequencer>();
    private bool patternsCreated = false;
    private bool isPlaying = false;
    private int currentSequencerIndex = -1;
    private bool isClockPaused = false;
    private float loopDuration = 0f;

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

        // Ensure all other sequencers have loop set to false
        foreach (var sequencer in targetSequencers)
        {
            if (sequencer != null && targetSequencers.IndexOf(sequencer) != currentSequencerIndex)
            {
                sequencer.loop = false;
                sequencer.AllNotesOff();
            }
        }

        while (isPlaying)
        {
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
                boardManager.UpdateBoardWithNotes(notes);
                boardManager.HighlightCellOnStep(nextSequencer.currentIndex);
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

            // Wait until the loop ends
            yield return new WaitUntil(() => boardManager.highlightedCellIndex == 15);
            
            int stepsPerBeat = 4; // Adjust this based on how many steps per beat

            // Calculate seconds per beat
            float secondsPerBeat = 60f / clock.bpm;

            // Calculate seconds per step
            float secondsPerStep = secondsPerBeat / stepsPerBeat;

            // Wait a little longer to ensure the sequencer has time to finish the step
            yield return new WaitForSeconds(secondsPerStep + 0.1f);

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
            Debug.Log($"Stopped sequencer: {sequencer.name}");
        }
    }

    void StartSequencer(HelmSequencer sequencer)
    {
        if (sequencer != null)
        {
            sequencer.loop = true;
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

        TransferNotes(sourceSequencer, newSequencer);

        targetSequencers.Insert(targetSequencers.Count, newSequencer);

        patternsCreated = true;
        currentSequencerIndex = 0;

        if (patternDisplayText != null)
        {
            int totalPatterns = targetSequencers.Count;
            int displayIndex = (totalPatterns > 0) ? (currentSequencerIndex + 1) : 0; // Display index should be 1-based
            patternDisplayText.text = $"{totalPatterns}/{totalPatterns}";
        }

        Debug.Log("Pattern created and added to the top of the list.");
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

        sourceSequencer.AllNotesOff();
        sourceSequencer.loop = false;
        sourceSequencer.gameObject.GetComponent<AudioSource>().volume = 0;

        isPlaying = true;

        clock.Reset();
        ResumeClock();

        currentSequencerIndex = -1;

        StartCoroutine(SmoothTransitionToNextSequencer());

        UpdatePatternDisplay();

        Debug.Log("Started playing patterns.");

        GameObject.Find("PAUSE").GetComponent<Toggle>().isOn = true;
    }

    public void StopCreatedPatterns()
    {
        if (!patternsCreated)
        {
            Debug.LogWarning("No patterns created to stop.");
            return;
        }

        foreach (var sequencer in targetSequencers)
        {
            StopSequencer(sequencer);
        }

        clock.Reset();

        currentSequencerIndex = -1;

        UpdatePatternDisplay();

        isPlaying = false;

        Debug.Log("Stopped all patterns.");
    }

    void RemovePattern()
    {
        if (!patternsCreated || targetSequencers.Count == 0)
        {
            Debug.LogWarning("No patterns created to remove.");
            return;
        }

        int indexToRemove = targetSequencers.Count - 1;
        HelmSequencer sequencerToRemove = targetSequencers[indexToRemove];

        if (sequencerToRemove != null)
        {
            StopSequencer(sequencerToRemove);

            targetSequencers.RemoveAt(indexToRemove);
            Destroy(sequencerToRemove.gameObject);

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

            if (targetSequencers.Count == 0)
            {
                currentSequencerIndex = -1;
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
            int displayIndex = (totalPatterns > 0) ? (currentSequencerIndex + 1) : 0; // Display index should be 1-based
            patternDisplayText.text = $"{displayIndex}/{totalPatterns}";
        }
    }

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
