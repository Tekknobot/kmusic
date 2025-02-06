using UnityEngine;
using System;
using System.IO;

public static class AudioExporter
{
    /// <summary>
    /// Extracts a segment from an AudioClip between startTime and endTime and saves it as a WAV file.
    /// </summary>
    /// <param name="clip">Source AudioClip.</param>
    /// <param name="startTime">Start time (in seconds) of the segment.</param>
    /// <param name="endTime">End time (in seconds) of the segment.</param>
    /// <param name="filePath">Full file path where the WAV file will be saved.</param>
    public static void SaveAudioSegmentToWav(AudioClip clip, float startTime, float endTime, string filePath)
    {
        byte[] wavBytes = ExportAudioSegmentToWavBytes(clip, startTime, endTime);
        if (wavBytes == null)
        {
            Debug.LogError("AudioExporter: Failed to export audio segment.");
            return;
        }
        try
        {
            File.WriteAllBytes(filePath, wavBytes);
            Debug.Log($"WAV file saved: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"AudioExporter: Error saving WAV file: {ex.Message}");
        }
    }

    /// <summary>
    /// Extracts a segment from an AudioClip between startTime and endTime and returns a byte array representing a WAV file (16-bit PCM).
    /// </summary>
    /// <param name="clip">Source AudioClip.</param>
    /// <param name="startTime">Start time (in seconds) of the segment.</param>
    /// <param name="endTime">End time (in seconds) of the segment.</param>
    /// <returns>A byte array containing the WAV file data, or null if an error occurs.</returns>
    public static byte[] ExportAudioSegmentToWavBytes(AudioClip clip, float startTime, float endTime)
    {
        if (clip == null)
        {
            Debug.LogError("AudioExporter: AudioClip is null.");
            return null;
        }
        if (startTime < 0 || endTime > clip.length || startTime >= endTime)
        {
            Debug.LogError("AudioExporter: Invalid start or end time.");
            return null;
        }

        int sampleRate = clip.frequency;
        int channels = clip.channels;
        int startSample = Mathf.FloorToInt(startTime * sampleRate);
        int endSample = Mathf.FloorToInt(endTime * sampleRate);
        int sampleCount = endSample - startSample;

        // Allocate a float array to store samples from all channels.
        float[] samples = new float[sampleCount * channels];
        clip.GetData(samples, startSample);

        byte[] wavBytes = ConvertAudioClipDataToWav(samples, sampleRate, channels);
        return wavBytes;
    }

    /// <summary>
    /// Converts float samples to a WAV file byte array (16-bit PCM).
    /// </summary>
    /// <param name="samples">Array of float samples.</param>
    /// <param name="sampleRate">The sample rate of the audio.</param>
    /// <param name="channels">The number of audio channels.</param>
    /// <returns>A byte array containing the WAV file data.</returns>
    private static byte[] ConvertAudioClipDataToWav(float[] samples, int sampleRate, int channels)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            int byteRate = sampleRate * channels * 2; // 16-bit = 2 bytes per sample
            int subChunk2Size = samples.Length * 2;   // total data size in bytes
            int chunkSize = 36 + subChunk2Size;

            // Write RIFF header.
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(chunkSize);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

            // Write fmt subchunk.
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16); // SubChunk1Size for PCM.
            writer.Write((short)1); // AudioFormat = PCM.
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)(channels * 2)); // BlockAlign.
            writer.Write((short)16); // BitsPerSample.

            // Write data subchunk.
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(subChunk2Size);

            // Write sample data (convert float samples to 16-bit integers).
            foreach (float sample in samples)
            {
                short intSample = (short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue);
                writer.Write(intSample);
            }
            writer.Flush();
            return stream.ToArray();
        }
    }
}
