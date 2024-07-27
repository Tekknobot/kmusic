using UnityEngine;

public class ManagerHandler : MonoBehaviour
{
    public static ManagerHandler Instance { get; private set; }

    public KeyManager keyManager;
    public PadManager padManager;

    private bool isKeyManagerLastClicked;

    private void Start()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: if you want this to persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ensure KeyManager and PadManager instances are assigned
        if (keyManager == null)
        {
            keyManager = KeyManager.Instance;
            if (keyManager == null)
            {
                Debug.LogError("KeyManager instance is not assigned or found.");
            }
        }

        if (padManager == null)
        {
            padManager = PadManager.Instance;
            if (padManager == null)
            {
                Debug.LogError("PadManager instance is not assigned or found.");
            }
        }
    }

    public void SetLastClickedManager(bool isKeyManager)
    {
        isKeyManagerLastClicked = isKeyManager;
    }

    public Sprite GetLastClickedSprite()
    {
        if (isKeyManagerLastClicked)
        {
            return keyManager != null ? keyManager.GetCurrentSprite() : null;
        }
        else
        {
            return padManager != null ? padManager.GetCurrentSprite() : null;
        }
    }

    public int GetLastClickedMidiNote()
    {
        if (isKeyManagerLastClicked)
        {
            return keyManager != null ? keyManager.midiNote : 0;
        }
        else
        {
            return padManager != null ? padManager.midiNote : 0;
        }
    }
    public bool IsKeyManagerLastClicked()
    {
        return isKeyManagerLastClicked;
    }  
}
