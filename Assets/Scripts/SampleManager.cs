using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class SampleManager : MonoBehaviour
{
    public static SampleManager Instance;   // Singleton instance

    public GameObject samplePrefab;         // Reference to the sample prefab
    public Sprite[] samples;                // Array of sprites to assign to each sample

    public Sprite currentSample;            // Current sample tracked by SampleManager
    private Sprite lastClickedSample;       // Last clicked sample

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>(); // Dictionary to store original scales of samples

    private AudioSource audioSource;        // AudioSource to play audio clips
    public Chop chopScript; // Reference to the Chop script

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
        // Validate samplePrefab and samples array
        if (!ValidateInitialSettings()) return;

        // Add an AudioSource component to the SampleManager
        audioSource = gameObject.AddComponent<AudioSource>();

        // Ensure the Chop script is assigned
        if (chopScript == null)
        {
            Debug.LogError("Chop script is not assigned.");
        }

        // Ensure the AudioSource is assigned
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is not assigned.");
        }

        // Generate samples
        GenerateSamples();
    }


    public void PlaySample(int padIndex)
    {
        if (chopScript != null && padIndex >= 0 && padIndex < chopScript.timestamps.Count)
        {
            // Get the timestamp from the Chop script
            float timestamp = chopScript.timestamps[padIndex];
            audioSource.time = timestamp;
            audioSource.Play();
            Debug.Log($"Playing sample at timestamp: {timestamp}");
        }
        else
        {
            Debug.LogError("Invalid pad index or Chop script not assigned.");
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
        currentSample = clickedSample.GetComponent<SpriteRenderer>().sprite;

        // Set SampleManager as the last clicked manager
        ManagerHandler.Instance.SetLastClickedManager(true);

        // Scale the clicked sample temporarily
        StartCoroutine(ScaleSample(clickedSample));

        // Play the corresponding audio clip
        PlaySampleAudio(currentSample);

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

    // Method to play the corresponding audio clip for the sample
    private void PlaySampleAudio(Sprite sample)
    {
        // Assuming you have a way to get the audio clip from the sprite name
        AudioClip audioClip = KMusicPlayer.Instance.GetCurrentClip();
        if (audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
            Debug.Log($"Playing audio for sample: {sample.name}");
        }
        else
        {
            Debug.LogError($"Audio clip for sample {sample.name} not found.");
        }
    }
}
