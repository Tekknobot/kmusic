using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioHelm;
using System;

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
    private int currentStepIndex = 0; // Track the current step index

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
        LoadPatterns();
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

        SavePatterns();
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

        if (clock == null)
        {
            Debug.LogError("Clock not assigned.");
            return;
        }

        isPlaying = true;
        clock.Reset();
        clock.pause = false;

        if (sourceSequencer != null)
        {
            sourceSequencer.GetComponent<HelmSequencer>().enabled = false;
        }

        StopAllCoroutines(); // Stop any previous coroutines to avoid conflicts
        StartCoroutine(PlayPatternsCoroutine());
    }

    private IEnumerator PlayPatternsCoroutine()
    {
        Debug.Log("Coroutine started.");

        while (isPlaying)
        {
            // Calculate the duration of one bar based on BPM
            float secondsPerBeat = 60f / clock.bpm;
            float oneBarDuration = secondsPerBeat * 4; // 4 beats per bar
            float quarterBarDuration = secondsPerBeat; // Duration of one beat

            float stepDuration = secondsPerBeat / 4; // Duration of one step

            // Move to the next pattern
            currentPatternIndex = (currentPatternIndex + 1) % patterns.Count;
            HelmSequencer currentPattern = patterns[currentPatternIndex];

            Debug.Log($"Playing pattern index: {currentPatternIndex}");

            // Stop all patterns
            foreach (var pattern in patterns)
            {
                StopPattern(pattern);
            }

            // Enable and play the current pattern
            currentPattern.enabled = true;
            UpdateBoardManager(currentPattern);
            UpdatePatternDisplay(); // Update UI
            Debug.Log($"Started pattern: {currentPattern.name}");

            // Wait for the duration of one bar
            yield return new WaitUntil(() => boardManager.GetHighlightedCellIndex() == 15);

            yield return new WaitForSeconds(stepDuration); // Adjust if needed
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

    public void RemovePattern(int index)
    {
        if (index >= 0 && index < patterns.Count)
        {
            HelmSequencer patternToRemove = patterns[index];
            Destroy(patternToRemove.gameObject);
            patterns.RemoveAt(index);

            Debug.Log($"Removed pattern at index: {index}");

            UpdateBoardManager();
            UpdatePatternDisplay(); // Update UI

            SavePatterns(); // Save the updated list of patterns
        }
        else
        {
            Debug.LogWarning("Invalid index. Cannot remove pattern.");
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
                boardManager.HighlightCellOnStep(currentStepIndex);
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

    public void SavePatterns()
    {
        // Convert patterns to PatternData
        List<PatternData> patternDataList = new List<PatternData>();
        foreach (var pattern in patterns)
        {
            PatternData patternData = ConvertSequencerToPatternData(pattern);
            patternDataList.Add(patternData);
        }

        // Save the list of PatternData to file
        DataManager.SavePatternsToFile(patternDataList);
        Debug.Log("Patterns saved to file.");
    }

    private PatternData ConvertSequencerToPatternData(HelmSequencer sequencer)
    {
        PatternData patternData = new PatternData
        {
            Name = sequencer.name // Or another identifier if needed
        };

        foreach (AudioHelm.Note note in sequencer.GetAllNotes())
        {
            // Convert each note to TileData or appropriate format
            TileData tileData = new TileData
            {
                SpriteName = note.note.ToString(), // Convert note to a string or use another method if needed
                Step = note.start // Use start or another property as needed
            };
            patternData.Tiles.Add(tileData);
        }

        return patternData;
    }

    public void LoadPatterns()
    {
        // Load the list of PatternData from file
        List<PatternData> patternDataList = DataManager.LoadPatternsFromFile();
        patterns.Clear(); // Clear existing patterns

        foreach (var patternData in patternDataList)
        {
            HelmSequencer newSequencer = Instantiate(sequencerPrefab).GetComponent<HelmSequencer>();
            if (newSequencer != null)
            {
                newSequencer.enabled = false;
                PopulateSequencerFromPatternData(newSequencer, patternData);
                patterns.Add(newSequencer);
            }
        }

        Debug.Log("Patterns loaded from file.");
        UpdatePatternDisplay(); // Update UI to reflect loaded patterns
    }

    private void PopulateSequencerFromPatternData(HelmSequencer sequencer, PatternData patternData)
    {
        foreach (var tile in patternData.Tiles)
        {
            // Convert TileData back to AudioHelm.Note
            // Note: Adjust parsing based on how you saved notes
            int noteValue;
            if (int.TryParse(tile.SpriteName, out noteValue)) // Example conversion
            {
                AudioHelm.Note note = new AudioHelm.Note
                {
                    note = noteValue, // Use integer value for the note
                    start = tile.Step, // Use start value
                    end = tile.Step + 1, // Example end value, adjust as needed
                    velocity = 1.0f // Example default value, adjust as needed
                };
                sequencer.AddNote(note.note, note.start, note.end, note.velocity);
            }
            else
            {
                Debug.LogError($"Failed to parse note value from {tile.SpriteName}");
            }
        }
    }
}
