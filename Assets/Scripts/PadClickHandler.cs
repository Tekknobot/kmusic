using UnityEngine;

public class PadClickHandler : MonoBehaviour
{
    private PadManager padManager; // Reference to the PadManager
    private GameObject padObject; // Reference to the pad GameObject
    private Sprite sprite; // Sprite associated with the pad
    private int midiNote; // MIDI note associated with the pad

    // Method to initialize the click handler with necessary data
    public void Initialize(PadManager manager, GameObject padObj, Sprite padSprite, int note)
    {
        padManager = manager;
        padObject = padObj;
        sprite = padSprite;
        midiNote = note;

        Debug.Log($"PadClickHandler initialized with padManager: {padManager}, padObject: {padObject}, sprite: {sprite}, midiNote: {midiNote}");
    }

    // Method called when the pad is clicked
    private void OnMouseDown()
    {
        if (padManager != null && padObject != null)
        {
            padManager.OnPadClicked(padObject);
        }
        else
        {
            Debug.LogError("PadManager or padObject is null in PadClickHandler.");
        }
    }

    // Property to get the MIDI note associated with the pad
    public int MidiNote
    {
        get { return midiNote; }
    }
}
