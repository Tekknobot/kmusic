using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PadManager : MonoBehaviour
{
    public static PadManager Instance;   // Singleton instance

    public GameObject padPrefab;         // Reference to the pad prefab
    public Sprite[] sprites;             // Array of sprites to assign to each pad

    private Sprite currentSprite;        // Current sprite tracked by PadManager

    public static Sprite DefaultSprite { get; private set; } // Static property to access defaultSprite

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>(); // Dictionary to store original scales of pads

    private Cell[,] boardCells; // 2D array to store references to all board cells

    public Dictionary<string, List<TileData>> tileDataGroups = new Dictionary<string, List<TileData>>(); // Dictionary to store TileData grouped by sprite

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

        // Initialize the boardCells 2D array
        boardCells = new Cell[8, 8];

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
        // Reset the board to display default configuration first
        BoardManager.Instance.ResetBoard();

        // Update the current sprite tracked by PadManager
        SpriteRenderer spriteRenderer = clickedPad.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            currentSprite = spriteRenderer.sprite;

            // Scale the clicked pad temporarily
            StartCoroutine(ScalePad(clickedPad));

            // Display the sprite on cells with matching step data
            DisplaySpriteOnMatchingSteps(currentSprite);
        }
    }

    // Coroutine to scale the clicked pad
    private IEnumerator ScalePad(GameObject padObject)
    {
        Vector3 originalScale = originalScales[padObject];
        float scaleUpTime = 0.1f;
        float scaleUpSpeed = 1.2f;
        float scaleDownTime = 0.1f;

        float elapsedTime = 0f;

        // Scale up
        while (elapsedTime < scaleUpTime)
        {
            padObject.transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleUpSpeed, elapsedTime / scaleUpTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        padObject.transform.localScale = originalScale * scaleUpSpeed;

        yield return new WaitForSeconds(0.2f);

        // Scale down
        elapsedTime = 0f;
        while (elapsedTime < scaleDownTime)
        {
            padObject.transform.localScale = Vector3.Lerp(originalScale * scaleUpSpeed, originalScale, elapsedTime / scaleDownTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        padObject.transform.localScale = originalScale;
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

    // Method to display the sprite on cells with matching step data
    private void DisplaySpriteOnMatchingSteps(Sprite sprite)
    {
        Debug.Log($"Displaying sprite on cells with matching steps for sprite: {sprite.name}");

        // Find the group that matches the current sprite
        if (tileDataGroups.ContainsKey(sprite.name))
        {
            List<TileData> tileDataList = tileDataGroups[sprite.name];

            Debug.Log($"Found {tileDataList.Count} tile data entries for sprite: {sprite.name}");

            // Iterate through tile data for the current sprite group
            foreach (TileData data in tileDataList)
            {
                int step = data.Step;

                Debug.Log($"Checking step {step}");

                // Iterate through boardCells to find cells with matching step
                for (int x = 0; x < BoardManager.Instance.boardCells.GetLength(0); x++)
                {
                    for (int y = 0; y < BoardManager.Instance.boardCells.GetLength(1); y++)
                    {
                        Cell cell = BoardManager.Instance.boardCells[x, y];
                        if (cell != null && cell.GetComponent<Cell>().step == step)
                        {
                            Debug.Log($"Found matching step {step} in cell ({x}, {y}). Replacing sprite.");

                            // Replace sprite in the cell with the matching step
                            cell.ReplaceSprite(sprite);
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning($"No tile data group found for sprite: {sprite.name}");
        }
    }


    // Method to add tile data to history and respective group
    public void AddTileData(TileData data)
    {
        // Check if there is already a list for this sprite, otherwise create one
        if (!tileDataGroups.ContainsKey(data.Sprite.name))
        {
            tileDataGroups[data.Sprite.name] = new List<TileData>();
        }

        // Add tile data to respective sprite's group
        tileDataGroups[data.Sprite.name].Add(data);

        Debug.Log($"Added TileData for sprite: {data.Sprite.name}, Step: {data.Step}");
    }
}
