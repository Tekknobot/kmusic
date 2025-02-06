using UnityEngine;

public class MediaStoreManager : MonoBehaviour
{
    public void WriteToMusicDirectory(string fileName, string content)
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using (AndroidJavaObject mediaStoreHelper = new AndroidJavaObject("com.example.mediastorehelper.MediaStoreHelper", activity))
            {
                mediaStoreHelper.Call("writeToMusicDirectory", fileName, content);
            }
        }
        #endif
    }
}
