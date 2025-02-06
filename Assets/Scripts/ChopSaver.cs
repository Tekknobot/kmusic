using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class ChopSaver
{
    /// <summary>
    /// Saves rendered audio segments (chops) based on provided timestamps.
    /// On Android, this saves into the device’s Music/Chops folder via MediaStore integration.
    /// On other platforms, it saves to Application.persistentDataPath/Chops.
    /// If a file already exists for a given chop, it is overwritten.
    /// </summary>
    /// <param name="sourceClip">The source AudioClip.</param>
    /// <param name="chopTimestamps">A sorted list of timestamps (in seconds) defining chop boundaries.</param>
    /// <param name="baseFileName">Base filename for the chops.</param>
    public static IEnumerator SaveRenderedChopsCoroutine(AudioClip sourceClip, List<float> chopTimestamps, string baseFileName)
    {
        Debug.Log("ChopSaver: SaveRenderedChopsCoroutine started.");
        
        if (sourceClip == null)
        {
            Debug.LogError("ChopSaver: Source clip is null.");
            yield break;
        }
        if (chopTimestamps == null || chopTimestamps.Count < 2)
        {
            Debug.LogWarning("ChopSaver: At least two timestamps are required to define a chop.");
            yield break;
        }

        // Ensure timestamps are sorted
        chopTimestamps.Sort();
        Debug.Log("ChopSaver: Timestamps sorted. Count: " + chopTimestamps.Count);

        // Determine target folder path
        string folderPath;
#if UNITY_ANDROID && !UNITY_EDITOR
        folderPath = Path.Combine(GetAndroidMusicFolder(), "Chops");
#else
        folderPath = Path.Combine(Application.persistentDataPath, "Chops");
#endif
        Debug.Log("ChopSaver: Target folder path: " + folderPath);

        // Ensure the folder exists
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using (AndroidJavaObject mediaStoreHelper = new AndroidJavaObject("com.example.mediastorehelper.MediaStoreHelper", activity))
                {
                    bool folderExists = mediaStoreHelper.Call<bool>("folderExists", folderPath);
                    Debug.Log("ChopSaver: MediaStore helper reports folderExists: " + folderExists);
                    if (!folderExists)
                    {
                        mediaStoreHelper.Call("createFolder", folderPath);
                        Debug.Log("ChopSaver: Created folder " + folderPath + " via MediaStore helper.");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("ChopSaver: Exception creating folder on Android: " + ex.Message);
        }
#else
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("ChopSaver: Created folder " + folderPath);
        }
#endif

        // Process each chop (segment)
        for (int i = 0; i < chopTimestamps.Count - 1; i++)
        {
            float startTime = chopTimestamps[i];
            float endTime = chopTimestamps[i + 1];

            string fileName = $"{baseFileName}_chop{i}.wav";
            string fullFilePath = Path.Combine(folderPath, fileName);
            Debug.Log($"ChopSaver: Processing chop {i}: {startTime} - {endTime}, File: {fileName}");

#if UNITY_ANDROID && !UNITY_EDITOR
            // Convert the audio segment into WAV byte data.
            byte[] wavData = null;
            try
            {
                wavData = AudioExporter.ExportAudioSegmentToWavBytes(sourceClip, startTime, endTime);
                if (wavData == null || wavData.Length == 0)
                {
                    Debug.LogError($"ChopSaver: ExportAudioSegmentToWavBytes returned no data for chop {i}.");
                    continue;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ChopSaver: Exception exporting WAV data for chop {i}: {ex.Message}");
                continue;
            }

            // Use the MediaStore helper to check for, delete, and then write the file.
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    using (AndroidJavaObject mediaStoreHelper = new AndroidJavaObject("com.example.mediastorehelper.MediaStoreHelper", activity))
                    {
                        bool fileExists = mediaStoreHelper.Call<bool>("fileExists", folderPath, fileName);
                        if (fileExists)
                        {
                            mediaStoreHelper.Call("deleteFile", folderPath, fileName);
                            Debug.Log($"ChopSaver: Existing file {fileName} deleted via MediaStore helper.");
                        }
                        
                        mediaStoreHelper.Call("writeToMusicDirectory", folderPath, fileName, wavData);
                        Debug.Log($"ChopSaver: Successfully wrote chop {i} to MediaStore.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ChopSaver: Exception writing chop {i} via MediaStore helper: {ex.Message}");
            }
#else
            // Non-Android: Delete existing file if present before writing a new one.
            try
            {
                if (File.Exists(fullFilePath))
                {
                    File.Delete(fullFilePath);
                    Debug.Log($"ChopSaver: Existing file {fullFilePath} deleted.");
                }

                AudioExporter.SaveAudioSegmentToWav(sourceClip, startTime, endTime, fullFilePath);
                Debug.Log($"ChopSaver: Successfully saved chop {i} to {fullFilePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ChopSaver: Exception saving chop {i}: {ex.Message}");
            }
#endif
            yield return null;
        }
        Debug.Log("ChopSaver: SaveRenderedChopsCoroutine completed.");
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static string GetAndroidMusicFolder()
    {
        using (AndroidJavaClass env = new AndroidJavaClass("android.os.Environment"))
        {
            AndroidJavaObject musicDir = env.CallStatic<AndroidJavaObject>("getExternalStoragePublicDirectory", env.GetStatic<string>("DIRECTORY_MUSIC"));
            return musicDir.Call<string>("getAbsolutePath");
        }
    }
#else
    private static string GetAndroidMusicFolder()
    {
        return Application.persistentDataPath;
    }
#endif
}
