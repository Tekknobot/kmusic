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

        // Optionally, add to history or perform other actions
        TileData data = new TileData(sprite, midiNote);
        padManager.AddTileData(data);
    }
}
