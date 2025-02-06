using UnityEngine;
using System.IO;
using System.Collections;

public class PermissionHandler : MonoBehaviour
{
    private const string ReadStoragePermission = "android.permission.READ_EXTERNAL_STORAGE";
    private const string WriteStoragePermission = "android.permission.WRITE_EXTERNAL_STORAGE";

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log("Checking permissions on start...");
        RequestStoragePermissions();
#endif
    }

    private void RequestStoragePermissions()
    {
        if (!HasPermission(ReadStoragePermission))
        {
            Debug.Log("READ_EXTERNAL_STORAGE permission is not granted. Requesting now...");
            RequestPermission(ReadStoragePermission);
        }
        else
        {
            Debug.Log("READ_EXTERNAL_STORAGE permission is already granted.");
        }

        if (!HasPermission(WriteStoragePermission))
        {
            Debug.Log("WRITE_EXTERNAL_STORAGE permission is not granted. Requesting now...");
            RequestPermission(WriteStoragePermission);
        }
        else
        {
            Debug.Log("WRITE_EXTERNAL_STORAGE permission is already granted.");
        }
    }

    private bool HasPermission(string permission)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityActivity.GetStatic<AndroidJavaObject>("currentActivity");
            using (var contextCompat = new AndroidJavaClass("androidx.core.content.ContextCompat"))
            {
                int result = contextCompat.CallStatic<int>("checkSelfPermission", activity, permission);
                Debug.Log($"{permission} check result: {result}");
                return result == 0; // PERMISSION_GRANTED
            }
        }
#else
        return true; // Assume permission is granted on non-Android platforms.
#endif
    }

    private void RequestPermission(string permission)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log($"Requesting permission: {permission}");
        using (var unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityActivity.GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("requestPermissions", new string[] { permission }, 0);
        }
#endif
    }

    private void PerformMediaStoreOperation()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log("Attempting to write to MediaStore...");
        using (var unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityActivity.GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                using (var mediaStoreHelper = new AndroidJavaObject("com.example.mediastorehelper.MediaStoreHelper", activity))
                {
                    mediaStoreHelper.Call("writeToMusicDirectory", "kmusicPermissionTest.txt", "Testing permissions");
                }
            }));
        }
#endif
    }
}
