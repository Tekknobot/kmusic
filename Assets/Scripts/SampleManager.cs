using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SampleManager : MonoBehaviour
{
    public static SampleManager Instance;   // Singleton instance

    public GameObject samplePrefab;         // Reference to the sample prefab
    public Sprite[] samples;                // Array of sprites to assign to each sample

    public Sprite currentSample;            // Current sample tracked by SampleManager
    private Sprite lastClickedSample;       // Last clicked sample

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>(); // Dictionary to store original scales of samples

    private AudioSource audioSource;        // AudioSource to play audio clips
    public Chop chopScript;                 // Reference to the Chop script

    public float bpm = 120f;                // Beats per minute, adjust as needed

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

            // Set the sprite for the sample using the samples array
            if (!SetSampleSprite(newSample, i)) continue;

            // Store the original scale of the sample
            originalScales[newSample] = newSample.transform.localScale;

            // Attach click handler to the sample
            SampleClickHandler sampleClickHandler = newSample.AddComponent<SampleClickHandler>();
            sampleClickHandler.Initialize(this, newSample, samples[i]);

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
        lastClickedSample = clickedSample.GetComponent<SpriteRenderer>().sprite;
        currentSample = lastClickedSample;

        // Set SampleManager as the last clicked manager
        ManagerHandler.Instance.SetLastClickedManager(true);

        // Scale the clicked sample temporarily
        StartCoroutine(ScaleSample(clickedSample));

        // Play the corresponding audio clip
        StartCoroutine(PlaySampleAudio(currentSample.name));

        Debug.Log($"Clicked Sample: {clickedSample.name}");
    }

    // Coroutine to scale the clicked sample
    private IEnumerator ScaleSample(GameObject sampleObject)
    {
        Vector3 originalScale = originalScales[sampleObject];
        float scaleUpTime = 0.1f;
        float scaleUpSpeed = 1.2f;
        float scaleDownTime = 0.1f;

        float elapsedTime = 0f;

        // Scale up
        while (elapsedTime < scaleUpTime)
        {
            sampleObject.transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleUpSpeed, elapsedTime / scaleUpTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        sampleObject.transform.localScale = originalScale * scaleUpSpeed;

        yield return new WaitForSeconds(0.2f);

        // Scale down
        elapsedTime = 0f;
        while (elapsedTime < scaleDownTime)
        {
            sampleObject.transform.localScale = Vector3.Lerp(originalScale * scaleUpSpeed, originalScale, elapsedTime / scaleDownTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        sampleObject.transform.localScale = originalScale;
    }

    private IEnumerator PlaySampleAudio(string sampleName)
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
                yield break;
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
                yield break;
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
            
            // Wait until the next timestamp or end of clip
            yield return new WaitForSeconds(durationToNextTimestamp);

            // Stop the audio after the duration to the next timestamp
            audioSource.Stop();
            Debug.Log($"Stopped sample: {sampleName} after {durationToNextTimestamp} seconds.");
        }
        else
        {
            // Log an error if the sample name is not found or index is out of bounds
            Debug.LogError($"Sample name '{sampleName}' not found or index is out of bounds.");
        }
    }
}
