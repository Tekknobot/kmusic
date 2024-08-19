using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO; // Add this for file operations
using AudioHelm;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;

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
    public GameObject chopButton;
    public int sequencersLength;
    public int patternCount = 0;
    // A variable to keep track of the previously displayed pattern to avoid unnecessary updates
    public int previousPattern = -1;
    public int currenPatternIndex = -1;
        
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

    void Update()
    {
        // Assuming you have a way to get the current index, e.g., from a clock or sequencer
        int currentIndex = GetCurrentIndex(); // This is a placeholder for however you're tracking the current step index

        // Determine the current pattern number based on the index
        int currentPattern = (currentIndex / 16) + 1;

        currenPatternIndex = currentPattern;
        
        // Update the pattern display if the pattern has changed
        if (currentPattern != previousPattern)
        {
            UpdatePatternDisplay();
            previousPattern = currentPattern;
        }
    }

    private int GetCurrentIndex()
    {
        // Placeholder for actual logic to retrieve the current index
        // For example, it could be tied to the clock or sequencer's current step
        return sequencerPrefab.GetComponent<HelmSequencer>().currentIndex; // Assuming clock.currentStep gives you the current index
    }


    public void CreatePattern()
    {
        if (clock.pause == false)
        {
            Debug.LogWarning("Cannot create pattern while clock is running.");
            return;
        }

        // Calculate the length based on the number of patterns
        int newLength = CalculateNewLength();

        // Initialize and set length for each sequencer
        HelmSequencer helmSequencer = sequencerPrefab.GetComponent<HelmSequencer>();
        SampleSequencer sampleSequencer = sampleSequencerPrefab.GetComponent<SampleSequencer>();
        SampleSequencer drumSequencer = drumSequencerPrefab.GetComponent<SampleSequencer>();

        if (helmSequencer != null)
        {
            CopySteps(helmSequencer, newLength, patternCount == 0);
            helmSequencer.length = newLength;
        }

        if (sampleSequencer != null)
        {
            CopySteps(sampleSequencer, newLength, patternCount == 0);
            sampleSequencer.length = newLength;
        }

        if (drumSequencer != null)
        {
            CopySteps(drumSequencer, newLength, patternCount == 0);
            drumSequencer.length = newLength;
        }

        // Update the pattern count
        patternCount++;

        // Optionally, store the length for future reference if needed
        sequencersLength = newLength;

        // Update UI and save patterns
        UpdateBoardManager();
        UpdatePatternDisplay();
        SavePatterns();
    }

    private void CopySteps(HelmSequencer sequencer, int newLength, bool isFirstPattern)
    {
        if (sequencer == null)
            return;

        int copyStart = isFirstPattern ? 0 : newLength - 32;  // For first pattern, copy from start; otherwise, from last 16 steps
        int copyEnd = isFirstPattern ? 16 : newLength - 16;

        // Assume HelmSequencer has a method GetAllNotes() that returns a list of notes
        var notes = sequencer.GetAllNotes();  // Hypothetical method
        foreach (var note in notes)
        {
            if (note.start >= copyStart && note.start < copyEnd)
            {
                // Create a copy of the note for the next 16 steps
                var newNote = new Note
                {
                    note = note.note,
                    start = note.start + 16,
                    end = Mathf.Min(note.end + 16, newLength),
                    velocity = note.velocity
                };
                sequencer.AddNote(newNote.note, newNote.start, newNote.end);  // Add the copied note
            }
        }
    }

    private void CopySteps(SampleSequencer sequencer, int newLength, bool isFirstPattern)
    {
        if (sequencer == null)
            return;

        int copyStart = isFirstPattern ? 0 : newLength - 32;  // For first pattern, copy from start; otherwise, from last 16 steps
        int copyEnd = isFirstPattern ? 16 : newLength - 16;

        // Assume SampleSequencer has a method GetAllNotes() that returns a list of notes
        var notes = sequencer.GetAllNotes();  // Hypothetical method
        foreach (var note in notes)
        {
            if (note.start >= copyStart && note.start < copyEnd)
            {
                // Create a copy of the note for the next 16 steps
                var newNote = new Note
                {
                    note = note.note,
                    start = note.start + 16,
                    end = Mathf.Min(note.end + 16, newLength),
                    velocity = note.velocity
                };
                sequencer.AddNote(newNote.note, newNote.start, newNote.end);  // Add the copied note
            }
        }
    }

    private int CalculateNewLength()
    {
        // Length is 16 for the first pattern, increment by 16 for each additional pattern
        return 16 + 16 * patternCount;
    }



    public void StartPatterns()
    {
        if (clock == null)
        {
            Debug.LogError("Clock not assigned.");
            return;
        }

        isPlaying = true;
        clock.Reset();
        clock.pause = false;

        StopAllCoroutines(); // Stop any previous coroutines to avoid conflicts
    }

    public void StopPatterns()
    {
        isPlaying = false;
        clock.pause = true;

        DisableAllSequencers();

        UpdatePatternDisplay(); // Update UI
        Debug.Log("Stopped all patterns.");
    }
    public void RemovePattern()
    {
        if (!clock.pause)
        {
            Debug.LogWarning("Cannot remove pattern while clock is running.");
            return;
        }

        // Decrease the length of each sequencer by 16
        HelmSequencer helmSequencer = sequencerPrefab.GetComponent<HelmSequencer>();
        if (helmSequencer != null)
        {
            helmSequencer.length = Mathf.Max(16, helmSequencer.length - 16);
        }

        SampleSequencer sampleSequencer = sampleSequencerPrefab.GetComponent<SampleSequencer>();
        if (sampleSequencer != null)
        {
            sampleSequencer.length = Mathf.Max(16, sampleSequencer.length - 16);
        }

        SampleSequencer drumSequencer = drumSequencerPrefab.GetComponent<SampleSequencer>();
        if (drumSequencer != null)
        {
            drumSequencer.length = Mathf.Max(16, drumSequencer.length - 16);
        }

        // Update patternCount to ensure it does not go below 0
        patternCount = Mathf.Max(0, patternCount - 1);

        // Update sequencersLength to the length of helmSequencer
        sequencersLength = helmSequencer != null ? helmSequencer.length : 0;

        // Update UI and save the patterns
        UpdateBoardManager();
        UpdatePatternDisplay(); // Update UI
        SavePatterns(); // Save the updated list of patterns
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

    public void DisableAllSequencers()
    {
        // Disable all Helm sequencers
        foreach (var helmSequencer in patterns)
        {
            helmSequencer.enabled = false;
        }

        // Disable all Sample sequencers
        foreach (var sampleSequencer in samplePatterns)
        {
            sampleSequencer.enabled = false;
        }

        // Disable all Drum sequencers
        foreach (var drumSequencer in drumPatterns)
        {
            drumSequencer.enabled = false;
        }
    }            
    private string CreateNewProjectFile()
    {
        // Generate a unique filename for the new project
        string newFilename = GenerateUniqueFilename();

        // Create a new ProjectData object with default values
        ProjectData newProjectData = new ProjectData
        {
            Patterns = new List<PatternData>(),
            SamplePatterns = new List<PatternData>(),
            DrumPatterns = new List<PatternData>()
        };

        // Convert the ProjectData object to JSON
        string json = JsonUtility.ToJson(newProjectData, true);

        // Define the path to save the new project file
        string path = Path.Combine(Application.persistentDataPath, newFilename);

        // Write the JSON to the file
        File.WriteAllText(path, json);

        Debug.Log($"New project file created: {newFilename}");

        // Return the filename for immediate loading
        return newFilename;
    }

    public void CreateAndLoadNewProject()
    {
        UnloadCurrentProject();

        // Create a new project file and get the filename
        string newFilename = CreateNewProjectFile();
        LastProjectFilename = newFilename;
        // Load the newly created project
        LoadProject(newFilename);

        // Optionally, set all sequencers to loop and update the display
        //SetAllSequencersLoop(true); // Ensure all sequencers have looping enabled
        UpdatePatternDisplay(); // Update UI to reflect loaded patterns

        Debug.Log($"New project created and loaded: {newFilename}");

        UpdateProjectFileText();
    }

    public void SaveProject(string filename)
    {
        ProjectData projectData = new ProjectData
        {
            Patterns = new List<PatternData>(),
            SamplePatterns = new List<PatternData>(),
            DrumPatterns = new List<PatternData>(),
            songIndex = MultipleAudioLoader.Instance.currentIndex, // Save the current song index
            bpm = (int)clock.bpm,
            timestamps = chopButton.GetComponent<Chop>().timestamps
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
        UnloadCurrentProject();

        string path = Path.Combine(Application.persistentDataPath, filename);
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                ProjectData projectData = JsonUtility.FromJson<ProjectData>(json);

                // Clear current patterns
                RemoveAllPatterns();

                // Load patterns as before
                if (projectData != null && projectData.Patterns != null)
                {
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

                if (projectData.SamplePatterns != null)
                {
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

                if (projectData.DrumPatterns != null)
                {
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

                // Restore the song index
                if (projectData.songIndex >= 0 && projectData.songIndex < MultipleAudioLoader.Instance.clipFileNames.Count)
                {
                    MultipleAudioLoader.Instance.currentIndex = projectData.songIndex;
                    string songToLoad = MultipleAudioLoader.Instance.clipFileNames[projectData.songIndex];
                    StartCoroutine(MultipleAudioLoader.Instance.LoadClip(songToLoad));
                }

                // Restore the BPM
                if (projectData.bpm > 0)
                {
                    clock.bpm = projectData.bpm;
                    if (GameObject.Find("BPM") != null)
                    {
                        GameObject.Find("BPM").GetComponent<Slider>().value = clock.bpm;
                    }
                }

                // Restore timestamps
                var chopComponent = chopButton.GetComponent<Chop>();
                if (chopComponent != null)
                {
                    chopComponent.timestamps = projectData.timestamps;
                }
                else
                {
                    Debug.LogError("Chop component not found on chopButton.");
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

        // Log the new filename for debugging
        Debug.Log($"Generated new filename: Project_{newNumber}.json");

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
        UnloadCurrentProject();

        string nextFilename = GetNextProjectFile();
        if (!string.IsNullOrEmpty(nextFilename))
        {
            LoadProject(nextFilename);
            SetAllSequencersLoop(true); // Ensure all sequencers have looping enabled
            UpdatePatternDisplay(); // Update UI
        }
    }  

    public void LoadNewProject(string filename)
    {
        // Unload the current project if any
        UnloadCurrentProject();

        // Ensure the filename is not empty
        if (!string.IsNullOrEmpty(filename))
        {
            // Load the new project
            LoadProject(filename);

            // Ensure all sequencers have looping enabled
            SetAllSequencersLoop(true);

            // Update UI to reflect new patterns
            UpdatePatternDisplay();

            // Optionally, update the last accessed file
            lastAccessedFile = Path.Combine(Application.persistentDataPath, filename);
            LastProjectFilename = filename;
        }
        else
        {
            Debug.LogError("Filename is null or empty.");
        }
    }

    private void SetAllSequencersLoop(bool loopEnabled)
    {
        // Set loop for all HelmSequencers
        foreach (var pattern in patterns)
        {
            if (pattern != null)
            {
                pattern.loop = loopEnabled;
            }
        }

        // Set loop for all SampleSequencers
        foreach (var samplePattern in samplePatterns)
        {
            if (samplePattern != null)
            {
                samplePattern.loop = loopEnabled;
            }
        }

        // Set loop for all DrumSequencers
        foreach (var drumPattern in drumPatterns)
        {
            if (drumPattern != null)
            {
                drumPattern.loop = loopEnabled;
            }
        }

        Debug.Log($"All sequencers have had their loop set to {loopEnabled}.");
    }

    public void DeleteCurrentProject()
    {
        if (string.IsNullOrEmpty(LastProjectFilename))
        {
            Debug.LogError("No project is currently loaded. Cannot delete.");
            return;
        }

        // Get the full path of the current project file
        string path = Path.Combine(Application.persistentDataPath, LastProjectFilename);

        if (File.Exists(path))
        {
            try
            {
                // Delete the project file
                File.Delete(path);
                Debug.Log($"Project file deleted: {LastProjectFilename}");

                // Clear the current project data
                UnloadCurrentProject();
                lastAccessedFile = null;
                LastProjectFilename = null;

                // Update the UI to reflect the deletion
                UpdatePatternDisplay();
                UpdateProjectFileText();

                Debug.Log("Current project data cleared and UI updated.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error deleting project file: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"File not found: {path}. Cannot delete.");
        }
    }


    // Method to unload all existing project data
    private void UnloadCurrentProject()
    {
        // Clear all existing patterns and sequencers
        RemoveAllPatterns();
        RemoveAllSamplePatterns();
        RemoveAllDrumPatterns();

        Debug.Log("Current project has been unloaded.");
    }

    // Method to remove all HelmSequencers
    private void RemoveAllPatterns()
    {
        foreach (var pattern in patterns)
        {
            if (pattern != null)
            {
                Destroy(pattern.gameObject);
            }
        }
        patterns.Clear();
    }

    // Method to remove all SampleSequencers
    private void RemoveAllSamplePatterns()
    {
        foreach (var samplePattern in samplePatterns)
        {
            if (samplePattern != null)
            {
                Destroy(samplePattern.gameObject);
            }
        }
        samplePatterns.Clear();
    }

    // Method to remove all DrumSequencers
    private void RemoveAllDrumPatterns()
    {
        foreach (var drumPattern in drumPatterns)
        {
            if (drumPattern != null)
            {
                Destroy(drumPattern.gameObject);
            }
        }
        drumPatterns.Clear();
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

    public int songIndex; // Store the index or identifier of the song

    public float bpm;

    public List<float> timestamps = new List<float>();
}
