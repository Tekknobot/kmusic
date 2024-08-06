using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO;
using UnityEngine.Networking; // Ensure this namespace is included

public class KMusicPlayer : MonoBehaviour
{
    public static KMusicPlayer Instance { get; private set; } // Singleton instance

    public AudioSource audioSource; // AudioSource to play audio clips
    public TMP_Text trackNameText;  // TextMesh Pro text to display the current track name

    private List<string> clipFileNames = new List<string>(); // List to store filenames
    private int currentIndex = -1; // Index of the currently playing track

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
        // Ensure the AudioSource is set
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned.");
            return;
        }

        // Load all audio file names
        LoadAudioFileNames();
    }

    // Load all audio file names from a directory
    private void LoadAudioFileNames()
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, "AudioFiles");
        if (Directory.Exists(directoryPath))
        {
            string[] files = Directory.GetFiles(directoryPath);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                clipFileNames.Add(fileName);
                Debug.Log("Found file: " + fileName);
            }
        }
        else
        {
            Debug.LogError("Directory does not exist: " + directoryPath);
        }
    }

    // Play a specific track by filename
    public void PlayTrack(string fileName)
    {
        if (clipFileNames.Contains(fileName))
        {
            StartCoroutine(LoadAndPlayClip(fileName));
        }
        else
        {
            Debug.LogError("File not found in the list: " + fileName);
        }
    }

    // Play the next track
    public void PlayNextTrack()
    {
        if (clipFileNames.Count == 0) return;

        // Determine the next track index
        currentIndex = (currentIndex + 1) % clipFileNames.Count;
        PlayTrack(clipFileNames[currentIndex]);
    }

    // Play the previous track
    public void PlayPreviousTrack()
    {
        if (clipFileNames.Count == 0) return;

        // Determine the previous track index
        currentIndex = (currentIndex - 1 + clipFileNames.Count) % clipFileNames.Count;
        PlayTrack(clipFileNames[currentIndex]);
    }

    // Stop playback
    public void StopPlayback()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            UpdateTrackName(null);
        }
    }

    // Load and play an audio clip
    private IEnumerator LoadAndPlayClip(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "AudioFiles", fileName);
        string fileUrl = "file://" + filePath;
        string extension = Path.GetExtension(fileName).ToLower();

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUrl, GetAudioType(extension)))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
                currentIndex = clipFileNames.IndexOf(fileName);
                UpdateTrackName(fileName);
            }
            else
            {
                Debug.LogError("Failed to load audio file: " + www.error);
                UpdateTrackName(null);
            }
        }
    }

    // Update the track name display
    private void UpdateTrackName(string fileName)
    {
        if (trackNameText != null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                trackNameText.text = "No Track Loaded";
            }
            else
            {
                trackNameText.text = "Now Playing: " + fileName;
            }
        }
    }

    // Get the type of audio file
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
}
