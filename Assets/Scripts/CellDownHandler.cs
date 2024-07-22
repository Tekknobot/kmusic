using UnityEngine;

public class CellDownHandler : MonoBehaviour
{
    private Cell cell;

    private void Start()
    {
        cell = GetComponent<Cell>();
        if (cell == null)
        {
            Debug.LogError("Cell component not found on GameObject.");
            enabled = false; // Disable the script if Cell component is not found
        }
    }

    private void OnMouseDown()
    {
        // Check if the mouse is over this cell
        if (cell.IsMouseOver())
        {
            // Rotate and return the cell
            cell.RotateAndReturn();
        }
    }
}