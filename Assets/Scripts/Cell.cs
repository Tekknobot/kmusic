using UnityEngine;
using System.Collections;

public class Cell : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Quaternion originalRotation;
    private Coroutine rotationCoroutine;

    public Sprite defaultSprite; // Make sure this is public if you need to access it externally

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
        }
        else
        {
            Debug.LogError("SpriteRenderer is not assigned in Cell.");
        }
    }

    // Method to replace the sprite with a new sprite
    public void ReplaceSprite(Sprite newSprite)
    {
        spriteRenderer.sprite = newSprite;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Check if the mouse is over this cell
            if (IsMouseOver())
            {
                if (rotationCoroutine != null)
                {
                    StopCoroutine(rotationCoroutine);
                }

                // Immediately replace the sprite before starting rotation
                Sprite currentSprite = PadManager.Instance.GetCurrentSprite();
                ReplaceSprite(currentSprite);

                rotationCoroutine = StartCoroutine(RotateAndReturn());
            }
        }
    }

    private IEnumerator RotateAndReturn()
    {
        // Rotate 180 degrees
        Quaternion targetRotation = originalRotation * Quaternion.Euler(0, 0, 180);
        float rotationTime = 0.5f; // Adjust rotation time as needed
        float elapsedTime = 0f;

        // Rotate to 180 degrees
        while (elapsedTime < rotationTime)
        {
            transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, elapsedTime / rotationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;

        // Rotate back to original rotation
        elapsedTime = 0f;
        Quaternion startRotation = targetRotation;
        while (elapsedTime < rotationTime)
        {
            transform.rotation = Quaternion.Slerp(startRotation, originalRotation, elapsedTime / rotationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = originalRotation;
    }

    private bool IsMouseOver()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);

        if (hitCollider != null && hitCollider.gameObject == gameObject)
        {
            return true;
        }

        return false;
    }
}
