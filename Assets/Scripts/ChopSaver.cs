using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class ChopSaver
{
    /// <summary>
    /// Saves rendered chops from the source clip into separate WAV files.
    /// The folder "Music/Chops" will be created (if it doesn’t exist) inside Application.persistentDataPath.
    /// Each chop is saved as [baseFileName]_chopX.wav.
    /// </summary>
    /// <param name="sourceClip">The AudioClip to chop.</param>
    /// <param name="chopTimestamps">
    /// A list of timestamps (in seconds) marking the boundaries of chops.
    /// Each consecutive pair [i, i+1] defines one chop.
    /// </param>
    /// <param name="baseFileName">The base filename for each chop.</param>
    public static void SaveRenderedChops(AudioClip sourceClip, List<float> chopTimestamps, string baseFileName)
    {
        if (sourceClip == null)
        {
            Debug.LogError("ChopSaver: Source clip is null.");
            return;
        }
        if (chopTimestamps == null || chopTimestamps.Count < 2)
        {
            Debug.LogWarning("ChopSaver: At least two timestamps are required to define a chop.");
            return;
        }

        // Ensure the chop timestamps are sorted.
        chopTimestamps.Sort();

        // Create the destination folder: e.g. persistentDataPath/Music/Chops
        string folderPath = Path.Combine(Application.persistentDataPath, "Audio", "Chops");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log("ChopSaver: Created folder " + folderPath);
        }

        // Loop through the timestamps in pairs (chop from time[i] to time[i+1])
        for (int i = 0; i < chopTimestamps.Count - 1; i++)
        {
            float startTime = chopTimestamps[i];
            float endTime = chopTimestamps[i + 1];

            // Construct a filename such as: baseFileName_chop0.wav, baseFileName_chop1.wav, etc.
            string fileName = $"{baseFileName}_chop{i}.wav";
            string filePath = Path.Combine(folderPath, fileName);

            AudioExporter.SaveAudioSegmentToWav(sourceClip, startTime, endTime, filePath);
        }
    }
}
