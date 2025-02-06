using UnityEngine;
using TMPro;

public class LogcatUI : MonoBehaviour
{
    [Tooltip("Reference to the TextMesh Pro UI element where logs will be displayed.")]
    public TMP_Text logText;

    // Optional: limit number of log lines to avoid memory issues
    public int maxLogLines = 100;

    // An internal list to store the logs.
    private readonly System.Collections.Generic.Queue<string> logQueue = new System.Collections.Generic.Queue<string>();

    private void Awake()
    {
        // Register our callback when a log message is received.
        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        // Unregister the callback.
        Application.logMessageReceived -= HandleLog;
    }

    /// <summary>
    /// Callback for Unity log messages. Only warnings, errors, and exceptions are logged.
    /// </summary>
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Only process Warning, Error, and Exception logs.
        if (type == LogType.Warning || type == LogType.Error || type == LogType.Exception)
        {
            // Format the log message.
            string formattedLog = $"[{type}] {logString}";
            if (type == LogType.Error || type == LogType.Exception)
            {
                formattedLog += "\n" + stackTrace;
            }

            // Enqueue the new log message.
            logQueue.Enqueue(formattedLog);

            // If we've exceeded the maximum number of lines, remove the oldest.
            while (logQueue.Count > maxLogLines)
            {
                logQueue.Dequeue();
            }

            // Rebuild the log text.
            logText.text = string.Join("\n", logQueue.ToArray());
        }
    }
}
