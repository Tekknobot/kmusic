using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cell : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Quaternion originalRotation;
    private Coroutine rotationCoroutine;

    public Sprite defaultSprite; // Make sure this is public if you need to access it externally

    public int step;

    // Properties to expose sprite and step information
    public Sprite CurrentSprite { get; private set; }
    public int CurrentStep { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalRotation = transform.rotation;
    }

    // Method to set the sprite for the Cell
    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
            CurrentSprite = sprite; // Update the current sprite
        }
        else
        {
            Debug.LogError("SpriteRenderer is not assigned in Cell.");
        }
    }

    // Method to replace the sprite with a new sprite and save sprite/step information
    public void ReplaceSprite(Sprite newSprite)
    {
        spriteRenderer.sprite = newSprite;
        CurrentSprite = newSprite; // Update the current sprite

        // Save sprite and step information into respective group
        SaveTileData(CurrentSprite, step);

        // Add note to sequencer with pitch, at step, and duration of 1 step
        if (BoardManager.Instance.sequencer != null)
        {
            BoardManager.Instance.sequencer.AddNote(48, step, step + 1, 1.0f); // Ensure duration is passed correctly
        }
        else
        {
            Debug.LogError("Sequencer is not assigned in BoardManager.");
        }
    }

    // Method to rotate the cell and return to original rotation
    public void RotateAndReturn()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }

        // Immediately replace the sprite before starting rotation
        Sprite currentSprite = PadManager.Instance.GetCurrentSprite();
        ReplaceSprite(currentSprite);

        rotationCoroutine = StartCoroutine(RotateCoroutine());
    }

    private IEnumerator RotateCoroutine()
    {
        Quaternion targetRotation = originalRotation * Quaternion.Euler(0, 0, 180);
        float rotationTime = 0.5f; // Adjust rotation time as needed
        float elapsedTime = 0f;

        while (elapsedTime < rotationTime)
        {
            transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, elapsedTime / rotationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;

        // Swap sprites after rotation
        Sprite tempSprite = CurrentSprite;
        ReplaceSprite(PadManager.Instance.GetCurrentSprite());

        // Rotate back to original rotation
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

    // Method to reset the cell to its initial state
    public void ResetCell()
    {
        SetSprite(defaultSprite);
        // Optionally reset other properties of the cell
    }

    // Method to save sprite and step information in TileData history, grouped by sprite
    private void SaveTileData(Sprite sprite, int step)
    {
        TileData data = new TileData(sprite, step);

        // Check if there is already a list for this sprite
        if (!PadManager.Instance.tileDataGroups.ContainsKey(sprite.name))
        {
            PadManager.Instance.tileDataGroups[sprite.name] = new List<TileData>();
        }

        // Add the TileData to the respective sprite's group
        PadManager.Instance.tileDataGroups[sprite.name].Add(data);

        Debug.Log($"Saved Tile Data: Sprite = {data.Sprite.name}, Step = {data.Step}, Group = {sprite.name}");
    }

    // Method to get all saved TileData instances for a specific sprite
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
}

// Data structure to hold sprite and step information
public class TileData
{
    public Sprite Sprite { get; private set; }
    public int Step { get; private set; }

    public TileData(Sprite sprite, int step)
    {
        Sprite = sprite;
        Step = step;
    }
}
