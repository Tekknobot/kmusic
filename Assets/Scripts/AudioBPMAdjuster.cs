using System;
using System.IO;
using AudioHelm;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private bool isSliderInitialized = false; // Tracks whether the slider is initialized for the current project

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
    }

    public void InitializeSlider()
    {
        if (isSliderInitialized)
        {
            Debug.Log("Slider is already initialized for the current project. Skipping reinitialization.");
            return;
        }

        // Check for saved project data
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
            bpmText.text = $"{savedTargetBPM:F0}";
        }

        isSliderInitialized = true; // Mark the slider as initialized
    }

    public void ResetSliderInitialization()
    {
        isSliderInitialized = false; // Allow reinitialization for the next project
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
