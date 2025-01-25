using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AudioHelm;
using TMPro;
using System.IO;
using System;

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

    private bool isTrackChanged = false; // Tracks if a new track has been selected

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

        // Initialize slider after original BPM is fetched
        if (bpmDetector != null)
        {
            FetchOriginalBPM();
        }
        else if (clockController != null)
        {
            originalBPM = clockController.bpm; // Fallback
        }

        InitializeSlider();
    }

    public void InitializeSlider()
    {
        // Check if there's saved project data
        float savedTargetBPM = originalBPM; // Default to original BPM

        if (PatternManager.Instance != null)
        {
            var lastProject = PatternManager.LastProjectFilename;
            if (!string.IsNullOrEmpty(lastProject))
            {
                string path = Path.Combine(Application.persistentDataPath, lastProject);
                if (File.Exists(path))
                {
                    try
                    {
                        string json = File.ReadAllText(path);
                        ProjectData projectData = JsonUtility.FromJson<ProjectData>(json);

                        if (projectData != null && projectData.pitch > 0 && originalBPM > 0)
                        {
                            savedTargetBPM = originalBPM * projectData.pitch;
                            Debug.Log($"Loaded saved target BPM from project data: {savedTargetBPM}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error loading saved project data: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning("No project data file found. Initializing with default BPM.");
                }
            }
        }

        // Initialize the slider
        if (bpmSlider != null)
        {
            bpmSlider.value = savedTargetBPM; // Set the slider's initial value

            targetBPM = savedTargetBPM; // Set the target BPM to the saved value or original BPM
            bpmSlider.onValueChanged.AddListener(OnSliderValueChanged);

            Debug.Log($"Slider initialized. Min: {bpmSlider.minValue}, Max: {bpmSlider.maxValue}, Initial Value: {bpmSlider.value}");
        }

        // Update the BPM display text
        if (bpmText != null)
        {
            bpmText.text = $"{savedTargetBPM:F0} : {originalBPM:F0}";
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
            bpmText.text = $"{targetBPM:F0} : {originalBPM:F0}";
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

    public void OnTrackChanged()
    {
        // Reset pitch and reload the original BPM for the new track
        if (audioSource != null)
        {
            audioSource.pitch = 1.0f; // Reset pitch to normal
        }

        if (bpmDetector != null)
        {
            FetchOriginalBPM(); // Fetch the original BPM for the new track
        }
        else
        {
            Debug.LogWarning("BPMDetector is not assigned. Unable to fetch BPM for the new track.");
        }

        InitializeSlider(); // Reinitialize the slider for the new track
        Debug.Log("Track changed. Pitch reset and slider reinitialized.");
    }
}
