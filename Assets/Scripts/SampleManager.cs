using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using AudioHelm;

public class SampleManager : MonoBehaviour
{
    public static SampleManager Instance  { get; private set; }   // Singleton instance

    public GameObject samplePrefab;         // Reference to the sample prefab
    public Sprite[] samples;                // Array of sprites to assign to each sample

    public Sprite currentSample;            // Current sample tracked by SampleManager
    private Sprite lastClickedSample;       // Last clicked sample

    public Sprite currentPadSprite;         // Current pad sprite tracked by SampleManager
    public Sprite lastClickedPadSprite;     // Last clicked pad sprite

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>(); // Dictionary to store original scales of samples
    private Dictionary<GameObject, Vector3> padOriginalScales = new Dictionary<GameObject, Vector3>(); // Dictionary to store original scales of pads

    private AudioSource audioSource;        // AudioSource to play audio clips
    public Chop chopScript;                 // Reference to the Chop script

    public float bpm = 120f;                // Beats per minute, adjust as needed

    private float timeToPlayNextSample = -1f; // Time to play the next sample

    private AudioClip currentClip;           // Currently playing audio clip
    private float playbackStartTime;         // Time when the playback starts
    public int midiNote;
    private Cell[,] boardCells; // 2D array to store references to all board cells

    private Dictionary<int, Sprite> midiNoteToSpriteMap = new Dictionary<int, Sprite>();
    private bool isPlayingSample = false;    // Flag to check if a sample is currently being played

    public Dictionary<string, List<int>> sampleTileData = new Dictionary<string, List<int>>(); // Changed to use string keys

    public Sprite defaultSprite;

    public SampleSequencer sampleSequencer;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Debug.Log("SampleManager Start method called");

        // Validate samplePrefab and samples array
        if (!ValidateInitialSettings()) return;

        // Add an AudioSource component to the SampleManager
        audioSource = sampleSequencer.GetComponent<AudioSource>();

        // Ensure the Chop script is assigned
        if (chopScript == null)
        {
            Debug.LogError("Chop script is not assigned.");
            return;
        }

        // Debugging: Print out timestamps and sample names
        Debug.Log("Printing timestamps:");
        for (int i = 0; i < chopScript.timestamps.Count; i++)
        {
            Debug.Log($"Timestamp for sample {i + 1}: {chopScript.timestamps[i]}");
        }

