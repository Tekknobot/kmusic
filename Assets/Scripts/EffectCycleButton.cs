using UnityEngine;

public class EffectCycleButton : MonoBehaviour
{
    public CameraEffectSelector cameraEffectSelector; // Reference to the CameraEffectSelector script
    private int totalEffects; // Number of effects
    private int currentEffect = 0; // Tracks the current effect

    void Start()
    {
        if (cameraEffectSelector == null)
        {
            Debug.LogError("CameraEffectSelector is not assigned to the EffectCycleButton.");
            return;
        }

        // Initialize totalEffects from the enum length
        totalEffects = System.Enum.GetNames(typeof(CameraEffectSelector.EffectType)).Length;

        // Set the initial effect
        UpdateEffect();
    }

    public void OnMouseDown()
    {
        // Cycle through the effects
        CycleEffect();
    }

    public void CycleEffect()
    {
        currentEffect = (currentEffect + 1) % totalEffects;
        UpdateEffect();
    }

    private void UpdateEffect()
    {
        if (cameraEffectSelector != null)
        {
            // Update the selectedEffect in the CameraEffectSelector
            cameraEffectSelector.selectedEffect = (CameraEffectSelector.EffectType)currentEffect;
            Debug.Log($"Effect set to: {cameraEffectSelector.selectedEffect}");
        }
    }
}
