using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class SampleManager : MonoBehaviour
{
    public static SampleManager Instance;   // Singleton instance

    public GameObject samplePrefab;         // Reference to the sample prefab
    public Sprite[] samples;                // Array of sprites to assign to each sample

    public Sprite currentSample;            // Current sample tracked by SampleManager
    private Sprite lastClickedSample;       // Last clicked sample

    public Sprite currentPadSprite;        // Current pad sprite tracked by SampleManager
    public Sprite lastClickedPadSprite;    // Last clicked pad sprite

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
        audioSource = gameObject.AddComponent<AudioSource>();

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

        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                if (GameObject.Find("SampleSequencer").GetComponent<AudioHelm.SampleSequencer>().NoteExistsInRange(75 - i, j, j + 1) &&
                    GameObject.Find("SampleSequencer").GetComponent<AudioHelm.SampleSequencer>().currentIndex == j)
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
                                OnSampleClicked(sample);
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

        // Scale the clicked sample temporarily
        StartCoroutine(ScaleObject(clickedSample, originalScales[clickedSample], 0.1f, 1.2f, 0.1f));

        // Play the corresponding audio clip
        PlaySampleAudio(currentSample.name);

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
        // Find the index of the sprite
        int index = System.Array.FindIndex(samples, sprite => sprite.name == sampleName);

        if (index != -1 && index < chopScript.timestamps.Count)
        {
            // Get the timestamp for the sample
            float timestamp = chopScript.timestamps[index];
            Debug.Log($"Playing sample: {sampleName} with timestamp: {timestamp}");

            // Ensure the audio clip is assigned to the AudioSource
            AudioClip clip = KMusicPlayer.Instance.audioSource.clip;

            if (clip == null)
            {
                Debug.LogError("AudioSource does not have a clip assigned.");
                return;
            }

            // Log clip length for reference
            Debug.Log($"Audio clip length: {clip.length}");

            // Ensure audioSource is stopped before scheduling
            audioSource.Stop();
            audioSource.clip = clip;

            // Calculate the time to play the clip based on the timestamp
            double playbackTime = timestamp;

            if (playbackTime < 0 || playbackTime > clip.length)
            {
                Debug.LogError($"Timestamp {timestamp} is out of bounds for audio clip length {clip.length}.");
                return;
            }

            // Calculate the duration until the next timestamp
            float durationToNextTimestamp = 0f;
            if (index + 1 < chopScript.timestamps.Count)
            {
                durationToNextTimestamp = chopScript.timestamps[index + 1] - timestamp;
            }
            else
            {
                durationToNextTimestamp = clip.length - (float)playbackTime;
            }

            // Play the clip from the calculated time
            audioSource.time = (float)playbackTime;
            audioSource.Play();

            Debug.Log($"Playing sample: {sampleName} from time: {playbackTime} for {durationToNextTimestamp} seconds.");

            // Schedule stopping of the audio after the duration to the next timestamp
            timeToPlayNextSample = Time.time + durationToNextTimestamp;
            isPlayingSample = true;
        }
        else
        {
            // Log an error if the sample name is not found or index is out of bounds
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
}
