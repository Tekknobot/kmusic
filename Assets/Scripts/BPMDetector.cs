using System.Collections.Generic;
using UnityEngine;

public class BPMDetector : MonoBehaviour
{
    public int sampleRate = 44100; // Default sample rate for most audio clips

    // Fetch the audio clip dynamically from MultipleAudioLoader
    public AudioClip audioClip => MultipleAudioLoader.Instance?.audioSource?.clip;

    public float DetectBPM()
    {
        if (audioClip == null)
        {
            Debug.LogError("No AudioClip is loaded in the AudioSource!");
            return 0f;
        }

        float[] samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);

        List<float> beatIntervals = new List<float>();

        float threshold = 0.1f; // Amplitude threshold for detecting beats
        float lastBeatTime = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            float currentTime = (float)i / sampleRate;

            // Detect amplitude peaks
            if (samples[i] > threshold)
            {
                if (currentTime - lastBeatTime > 0.2f) // Ignore small intervals (avoid noise)
                {
                    if (lastBeatTime > 0f)
                    {
                        float interval = currentTime - lastBeatTime;
                        beatIntervals.Add(interval);
                    }

                    lastBeatTime = currentTime;
                }
            }
        }

        // Calculate average interval
        if (beatIntervals.Count == 0)
        {
            Debug.LogWarning("No beats detected!");
            return 0f;
        }

        float averageInterval = 0f;
        foreach (float interval in beatIntervals)
        {
            averageInterval += interval;
        }
        averageInterval /= beatIntervals.Count;

        // Convert interval to BPM (beats per minute)
        float bpm = 60f / averageInterval;
        Debug.Log($"Detected BPM: {bpm}");
        return bpm;
    }
}
