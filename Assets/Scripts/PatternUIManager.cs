using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PatternUIManager : MonoBehaviour
{
    public Button createPatternButton;
    public Button playPatternsButton;
    public Button stopPatternsButton;
    public Button removePatternButton;
    public Button saveNewProjectButton; // New button for saving the project
    public Button loadProjectButton;    // New button for loading a project
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
        patternManager.RemoveLastPattern();
        UpdatePatternDisplay();
    }

    void SaveNewProject()
    {
        patternManager.SavePatterns(); // Call the method to save patterns with a unique filename
        UpdatePatternDisplay();
    }

    void LoadProject()
    {
        patternManager.LoadPatterns(); // Call the method to load patterns
        UpdatePatternDisplay();
    }

    public void UpdatePatternDisplay()
    {
        int totalPatterns = patternManager.PatternsCount;
        int currentPatternIndex = patternManager.CurrentPatternIndex + 1; // Display index should be 1-based

        patternDisplayText.text = $"{currentPatternIndex}/{totalPatterns}";
    }
}
