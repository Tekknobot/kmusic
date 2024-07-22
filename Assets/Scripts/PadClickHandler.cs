using UnityEngine;

public class PadClickHandler : MonoBehaviour
{
    private PadManager padManager;
    private GameObject padObject;
    private Sprite sprite;
    private int midiNote;

    public void Initialize(PadManager manager, GameObject pad, Sprite sprite, int note)
    {
        padManager = manager;
        padObject = pad;
        this.sprite = sprite;
        midiNote = note;
    }

    private void OnMouseDown()
    {
        padManager.OnPadClicked(padObject);
    }
}