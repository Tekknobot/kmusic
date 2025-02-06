using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public static class ChopSaver
{
    public static IEnumerator SaveRenderedChopsCoroutine(AudioClip sourceClip, List<float> chopTimestamps, string baseFileName)
    {
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

        // Ensure timestamps are sorted.
        chopTimestamps.Sort();

        // Set the folder path based on platform.
        string folderPath;
#if UNITY_ANDROID && !UNITY_EDITOR
        folderPath = System.IO.Path.Combine(GetAndroidMusicFolder(), "Chops");
#else
        folderPath = System.IO.Path.Combine(Application.persistentDataPath, "Chops");
#endif
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("ChopSaver: Created folder " + folderPath);
        }

        // Loop through timestamps and process one chop per iteration.
        for (int i = 0; i < chopTimestamps.Count - 1; i++)
        {
            float startTime = chopTimestamps[i];
            float endTime = chopTimestamps[i + 1];

            // Construct filename.
            string fileName = $"{baseFileName}_chop{i}.wav";
            string filePath = Path.Combine(folderPath, fileName);

            // Process and save this chop.
#if UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using (AndroidJavaObject mediaStoreHelper = new AndroidJavaObject("com.example.mediastorehelper.MediaStoreHelper", activity))
                {
                    mediaStoreHelper.Call("writeToMusicDirectory", fileName, "Chopped audio data");
                }
            }
#else
            AudioExporter.SaveAudioSegmentToWav(sourceClip, startTime, endTime, filePath);
#endif
            
            // Optionally yield to avoid blocking.
            yield return null;
        }
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
