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
        if (PatternManager.Instance.currentPatternIndex == 0 || PatternManager.Instance.projectFileText.text == "Project 0" ) {
            return;
        }
        // Rotate and return the cell
        cell.RotateAndReturn();
    }
}
