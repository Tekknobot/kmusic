using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; } // Singleton instance

    [Header("Prefabs")]
    public GameObject cellPrefab;   // Reference to the cell prefab

    [Header("Grid Settings")]
    public Vector2Int gridSize = new Vector2Int(8, 8);   // Size of the grid (8x8)

    public Sprite defaultSprite; // Default sprite for cells

    private int cellStep = 0; // Variable to track the step count
    private List<Sprite> tileSprites = new List<Sprite>(); // List to store tile sprites based on cell interactions

    public int CellStep => cellStep; // Expression-bodied property for cellStep

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Duplicate instance of BoardManager found. Destroying this instance.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        // Reset cellStep to 0 each time GenerateBoard is called
        cellStep = 0;

        // Validate that cellPrefab is assigned
        if (cellPrefab == null)
        {
            Debug.LogError("Cell prefab is not assigned in BoardManager.");
            return;
        }

        // Loop through each cell position in the grid
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                // Calculate the position for the new cell
                Vector3 cellPosition = new Vector3(x, y, 0);

                // Instantiate a new cell from the prefab at the calculated position
                GameObject newCellObject = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                Cell newCell = newCellObject.GetComponent<Cell>();

                // Optionally, you can parent the new cell under the BoardManager GameObject for organization
                newCellObject.transform.parent = transform;
                newCellObject.name = cellStep.ToString();

                // Set the default sprite for the cell
                newCell.GetComponent<SpriteRenderer>().sprite = defaultSprite;

                // Increment cellStep for each instantiated cell
                cellStep++;
            }
        }
    }

    // Method to reset the board to display default sprites
    public void ResetBoard()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child != null)
            {
                Cell cell = child.GetComponent<Cell>();
                if (cell != null)
                {
                    cell.ReplaceSprite(defaultSprite); // Replace each cell's sprite with defaultSprite
                }
            }
        }
    }

    // Method to save tile sprite
    public void SaveTileSprite(Sprite sprite)
    {
        tileSprites.Add(sprite);
    }

    // Method to update the board with saved tile sprites
    public void UpdateBoardWithSavedSprites()
    {
        // Clear the board
        ResetBoard();

        // Display saved tile sprites
        for (int i = 0; i < tileSprites.Count; i++)
        {
            Cell cell = GetCellAtIndex(i);
            if (cell != null)
            {
                cell.ReplaceSprite(tileSprites[i]);
            }
        }
    }

    private Cell GetCellAtIndex(int index)
    {
        if (index >= 0 && index < transform.childCount)
        {
            Transform child = transform.GetChild(index);
            if (child != null)
            {
                return child.GetComponent<Cell>();
            }
        }
        return null;
    }
}
