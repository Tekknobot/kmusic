using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MultipleAudioLoader : MonoBehaviour
{
    public static MultipleAudioLoader Instance { get; private set; }

    // For non-Android or pre-API30 platforms we use a directory path.
    public string directoryPath = "AudioFiles"; 
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

    public AudioBPMAdjuster audioBPMAdjuster;

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
#if UNITY_ANDROID && !UNITY_EDITOR
        if (AndroidVersionIsAtLeast30())
        {
            // When using MediaStore, we do not need to set directoryPath
            fullPath = "";
        }
        else
        {
            directoryPath = Path.Combine(Application.persistentDataPath, "AudioFiles");
            fullPath = directoryPath;
        }
#else
        directoryPath = Path.Combine(Application.persistentDataPath, "AudioFiles");
        fullPath = directoryPath;
#endif

        Debug.Log($"Audio directory path: {fullPath}");

        songTimeline.onValueChanged.AddListener(OnTimelineSliderChanged);

        // Add dragging listeners
        EventTrigger trigger = songTimeline.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = songTimeline.gameObject.AddComponent<EventTrigger>();
        }

        // Add OnDragStart
        EventTrigger.Entry dragStartEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        dragStartEntry.callback.AddListener((BaseEventData data) => { isUserDragging = true; });
        trigger.triggers.Add(dragStartEntry);

        // Add OnDragEnd
        EventTrigger.Entry dragEndEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        dragEndEntry.callback.AddListener((BaseEventData data) =>
        {
            isUserDragging = false;
            if (audioSource.clip != null)
            {
                audioSource.time = songTimeline.value; // Update playback position
            }
        });
        trigger.triggers.Add(dragEndEntry);

        // Initialize audio file loading
        StartCoroutine(InitializeAudioFiles());
    }

    private void Update()
    {
        if (audioSource.clip != null && !isUserDragging)
        {
            // Update the slider value based on the current playback position
            songTimeline.value = audioSource.time;
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

    private IEnumerator InitializeAudioFiles()
    {
        yield return new WaitForSeconds(1f);

#if UNITY_ANDROID && !UNITY_EDITOR
        if (AndroidVersionIsAtLeast30())
        {
            Debug.Log("Using MediaStore integration for Android API 30+.");
            QueryMediaStoreAudioFiles();
        }
        else
        {
            LoadAllAudioFiles();
        }
#else
        LoadAllAudioFiles();
#endif

        // Load the last played clip if it exists
        string lastLoadedClip = PlayerPrefs.GetString(LastLoadedClipKey, null);
        if (!string.IsNullOrEmpty(lastLoadedClip) && clipFileNames.Contains(lastLoadedClip))
        {
            yield return LoadClip(lastLoadedClip); // Use the LoadClip method
        }
        else if (clipFileNames.Count > 0)
        {
            // Optionally, you can automatically load the first track
            // yield return LoadAndPlayClip(clipFileNames[0]);
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    // Query MediaStore for audio file paths.
    private void QueryMediaStoreAudioFiles()
    {
        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");

            AndroidJavaClass mediaStoreAudioMedia = new AndroidJavaClass("android.provider.MediaStore$Audio$Media");
            AndroidJavaObject uri = mediaStoreAudioMedia.GetStatic<AndroidJavaObject>("EXTERNAL_CONTENT_URI");

            string[] projection = new string[] { "_data" };

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
                        // With MediaStore, store the full path.
                        clipFileNames.Add(filePath);
                        Debug.Log($"Found file from MediaStore: {filePath}");
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
                    // For file system scanning, store the relative path.
                    string relativePath = file.Replace(directoryPath + Path.DirectorySeparatorChar, "");
                    clipFileNames.Add(relativePath);
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

    // Load a clip from the given fileName (which may be a full path if coming from MediaStore)
    public IEnumerator LoadClip(string fileName)
    {
        if (clipDictionary.TryGetValue(fileName, out AudioClip loadedClip))
        {
            audioSource.clip = loadedClip;
            if(PatternManager.Instance != null)
                PatternManager.Instance.songClip = loadedClip;

            UpdateStatusText("Loaded: " + fileName);
            SaveCurrentClip(fileName);
            currentIndex = clipFileNames.IndexOf(fileName);
            SetTimelineSliderValues(loadedClip);
            yield break;
        }

        // If we are using MediaStore integration, fileName is a full path.
        string filePath = fileName;
        if (Application.platform != RuntimePlatform.Android || Application.isEditor || !AndroidVersionIsAtLeast30())
        {
            // For non-MediaStore mode, combine with directory path.
            filePath = Path.Combine(directoryPath, fileName);
        }

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
                if (PatternManager.Instance != null)
                    PatternManager.Instance.songClip = newClip;

                UpdateStatusText("Loaded: " + fileName);
                SaveCurrentClip(fileName);
                currentIndex = clipFileNames.IndexOf(fileName);
                SetTimelineSliderValues(newClip);
            }
            else
            {
                Debug.LogError($"Failed to load audio file: {www.error}");
                UpdateStatusText($"Failed to load: {fileName}");
            }
        }

        if (audioBPMAdjuster != null)
        {
            audioBPMAdjuster.FetchOriginalBPM();
        }
    }

    public IEnumerator LoadAndPlayClip(string fileName)
    {
        Debug.Log($"Loading and playing clip: {fileName}");

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

        string filePath = fileName;
        if (Application.platform != RuntimePlatform.Android || Application.isEditor || !AndroidVersionIsAtLeast30())
        {
            filePath = Path.Combine(fullPath, fileName);
        }
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
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned in MultipleAudioLoader.");
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();

        Debug.Log($"Now playing: {fileName}");
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

    // Replacing the C# 8.0 switch expression with a traditional switch statement
    private AudioType GetAudioType(string extension)
    {
        switch (extension)
        {
            case ".mp3":
                return AudioType.MPEG;
            case ".wav":
                return AudioType.WAV;
            default:
                return AudioType.UNKNOWN;
        }
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
