using UnityEngine;
using AudioHelm;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; } // Singleton instance
    public GameObject cellPrefab;
    public AudioHelm.SampleSequencer sequencer;
    public AudioHelm.HelmSequencer helm;
    public GameObject sampler;
    public Cell[,] boardCells; // 2D array to store references to all board cells
    private Dictionary<Vector2Int, Sprite> cellSprites; // Dictionary to keep track of sprites on each cell
    public Dictionary<float, Sprite> stepToSpriteMap = new Dictionary<float, Sprite>(); // Dictionary to store sprite by step

    private float stepCount;

    public int highlightedCellIndex = 0; // Example placeholder for the highlighted cell index

    // Method to get the current highlighted cell index
    public int GetHighlightedCellIndex()
    {
        return highlightedCellIndex;
    }

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: if you want this to persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (sequencer == null)
        {
            Debug.LogError("Sequencer is not assigned in BoardManager.");
        }

        // Initialize the board and sprite dictionaries
        InitializeBoard();
        InitializeSpriteDictionary();
        InitializeStepToSpriteMap();
    }

    void Start()
    {
        
    }

    void Update()
    {
        sequencer = PatternManager.Instance.drumSequencer.GetComponent<SampleSequencer>();

        // Get the current step index based on currentIndex
        int stepIndexInRange = sequencer.currentIndex % 16;

        // Iterate through each cell on the board
        foreach (Cell cell in boardCells)
        {
            if (cell != null)
            {
                int cellStepIndex = (int)cell.step % 16; // Convert cell step to 0-15 range

                // Check if the cell should be highlighted
                if (stepIndexInRange == cellStepIndex)
                {
                    HighlightCell(cell);
                }
                else
                {
                    // Reset the cell color if not highlighted
                    cell.GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
        }
    }


    // Method to highlight a cell when sequencer.currentIndex matches cell step
    public void HighlightCellOnStep(int stepIndex)
    {
        // Iterate through all board cells
        for (int x = 0; x < boardCells.GetLength(0); x++)
        {
            for (int y = 0; y < boardCells.GetLength(1); y++)
            {
                Cell cell = boardCells[x, y];
                if (cell != null && cell.step == stepIndex)
                {
                    HighlightCell(cell);
                    highlightedCellIndex = stepIndex;
                }
            }
        }
    }

    // Method to highlight a single cell by changing its sprite color to grey
    private void HighlightCell(Cell cell)
    {
        // Change sprite color to grey
        SpriteRenderer spriteRenderer = cell.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.grey;
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found on cell.");
        }
    }

    private void InitializeBoard()
    {
        // Assuming a fixed size board of 8x8 cells
        int boardSizeX = 2;
        int boardSizeY = 8;

        // Initialize the boardCells 2D array
        boardCells = new Cell[boardSizeX, boardSizeY];

        // Loop through each cell position and instantiate a new cell
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Vector3 cellPosition = new Vector3(y, x, 0); // Adjust position as needed

                // Instantiate a new cell prefab (assuming you have a prefab assigned in the inspector)
                GameObject cellObject = Instantiate(cellPrefab, cellPosition, Quaternion.identity);

                // Parent the cell under the BoardManager for organization (optional)
                cellObject.transform.parent = transform;
                cellObject.name = stepCount.ToString();
                cellObject.GetComponent<Cell>().step = stepCount;
                stepCount++;

                // Get the Cell component from the instantiated GameObject
                Cell cell = cellObject.GetComponent<Cell>();

                // Set default sprite for the cell
                cell.SetSprite(cell.defaultSprite);

                // Store the cell in the boardCells array
                boardCells[x, y] = cell;

                // Optionally, you can store additional data or handle other cell initialization here
            }
        }
    }

    private void InitializeSpriteDictionary()
    {
        // Initialize the dictionary to keep track of sprites on each cell
        cellSprites = new Dictionary<Vector2Int, Sprite>();

        // Populate the dictionary with default sprites
        for (int x = 0; x < boardCells.GetLength(0); x++)
        {
            for (int y = 0; y < boardCells.GetLength(1); y++)
            {
                Cell cell = boardCells[x, y];
                if (cell != null)
                {
                    // Set the default sprite for the cell
                    cellSprites[new Vector2Int(x, y)] = cell.defaultSprite;
                }
            }
        }
    }

    private void InitializeStepToSpriteMap()
    {
        stepToSpriteMap.Clear(); // Ensure it's cleared before populating

        // Populate the stepToSpriteMap dictionary with default sprites from cells
        foreach (Cell cell in boardCells)
        {
            if (cell != null)
            {
                stepToSpriteMap[cell.step] = cell.defaultSprite;
            }
        }
    }

    // Method to update the sprite for a cell
    public void UpdateCellSprite(int x, int y, Sprite newSprite)
    {
        if (x >= 0 && x < boardCells.GetLength(0) && y >= 0 && y < boardCells.GetLength(1))
        {
            Cell cell = boardCells[x, y];
            if (cell != null)
            {
                cell.SetSprite(newSprite);
                cellSprites[new Vector2Int(x, y)] = newSprite; // Update the dictionary

                // Update the stepToSpriteMap with the new sprite
                stepToSpriteMap[cell.step] = newSprite;
            }
            else
            {
                Debug.LogError("Cell not found at coordinates (" + x + ", " + y + ").");
            }
        }
        else
        {
            Debug.LogError("Attempted to update sprite for cell out of board bounds.");
        }
    }

    // Method to get the current sprite of a cell by iterating through all cells
    public Sprite GetSpriteByStep(float step)
    {
        // Iterate through all cells in the board
        foreach (Cell cell in boardCells)
        {
            if (cell != null && cell.step == step)
            {
                return cell.CurrentSprite;
            }
        }

        Debug.LogError($"No sprite found for step {step}.");
        return null;
    }


    // Example method to reset the board to its default state
    public void ResetBoard()
    {
        foreach (Cell cell in boardCells)
        {
            if (cell != null)
            {
                cell.SetSprite(cell.defaultSprite);
                // Optionally reset other properties of the cell
            }
        }

        // Reset the sprite dictionary
        InitializeSpriteDictionary();
        InitializeStepToSpriteMap();
    }

    // Method to get a reference to the cell at specified coordinates
    public Cell GetCell(int x, int y)
    {
        if (x >= 0 && x < boardCells.GetLength(0) && y >= 0 && y < boardCells.GetLength(1))
        {
            return boardCells[x, y];
        }
        else
        {
            Debug.LogError("Attempted to access cell out of board bounds.");
            return null;
        }
    }

    // Method to get the position of a cell on the board
    public Vector2Int GetCellPosition(Cell cell)
    {
        for (int x = 0; x < boardCells.GetLength(0); x++)
        {
            for (int y = 0; y < boardCells.GetLength(1); y++)
            {
                if (boardCells[x, y] == cell)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        Debug.LogError("Cell not found on the board.");
        return Vector2Int.zero;
    }

    // Method to get the size of the board (x, y)
    public Vector2Int GetBoardSize()
    {
        return new Vector2Int(boardCells.GetLength(0), boardCells.GetLength(1));
    }



    public void UpdateBoardWithNotes(List<AudioHelm.Note> notes)
    {
        int stepsPerPattern = 16;
        int currentPatternIndex = PatternManager.Instance.currentPatternIndex;

        // Validate currentPatternIndex
        if (currentPatternIndex < 1)
        {
            Debug.LogError("Invalid currentPatternIndex. It must be 1 or greater.");
            return;
        }

        int patternStartStep = (currentPatternIndex - 1) * stepsPerPattern;
        int patternEndStep = patternStartStep + stepsPerPattern - 1;

        // Log pattern range details
        Debug.Log($"Updating board for Pattern Index: {currentPatternIndex}");
        Debug.Log($"Pattern Start Step: {patternStartStep}");
        Debug.Log($"Pattern End Step: {patternEndStep}");

        HelmSequencer currentPattern = PatternManager.Instance.sequencerPrefab.GetComponent<HelmSequencer>();
        if (currentPattern != null)
        {
            int totalSteps = currentPattern.length;

            // Ensure pattern end step does not exceed the total number of steps
            if (patternEndStep >= totalSteps)
            {
                patternEndStep = totalSteps - 1;
            }
        }
        else
        {
            Debug.LogWarning("Current pattern is not available.");
            ResetBoard();
            return;
        }

        // Process notes
        foreach (var note in notes)
        {
            int noteStep = (int)note.start;

            // Ensure the note is within the current pattern range
            if (noteStep < patternStartStep || noteStep > patternEndStep)
            {
                Debug.Log($"Skipping note at step {noteStep}, outside of pattern range.");
                continue;
            }

            int adjustedStep = noteStep - patternStartStep;

            if (adjustedStep < 0 || adjustedStep >= stepsPerPattern)
            {
                Debug.LogWarning($"Adjusted step {adjustedStep} is out of the 16-step range for note {note.note}.");
                continue;
            }

            var sprite = KeyManager.Instance.GetSpriteFromNote(note.note);

            if (sprite == null)
            {
                Debug.LogError($"No sprite found for adjusted step {adjustedStep}. Note: {note.note}");
                continue;
            }

            var cell = GetCellByStep(adjustedStep);
            if (cell != null)
            {
                Debug.Log($"Setting sprite {sprite.name} for cell at adjusted step {adjustedStep}");
                cell.SetSprite(sprite);
            }
            else
            {
                Debug.LogError($"Cell not found for adjusted step {adjustedStep}. Note: {note.note}");
            }
        }

        // Optional: Log if no notes were processed
        if (notes.Count == 0)
        {
            Debug.LogWarning("No notes provided to update the board.");
        }
    }

    public void UpdateBoardWithSampleNotes(List<AudioHelm.Note> notes)
    {
        Debug.Log("Updating board with sample notes.");

        if (PatternManager.Instance.boardManager == null)
        {
            Debug.LogError("BoardManager not assigned.");
            return;
        }

        if (PatternManager.Instance == null || PatternManager.Instance.sampleSequencerPrefab == null)
        {
            Debug.LogError("PatternManager or SampleSequencerPrefab is not set.");
            return;
        }

        SampleSequencer currentPattern = PatternManager.Instance.sampleSequencerPrefab.GetComponent<SampleSequencer>();

        if (currentPattern != null)
        {
            int stepsPerPattern = 16;
            int totalSteps = currentPattern.length;

            // Adjust pattern index to be 1-based and calculate start and end steps
            int currentPatternIndex = PatternManager.Instance.currentPatternIndex;
            int patternStartStep = (currentPatternIndex - 1) * stepsPerPattern;
            int patternEndStep = patternStartStep + stepsPerPattern - 1;

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

            // Reset the board before updating
            PatternManager.Instance.boardManager.ResetBoard();

            // Filter notes based on the current pattern section
            List<AudioHelm.Note> notesInSection = notes.FindAll(note =>
                note.start >= patternStartStep && note.start <= patternEndStep
            );

            // Log filtered notes
            Debug.Log($"Notes in Section: {notesInSection.Count}");
            foreach (var note in notesInSection)
            {
                Debug.Log($"Filtered Note at step {note.start}");
            }

            // Process filtered notes
            foreach (var note in notesInSection)
            {
                int noteStep = (int)note.start;

                // Calculate the local step within the pattern section
                int adjustedStep = noteStep - patternStartStep;

                if (adjustedStep < 0 || adjustedStep >= stepsPerPattern)
                {
                    Debug.LogWarning($"Adjusted step {adjustedStep} is out of the 16-step range for note {note.note}.");
                    continue;
                }

                // Get the sprite for the adjusted step
                var sprite = SampleManager.Instance.GetSpriteFromNote(note.note);

                if (sprite == null)
                {
                    Debug.LogError($"No sprite found for step {adjustedStep}.");
                    continue;
                }

                // Find the cell and update its sprite
                var cell = PatternManager.Instance.boardManager.GetCellByStep(adjustedStep);
                if (cell != null)
                {
                    Debug.Log($"Setting sprite {sprite.name} for cell at adjusted step {adjustedStep}");
                    cell.SetSprite(sprite);
                }
                else
                {
                    Debug.LogError($"Cell not found for adjusted step {adjustedStep}. Note: {note.note}");
                }
            }

            // Optional: Log if no notes were processed
            if (notesInSection.Count == 0)
            {
                Debug.LogWarning("No notes within the current pattern section.");
            }
        }
        else
        {
            Debug.LogWarning("Current sample pattern is not available.");
        }
    }

    public Cell GetCellByStep(float step)
    {
        foreach (Cell cell in boardCells)
        {
            if (cell != null && cell.step == step)
            {
                return cell;
            }
        }

        Debug.LogError($"No cell found with step {step}.");
        return null;
    }
}
