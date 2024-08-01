using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioHelm;

public class PatternManager : MonoBehaviour
{
    public HelmSequencer sourceSequencer;
    public GameObject sequencerPrefab;
    public AudioHelmClock clock;
    public BoardManager boardManager;  // Add reference to BoardManager

    private List<HelmSequencer> patterns = new List<HelmSequencer>();
    private int currentPatternIndex = -1;
    private bool isPlaying = false;

    public int PatternsCount => patterns.Count;
    public int CurrentPatternIndex => currentPatternIndex;

    void Start()
    {
        // Ensure clock is assigned
        if (clock == null)
        {
            Debug.LogError("AudioHelmClock not assigned.");
            return;
        }

        // Pause the clock initially
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

        newSequencer.loop = false;
        newSequencer.GetComponent<AudioSource>().volume = 0;

        TransferNotes(sourceSequencer, newSequencer);
        patterns.Add(newSequencer);

        Debug.Log("Pattern created and added to the list.");
    }

    private void TransferNotes(HelmSequencer source, HelmSequencer target)
    {
        target.Clear();

        foreach (AudioHelm.Note note in source.GetAllNotes())
        {
            target.AddNote(note.note, note.start, note.end, note.velocity);
        }
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

        StartCoroutine(PlayPatternsCoroutine());
    }

    private IEnumerator PlayPatternsCoroutine()
    {
        while (isPlaying)
        {
            currentPatternIndex = (currentPatternIndex + 1) % patterns.Count;
            HelmSequencer currentPattern = patterns[currentPatternIndex];

            StartPattern(currentPattern);
            UpdateBoardManager(currentPattern);  // Update board manager with the current pattern notes

            float secondsPerBeat = 60f / clock.bpm;
            float oneBarDuration = secondsPerBeat * 4;

            yield return new WaitForSeconds(oneBarDuration);

            StopPattern(currentPattern);
        }
    }

    private void StartPattern(HelmSequencer pattern)
    {
        pattern.loop = true;
        pattern.GetComponent<AudioSource>().volume = 1;
        Debug.Log($"Started pattern: {pattern.name}");
    }

    private void StopPattern(HelmSequencer pattern)
    {
        pattern.loop = false;
        pattern.GetComponent<AudioSource>().volume = 0;
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
        }
        else
        {
            Debug.LogWarning("No patterns to remove.");
        }
    }

    private void UpdateBoardManager(HelmSequencer currentPattern)
    {
        if (boardManager != null)
        {
            List<AudioHelm.Note> notes = new List<AudioHelm.Note>(currentPattern.GetAllNotes());
            boardManager.ResetBoard();
            boardManager.UpdateBoardWithNotes(notes);
            boardManager.HighlightCellOnStep(currentPattern.currentIndex);
        }
        else
        {
            Debug.LogError("BoardManager not assigned.");
        }
    }
}
