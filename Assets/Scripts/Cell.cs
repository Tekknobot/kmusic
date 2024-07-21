using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Quaternion originalRotation;
    private Coroutine rotationCoroutine;

    public Sprite defaultSprite; // Make sure this is public if you need to access it externally

    public int step;
    public bool hasNote; // Private field to store hasNote information

    // Properties to expose sprite and step information
    public Sprite CurrentSprite { get; private set; }
    public int CurrentStep { get; private set; }
    public bool HasNote => hasNote; // Public property to read hasNote

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

    // Method to replace the sprite with a new sprite and update hasNote
    public void ReplaceSprite(Sprite newSprite)
    {
        spriteRenderer.sprite = newSprite;
        CurrentSprite = newSprite; // Update the current sprite
        hasNote = true; // Set hasNote to true when sprite is replaced
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
        PadManager.Instance.SaveTileSprite(tempSprite, step, CurrentStep); // Pass CurrentStep as the third argument

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
        hasNote = false; // Reset hasNote to false
        // Optionally reset other properties of the cell
    }
}
