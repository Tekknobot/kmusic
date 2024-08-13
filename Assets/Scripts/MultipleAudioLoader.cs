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

    public string directoryPath = "AudioFiles";
    private string fullPath;
    public AudioSource audioSource;
    public TMP_Text statusText;

    private Dictionary<string, AudioClip> clipDictionary = new Dictionary<string, AudioClip>();
    public List<string> clipFileNames = new List<string>();
    private string[] allFilePaths;
    public AudioClip currentClip;
    public GameObject waveform;
    public int currentIndex = -1; // To keep track of the current clip index

    public Slider songTimeline;

    private const string LastLoadedClipKey = "LastLoadedClip"; // Key for saving the last loaded clip name

    private bool isUserDragging = false; // To keep track if the user is dragging the slider

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
        Debug.Log("Start method called.");
        fullPath = Path.Combine(Application.persistentDataPath, directoryPath);
        Debug.Log("Audio directory path: " + fullPath);

        // Add listener for slider value changes
        songTimeline.onValueChanged.AddListener(OnTimelineSliderChanged);
        songTimeline.onValueChanged.AddListener(delegate { isUserDragging = true; });

        StartCoroutine(InitializeAudioFiles());
    }

    private void Update()
    {
        if (audioSource.isPlaying && !isUserDragging)
        {
            songTimeline.value = audioSource.time; // Update slider to reflect the current playback time
        }

        if (!audioSource.isPlaying && isUserDragging)
        {
            isUserDragging = false;
        }
    }

    private IEnumerator InitializeAudioFiles()
    {
        yield return new WaitForSeconds(1f);
        LoadAllAudioFiles();

        // Load the last played clip if it exists
        string lastLoadedClip = PlayerPrefs.GetString(LastLoadedClipKey, null);
        if (!string.IsNullOrEmpty(lastLoadedClip))
        {
            StartCoroutine(LoadClip(lastLoadedClip));
        }
    }

    private void LoadAllAudioFiles()
    {
        if (Directory.Exists(fullPath))
        {
            string[] files = Directory.GetFiles(fullPath);
            allFilePaths = files;

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                clipFileNames.Add(fileName);
                Debug.Log("Found file: " + fileName);
            }
        }
        else
        {
            Debug.LogError("Directory does not exist: " + fullPath);
        }
    }

    public IEnumerator LoadAndPlayClip(string fileName)
    {
        Debug.Log("Starting to load and play clip: " + fileName);

        if (clipDictionary.TryGetValue(fileName, out AudioClip loadedClip))
        {
            Debug.Log("Clip found in dictionary.");
            audioSource.clip = loadedClip;
            PlayAudioClip(loadedClip, fileName);
            yield break;
        }

        string filePath = Path.Combine(fullPath, fileName);
        string fileUrl = "file://" + filePath;
        string extension = Path.GetExtension(fileName).ToLower();

        Debug.Log("Loading audio clip from: " + fileUrl);

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUrl, GetAudioType(extension)))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Successfully loaded audio clip.");
                if (currentClip != null)
                {
                    Resources.UnloadAsset(currentClip);
                    Debug.Log("Unloaded previous clip.");
                }

                AudioClip newClip = DownloadHandlerAudioClip.GetContent(www);
                if (newClip == null)
                {
                    Debug.LogError("AudioClip is null. Failed to load: " + fileName);
                    UpdateStatusText("Failed to load: " + fileName);
                    yield break;
                }

                clipDictionary[fileName] = newClip;
                currentClip = newClip;
                PlayAudioClip(newClip, fileName);
            }
            else
            {
                Debug.LogError("Failed to load audio file: " + www.error);
                UpdateStatusText("Failed to play: " + fileName);
            }
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

        string filePath = Path.Combine(fullPath, fileName);
        string fileUrl = "file://" + filePath;
        string extension = Path.GetExtension(fileName).ToLower();

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
                Debug.LogError("Failed to load audio file: " + www.error);
                UpdateStatusText("Failed to load: " + fileName);
            }
        }
    }

    private void PlayAudioClip(AudioClip clip, string fileName)
    {
        audioSource.clip = clip;
        audioSource.Play();
        UpdateStatusText("Playing: " + fileName);
        SaveCurrentClip(fileName);
        currentIndex = clipFileNames.IndexOf(fileName);
        SetTimelineSliderValues(clip); // Set the slider min/max values
    }

    private void SetTimelineSliderValues(AudioClip clip)
    {
        songTimeline.minValue = 0;
        songTimeline.maxValue = clip.length;
        songTimeline.value = 0; // Reset the slider to the start
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
