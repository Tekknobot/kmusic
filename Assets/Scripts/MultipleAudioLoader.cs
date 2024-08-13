using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using TMPro;

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

    private const string LastLoadedClipKey = "LastLoadedClip"; // Key for saving the last loaded clip name

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

        StartCoroutine(InitializeAudioFiles());
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
            audioSource.Play();
            UpdateStatusText("Playing: " + fileName);
            SaveCurrentClip(fileName);
            currentIndex = clipFileNames.IndexOf(fileName);
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
                audioSource.clip = newClip;
                audioSource.Play();
                UpdateStatusText("Playing: " + fileName);
                SaveCurrentClip(fileName);
                currentIndex = clipFileNames.IndexOf(fileName);
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
            }
            else
            {
                Debug.LogError("Failed to load audio file: " + www.error);
                UpdateStatusText("Failed to load: " + fileName);
            }
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
