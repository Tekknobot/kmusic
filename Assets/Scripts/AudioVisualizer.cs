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
    public Material waveformMaterial; // Reference to the material

    public Chop chopScript;

    private RectTransform markerContainer; // New RectTransform for markers

    public void StartRender()
    {
        // Clear existing waveform and marker elements
        foreach (Transform child in waveformContainer)
        {
            Destroy(child.gameObject);
        }

        if (markerContainer != null)
        {
            Destroy(markerContainer.gameObject);
        }

        // Retrieve the audio clip
        audioClip = MultipleAudioLoader.Instance.audioSource.clip;

        if (audioClip == null)
        {
            Debug.LogError("No AudioClip assigned!");
            return;
        }

        // Initialize audio samples
        sampleRate = audioClip.samples;
        audioSamples = new float[sampleRate * audioClip.channels];
        audioClip.GetData(audioSamples, 0);

        // Draw waveform and create marker container
        DrawWaveform(chopScript.timestamps[0], chopScript.timestamps[chopScript.timestamps.Count - 1]);
        CreateMarkerContainer(); // Create marker container

        // Ensure markerContainer is created and valid
        if (markerContainer == null)
        {
            Debug.LogError("MarkerContainer is not created.");
            return;
        }

        // Add timestamp markers
        AddTimestampMarkers(chopScript.timestamps);
    }


    private void CreateMarkerContainer()
    {
        // Create a new GameObject for the marker container
        GameObject markerContainerObject = new GameObject("MarkerContainer");
        markerContainerObject.transform.SetParent(waveformContainer, false);

        // Add RectTransform component and set its properties
        markerContainer = markerContainerObject.AddComponent<RectTransform>();
        markerContainer.anchorMin = new Vector2(0, 0);
        markerContainer.anchorMax = new Vector2(0, 1); // Adjust anchorMax to match the width and align with the left edge
        markerContainer.pivot = new Vector2(0, 0.5f); // Align pivot to the left edge center

        // Calculate the width of the markerContainer based on the left and right edge positions
        float leftEdge = -351.9445f;
        float rightEdge = 1.104523f;
        float width = rightEdge - leftEdge; // Width = right - left

        // Set the sizeDelta to the calculated width and the full height of the waveformContainer
        float containerHeight = waveformContainer.rect.height;
        markerContainer.sizeDelta = new Vector2(width, containerHeight);

        // Set the anchoredPosition to align the left edge at -351.9445 units
        markerContainer.anchoredPosition = new Vector2(leftEdge + (width / 2), 0); // Center the container horizontally
    }



    private void DrawWaveform(float startTimestamp, float endTimestamp)
    {
        float containerWidth = waveformContainer.rect.width;
        float containerHeight = waveformContainer.rect.height;

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
        if (markerContainer == null)
        {
            Debug.LogError("Marker container is not initialized.");
            return;
        }

        // Calculate the width and height of the segment to use for marker placement
        float segmentWidth = waveformContainer.rect.width;
        float segmentHeight = waveformContainer.rect.height;

        foreach (float timestamp in timestamps)
        {
            // Get the start and end timestamps for the segment being visualized
            float startTimestamp = chopScript.timestamps[0];
            float endTimestamp = chopScript.timestamps[chopScript.timestamps.Count - 1];

            // Only add markers for timestamps within the current segment
            if (timestamp < startTimestamp || timestamp > endTimestamp)
                continue;

            // Normalize the timestamp relative to the segment
            float normalizedTime = (timestamp - startTimestamp) / (endTimestamp - startTimestamp);
            float markerPositionX = normalizedTime * segmentWidth;

            // Instantiate the marker as a child of markerContainer
            GameObject marker = Instantiate(markerPrefab, markerContainer);

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
            rt.sizeDelta = new Vector2(2, segmentHeight); // Adjust width (2) as needed

            // Ensure the marker stretches across the height of the container
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 1);

            // Adjust the color of the marker (optional)
            RawImage rawImage = marker.GetComponent<RawImage>();
            if (rawImage != null)
            {
                rawImage.color = Color.red; // Set to a visible color
            }

            // Set the marker to be rendered in front of other elements
            marker.transform.SetSiblingIndex(markerContainer.childCount - 1);
        }
    }
}
