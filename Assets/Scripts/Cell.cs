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
    public HelmSequencer activeSequencer;
    public float step;

    public Sprite CurrentSprite { get; private set; }
    public int CurrentStep { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Ensure spriteRenderer is not null
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on this GameObject.");
        }
        originalRotation = transform.rotation;

        if (sequencer == null)
        {
            sequencer = GameObject.Find("Sequencer");
            if (sequencer == null)
            {
                Debug.LogError("Sequencer GameObject not found in scene.");
                return;
            }
        }

        InitializeCell();
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

        Sprite lastClickedSprite = ManagerHandler.Instance.GetLastClickedSprite();
        midiNote = ManagerHandler.Instance.GetLastClickedMidiNote();

        // If the last clicked sprite and the sprite at the step do not match, return early
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

        // Handle removal and replacement logic
        if (spriteRenderer.sprite != defaultSprite)
        {
            var oldSprite = spriteRenderer.sprite;
            RemoveTileData(spriteRenderer.sprite.name, step);
            spriteRenderer.sprite = defaultSprite;
            CurrentSprite = defaultSprite;

            BoardManager.Instance.stepToSpriteMap.Remove(step);

            if (ManagerHandler.Instance.IsKeyManagerLastClicked())
            {
                if (PatternManager.Instance.patterns.Count > 0) {
                    activeSequencer = PatternManager.Instance.GetActiveSequencer(); // Retrieve the current sequencer
                }
                else {
                    activeSequencer = BoardManager.Instance.helm.GetComponent<HelmSequencer>(); 
                }                
                if (activeSequencer != null)
                {
                    activeSequencer.RemoveNotesInRange(midiNote, this.step, this.step + 1);
                    KeyManager.Instance.RemoveKeyTileData(oldSprite, (int)step);
                    DataManager.EraseKeyTileDataToFile(KeyManager.Instance.currentSprite.name, (int)step);
                    Debug.Log($"Removed MIDI {midiNote} at Step = {step}");
                }
                else
                {
                    Debug.LogError("HelmSequencer component not found on Helm.");
                }
            }
            else
            {
                var sampleSequencer = sequencer.GetComponent<AudioHelm.SampleSequencer>();
                if (sampleSequencer != null)
                {
                    sampleSequencer.RemoveNotesInRange(midiNote, step, step + 1);
                    DataManager.EraseTileDataToFile(PadManager.Instance.currentSprite.name, PadManager.Instance.currentSprite.name, (int)step);
                    Debug.Log($"Removed MIDI {midiNote} at Step = {step}");
                }
                else
                {
                    Debug.LogError("SampleSequencer component not found on Sequencer.");
                }
            }

            PatternManager.Instance.SavePatterns();
        }
        else
        {
            spriteRenderer.sprite = newSprite;
            CurrentSprite = newSprite;

            bool isKey = ManagerHandler.Instance.IsKeyManagerLastClicked();
            SaveTileData(newSprite, step, isKey);

            BoardManager.Instance.stepToSpriteMap[step] = newSprite;

            if (ManagerHandler.Instance.IsKeyManagerLastClicked())
            {
                if (PatternManager.Instance.patterns.Count > 0) {
                    activeSequencer = PatternManager.Instance.GetActiveSequencer(); // Retrieve the current sequencer
                }
                else {
                    activeSequencer = BoardManager.Instance.helm.GetComponent<HelmSequencer>(); 
                }      
                if (activeSequencer != null)
                {
                    activeSequencer.AddNote(midiNote, step, step + 1, 1.0f);
                    KeyManager.Instance.SaveKeyTileData(newSprite, (int)step);
                    Debug.Log($"Added MIDI {midiNote} at Step = {step}");
                }
                else
                {
                    Debug.LogError("HelmSequencer component not found on Helm.");
                }
            }
            else
            {
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
            DataManager.SaveKeyTileDataToFile(KeyManager.Instance.tileData);
            PatternManager.Instance.SavePatterns();
        }
    }

    public void RotateAndReturn()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }

        Sprite currentSprite = ManagerHandler.Instance.GetLastClickedSprite();
        int midiNote = ManagerHandler.Instance.GetLastClickedMidiNote();

        if (currentSprite == null)
        {
            Debug.LogError("Current sprite is null. Cannot rotate and replace sprite.");
            return;
        }

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
            KeyManager.Instance.SaveKeyTileData(sprite, (int)step);
        }
        else
        {
            Dictionary<string, List<TileData>> targetDictionary = PadManager.Instance.tileDataGroups;
            Debug.Log("Saving to PadManager's tile data groups.");

            if (!targetDictionary.ContainsKey(sprite.name))
            {
                targetDictionary[sprite.name] = new List<TileData>();
            }

            targetDictionary[sprite.name].Add(data);
        }

        Debug.Log($"Saved Tile Data: Sprite = {data.SpriteName}, Step = {data.Step}, Dictionary = {(isKey ? "KeyManager" : "PadManager")}");
    }

    private void RemoveTileData(string spriteName, float step)
    {
        if (KeyManager.Instance.tileData.ContainsKey(spriteName))
        {
            List<int> steps = KeyManager.Instance.tileData[spriteName];
            steps.Remove((int)step);

            // Remove the sprite from KeyManager's tile data if no steps are left
            if (steps.Count == 0)
            {
                KeyManager.Instance.tileData.Remove(spriteName);
            }

            Debug.Log($"Removed Tile Data for Key Sprite: {spriteName}, Step: {step}");
        }
        else if (PadManager.Instance.tileDataGroups.ContainsKey(spriteName))
        {
            List<TileData> tileDataList = PadManager.Instance.tileDataGroups[spriteName];
            tileDataList.RemoveAll(data => data.Step == step);

            // Remove the sprite from PadManager's tile data if no TileData left
            if (tileDataList.Count == 0)
            {
                PadManager.Instance.tileDataGroups.Remove(spriteName);
            }

            Debug.Log($"Removed Tile Data for Pad Sprite: {spriteName}, Step: {step}");
        }
        else
        {
            Debug.LogWarning($"Sprite {spriteName} not found in tile data.");
        }
    }

    private void InitializeCell()
    {
        // Ensure that all required components and references are properly set
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component is not set.");
        }
    }    

}

[System.Serializable]
public class TileData
{
    public string SpriteName;
    public float Step;

    public TileData(string spriteName, float step)
    {
        SpriteName = spriteName;
        Step = step;
    }

    public TileData() { }
}
