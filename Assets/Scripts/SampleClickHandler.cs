using UnityEngine;

public class SampleClickHandler : MonoBehaviour
{
    private SampleManager sampleManager;
    private GameObject sampleObject;
    private Sprite sampleSprite;

    public void Initialize(SampleManager manager, GameObject obj, Sprite sprite)
    {
        sampleManager = manager;
        sampleObject = obj;
        sampleSprite = sprite;
    }

    private void OnMouseDown()
    {
        sampleManager.OnSampleClicked(sampleObject);
    }
}
