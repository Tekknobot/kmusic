using UnityEngine;

public class EffectTrigger : MonoBehaviour
{
    public Material cameraEffectMaterial; // The material using the custom shader
    private int currentEffect = 0;        // Default to no effect

    void Start()
    {
        // Ensure the effect type is set to None at the start
        cameraEffectMaterial.SetFloat("_EffectType", currentEffect);
    }

    public void TriggerEffect(int effectType)
    {
        // Set the effect type on the material
        currentEffect = effectType;
        cameraEffectMaterial.SetFloat("_EffectType", currentEffect);
    }
}
