using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// KeyClickHandler script to handle key press and release events
public class KeyClickHandler : MonoBehaviour
{
    private KeyManager keyManager;
    private GameObject keyObject;
    private Sprite keySprite;
    public int midiNote;

    public void Initialize(KeyManager manager, GameObject obj, Sprite sprite, int note)
    {
        keyManager = manager;
        keyObject = obj;
        keySprite = sprite;
        midiNote = note;
    }

    private void OnMouseDown()
    {
        keyManager.OnKeyClicked(keyObject);
        keyManager.OnKeyPressDown(keyObject);
    }

    private void OnMouseUp()
    {
        keyManager.OnKeyRelease(keyObject);
        keyManager.OnManagerClicked();
    }
}