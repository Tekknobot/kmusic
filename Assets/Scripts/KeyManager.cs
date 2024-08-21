using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AudioHelm;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance  { get; private set; }   // Singleton instance

    public GameObject keyPrefab;         // Reference to the key prefab
    public Sprite[] sprites;             // Array of sprites to assign to each key

    public Sprite currentSprite;         // Current sprite tracked by KeyManager
    private Sprite lastClickedSprite;    // Last clicked sprite

    public static Sprite DefaultSprite { get; private set; } // Static property to access defaultSprite

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>(); // Dictionary to store original scales of keys

    private Cell[,] boardCells; // 2D array to store references to all board cells

    public Dictionary<string, List<int>> tileData = new Dictionary<string, List<int>>(); // Changed to use string keys

    private Dictionary<int, Sprite> noteToSpriteMap;

    public int midiNote;

    private Dictionary<Sprite, int> spriteToNoteMap;

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
        PopulateNoteToSpriteMap();
        
        // Start the coroutine to load tile data after a delay
        StartCoroutine(LoadTileDataAfterDelay(0.2f)); // Adjust the delay time as needed
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

    private void PopulateNoteToSpriteMap()
    {
        // Initialize the dictionary
        noteToSpriteMap = new Dictionary<int, Sprite>();

        // Assuming you have MIDI notes and sprites mapped
        for (int i = 0; i < sprites.Length; i++)
        {
            int midiNote = i + 33; // Example MIDI note calculation
            noteToSpriteMap[midiNote] = sprites[i];
        }
    }

    // Method to handle when a key is clicked
    public void OnKeyClicked(GameObject clickedKey)
    {
        midiNote = clickedKey.GetComponent<KeyClickHandler>().midiNote;

        // Set KeyManager as the last clicked manager
        ManagerHandler.Instance.SetLastClickedManager(true, false, false);

        // Update the current sprite tracked by KeyManager
        SpriteRenderer spriteRenderer = clickedKey.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            currentSprite = spriteRenderer.sprite;

            // Scale the clicked key temporarily
            StartCoroutine(ScaleKey(clickedKey));
            //BoardManager.Instance.ResetBoard();
        }

        // Additional debug information
        Debug.Log($"Clicked Key: {clickedKey.name}");
        
        // Load tile data for keys
        //DisplaySpriteOnMatchingSteps();
    }

    // Method to handle when a key is pressed down
    public void OnKeyPressDown(GameObject clickedKey)
    {
        KeyClickHandler keyClickHandler = clickedKey.GetComponent<KeyClickHandler>();
        if (keyClickHandler != null)
        {
            int midiNote = keyClickHandler.midiNote;
            BoardManager.Instance.helm.GetComponent<HelmSequencer>().NoteOn(midiNote, 1.0f);

            // Reset the board to display default configuration first
            //BoardManager.Instance.ResetBoard();

           //DisplaySpriteOnMatchingSteps();

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
        ManagerHandler.Instance.SetLastClickedManager(true, false, false);
    }

    // Method to display sprites on cells with matching step data
    public void DisplaySpriteOnMatchingSteps()
    {
        Debug.Log("Displaying all saved tiles for KeyManager.");

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
                            HelmSequencer helmSequencer = BoardManager.Instance.helm.GetComponent<HelmSequencer>();
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

    public void RemoveKeyTileData(Sprite sprite, int step)
    {
        if (sprite == null)
        {
            Debug.LogError("Sprite is null. Cannot remove tile data.");
            return;
        }

        string spriteName = sprite.name;

        // Check if the sprite to be removed is in the tileData
        if (!tileData.ContainsKey(spriteName))
        {
            Debug.LogWarning($"Sprite = {spriteName} not found in tile data.");
            return;
        }

        List<int> steps = tileData[spriteName];

        // Check if the step exists in the list for the sprite
        if (!steps.Contains(step))
        {
            Debug.LogWarning($"Step {step} not found for Sprite = {spriteName}.");
            return;
        }

        // Remove the step from the list
        steps.Remove(step);

        // Remove the sprite from tileData if it has no remaining steps
        if (steps.Count == 0)
        {
            tileData.Remove(spriteName);

            // If the sprite being removed is not the default sprite
            if (sprite != DefaultSprite)
            {
                // Update tileData to add the step to the default sprite
                if (DefaultSprite != null)
                {
                    string defaultSpriteName = DefaultSprite.name;
                    
                    if (!tileData.ContainsKey(defaultSpriteName))
                    {
                        tileData[defaultSpriteName] = new List<int>();
                    }

                    // Add the step to default sprite if not already present
                    if (!tileData[defaultSpriteName].Contains(step))
                    {
                        tileData[defaultSpriteName].Add(step);
                    }
                }
            }

            Debug.Log($"Removed Key Tile Data: Sprite = {sprite.name}, Step = {step}");
        }
        else
        {
            Debug.Log($"Removed Step = {step} from Sprite = {sprite.name}");
        }
    }

    public void UpdateCellSprite(Cell cell, Sprite newSprite)
    {
        if (cell == null || newSprite == null)
        {
            Debug.LogError("Cell or new sprite is null.");
            return;
        }

        // Get the old sprite from the cell
        Sprite oldSprite = cell.CurrentSprite; // Assume you have a method to get the current sprite

        // Remove old sprite data from tileData
        if (oldSprite != null)
        {
            string oldSpriteName = oldSprite.name;
            if (tileData.ContainsKey(oldSpriteName))
            {
                tileData[oldSpriteName].Remove((int)cell.step);
                if (tileData[oldSpriteName].Count == 0)
                {
                    tileData.Remove(oldSpriteName);
                }
            }
        }

        // Set new sprite on the cell
        cell.SetSprite(newSprite); // Assume you have a method to set the sprite

        // Save new sprite data
        SaveKeyTileData(newSprite, (int)cell.step);

        Debug.Log($"Updated cell at step {cell.step} to new sprite: {newSprite.name}");
    }

    public void ReapplyPatterns()
    {
        foreach (var entry in tileData)
        {
            string spriteName = entry.Key;
            List<int> steps = entry.Value;
            Sprite sprite = GetSpriteByName(spriteName);

            if (sprite != null)
            {
                foreach (var step in steps)
                {
                    Cell cell = BoardManager.Instance.GetCellByStep(step); // Assume you have a method to get a cell by step
                    if (cell != null)
                    {
                        cell.SetSprite(sprite);
                        Debug.Log($"Reapplied sprite {sprite.name} to cell at step {step}");
                    }
                }
            }
            else
            {
                Debug.LogError($"Sprite with name {spriteName} not found.");
            }
        }
    }


    // Helper method to get a sprite by its name
    public Sprite GetSpriteByName(string spriteName)
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

    private int GetMidiNoteForSprite(string spriteName)
    {
        // Try to extract the number from the sprite name and use it to calculate the MIDI note
        if (int.TryParse(spriteName.Replace("key_", ""), out int keyNumber) && keyNumber >= 1 && keyNumber <= 36)
        {
            return 32 + keyNumber; // MIDI note calculation: 33 (A0) + (keyNumber - 1)
        }
        return 32;
    }    

    public Sprite GetSpriteByStep(int step)
    {
        if (BoardManager.Instance.stepToSpriteMap.ContainsKey(step))
        {
            return BoardManager.Instance.stepToSpriteMap[step];
        }
        else
        {
            Debug.LogError($"No sprite found for step {step}.");
        }

        // Ensure we have tile data populated
        if (tileData == null || tileData.Count == 0)
        {
            Debug.LogError("Tile data is not initialized or empty.");
            return null;
        }

        // Iterate through all sprite and step pairs in tileData
        foreach (var entry in tileData)
        {
            string spriteName = entry.Key;
            List<int> steps = entry.Value;

            // Check if the step is present in the list of steps for the current sprite
            if (steps.Contains(step))
            {
                // Get the sprite by name
                Sprite sprite = GetSpriteByName(spriteName);
                if (sprite != null)
                {
                    return sprite;
                }
                else
                {
                    Debug.LogError($"Sprite with name {spriteName} not found.");
                }
            }
        }

        Debug.LogError($"No sprite found for step {step}.");
        return null;
    }

    public Sprite GetSpriteFromNote(int midiNote)
    {
        // Check if the dictionary has a mapping for the given MIDI note
        if (noteToSpriteMap != null && noteToSpriteMap.ContainsKey(midiNote))
        {
            return noteToSpriteMap[midiNote];
        }
        else
        {
            Debug.LogError($"No sprite found for MIDI note {midiNote}.");
            return null;
        }
    }


    private void InitializeReverseDictionary()
    {
        spriteToNoteMap = new Dictionary<Sprite, int>();
        foreach (var kvp in noteToSpriteMap)
        {
            if (!spriteToNoteMap.ContainsKey(kvp.Value))
            {
                spriteToNoteMap.Add(kvp.Value, kvp.Key);
            }
        }
    }

    public int GetNoteFromSprite(Sprite sprite)
    {
        // Check if the reverse dictionary has a mapping for the given sprite
        if (spriteToNoteMap != null && spriteToNoteMap.ContainsKey(sprite))
        {
            return spriteToNoteMap[sprite];
        }
        else
        {
            Debug.LogError($"No MIDI note found for sprite {sprite.name}.");
            return -1; // or some other invalid note indicator
        }
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
