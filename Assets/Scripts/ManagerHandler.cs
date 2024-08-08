using UnityEngine;

public class ManagerHandler : MonoBehaviour
{
    public static ManagerHandler Instance { get; private set; }

    public KeyManager keyManager;
    public PadManager padManager;
    public SampleManager sampleManager;

    private bool isKeyManagerLastClicked;
    private bool isPadManagerLastClicked;
    private bool isSampleManagerLastClicked; // Track if SampleManager was last clicked

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

        if (padManager == null)
        {
            padManager = PadManager.Instance;
            if (padManager == null)
            {
                Debug.LogError("PadManager instance is not assigned or found.");
            }
        }

        if (sampleManager == null)
        {
            sampleManager = SampleManager.Instance;
            if (sampleManager == null)
            {
                Debug.LogError("SampleManager instance is not assigned or found.");
            }
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
    }

    public void SetLastClickedManager(bool isKeyManager, bool isPadManager, bool isSampleManager)
    {
        isKeyManagerLastClicked = isKeyManager;
        isPadManagerLastClicked = isPadManager;
        isSampleManagerLastClicked = isSampleManager;
    }

    public Sprite GetLastClickedSprite()
    {
        if (isKeyManagerLastClicked)
        {
            return keyManager != null ? keyManager.GetCurrentSprite() : null;
        }
        else if (isPadManagerLastClicked)
        {
            return padManager != null ? padManager.GetCurrentSprite() : null;
        }
        else if (isSampleManagerLastClicked)
        {
            return sampleManager != null ? sampleManager.GetCurrentSprite() : null;
        }
        return null;
    }

    public int GetLastClickedMidiNote()
    {
        if (isKeyManagerLastClicked)
        {
            return keyManager != null ? keyManager.midiNote : 0;
        }
        else if (isSampleManagerLastClicked)
        {
            return sampleManager != null ? sampleManager.midiNote : 0;
        }
        else if (isPadManagerLastClicked)
        {
            return padManager != null ? padManager.midiNote : 0;
        }
        return -1;
    }
    public bool IsKeyManagerLastClicked()
    {
        return isKeyManagerLastClicked;
    }

    public bool IsSampleManagerLastClicked()
    {
        return isSampleManagerLastClicked;
    }

    public bool IsPadManagerLastClicked()
    {
        return isPadManagerLastClicked;
    }
}
