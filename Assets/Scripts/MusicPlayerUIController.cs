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

    private void Start()
    {
        // Ensure the KMusicPlayer instance is available
        if (KMusicPlayer.Instance == null)
        {
            Debug.LogError("KMusicPlayer instance is missing.");
            return;
        }

        // Add listeners to buttons
        playButton.onClick.AddListener(PlayTrack);
        pauseButton.onClick.AddListener(PauseTrack);
        stopButton.onClick.AddListener(StopTrack);
        nextButton.onClick.AddListener(PlayNextTrack);
        previousButton.onClick.AddListener(PlayPreviousTrack);

        // Initialize the UI
        StartCoroutine(InitializeUI());
    }

    private IEnumerator InitializeUI()
    {
        // Wait until audio files are loaded
        yield return new WaitUntil(() => KMusicPlayer.Instance.clipFileNames.Count > 0);

        // Initialize the waveform visualizer
        waveform.GetComponent<WaveformVisualizer>().CreateWave();

        // Play the first track if available
        //PlayTrack();
    }

    private void PlayTrack()
    {
        // Play the current track if one is loaded
        if (KMusicPlayer.Instance.currentIndex >= 0)
        {
            KMusicPlayer.Instance.audioSource.Play();
        }
        else if (KMusicPlayer.Instance.clipFileNames.Count > 0)
        {
            // Load and play the first track if no track is currently loaded
            KMusicPlayer.Instance.PlayTrack(KMusicPlayer.Instance.clipFileNames[0]);
        }
        waveform.GetComponent<WaveformVisualizer>().StartWave();
        UpdateTrackName();
    }

    private void PauseTrack()
    {
        // Pause the current track if it is playing
        if (KMusicPlayer.Instance.audioSource.isPlaying)
        {
            KMusicPlayer.Instance.audioSource.Pause();
            waveform.GetComponent<WaveformVisualizer>().StopWave();
        }
        else
        {
            Debug.LogWarning("No track is playing to pause.");
        }
    }

    private void StopTrack()
    {
        // Stop the current track and reset the AudioSource
        if (KMusicPlayer.Instance.audioSource.isPlaying)
        {
            KMusicPlayer.Instance.audioSource.Stop();
            waveform.GetComponent<WaveformVisualizer>().StopWave();
            UpdateTrackName();
        }
    }

    private void PlayNextTrack()
    {
        // Play the next track and update the UI
        KMusicPlayer.Instance.PlayNextTrack();
        waveform.GetComponent<WaveformVisualizer>().StartWave();
        UpdateTrackName();
    }

    private void PlayPreviousTrack()
    {
        // Play the previous track and update the UI
        KMusicPlayer.Instance.PlayPreviousTrack();
        waveform.GetComponent<WaveformVisualizer>().StartWave();
        UpdateTrackName();
    }

    private void UpdateTrackName()
    {
        // Update the UI text to show the current track name
        if (KMusicPlayer.Instance.currentIndex >= 0 && KMusicPlayer.Instance.currentIndex < KMusicPlayer.Instance.clipFileNames.Count)
        {
            trackNameText.text = "Now Playing: " + KMusicPlayer.Instance.clipFileNames[KMusicPlayer.Instance.currentIndex];
        }
        else
        {
            trackNameText.text = "No Track Loaded";
        }
    }
}
