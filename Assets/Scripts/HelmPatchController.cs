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
    public Slider[] parameterSliders; // Array of sliders for Helm parameters
    public AudioHelm.Param[] helmParams; // Array of Helm parameters

    public int currentPatchIndex;
    private const string PLAYER_PREFS_PREFIX = "HelmParameter_"; // Prefix for PlayerPrefs keys

    private void Awake()
    {
        // Reverse the order of patches
        //System.Array.Reverse(patches);

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

        // Add listeners to the parameter sliders
        for (int i = 0; i < parameterSliders.Length; i++)
        {
            int index = i; // Capture the index for use in the lambda
            parameterSliders[i].onValueChanged.AddListener((value) => OnParameterSliderValueChanged(index, value));
        }

        // Load the saved parameter values
        LoadParameterValues();
    }

    void Start()
    {
        // Load the current patch based on the saved index
        LoadCurrentPatch();
        SyncSlidersWithParameters();       
    }

    public void LoadCurrentPatch()
    {
        // Check if helmController and patches array are properly assigned
        if (helmController != null && patches != null && patches.Length > 0)
        {
            // Load the patch based on the current index
            helmController.LoadPatch(patches[currentPatchIndex]);

            // Update the patch name label
            UpdatePatchNameLabel();

            // Sync the parameter sliders with HelmController parameter values
            SyncSlidersWithParameters();

            // Start coroutine to nudge parameters after a short delay
            StartCoroutine(DelayedNudgeParameters(0.1f)); // 0.1-second delay
        }
        else
        {
            Debug.LogError("HelmController or patches array is not assigned properly.");
        }
    }

    private IEnumerator DelayedNudgeParameters(float delay)
    {
        yield return new WaitForSeconds(delay);

        NudgeParameters();
    }


    private void NudgeParameters()
    {
        if (helmController != null && helmParams != null && parameterSliders != null)
        {
            for (int i = 0; i < helmParams.Length; i++)
            {
                // Get the current parameter value from the slider
                float currentValue = parameterSliders[i].value;

                // Force HelmController to reapply the value
                helmController.SetParameterValue(helmParams[i], currentValue);
            }

            Debug.Log("All parameters nudged to refresh Helm Synth state.");
        }
        else
        {
            Debug.LogError("HelmController, helmParams, or parameterSliders is not properly assigned.");
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

        // Sync the parameter sliders with the new patch parameters
        SyncSlidersWithParameters();
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

        // Sync the parameter sliders with the new patch parameters
        SyncSlidersWithParameters();
    }

    private void UpdatePatchNameLabel()
    {
        // Update the label with the current patch name
        if (patchNameLabel != null)
        {
            patchNameLabel.text = patches[currentPatchIndex].name;
        }
    }

    private void OnParameterSliderValueChanged(int index, float value)
    {
        // Set the parameter value in HelmController
        if (helmController != null)
        {
            SetParameter(index, value);
            // Save the parameter value
            SaveParameterValue(index, value);
        }
        else
        {
            Debug.LogError("HelmController is not assigned.");
        }
    }

    private void SaveParameterValue(int index, float value)
    {
        PlayerPrefs.SetFloat(PLAYER_PREFS_PREFIX + index, value);
        PlayerPrefs.Save();
    }

    private void LoadParameterValues()
    {
        if (helmController != null)
        {
            for (int i = 0; i < parameterSliders.Length; i++)
            {
                string key = PLAYER_PREFS_PREFIX + i;
                if (PlayerPrefs.HasKey(key))
                {
                    float savedValue = PlayerPrefs.GetFloat(key);
                    parameterSliders[i].value = savedValue;
                    SetParameter(i, savedValue); // Update HelmController
                }
            }
        }
        else
        {
            Debug.LogError("HelmController is not assigned.");
        }
    }

    // Method to set parameter value in HelmController
    private void SetParameter(int index, float value)
    {
        // Assuming HelmController has a method to set parameters directly
        // You need to adjust this method based on the actual method signature
        helmController.SetParameterValue(helmParams[index], value);
    }

    // Sync the sliders with the HelmController parameter values
    private void SyncSlidersWithParameters()
    {
        if (helmController != null)
        {
            for (int i = 0; i < parameterSliders.Length; i++)
            {
                parameterSliders[i].value = helmController.GetParameterValue(helmParams[i]);
            }
        }
        else
        {
            Debug.LogError("HelmController is not assigned.");
        }
    }

    public int GetCurrentPatchIndex()
    {
        return currentPatchIndex;
    }  

    public ProjectData SaveCurrentState()
    {
        ProjectData projectData = new ProjectData();

        // Save the current patch index
        projectData.patch = currentPatchIndex;

        // Save slider values
        projectData.sliderValues = GetAllSliderValues();

        // Save other properties as needed
        // For example, BPM, sequencer lengths, etc., would come from other controllers.

        return projectData;
    }
    public List<float> GetAllSliderValues()
    {
        List<float> values = new List<float>();

        // Iterate through all sliders and store their current values
        foreach (var slider in parameterSliders)
        {
            values.Add(slider.value);
        }

        return values;
    }

    public void LoadFromProjectData(ProjectData projectData)
    {
        if (projectData == null)
        {
            Debug.LogError("Project data is null.");
            return;
        }

        // Load the patch index and set the patch
        if (patches != null && projectData.patch >= 0 && projectData.patch < patches.Length)
        {
            currentPatchIndex = projectData.patch;
            LoadCurrentPatch(); // Load the patch based on the index
        }
        else
        {
            Debug.LogWarning("Invalid patch index in project data.");
        }

        // Load slider values and update Helm parameters
        SetAllSliderValues(projectData.sliderValues);

        // Nudge all parameters to ensure Helm Synth refreshes
        NudgeParameters();
    }


    public void SetAllSliderValues(List<float> values)
    {
        // Validate input
        if (values == null || values.Count != parameterSliders.Length)
        {
            Debug.LogError("Invalid slider values list provided.");
            return;
        }

        // Update each slider and synchronize the parameter in HelmController
        for (int i = 0; i < parameterSliders.Length; i++)
        {
            parameterSliders[i].value = values[i];  // Update slider
            SetParameter(i, values[i]);            // Sync HelmController parameter
        }
    }

}
