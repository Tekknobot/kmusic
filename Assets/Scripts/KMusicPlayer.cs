using UnityEngine;
using System.Collections;
using TMPro; // Import the TextMesh Pro namespace

public class KMusicPlayer : MonoBehaviour
{
    public MultipleAudioLoader audioLoader; // Reference to the MultipleAudioLoader script
    public AudioSource audioSource; // AudioSource to play audio clips
    public TMP_Text statusText; // UI TextMesh Pro element to display the status

    public int currentClipIndex = -1; // Index of the currently playing audio clip

    private void Start()
    {
        if (audioLoader == null || audioSource == null || statusText == null)
        {
            Debug.LogError("AudioLoader, AudioSource, or StatusText is not assigned.");
            return;
        }

        // Start with an invalid index
        currentClipIndex = -1;
    }

    public void PlayNextClip()
    {
        if (audioLoader.audioClips.Count == 0)
        {
            statusText.text = "No audio clips available.";
            return;
        }

        // Increment index and wrap around if needed
        currentClipIndex = (currentClipIndex + 1) % audioLoader.audioClips.Count;

        StartCoroutine(LoadAndPlayClipAtIndex(currentClipIndex));
    }

    public void PlayPreviousClip()
    {
        if (audioLoader.audioClips.Count == 0)
        {
            statusText.text = "No audio clips available.";
            return;
        }

        // Decrement index and wrap around if needed
        currentClipIndex = (currentClipIndex - 1 + audioLoader.audioClips.Count) % audioLoader.audioClips.Count;

        StartCoroutine(LoadAndPlayClipAtIndex(currentClipIndex));
    }

    public IEnumerator LoadAndPlayClipAtIndex(int index)
    {
        if (index >= 0 && index < audioLoader.audioClips.Count)
        {
            AudioClip clip = audioLoader.audioClips[index];

            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                statusText.text = "Playing: Sample " + (index + 1);
            }
            else
            {
                // Request loading if clip is not available
                yield return StartCoroutine(audioLoader.LoadAudioClip("clip_" + index + ".mp3", loadedClip =>
                {
                    if (loadedClip != null)
                    {
                        audioSource.clip = loadedClip;
                        audioSource.Play();
                        statusText.text = "Playing: Sample " + (index + 1);
                    }
                    else
                    {
                        statusText.text = "Failed to load clip.";
                    }
                }));
            }
        }
        else
        {
            Debug.LogError("Invalid clip index: " + index);
            statusText.text = "Invalid clip index.";
        }
    }

    public void Pause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            statusText.text = "Paused: Sample " + (currentClipIndex + 1);
        }
        else
        {
            statusText.text = "No clip is playing to pause.";
        }
    }

    public void Stop()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            statusText.text = "Stopped";
        }
        else
        {
            statusText.text = "No clip is playing to stop.";
        }
    }

    public void Resume()
    {
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.Play();
            statusText.text = "Resumed: Sample " + (currentClipIndex + 1);
        }
        else if (audioSource.clip == null)
        {
            statusText.text = "No clip is available to resume.";
        }
    }

    public IEnumerator LoadAndPlayClipByFileName(string fileName)
    {
        // Check if the clip is already loaded
        AudioClip clip = audioLoader.GetClipByFileName(fileName);
        if (clip == null)
        {
            // Load the clip if it's not already loaded
            yield return StartCoroutine(audioLoader.LoadAudioClip(fileName, loadedClip =>
            {
                if (loadedClip != null)
                {
                    audioSource.clip = loadedClip;
                    audioSource.Play();
                    // Assume file names follow "clip_1.mp3" format
                    int index = ExtractIndexFromFileName(fileName);
                    statusText.text = "Playing: Sample " + (index + 1);
                }
                else
                {
                    statusText.text = "Failed to load and play the audio clip: " + fileName;
                }
            }));
        }
        else
        {
            // Play the already loaded clip
            audioSource.clip = clip;
            audioSource.Play();
            int index = audioLoader.audioClips.IndexOf(clip);
            statusText.text = "Playing: Sample " + (index + 1);
        }
    }

    // Helper method to extract the index from the filename
    private int ExtractIndexFromFileName(string fileName)
    {
        // Example filename: "clip_1.mp3"
        string numberPart = fileName.Replace("clip_", "").Replace(".mp3", "");
        int index;
        if (int.TryParse(numberPart, out index))
        {
            return index;
        }
        else
        {
            Debug.LogError("Failed to parse index from filename: " + fileName);
            return 0;
        }
    }
}
