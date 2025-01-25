using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioHelm;
using TMPro;

public class AudioBPMAdjuster : MonoBehaviour
{
    public static AudioBPMAdjuster Instance { get; private set; } // Singleton instance

    public AudioSource audioSource; // Will be dynamically retrieved from MultipleAudioLoader
    public BPMDetector bpmDetector; // Reference to the BPMDetector
    public AudioHelmClock clockController; // Reference to the AudioHelmClock for original BPM

    public Slider bpmSlider; // Reference to the slider for target BPM
    public TMP_Text bpmText; // Optional: Display the current BPM value

    public float originalBPM = 120f; // Detected dynamically
    public float targetBPM = 150f;  // Updated dynamically based on the slider

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Debug.LogWarning($"Duplicate AudioBPMAdjuster found. Destroying the duplicate on {gameObject.name}.");
            Destroy(gameObject); // Destroy duplicate
        }
    }

    private void Start()
    {
        // Dynamically fetch the AudioSource from MultipleAudioLoader
        if (MultipleAudioLoader.Instance != null)
        {
            audioSource = MultipleAudioLoader.Instance.audioSource;
        }

        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned or could not be fetched from MultipleAudioLoader.");
        }

        // Fetch the original BPM from the BPMDetector
        if (bpmDetector != null)
        {
            FetchOriginalBPM();
        }
        else if (clockController != null)
        {
            // Fallback to the clock controller if BPMDetector is not available
            originalBPM = clockController.bpm;
        }
        else
        {
            Debug.LogError("Neither BPMDetector nor ClockController is assigned!");
        }

        // Initialize slider after original BPM is fetched
        InitializeSlider();
    }

    public void InitializeSlider()
    {
        if (bpmSlider != null)
        {
            bpmSlider.value = originalBPM; // Set the slider's initial value to the original BPM

            targetBPM = originalBPM; // Set the target BPM to match the original BPM initially
            bpmSlider.onValueChanged.AddListener(OnSliderValueChanged);

            Debug.Log($"Slider initialized. Min: {bpmSlider.minValue}, Max: {bpmSlider.maxValue}, Initial Value: {bpmSlider.value}");
        }

        // Update the BPM display text
        if (bpmText != null)
        {
            bpmText.text = $"{originalBPM:F0}";
        }
    }

    private void OnSliderValueChanged(float newValue)
    {
        targetBPM = newValue; // Update the target BPM based on the slider
        AdjustPlaybackSpeed(); // Adjust the playback speed
        Debug.Log($"Target BPM updated to: {targetBPM}");

        // Update the BPM text display
        if (bpmText != null)
        {
            bpmText.text = $"{targetBPM:F0}";
        }
    }

    public void FetchOriginalBPM()
    {
        if (bpmDetector == null)
        {
            Debug.LogError("BPMDetector is not assigned.");
            return;
        }

        originalBPM = bpmDetector.DetectBPM();

        if (originalBPM <= 0)
        {
            Debug.LogError("Could not fetch the original BPM of the audio clip.");
        }
        else
        {
            Debug.Log($"Fetched Original BPM: {originalBPM}");
            targetBPM = originalBPM; // Match the target BPM to the original BPM initially
            AdjustPlaybackSpeed(); // Adjust speed after fetching the original BPM
        }
    }

    public void AdjustPlaybackSpeed()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null. Cannot adjust playback speed.");
            return;
        }

        if (originalBPM <= 0)
        {
            Debug.LogError("Original BPM is invalid. Cannot adjust playback speed.");
            return;
        }

        if (targetBPM <= 0)
        {
            Debug.LogError("Target BPM is invalid. Cannot adjust playback speed.");
            return;
        }

        // Calculate playback speed
        float speedFactor = targetBPM / originalBPM;
        Debug.Log($"Adjusting pitch. Original BPM: {originalBPM}, Target BPM: {targetBPM}, Speed Factor: {speedFactor}");

        audioSource.pitch = speedFactor; // Adjust pitch
        Debug.Log($"AudioSource pitch adjusted to: {audioSource.pitch}");
    }
}
