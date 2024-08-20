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

    public int currentPatternIndex = 0;
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
        //LoadPatterns();
    }

    void Update()
    {
        // Assuming you have a way to get the current index, e.g., from a clock or sequencer
        int currentIndex = GetCurrentIndex(); // Placeholder for getting the current step index

        // Define the number of steps per pattern
        int stepsPerPattern = 16;

        // Determine the current pattern number based on the index and round up
        // Cast currentIndex to double to resolve ambiguity with Math.Ceiling
        int currentPattern = (int)Math.Ceiling((double)currentIndex / stepsPerPattern);

        // Update currentPatternIndex
        currentPatternIndex = currentPattern;

        // Update the pattern display if the pattern has changed
        if (currentPattern != previousPattern)
        {
            previousPattern = currentPattern;
            if (componentButton.GetComponent<ComponentButton>().currentPatternGroup == 1) {
                UpdateBoardManager();
            }
            if (componentButton.GetComponent<ComponentButton>().currentPatternGroup == 2) {
                UpdateBoardManageForSamples();
            }            
            UpdatePatternDisplay();
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

    private void UpdateBoardManager()
    {
        if (boardManager == null)
        {
            Debug.LogError("BoardManager not assigned.");
            return;
        }

        if (PatternManager.Instance == null || boardManager == null)
        {
            Debug.LogError("PatternManager or BoardManager is not set.");
            return;
        }

        HelmSequencer currentPattern = sequencerPrefab.GetComponent<HelmSequencer>();

        if (currentPattern != null)
        {
            int stepsPerPattern = 16;
            int totalSteps = currentPattern.length;

            // Adjust pattern index to be 1-based and calculate start and end steps
            int currentPatternIndex = PatternManager.Instance.currentPatternIndex;
            int patternStartStep = (currentPatternIndex - 1) * stepsPerPattern;
            int patternEndStep = patternStartStep + stepsPerPattern - 1;

            // Ensure patternEndStep does not exceed total steps of the sequencer
            if (patternEndStep >= totalSteps)
            {
                patternEndStep = totalSteps - 1;
            }

            // Log values for debugging
            Debug.Log($"Pattern Index: {currentPatternIndex}");
            Debug.Log($"Pattern Start Step: {patternStartStep}");
            Debug.Log($"Pattern End Step: {patternEndStep}");

            // Get all notes from the current pattern
            List<AudioHelm.Note> allNotes = new List<AudioHelm.Note>(currentPattern.GetAllNotes());

            // Log total notes and their steps
            Debug.Log($"Total Notes in Pattern: {allNotes.Count}");
            foreach (var note in allNotes)
            {
                Debug.Log($"Note at step {note.start}");
            }

            // Filter notes based on the current pattern section
            List<AudioHelm.Note> notesInSection = allNotes.FindAll(note =>
                note.start >= patternStartStep && note.start <= patternEndStep
            );

            // Log filtered notes
            Debug.Log($"Notes in Section: {notesInSection.Count}");
            foreach (var note in notesInSection)
            {
                Debug.Log($"Filtered Note at step {note.start}");
            }

            // Reset the board and update with filtered notes
            boardManager.ResetBoard();
            boardManager.UpdateBoardWithNotes(notesInSection);
        }
        else
        {
            // Handle the case where currentPattern is null
            Debug.LogWarning("Current pattern is not available.");
            //boardManager.ResetBoard();
        }
    }

    private void UpdateBoardManageForSamples()
    {
        if (boardManager == null)
        {
            Debug.LogError("BoardManager not assigned.");
            return;
        }

        if (PatternManager.Instance == null || sampleSequencerPrefab == null)
        {
            Debug.LogError("PatternManager or SampleSequencerPrefab is not set.");
            return;
        }

        SampleSequencer currentPattern = sampleSequencerPrefab.GetComponent<SampleSequencer>();

        if (currentPattern != null)
        {
            int stepsPerPattern = 16;
            int totalSteps = currentPattern.length;

            // Adjust pattern index to be 1-based and calculate start and end steps
            int currentPatternIndex = PatternManager.Instance.currentPatternIndex;
            int patternStartStep = (currentPatternIndex - 1) * stepsPerPattern;
            int patternEndStep = patternStartStep + stepsPerPattern - 1;

            // Ensure patternEndStep does not exceed total steps of the sequencer
            if (patternEndStep >= totalSteps)
            {
                patternEndStep = totalSteps - 1;
            }

            // Log values for debugging
            Debug.Log($"Pattern Index: {currentPatternIndex}");
            Debug.Log($"Pattern Start Step: {patternStartStep}");
            Debug.Log($"Pattern End Step: {patternEndStep}");

            // Get all notes from the current pattern
            List<AudioHelm.Note> allNotes = new List<AudioHelm.Note>(currentPattern.GetAllNotes());

            // Log total notes and their steps
            Debug.Log($"Total Notes in Pattern: {allNotes.Count}");
            foreach (var note in allNotes)
            {
                Debug.Log($"Note at step {note.start}");
            }

            // Filter notes based on the current pattern section
            List<AudioHelm.Note> notesInSection = allNotes.FindAll(note =>
                note.start >= patternStartStep && note.start <= patternEndStep
            );

            // Log filtered notes
            Debug.Log($"Notes in Section: {notesInSection.Count}");
            foreach (var note in notesInSection)
            {
                Debug.Log($"Filtered Note at step {note.start}");
            }

            // Reset the board and update with filtered notes
            boardManager.ResetBoard();
            boardManager.UpdateBoardWithSampleNotes(notesInSection);
        }
        else
        {
            // Handle the case where currentPattern is null
            Debug.LogWarning("Current sample pattern is not available.");
            // Optionally reset the board
            //boardManager.ResetBoard();
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
            HelmPattern = GetPatternDataForSequencer(PatternManager.Instance.sequencerPrefab),
            SamplePattern = GetPatternDataForSequencer(PatternManager.Instance.sampleSequencerPrefab),
            DrumPattern = GetPatternDataForSequencer(PatternManager.Instance.drumSequencerPrefab),
            HelmSequencerLength = GetSequencerLength(PatternManager.Instance.sequencerPrefab),
            SampleSequencerLength = GetSequencerLength(PatternManager.Instance.sampleSequencerPrefab),
            DrumSequencerLength = GetSequencerLength(PatternManager.Instance.drumSequencerPrefab),
            songIndex = MultipleAudioLoader.Instance.currentIndex,
            bpm = clock.bpm,
            timestamps = chopButton.GetComponent<Chop>().timestamps
        };

        // Save all pattern data to file
        DataManager.SaveProjectToFile(projectData);
        Debug.Log("Patterns saved to file.");
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
        foreach (var tile in patternData.keyTiles)
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
        foreach (var tile in patternData.sampleTiles)
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
        foreach (var tile in patternData.sampleTiles)
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

    private string CreateNewProjectFile()
    {
        // Generate a unique filename for the new project
        string newFilename = GenerateUniqueFilename();

        // Create a new ProjectData object with default values
        ProjectData newProjectData = new ProjectData
        {
            HelmPattern = null,
            SamplePattern = null,
            DrumPattern = null,
            HelmSequencerLength = 0,  // Default length for new project
            SampleSequencerLength = 0,
            DrumSequencerLength = 0,
            songIndex = 0,
            bpm = 120f,
            timestamps = new List<float>()
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
        // Create a new project file and get the filename
        string newFilename = CreateNewProjectFile();
        LastProjectFilename = newFilename;

        // Load the newly created project
        LoadProject(newFilename);

        // Optionally, set all sequencers to loop and update the display
        // SetAllSequencersLoop(true); // Ensure all sequencers have looping enabled
        UpdatePatternDisplay(); // Update UI to reflect loaded patterns

        Debug.Log($"New project created and loaded: {newFilename}");

        UpdateProjectFileText();
    }


    public void SaveProject(string filename)
    {
        try
        {
            ProjectData projectData = new ProjectData
            {
                HelmPattern = GetPatternDataForSequencer(PatternManager.Instance.sequencerPrefab),
                SamplePattern = GetPatternDataForSequencer(PatternManager.Instance.sampleSequencerPrefab),
                DrumPattern = GetPatternDataForSequencer(PatternManager.Instance.drumSequencerPrefab),
                songIndex = MultipleAudioLoader.Instance.currentIndex,
                bpm = clock.bpm,
                timestamps = chopButton.GetComponent<Chop>().timestamps,
                HelmSequencerLength = GetSequencerLength(PatternManager.Instance.sequencerPrefab),
                SampleSequencerLength = GetSequencerLength(PatternManager.Instance.sampleSequencerPrefab),
                DrumSequencerLength = GetSequencerLength(PatternManager.Instance.drumSequencerPrefab),                
            };

            string json = JsonUtility.ToJson(projectData, true);
            File.WriteAllText(Path.Combine(Application.persistentDataPath, filename), json);
            Debug.Log("Project saved successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving project: {ex.Message}");
        }
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

                if (projectData == null)
                {
                    Debug.LogError("Failed to load project: projectData is null.");
                    return;
                }

                // Load HelmSequencer pattern
                if (projectData.HelmPattern != null)
                {
                    var helmSequencer = PatternManager.Instance.sequencerPrefab.GetComponent<HelmSequencer>();
                    if (helmSequencer != null)
                    {
                        helmSequencer.Clear();
                        helmSequencer.length = projectData.HelmSequencerLength;
                        PopulateSequencerFromPatternData(helmSequencer, projectData.HelmPattern);
                        sequencersLength = PatternManager.Instance.sampleSequencerPrefab.GetComponent<SampleSequencer>().length;
                        UpdatePatternDisplay();
                        Debug.Log($"Loaded HelmSequencer pattern: {projectData.HelmPattern.Type}");
                    }
                    else
                    {
                        Debug.LogError("HelmSequencer not found.");
                    }
                }
                else
                {
                    Debug.LogError("Failed to load project: projectData.HelmPattern is null.");
                }

                // Load SampleSequencer pattern
                if (projectData.SamplePattern != null)
                {
                    var sampleSequencer = PatternManager.Instance.sampleSequencerPrefab.GetComponent<SampleSequencer>();
                    if (sampleSequencer != null)
                    {
                        sampleSequencer.Clear();
                        sampleSequencer.length = projectData.SampleSequencerLength;
                        PopulateSampleSequencerFromPatternData(sampleSequencer, projectData.SamplePattern);
                        sequencersLength = PatternManager.Instance.sampleSequencerPrefab.GetComponent<SampleSequencer>().length;
                        UpdatePatternDisplay();
                        Debug.Log($"Loaded SampleSequencer pattern: {projectData.SamplePattern.Type}");
                    }
                    else
                    {
                        Debug.LogError("SampleSequencer not found.");
                    }
                }
                else
                {
                    Debug.LogError("Failed to load project: projectData.SamplePattern is null.");
                }

                // Load DrumSequencer pattern
                if (projectData.DrumPattern != null)
                {
                    var drumSequencer = PatternManager.Instance.drumSequencerPrefab.GetComponent<SampleSequencer>();
                    if (drumSequencer != null)
                    {
                        drumSequencer.Clear();
                        drumSequencer.length = projectData.DrumSequencerLength;
                        PopulateDrumSequencerFromPatternData(drumSequencer, projectData.DrumPattern);
                        sequencersLength = PatternManager.Instance.sampleSequencerPrefab.GetComponent<SampleSequencer>().length;
                        UpdatePatternDisplay();
                        Debug.Log($"Loaded DrumSequencer pattern: {projectData.DrumPattern.Type}");
                    }
                    else
                    {
                        Debug.LogError("DrumSequencer not found.");
                    }
                }
                else
                {
                    Debug.LogError("Failed to load project: projectData.DrumPattern is null.");
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
                    var bpmSlider = GameObject.Find("BPM")?.GetComponent<Slider>();
                    if (bpmSlider != null)
                    {
                        bpmSlider.value = clock.bpm;
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

    private PatternData GetPatternDataForSequencer(GameObject sequencerPrefab)
    {
        PatternData patternData = new PatternData();

        // Check if it's a HelmSequencer
        var helmSequencer = sequencerPrefab.GetComponent<HelmSequencer>();
        if (helmSequencer != null)
        {
            Debug.Log("HelmSequencer detected.");
            patternData.Type = PatternData.SequencerType.Helm;
            patternData.keyTiles = new List<TileData>();

            var notes = helmSequencer.GetAllNotes(); // Method to get all notes
            foreach (var note in notes)
            {
                TileData tileData = new TileData
                {
                    SpriteName = note.note.ToString(),
                    StartTime = note.start,
                    EndTime = note.end,
                    Step = note.start, // Ensure this is correct
                    Velocity = note.velocity,
                    NoteValue = note.note // If needed
                };
                patternData.keyTiles.Add(tileData);
            }

            patternData.Length = helmSequencer.length; // Set length if available
            Debug.Log("HelmSequencer data successfully retrieved.");
            return patternData;
        }

        // Check if it's a SampleSequencer
        var sampleSequencer = sequencerPrefab.GetComponent<SampleSequencer>();
        if (sampleSequencer != null)
        {
            Debug.Log("SampleSequencer detected.");
            patternData.Type = PatternData.SequencerType.Sample;
            patternData.sampleTiles = new List<TileData>();

            var notes = sampleSequencer.GetAllNotes(); // Method to get all notes
            foreach (var note in notes)
            {
                TileData tileData = new TileData
                {
                    SpriteName = note.note.ToString(),
                    StartTime = note.start,
                    EndTime = note.end,
                    Step = note.start, // Ensure this is correct
                    Velocity = note.velocity,
                    NoteValue = note.note // If needed
                };
                patternData.sampleTiles.Add(tileData);
            }

            patternData.Length = sampleSequencer.length; // Set length if available
            Debug.Log("SampleSequencer data successfully retrieved.");
            return patternData;
        }

        Debug.LogWarning("Sequencer component not found.");
        return null;
    }
    private int GetSequencerLength(GameObject sequencerPrefab)
    {
        var helmSequencer = sequencerPrefab.GetComponent<HelmSequencer>();
        if (helmSequencer != null)
        {
            return helmSequencer.length; // Method to get the length for Helm sequencer
        }

        var sampleSequencer = sequencerPrefab.GetComponent<SampleSequencer>();
        if (sampleSequencer != null)
        {
            return sampleSequencer.length; // Method to get the length for Sample sequencer
        }

        Debug.LogWarning("Sequencer component not found.");
        return 0; // Default length if the sequencer is not found
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
            //UpdateBoardManager();
            //UpdateBoardManageForSamples(currentSamplePattern);
            //UpdateBoardManageForSamples(currentDrumPattern);

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
            Debug.Log("About to save.");
            SaveProject(LastProjectFilename);
            Debug.Log("Project Saved.");
        }
        else
        {
            Debug.LogWarning("No project filename specified. Patterns will not be saved.");
        }
    }    
}


[System.Serializable]
public class TileData
{
    public string SpriteName;
    public float StartTime;
    public float EndTime;
    public float Step;
    public float Velocity;
    public int NoteValue; // Add NoteValue if it's used

    public TileData() { }

    public TileData(string spriteName, float startTime, float endTime, float step, float velocity, int noteValue)
    {
        SpriteName = spriteName;
        StartTime = startTime;
        EndTime = endTime;
        Step = step;
        Velocity = velocity;
        NoteValue = noteValue;
    }
}

[System.Serializable]
public class PatternData
{
    public enum SequencerType { Helm, Sample, Drum }
    public SequencerType Type;
    public List<TileData> keyTiles; // For HelmSequencer
    public List<TileData> sampleTiles; // For SampleSequencer
    public float Length; // Total length of the pattern or sequencer
}

[System.Serializable]
public class ProjectData
{
    public PatternData HelmPattern;
    public PatternData SamplePattern;
    public PatternData DrumPattern;
    public int songIndex;
    public float bpm;
    public List<float> timestamps;
    public int HelmSequencerLength;    
    public int SampleSequencerLength;    
    public int DrumSequencerLength;
}
