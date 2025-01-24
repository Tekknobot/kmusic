using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class MultipleAudioLoader : MonoBehaviour
{
    public static MultipleAudioLoader Instance { get; private set; }

    public string directoryPath = "AudioFiles"; // Default for other platforms
    private string fullPath; // Complete path to the directory
    public AudioSource audioSource;
    public TMP_Text statusText;

    private Dictionary<string, AudioClip> clipDictionary = new Dictionary<string, AudioClip>();
    public List<string> clipFileNames = new List<string>();
    public Slider songTimeline;
    public GameObject waveform;

    public int currentIndex = -1; // To track the current audio clip
    private bool isUserDragging = false;
    private Coroutine currentLoadCoroutine;
    private const string LastLoadedClipKey = "LastLoadedClip";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
#if UNITY_ANDROID
        if (AndroidVersionIsAtLeast30())
        {
            directoryPath = "/storage/emulated/0/Music"; // Android Music folder for API 30+
        }
        else
        {
            directoryPath = Path.Combine(Application.persistentDataPath, "AudioFiles"); // Scoped storage for API 29
        }
#else
        directoryPath = Path.Combine(Application.persistentDataPath, "AudioFiles"); // Default for non-Android platforms
#endif

        fullPath = directoryPath;

        Debug.Log($"Audio directory path: {fullPath}");

        songTimeline.onValueChanged.AddListener(OnTimelineSliderChanged);
        StartCoroutine(InitializeAudioFiles());
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


    private IEnumerator InitializeAudioFiles()
    {
        yield return new WaitForSeconds(1f);
        LoadAllAudioFiles();

        // Load the last played clip if it exists
        string lastLoadedClip = PlayerPrefs.GetString(LastLoadedClipKey, null);
        if (!string.IsNullOrEmpty(lastLoadedClip))
        {
            yield return LoadClip(lastLoadedClip);
        }
    }

    public IEnumerator LoadClip(string fileName)
    {
        if (clipDictionary.TryGetValue(fileName, out AudioClip loadedClip))
        {
            audioSource.clip = loadedClip;
            UpdateStatusText("Loaded: " + fileName);
            SaveCurrentClip(fileName); // Save the currently loaded clip
            currentIndex = clipFileNames.IndexOf(fileName); // Update the index
            SetTimelineSliderValues(loadedClip); // Set the slider min/max values
            yield break;
        }

        string filePath = Path.Combine(directoryPath, fileName); // Resolve full path
        string fileUrl = "file://" + filePath;
        string extension = Path.GetExtension(fileName).ToLower();

        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            UpdateStatusText($"File not found: {fileName}");
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUrl, GetAudioType(extension)))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip newClip = DownloadHandlerAudioClip.GetContent(www);
                clipDictionary[fileName] = newClip;
                audioSource.clip = newClip;
                UpdateStatusText("Loaded: " + fileName);
                SaveCurrentClip(fileName); // Save the currently loaded clip
                currentIndex = clipFileNames.IndexOf(fileName); // Update the index
                SetTimelineSliderValues(newClip); // Set the slider min/max values
            }
            else
            {
                Debug.LogError($"Failed to load audio file: {www.error}");
                UpdateStatusText($"Failed to load: {fileName}");
            }
        }
    }


    private void LoadAllAudioFiles()
    {
        if (Directory.Exists(directoryPath))
        {
            // Get all files recursively
            string[] files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (extension == ".mp3" || extension == ".wav") // Filter supported formats
                {
                    string relativePath = file.Replace(directoryPath + Path.DirectorySeparatorChar, ""); // Get relative path
                    clipFileNames.Add(relativePath); // Store the relative path
                    Debug.Log($"Found file: {relativePath}");
                }
            }

            if (clipFileNames.Count == 0)
            {
                Debug.LogError("No audio files found in directory: " + directoryPath);
            }
        }
        else
        {
            Debug.LogError("Directory does not exist: " + directoryPath);
        }
    }



    public IEnumerator LoadAndPlayClip(string fileName)
    {
        Debug.Log($"Loading and playing clip: {fileName}");

        // Stop any ongoing load coroutine
        if (currentLoadCoroutine != null)
        {
            StopCoroutine(currentLoadCoroutine);
        }

        if (clipDictionary.TryGetValue(fileName, out AudioClip cachedClip))
        {
            Debug.Log("Clip found in cache.");
            audioSource.clip = cachedClip;
            PlayAudioClip(cachedClip, fileName);
            yield break;
        }

        string filePath = Path.Combine(fullPath, fileName);
        string fileUrl = "file://" + filePath;
        string extension = Path.GetExtension(fileName).ToLower();

        Debug.Log($"Loading audio clip from: {fileUrl}");

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUrl, GetAudioType(extension)))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Audio clip loaded successfully.");
                AudioClip newClip = DownloadHandlerAudioClip.GetContent(www);
                clipDictionary[fileName] = newClip;
                PlayAudioClip(newClip, fileName);
            }
            else
            {
                Debug.LogError($"Failed to load audio clip: {www.error}");
                UpdateStatusText($"Failed to play: {fileName}");
            }
        }
    }

    private void PlayAudioClip(AudioClip clip, string fileName)
    {
        audioSource.clip = clip;
        audioSource.Play();
        UpdateStatusText($"Playing: {fileName}");
        SaveCurrentClip(fileName);
        currentIndex = clipFileNames.IndexOf(fileName);
        SetTimelineSliderValues(clip);
    }

    private void SetTimelineSliderValues(AudioClip clip)
    {
        songTimeline.minValue = 0;
        songTimeline.maxValue = clip.length;
        songTimeline.value = 0; // Reset slider
    }

    private void OnTimelineSliderChanged(float value)
    {
        if (audioSource.clip != null && isUserDragging)
        {
            audioSource.time = value;
            UpdateStatusText($"Seeking to: {value} sec");
        }
    }

    private AudioType GetAudioType(string extension)
    {
        return extension switch
        {
            ".mp3" => AudioType.MPEG,
            ".wav" => AudioType.WAV,
            _ => AudioType.UNKNOWN
        };
    }

    private void UpdateStatusText(string text)
    {
        if (statusText != null)
        {
            statusText.text = text;
        }
    }

    private void SaveCurrentClip(string fileName)
    {
        PlayerPrefs.SetString(LastLoadedClipKey, fileName);
        PlayerPrefs.Save();
    }
}
