using UnityEngine;
using System.Collections.Generic;

public class MySampleSequencer : MonoBehaviour
{
    public GameObject notePrefab; // Prefab for visualizing notes
    public Transform notesParent; // Parent transform for instantiated notes
    public int totalSteps = 16; // Total number of steps in the sequencer
    private Dictionary<int, List<NoteDisplay>> notesDisplay = new Dictionary<int, List<NoteDisplay>>();

    private void Start()
    {
        // Initialize the display
        InitializeDisplay();
    }

    private void InitializeDisplay()
    {
        // Set up the display grid based on the total number of steps
        for (int step = 0; step < totalSteps; step++)
        {
            notesDisplay[step] = new List<NoteDisplay>();
        }

        UpdateSequencerDisplay();
    }

    public void AddNote(int midiNote, int startStep, int endStep, float duration)
    {
        // Add the note to the internal data structure
        // Assume there's a way to store notes in a more complex structure
        // For simplicity, just updating display
        NoteDisplay noteDisplay = Instantiate(notePrefab, notesParent).GetComponent<NoteDisplay>();
        noteDisplay.Setup(midiNote, startStep, endStep, duration);
        notesDisplay[startStep].Add(noteDisplay);

        // Update display
        UpdateSequencerDisplay();
    }

    public void RemoveNotesInRange(int midiNote, int startStep, int endStep)
    {
        // Remove notes from the internal data structure
        for (int step = startStep; step <= endStep; step++)
        {
            if (notesDisplay.ContainsKey(step))
            {
                foreach (var noteDisplay in notesDisplay[step])
                {
                    if (noteDisplay.MidiNote == midiNote)
                    {
                        Destroy(noteDisplay.gameObject);
                    }
                }

                // Remove empty entries
                notesDisplay[step].RemoveAll(note => note.MidiNote == midiNote);
            }
        }

        // Update display
        UpdateSequencerDisplay();
    }

    public void UpdateSequencerDisplay()
    {
        // Clear existing visual notes
        foreach (Transform child in notesParent)
        {
            Destroy(child.gameObject);
        }

        // Reinstantiate notes based on the internal data structure
        foreach (var stepNotes in notesDisplay)
        {
            int step = stepNotes.Key;
            foreach (var noteDisplay in stepNotes.Value)
            {
                NoteDisplay noteInstance = Instantiate(notePrefab, notesParent).GetComponent<NoteDisplay>();
                noteInstance.Setup(noteDisplay.MidiNote, step, noteDisplay.EndStep, noteDisplay.Duration);
            }
        }

        Debug.Log("Sequencer display updated.");
    }
}

public class NoteDisplay : MonoBehaviour
{
    public int MidiNote { get; private set; }
    public int EndStep { get; private set; }
    public float Duration { get; private set; }

    public void Setup(int midiNote, int startStep, int endStep, float duration)
    {
        MidiNote = midiNote;
        EndStep = endStep;
        Duration = duration;
        
        // Set the position and size of the note display based on steps and duration
        // Adjust based on your visual representation needs
        // Example:
        transform.localPosition = new Vector3(startStep * 1.0f, 0, 0); // Position based on step
        transform.localScale = new Vector3(duration, 1, 1); // Scale based on duration
    }
}
