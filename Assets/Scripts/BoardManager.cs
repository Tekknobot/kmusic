using UnityEngine;
using AudioHelm;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;   // Singleton instance
    public GameObject cellPrefab;
    public AudioHelm.SampleSequencer sequencer;
    public GameObject sampler;

    public Cell[,] boardCells; // 2D array to store references to all board cells

    private float stepCount;

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

        if (sequencer == null)
        {
            Debug.LogError("Sequencer is not assigned in BoardManager.");
        }

        InitializeBoard();
    }

    void Start() {
        sequencer = GameObject.Find("Sequencer").GetComponent<AudioHelm.SampleSequencer>();
    }

    void Update() {
        foreach (Cell cell in boardCells)
        {
            if (cell != null)
            {        
                if (sequencer.currentIndex == cell.step) {
                    HighlightCellOnStep(sequencer.currentIndex);
                }
                else {
                    cell.GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
        }
    }

    private void InitializeBoard()
    {
        // Assuming a fixed size board of 8x8 cells
        int boardSizeX = 8;
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

    // Method to highlight a cell when sequencer.currentIndex matches cell step
    private void HighlightCellOnStep(int stepIndex)
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
}
