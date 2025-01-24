using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.IO;
using UnityEngine.Networking;

public class KMusicPlayer : MonoBehaviour
{
    public static KMusicPlayer Instance { get; private set; } // Singleton instance

    public AudioSource audioSource; // AudioSource to play audio clips
    public TMP_Text trackNameText;  // TextMesh Pro text to display the current track name

    public List<string> clipFileNames = new List<string>(); // List to store filenames
    public int currentIndex = -1; // Index of the currently playing track

    private string directoryPath; // Path to the music files

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
        // Determine the music directory path
#if UNITY_ANDROID
        if (AndroidVersionIsAtLeast30())
        {
            directoryPath = "/storage/emulated/0/Music"; // Android Music folder for API 30+
        }
        else
        {
            directoryPath = Application.persistentDataPath + "/AudioFiles"; // Fallback for older APIs
        }
#else
        directoryPath = Path.Combine(Application.persistentDataPath, "AudioFiles"); // Default for other platforms
#endif

        Debug.Log("Music directory: " + directoryPath);

        // Ensure the AudioSource is set
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned.");
            return;
        }

        // Load all audio file names
        LoadAudioFileNames();
    }

    private bool AndroidVersionIsAtLeast30()
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        using (var versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            int sdkInt = versionClass.GetStatic<int>("SDK_INT");
            return sdkInt >= 30;
        }
    #else
        return false; // Assume false when not on Android or in the Unity Editor
    #endif
    }


    // Load all audio file names from the directory
    private void LoadAudioFileNames()
    {
    #if UNITY_ANDROID
        // Use Android's Music folder path
        if (AndroidVersionIsAtLeast30())
        {
            directoryPath = "/storage/emulated/0/Music"; // Android Music folder for API 30+
        }
        else
        {
            directoryPath = Path.Combine(Application.persistentDataPath, "AudioFiles"); // Fallback for older APIs
        }
    #else
        // Default path for other platforms
        directoryPath = Path.Combine(Application.persistentDataPath, "AudioFiles");
    #endif

        if (Directory.Exists(directoryPath))
        {
            // Recursively find all files with supported extensions
            string[] files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (extension == ".mp3" || extension == ".wav") // Supported formats
                {
                    string fileName = Path.GetFileName(file);
                    clipFileNames.Add(fileName);
                    Debug.Log($"Found file: {fileName} in {file}");
                }
            }

            if (clipFileNames.Count == 0)
            {
                Debug.LogError("No audio files found in the Music folder or subdirectories.");
            }
        }
        else
        {
            Debug.LogError($"Directory does not exist: {directoryPath}");
        }
    }


    // Get the type of audio file
    private AudioType GetAudioType(string extension)
    {
        return extension switch
        {
            ".wav" => AudioType.WAV,
            ".mp3" => AudioType.MPEG,
            _ => AudioType.UNKNOWN
        };
    }

    // Update the track name text
    private void UpdateTrackName(string trackName)
    {
        if (trackNameText != null)
        {
            trackNameText.text = "Now Playing: " + trackName;
        }
    }
}
