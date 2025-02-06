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

    private Coroutine currentTrackCoroutine; // Reference to the currently running track coroutine
    private bool isOperationInProgress = false; // Lock to prevent overlapping operations
    private float debounceTime = 0.5f; // Debounce time in seconds

    private void Start()
    {
        if (MultipleAudioLoader.Instance == null)
        {
            Debug.LogError("MultipleAudioLoader instance is missing.");
            return;
        }

        playButton.onClick.AddListener(PlayTrack);
        stopButton.onClick.AddListener(StopTrack);
        nextButton.onClick.AddListener(() => StartCoroutine(HandleTrackChange(PlayNextTrack)));
        previousButton.onClick.AddListener(() => StartCoroutine(HandleTrackChange(PlayPreviousTrack)));

        StartCoroutine(InitializeUI());
    }

    private IEnumerator InitializeUI()
    {
        yield return new WaitUntil(() => MultipleAudioLoader.Instance.clipFileNames.Count > 0);

        Debug.Log("Audio files loaded.");

        if (waveform != null)
        {
            var visualizer = waveform.GetComponent<WaveformVisualizer>();
            if (visualizer != null)
            {
                visualizer.CreateWave();
            }
        }

        UpdateTrackName();
    }

    private void PlayTrack()
    {
        if (isOperationInProgress) return;

        if (MultipleAudioLoader.Instance.clipFileNames.Count == 0)
        {
            Debug.LogWarning("No audio files available to play.");
            return;
        }

        isOperationInProgress = true;
        int currentTrackIndex = MultipleAudioLoader.Instance.currentIndex;

        if (currentTrackIndex >= 0 && currentTrackIndex < MultipleAudioLoader.Instance.clipFileNames.Count)
        {
            StartNewCoroutine(PlayTrackCoroutine(MultipleAudioLoader.Instance.clipFileNames[currentTrackIndex]));
        }

        StartCoroutine(ResetOperationLock());
    }

    private IEnumerator PlayTrackCoroutine(string clipFileName)
    {
        Debug.Log($"Playing track: {clipFileName}");
        yield return StartCoroutine(MultipleAudioLoader.Instance.LoadAndPlayClip(clipFileName));

        if (waveform != null)
        {
            var visualizer = waveform.GetComponent<WaveformVisualizer>();
            if (visualizer != null)
            {
                visualizer.StartWave();
            }
        }

        AudioBPMAdjuster.Instance.OnTrackChanged();
        UpdateTrackName();
    }

    private void StopTrack()
    {
        if (isOperationInProgress) return;

        isOperationInProgress = true;

        if (MultipleAudioLoader.Instance.audioSource.isPlaying)
        {
            MultipleAudioLoader.Instance.audioSource.Stop();

            if (waveform != null)
            {
                var visualizer = waveform.GetComponent<WaveformVisualizer>();
                if (visualizer != null)
                {
                    visualizer.StopWave();
                }
            }

            UpdateTrackName("Playback Stopped");
        }

        StartCoroutine(ResetOperationLock());
    }

    private void PlayNextTrack()
    {
        if (MultipleAudioLoader.Instance.clipFileNames.Count == 0) return;

        MultipleAudioLoader.Instance.currentIndex = 
            (MultipleAudioLoader.Instance.currentIndex + 1) % MultipleAudioLoader.Instance.clipFileNames.Count;

        StartCoroutine(PlayTrackCoroutine(MultipleAudioLoader.Instance.clipFileNames[MultipleAudioLoader.Instance.currentIndex]));
    }

    private void PlayPreviousTrack()
    {
        if (MultipleAudioLoader.Instance.clipFileNames.Count == 0) return;

        MultipleAudioLoader.Instance.currentIndex--;
        if (MultipleAudioLoader.Instance.currentIndex < 0)
        {
            MultipleAudioLoader.Instance.currentIndex = MultipleAudioLoader.Instance.clipFileNames.Count - 1;
        }

        StartCoroutine(PlayTrackCoroutine(MultipleAudioLoader.Instance.clipFileNames[MultipleAudioLoader.Instance.currentIndex]));
    }

    private void UpdateTrackName(string customMessage = null)
    {
        if (trackNameText == null) return;

        if (!string.IsNullOrEmpty(customMessage))
        {
            trackNameText.text = customMessage;
        }
        else if (MultipleAudioLoader.Instance.currentIndex >= 0 && 
                 MultipleAudioLoader.Instance.currentIndex < MultipleAudioLoader.Instance.clipFileNames.Count)
        {
            string trackName = MultipleAudioLoader.Instance.clipFileNames[MultipleAudioLoader.Instance.currentIndex];
            trackNameText.text = $"Now Playing: {trackName}";
        }
        else
        {
            trackNameText.text = "No Track Loaded";
        }
    }

    private void StartNewCoroutine(IEnumerator coroutine)
    {
        if (currentTrackCoroutine != null)
        {
            StopCoroutine(currentTrackCoroutine);
        }

        currentTrackCoroutine = StartCoroutine(coroutine);
    }

    private IEnumerator HandleTrackChange(System.Action trackChangeAction)
    {
        if (isOperationInProgress) yield break;

        isOperationInProgress = true;
        trackChangeAction();
        yield return new WaitForSeconds(debounceTime);
        isOperationInProgress = false;
    }

    private IEnumerator ResetOperationLock()
    {
        yield return new WaitForSeconds(0.1f);
        isOperationInProgress = false;
    }
}
