using UnityEngine;
using System.Collections;
using AudioHelm;

public class Key : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Quaternion originalRotation;
    private Coroutine rotationCoroutine;

    public Sprite defaultSprite;
    public GameObject sequencer;
    public float step; // Changed from step to note

    public Sprite CurrentSprite { get; private set; }
    public float CurrentNote { get; private set; } // Changed from CurrentStep to CurrentNote

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalRotation = transform.rotation;

        // Attempt to find the sequencer if not assigned
        if (sequencer == null)
        {
            sequencer = GameObject.Find("HelmSequencer");
            if (sequencer == null)
            {
                Debug.LogError("Sequencer GameObject not found in scene.");
                return;
            }
        }
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
            CurrentSprite = sprite;
            Debug.Log($"SetSprite called: Note = {step}, Sprite = {sprite.name}");
        }
        else
        {
            Debug.LogError("SpriteRenderer is not assigned in Key.");
        }
    }

    public void ReplaceSprite(Sprite newSprite)
    {
        Debug.Log($"ReplaceSprite called: Old Sprite = {spriteRenderer.sprite?.name ?? "None"}, New Sprite = {newSprite.name}, Note = {step}");

        if (spriteRenderer.sprite != defaultSprite)
        {
            // No longer using SaveTileData and RemoveTileData
            spriteRenderer.sprite = defaultSprite;
            CurrentSprite = defaultSprite;

            var sampleSequencer = sequencer.GetComponent<AudioHelm.SampleSequencer>();
            if (sampleSequencer != null)
            {
                int midiNote = PadManager.Instance.public_clickedPad.GetComponent<PadClickHandler>().midiNote;
                sampleSequencer.RemoveNotesInRange(midiNote, step, step + 1);
                Debug.Log($"Removed MIDI {midiNote} at Note = {step}");
            }
            else
            {
                Debug.LogError("SampleSequencer component not found on Sequencer.");
            }
        }
        else
        {
            spriteRenderer.sprite = newSprite;
            CurrentSprite = newSprite;

            var sampleSequencer = sequencer.GetComponent<AudioHelm.SampleSequencer>();
            if (sampleSequencer != null)
            {
                int midiNote = PadManager.Instance.public_clickedPad.GetComponent<PadClickHandler>().midiNote;
                sampleSequencer.AddNote(midiNote, step, step + 1, 1.0f);
                Debug.Log($"Added MIDI {midiNote} at Note = {step}");
            }
            else
            {
                Debug.LogError("SampleSequencer component not found on Sequencer.");
            }
        }
    }

    public void RotateAndReturn()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }

        // Retrieve the current sprite from PadManager
        Sprite currentSprite = PadManager.Instance.GetCurrentSprite();
        
        // Check if currentSprite is null
        if (currentSprite == null)
        {
            Debug.LogError("Current sprite is null. Cannot rotate and replace sprite.");
            return; // Exit the method early if currentSprite is null
        }

        // Proceed if currentSprite is not null
        Debug.Log($"RotateAndReturn called: Current Sprite = {currentSprite.name}, Note = {step}");

        ReplaceSprite(currentSprite);

        rotationCoroutine = StartCoroutine(RotateCoroutine());
    }

    private IEnumerator RotateCoroutine()
    {
        Quaternion targetRotation = originalRotation * Quaternion.Euler(0, 0, 180);
        float rotationTime = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < rotationTime)
        {
            transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, elapsedTime / rotationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;

        elapsedTime = 0f;
        while (elapsedTime < rotationTime)
        {
            transform.rotation = Quaternion.Slerp(targetRotation, originalRotation, elapsedTime / rotationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = originalRotation;
    }
}

[System.Serializable]
public class TileDataKeys
{
    public string SpriteName; // Name of the sprite associated with this TileData
    public int Step;          // Step associated with this TileData
}