using UnityEngine;

public class SampleClickHandler : MonoBehaviour
{
    private SampleManager sampleManager;
    private GameObject sampleObject;
    private Sprite sampleSprite;
    public int midiNote;

    public void Initialize(SampleManager manager, GameObject obj, Sprite sprite, int note)
    {
        sampleManager = manager;
        sampleObject = obj;
        this.sampleSprite = sprite;
        midiNote = note;
    }

    private void OnMouseDown()
    {
        sampleManager.OnSampleClicked(sampleObject);
        ManagerHandler.Instance.SetLastClickedManager(false, false, true); 
    }
}
