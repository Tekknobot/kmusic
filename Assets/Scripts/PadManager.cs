using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioHelm;

public class PadManager : MonoBehaviour
{
    public static PadManager Instance  { get; private set; }   // Singleton instance

    public GameObject padPrefab;         // Reference to the pad prefab
    public Sprite[] sprites;             // Array of sprites to assign to each pad

    public Sprite currentSprite;         // Current sprite tracked by PadManager
    public Sprite lastClickedSprite;    // Last clicked sprite

    public static Sprite DefaultSprite { get; private set; } // Static property to access defaultSprite

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>(); // Dictionary to store original scales of pads

    private Cell[,] boardCells; // 2D array to store references to all board cells

    public Dictionary<string, List<TileData>> tileDataGroups = new Dictionary<string, List<TileData>>(); // Dictionary to store TileData grouped by sprite

    public SampleSequencer sequencer;

    // Dictionary to map sprites to MIDI notes
    private Dictionary<Sprite, int> spriteMidiNoteMap = new Dictionary<Sprite, int>();

    public int midiNote;
    public Sprite selectedSprite; // New variable to hold the currently selected sprite


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

        InitializeSpriteMidiNoteMap(sprites);
        
        GeneratePads();
        tileDataGroups = DataManager.LoadTileDataFromFile();       
    }

    // Method to initialize the dictionary with sprites and MIDI notes
    private void InitializeSpriteMidiNoteMap(IEnumerable<Sprite> sprites)
    {
        int startingMidiNote = 60;
        
        foreach (var sprite in sprites)
        {
            if (!spriteMidiNoteMap.ContainsKey(sprite))
            {
                spriteMidiNoteMap[sprite] = startingMidiNote;
                startingMidiNote++;
            }
        }
        
        Debug.Log($"Initialized sprite MIDI note map with {spriteMidiNoteMap.Count} entries.");
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
        ManagerHandler.Instance.SetLastClickedManager(true, false, false); 

        // Reset the board to display default configuration first
        BoardManager.Instance.ResetBoard();

        // Update the current sprite tracked by PadManager
        SpriteRenderer spriteRenderer = clickedPad.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // lastClickedSprite = spriteRenderer.sprite; // Update last clicked sprite
            currentSprite = spriteRenderer.sprite;
            selectedSprite = spriteRenderer.sprite;

            // Scale the clicked pad temporarily
            StartCoroutine(ScalePad(clickedPad));

            // Display the sprite on cells with matching step data
            DisplaySpriteOnMatchingSteps(currentSprite, spriteMidiNoteMap[currentSprite]);

            // Play sample
            BoardManager.Instance.sequencer.GetComponent<SampleSequencer>().NoteOn(midiNote, 1.0f);
        }

        // Additional debug information
        Debug.Log($"Clicked Pad: {clickedPad.name}");
    }

    /// <summary>
    /// Updates the board for the given pad without scaling or playing a sound.
    /// </summary>
    /// <param name="clickedPad">The GameObject representing the clicked pad.</param>
    public void UpdateBoardForPad(GameObject clickedPad)
    {
        if (clickedPad == null)
        {
            Debug.LogWarning("No pad provided to update the board.");
            return;
        }

        // Update the MIDI note from the clicked pad
        midiNote = clickedPad.GetComponent<PadClickHandler>().midiNote;

        // Reset the board to display default configuration first
        BoardManager.Instance.ResetBoard();

        // Update the current sprite tracked by PadManager
        SpriteRenderer spriteRenderer = clickedPad.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            currentSprite = spriteRenderer.sprite;

            // Display the sprite on cells with matching step data
            DisplaySpriteOnMatchingSteps(currentSprite, spriteMidiNoteMap[currentSprite]);
        }

        // Additional debug information
        Debug.Log($"Updated board for pad: {clickedPad.name}");
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

    private void DisplaySpriteOnMatchingSteps(Sprite sprite, int spriteMidiNote)
    {
        Debug.Log($"Displaying sprite '{sprite.name}' on matching steps for MIDI Note {spriteMidiNote}.");

        SampleSequencer currentDrumSequencer = PatternManager.Instance.drumSequencer.GetComponent<SampleSequencer>();

        if (currentDrumSequencer == null)
        {
            Debug.LogError("No active drum sequencer found.");
            return;
        }

        int stepsPerPattern = 16;
        int currentPatternIndex = PatternManager.Instance.currentPatternIndex;
        int patternStartStep = (currentPatternIndex - 1) * stepsPerPattern;
        int patternEndStep = patternStartStep + stepsPerPattern - 1;
        int totalSteps = currentDrumSequencer.length;

        // Ensure patternEndStep does not exceed total steps of the sequencer
        if (patternEndStep >= totalSteps)
        {
            patternEndStep = totalSteps - 1;
        }

        // Debugging logs to verify calculations
        Debug.Log($"Current Pattern Index: {currentPatternIndex}");
        Debug.Log($"Pattern Start Step: {patternStartStep}");
        Debug.Log($"Pattern End Step: {patternEndStep}");
        Debug.Log($"Total Steps in Sequencer: {totalSteps}");

        foreach (var note in currentDrumSequencer.GetAllNotes())
        {
            int step = (int)note.start;
            int midiNote = note.note;

            // Calculate local step within the current pattern section
            int localStep = step - patternStartStep;

            // Debugging each note
            Debug.Log($"Processing note at step {step} (Local Step: {localStep}), MIDI Note: {midiNote}");

            if (midiNote == spriteMidiNote)
            {
                if (localStep >= 0 && localStep < stepsPerPattern)
                {
                    Debug.Log($"Note at step {step} (Local Step: {localStep}) is within the range.");

                    Cell cell = BoardManager.Instance.GetCellByStep(localStep);
                    if (cell != null)
                    {
                        cell.SetSprite(sprite);
                        Debug.Log($"Set sprite '{sprite.name}' on cell at step {localStep} (MIDI Note: {midiNote}).");
                    }
                    else
                    {
                        Debug.LogWarning($"No cell found for step {localStep}. Unable to set sprite.");
                    }
                }
                else
                {
                    Debug.Log($"Note at step {step} (Local Step: {localStep}) is outside the range.");
                }
            }
            else
            {
                Debug.Log($"Note MIDI Note {midiNote} does not match sprite MIDI Note {spriteMidiNote}.");
            }
        }

        Debug.Log($"Displayed sprite '{sprite.name}' on board based on the current drum pattern for MIDI Note {spriteMidiNote}.");
    }

    /// <summary>
    /// Finds the pad GameObject corresponding to the selectedSprite.
    /// </summary>
    /// <returns>The GameObject of the corresponding pad, or null if no match is found.</returns>
    public GameObject GetPadByCurrentPad()
    {
        if (selectedSprite == null)
        {
            Debug.LogWarning("No current sprite is available. Returning null.");
            return null;
        }

        foreach (var padObject in originalScales.Keys)
        {
            SpriteRenderer spriteRenderer = padObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite == selectedSprite)
            {
                Debug.Log($"Found pad corresponding to current sprite: {padObject.name}");
                return padObject;
            }
        }

        Debug.LogWarning("No pad found corresponding to the current sprite.");
        return null;
    }


}
