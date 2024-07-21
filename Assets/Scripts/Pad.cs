using UnityEngine;
using System.Collections;

public class Pad : MonoBehaviour
{
    public Sprite padSprite; // Reference to the pad sprite
    public string midiNote; // MIDI note associated with the pad

    public static Sprite DefaultSprite { get; private set; } // Static property to access defaultSprite

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private Sprite currentSprite; // Store the current sprite of the pad

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void OnMouseDown()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }

        scaleCoroutine = StartCoroutine(ScaleUpAndDown());

        BoardManager.Instance.DisplayTileSprites();
    }

    private IEnumerator ScaleUpAndDown()
    {
        float scaleUpTime = 0.1f;
        float scaleUpSpeed = 1.2f;
        float elapsedTime = 0f;

        while (elapsedTime < scaleUpTime)
        {
            transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleUpSpeed, elapsedTime / scaleUpTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale * scaleUpSpeed;

        yield return new WaitForSeconds(0.2f);

        float scaleDownTime = 0.1f;
        float elapsedTime2 = 0f;

        while (elapsedTime2 < scaleDownTime)
        {
            transform.localScale = Vector3.Lerp(originalScale * scaleUpSpeed, originalScale, elapsedTime2 / scaleDownTime);
            elapsedTime2 += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;

        scaleCoroutine = null;
    }

    public void SetSprite(Sprite newSprite)
    {
        GetComponent<SpriteRenderer>().sprite = newSprite; // Set the sprite of the pad
        currentSprite = newSprite; // Update the current sprite
    }

    public Sprite GetCurrentSprite()
    {
        return currentSprite;
    }

    void Awake()
    {
        DefaultSprite = padSprite; // Initialize the defaultSprite static property
    }
}
