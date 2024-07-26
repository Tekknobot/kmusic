using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioHelm;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance;   // Singleton instance

    public GameObject keyPrefab;         // Reference to the key prefab
    public Sprite[] sprites;             // Array of sprites to assign to each key

    public Sprite currentSprite;        // Current sprite tracked by KeyManager
    private Sprite lastClickedSprite;    // Last clicked sprite

    public static Sprite DefaultSprite { get; private set; } // Static property to access defaultSprite

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>(); // Dictionary to store original scales of keys

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
        // Check if keyPrefab is assigned
        if (keyPrefab == null)
        {
            Debug.LogError("Key prefab is not assigned in KeyManager.");
            return;
        }

        // Check if sprites array has elements
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("No sprites provided in KeyManager.");
            return;
        }

        // Initialize default sprite
        if (sprites.Length > 0)
        {
            DefaultSprite = sprites[0]; // Set the first sprite in the array as default
        }

        GenerateKeys();
        tileDataGroups = DataManager.LoadTileDataFromFile();       
    }

    private void GenerateKeys()
    {
        int numKeysPerRow = 8; // Number of keys per row
        int numRows = 8; // Number of rows

        // Ensure we have enough sprites to fill the grid
        int numKeysToCreate = Mathf.Min(numRows * numKeysPerRow, sprites.Length); 

        // Initialize the boardCells 2D array
        boardCells = new Cell[8, 8];

        // Loop to create keys
        for (int i = 0; i < numKeysToCreate; i++)
        {
            // Calculate position for the new key
            int row = i / numKeysPerRow;
            int col = i % numKeysPerRow;
            Vector3 keyPosition = new Vector3(col, (-row)-4, 0); // Adjust Y position as needed

            // Instantiate a new key from the prefab at the calculated position
            GameObject newKey = Instantiate(keyPrefab, keyPosition, Quaternion.identity);

            // Optionally, parent the new key under the KeyManager GameObject for organization
            newKey.transform.parent = transform;

            // Assign a unique identifier or logic to each key (for example, MIDI note)
            int midiNote = i; // Example MIDI note generation

            // Get the SpriteRenderer component from the key
            SpriteRenderer spriteRenderer = newKey.GetComponent<SpriteRenderer>();

            // Set the sprite for the key using the sprites array
            if (spriteRenderer != null && i < sprites.Length)
            {
                spriteRenderer.sprite = sprites[i];
            }
            else
            {
                Debug.LogError("SpriteRenderer component not found on key prefab or sprite array index out of bounds.");
                continue; // Continue to the next iteration if sprite setting fails
            }

            // Store the original scale of the key
            originalScales[newKey] = newKey.transform.localScale;

            // Attach click handler to the key
            KeyClickHandler keyClickHandler = newKey.AddComponent<KeyClickHandler>();
            keyClickHandler.Initialize(this, newKey, sprites[i], midiNote);

            // Change name of key
            newKey.name = sprites[i].name;
        }
    }

    // Method to handle when a key is clicked
    public void OnKeyClicked(GameObject clickedKey)
    {
        midiNote = clickedKey.GetComponent<KeyClickHandler>().midiNote;

        // Reset the board to display default configuration first
        //BoardManager.Instance.ResetBoard();

        // Update the current sprite tracked by KeyManager
        SpriteRenderer spriteRenderer = clickedKey.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            //lastClickedSprite = spriteRenderer.sprite; // Update last clicked sprite
            currentSprite = spriteRenderer.sprite;

            // Scale the clicked key temporarily
            StartCoroutine(ScaleKey(clickedKey));

            // Display the sprite on cells with matching step data
            DisplaySpriteOnMatchingSteps(currentSprite);
        }

        // Example of using DataManager static methods correctly
        DataManager.EraseDataFile("tileData.txt");
        DataManager.ClearAllData();

        // Additional debug information
        Debug.Log($"Clicked Key: {clickedKey.name}");
    }

    // Method to handle when a key is pressed down
    public void OnKeyPressDown(GameObject clickedKey)
    {
        KeyClickHandler keyClickHandler = clickedKey.GetComponent<KeyClickHandler>();
        if (keyClickHandler != null)
        {
            int midiNote = keyClickHandler.midiNote;
            BoardManager.Instance.helm.GetComponent<HelmSequencer>().NoteOn(midiNote + 50, 1.0f);

            // Additional debug information
            Debug.Log($"Key Pressed Down: {clickedKey.name}, MIDI Note: {midiNote}");
        }
        else
        {
            Debug.LogError("KeyClickHandler component not found on clicked key.");
        }
    }

    // Method to handle when a key is released
    public void OnKeyRelease(GameObject clickedKey)
    {
        KeyClickHandler keyClickHandler = clickedKey.GetComponent<KeyClickHandler>();
        if (keyClickHandler != null)
        {
            int midiNote = keyClickHandler.midiNote;
            BoardManager.Instance.helm.GetComponent<HelmSequencer>().NoteOff(midiNote + 50);

            // Additional debug information
            Debug.Log($"Key Released: {clickedKey.name}, MIDI Note: {midiNote}");
        }
        else
        {
            Debug.LogError("KeyClickHandler component not found on clicked key.");
        }
    }

    // Coroutine to scale the clicked key
    private IEnumerator ScaleKey(GameObject keyObject)
    {
        Vector3 originalScale = originalScales[keyObject];
        float scaleUpTime = 0.1f;
        float scaleUpSpeed = 1.2f;
        float scaleDownTime = 0.1f;

        float elapsedTime = 0f;

        // Scale up
        while (elapsedTime < scaleUpTime)
        {
            keyObject.transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleUpSpeed, elapsedTime / scaleUpTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        keyObject.transform.localScale = originalScale * scaleUpSpeed;

        yield return new WaitForSeconds(0.2f);

        // Scale down
        elapsedTime = 0f;
        while (elapsedTime < scaleDownTime)
        {
            keyObject.transform.localScale = Vector3.Lerp(originalScale * scaleUpSpeed, originalScale, elapsedTime / scaleDownTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        keyObject.transform.localScale = originalScale;
    }

    // Method to reset the scale of the previously clicked key
    private void ResetKeyScale()
    {
        foreach (GameObject keyObject in originalScales.Keys)
        {
            keyObject.transform.localScale = originalScales[keyObject];
        }
    }

    // Method to get the current sprite tracked by KeyManager
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

                // Check each TileData entry for a match with the cell's step
                foreach (TileData data in tileDataList)
                {
                    if (cell.GetComponent<Cell>().step == data.Step)
                    {
                        Debug.Log($"Found matching step {data.Step} in cell ({x}, {y}). Replacing sprite.");
                        cell.ReplaceSprite(sprite, 4);
                        break; // Replace sprite in only one cell per step match
                    }
                }
            }
        }
    }

    // Method to add tile data to history and respective group
    public void AddTileData(TileData data)
    {
        // Check if there is already a list for this sprite, otherwise create one
        if (!tileDataGroups.ContainsKey(data.SpriteName))
        {
            tileDataGroups[data.SpriteName] = new List<TileData>();
        }

        // Add tile data to respective sprite's group
        tileDataGroups[data.SpriteName].Add(data);

        Debug.Log($"Added TileData for sprite: {data.SpriteName}, Step: {data.Step}");
    }

    public void OnManagerClicked()
    {
        ManagerHandler.Instance.SetLastClickedManager(true);
    }    
}
