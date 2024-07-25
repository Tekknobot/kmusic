using UnityEngine;

public class KeyDownHandler : MonoBehaviour
{
    private Key key;
    private KeyManager keyManager;

    private void Start()
    {
        key = GetComponent<Key>();
        if (key == null)
        {
            Debug.LogError("Key component not found on GameObject.");
            enabled = false; // Disable the script if Key component is not found
            return;
        }

        // Get the KeyManager instance
        keyManager = KeyManager.Instance;
        if (keyManager == null)
        {
            Debug.LogError("KeyManager instance not found.");
            enabled = false; // Disable the script if KeyManager instance is not found
        }
    }

    private void OnMouseDown()
    {
        if (key != null && keyManager != null)
        {
            // Perform actions when the key is clicked
            HandleKeyClick();
        }
    }

    private void HandleKeyClick()
    {
        // Perform rotation and return
        key.RotateAndReturn();

        // Inform the KeyManager about the key click
        keyManager.OnKeyClicked(gameObject);

        // Optionally, highlight the key if needed
        HighlightKey();
    }

    private void HighlightKey()
    {
        SpriteRenderer spriteRenderer = key.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Change sprite color or perform other highlighting actions
            spriteRenderer.color = Color.yellow; // Example: highlight with yellow color
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found on key.");
        }
    }
}
