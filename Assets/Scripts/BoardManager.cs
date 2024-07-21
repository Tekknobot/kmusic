using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; } // Singleton instance

    public GameObject cellPrefab;   // Reference to the cell prefab
    public Vector2Int gridSize = new Vector2Int(8, 8);   // Size of the grid (8x8)

    private int cellStep = 0; // Variable to track the step count
    private List<Sprite> tileSprites = new List<Sprite>(); // List to store tile sprites based on cell interactions
    private Sprite defaultSprite; // Default sprite to display

    public int CellStep
    {
        get { return cellStep; }
    }

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

                // Set the default sprite from the PadManager class
                defaultSprite = PadManager.DefaultSprite;

                // Increment cellStep for each instantiated cell
                cellStep++;
            }
        }

        // Display default sprites initially since there are no saved tile sprites
        DisplayDefaultSprites();
    }

    public void SaveTileSprite(Sprite sprite, string midiNote)
    {
        tileSprites.Add(sprite);
    }

    public void DisplayTileSprites()
    {
        for (int i = 0; i < tileSprites.Count; i++)
        {
            Cell cell = GetCellAtIndex(i);
            if (cell != null)
            {
                cell.ReplaceSprite(tileSprites[i]);
            }
        }
    }

    private void DisplayDefaultSprites()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Cell cell = GetCellAtIndex(i);
            if (cell != null)
            {
                cell.ReplaceSprite(defaultSprite); // Use default sprite from PadManager class
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
