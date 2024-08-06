using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq; // Add this namespace

public class SampleManager : MonoBehaviour
{
    public static SampleManager Instance;   // Singleton instance

    public GameObject samplePrefab;         // Reference to the sample prefab
    public Sprite[] samples;                // Array of sprites to assign to each sample

    public Sprite currentSample;            // Current sample tracked by SampleManager
    private Sprite lastClickedSample;       // Last clicked sample

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>(); // Dictionary to store original scales of samples

    private Dictionary<string, List<SampleData>> sampleData = new Dictionary<string, List<SampleData>>(); // Dictionary to store sample data

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

        // Generate samples
        GenerateSamples();

        // Start the coroutine to load sample data after a delay
        StartCoroutine(LoadSampleDataAfterDelay(0.2f)); // Adjust the delay time as needed
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

    private IEnumerator LoadSampleDataAfterDelay(float delaySeconds)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delaySeconds);

        // Load the sample data from file
        sampleData = DataManager.LoadSampleDataFromFile();

        // Log the loaded sample data
        foreach (var entry in sampleData)
        {
            Debug.Log($"Loaded sample data: Sample = {entry.Key}, Data = {string.Join(", ", entry.Value.Select(d => $"Index={d.songIndex}, Timestamp={d.timestamp}, Pad={d.padNumber}"))}");
        }
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

    // Method to save sample data
    public void SaveSampleData(Sprite sample, int songIndex, float timestamp, int padNumber)
    {
        if (sample != null)
        {
            string sampleName = sample.name;

            var newSampleData = new SampleData(songIndex, timestamp, padNumber);

            if (sampleData.ContainsKey(sampleName))
            {
                sampleData[sampleName].Add(newSampleData);
            }
            else
            {
                sampleData.Add(sampleName, new List<SampleData> { newSampleData });
            }

            Debug.Log($"Saved Sample Data: Sample = {sample.name}, SongIndex = {songIndex}, Timestamp = {timestamp}, PadNumber = {padNumber}");
        }
        else
        {
            Debug.LogError("Sample is null. Cannot save sample data.");
        }
    }

    // Method to remove sample data
    public void RemoveSampleData(Sprite sample, int songIndex, float timestamp, int padNumber)
    {
        if (sample == null)
        {
            Debug.LogError("Sample is null. Cannot remove sample data.");
            return;
        }

        string sampleName = sample.name;

        if (!sampleData.ContainsKey(sampleName))
        {
            Debug.LogWarning($"Sample = {sampleName} not found in sample data.");
            return;
        }

        List<SampleData> sampleDatas = sampleData[sampleName];

        SampleData dataToRemove = sampleDatas.Find(d => d.songIndex == songIndex && d.timestamp == timestamp && d.padNumber == padNumber);

        if (dataToRemove != null)
        {
            sampleDatas.Remove(dataToRemove);

            if (sampleDatas.Count == 0)
            {
                sampleData.Remove(sampleName);
                Debug.Log($"Removed Sample Data: Sample = {sample.name}, SongIndex = {songIndex}, Timestamp = {timestamp}, PadNumber = {padNumber}");
            }
            else
            {
                Debug.Log($"Removed Sample Data: SongIndex = {songIndex}, Timestamp = {timestamp}, PadNumber = {padNumber} from Sample = {sample.name}");
            }
        }
        else
        {
            Debug.LogWarning($"Sample Data not found: SongIndex = {songIndex}, Timestamp = {timestamp}, PadNumber = {padNumber} for Sample = {sampleName}");
        }
    }

    // Method to display samples on steps with matching data
    public void DisplaySamplesOnMatchingSteps()
    {
        Debug.Log("Displaying all saved samples for SampleManager.");

        foreach (var entry in sampleData)
        {
            string sampleName = entry.Key;
            List<SampleData> dataList = entry.Value;
            Sprite sample = GetSampleByName(sampleName);

            if (sample != null)
            {
                Debug.Log($"Displaying sample {sample.name} for data {string.Join(", ", dataList.Select(d => $"Index={d.songIndex}, Timestamp={d.timestamp}, Pad={d.padNumber}"))}");

                foreach (var data in dataList)
                {
                    Cell cell = BoardManager.Instance.GetCellByStep(data.padNumber); // Adjust as needed
                    if (cell != null)
                    {
                        cell.SetSprite(sample); // Assume Cell has a method to set the sprite
                        Debug.Log($"Displayed sample {sample.name} on cell at pad number {data.padNumber}");
                    }
                }
            }
            else
            {
                Debug.LogError($"Sample with name {sampleName} not found.");
            }
        }
    }

    // Helper method to get a sample by its name
    public Sprite GetSampleByName(string sampleName)
    {
        if (samples == null || samples.Length == 0)
        {
            Debug.LogError("The samples array is null or empty.");
            return null;
        }

        foreach (Sprite sample in samples)
        {
            if (sample.name == sampleName)
            {
                return sample;
            }
        }

        Debug.LogError($"Sample with name {sampleName} not found.");
        return null;
    }
}

[System.Serializable]
public class SampleData
{
    public int songIndex;
    public float timestamp;
    public int padNumber;

    public SampleData() { }

    public SampleData(int songIndex, float timestamp, int padNumber)
    {
        this.songIndex = songIndex;
        this.timestamp = timestamp;
        this.padNumber = padNumber;
    }
}
