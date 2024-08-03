using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO; // Add this for file operations
using AudioHelm;
using System;
using System.Linq;
using TMPro;

public class PatternManager : MonoBehaviour
{
    public static PatternManager Instance { get; private set; }

    public HelmSequencer sourceSequencer;
    public GameObject sequencerPrefab;
    public AudioHelmClock clock;
    public BoardManager boardManager;
    public PatternUIManager patternUIManager; // Reference to the UI manager

    public List<HelmSequencer> patterns = new List<HelmSequencer>();
    public int currentPatternIndex = -1;
    public bool isPlaying = false;
    private int currentStepIndex = 0; // Track the current step index

    public int PatternsCount => patterns.Count;
    public int CurrentPatternIndex => currentPatternIndex;
    public static string LastProjectFilename { get; private set; }
    private static string lastAccessedFile = null;
    public TextMeshProUGUI projectFileText; // Reference to the TextMeshPro component
    private void Awake()
    {
        // Ensure this is the only instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: Keep instance between scenes
    }

    private void Start()
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
        if (sequencerPrefab == null)
        {
            Debug.LogError("Sequencer Prefab not assigned.");
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

        if (patterns.Count > 0)
        {
            HelmSequencer lastSequencer = patterns[patterns.Count - 1];
            TransferNotes(lastSequencer, newSequencer);
        }
        else if (sourceSequencer != null)
        {
            TransferNotes(sourceSequencer, newSequencer);
        }
        else
        {
            Debug.LogWarning("No existing sequencer to copy notes from.");
        }

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
            float secondsPerBeat = 60f / clock.bpm;
            float stepDuration = secondsPerBeat / 4; // Duration of one step

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
        List<PatternData> patternDataList = new List<PatternData>();
        foreach (var pattern in patterns)
        {
            PatternData patternData = ConvertSequencerToPatternData(pattern);
            patternDataList.Add(patternData);
        }

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
            TileData tileData = new TileData
            {
                SpriteName = note.note.ToString(),
                Step = note.start
            };
            patternData.Tiles.Add(tileData);
        }

