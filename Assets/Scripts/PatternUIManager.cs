using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AudioHelm;

public class PatternUIManager : MonoBehaviour
{
    public Button createPatternButton;
    public Button playPatternsButton;
    public Button stopPatternsButton;
    public Button removePatternButton;

    public Button loadProjectButton;    // New button for loading a project
    public Button clearPatternsButton;  // New button for clearing patterns
    public Button saveOverButton;
    public Button delete;
    public TextMeshProUGUI patternDisplayText;
    public PatternManager patternManager;

    public TMP_InputField projectNameInputField; // UI Input Field for user input
    public Button createProjectButton; // Button to create a new project
    public Button goButton;
       
    void Start()
    {
        if (createPatternButton != null) createPatternButton.onClick.AddListener(CreatePattern);
        if (playPatternsButton != null) playPatternsButton.onClick.AddListener(StartPatterns);
        if (stopPatternsButton != null) stopPatternsButton.onClick.AddListener(StopPatterns);
        if (removePatternButton != null) removePatternButton.onClick.AddListener(RemoveLastPattern);
        if (loadProjectButton != null) loadProjectButton.onClick.AddListener(LoadProject);           // Register new button
        if (clearPatternsButton != null) clearPatternsButton.onClick.AddListener(ClearPatterns);    // Register new button
        if (saveOverButton != null) saveOverButton.onClick.AddListener(SaveOver);    // Register new button
        if (delete != null) delete.onClick.AddListener(DeleteCurrentProject);    // Register new button

        if (createProjectButton != null) createProjectButton.onClick.AddListener(OnCreateProjectButtonClicked);
        if (goButton != null) goButton.onClick.AddListener(GoToProject);
    }

    void CreatePattern()
    {
        patternManager.CreatePattern();
        UpdatePatternDisplay();
    }

    void StartPatterns()
    {
        patternManager.StartPatterns();
        UpdatePatternDisplay();
    }

    void StopPatterns()
    {
        patternManager.StopPatterns();
        UpdatePatternDisplay();
    }

    void RemoveLastPattern()
    {
        patternManager.RemovePattern();
        UpdatePatternDisplay();
    }

    void LoadProject()
    {
        if (PatternManager.Instance.isPlaying) {
            return;
        }
        patternManager.LoadNextProject(); // Call the method to load project
        
        projectNameInputField.text = string.Empty;
        projectNameInputField.gameObject.SetActive(false);
        projectNameInputField.enabled = false;

        UpdatePatternDisplay();
    }

    void ClearPatterns()
    {
        patternManager.ClearCurrentPattern(); // Clear all patterns
        UpdatePatternDisplay();
    }

    void SaveOver() {
        patternManager.SaveOver(); // Clear all patterns
        UpdatePatternDisplay();
    }

    void DeleteCurrentProject() {
        if (PatternManager.Instance.isPlaying) {
            return;
        }        
        patternManager.DeleteCurrentProject(); // Clear all patterns
        UpdatePatternDisplay();
    }

    public void UpdatePatternDisplay()
    {
        int totalPatterns = PatternManager.Instance.sequencersLength / 16;
        int currentPatternIndex = PatternManager.Instance.currentPatternIndex; // Display index should be 1-based
        patternDisplayText.text = $"{currentPatternIndex}/{totalPatterns}";
    }

    private void OnCreateProjectButtonClicked()
    {
        if (PatternManager.Instance.isPlaying) {
            return;
        }       

        projectNameInputField.gameObject.SetActive(true);
        projectNameInputField.enabled = true;
    }    

    private void GoToProject() {
        string customName = projectNameInputField.text;

        // Ensure the custom name is not empty
        if (string.IsNullOrWhiteSpace(customName))
        {
            //statusText.text = "Project name cannot be empty!";
            return;
        }

        // Call method to create and load new project
        FindObjectOfType<PatternManager>().CreateAndLoadNewProject(customName);

        UpdatePatternDisplay();    

        projectNameInputField.text = string.Empty;
        projectNameInputField.gameObject.SetActive(false);
    }
}
