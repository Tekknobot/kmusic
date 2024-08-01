using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioHelm;

public class PatternManager : MonoBehaviour
{
    public HelmSequencer sourceSequencer;
    public GameObject sequencerPrefab;
    public AudioHelmClock clock;
    public BoardManager boardManager;
    public PatternUIManager patternUIManager; // Reference to the UI manager

    private List<HelmSequencer> patterns = new List<HelmSequencer>();
    private int currentPatternIndex = -1;
    private bool isPlaying = false;

    public int PatternsCount => patterns.Count;
    public int CurrentPatternIndex => currentPatternIndex;

    void Start()
    {
        if (clock == null)
        {
            Debug.LogError("AudioHelmClock not assigned.");
            return;
        }

        clock.pause = true;
    }

    public void CreatePattern()
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

        newSequencer.enabled = false;

        TransferNotes(sourceSequencer, newSequencer);
        patterns.Add(newSequencer);

        Debug.Log($"Pattern created and added to the list. Total patterns: {patterns.Count}");

        UpdateBoardManager();
        UpdatePatternDisplay(); // Update UI
    }

    private void TransferNotes(HelmSequencer source, HelmSequencer target)
    {
        target.Clear();
        foreach (AudioHelm.Note note in source.GetAllNotes())
        {
            target.AddNote(note.note, note.start, note.end, note.velocity);
        }
        Debug.Log("Notes transferred from source to target sequencer.");
    }

    public void StartPatterns()
    {
        if (patterns.Count == 0)
        {
            Debug.LogError("No patterns created to play.");
            return;
        }

        isPlaying = true;
        clock.Reset();
        clock.pause = false;

        if (sourceSequencer != null)
        {
            sourceSequencer.GetComponent<HelmSequencer>().enabled = false;
        }

        StartCoroutine(PlayPatternsCoroutine());
    }

    private IEnumerator PlayPatternsCoroutine()
    {
        while (isPlaying)
        {
            currentPatternIndex = (currentPatternIndex + 1) % patterns.Count;
            HelmSequencer currentPattern = patterns[currentPatternIndex];

            Debug.Log($"Playing pattern index: {currentPatternIndex}");

            foreach (var pattern in patterns)
            {
                StopPattern(pattern);
            }

            currentPattern.enabled = true;
            UpdateBoardManager(currentPattern);
            UpdatePatternDisplay(); // Update UI
            Debug.Log($"Started pattern: {currentPattern.name}");

            float secondsPerBeat = 60f / clock.bpm;
            float oneBarDuration = secondsPerBeat * 4;

            yield return new WaitForSeconds(oneBarDuration);
        }
    }

    private void StopPattern(HelmSequencer pattern)
    {
        pattern.enabled = false;
        Debug.Log($"Stopped pattern: {pattern.name}");
    }

    public void StopPatterns()
    {
        isPlaying = false;
        clock.pause = true;

        foreach (var pattern in patterns)
        {
            StopPattern(pattern);
        }

        if (sourceSequencer != null)
        {
            sourceSequencer.GetComponent<HelmSequencer>().enabled = true;
        }

        UpdateBoardManager();
        UpdatePatternDisplay(); // Update UI
        Debug.Log("Stopped all patterns.");
    }

    public void RemoveLastPattern()
    {
        if (patterns.Count > 0)
        {
            HelmSequencer lastPattern = patterns[patterns.Count - 1];
            Destroy(lastPattern.gameObject);
            patterns.RemoveAt(patterns.Count - 1);

            Debug.Log("Removed last pattern.");

            UpdateBoardManager();
            UpdatePatternDisplay(); // Update UI
        }
        else
        {
            Debug.LogWarning("No patterns to remove.");
        }
    }

    private void UpdateBoardManager(HelmSequencer currentPattern = null)
    {
        if (boardManager != null)
        {
            if (currentPattern != null)
            {
                List<AudioHelm.Note> notes = new List<AudioHelm.Note>(currentPattern.GetAllNotes());
                boardManager.ResetBoard();
                boardManager.UpdateBoardWithNotes(notes);
                boardManager.HighlightCellOnStep(currentPattern.currentIndex);
            }
            else
            {
                boardManager.ResetBoard();
            }
        }
        else
        {
            Debug.LogError("BoardManager not assigned.");
        }
    }

    private void UpdatePatternDisplay()
    {
        if (patternUIManager != null)
        {
            patternUIManager.UpdatePatternDisplay();
        }
        else
        {
            Debug.LogError("PatternUIManager not assigned.");
        }
    }
}
