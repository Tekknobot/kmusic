using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioHelm;

public class Cell : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Quaternion originalRotation;
    private Coroutine rotationCoroutine;

    public Sprite defaultSprite;
    public GameObject sequencer;
    public float step;

    public Sprite CurrentSprite { get; private set; }
    public int CurrentStep { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalRotation = transform.rotation;

        // Attempt to find the sequencer if not assigned
        if (sequencer == null)
        {
            sequencer = GameObject.Find("Sequencer");
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
            Debug.Log($"SetSprite called: Step = {step}, Sprite = {sprite.name}");
        }
        else
        {
            Debug.LogError("SpriteRenderer is not assigned in Cell.");
        }
    }

    public void ReplaceSprite(Sprite newSprite)
    {
        Debug.Log($"ReplaceSprite called: Old Sprite = {spriteRenderer.sprite?.name ?? "None"}, New Sprite = {newSprite.name}, Step = {step}");

        if (spriteRenderer.sprite != defaultSprite)
        {
            RemoveTileData(spriteRenderer.sprite, step);
            spriteRenderer.sprite = defaultSprite;
            CurrentSprite = defaultSprite;

            var sampleSequencer = sequencer.GetComponent<AudioHelm.SampleSequencer>();
            if (sampleSequencer != null)
            {
                int midiNote = PadManager.Instance.public_clickedPad.GetComponent<PadClickHandler>().midiNote;
                sampleSequencer.RemoveNotesInRange(midiNote, step, step + 1);
                DataManager.EraseTileDataToFile(PadManager.Instance.currentSprite.name, CurrentSprite.name, step);
                Debug.Log($"Removed MIDI {midiNote} at Step = {step}");
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

            SaveTileData(newSprite, step);

            var sampleSequencer = sequencer.GetComponent<AudioHelm.SampleSequencer>();
            if (sampleSequencer != null)
            {
                int midiNote = PadManager.Instance.public_clickedPad.GetComponent<PadClickHandler>().midiNote;
                sampleSequencer.AddNote(midiNote, step, step + 1, 1.0f);
                Debug.Log($"Added MIDI {midiNote} at Step = {step}");
            }
            else
            {
                Debug.LogError("SampleSequencer component not found on Sequencer.");
            }

            DataManager.SaveTileDataToFile(PadManager.Instance.tileDataGroups);
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
        Debug.Log($"RotateAndReturn called: Current Sprite = {currentSprite.name}, Step = {step}");

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

    private void SaveTileData(Sprite sprite, float step)
    {
        TileData data = new TileData(sprite.name, step);

        if (!PadManager.Instance.tileDataGroups.ContainsKey(sprite.name))
        {
            PadManager.Instance.tileDataGroups[sprite.name] = new List<TileData>();
        }

        PadManager.Instance.tileDataGroups[sprite.name].Add(data);

        Debug.Log($"Saved Tile Data: Sprite = {data.SpriteName}, Step = {data.Step}, Group = {sprite.name}");
    }

    private void RemoveTileData(Sprite sprite, float step)
    {
        if (PadManager.Instance.tileDataGroups.ContainsKey(sprite.name))
        {
            List<TileData> tileDataList = PadManager.Instance.tileDataGroups[sprite.name];
            tileDataList.RemoveAll(data => data.Step == step);

            if (tileDataList.Count == 0)
            {
                PadManager.Instance.tileDataGroups.Remove(sprite.name);
            }

            Debug.Log($"Removed Tile Data for Sprite: {sprite.name}, Step: {step}");
        }
    }
}

[System.Serializable]
public class TileData
{
    public string SpriteName; // Store sprite name instead of sprite object
    public float Step;

    public TileData(string spriteName, float step)
    {
        SpriteName = spriteName;
        Step = step;
    }

    // Parameterless constructor for deserialization
    public TileData() {}
}
