using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Add this line for using Dictionary

public class PadManager : MonoBehaviour
{
    public static PadManager Instance;   // Singleton instance

    public GameObject padPrefab;         // Reference to the pad prefab
    public Sprite[] sprites;             // Array of sprites to assign to each pad

    private Sprite currentSprite;        // Current sprite tracked by PadManager

    public static Sprite DefaultSprite { get; private set; } // Static property to access defaultSprite

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>(); // Dictionary to store original scales of pads

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Check if padPrefab is assigned
        if (padPrefab == null)
        {
            Debug.LogError("Pad prefab is not assigned in PadManager.");
            return;
        }

        // Check if sprites array has elements
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("No sprites provided in PadManager.");
            return;
        }

        // Initialize default sprite
        if (sprites.Length > 0)
        {
            DefaultSprite = sprites[0]; // Set the first sprite in the array as default
        }

        GeneratePads();
    }

    private void GeneratePads()
    {
        int numPadsToCreate = Mathf.Min(8, sprites.Length); // Ensure we create a maximum of 8 pads

        // Loop to create exactly 8 pads
        for (int i = 0; i < numPadsToCreate; i++)
        {
            // Calculate position for the new pad
            Vector3 padPosition = new Vector3(i, -2, 0); // Adjust Y position as needed

            // Instantiate a new pad from the prefab at the calculated position
            GameObject newPad = Instantiate(padPrefab, padPosition, Quaternion.identity);

            // Optionally, parent the new pad under the PadManager GameObject for organization
            newPad.transform.parent = transform;

            // Assign a unique identifier or logic to each pad (for example, MIDI note)
            int midiNote = i; // Example MIDI note generation

            // Get the SpriteRenderer component from the pad
            SpriteRenderer spriteRenderer = newPad.GetComponent<SpriteRenderer>();

            // Set the sprite for the pad using the sprites array
            if (spriteRenderer != null && i < sprites.Length)
            {
                spriteRenderer.sprite = sprites[i];
            }
            else
            {
                Debug.LogError("SpriteRenderer component not found on pad prefab or sprite array index out of bounds.");
                continue; // Continue to the next iteration if sprite setting fails
            }

            // Store the original scale of the pad
            originalScales[newPad] = newPad.transform.localScale;

            // Attach click handler to the pad
            PadClickHandler padClickHandler = newPad.AddComponent<PadClickHandler>();
            padClickHandler.Initialize(this, newPad, sprites[i], midiNote);
        }
    }

    // Method to handle when a pad is clicked
    public void OnPadClicked(GameObject clickedPad)
    {
        // Reset scale of the previously clicked pad if there was one
        ResetPadScale();

        // Update the current sprite tracked by PadManager
        SpriteRenderer spriteRenderer = clickedPad.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            currentSprite = spriteRenderer.sprite;

            // Notify BoardManager about the clicked pad's sprite and associated MIDI note
            BoardManager.Instance.SaveTileSprite(currentSprite, clickedPad.GetComponent<PadClickHandler>().MidiNote);

            // Start scaling coroutine on the clicked pad
            StartCoroutine(ScalePad(clickedPad));
        }
    }

    // Coroutine to scale the clicked pad
    private IEnumerator ScalePad(GameObject padObject)
    {
        float scaleUpTime = 0.1f;
        float scaleUpSpeed = 1.2f;
        Vector3 originalScale = padObject.transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < scaleUpTime)
        {
            padObject.transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleUpSpeed, elapsedTime / scaleUpTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        padObject.transform.localScale = originalScale * scaleUpSpeed;

        yield return new WaitForSeconds(0.2f);

        float scaleDownTime = 0.1f;
        float elapsedTime2 = 0f;

        while (elapsedTime2 < scaleDownTime)
        {
            padObject.transform.localScale = Vector3.Lerp(originalScale * scaleUpSpeed, originalScale, elapsedTime2 / scaleDownTime);
            elapsedTime2 += Time.deltaTime;
            yield return null;
        }

        padObject.transform.localScale = originalScale;

        // Store the original scale again after scaling
        originalScales[padObject] = originalScale;
    }

    // Method to reset the scale of the previously clicked pad
    private void ResetPadScale()
    {
        foreach (GameObject padObject in originalScales.Keys)
        {
            padObject.transform.localScale = originalScales[padObject];
        }
    }

    // Method to get the current sprite tracked by PadManager
    public Sprite GetCurrentSprite()
    {
        return currentSprite;
    }
}
