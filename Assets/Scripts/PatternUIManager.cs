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
    public Button saveNewProjectButton; // New button for saving the project
    public Button loadProjectButton;    // New button for loading a project
    public Button clearPatternsButton;  // New button for clearing patterns
    public Button saveOverButton;
    public Button delete;
    public TextMeshProUGUI patternDisplayText;
    public PatternManager patternManager;

    void Start()
    {
        if (createPatternButton != null) createPatternButton.onClick.AddListener(CreatePattern);
        if (playPatternsButton != null) playPatternsButton.onClick.AddListener(StartPatterns);
        if (stopPatternsButton != null) stopPatternsButton.onClick.AddListener(StopPatterns);
        if (removePatternButton != null) removePatternButton.onClick.AddListener(RemoveLastPattern);
        if (saveNewProjectButton != null) saveNewProjectButton.onClick.AddListener(SaveNewProject); // Register new button
        if (loadProjectButton != null) loadProjectButton.onClick.AddListener(LoadProject);           // Register new button
        if (clearPatternsButton != null) clearPatternsButton.onClick.AddListener(ClearPatterns);    // Register new button
        if (saveOverButton != null) saveOverButton.onClick.AddListener(SaveOver);    // Register new button
        if (delete != null) delete.onClick.AddListener(DeleteCurrentProject);    // Register new button
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

    void SaveNewProject()
    {
        patternManager.CreateAndLoadNewProject(); // Call the method to save project
        UpdatePatternDisplay();
    }

    void LoadProject()
    {
        if (PatternManager.Instance.isPlaying) {
            return;
        }
        patternManager.LoadNextProject(); // Call the method to load project
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
        patternManager.DeleteCurrentProject(); // Clear all patterns
        UpdatePatternDisplay();
    }

    public void UpdatePatternDisplay()
    {
        int totalPatterns = PatternManager.Instance.sequencersLength / 16;
        int currentPatternIndex = patternManager.CurrentPatternIndex + 1; // Display index should be 1-based

        patternDisplayText.text = $"{currentPatternIndex}/{totalPatterns}";
    }
}
