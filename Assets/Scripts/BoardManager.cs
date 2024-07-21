using UnityEngine;
using System.Collections;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;   // Singleton instance
    public GameObject cellPrefab;
    
    private Cell[,] boardCells; // 2D array to store references to all board cells

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

        InitializeBoard();
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
                Vector3 cellPosition = new Vector3(x, y, 0); // Adjust position as needed

                // Instantiate a new cell prefab (assuming you have a prefab assigned in the inspector)
                GameObject cellObject = Instantiate(cellPrefab, cellPosition, Quaternion.identity);

                // Parent the cell under the BoardManager for organization (optional)
                cellObject.transform.parent = transform;

                // Get the Cell component from the instantiated GameObject
                Cell cell = cellObject.GetComponent<Cell>();

                // Set default sprite for the cell
                cell.SetSprite(cell.defaultSprite);

                // Store the cell in the boardCells array
                boardCells[x, y] = cell;
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

    // Example method to display saved tiles for a specific sprite
    public void DisplaySavedTilesForSprite(Sprite sprite)
    {
        foreach (Cell cell in boardCells)
        {
            if (cell != null && cell.hasNote)
            {
                cell.ReplaceSprite(sprite);
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

    // Example method to handle gameplay logic when a cell is clicked
    public void OnCellClicked(int x, int y)
    {
        Cell clickedCell = GetCell(x, y);
        if (clickedCell != null)
        {
            // Perform actions based on the clicked cell
            // Example: Rotate the clicked cell
            clickedCell.RotateAndReturn();
        }
    }

    // Method to get the size of the board (x, y)
    public Vector2Int GetBoardSize()
    {
        return new Vector2Int(boardCells.GetLength(0), boardCells.GetLength(1));
    }
}
