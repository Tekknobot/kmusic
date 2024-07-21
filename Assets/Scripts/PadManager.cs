using UnityEngine;
using System.Collections.Generic;

public class PadManager : MonoBehaviour
{
    public static PadManager Instance;   // Singleton instance

    public GameObject padPrefab;         // Reference to the pad prefab
    public Sprite[] sprites;             // Array of sprites to assign to each pad

    private Sprite currentSprite;        // Current sprite tracked by PadManager

    public static Sprite DefaultSprite { get; private set; } // Static property to access defaultSprite

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

            // Get the Pad component from the instantiated pad
            Pad pad = newPad.GetComponent<Pad>();

            // Check if Pad component is found
            if (pad == null)
            {
                Debug.LogError("Pad component not found on the prefab.");
                continue; // Continue to the next iteration if pad component is missing
            }

            // Set the sprite for the pad using the sprites array
            pad.SetSprite(sprites[i]);

            // Optionally, you may want to assign some identifier or logic to each pad
            // to differentiate them as needed.
        }
    }

    // Method to notify PadManager about the clicked pad
    public void NotifyPadClicked(Pad clickedPad)
    {
        // Update the current sprite tracked by PadManager
        currentSprite = clickedPad.GetCurrentSprite();

        // Notify BoardManager about the clicked pad's sprite and associated MIDI note
        BoardManager.Instance.SaveTileSprite(currentSprite, clickedPad.midiNote);
        BoardManager.Instance.DisplayTileSprites();
    }

    // Method to get the current sprite tracked by PadManager
    public Sprite GetCurrentSprite()
    {
        return currentSprite;
    }
}
