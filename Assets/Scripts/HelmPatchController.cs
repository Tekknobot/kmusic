using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using AudioHelm;
using UnityEngine.UI;

public class HelmPatchController : MonoBehaviour
{
    public HelmController helmController; // Single HelmController
    public TextMeshProUGUI patchNameLabel;
    public Button nextButton;
    public Button prevButton;
    public HelmPatch[] patches; // Array of Helm patches

    private int currentPatchIndex;

    private void Awake()
    {
        // Reverse the order of patches
        System.Array.Reverse(patches);

        // Add listeners to the buttons
        nextButton.onClick.AddListener(NextPatch);
        prevButton.onClick.AddListener(PreviousPatch);

        // Load the patch index from PlayerPrefs if it exists
        if (PlayerPrefs.HasKey("PatchIndex"))
        {
            currentPatchIndex = PlayerPrefs.GetInt("PatchIndex");
        }
        else
        {
            currentPatchIndex = 0;
        }
    }

    void Start() {
        // Load the current patch based on the saved index
        LoadCurrentPatch();
    }

    private void LoadCurrentPatch()
    {
        // Check if helmController and patches array are properly assigned
        if (helmController != null && patches != null && patches.Length > 0)
        {
            // Load the patch based on the current index
            helmController.LoadPatch(patches[currentPatchIndex]);
            // Update the patch name label
            UpdatePatchNameLabel();
        }
        else
        {
            Debug.LogError("HelmController or patches array is not assigned properly.");
        }
    }

    private void NextPatch()
    {
        // Increment the patch index and wrap around if necessary
        currentPatchIndex++;
        if (currentPatchIndex >= patches.Length)
        {
            currentPatchIndex = 0;
        }

        // Load the new patch and save the index
        LoadCurrentPatch();
        PlayerPrefs.SetInt("PatchIndex", currentPatchIndex);
    }

    private void PreviousPatch()
    {
        // Decrement the patch index and wrap around if necessary
        currentPatchIndex--;
        if (currentPatchIndex < 0)
        {
            currentPatchIndex = patches.Length - 1;
        }

        // Load the new patch and save the index
        LoadCurrentPatch();
        PlayerPrefs.SetInt("PatchIndex", currentPatchIndex);
    }

    private void UpdatePatchNameLabel()
    {
        // Update the label with the current patch name
        if (patchNameLabel != null)
        {
            patchNameLabel.text = patches[currentPatchIndex].name;
        }
    }
}
