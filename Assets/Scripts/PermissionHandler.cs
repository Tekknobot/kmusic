using UnityEngine;
using System.Collections;

public class PermissionHandler : MonoBehaviour
{
    // Define permission strings for both legacy and new permissions.
    private const string LegacyReadPermission = "android.permission.READ_EXTERNAL_STORAGE";
    private const string NewReadPermission = "android.permission.READ_MEDIA_AUDIO";
    private const string LegacyWritePermission = "android.permission.WRITE_EXTERNAL_STORAGE";
    private const string ManageStoragePermission = "android.permission.MANAGE_EXTERNAL_STORAGE";

    private string GetReadPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            int sdkInt = versionClass.GetStatic<int>("SDK_INT");
            // For API 33 (Android 13) and above, use the new read permission.
            if (sdkInt >= 33)
                return NewReadPermission;
            else
                return LegacyReadPermission;
        }
#else
        return "";
#endif
    }

    private string GetWritePermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            int sdkInt = versionClass.GetStatic<int>("SDK_INT");
            // For API 30+ (Android 11 and above), request MANAGE_EXTERNAL_STORAGE.
            if (sdkInt >= 30)
                return ManageStoragePermission;
            else
                return LegacyWritePermission;
        }
#else
        return "";
#endif
    }

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log("Checking permissions on start...");
        RequestStoragePermissions();
        StartCoroutine(WaitForPermissionsThenDoMediaStoreOperation());
#endif
    }

    private void RequestStoragePermissions()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string readPermission = GetReadPermission();
        if (!HasPermission(readPermission))
        {
            Debug.Log($"{readPermission} is not granted. Requesting now...");
            RequestPermission(readPermission);
        }
        else
        {
            Debug.Log($"{readPermission} is already granted.");
        }

        string writePermission = GetWritePermission();
        if (!HasPermission(writePermission))
        {
            Debug.Log($"{writePermission} is not granted. Requesting now...");
            RequestPermission(writePermission);
        }
        else
        {
            Debug.Log($"{writePermission} is already granted.");
        }
#endif
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
        return true;
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

    private IEnumerator WaitForPermissionsThenDoMediaStoreOperation()
    {
        // Wait until both read and write permissions are granted
        while (!HasPermission(GetReadPermission()) || !HasPermission(GetWritePermission()))
        {
            Debug.Log("Waiting for permissions...");
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("Permissions granted. Proceeding with MediaStore operation.");
        PerformMediaStoreOperation();
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
