using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioHelm;

public class PadManager : MonoBehaviour
{
    public static PadManager Instance;   // Singleton instance

    public GameObject padPrefab;         // Reference to the pad prefab
    public Sprite[] sprites;             // Array of sprites to assign to each pad

    public Sprite currentSprite;         // Current sprite tracked by PadManager
    private Sprite lastClickedSprite;    // Last clicked sprite

    public static Sprite DefaultSprite { get; private set; } // Static property to access defaultSprite

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>(); // Dictionary to store original scales of pads

    private Cell[,] boardCells; // 2D array to store references to all board cells

    public Dictionary<string, List<TileData>> tileDataGroups = new Dictionary<string, List<TileData>>(); // Dictionary to store TileData grouped by sprite

    public int midiNote;

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
        tileDataGroups = DataManager.LoadTileDataFromFile();       
    }

    private void GeneratePads()
    {
        int numPadsPerRow = 8; // Number of pads per row
        int numRows = 8; // Number of rows

        // Ensure we have enough sprites to fill the grid
        int numPadsToCreate = Mathf.Min(numRows * numPadsPerRow, sprites.Length); 

        // Initialize the boardCells 2D array
        boardCells = new Cell[8, 8];

        // Loop to create pads
        for (int i = 0; i < numPadsToCreate; i++)
        {
            // Calculate position for the new pad
            int row = i / numPadsPerRow;
            int col = i % numPadsPerRow;
            Vector3 padPosition = new Vector3(col, (-row)-2, 0); // Adjust Y position as needed

            // Instantiate a new pad from the prefab at the calculated position
            GameObject newPad = Instantiate(padPrefab, padPosition, Quaternion.identity);

            // Optionally, parent the new pad under the PadManager GameObject for organization
            newPad.transform.parent = transform;

            // Assign a unique identifier or logic to each pad (for example, MIDI note)
            int midiNote = i + 60; // Example MIDI note generation

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

            // Change name of pad
            newPad.name = sprites[i].name;
        }
    }

    // Method to handle when a pad is clicked
    public void OnPadClicked(GameObject clickedPad)
    {
        midiNote = clickedPad.GetComponent<PadClickHandler>().midiNote;

        // Set PadManager as the last clicked manager
        ManagerHandler.Instance.SetLastClickedManager(false);

        // Reset the board to display default configuration first
        BoardManager.Instance.ResetBoard();

        // Update the current sprite tracked by PadManager
        SpriteRenderer spriteRenderer = clickedPad.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // lastClickedSprite = spriteRenderer.sprite; // Update last clicked sprite
            currentSprite = spriteRenderer.sprite;

            // Scale the clicked pad temporarily
            StartCoroutine(ScalePad(clickedPad));

            // Display the sprite on cells with matching step data
            DisplaySpriteOnMatchingSteps(currentSprite);

            // Play sample
            BoardManager.Instance.sequencer.GetComponent<SampleSequencer>().NoteOn(midiNote, 1.0f);
        }

        // Additional debug information
        Debug.Log($"Clicked Pad: {clickedPad.name}");
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

    // Method to get the last clicked sprite
    public Sprite GetLastClickedSprite()
    {
        return lastClickedSprite;
    }

    // Method to display all saved tiles for a specific sprite on matching cells
    private void DisplaySpriteOnMatchingSteps(Sprite sprite)
    {
        Debug.Log($"Displaying all saved tiles for sprite: {sprite.name}");

        // Find the group that matches the current sprite
        if (!tileDataGroups.ContainsKey(sprite.name))
        {
            Debug.LogWarning($"No tile data group found for sprite: {sprite.name}");
            return;
        }

        List<TileData> tileDataList = tileDataGroups[sprite.name];
        Debug.Log($"Found {tileDataList.Count} tile data entries for sprite: {sprite.name}");

        // Iterate through boardCells to find cells with matching step
        for (int x = 0; x < BoardManager.Instance.boardCells.GetLength(0); x++)
        {
            for (int y = 0; y < BoardManager.Instance.boardCells.GetLength(1); y++)
            {
                Cell cell = BoardManager.Instance.boardCells[x, y];
                if (cell == null)
                {
                    continue; // Skip null cells
                }

                // Check each TileData entry for matching steps
                foreach (TileData tileData in tileDataList)
                {
                    if (cell.step == tileData.Step)
                    {
                        cell.SetSprite(sprite);
                        GameObject.Find("Sequencer").GetComponent<SampleSequencer>().AddNote(midiNote, cell.step, cell.step + 1, 1.0f);
                    }
                }
            }
        }
    }
}
