using UnityEngine;

public class MediaStoreManager : MonoBehaviour
{
    /// <summary>
    /// Writes binary data into the specified folder within the Music directory using MediaStore integration.
    /// </summary>
    /// <param name="folderPath">The full path of the target folder (e.g. Music/Chops).</param>
    /// <param name="fileName">The name of the file to create.</param>
    /// <param name="data">The binary data to write (for example, WAV file data).</param>
    public void WriteToMusicDirectory(string folderPath, string fileName, byte[] data)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using (AndroidJavaObject mediaStoreHelper = new AndroidJavaObject("com.example.mediastorehelper.MediaStoreHelper", activity))
                {
                    mediaStoreHelper.Call("writeToMusicDirectory", folderPath, fileName, data);
                    Debug.Log($"MediaStoreManager: Successfully wrote {fileName} to {folderPath}.");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("MediaStoreManager: Exception writing to Music directory: " + ex.Message);
        }
#else
        Debug.LogWarning("MediaStoreManager: WriteToMusicDirectory is only implemented for Android.");
#endif
    }
}
