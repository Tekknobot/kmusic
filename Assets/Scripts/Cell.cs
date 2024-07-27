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

    public void ReplaceSprite(Sprite newSprite, int midiNote)
    {
        Debug.Log($"ReplaceSprite called: Old Sprite = {spriteRenderer.sprite?.name ?? "None"}, New Sprite = {newSprite.name}, Step = {step}");

        // Get the last clicked sprite and midiNote from ManagerHandler
        Sprite lastClickedSprite = ManagerHandler.Instance.GetLastClickedSprite();
        midiNote = ManagerHandler.Instance.GetLastClickedMidiNote();

        if (lastClickedSprite != BoardManager.Instance.GetSpriteByStep(step) && BoardManager.Instance.GetSpriteByStep(step) != defaultSprite)
        {
            Debug.Log("Returning early due to sprite mismatch.");
            return;
        }

        if (lastClickedSprite != null)
        {
            newSprite = lastClickedSprite;
            Debug.Log($"ReplaceSprite using last clicked sprite: {newSprite.name}");
        }

        // Check if the current sprite is not the default sprite
        if (spriteRenderer.sprite != defaultSprite)
        {
            // Remove tile data related to the current sprite
            RemoveTileData(spriteRenderer.sprite, step);
            spriteRenderer.sprite = defaultSprite;
            CurrentSprite = defaultSprite;

            // Remove the old sprite from the stepToSpriteMap
            BoardManager.Instance.stepToSpriteMap.Remove(step);

            // Determine which sequencer to use
            if (ManagerHandler.Instance.IsKeyManagerLastClicked())
            {
                // Use HelmSequencer if the last clicked manager is KeyManager
                var helmSequencer = BoardManager.Instance.helm.GetComponent<HelmSequencer>();
                if (helmSequencer != null)
                {
                    helmSequencer.RemoveNotesInRange(midiNote, step, step + 1);
                    DataManager.EraseTileDataToFile(KeyManager.Instance.currentSprite.name, KeyManager.Instance.currentSprite.name, step);
                    Debug.Log($"Removed MIDI {midiNote} at Step = {step}");
                }
                else
                {
                    Debug.LogError("HelmSequencer component not found on Helm.");
                }
            }
            else
            {
                // Use SampleSequencer if the last clicked manager is PadManager
                var sampleSequencer = sequencer.GetComponent<AudioHelm.SampleSequencer>();
                if (sampleSequencer != null)
                {
                    sampleSequencer.RemoveNotesInRange(midiNote, step, step + 1);
                    DataManager.EraseTileDataToFile(PadManager.Instance.currentSprite.name, PadManager.Instance.currentSprite.name, step);
                    Debug.Log($"Removed MIDI {midiNote} at Step = {step}");
                }
                else
                {
                    Debug.LogError("SampleSequencer component not found on Sequencer.");
                }
            }
        }
        else
        {
            // Set the new sprite
            spriteRenderer.sprite = newSprite;
            CurrentSprite = newSprite;

            bool isKey = ManagerHandler.Instance.IsKeyManagerLastClicked();

            // Save tile data for the new sprite
            SaveTileData(newSprite, step, isKey);

            // Update the stepToSpriteMap dictionary
            BoardManager.Instance.stepToSpriteMap[step] = newSprite;

            // Determine which sequencer to use
            if (ManagerHandler.Instance.IsKeyManagerLastClicked())
            {
                // Use HelmSequencer if the last clicked manager is KeyManager
                var helmSequencer = BoardManager.Instance.helm.GetComponent<HelmSequencer>();
                if (helmSequencer != null)
                {
                    helmSequencer.AddNote(midiNote, step, step + 1, 1.0f);
                    Debug.Log($"Added MIDI {midiNote} at Step = {step}");
                }
                else
                {
                    Debug.LogError("HelmSequencer component not found on Helm.");
                }
            }
            else
            {
                // Use SampleSequencer if the last clicked manager is PadManager
                var sampleSequencer = sequencer.GetComponent<AudioHelm.SampleSequencer>();
                if (sampleSequencer != null)
                {
                    sampleSequencer.AddNote(midiNote, step, step + 1, 1.0f);
                    Debug.Log($"Added MIDI {midiNote} at Step = {step}");
                }
                else
                {
                    Debug.LogError("SampleSequencer component not found on Sequencer.");
                }
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

        // Get the last clicked sprite and midiNote from ManagerHandler
        Sprite currentSprite = ManagerHandler.Instance.GetLastClickedSprite();
        int midiNote = ManagerHandler.Instance.GetLastClickedMidiNote();

        // Check if currentSprite is null
        if (currentSprite == null)
        {
            Debug.LogError("Current sprite is null. Cannot rotate and replace sprite.");
            return; // Exit the method early if currentSprite is null
        }

        // Proceed if currentSprite is not null
        Debug.Log($"RotateAndReturn called: Current Sprite = {currentSprite.name}, Step = {step}");

        ReplaceSprite(currentSprite, midiNote);

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

    public void SaveTileData(Sprite sprite, float step, bool isKey)
    {
        TileData data = new TileData(sprite.name, step);

        if (isKey)
        {
            // Use KeyManager's tile data
            KeyManager.Instance.SaveKeyTileData(sprite, (int)step);
        }
        else
        {
            // Use PadManager's tile data groups
            Dictionary<string, List<TileData>> targetDictionary = PadManager.Instance.tileDataGroups;
            Debug.Log("Saving to PadManager's tile data groups.");

            // Ensure the dictionary contains a list for this sprite
            if (!targetDictionary.ContainsKey(sprite.name))
            {
                targetDictionary[sprite.name] = new List<TileData>();
            }

            // Add the new tile data to the list
            targetDictionary[sprite.name].Add(data);
        }

        Debug.Log($"Saved Tile Data: Sprite = {data.SpriteName}, Step = {data.Step}, Dictionary = {(isKey ? "KeyManager" : "PadManager")}");
    }

    private void RemoveTileData(Sprite sprite, float step)
    {
        // Handle removal in KeyManager
        if (KeyManager.Instance.tileData.ContainsKey(sprite))
        {
            List<int> steps = KeyManager.Instance.tileData[sprite];
            steps.Remove((int)step);

            if (steps.Count == 0)
            {
                KeyManager.Instance.tileData.Remove(sprite);
            }

            Debug.Log($"Removed Tile Data for Key Sprite: {sprite.name}, Step: {step}");
        }

        // Handle removal in PadManager
        else if (PadManager.Instance.tileDataGroups.ContainsKey(sprite.name))
        {
            List<TileData> tileDataList = PadManager.Instance.tileDataGroups[sprite.name];
            tileDataList.RemoveAll(data => data.Step == step);

            if (tileDataList.Count == 0)
            {
                PadManager.Instance.tileDataGroups.Remove(sprite.name);
            }

            Debug.Log($"Removed Tile Data for Pad Sprite: {sprite.name}, Step: {step}");
        }
    }


    // Method to call SaveKeyTileData
    private void SaveKeyTileData(Sprite sprite, int step)
    {
        KeyManager.Instance.SaveKeyTileData(sprite, step);
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
