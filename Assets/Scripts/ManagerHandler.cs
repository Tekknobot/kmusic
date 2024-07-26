using UnityEngine;

public class ManagerHandler : MonoBehaviour
{
    public static ManagerHandler Instance { get; private set; }

    public KeyManager keyManager;
    public PadManager padManager;

    private bool isKeyManagerLastClicked;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: if you want this to persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }

        keyManager = KeyManager.Instance.GetComponent<KeyManager>();
        padManager = PadManager.Instance.GetComponent<PadManager>();

        if (keyManager == null || padManager == null)
        {
            Debug.LogError("KeyManager or PadManager instance not found.");
        }
    }

    public void SetLastClickedManager(bool isKeyManager)
    {
        isKeyManagerLastClicked = isKeyManager;
    }

    public Sprite GetLastClickedSprite()
    {
        return isKeyManagerLastClicked ? keyManager.GetCurrentSprite() : padManager.GetCurrentSprite();
    }

    public int GetLastClickedMidiNote()
    {
        return isKeyManagerLastClicked ? keyManager.midiNote : padManager.midiNote;
    }

    public bool IsKeyManagerLastClicked()
    {
        return isKeyManagerLastClicked;
    }
}
