using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PatternUIManager : MonoBehaviour
{
    public Button createPatternButton;
    public Button playPatternsButton;
    public Button stopPatternsButton;
    public Button removePatternButton;
    public TextMeshProUGUI patternDisplayText;
    public PatternManager patternManager;

    void Start()
    {
        if (createPatternButton != null) createPatternButton.onClick.AddListener(CreatePattern);
        if (playPatternsButton != null) playPatternsButton.onClick.AddListener(StartPatterns);
        if (stopPatternsButton != null) stopPatternsButton.onClick.AddListener(StopPatterns);
        if (removePatternButton != null) removePatternButton.onClick.AddListener(RemoveLastPattern);
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

    void UpdatePatternDisplay()
    {
        int totalPatterns = patternManager.PatternsCount;
        int currentPatternIndex = patternManager.CurrentPatternIndex + 1; // Display index should be 1-based

        patternDisplayText.text = $"{currentPatternIndex}/{totalPatterns}";
    }
}
