using UnityEngine;
using UnityEngine.UI; // For UI Button
using TMPro; // For TextMesh Pro
using System.Collections;

public class MusicPlayerUIController : MonoBehaviour
{
    public Button playButton;       // Button to play the current track
    public Button pauseButton;      // Button to pause the current track
    public Button stopButton;       // Button to stop the current track
    public Button nextButton;       // Button to play the next track
    public Button previousButton;   // Button to play the previous track
    public TMP_Text trackNameText;  // TextMesh Pro text to display the current track name

    public GameObject waveform;     // Reference to the waveform visualizer GameObject

    private int currentTrackIndex = 0; // Index of the currently selected track
    private Coroutine currentTrackCoroutine; // Reference to the currently running track coroutine
    private bool isOperationInProgress = false; // Lock to prevent overlapping operations
    private float debounceTime = 0.5f; // Debounce time in seconds

    private void Start()
    {
        // Ensure the MultipleAudioLoader instance is available
        if (MultipleAudioLoader.Instance == null)
        {
            Debug.LogError("MultipleAudioLoader instance is missing.");
            return;
        }

        // Add listeners to buttons
        playButton.onClick.AddListener(PlayTrack);
        stopButton.onClick.AddListener(StopTrack);
        nextButton.onClick.AddListener(() => StartCoroutine(HandleTrackChange(PlayNextTrack)));
        previousButton.onClick.AddListener(() => StartCoroutine(HandleTrackChange(PlayPreviousTrack)));

        // Initialize the UI
        StartCoroutine(InitializeUI());
    }

    private IEnumerator InitializeUI()
    {
        // Wait until audio files are loaded
        yield return new WaitUntil(() => MultipleAudioLoader.Instance.clipFileNames.Count > 0);

        // Initialize the waveform visualizer
        waveform.GetComponent<WaveformVisualizer>().CreateWave();

        // Optionally play the first track
        // PlayTrack();
    }

    private void PlayTrack()
    {
        if (isOperationInProgress) return;
        isOperationInProgress = true;

        currentTrackIndex = MultipleAudioLoader.Instance.currentIndex;

        // Play the current track if one is loaded
        if (currentTrackIndex >= 0 && currentTrackIndex < MultipleAudioLoader.Instance.clipFileNames.Count)
        {
            StartNewCoroutine(PlayTrackCoroutine(MultipleAudioLoader.Instance.clipFileNames[currentTrackIndex]));
        }
        UpdateTrackName();
        StartCoroutine(ResetOperationLock());
    }

    private IEnumerator PlayTrackCoroutine(string clipFileName)
    {
        // Load and play the clip
        yield return StartCoroutine(MultipleAudioLoader.Instance.LoadAndPlayClip(clipFileName));

        // Delay slightly to ensure the clip has started playing
        yield return new WaitForSeconds(0.1f);

        // Start the waveform visualization
        waveform.GetComponent<WaveformVisualizer>().StartWave();
    }

    private void StopTrack()
    {
        if (isOperationInProgress) return;
        isOperationInProgress = true;

        // Stop the current track and reset the AudioSource
        if (MultipleAudioLoader.Instance.audioSource.isPlaying)
        {
            MultipleAudioLoader.Instance.audioSource.Stop();
            waveform.GetComponent<WaveformVisualizer>().StopWave();
            UpdateTrackName();
        }

        StartCoroutine(ResetOperationLock());
    }

    private void PlayNextTrack()
    {
        // Increment the current track index and wrap around if necessary
        currentTrackIndex = (currentTrackIndex + 1) % MultipleAudioLoader.Instance.clipFileNames.Count;

        // Play the next track and update the UI
        StartNewCoroutine(PlayTrackCoroutine(MultipleAudioLoader.Instance.clipFileNames[currentTrackIndex]));
        UpdateTrackName();
    }

    private void PlayPreviousTrack()
    {
        // Decrement the current track index and wrap around if necessary
        currentTrackIndex--;
        if (currentTrackIndex < 0)
        {
            currentTrackIndex = MultipleAudioLoader.Instance.clipFileNames.Count - 1;
        }

        // Play the previous track and update the UI
        StartNewCoroutine(PlayTrackCoroutine(MultipleAudioLoader.Instance.clipFileNames[currentTrackIndex]));
        UpdateTrackName();
    }

    private void UpdateTrackName()
    {
        // Update the UI text to show the current track name
        if (currentTrackIndex >= 0 && currentTrackIndex < MultipleAudioLoader.Instance.clipFileNames.Count)
        {
            trackNameText.text = "Now Playing: " + MultipleAudioLoader.Instance.clipFileNames[currentTrackIndex];
        }
        else
        {
            trackNameText.text = "No Track Loaded";
        }
    }

    private void StartNewCoroutine(IEnumerator coroutine)
    {
        // Stop the currently running coroutine if any
        if (currentTrackCoroutine != null)
        {
            StopCoroutine(currentTrackCoroutine);
        }

        // Start the new coroutine and store its reference
        currentTrackCoroutine = StartCoroutine(coroutine);
    }

    private IEnumerator HandleTrackChange(System.Action trackChangeAction)
    {
        if (isOperationInProgress) yield break;

        isOperationInProgress = true;

        trackChangeAction();

        // Debounce to prevent rapid clicking
        yield return new WaitForSeconds(debounceTime);

        isOperationInProgress = false;
    }

    private IEnumerator ResetOperationLock()
    {
        // Ensure there's a short delay before the next operation can begin
        yield return new WaitForSeconds(0.1f);
        isOperationInProgress = false;
    }
}