        return patternData;
    }

    public void LoadPatterns()
    {
        List<PatternData> patternDataList = DataManager.LoadPatternsFromFile();
        patterns.Clear();

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
            int noteValue;
            if (int.TryParse(tile.SpriteName, out noteValue))
            {
                AudioHelm.Note note = new AudioHelm.Note
                {
                    note = noteValue,
                    start = tile.Step,
                    end = tile.Step + 1,
                    velocity = 1.0f
                };
                sequencer.AddNote(note.note, note.start, note.end, note.velocity);
            }
            else
            {
                Debug.LogError($"Failed to parse note value from {tile.SpriteName}");
            }
        }
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

    public HelmSequencer GetActiveSequencer()
    {
        if (currentPatternIndex >= 0 && currentPatternIndex < patterns.Count)
        {
            return patterns[currentPatternIndex];
        }
        else
        {
            Debug.LogWarning("No active pattern.");
            return null;
        }
    }    

    public void SaveProject(string filename)
    {
        ProjectData projectData = new ProjectData
        {
            Patterns = new List<PatternData>(),
        };

        foreach (var pattern in patterns)
        {
            PatternData patternData = ConvertSequencerToPatternData(pattern);
            projectData.Patterns.Add(patternData);
        }

        string json = JsonUtility.ToJson(projectData, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, filename), json);
        LastProjectFilename = filename; // Store the filename
        Debug.Log($"Project saved to file: {filename}");
    }


    public void LoadProject(string filename)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                ProjectData projectData = JsonUtility.FromJson<ProjectData>(json);

                // Clear current patterns
                RemoveAllPatterns();

                // Load patterns from project data
                if (projectData != null && projectData.Patterns != null)
                {
                    foreach (var patternData in projectData.Patterns)
                    {
                        HelmSequencer newSequencer = Instantiate(sequencerPrefab).GetComponent<HelmSequencer>();
                        if (newSequencer != null)
                        {
                            newSequencer.enabled = false;
                            PopulateSequencerFromPatternData(newSequencer, patternData);
                            patterns.Add(newSequencer);
                        }
                    }

                    Debug.Log($"Project loaded from file: {filename}");
                }
                else
                {
                    Debug.LogError("Project data is null or empty.");
                }

                UpdatePatternDisplay(); // Update UI to reflect loaded patterns
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading project: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"File not found: {filename}");
        }
    }

    private void RemoveAllPatterns()
    {
        foreach (var pattern in patterns)
        {
            Destroy(pattern.gameObject);
        }
        patterns.Clear();

        Debug.Log("All patterns removed.");
    }


    public string GenerateUniqueFilename()
    {
        // Define the filename pattern to search for
        string pattern = "Project_*.json";
        string[] existingFiles = Directory.GetFiles(Application.persistentDataPath, pattern);

        int highestNumber = 0;

        // Iterate through existing files to find the highest number
        foreach (string file in existingFiles)
        {
            // Extract the number from the filename
            string filename = Path.GetFileNameWithoutExtension(file);
            string numberPart = filename.Substring("Project_".Length);

            // Try to parse the number
            if (int.TryParse(numberPart, out int number))
            {
                if (number > highestNumber)
                {
                    highestNumber = number;
                }
            }
        }

        // Increment the highest number by 1 for the new filename
        int newNumber = highestNumber + 1;
        return $"Project_{newNumber}.json";
    }


    public string GetNextProjectFile()
    {
        // Get all files starting with "Project" in the persistent data path
        string[] projectFiles = Directory.GetFiles(Application.persistentDataPath, "Project*.json");

        if (projectFiles.Length == 0)
        {
            Debug.LogWarning("No project files found.");
            return null;
        }

        // Sort files by creation time to ensure consistent ordering
        var sortedFiles = projectFiles
            .Select(file => new FileInfo(file))
            .OrderBy(fileInfo => fileInfo.CreationTime)
            .Select(fileInfo => fileInfo.FullName)
            .ToArray();

        // Find the index of the last accessed file
        int startIndex = 0;
        if (!string.IsNullOrEmpty(lastAccessedFile))
        {
            startIndex = Array.IndexOf(sortedFiles, lastAccessedFile);
            startIndex = (startIndex + 1) % sortedFiles.Length; // Move to next file
        }

        // Get the next file
        string nextFile = sortedFiles[startIndex];
        lastAccessedFile = nextFile; // Update the last accessed file
        LastProjectFilename = Path.GetFileName(nextFile); // Update the last project filename
        
        UpdateProjectFileText(); // Update the UI text

        Debug.Log($"Next project file selected: {LastProjectFilename}");
        return LastProjectFilename;
    }

    public void LoadNextProject()
    {
        string nextFilename = GetNextProjectFile();
        if (!string.IsNullOrEmpty(nextFilename))
        {
            LoadProject(nextFilename);
            UpdatePatternDisplay(); // Update UI
        }
    }  

    private void UpdateProjectFileText()
    {
        if (projectFileText != null)
        {
            projectFileText.text = $"{LastProjectFilename}";
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component is not assigned.");
        }
    }      

    public void ClearCurrentPattern()
    {
        if (currentPatternIndex >= 0 && currentPatternIndex < patterns.Count)
        {
            // Clear the current pattern's sequencer
            HelmSequencer currentPattern = patterns[currentPatternIndex];
            if (currentPattern != null)
            {
                currentPattern.Clear(); // Assuming Clear() method clears all notes and resets the sequencer
                Debug.Log("Current pattern cleared.");
            }

            // Update the board
            UpdateBoardManager(currentPattern);

            // Update the UI
            UpdatePatternDisplay();

            // Save the updated state to reflect the cleared pattern
            SavePatterns();

            Debug.Log("Board reset, patterns updated, and patterns saved.");
        }
        else
        {
            Debug.LogWarning("No current pattern to clear.");
        }
    }

    private void OnApplicationQuit()
    {
        // Check if there is a filename for the current project
        if (!string.IsNullOrEmpty(LastProjectFilename))
        {
            SaveProject(LastProjectFilename);
        }
        else
        {
            Debug.LogWarning("No project filename specified. Patterns will not be saved.");
        }
    }    
   
}

[System.Serializable]
public class ProjectData
{
    public List<PatternData> Patterns = new List<PatternData>();
}
