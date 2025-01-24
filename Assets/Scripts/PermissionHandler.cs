using UnityEngine;
using System.IO;

public class PermissionHandler : MonoBehaviour
{
    private const string ReadStoragePermission = "android.permission.READ_EXTERNAL_STORAGE";
    private const string WriteStoragePermission = "android.permission.WRITE_EXTERNAL_STORAGE";
    private const string ManageStoragePermission = "android.permission.MANAGE_EXTERNAL_STORAGE";

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log("Checking permissions on start...");
        RequestAllPermissions();
#endif
    }

    private void RequestAllPermissions()
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

        if (AndroidVersionIsAtLeast30() && !HasManageStorageAccess())
        {
            Debug.Log("MANAGE_EXTERNAL_STORAGE access is not granted. Opening settings...");
            OpenManageStorageSettings();
        }
        else if (AndroidVersionIsAtLeast30())
        {
            Debug.Log("MANAGE_EXTERNAL_STORAGE access is already granted.");
        }

        PerformFileOperation(); // Attempt a file operation to confirm permissions.
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

    private bool HasManageStorageAccess()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var environment = new AndroidJavaClass("android.os.Environment"))
        {
            bool isManageStorageGranted = environment.CallStatic<bool>("isExternalStorageManager");
            Debug.Log($"MANAGE_EXTERNAL_STORAGE access: {isManageStorageGranted}");
            return isManageStorageGranted;
        }
#else
        return true; // Assume access is granted on non-Android platforms.
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

    private void OpenManageStorageSettings()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log("Opening MANAGE_EXTERNAL_STORAGE settings...");
        using (var unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityActivity.GetStatic<AndroidJavaObject>("currentActivity");
            using (var intentClass = new AndroidJavaClass("android.content.Intent"))
            {
                var intent = new AndroidJavaObject("android.content.Intent", "android.settings.MANAGE_ALL_FILES_ACCESS_PERMISSION");
                activity.Call("startActivity", intent);
            }
        }
#endif
    }

    private bool AndroidVersionIsAtLeast30()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            int sdkInt = versionClass.GetStatic<int>("SDK_INT");
            Debug.Log($"Android SDK version: {sdkInt}");
            return sdkInt >= 30;
        }
#else
        return false;
#endif
    }

    private void PerformFileOperation()
    {
        string path = "/storage/emulated/0/Music/kmusicPermissionTest.txt"; // Common music directory on Android
        try
        {
            // Attempt to write a file
            File.WriteAllText(path, "Testing permissions");
            Debug.Log("Successfully wrote to file.");

            // Attempt to read the file
            string content = File.ReadAllText(path);
            Debug.Log($"Read from file: {content}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"File operation failed: {ex.Message}");
        }
    }
}
