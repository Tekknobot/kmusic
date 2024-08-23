using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioVisualizer : MonoBehaviour
{
    public AudioClip audioClip; // The audio clip to visualize
    public RectTransform waveformContainer; // UI element to hold the waveform
    public GameObject markerPrefab; // Prefab for the timestamp markers

    private float[] audioSamples;
    private int sampleRate;
    public Material waveformMaterial; // Reference to the material\

    public Chop chopScript;

    public void StartRender()
    {
        foreach (Transform child in waveformContainer)
        {
            Destroy(child.gameObject);
        }

        audioClip = MultipleAudioLoader.Instance.audioSource.clip;

        if (audioClip == null)
        {
            Debug.LogError("No AudioClip assigned!");
            return;
        }

        sampleRate = audioClip.samples;
        audioSamples = new float[sampleRate * audioClip.channels];
        audioClip.GetData(audioSamples, 0);

        DrawWaveform(chopScript.timestamps[0], chopScript.timestamps[chopScript.timestamps.Count - 1]);
        AddTimestampMarkers(chopScript.timestamps);
    }

    private void DrawWaveform(float startTimestamp, float endTimestamp)
    {
        float containerWidth = waveformContainer.GetComponent<RectTransform>().rect.width;
        float containerHeight = waveformContainer.GetComponent<RectTransform>().rect.height;

        // Ensure timestamps are within the audio clip length
        startTimestamp = Mathf.Clamp(startTimestamp, 0, audioClip.length);
        endTimestamp = Mathf.Clamp(endTimestamp, 0, audioClip.length);

        // Convert timestamps to sample indices
        int startSample = Mathf.FloorToInt(startTimestamp * audioClip.frequency);
        int endSample = Mathf.FloorToInt(endTimestamp * audioClip.frequency);
        int totalSamples = endSample - startSample;

        // Reduce the number of segments by downsampling
        int resolution = 1000; // Number of segments (you can adjust this value)
        int samplesPerSegment = Mathf.Max(1, totalSamples / resolution);

        // Determine the maximum amplitude for normalization within the segment
        float maxAmplitude = 0f;
        for (int i = startSample; i < endSample; i++)
        {
            maxAmplitude = Mathf.Max(maxAmplitude, Mathf.Abs(audioSamples[i]));
        }

        for (int i = 0; i < resolution; i++)
        {
            // Calculate the average amplitude for this segment
            float sum = 0f;
            int segmentStartSample = startSample + i * samplesPerSegment;
            int segmentEndSample = Mathf.Min(segmentStartSample + samplesPerSegment, endSample);

            for (int j = segmentStartSample; j < segmentEndSample; j++)
            {
                sum += Mathf.Abs(audioSamples[j]);
            }
            float averageAmplitude = sum / (segmentEndSample - segmentStartSample);

            // Normalize the amplitude relative to the maximum amplitude
            float normalizedAmplitude = averageAmplitude / maxAmplitude;

            // Create a segment
            GameObject segment = new GameObject("WaveformSegment");
            segment.transform.SetParent(waveformContainer, false);

            // Add Image component
            var image = segment.AddComponent<Image>();
            image.material = waveformMaterial;

            // Calculate the height based on the normalized amplitude
            float height = normalizedAmplitude * containerHeight;

            // Set RectTransform for UI
            RectTransform rectTransform = segment.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(containerWidth / resolution, height);
            rectTransform.anchoredPosition = new Vector2(i * (containerWidth / resolution), 0);
        }
    }

    public void AddTimestampMarkers(List<float> timestamps)
    {
        foreach (float timestamp in timestamps)
        {
            float normalizedTime = timestamp / audioClip.length;
            float markerPositionX = normalizedTime * waveformContainer.rect.width;

            // Instantiate the marker as a child of waveformContainer
            GameObject marker = Instantiate(markerPrefab, waveformContainer);

            // Ensure the marker has a RectTransform component
            RectTransform rt = marker.GetComponent<RectTransform>();
            if (rt == null)
            {
                Debug.LogError("Marker prefab does not have a RectTransform component.");
                return;
            }

            // Set the position of the marker
            rt.anchoredPosition = new Vector2(markerPositionX, 0);

            // Scale the marker to fit the height of the container
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, waveformContainer.rect.height);

            // Ensure the marker stretches across the height of the container
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 1);

            // Adjust marker visibility
            Image image = marker.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.white; // Set the marker color to white or any other visible color
            }

            // Set the marker to be rendered in front of other elements
            marker.transform.SetSiblingIndex(waveformContainer.childCount - 1);
        }
    }

}
