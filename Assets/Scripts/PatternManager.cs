using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO; // Add this for file operations.
using AudioHelm;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Sanford.Multimedia.Midi;
using kmusic.kmusicMIDI;

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

    public int currentPatternIndex = 1;
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
    public bool isBoardUpdateRequired = false;
    public string currentProjectFilename; // Keep track of the current project filename

    private bool isClearingPattern = false; // Flag to track if a pattern is being cleared
    // Reference to the source AudioClip (the song) you wish to chop.
    public AudioClip songClip;
    // Reference to the Chop component which holds the chop timestamps.
    public Chop chopComponent;
    
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
        currentPatternIndex = 1;
        CreatePattern();
    }

    void Update()
    {
        // Skip updates if a pattern is being cleared
        if (isClearingPattern)
        {
            return;
        }

        // Get the current step index (e.g., from a clock or sequencer)
        int currentIndex = GetCurrentIndex();

        // Define the number of steps per pattern
        int stepsPerPattern = 16;

        // Determine the current pattern number based on the index
        int currentPattern = (int)Math.Ceiling((double)currentIndex / stepsPerPattern);

        // Ensure currentPattern is at least 1
        currentPattern = Math.Max(currentPattern, 1);

        // If the current pattern changes, flag a board update
        if (currentPattern != previousPattern)
        {
            previousPattern = currentPattern;
            currentPatternIndex = currentPattern;
            isBoardUpdateRequired = true;
            Debug.Log($"Pattern changed to {currentPattern}. Marking board update required.");
        }

        // Only update the board if explicitly required
        if (isBoardUpdateRequired)
        {
            var componentButtonScript = componentButton.GetComponent<ComponentButton>();

            if (componentButtonScript == null)
            {
                Debug.LogError("ComponentButton script is missing on componentButton.");
                return;
            }

            switch (componentButtonScript.currentPatternGroup)
            {
                case 1: // Helm (keys)
                    Debug.Log("Updating board for Helm/Keys group.");
                    UpdateBoardManager();
                    break;

                case 2: // Samples
                    Debug.Log("Updating board for Samples group.");
                    UpdateBoardManageForSamples();
                    break;

                case 3: // Drums
                    Debug.Log("Updating board for Drums group.");
                    ExecuteDrumDisplay();
                    break;

                case 0: // Drums (alternative group)
                    Debug.Log("Updating board for Drums group.");
                    ExecuteDrumDisplay();
                    break;

                default:
                    Debug.LogWarning($"Unhandled pattern group: {componentButtonScript.currentPatternGroup}");
                    break;
            }

            UpdatePatternDisplay();

            // Reset the flag after updating the board
            isBoardUpdateRequired = false;
            Debug.Log("Board update completed. Resetting update flag.");
        }
    }

    /// <summary>
    /// Executes the logic immediately without a delay.
    /// </summary>
    public void ExecuteDrumDisplay()
    {
        // Get the pad corresponding to the current sprite
        GameObject pad = PadManager.Instance.GetPadByCurrentPad();

        // Reset the board before updating
        BoardManager.Instance.ResetBoard();

        if (pad != null)
        {
            Debug.Log($"Executing UpdateBoardForPad for pad: {pad.name}");
            PadManager.Instance.UpdateBoardForPad(pad);
        }
        else
        {
            Debug.LogWarning("No pad found for the current sprite.");
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
        if (!clock.pause)
        {
            Debug.LogWarning("Cannot create pattern while clock is running.");
            return;
        }

        // Get the current length of the sequencer
        HelmSequencer helmSequencer = sequencerPrefab.GetComponent<HelmSequencer>();
        SampleSequencer sampleSequencer = sampleSequencerPrefab.GetComponent<SampleSequencer>();
        SampleSequencer drumSequencer = drumSequencerPrefab.GetComponent<SampleSequencer>();

        if (helmSequencer == null || sampleSequencer == null || drumSequencer == null)
        {
            Debug.LogError("One or more sequencer components are not assigned.");
            return;
        }

        // Calculate the current number of patterns based on sequencer length
        int stepsPerPattern = 16;
        int currentLength = helmSequencer.length;
        int currentPatternCount = currentLength / stepsPerPattern;

        // Calculate the new length by adding a pattern
        int newLength = (currentPatternCount + 1) * stepsPerPattern;

        // Initialize and set length for each sequencer
        if (helmSequencer != null)
        {
            // If it's the first pattern, initialize or copy default steps
            CopySteps(helmSequencer, newLength, currentPatternCount == 0);
            helmSequencer.length = newLength;
        }

        if (sampleSequencer != null)
        {
            CopySteps(sampleSequencer, newLength, currentPatternCount == 0);
            sampleSequencer.length = newLength;
        }

        if (drumSequencer != null)
        {
            CopySteps(drumSequencer, newLength, currentPatternCount == 0);
            drumSequencer.length = newLength;
        }

        // Update the pattern count
        patternCount = currentPatternCount + 1;

        // Optionally, store the length for future reference if needed
        sequencersLength = newLength;

        // Update UI and save patterns
        UpdateBoardManager();
        UpdatePatternDisplay();
        SavePatterns();

        Debug.Log($"Pattern created with length {newLength}. Total patterns: {patternCount}");
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
        //clock.Reset();
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

        if (PatternManager.Instance.sequencersLength / 16 == 1) {
            return;
        }

        ClearLast16Steps();

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

        currentPatternIndex = patternCount;

        // Update UI and save the patterns
        UpdateBoardManager();
        UpdatePatternDisplay(); // Update UI
        SavePatterns(); // Save the updated list of patterns
    }

    public void UpdateBoardManager()
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
            boardManager.ResetBoard();
        }
    }

    public void UpdateBoardManageForSamples()
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
        try
        {
            // Ensure MultipleAudioLoader and other required components are available
            if (MultipleAudioLoader.Instance == null || clock == null)
            {
                Debug.LogError("MultipleAudioLoader or Clock is not assigned.");
                return;
            }

            // Retrieve the current song file name
            string songFileName = null;
            if (MultipleAudioLoader.Instance.clipFileNames != null && MultipleAudioLoader.Instance.currentIndex >= 0)
            {
                songFileName = MultipleAudioLoader.Instance.clipFileNames[MultipleAudioLoader.Instance.currentIndex];
            }
            else
            {
                Debug.LogWarning("No valid song file name found. Saving patterns without a song file.");
            }

            // Create a new ProjectData instance to hold all pattern data
            ProjectData projectData = new ProjectData
            {
                HelmPattern = GetPatternDataForSequencer(PatternManager.Instance.sequencerPrefab),
                SamplePattern = GetPatternDataForSequencer(PatternManager.Instance.sampleSequencerPrefab),
                DrumPattern = GetPatternDataForSequencer(PatternManager.Instance.drumSequencerPrefab),
                HelmSequencerLength = GetSequencerLength(PatternManager.Instance.sequencerPrefab),
                SampleSequencerLength = GetSequencerLength(PatternManager.Instance.sampleSequencerPrefab),
                DrumSequencerLength = GetSequencerLength(PatternManager.Instance.drumSequencerPrefab),
                songFileName = songFileName, // Use songFileName instead of songIndex
                bpm = clock.bpm,
                timestamps = chopButton.GetComponent<Chop>()?.timestamps ?? new List<float>(), // Handle null Chop component
                patch = PatternManager.Instance.sequencerPrefab.GetComponent<HelmPatchController>()?.currentPatchIndex ?? -1,
            };

            // Save all pattern data to file
            DataManager.SaveProjectToFile(projectData);
            Debug.Log("Patterns saved to file.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving patterns: {ex.Message}");
        }
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

    private string CreateNewProjectFile(string customName)
    {
        try
        {
            // Generate a unique filename for the new project using the custom name
            string newFilename = UserGenerateUniqueFilename(customName);

            // Get the current song file name
            string songFileName = null;
            if (MultipleAudioLoader.Instance != null && MultipleAudioLoader.Instance.clipFileNames != null && MultipleAudioLoader.Instance.clipFileNames.Count > 0)
            {
                songFileName = MultipleAudioLoader.Instance.clipFileNames[0]; // Default to the first song in the list for a new project
            }
            else
            {
                Debug.LogWarning("No valid song file names found. Creating project without a song file.");
            }

            // Create a new ProjectData object with default values
            ProjectData newProjectData = new ProjectData
            {
                HelmPattern = null,
                SamplePattern = null,
                DrumPattern = null,
                HelmSequencerLength = 0, // Default length for new project
                SampleSequencerLength = 0,
                DrumSequencerLength = 0,
                songFileName = songFileName, // Use songFileName instead of songIndex
                bpm = 120f, // Default BPM for new project
                timestamps = new List<float>(), // Initialize an empty list for timestamps
                patch = PatternManager.Instance.sequencerPrefab.GetComponent<HelmPatchController>()?.currentPatchIndex ?? -1, // Use current patch index or default to -1
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
        catch (Exception ex)
        {
            Debug.LogError($"Error creating new project file: {ex.Message}");
            return null; // Return null if there is an error
        }
    }

    public void CreateAndLoadNewProject(string customName)
    {
        // Ensure the custom name is not empty
        if (string.IsNullOrWhiteSpace(customName))
        {
            Debug.LogError("Project name cannot be empty!");
            return;
        }

        // Create a new project file with the custom name and get the filename
        string newFilename = CreateNewProjectFile(customName);
        LastProjectFilename = newFilename;

        // Load the newly created project
        LoadProject(newFilename);

        sequencerPrefab.GetComponent<HelmSequencer>().length = 16;
        sampleSequencerPrefab.GetComponent<SampleSequencer>().length = 16;
        drumSequencerPrefab.GetComponent<SampleSequencer>().length = 16;

        currentPatternIndex = 1;

        // Optionally, set all sequencers to loop and update the display    
        SetAllSequencersLoop(true); // Ensure all sequencers have looping enabled
        UpdatePatternDisplay(); // Update UI to reflect loaded patterns

        Debug.Log($"New project created and loaded: {newFilename}");

        sequencersLength = 16;

        MultipleAudioLoader.Instance.currentIndex = 0;

        if (MultipleAudioLoader.Instance.clipFileNames != null && MultipleAudioLoader.Instance.clipFileNames.Count > 0)
        {
            string songToLoad = MultipleAudioLoader.Instance.clipFileNames[0];
            // Proceed with loading the song
            if (!string.IsNullOrEmpty(songToLoad))
            {
                StartCoroutine(MultipleAudioLoader.Instance.LoadClip(songToLoad));
            }
            else
            {
                Debug.LogWarning("No song to load; skipping song load process.");
            }            
        }
        else
        {
            Debug.LogError("clipFileNames list is empty or null.");
        }

        SaveOver();
        UpdateProjectFileText();
        boardManager.ResetBoard();
    }

    public void SaveProject(string filename)
    {
        try
        {
            // Check for null dependencies
            if (PatternManager.Instance == null || clock == null)
            {
                Debug.LogError("PatternManager or Clock is not assigned!");
                return;
            }

            if (sequencerPrefab == null || sampleSequencerPrefab == null || drumSequencerPrefab == null)
            {
                Debug.LogError("One or more sequencer prefabs are not assigned!");
                return;
            }

            if (chopButton == null || chopButton.GetComponent<Chop>() == null)
            {
                Debug.LogError("ChopButton or Chop component is not assigned!");
                return;
            }

            // Calculate the pitch based on AudioBPMAdjuster
            float pitch = 1.0f;
            if (AudioBPMAdjuster.Instance != null && AudioBPMAdjuster.Instance.originalBPM > 0)
            {
                pitch = AudioBPMAdjuster.Instance.targetBPM / AudioBPMAdjuster.Instance.originalBPM;
            }

            // Create a new ProjectData object and populate its fields
            ProjectData projectData = new ProjectData
            {
                HelmPattern = GetPatternDataForSequencer(sequencerPrefab),
                SamplePattern = GetPatternDataForSequencer(sampleSequencerPrefab),
                DrumPattern = GetPatternDataForSequencer(drumSequencerPrefab),
                songFileName = MultipleAudioLoader.Instance != null && MultipleAudioLoader.Instance.clipFileNames.Count > 0 
                               ? MultipleAudioLoader.Instance.clipFileNames[MultipleAudioLoader.Instance.currentIndex] 
                               : null,
                bpm = clock.bpm,
                timestamps = chopButton.GetComponent<Chop>().timestamps,
                HelmSequencerLength = GetSequencerLength(sequencerPrefab),
                SampleSequencerLength = GetSequencerLength(sampleSequencerPrefab),
                DrumSequencerLength = GetSequencerLength(drumSequencerPrefab),
                patch = sequencerPrefab.GetComponent<HelmPatchController>()?.currentPatchIndex ?? -1,
                sliderValues = sequencerPrefab.GetComponent<HelmPatchController>()?.GetAllSliderValues(),
                pitch = pitch // Save pitch
            };

            // Serialize to JSON
            string json = JsonUtility.ToJson(projectData, true);
            string projectPath = Path.Combine(Application.persistentDataPath, filename);
            File.WriteAllText(projectPath, json);

            Debug.Log($"Project saved successfully to: {projectPath}");
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
                    var helmSequencer = sequencerPrefab.GetComponent<HelmSequencer>();
                    if (helmSequencer != null)
                    {
                        helmSequencer.Clear();
                        helmSequencer.length = projectData.HelmSequencerLength;
                        PopulateSequencerFromPatternData(helmSequencer, projectData.HelmPattern);
                        sequencersLength = helmSequencer.length;
                        UpdatePatternDisplay();
                        Debug.Log($"Loaded HelmSequencer pattern: {projectData.HelmPattern.Type}");
                    }
                }

                // Load SampleSequencer pattern
                if (projectData.SamplePattern != null)
                {
                    var sampleSequencer = sampleSequencerPrefab.GetComponent<SampleSequencer>();
                    if (sampleSequencer != null)
                    {
                        sampleSequencer.Clear();
                        sampleSequencer.length = projectData.SampleSequencerLength;
                        PopulateSampleSequencerFromPatternData(sampleSequencer, projectData.SamplePattern);
                        sequencersLength = sampleSequencer.length;
                        UpdatePatternDisplay();
                        Debug.Log($"Loaded SampleSequencer pattern: {projectData.SamplePattern.Type}");
                    }
                }

                // Load DrumSequencer pattern
                if (projectData.DrumPattern != null)
                {
                    var drumSequencer = drumSequencerPrefab.GetComponent<SampleSequencer>();
                    if (drumSequencer != null)
                    {
                        drumSequencer.Clear();
                        drumSequencer.length = projectData.DrumSequencerLength;
                        PopulateDrumSequencerFromPatternData(drumSequencer, projectData.DrumPattern);
                        sequencersLength = drumSequencer.length;
                        UpdatePatternDisplay();
                        Debug.Log($"Loaded DrumSequencer pattern: {projectData.DrumPattern.Type}");
                    }
                }

                // Restore the song file name
                if (!string.IsNullOrEmpty(projectData.songFileName))
                {
                    int songIndex = MultipleAudioLoader.Instance.clipFileNames.IndexOf(projectData.songFileName);
                    if (songIndex >= 0)
                    {
                        // Song file exists, load it
                        MultipleAudioLoader.Instance.currentIndex = songIndex;
                        StartCoroutine(MultipleAudioLoader.Instance.LoadClip(projectData.songFileName));
                        GameObject.Find("ComponentButton").GetComponent<ComponentButton>().ShowCorrectPanel(false);
                    }
                    else
                    {
                        // Song file is missing
                        Debug.LogWarning($"Song file '{projectData.songFileName}' not found in clip list.");

                        // Show the music player panel explicitly
                        GameObject.Find("ComponentButton").GetComponent<ComponentButton>().ShowCorrectPanel(true);

                        // Display a missing song message
                        StartCoroutine(ShowMissingSongMessageWithDelay("Sample does not exist anymore. Another sample in folder used!", 0.1f));
                    }
                }
                else
                {
                    // Handle the case where projectData.songFileName is null or empty
                    Debug.LogWarning("Project data song file name is empty or null.");

                    // Show the music player panel explicitly
                    GameObject.Find("ComponentButton").GetComponent<ComponentButton>().ShowCorrectPanel(true);

                    // Display a missing song message
                    StartCoroutine(ShowMissingSongMessageWithDelay("No song file specified in the project data!", 0.1f));
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

                // Restore pitch
                if (projectData.pitch > 0 && AudioBPMAdjuster.Instance != null && AudioBPMAdjuster.Instance.originalBPM > 0)
                {
                    AudioBPMAdjuster.Instance.targetBPM = AudioBPMAdjuster.Instance.originalBPM * projectData.pitch;
                    AudioBPMAdjuster.Instance.AdjustPlaybackSpeed(); // Apply the pitch
                    var pitchSlider = GameObject.Find("PitchBPM")?.GetComponent<Slider>();
                    if (pitchSlider != null)
                    {
                        AudioBPMAdjuster.Instance.InitializeSlider();
                    }
                }

                // Restore timestamps
                var chopComponent = chopButton.GetComponent<Chop>();
                if (chopComponent != null)
                {
                    chopComponent.timestamps = projectData.timestamps;
                }

                // Restore patch and slider values
                var helmPatchController = sequencerPrefab.GetComponent<HelmPatchController>();
                if (helmPatchController != null)
                {
                    helmPatchController.currentPatchIndex = projectData.patch;
                    helmPatchController.LoadCurrentPatch();

                    if (projectData.sliderValues != null && projectData.sliderValues.Count > 0)
                    {
                        helmPatchController.SetAllSliderValues(projectData.sliderValues);
                    }
                }

                currentPatternIndex = 1;

                Debug.Log($"Project loaded from file: {filename}");
                UpdatePatternDisplay();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading project: {ex.Message}");
            }
        }
    }

    private bool IsSampleMissing(string songFileName)
    {
        if (string.IsNullOrEmpty(songFileName))
        {
            Debug.LogWarning("Song file name is empty or null.");
            return true; // Treat as missing
        }

        if (MultipleAudioLoader.Instance == null || MultipleAudioLoader.Instance.clipFileNames == null)
        {
            Debug.LogError("MultipleAudioLoader or clipFileNames is null.");
            return true; // Treat as missing
        }

        // Check if the song file name exists in the list and also verify the actual file's existence
        bool existsInList = MultipleAudioLoader.Instance.clipFileNames.Contains(songFileName);
        string filePath = Path.Combine(Application.persistentDataPath, songFileName);
        bool fileExists = File.Exists(filePath);

        if (!existsInList || !fileExists)
        {
            Debug.LogWarning($"Song file '{songFileName}' is missing or does not exist on disk.");
            return true;
        }

        return false; // File exists and is valid
    }


    private IEnumerator ShowMissingSongMessageWithDelay(string message, float delay)
    {
        // Find the 'SongFileName' GameObject
        GameObject songFileNameObject = GameObject.Find("SongFileName");

        if (songFileNameObject != null)
        {
            // Get the TextMeshProUGUI component
            TextMeshProUGUI songFileText = songFileNameObject.GetComponent<TextMeshProUGUI>();

            if (songFileText != null)
            {
                // Wait for the specified delay
                yield return new WaitForSeconds(delay);

                // Display the missing song message
                songFileText.text = message;

                Debug.Log($"Displayed missing song message: {message}");
            }
            else
            {
                Debug.LogError("TextMeshProUGUI component is missing on 'SongFileName' GameObject.");
            }
        }
        else
        {
            Debug.LogError("GameObject 'SongFileName' not found.");
        }
    }


    private void ExportMidi(string filename)
    {
        try
        {
            // Ensure dependencies are valid
            if (clock == null || sequencerPrefab == null || sampleSequencerPrefab == null || drumSequencerPrefab == null)
            {
                Debug.LogError("One or more dependencies are null. Cannot export MIDI.");
                return;
            }

            // Extract data from Unity objects
            var bpm = clock.bpm;
            var helmNotes = sequencerPrefab.GetComponent<HelmSequencer>()?.GetAllNotes();
            var sampleNotes = sampleSequencerPrefab.GetComponent<SampleSequencer>()?.GetAllNotes();
            var drumNotes = drumSequencerPrefab.GetComponent<SampleSequencer>()?.GetAllNotes();

            if (helmNotes == null || sampleNotes == null || drumNotes == null)
            {
                Debug.LogError("Failed to retrieve notes from one or more sequencers.");
                return;
            }

            // Convert notes to simple data structures (if needed)
            var helmNoteData = ConvertNotesToPlainData(helmNotes);
            var sampleNoteData = ConvertNotesToPlainData(sampleNotes);
            var drumNoteData = ConvertNotesToPlainData(drumNotes);

            // Pass the data to the MIDI exporter
            var midiExporter = new MidiExporter();
            midiExporter.ExportMidiWithSanford(
                filename,
                bpm,
                helmNoteData,
                sampleNoteData,
                drumNoteData
            );

            Debug.Log($"MIDI file exported successfully as: {filename}.mid");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error exporting MIDI: {ex.Message}");
        }
    }

    private List<kMidiNote> ConvertNotesToPlainData(IEnumerable<AudioHelm.Note> notes)
    {
        var midiNotes = new List<kMidiNote>();
        foreach (var note in notes)
        {
            midiNotes.Add(new kMidiNote
            {
                Start = note.start,
                End = note.end,
                Note = note.note,
                Velocity = note.velocity
            });
        }
        return midiNotes;
    }



    private void SaveProjectAndExportMidi(string filename)
    {
        SaveProject(filename); // Save the project as JSON
        ExportMidi(filename);  // Export the project as MIDI
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

    public string UserGenerateUniqueFilename(string customName)
    {
        // Define the filename pattern to search for
        string pattern = customName + "*.json";
        string[] existingFiles = Directory.GetFiles(Application.persistentDataPath, pattern);

        int highestNumber = 0;

        // Iterate through existing files to find the highest number
        foreach (string file in existingFiles)
        {
            // Extract the number from the filename
            string filename = Path.GetFileNameWithoutExtension(file);
            string numberPart = filename.Substring(customName.Length);

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
        Debug.Log($"Generated new filename: {customName}{newNumber}.json");

        return $"{customName}.json";
    }

    public string[] GetSortedProjectFilesArray()
    {
        // Get all files in the directory
        string[] existingFiles = Directory.GetFiles(Application.persistentDataPath, "*.json");

        // Sort files alphabetically by their names
        var sortedFiles = existingFiles
            .OrderBy(file => Path.GetFileName(file)) // Sort by the filename
            .ToArray(); // Convert to string[]

        return sortedFiles;
    }



    // Method to get the next project file from the sorted array
    public void LoadNextProject()
    {
        // Get the sorted list of project files
        string[] sortedFiles = GetSortedProjectFilesArray();

        // Check if there are any files
        if (sortedFiles.Length == 0)
        {
            Debug.LogWarning("No project files found.");
            return;
        }

        // Find the index of the current project file
        int currentIndex = Array.IndexOf(sortedFiles, Path.Combine(Application.persistentDataPath, currentProjectFilename));

        // Handle case where current filename is not found
        if (currentIndex == -1)
        {
            Debug.LogWarning($"Current project filename '{currentProjectFilename}' not found in sorted files. Loading the first project.");
            currentIndex = -1; // Force to load the first project
        }

        // Determine the next index
        int nextIndex = (currentIndex + 1) % sortedFiles.Length;

        // Get the next filename from the sorted list
        string nextFilePath = sortedFiles[nextIndex];
        string nextFilename = Path.GetFileName(nextFilePath);

        // Load the next project if the filename is valid
        if (!string.IsNullOrEmpty(nextFilename))
        {
            LoadProject(nextFilename);
            
            // Update the current project filename and last project filename
            currentProjectFilename = nextFilename;
            LastProjectFilename = nextFilename;

            SetAllSequencersLoop(true); // Ensure all sequencers have looping enabled
            UpdatePatternDisplay(); // Update UI

            // Update project file text with the new filename
            UpdateProjectFileText();
            
            Debug.Log($"Loaded next project: {nextFilename}");
        }
        else
        {
            Debug.LogWarning("Next filename is empty or invalid.");
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
        string projectPath = Path.Combine(Application.persistentDataPath, LastProjectFilename);
        string midiPath = Path.Combine(Application.persistentDataPath, Path.GetFileNameWithoutExtension(LastProjectFilename) + ".json.mid");

        // Delete the project file
        if (File.Exists(projectPath))
        {
            try
            {
                File.Delete(projectPath);
                Debug.Log($"Project file deleted: {LastProjectFilename}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error deleting project file: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Project file not found: {projectPath}. Skipping project deletion.");
        }

        // Delete the corresponding MIDI file
        if (File.Exists(midiPath))
        {
            try
            {
                File.Delete(midiPath);
                Debug.Log($"MIDI file deleted: {Path.GetFileName(midiPath)}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error deleting MIDI file: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"MIDI file not found: {midiPath}. Skipping MIDI deletion.");
        }

        // Clear the current project data
        lastAccessedFile = null;
        LastProjectFilename = null;

        sequencersLength = 1;
        currentPatternIndex = 0;

        sequencerPrefab.GetComponent<HelmSequencer>().Clear();
        sampleSequencerPrefab.GetComponent<SampleSequencer>().Clear();
        drumSequencerPrefab.GetComponent<SampleSequencer>().Clear();

        sequencerPrefab.GetComponent<HelmSequencer>().length = 16;
        sampleSequencerPrefab.GetComponent<SampleSequencer>().length = 16;
        drumSequencerPrefab.GetComponent<SampleSequencer>().length = 16;

        // Update the UI to reflect the deletion
        UpdatePatternDisplay();
        UpdateProjectFileText();

        Debug.Log("Current project data cleared and UI updated.");
    }

    private void UpdateProjectFileText()
    {
        if (projectFileText != null)
        {
            Debug.Log($"Updating project file text to: {LastProjectFilename}");
            projectFileText.text = $"{LastProjectFilename}";

            // Additional Debugging: Check if the text has been updated successfully
            Debug.Log($"TextMeshProUGUI text after update: {projectFileText.text}");
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component is not assigned.");
        }
    }

    public void ClearCurrentPattern()
    {
        if (currentPatternIndex < 0)
        {
            Debug.LogWarning("No valid currentPatternIndex to clear.");
            return;
        }

        // Set the clearing flag to true
        isClearingPattern = true;

        // Define the range of steps based on the currentPatternIndex
        int stepsPerPattern = 16; // Assuming each pattern is 16 steps long
        int startStep = (currentPatternIndex - 1) * stepsPerPattern;
        int endStep = startStep + stepsPerPattern;

        Debug.Log($"Starting to clear current pattern. Group: {componentButton.GetComponent<ComponentButton>()?.currentPatternGroup}, Start Step: {startStep}, End Step: {endStep}");

        // Get the current pattern group (Helm, Samples, or Drums) from the ComponentButton script
        var componentButtonScript = componentButton.GetComponent<ComponentButton>();
        if (componentButtonScript == null)
        {
            Debug.LogError("ComponentButton script not found on componentButton.");
            isClearingPattern = false; // Reset flag if there's an error
            return;
        }

        // Check which group is currently visible
        int currentPatternGroup = componentButtonScript.currentPatternGroup;
        Debug.Log($"Current Pattern Group: {currentPatternGroup}");

        // Handle clearing notes for the current pattern group
        switch (currentPatternGroup)
        {
            case 1: // Helm (keys)
                Debug.Log("Clearing notes for Helm/Keys.");
                ClearVisibleNotesOnSequencer(sequencerPrefab.GetComponent<HelmSequencer>(), startStep, endStep, "Helm");
                break;

            case 2: // Samples
                Debug.Log("Clearing notes for Samples.");
                ClearVisibleNotesOnSequencer(sampleSequencerPrefab.GetComponent<SampleSequencer>(), startStep, endStep, "Samples");
                break;

            case 3: // Drums
                Debug.Log("Clearing notes for Drums (Group 3).");
                ClearVisibleNotesOnSequencer(drumSequencerPrefab.GetComponent<SampleSequencer>(), startStep, endStep, "Drums");
                break;

            case 0: // Drums (for group 0 as well)
                Debug.Log("Clearing notes for Drums (Group 0).");
                ClearVisibleNotesOnSequencer(drumSequencerPrefab.GetComponent<SampleSequencer>(), startStep, endStep, "Drums");
                break;

            default:
                Debug.LogWarning($"No valid pattern group selected for clearing. Group: {currentPatternGroup}");
                break;
        }

        // Save the cleared pattern
        Debug.Log("Saving patterns after clearing.");
        SavePatterns();

        // Update the UI to reflect the cleared notes
        Debug.Log("Updating pattern display after clearing.");
        UpdatePatternDisplay();

        // Reset the clearing flag
        isClearingPattern = false;

        Debug.Log($"Successfully cleared notes for visible group {currentPatternGroup} between steps {startStep} and {endStep}.");
    }

    private void ClearVisibleNotesOnSequencer(AudioHelm.Sequencer sequencer, int startStep, int endStep, string groupName)
    {
        if (sequencer == null)
        {
            Debug.LogWarning($"{groupName} sequencer is null, cannot clear visible notes.");
            return;
        }

        Debug.Log($"Starting to clear visible notes for group: {groupName}, startStep: {startStep}, endStep: {endStep}");

        // Get visible cells
        var visibleCells = GetVisibleCells();
        Debug.Log($"Visible cells count: {visibleCells.Count}");

        if (visibleCells == null || visibleCells.Count == 0)
        {
            Debug.LogWarning("No visible cells found on the board manager.");
            return;
        }

        // Clear notes based on the group name
        switch (groupName)
        {
            case "Helm": // Keys
                foreach (var cell in visibleCells)
                {
                    if (cell.CurrentSprite == null)
                    {
                        Debug.LogWarning("Cell has no CurrentSprite. Skipping.");
                        continue;
                    }

                    int noteValue = KeyManager.Instance.GetMidiNoteForSprite(cell.CurrentSprite.name);
                    Debug.Log($"Processing Helm note. Cell: {cell.name}, NoteValue: {noteValue}");

                    if (noteValue >= 0)
                    {
                        sequencer.RemoveNotesInRange(noteValue, startStep, endStep);
                        KeyManager.Instance.RemoveKeyTileData(cell.CurrentSprite, (int)cell.step);
                        Debug.Log($"Cleared Helm key note {noteValue} from step {startStep} to {endStep}.");
                    }
                }
                break;

            case "Samples": // Sample Manager
                foreach (var cell in visibleCells)
                {
                    if (cell.CurrentSprite == null)
                    {
                        Debug.LogWarning("Cell has no CurrentSprite. Skipping.");
                        continue;
                    }

                    int noteValue = SampleManager.Instance.GetMidiNoteForSprite(cell.CurrentSprite.name);
                    Debug.Log($"Processing Sample note. Cell: {cell.name}, NoteValue: {noteValue}");

                    if (noteValue >= 0)
                    {
                        sequencer.RemoveNotesInRange(noteValue, startStep, endStep);
                        SampleManager.Instance.RemoveSampleTileData(cell.CurrentSprite, (int)cell.step);
                        Debug.Log($"Cleared Sample note {noteValue} from step {startStep} to {endStep}.");
                    }
                }
                break;

            case "Drums": // Drum Manager
                foreach (var cell in visibleCells)
                {
                    if (cell.CurrentSprite == null)
                    {
                        Debug.LogWarning("Cell has no CurrentSprite. Skipping.");
                        continue;
                    }

                    int noteValue = PadManager.Instance.GetMidiNoteForSprite(cell.CurrentSprite.name);
                    Debug.Log($"Processing Drum note. Cell: {cell.name}, NoteValue: {noteValue}");

                    if (noteValue >= 0)
                    {
                        sequencer.RemoveNotesInRange(noteValue, startStep, endStep);
                        PadManager.Instance.RemovePadTileData(cell.CurrentSprite, (int)cell.step);
                        Debug.Log($"Cleared Drum note {noteValue} from step {startStep} to {endStep}.");
                    }
                }
                break;

            default:
                Debug.LogWarning($"No valid group name provided: {groupName}");
                break;
        }

        // Refresh the board visually
        Debug.Log("Resetting the board after clearing visible notes.");
        StartCoroutine(DelayedResetBoard());
    }

    IEnumerator DelayedResetBoard()
    {
        yield return new WaitForSeconds(0.1f); // Wait for one frame
        BoardManager.Instance.ResetBoard();
    }

    public List<Cell> GetVisibleCells()
    {
        List<Cell> visibleCells = new List<Cell>();

        // Loop through all cells
        foreach (Cell cell in BoardManager.Instance.boardCells)
        {
            if (cell == null)
            {
                Debug.LogWarning("Null cell found. Skipping.");
                continue;
            }

            var spriteRenderer = cell.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"Cell {cell.name} has no SpriteRenderer. Skipping.");
                continue;
            }

            if (spriteRenderer.sprite == null)
            {
                Debug.LogWarning($"Cell {cell.name} has a SpriteRenderer but no sprite assigned. Skipping.");
                continue;
            }

            string spriteName = spriteRenderer.sprite.name;
            Debug.Log($"Checking cell {cell.name} with sprite {spriteName}");

            if (spriteName != "cell_default")
            {
                visibleCells.Add(cell);
                Debug.Log($"Cell {cell.name} added to visible cells.");
            }
        }


        Debug.Log($"Found {visibleCells.Count} visible cells.");
        return visibleCells;
    }

    public void ClearLast16Steps()
    {
        if (currentPatternIndex < 0)
        {
            Debug.LogWarning("No valid currentPatternIndex to clear.");
            return;
        }

        // Define the range of steps to clear
        int stepsToClear = 16;  // Number of steps to clear from the end

        // Get the current pattern to determine its length
        var helmSequencer = sequencerPrefab.GetComponent<HelmSequencer>();
        if (helmSequencer != null)
        {
            int totalSteps = helmSequencer.length; // Total steps in the sequencer

            // Calculate the start step for clearing the last section
            int startStep = Mathf.Max(0, totalSteps - stepsToClear); // Ensure startStep is not less than 0
            int endStep = totalSteps; // End step is the total number of steps

            // Clear the notes in the specified range for HelmSequencer
            for (int noteValue = 0; noteValue <= 127; noteValue++)
            {
                helmSequencer.RemoveNotesInRange(noteValue, startStep, endStep);
            }
            Debug.Log($"Helm pattern notes cleared from step {startStep} to {endStep}.");
        }
        else
        {
            Debug.LogWarning("HelmSequencer component is not assigned.");
        }

        // Clear the notes in the specified range for SampleSequencer
        var sampleSequencer = sampleSequencerPrefab.GetComponent<SampleSequencer>();
        if (sampleSequencer != null)
        {
            int totalSteps = sampleSequencer.length; // Total steps in the sequencer
            int startStep = Mathf.Max(0, totalSteps - stepsToClear);
            int endStep = totalSteps;

            for (int noteValue = 0; noteValue <= 127; noteValue++)
            {
                sampleSequencer.RemoveNotesInRange(noteValue, startStep, endStep);
            }
            Debug.Log($"Sample pattern notes cleared from step {startStep} to {endStep}.");
        }
        else
        {
            Debug.LogWarning("SampleSequencer component is not assigned.");
        }

        // Clear the notes in the specified range for DrumSequencer
        var drumSequencer = drumSequencerPrefab.GetComponent<SampleSequencer>();
        if (drumSequencer != null)
        {
            int totalSteps = drumSequencer.length; // Total steps in the sequencer
            int startStep = Mathf.Max(0, totalSteps - stepsToClear);
            int endStep = totalSteps;

            for (int noteValue = 0; noteValue <= 127; noteValue++)
            {
                drumSequencer.RemoveNotesInRange(noteValue, startStep, endStep);
            }
            Debug.Log($"Drum pattern notes cleared from step {startStep} to {endStep}.");
        }
        else
        {
            Debug.LogWarning("DrumSequencer component is not assigned.");
        }

        // Reset the board and update the UI to reflect the cleared sections
        BoardManager.Instance.ResetBoard();
        UpdatePatternDisplay();
        SavePatterns();

        Debug.Log("Board reset, pattern updated, and patterns saved.");
    }

    public void SaveOver()
    {
        if (!string.IsNullOrEmpty(LastProjectFilename))
        {
            Debug.Log("About to save.");
            SaveProject(LastProjectFilename);
            Debug.Log("Project Saved.");

            // Use the project filename (without extension) as the base filename.
            string baseFileName = Path.GetFileNameWithoutExtension(LastProjectFilename);

            if (songClip != null && chopComponent != null)
            {
                // Start the coroutine to save chops asynchronously.
                StartCoroutine(ChopSaver.SaveRenderedChopsCoroutine(songClip, chopComponent.timestamps, baseFileName));
            }
            else
            {
                Debug.LogWarning("Either songClip or chopComponent is not assigned. Rendered chops not saved.");
            }
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
    public string songFileName; // Replacing songIndex with songFileName
    public float bpm;
    public List<float> timestamps;
    public int patch;
    public int HelmSequencerLength;    
    public int SampleSequencerLength;    
    public int DrumSequencerLength;

    public List<float> sliderValues; // List to store slider values

    public float pitch;

}
