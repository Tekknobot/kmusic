using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SynthButtonUIManager : MonoBehaviour
{
    public RectTransform[] objectsToMove; // Array of UI objects to move
    public Button[] moveButtons;          // Array of buttons that will move the objects

    // Target positions for the objects
    private Vector2 targetPositionActive = new Vector2(-25, 438.2639f);
    private Vector2 targetPositionInactive = new Vector2(-25, 3000);

    void Start()
    {
        // Ensure we have the correct number of buttons and objects
        if (objectsToMove.Length != 4 || moveButtons.Length != 4)
        {
            Debug.LogError("You need exactly 4 objects and 4 buttons assigned.");
            return;
        }

        // Initialize all objects to the inactive position
        for (int i = 0; i < objectsToMove.Length; i++)
        {
            if (i == 0)
            {
                objectsToMove[i].anchoredPosition = targetPositionActive; // Element 0 starts at the active position
            }
            else
            {
                objectsToMove[i].anchoredPosition = targetPositionInactive; // Other elements start at the inactive position
            }
        }

        // Assign button click events
        moveButtons[0].onClick.AddListener(() => MoveObject(0));
        moveButtons[1].onClick.AddListener(() => MoveObject(1));
        moveButtons[2].onClick.AddListener(() => MoveObject(2));
        moveButtons[3].onClick.AddListener(() => MoveObject(3));
    }

    // Method to move the clicked object and reset others
    void MoveObject(int index)
    {
        if (index >= 0 && index < objectsToMove.Length)
        {
            // Move the selected object to the active position
            objectsToMove[index].anchoredPosition = targetPositionActive;

            // Move the rest of the objects to the inactive position
            for (int i = 0; i < objectsToMove.Length; i++)
            {
                if (i != index)
                {
                    objectsToMove[i].anchoredPosition = targetPositionInactive;
                }
            }
        }
        else
        {
            Debug.LogError("Invalid object index: " + index);
        }
    }
}
