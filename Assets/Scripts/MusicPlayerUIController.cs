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
        nextButton.onClick.AddListener(PlayNextTrack);
        previousButton.onClick.AddListener(PlayPreviousTrack);

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

        currentTrackIndex = MultipleAudioLoader.Instance.currentIndex;
    }

    private void PlayTrack()
    {
        // Play the current track if one is loaded
        if (currentTrackIndex >= 0 && currentTrackIndex < MultipleAudioLoader.Instance.clipFileNames.Count)
        {
            StartCoroutine(MultipleAudioLoader.Instance.LoadAndPlayClip(MultipleAudioLoader.Instance.clipFileNames[currentTrackIndex]));
            waveform.GetComponent<WaveformVisualizer>().StartWave();
        }
        UpdateTrackName();
    }

    private void StopTrack()
    {
        // Stop the current track and reset the AudioSource
        if (MultipleAudioLoader.Instance.audioSource.isPlaying)
        {
            MultipleAudioLoader.Instance.audioSource.Stop();
            waveform.GetComponent<WaveformVisualizer>().StopWave();
            UpdateTrackName();
        }
    }

    private void PlayNextTrack()
    {
        // Increment the current track index and wrap around if necessary
        currentTrackIndex = (currentTrackIndex + 1) % MultipleAudioLoader.Instance.clipFileNames.Count;

        // Play the next track and update the UI
        StartCoroutine(MultipleAudioLoader.Instance.LoadAndPlayClip(MultipleAudioLoader.Instance.clipFileNames[currentTrackIndex]));
        waveform.GetComponent<WaveformVisualizer>().StartWave();
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
        StartCoroutine(MultipleAudioLoader.Instance.LoadAndPlayClip(MultipleAudioLoader.Instance.clipFileNames[currentTrackIndex]));
        waveform.GetComponent<WaveformVisualizer>().StartWave();
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
}
