using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AudioHelm;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance;   // Singleton instance

    public GameObject keyPrefab;         // Reference to the key prefab
    public Sprite[] sprites;             // Array of sprites to assign to each key

    public Sprite currentSprite;         // Current sprite tracked by KeyManager
    private Sprite lastClickedSprite;    // Last clicked sprite

    public static Sprite DefaultSprite { get; private set; } // Static property to access defaultSprite

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>(); // Dictionary to store original scales of keys

    private Cell[,] boardCells; // 2D array to store references to all board cells

    public Dictionary<string, List<int>> tileData = new Dictionary<string, List<int>>(); // Changed to use string keys

    public int midiNote;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

        // Log the names of sprites in the array
        Debug.Log("Sprites available:");
        foreach (var sprite in sprites)
        {
            Debug.Log(sprite.name);
        }

        // Initialize default sprite
        if (sprites.Length > 0)
        {
            DefaultSprite = sprites[0]; // Set the first sprite in the array as default
        }

        GenerateKeys();

        // Start the coroutine to load tile data after a delay
        StartCoroutine(LoadTileDataAfterDelay(1.0f)); // Adjust the delay time as needed
    }

    private IEnumerator LoadTileDataAfterDelay(float delaySeconds)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delaySeconds);

        // Load the tile data from file
        tileData = DataManager.LoadKeyTileDataFromFile();

        // Log the loaded tile data
        foreach (var entry in tileData)
        {
            Debug.Log($"Loaded tile data: Sprite = {entry.Key}, Steps = {string.Join(", ", entry.Value)}");
        }
    }

    // Method to display sprites on cells with matching step data
    public void DisplaySpriteOnMatchingSteps()
    {
        Debug.Log("Displaying all saved tiles for KeyManager.");

        // Ensure we have the boardCells populated
        if (boardCells == null)
        {
            Debug.LogError("Board cells are not initialized.");
            return;
        }

        // Get the HelmSequencer component
        HelmSequencer helmSequencer = BoardManager.Instance.helm.GetComponent<HelmSequencer>();
        if (helmSequencer == null)
        {
            Debug.LogError("HelmSequencer component not found on Helm.");
            return;
        }

        // Iterate through all sprite and step pairs in tileData
        foreach (var entry in tileData)
        {
            string spriteName = entry.Key;
            List<int> steps = entry.Value;
            Sprite sprite = GetSpriteByName(spriteName);

            if (sprite != null)
            {
                Debug.Log($"Displaying sprite {sprite.name} for steps {string.Join(", ", steps)}");

                // Iterate through boardCells to find cells with matching steps
                for (int x = 0; x < BoardManager.Instance.boardCells.GetLength(0); x++)
                {
                    for (int y = 0; y < BoardManager.Instance.boardCells.GetLength(1); y++)
                    {
                        Cell cell = BoardManager.Instance.boardCells[x, y];
                        if (cell != null && steps.Contains((int)cell.step))
                        {
                            cell.SetSprite(sprite);
                            Debug.Log($"Displayed sprite {sprite.name} on cell at position ({x}, {y}) with step {cell.step}.");

                            // Apply note to HelmSequencer
                            int midiNote = GetMidiNoteForSprite(spriteName);
                            helmSequencer.AddNote(midiNote, cell.step, cell.step + 1, 1.0f);
                            Debug.Log($"Added MIDI {midiNote} at Step = {cell.step}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError($"Sprite with name {spriteName} not found.");
            }
        }
    }

    private int GetMidiNoteForSprite(string spriteName)
    {
        // Try to extract the number from the sprite name and use it to calculate the MIDI note
        if (int.TryParse(spriteName.Replace("key_", ""), out int keyNumber) && keyNumber >= 1 && keyNumber <= 36)
        {
            return 59 + keyNumber; // MIDI note calculation: 60 (C4) + (keyNumber - 1)
        }
        return 60; // Default to C4 if sprite name is not recognized or out of range
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
            int midiNote = i + 33; // Example MIDI note generation

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

        // Set KeyManager as the last clicked manager
        ManagerHandler.Instance.SetLastClickedManager(true);

        // Update the current sprite tracked by KeyManager
        SpriteRenderer spriteRenderer = clickedKey.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            currentSprite = spriteRenderer.sprite;

            // Scale the clicked key temporarily
            StartCoroutine(ScaleKey(clickedKey));
            BoardManager.Instance.ResetBoard();
            
            // Load tile data for keys
            DisplaySpriteOnMatchingSteps();
        }

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
            BoardManager.Instance.helm.GetComponent<HelmSequencer>().NoteOn(midiNote, 1.0f);

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
            BoardManager.Instance.helm.GetComponent<HelmSequencer>().NoteOff(midiNote);

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

    public void OnManagerClicked()
    {
        ManagerHandler.Instance.SetLastClickedManager(true);
    }

    public void SaveKeyTileData(Sprite sprite, int step)
    {
        if (sprite != null)
        {
            string spriteName = sprite.name;

            if (tileData.ContainsKey(spriteName))
            {
                if (!tileData[spriteName].Contains(step))
                {
                    tileData[spriteName].Add(step); // Add the new step if it doesn't already exist
                }
            }
            else
            {
                tileData.Add(spriteName, new List<int> { step }); // Create a new entry with the step
            }

            Debug.Log($"Saved Key Tile Data: Sprite = {sprite.name}, Step = {step}");
        }
        else
        {
            Debug.LogError("Sprite is null. Cannot save tile data.");
        }
    }

    // Helper method to get a sprite by its name
    private Sprite GetSpriteByName(string spriteName)
    {
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("The sprites array is null or empty.");
            return null;
        }

        Debug.Log($"Looking for sprite with name: {spriteName}");

        foreach (Sprite sprite in sprites)
        {
            Debug.Log($"Checking sprite: {sprite.name}");
            if (sprite.name == spriteName)
            {
                Debug.Log($"Found sprite: {sprite.name}");
                return sprite;
            }
        }

        Debug.LogError($"Sprite with name {spriteName} not found.");
        return null;
    }
}

[System.Serializable]
public class KeyTileData
{
    public string SpriteName; // Store sprite name instead of sprite object
    public float Step;
}

[System.Serializable]
public class KeyTileDataList
{
    public List<KeyTileData> tileData;
}
