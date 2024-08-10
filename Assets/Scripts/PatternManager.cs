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
    public SampleSequencer sampleSequencer; // New sequencer for samples
    public SampleSequencer drumSequencer;   // New sequencer for drums
    public GameObject sequencerPrefab;
    public GameObject sampleSequencerPrefab;
    public GameObject drumSequencerPrefab;
    public AudioHelmClock clock;
    public BoardManager boardManager;
    public PatternUIManager patternUIManager; // Reference to the UI manager

    public List<HelmSequencer> patterns = new List<HelmSequencer>();
    public List<SampleSequencer> samplePatterns = new List<SampleSequencer>();
    public List<SampleSequencer> drumPatterns = new List<SampleSequencer>();

    public int currentPatternIndex = -1;
    public int currentSamplePatternIndex = -1;
    public int currentDrumPatternIndex = -1;

    public bool isPlaying = false;
    private int currentStepIndex = 0; // Track the current step index

    public int PatternsCount => patterns.Count;
    public int SamplePatternsCount => samplePatterns.Count;
    public int DrumPatternsCount => drumPatterns.Count;
    public int CurrentPatternIndex => currentPatternIndex;
    public int CurrentSamplePatternIndex => currentSamplePatternIndex;
    public int CurrentDrumPatternIndex => currentDrumPatternIndex;        
    public static string LastProjectFilename { get; private set; }
    private static string lastAccessedFile = null;
    public TextMeshProUGUI projectFileText; // Reference to the TextMeshPro component

    public GameObject componentButton;

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

    void Update() {
        if (isPlaying) {
            sequencerPrefab.GetComponent<HelmSequencer>().loop = false;
            sampleSequencerPrefab.GetComponent<SampleSequencer>().loop = false;
            drumSequencerPrefab.GetComponent<SampleSequencer>().loop = false;
        }           
    }

    public void CreatePattern()
    {
        if (sequencerPrefab == null)
        {
            Debug.LogError("Sequencer Prefab not assigned.");
            return;
        }

        // Create HelmSequencer
        GameObject helmSequencerObj = Instantiate(sequencerPrefab, transform);
        helmSequencerObj.GetComponent<HelmSequencer>().loop = true;
        HelmSequencer newHelmSequencer = helmSequencerObj.GetComponent<HelmSequencer>();
        if (newHelmSequencer == null)
        {
            Debug.LogError("New sequencer prefab does not have a HelmSequencer component.");
            return;
        }
        newHelmSequencer.enabled = false;
        newHelmSequencer.name = $"{sequencerPrefab.name}_{patterns.Count + 1}"; // Name the HelmSequencer

        // Create SampleSequencer for samples
        GameObject sampleSequencerObj = Instantiate(sampleSequencerPrefab, transform);
        sampleSequencerObj.GetComponent<SampleSequencer>().loop = true;
        SampleSequencer newSampleSequencer = sampleSequencerObj.GetComponent<SampleSequencer>();
        if (newSampleSequencer == null)
        {
            Debug.LogError("New sequencer prefab does not have a SampleSequencer component.");
            return;
        }
        newSampleSequencer.enabled = false;
        newSampleSequencer.name = $"{sequencerPrefab.name}_Sample_{samplePatterns.Count + 1}"; // Name the SampleSequencer

        // Create SampleSequencer for drums
        GameObject drumSequencerObj = Instantiate(drumSequencerPrefab, transform);
        drumSequencerObj.GetComponent<SampleSequencer>().loop = true;
        SampleSequencer newDrumSequencer = drumSequencerObj.GetComponent<SampleSequencer>();
        if (newDrumSequencer == null)
        {
            Debug.LogError("New sequencer prefab does not have a SampleSequencer component.");
            return;
        }
        newDrumSequencer.enabled = false;
        newDrumSequencer.name = $"{sequencerPrefab.name}_Drum_{drumPatterns.Count + 1}"; // Name the DrumSequencer

        // Transfer notes from existing HelmSequencer to the new HelmSequencer
        if (patterns.Count > 0)
        {
            HelmSequencer lastHelmSequencer = patterns[patterns.Count - 1];
            TransferNotes(lastHelmSequencer, newHelmSequencer);
        }
        else if (sourceSequencer != null)
        {
            TransferNotes(sourceSequencer, newHelmSequencer);
        }

        // Transfer notes from existing SampleSequencer to the new SampleSequencer
        if (samplePatterns.Count > 0)
        {
            SampleSequencer lastSampleSequencer = samplePatterns[samplePatterns.Count - 1];
            TransferSamplerNotes(lastSampleSequencer, newSampleSequencer);
        }
        else if (sampleSequencer != null)
        {
            TransferSamplerNotes(sampleSequencer, newSampleSequencer);
        }

        // Transfer notes from existing DrumSequencer to the new DrumSequencer
        if (drumPatterns.Count > 0)
        {
            SampleSequencer lastDrumSequencer = drumPatterns[drumPatterns.Count - 1];
            TransferSamplerNotes(lastDrumSequencer, newDrumSequencer);
        }
        else if (drumSequencer != null)
        {
            TransferSamplerNotes(drumSequencer, newDrumSequencer);
        }

        // Add new sequencers to the lists
        patterns.Add(newHelmSequencer);
        samplePatterns.Add(newSampleSequencer);
        drumPatterns.Add(newDrumSequencer);

        Debug.Log($"Three patterns created and added to the list. Total patterns: {patterns.Count}");

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

    private void TransferSamplerNotes(SampleSequencer source, SampleSequencer target)
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
        if (patterns.Count == 0 && samplePatterns.Count == 0 && drumPatterns.Count == 0)
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

            // Update the current pattern index
            currentPatternIndex = (currentPatternIndex + 1) % patterns.Count;
            currentSamplePatternIndex = (currentSamplePatternIndex + 1) % samplePatterns.Count;
            currentDrumPatternIndex = (currentDrumPatternIndex + 1) % drumPatterns.Count;

            HelmSequencer currentPattern = patterns[currentPatternIndex];
            SampleSequencer currentSamplePattern = samplePatterns[currentSamplePatternIndex];
            SampleSequencer currentDrumPattern = drumPatterns[currentDrumPatternIndex];

            Debug.Log($"Playing pattern index: {currentPatternIndex}");

            // Stop all patterns
            foreach (var pattern in patterns)
            {
                StopPattern(pattern);
            }

            // Start the new pattern
            currentPattern.enabled = true;
            currentSamplePattern.enabled = true;
            currentDrumPattern.enabled = true;
            if (componentButton.GetComponent<ComponentButton>().currentPatternGroup == 1) {
                UpdateBoardManager(currentPattern);
            }
            if (componentButton.GetComponent<ComponentButton>().currentPatternGroup == 2) {
                UpdateBoardManageForSamples(currentSamplePattern);
            } 
            
            UpdatePatternDisplay(); // Update UI

            // Wait for board manager to reach the desired cell index (or other condition)
            yield return new WaitUntil(() => boardManager.GetHighlightedCellIndex() == 15);

            // Wait for the duration of one step
            yield return new WaitForSeconds(stepDuration);

            currentPattern.enabled = false;
            currentSamplePattern.enabled = false;
            currentDrumPattern.enabled = false;
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

        UpdatePatternDisplay(); // Update UI
        Debug.Log("Stopped all patterns.");
    }

    public void RemovePattern(int index)
    {
        bool removedAny = false; // Flag to track if any pattern was removed

        // Log the counts of all lists
        Debug.Log($"Patterns count: {patterns.Count}, SamplePatterns count: {samplePatterns.Count}, DrumPatterns count: {drumPatterns.Count}");

        // Remove HelmSequencer pattern if index is valid
        if (index >= 0 && index < patterns.Count)
        {
            HelmSequencer patternToRemove = patterns[index];
            Destroy(patternToRemove.gameObject);
            patterns.RemoveAt(index);
            removedAny = true;
            Debug.Log($"Removed HelmSequencer pattern at index: {index}");
        }
        else
        {
            Debug.LogWarning($"Invalid HelmSequencer index: {index}. No HelmSequencer pattern was removed.");
        }

        // Remove SampleSequencer pattern if index is valid
        if (index >= 0 && index < samplePatterns.Count)
        {
            SampleSequencer samplePatternToRemove = samplePatterns[index];
            Destroy(samplePatternToRemove.gameObject);
            samplePatterns.RemoveAt(index);
            removedAny = true;
            Debug.Log($"Removed SampleSequencer pattern at index: {index}");
        }
        else
        {
            Debug.LogWarning($"Invalid SampleSequencer index: {index}. No SampleSequencer pattern was removed.");
        }

        // Remove DrumSequencer pattern if index is valid
        if (index >= 0 && index < drumPatterns.Count)
        {
            SampleSequencer drumPatternToRemove = drumPatterns[index];
            Destroy(drumPatternToRemove.gameObject);
            drumPatterns.RemoveAt(index);
            removedAny = true;
            Debug.Log($"Removed DrumSequencer pattern at index: {index}");
        }
        else
        {
            Debug.LogWarning($"Invalid DrumSequencer index: {index}. No DrumSequencer pattern was removed.");
        }

        if (removedAny)
        {
            UpdateBoardManager();
            UpdatePatternDisplay(); // Update UI
            SavePatterns(); // Save the updated list of patterns
        }
        else
        {
            Debug.LogWarning("No patterns were removed.");
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

    private void UpdateBoardManageForSamples(SampleSequencer currentPattern = null)
    {
        if (boardManager != null)
        {
            if (currentPattern != null)
            {
                List<AudioHelm.Note> notes = new List<AudioHelm.Note>(currentPattern.GetAllNotes());
                boardManager.ResetBoard();
                boardManager.UpdateBoardWithSampleNotes(notes);
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
        // Create a new ProjectData instance to hold all pattern data
        ProjectData projectData = new ProjectData
        {
            Patterns = new List<PatternData>(),
            SamplePatterns = new List<PatternData>(),
            DrumPatterns = new List<PatternData>()
        };

        // Convert and add HelmSequencer patterns
        foreach (var pattern in patterns)
        {
            PatternData patternData = ConvertSequencerToPatternData(pattern);
            projectData.Patterns.Add(patternData);
        }

        // Convert and add SampleSequencer patterns
        foreach (var samplePattern in samplePatterns)
        {
            PatternData patternData = ConvertSamplerSequencerToPatternData(samplePattern);
            projectData.SamplePatterns.Add(patternData);
        }

        // Convert and add DrumSequencer patterns
        foreach (var drumPattern in drumPatterns)
        {
            PatternData patternData = ConvertDrumSequencerToPatternData(drumPattern);
            projectData.DrumPatterns.Add(patternData);
        }

        // Save all pattern data to file
        DataManager.SaveProjectToFile(projectData);
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

    private PatternData ConvertSamplerSequencerToPatternData(SampleSequencer sequencer)
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

    private PatternData ConvertDrumSequencerToPatternData(SampleSequencer sequencer)
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

    private void PopulateSampleSequencerFromPatternData(SampleSequencer sequencer, PatternData patternData)
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

    private void PopulateDrumSequencerFromPatternData(SampleSequencer sequencer, PatternData patternData)
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

    public HelmSequencer GetActiveSequencer()
    {
        if (currentPatternIndex >= 0 && currentPatternIndex < patterns.Count)
        {
            return patterns[currentPatternIndex];
        }
        else
        {
            Debug.LogWarning("No active helm pattern.");
            return null;
        }
    }    

    public SampleSequencer GetActiveSampleSequencer()
    {
        if (currentPatternIndex >= 0 && currentPatternIndex < samplePatterns.Count)
        {
            return samplePatterns[currentPatternIndex];
        }
        else
        {
            Debug.LogWarning("No active sample pattern.");
            return null;
        }
    }    

    public SampleSequencer GetActiveDrumSequencer()
    {
        if (currentPatternIndex >= 0 && currentPatternIndex < drumPatterns.Count)
        {
            return drumPatterns[currentPatternIndex];
        }
        else
        {
            Debug.LogWarning("No active drum pattern.");
            return null;
        }
    }            

    public void SaveProject(string filename)
    {
        ProjectData projectData = new ProjectData
        {
            Patterns = new List<PatternData>(),
            SamplePatterns = new List<PatternData>(),
            DrumPatterns = new List<PatternData>()
        };

        // Collect HelmSequencer patterns
        foreach (var pattern in patterns)
        {
            PatternData patternData = ConvertSequencerToPatternData(pattern);
            projectData.Patterns.Add(patternData);
        }

        // Collect SampleSequencer patterns
        foreach (var pattern in samplePatterns)
        {
            PatternData patternData = ConvertSamplerSequencerToPatternData(pattern);
            projectData.SamplePatterns.Add(patternData);
        }

        // Collect DrumSequencer patterns
        foreach (var pattern in drumPatterns)
        {
            PatternData patternData = ConvertSamplerSequencerToPatternData(pattern);
            projectData.DrumPatterns.Add(patternData);
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

                // Load HelmSequencer patterns
                if (projectData != null && projectData.Patterns != null)
                {
                    Debug.Log("Loading HelmSequencer patterns...");
                    foreach (var patternData in projectData.Patterns)
                    {
                        HelmSequencer newSequencer = Instantiate(sequencerPrefab)?.GetComponent<HelmSequencer>();
                        if (newSequencer != null)
                        {
                            newSequencer.enabled = false;
                            PopulateSequencerFromPatternData(newSequencer, patternData);
                            patterns.Add(newSequencer);
                            Debug.Log($"Added HelmSequencer pattern: {patternData.Name}");
                        }
                        else
                        {
                            Debug.LogError("Failed to instantiate HelmSequencer prefab.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No HelmSequencer patterns found in project data.");
                }

                // Load SampleSequencer patterns
                if (projectData.SamplePatterns != null)
                {
                    Debug.Log("Loading SampleSequencer patterns...");
                    foreach (var patternData in projectData.SamplePatterns)
                    {
                        SampleSequencer newSequencer = Instantiate(sampleSequencerPrefab)?.GetComponent<SampleSequencer>();
                        if (newSequencer != null)
                        {
                            newSequencer.enabled = false;
                            PopulateSampleSequencerFromPatternData(newSequencer, patternData);
                            samplePatterns.Add(newSequencer);
                            Debug.Log($"Added SampleSequencer pattern: {patternData.Name}");
                        }
                        else
                        {
                            Debug.LogError("Failed to instantiate SampleSequencer prefab.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No SampleSequencer patterns found in project data.");
                }

                // Load DrumSequencer patterns
                if (projectData.DrumPatterns != null)
                {
                    Debug.Log("Loading DrumSequencer patterns...");
                    foreach (var patternData in projectData.DrumPatterns)
                    {
                        SampleSequencer newSequencer = Instantiate(drumSequencerPrefab)?.GetComponent<SampleSequencer>();
                        if (newSequencer != null)
                        {
                            newSequencer.enabled = false;
                            PopulateDrumSequencerFromPatternData(newSequencer, patternData);
                            drumPatterns.Add(newSequencer);
                            Debug.Log($"Added DrumSequencer pattern: {patternData.Name}");
                        }
                        else
                        {
                            Debug.LogError("Failed to instantiate DrumSequencer prefab.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No DrumSequencer patterns found in project data.");
                }

                Debug.Log($"Project loaded from file: {filename}");
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

    public void LoadNewProject(string filename)
    {
        if (!string.IsNullOrEmpty(filename))
        {
            LoadProject(filename);
            GetNextProjectFile();
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

            SampleSequencer currentSamplePattern = samplePatterns[currentPatternIndex];
            if (currentSamplePattern != null)
            {
                currentSamplePattern.Clear(); // Assuming Clear() method clears all notes and resets the sequencer
                Debug.Log("Current pattern cleared.");
            }

            SampleSequencer currentDrumPattern = drumPatterns[currentPatternIndex];
            if (currentDrumPattern != null)
            {
                currentDrumPattern.Clear(); // Assuming Clear() method clears all notes and resets the sequencer
                Debug.Log("Current pattern cleared.");
            }

            // Update the board
            UpdateBoardManager(currentPattern);
            UpdateBoardManageForSamples(currentSamplePattern);
            UpdateBoardManageForSamples(currentDrumPattern);

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

    public void SaveOver()
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


[Serializable]
public class ProjectData
{
    public List<PatternData> Patterns = new List<PatternData>();
    public List<PatternData> SamplePatterns = new List<PatternData>();
    public List<PatternData> DrumPatterns = new List<PatternData>();
}
