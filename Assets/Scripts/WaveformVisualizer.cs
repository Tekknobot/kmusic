using UnityEngine;
using System.Collections;

public class WaveformVisualizer : MonoBehaviour
{
    public AudioSource audioSource; // Reference to the AudioSource
    public int sampleSize = 1024; // Number of samples to process
    public GameObject barPrefab; // Prefab for the waveform bars
    public Transform barsParent; // Parent transform to hold the bars

    private float[] samples;
    private GameObject[] bars;
    private Coroutine waveformCoroutine;

    private void Awake()
    {
        // Ensure AudioSource is assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void CreateWave()
    {
        samples = new float[sampleSize];
        bars = new GameObject[sampleSize];

        // Initialize waveform bars
        for (int i = 0; i < sampleSize; i++)
        {
            bars[i] = Instantiate(barPrefab, barsParent);
            bars[i].transform.localPosition = new Vector3(i * 0.1f, 0, 0); // Adjust spacing as needed
        }

        Debug.Log("Waveform bars created.");
    }

    public void StartWave()
    {
        // Stop the previous coroutine if it's running
        if (waveformCoroutine != null)
        {
            StopCoroutine(waveformCoroutine);
        }

        // Start the new coroutine
        waveformCoroutine = StartCoroutine(UpdateWaveform());
        Debug.Log("Waveform started.");
    }

    public void StopWave()
    {
        // Stop the coroutine if it's running
        if (waveformCoroutine != null)
        {
            StopCoroutine(waveformCoroutine);
            waveformCoroutine = null;
        }

        // Optionally, clear or reset the visualizer
        //ClearWaveform();
        Debug.Log("Waveform stopped.");
    }

    private IEnumerator UpdateWaveform()
    {
        Debug.Log("UpdateWaveform coroutine started.");
        
        while (audioSource.isPlaying)
        {
            audioSource.GetOutputData(samples, 0);
            for (int i = 0; i < sampleSize; i++)
            {
                float sample = Mathf.Abs(samples[i]);
                bars[i].transform.localScale = new Vector3(1, sample * 10, 1); // Adjust scaling factor as needed
            }
            yield return null; // Wait for the next frame
        }

        // Optionally stop the visualizer when audio stops
        StopWave();
    }

    private void ClearWaveform()
    {
        // Optionally, clear the waveform bars or reset their state
        if (bars != null)
        {
            foreach (var bar in bars)
            {
                if (bar != null)
                {
                    bar.transform.localScale = new Vector3(1, 0, 1); // Reset scale or hide
                }
            }
        }
        Debug.Log("Waveform cleared.");
    }
}
