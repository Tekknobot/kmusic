using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using TMPro; // Import the TextMesh Pro namespace

public class MultipleAudioLoader : MonoBehaviour
{
    public static MultipleAudioLoader Instance { get; private set; } // Singleton instance

    public string directoryPath = "AudioFiles"; // Sub-directory in the persistent data path
    private string fullPath;
    public AudioSource audioSource; // AudioSource to play audio clips

    public List<AudioClip> audioClips = new List<AudioClip>(); // List to hold loaded audio clips
    private Dictionary<string, AudioClip> clipDictionary = new Dictionary<string, AudioClip>(); // Dictionary to map filenames to clips
    public TMP_Text statusText; // UI TextMesh Pro element to display the status

    private int currentClipIndex = -1; // Index of the currently playing clip

    private void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ensure this instance persists across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    private void Start()
    {
        fullPath = Path.Combine(Application.persistentDataPath, directoryPath);
        Debug.Log("Audio directory path: " + fullPath);

        // Test if files exist
        if (Directory.Exists(fullPath))
        {
            string[] files = Directory.GetFiles(fullPath);
            foreach (string file in files)
            {
                Debug.Log("Found file: " + file);
            }

            // Load all audio files
            StartCoroutine(LoadAllAudioClips());
        }
        else
        {
            Debug.LogError("Directory does not exist: " + fullPath);
        }
    }

    private IEnumerator LoadAllAudioClips()
    {
        string[] files = Directory.GetFiles(fullPath);
        for (int i = 0; i < files.Length; i++)
        {
            string fileName = Path.GetFileName(files[i]);
            yield return StartCoroutine(LoadAudioClip(fileName));
        }

        if (audioClips.Count > 0)
        {
            currentClipIndex = 0; // Set the initial clip index
            Debug.Log("All audio clips loaded. Ready to play.");
        }
        else
        {
            Debug.LogError("No audio clips were loaded.");
        }
    }

    // Method to load an audio clip by filename
    public IEnumerator LoadAudioClip(string fileName, System.Action<AudioClip> onClipLoaded = null)
    {
        string filePath = Path.Combine(fullPath, fileName);
        string fileUrl = "file://" + filePath;
        string extension = Path.GetExtension(fileName).ToLower();

        // Debug log to verify the URL
        Debug.Log("Loading audio clip from: " + fileUrl);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUrl, GetAudioType(extension)))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioClips.Add(clip); // Cache the loaded clip
                clipDictionary[fileName] = clip; // Map filename to the loaded clip

                // Debug log to confirm successful loading
                Debug.Log("Successfully loaded audio clip: " + fileName);
                onClipLoaded?.Invoke(clip);
            }
            else
            {
                Debug.LogError("Failed to load audio file: " + www.error);
                onClipLoaded?.Invoke(null);
            }
        }
    }

    // Method to load and play the next audio clip
    public void PlayNextClip()
    {
        if (audioClips.Count > 0)
        {
            currentClipIndex = (currentClipIndex + 1) % audioClips.Count;
            StartCoroutine(PlayClipAtIndex(currentClipIndex));
        }
        else
        {
            Debug.LogError("No audio clips available to play.");
        }
    }

    // Method to load and play the previous audio clip
    public void PlayPreviousClip()
    {
        if (audioClips.Count > 0)
        {
            currentClipIndex = (currentClipIndex - 1 + audioClips.Count) % audioClips.Count;
            StartCoroutine(PlayClipAtIndex(currentClipIndex));
        }
        else
        {
            Debug.LogError("No audio clips available to play.");
        }
    }

    private IEnumerator PlayClipAtIndex(int index)
    {
        if (index >= 0 && index < audioClips.Count)
        {
            AudioClip clip = audioClips[index];
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log("Playing audio clip: " + clip.name);

                // Update status text if needed
                if (statusText != null)
                {
                    statusText.text = "Playing: " + clip.name;
                }
            }
            else
            {
                Debug.LogError("Audio clip at index " + index + " is null.");
            }
        }
        else
        {
            Debug.LogError("Invalid clip index: " + index);
        }

        yield return null; // Ensure the coroutine completes
    }

    private AudioType GetAudioType(string extension)
    {
        switch (extension)
        {
            case ".wav":
                return AudioType.WAV;
            case ".mp3":
                return AudioType.MPEG;
            default:
                Debug.LogError("Unsupported audio file type: " + extension);
                return AudioType.UNKNOWN;
        }
    }

    // Method to play an audio clip by filename
    public void PlayAudioClip(string fileName)
    {
        StartCoroutine(LoadAudioClip(fileName, clip =>
        {
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();

                // Debug log to confirm playback
                Debug.Log("Playing audio clip: " + fileName);

                // Update status text if needed
                if (statusText != null)
                {
                    statusText.text = "Playing: " + fileName;
                }
            }
            else
            {
                Debug.LogError("Failed to play audio clip: " + fileName);
            }
        }));
    }

    // Method to load an audio clip without playing it
    public IEnumerator LoadAudioClipWithoutPlaying(string fileName)
    {
        yield return StartCoroutine(LoadAudioClip(fileName));
    }

    // Method to get a clip by filename
    public AudioClip GetClipByFileName(string fileName)
    {
        clipDictionary.TryGetValue(fileName, out AudioClip clip);
        return clip;
    }
}
