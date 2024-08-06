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

        // Initialize index
        currentClipIndex = -1;
    }

    public void PlayNextClip()
    {
        if (audioLoader.audioClips.Count == 0)
        {
            statusText.text = "No audio clips available.";
            return;
        }

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
        AudioClip clip = audioLoader.GetClipByFileName(fileName);
        if (clip == null)
        {
            yield return StartCoroutine(audioLoader.LoadAudioClip(fileName, loadedClip =>
            {
                if (loadedClip != null)
                {
                    audioSource.clip = loadedClip;
                    audioSource.Play();
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
            audioSource.clip = clip;
            audioSource.Play();
            int index = audioLoader.audioClips.IndexOf(clip);
            statusText.text = "Playing: Sample " + (index + 1);
        }
    }

    private int ExtractIndexFromFileName(string fileName)
    {
        string numberPart = fileName.Replace("clip_", "").Replace(".mp3", "");
        if (int.TryParse(numberPart, out int index))
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
