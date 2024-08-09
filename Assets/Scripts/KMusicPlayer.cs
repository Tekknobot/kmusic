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

    public List<string> clipFileNames = new List<string>(); // List to store filenames
    public int currentIndex = -1; // Index of the currently playing track

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
