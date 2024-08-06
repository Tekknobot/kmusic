using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import the TextMesh Pro namespace

public class MusicPlayerUIController : MonoBehaviour
{
    public KMusicPlayer musicPlayer; // Reference to the KMusicPlayer script

    // UI Buttons
    public Button playNextButton;
    public Button playPreviousButton;
    public Button playButton;
    public Button pauseButton;
    public Button stopButton;
    public Button resumeButton;

    // UI Text
    public TMP_Text statusText; // TextMesh Pro text to display the status

    private void Start()
    {
        // Check if the musicPlayer is assigned
        if (musicPlayer == null)
        {
            Debug.LogError("KMusicPlayer is not assigned.");
            return;
        }

        // Check if buttons are assigned and add listeners
        if (playNextButton != null)
        {
            playNextButton.onClick.AddListener(OnPlayNextButtonClicked);
        }
        else
        {
            Debug.LogError("PlayNextButton is not assigned.");
        }

        if (playPreviousButton != null)
        {
            playPreviousButton.onClick.AddListener(OnPlayPreviousButtonClicked);
        }
        else
        {
            Debug.LogError("PlayPreviousButton is not assigned.");
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        else
        {
            Debug.LogError("PlayButton is not assigned.");
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
        }
        else
        {
            Debug.LogError("PauseButton is not assigned.");
        }

        if (stopButton != null)
        {
            stopButton.onClick.AddListener(OnStopButtonClicked);
        }
        else
        {
            Debug.LogError("StopButton is not assigned.");
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
        }
        else
        {
            Debug.LogError("ResumeButton is not assigned.");
        }

        // Set initial status text
        if (statusText != null)
        {
            statusText.text = "Ready to play";
        }
        else
        {
            Debug.LogError("StatusText is not assigned.");
        }
    }

    // Button click handlers
    private void OnPlayNextButtonClicked()
    {
        if (musicPlayer != null)
        {
            musicPlayer.PlayNextClip();
            UpdateStatusText();
        }
    }

    private void OnPlayPreviousButtonClicked()
    {
        if (musicPlayer != null)
        {
            musicPlayer.PlayPreviousClip();
            UpdateStatusText();
        }
    }

    private void OnPlayButtonClicked()
    {
        if (musicPlayer != null && musicPlayer.currentClipIndex >= 0)
        {
            StartCoroutine(musicPlayer.LoadAndPlayClipAtIndex(musicPlayer.currentClipIndex));
            UpdateStatusText();
        }
        else
        {
            Debug.LogError("Current clip index is not valid for playback.");
        }
    }

    private void OnPauseButtonClicked()
    {
        if (musicPlayer != null)
        {
            musicPlayer.Pause();
            UpdateStatusText();
        }
    }

    private void OnStopButtonClicked()
    {
        if (musicPlayer != null)
        {
            musicPlayer.Stop();
            UpdateStatusText();
        }
    }

    private void OnResumeButtonClicked()
    {
        if (musicPlayer != null)
        {
            musicPlayer.Resume();
            UpdateStatusText();
        }
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            statusText.text = musicPlayer.statusText.text;
        }
    }
}
