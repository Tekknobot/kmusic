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
    public TMP_Text logText;        // TextMesh Pro text to display logs

    public List<string> clipFileNames = new List<string>(); // List to store relative or full file paths
    public int currentIndex = -1; // Index of the currently playing track

    private string directoryPath; // Path to the music files (used on non-Android or pre-API30)

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
        Log("Starting KMusicPlayer...");
        LoadAudioFileNames();

        if (clipFileNames.Count > 0)
        {
            currentIndex = 0; // Start with the first track
            Log($"Found {clipFileNames.Count} tracks. Ready to play.");
        }
        else
        {
            Log("No tracks available to play.");
        }
    }

    private void UpdateTrackName(string trackPath)
    {
        if (trackNameText != null)
        {
            string trackName = Path.GetFileNameWithoutExtension(trackPath); // Extract file name
            trackNameText.text = "Now Playing: " + trackName;
            Log($"Track name updated: {trackName}");
        }
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

    // Load all audio file names from MediaStore on Android API 30+; else, use file system scanning.
    private void LoadAudioFileNames()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (AndroidVersionIsAtLeast30())
        {
            Log("Using MediaStore integration for Android API 30+.");
            QueryMediaStoreAudioFiles();
            return;
        }
#endif
        // Fallback: use file system scanning.
#if UNITY_ANDROID
        directoryPath = Path.Combine(Application.persistentDataPath, "AudioFiles"); // Fallback for older APIs
#else
        directoryPath = Path.Combine(Application.persistentDataPath, "AudioFiles");
#endif

        Debug.Log($"Scanning directory: {directoryPath}");

        if (Directory.Exists(directoryPath))
        {
            string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            Debug.Log($"Found {files.Length} total files in directory.");

            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                Debug.Log($"File found: {file}, Extension: {extension}");

                if (extension == ".mp3" || extension == ".wav")
                {
                    // For file system scanning, you may choose to store the relative path.
                    string relativePath = file.Replace(directoryPath + Path.DirectorySeparatorChar, "");
                    relativePath = relativePath.Replace("\\", "/"); // Normalize for cross-platform
                    clipFileNames.Add(relativePath);
                    Debug.Log($"Added audio file: {relativePath}");
                }
                else
                {
                    Debug.Log($"Skipped unsupported file: {file}");
                }
            }

            if (clipFileNames.Count == 0)
            {
                Debug.LogError("No audio files found with supported extensions (.mp3, .wav).");
            }
        }
        else
        {
            Debug.LogError($"Directory does not exist or is inaccessible: {directoryPath}");
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    // Query Android MediaStore to get audio file paths.
    private void QueryMediaStoreAudioFiles()
    {
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");

            AndroidJavaClass mediaStoreAudioMedia = new AndroidJavaClass("android.provider.MediaStore$Audio$Media");
            AndroidJavaObject uri = mediaStoreAudioMedia.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI");

            // We request only the _data column which contains the file path.
            string[] projection = new string[] { "_data" };

            // Query the MediaStore.
            AndroidJavaObject cursor = contentResolver.Call<AndroidJavaObject>(
                "query", uri, projection, null, null, null);

            if (cursor != null)
            {
                int dataIndex = cursor.Call<int>("getColumnIndex", "_data");
                while (cursor.Call<bool>("moveToNext"))
                {
                    string filePath = cursor.Call<string>("getString", dataIndex);
                    string extension = Path.GetExtension(filePath).ToLower();
                    if (extension == ".mp3" || extension == ".wav")
                    {
                        // Here, we store the full file path.
                        clipFileNames.Add(filePath);
                        Debug.Log($"Added audio file from MediaStore: {filePath}");
                    }
                }
                cursor.Call("close");
            }
            else
            {
                Debug.LogError("MediaStore query returned null cursor.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error querying MediaStore: " + ex);
        }
    }
#endif

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

    // Log messages to the TMP logText object
    private void Log(string message)
    {
        Debug.Log(message); // Always log to the Unity console as well
        if (logText != null)
        {
            logText.text += message + "\n"; // Append the message to the TMP_Text
        }
    }
}
