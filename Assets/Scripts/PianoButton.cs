using UnityEngine;
using UnityEngine.UI;

public class PianoButton : MonoBehaviour
{
    public GameObject mixerGroupObject; // Reference to the MixerGroup GameObject
    public GameObject keyManagerObject; // Reference to the KeyManager GameObject
    public Toggle toggle; // Reference to the Toggle UI element

    private void Start()
    {
        // Initialize the toggle and set up the listener
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggleChanged);
            // Set initial visibility based on toggle state
            OnToggleChanged(toggle.isOn);
        }
        else
        {
            Debug.LogError("Toggle is not assigned in ToggleManager.");
        }
    }

    private void OnToggleChanged(bool isOn)
    {
        // Toggle visibility based on the toggle's state
        if (mixerGroupObject != null)
        {
            mixerGroupObject.SetActive(!isOn); // Hide MixerGroup when toggle is on
        }
        else
        {
            Debug.LogError("MixerGroupObject is not assigned in ToggleManager.");
        }

        if (keyManagerObject != null)
        {
            keyManagerObject.SetActive(isOn); // Show KeyManager when toggle is on
        }
        else
        {
            Debug.LogError("KeyManagerObject is not assigned in ToggleManager.");
        }
    }
}