        // Ensure the AudioSource is assigned
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is not assigned.");
        }

        // Generate samples
        GenerateSamples();

        // Initialize MIDI Note to Sprite mapping (adjust as needed)
        InitializeMidiNoteMappings(); 

        StartCoroutine(LoadSampleTileDataAfterDelay(0.2f));       
    }

    private IEnumerator LoadSampleTileDataAfterDelay(float delaySeconds)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delaySeconds);

        // Load the tile data from file
        sampleTileData = DataManager.LoadSampleTileDataFromFile();

        // Log the loaded tile data
        Debug.Log("Loaded sampleTileData from file:");
        foreach (var entry in sampleTileData)
        {
            Debug.Log($"Sprite = {entry.Key}, Steps = {string.Join(", ", entry.Value)}");
        }
    }


    private void InitializeMidiNoteMappings()
    {
        // Example mapping - adjust according to your needs
        for (int i = 0; i < samples.Length; i++)
        {
            midiNoteToSpriteMap[60 + i] = samples[i]; // Assuming MIDI notes start from 60
        }
    }

    public void OnNoteOn()
    {
        bool foundAnySample = false; // To track if any sample was found and played

        // Use SampleSequencer instance directly
        SampleSequencer sequencer = PatternManager.Instance.GetActiveSampleSequencer();

        if (sequencer == null)
        {
            Debug.LogError("SampleSequencer not found.");
            return;
        }

        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                if (sequencer.NoteExistsInRange(75 - i, j, j + 1) && sequencer.currentIndex == j)
                {
                    int midiNote = 75 - i;

                    if (midiNoteToSpriteMap.TryGetValue(midiNote, out Sprite sampleSprite))
                    {
                        // Find the sample GameObject based on the sprite
                        foreach (GameObject sample in originalScales.Keys)
                        {
                            if (sample.GetComponent<SpriteRenderer>().sprite == sampleSprite)
                            {
                                // Trigger the click event to play the sample
                                OnSampleEvent(sample);
                                foundAnySample = true; // Set flag indicating a sample was found
                            }
                        }

                        if (!foundAnySample)
                        {
                            Debug.LogWarning($"No sample found for MIDI note {midiNote}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"No sprite mapped for MIDI note {midiNote}");
                    }
                }
            }
        }

        if (!foundAnySample)
        {
            Debug.LogWarning("No samples were found or played for the current NoteOn event.");
        }
    }

    private bool ValidateInitialSettings()
    {
        if (samplePrefab == null)
        {
            Debug.LogError("Sample prefab is not assigned in SampleManager.");
            return false;
        }

        if (samples == null || samples.Length == 0)
        {
            Debug.LogError("No samples provided in SampleManager.");
            return false;
        }

        return true;
    }

    private void GenerateSamples()
    {
        int numSamplesPerRow = 8; // Number of samples per row
        int numRows = 8; // Number of rows

        // Ensure we have enough samples to fill the grid
        int numSamplesToCreate = Mathf.Min(numRows * numSamplesPerRow, samples.Length);

        // Initialize the boardCells 2D array
        boardCells = new Cell[8, 8];

        // Loop to create samples
        for (int i = 0; i < numSamplesToCreate; i++)
        {
            // Calculate position for the new sample
            int row = i / numSamplesPerRow;
            int col = i % numSamplesPerRow;
            Vector3 samplePosition = new Vector3(col, (-row) - 4, 0); // Adjust Y position as needed

            // Instantiate a new sample from the prefab at the calculated position
            GameObject newSample = Instantiate(samplePrefab, samplePosition, Quaternion.identity);

            // Optionally, parent the new sample under the SampleManager GameObject for organization
            newSample.transform.parent = transform;

            int midiNote = i + 60;

            // Set the sprite for the sample using the samples array
            if (!SetSampleSprite(newSample, i)) continue;

            // Store the original scale of the sample
            originalScales[newSample] = newSample.transform.localScale;

            // Attach click handler to the sample
            SampleClickHandler sampleClickHandler = newSample.AddComponent<SampleClickHandler>();
            sampleClickHandler.Initialize(this, newSample, samples[i], midiNote);

            // Change name of sample
            newSample.name = samples[i].name;
        }
    }

    private bool SetSampleSprite(GameObject sampleObject, int index)
    {
        SpriteRenderer spriteRenderer = sampleObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && index < samples.Length)
        {
            spriteRenderer.sprite = samples[index];
            return true;
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found on sample prefab or samples array index out of bounds.");
            return false;
        }
    }

    public void OnSampleClicked(GameObject clickedSample)
    {
        midiNote = clickedSample.GetComponent<SampleClickHandler>().midiNote;

        lastClickedSample = clickedSample.GetComponent<SpriteRenderer>().sprite;
        currentSample = lastClickedSample;

        // Set SampleManager as the last clicked manager
        ManagerHandler.Instance.SetLastClickedManager(false, false, true);

        // Reset the board to display default configuration first
        //BoardManager.Instance.ResetBoard();

        // Scale the clicked sample temporarily
        StartCoroutine(ScaleObject(clickedSample, originalScales[clickedSample], 0.1f, 1.2f, 0.1f));

        // Play the corresponding audio clip
        PlaySampleAudio(currentSample.name);

        // Display the sprite on cells with matching step data
        //DisplaySpriteOnMatchingSteps();

        Debug.Log($"Clicked Sample: {clickedSample.name}");
    }

    public void OnSampleEvent(GameObject clickedSample)
    {
        //midiNote = clickedSample.GetComponent<SampleClickHandler>().midiNote;

        //lastClickedSample = clickedSample.GetComponent<SpriteRenderer>().sprite;
        //currentSample = lastClickedSample;

        // Set SampleManager as the last clicked manager
        //ManagerHandler.Instance.SetLastClickedManager(false, false, true);

        // Reset the board to display default configuration first
        //BoardManager.Instance.ResetBoard();

        // Scale the clicked sample temporarily
        //StartCoroutine(ScaleObject(clickedSample, originalScales[clickedSample], 0.1f, 1.2f, 0.1f));

        // Play the corresponding audio clip
        PlaySampleAudio(clickedSample.name);

        // Display the sprite on cells with matching step data
        //DisplaySpriteOnMatchingSteps();

        Debug.Log($"Clicked Sample: {clickedSample.name}");
    }

    private IEnumerator ScaleObject(GameObject obj, Vector3 originalScale, float scaleUpTime, float scaleUpSpeed, float scaleDownTime)
    {
        float elapsedTime = 0f;
        Vector3 targetScale = originalScale * scaleUpSpeed;

        // Scale up
        while (elapsedTime < scaleUpTime)
        {
            obj.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / scaleUpTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        obj.transform.localScale = targetScale;
        yield return new WaitForSeconds(0.2f);

        // Scale down
        elapsedTime = 0f;
        while (elapsedTime < scaleDownTime)
        {
            obj.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / scaleDownTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        obj.transform.localScale = originalScale;
    }

    public void PlaySampleAudio(string sampleName)
    {
        Debug.Log($"PlaySampleAudio called with sampleName: {sampleName}");

        int index = System.Array.FindIndex(samples, sprite => sprite.name == sampleName);
        Debug.Log($"Sample '{sampleName}' found at index: {index}");

        if (index != -1 && index < chopScript.timestamps.Count)
        {
            float timestamp = chopScript.timestamps[index];
            Debug.Log($"Sample '{sampleName}' has a timestamp of {timestamp}");

            AudioClip clip = MultipleAudioLoader.Instance.audioSource.clip;
            if (clip == null)
            {
                Debug.LogError("No AudioClip found in MultipleAudioLoader.Instance.audioSource.");
                return;
            }
            else
            {
                Debug.Log($"AudioClip '{clip.name}' successfully assigned.");
            }

            audioSource.Stop();
            audioSource.clip = clip;

            if (timestamp < 0 || timestamp > clip.length)
            {
                Debug.LogError($"Timestamp {timestamp} is out of bounds for the audio clip length {clip.length}.");
                return;
            }
            Debug.Log($"Timestamp {timestamp} is within bounds.");

            float durationToNextTimestamp = (index + 1 < chopScript.timestamps.Count) ? 
                                            chopScript.timestamps[index + 1] - timestamp : 
                                            clip.length - (float)timestamp;

            Debug.Log($"audioSource.time set to {timestamp}, calling Play().");
            audioSource.time = timestamp;
            audioSource.Play();

            Debug.Log($"Playing sample: {sampleName} from time: {timestamp} for {durationToNextTimestamp} seconds.");
            timeToPlayNextSample = Time.time + durationToNextTimestamp;
            isPlayingSample = true;
        }
        else
        {
            Debug.LogError($"Sample name '{sampleName}' not found or index is out of bounds.");
        }
    }

    public Sprite GetCurrentSprite()
    {
        return currentSample;
    }    

    private void Update()
    {
        // Check if we need to stop playing the sample
        if (isPlayingSample && Time.time >= timeToPlayNextSample)
        {
            audioSource.Stop();
            isPlayingSample = false;
            Debug.Log("Stopped sample playback.");
        }
    }    

    // Method to display sprites on cells with matching step data
    public void DisplaySpriteOnMatchingSteps()
    {
        Debug.Log("Displaying all saved tiles for SampleManager.");

        Debug.Log("Printing all sampleTileData entries:");
        foreach (var entry in sampleTileData)
        {
            Debug.Log($"Sprite = {entry.Key}, Steps = {string.Join(", ", entry.Value)}");
        }

        // Iterate through all sprite and step pairs in sampleTileData
        foreach (var entry in sampleTileData)
        {
            string spriteName = entry.Key;
            List<int> steps = entry.Value;
            Debug.Log($"Processing sprite: {spriteName} for steps: {string.Join(", ", steps)}");

            Sprite sprite = GetSpriteByName(spriteName);

            if (sprite != null)
            {
                Debug.Log($"Found sprite {sprite.name}");

                // Iterate through boardCells to find cells with matching steps
                for (int x = 0; x < BoardManager.Instance.boardCells.GetLength(0); x++)
                {
                    for (int y = 0; y < BoardManager.Instance.boardCells.GetLength(1); y++)
                    {
                        Cell cell = BoardManager.Instance.boardCells[x, y];

                        if (cell != null && steps.Contains((int)cell.step))
                        {
                            Debug.Log($"Attempting to display sprite {sprite.name} on cell at position ({x}, {y}) with step {cell.step}.");
                            
                            // Check if the cell already has a sprite
                            if (cell.CurrentSprite != null)
                            {
                                Debug.LogWarning($"Cell at ({x}, {y}) already has sprite {cell.CurrentSprite.name}, will be replaced by {sprite.name}");
                            }

                            cell.SetSprite(sprite);
                            Debug.Log($"Displayed sprite {sprite.name} on cell at position ({x}, {y}) with step {cell.step}. Current sprite in cell: {cell.CurrentSprite.name}");

                            // Apply note to HelmSequencer
                            //int midiNote = GetMidiNoteForSprite(sprite.name);
                            //sampleSequencer.GetComponent<SampleSequencer>().AddNote(midiNote, cell.step, cell.step + 1, 1.0f);
                            //Debug.Log($"Added MIDI {midiNote} at Step = {cell.step}");
                        }
                        else if (cell != null)
                        {
                            Debug.Log($"Cell at position ({x}, {y}) with step {cell.step} does not match the steps for sprite {sprite.name}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"Sprite with name {spriteName} not found.");
            }
        }
    }

    // Helper method to get a sprite by its name
    public Sprite GetSpriteByName(string spriteName)
    {
        if (samples == null || samples.Length == 0)
        {
            Debug.LogError("The sprites array is null or empty.");
            return null;
        }

        Debug.Log($"Looking for sprite with name: {spriteName}");

        foreach (Sprite sprite in samples)
        {
            Debug.Log($"Checking sprite: {sprite.name}");
            if (sprite.name == spriteName)
            {
                Debug.Log($"Found sprite: {sprite.name}");
                return sprite;
            }
        }

        Debug.LogError($"Sprite with name {spriteName} not found.");
        return null;
    }

    private int GetMidiNoteForSprite(string spriteName)
    {
        // Try to extract the number from the sprite name and use it to calculate the MIDI note
        if (int.TryParse(spriteName.Replace("sample_", ""), out int keyNumber) && keyNumber >= 0 && keyNumber <= 15)
        {
            return 60 + keyNumber; // MIDI note calculation: 33 (A0) + (keyNumber - 1)
        }
        return 60;
    }   

    public void SaveSampleTileData(Sprite sprite, int step)
    {
        if (sprite != null)
        {
            string spriteName = sprite.name;

            if (sampleTileData.ContainsKey(spriteName))
            {
                if (!sampleTileData[spriteName].Contains(step))
                {
                    sampleTileData[spriteName].Add(step); // Add the new step if it doesn't already exist
                }
            }
            else
            {
                sampleTileData.Add(spriteName, new List<int> { step }); // Create a new entry with the step
            }

            Debug.Log($"Saved Key Tile Data: Sprite = {sprite.name}, Step = {step}");

            // Log the entire dictionary after saving
            Debug.Log("Current sampleTileData contents:");
            foreach (var entry in sampleTileData)
            {
                Debug.Log($"Sprite = {entry.Key}, Steps = {string.Join(", ", entry.Value)}");
            }
        }
        else
        {
            Debug.LogError("Sprite is null. Cannot save tile data.");
        }
    }


    public void RemoveSampleTileData(Sprite sprite, int step)
    {
        if (sprite == null)
        {
            Debug.LogError("Sprite is null. Cannot remove tile data.");
            return;
        }

        string spriteName = sprite.name;

        // Check if the sprite to be removed is in the tileData
        if (!sampleTileData.ContainsKey(spriteName))
        {
            Debug.LogWarning($"Sprite = {spriteName} not found in tile data.");
            return;
        }

        List<int> steps = sampleTileData[spriteName];

        // Check if the step exists in the list for the sprite
        if (!steps.Contains(step))
        {
            Debug.LogWarning($"Step {step} not found for Sprite = {spriteName}.");
            return;
        }

        // Remove the step from the list
        steps.Remove(step);

        // Remove the sprite from tileData if it has no remaining steps
        if (steps.Count == 0)
        {
            sampleTileData.Remove(spriteName);

            // If the sprite being removed is not the default sprite
            if (sprite != defaultSprite)
            {
                // Update tileData to add the step to the default sprite
                if (defaultSprite != null)
                {
                    string defaultSpriteName = defaultSprite.name;
                    
                    if (!sampleTileData.ContainsKey(defaultSpriteName))
                    {
                        sampleTileData[defaultSpriteName] = new List<int>();
                    }

                    // Add the step to default sprite if not already present
                    if (!sampleTileData[defaultSpriteName].Contains(step))
                    {
                        sampleTileData[defaultSpriteName].Add(step);
                    }
                }
            }

            Debug.Log($"Removed Key Tile Data: Sprite = {sprite.name}, Step = {step}");
        }
        else
        {
            Debug.Log($"Removed Step = {step} from Sprite = {sprite.name}");
        }
    }

    public Sprite GetSpriteByStep(int step)
    {
        if (BoardManager.Instance.stepToSpriteMap.ContainsKey(step))
        {
            return BoardManager.Instance.stepToSpriteMap[step];
        }
        else
        {
            Debug.LogError($"No sprite found for step {step}.");
        }

        // Ensure we have tile data populated
        if (sampleTileData == null || sampleTileData.Count == 0)
        {
            Debug.LogError("Tile data is not initialized or empty.");
            return null;
        }

        // Iterate through all sprite and step pairs in tileData
        foreach (var entry in sampleTileData)
        {
            string spriteName = entry.Key;
            List<int> steps = entry.Value;

            // Check if the step is present in the list of steps for the current sprite
            if (steps.Contains(step))
            {
                // Get the sprite by name
                Sprite sprite = GetSpriteByName(spriteName);
                if (sprite != null)
                {
                    return sprite;
                }
                else
                {
                    Debug.LogError($"Sprite with name {spriteName} not found.");
                }
            }
        }

        Debug.LogError($"No sprite found for step {step}.");
        return null;
    }

    public Sprite GetSpriteFromNote(int midiNote)
    {
        // Check if the dictionary has a mapping for the given MIDI note
        if (midiNoteToSpriteMap != null && midiNoteToSpriteMap.ContainsKey(midiNote))
        {
            return midiNoteToSpriteMap[midiNote];
        }
        else
        {
            Debug.LogError($"No sprite found for MIDI note {midiNote}.");
            return null;
        }
    }
}



[System.Serializable]
public class SampleTileData
{
    public string SpriteName; // Store sprite name instead of sprite object
    public float Step;

    public SampleTileData(string spriteName, float step)
    {
        SpriteName = spriteName;
        Step = step;
    }    
}