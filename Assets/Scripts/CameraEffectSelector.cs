using UnityEngine;

[ExecuteInEditMode]
public class CameraEffectSelector : MonoBehaviour
{
    public Material effectMaterial;

    public enum EffectType
    {
        None,
        Effect_A,
        Effect_B,
        Effect_C,
        Effect_D
    }

    public EffectType selectedEffect = EffectType.None;

    private const string PlayerPrefKey = "SelectedEffect";

    void Start()
    {
        // Load the saved effect from PlayerPrefs
        if (PlayerPrefs.HasKey(PlayerPrefKey))
        {
            int savedEffect = PlayerPrefs.GetInt(PlayerPrefKey);
            if (System.Enum.IsDefined(typeof(EffectType), savedEffect))
            {
                selectedEffect = (EffectType)savedEffect;
            }
        }
    }

    void OnDisable()
    {
        // Save the selected effect to PlayerPrefs
        PlayerPrefs.SetInt(PlayerPrefKey, (int)selectedEffect);
        PlayerPrefs.Save();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (effectMaterial != null)
        {
            // Pass the selected effect to the shader
            effectMaterial.SetInt("_EffectType", (int)selectedEffect);

            // Apply the shader
            Graphics.Blit(source, destination, effectMaterial);
        }
        else
        {
            // Default behavior (no effect)
            Graphics.Blit(source, destination);
        }
    }

    // Optional: Method to update the selected effect and save it immediately
    public void UpdateSelectedEffect(EffectType newEffect)
    {
        selectedEffect = newEffect;
        PlayerPrefs.SetInt(PlayerPrefKey, (int)selectedEffect);
        PlayerPrefs.Save();
    }
}
