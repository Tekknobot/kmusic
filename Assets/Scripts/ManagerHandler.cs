using UnityEngine;

public class ManagerHandler : MonoBehaviour
{
    public static ManagerHandler Instance;

    private bool isKeyManagerLastClicked;
    private Sprite lastClickedSprite;
    private int lastClickedMidiNote;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to set the last clicked manager
    public void SetLastClickedManager(bool isKeyManager)
    {
        isKeyManagerLastClicked = isKeyManager;

        if (isKeyManager)
        {
            lastClickedSprite = KeyManager.Instance.GetCurrentSprite();
            lastClickedMidiNote = KeyManager.Instance.midiNote;
        }
        else
        {
            lastClickedSprite = PadManager.Instance.GetCurrentSprite();
            lastClickedMidiNote = PadManager.Instance.midiNote;
        }
    }

    // Method to check if the last clicked manager was KeyManager
    public bool IsKeyManagerLastClicked()
    {
        return isKeyManagerLastClicked;
    }

    // Methods to get the last clicked sprite and MIDI note
    public Sprite GetLastClickedSprite()
    {
        return lastClickedSprite;
    }

    public int GetLastClickedMidiNote()
    {
        return lastClickedMidiNote;
    }
}

