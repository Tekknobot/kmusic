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

        // Test AddNote (Remove this after confirming functionality)
        var sampleSequencer = sequencer.GetComponent<AudioHelm.SampleSequencer>();
        if (sampleSequencer != null)
        {
            sampleSequencer.AddNote(63, 1, 2, 1.0f);
        }
        else
        {
            Debug.LogError("SampleSequencer component not found on Sequencer.");
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
        }
    }

    public void RotateAndReturn()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }

        Sprite currentSprite = PadManager.Instance.GetCurrentSprite();
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

    public bool IsMouseOver()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);

        return hitCollider != null && hitCollider.gameObject == gameObject;
    }

    public void ResetCell()
    {
        SetSprite(defaultSprite);
    }

    private void SaveTileData(Sprite sprite, float step)
    {
        TileData data = new TileData(sprite, step);

        if (!PadManager.Instance.tileDataGroups.ContainsKey(sprite.name))
        {
            PadManager.Instance.tileDataGroups[sprite.name] = new List<TileData>();
        }

        PadManager.Instance.tileDataGroups[sprite.name].Add(data);

        Debug.Log($"Saved Tile Data: Sprite = {data.Sprite.name}, Step = {data.Step}, Group = {sprite.name}");
    }

    public List<TileData> GetTileDataHistory(Sprite sprite)
    {
        if (PadManager.Instance.tileDataGroups.ContainsKey(sprite.name))
        {
            return PadManager.Instance.tileDataGroups[sprite.name];
        }
        else
        {
            Debug.LogWarning($"No TileData found for sprite: {sprite.name}");
            return new List<TileData>();
        }
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

public class TileData
{
    public Sprite Sprite { get; private set; }
    public float Step { get; private set; }

    public TileData(Sprite sprite, float step)
    {
        Sprite = sprite;
        Step = step;
    }
}
