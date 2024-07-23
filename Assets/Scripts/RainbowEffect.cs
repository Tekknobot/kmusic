using UnityEngine;

public class RainbowEffect : MonoBehaviour
{
    public float speed = 1.0f; // Speed of the color transition
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private float time; // Time variable for animation

    void Start()
    {
        // Get the SpriteRenderer component attached to this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("RainbowEffect script requires a SpriteRenderer component.");
        }
    }

    void Update()
    {
        if (spriteRenderer != null)
        {
            // Update the time variable
            time += Time.deltaTime * speed;

            // Generate a rainbow color
            Color color = RainbowColor(time);

            // Apply the color to the SpriteRenderer component
            spriteRenderer.color = color;
        }
    }

    Color RainbowColor(float t)
    {
        // Converts t into a color that transitions through the rainbow spectrum
        float r = Mathf.Sin(t + 0.0f) * 0.5f + 0.5f;
        float g = Mathf.Sin(t + 2.0f) * 0.5f + 0.5f;
        float b = Mathf.Sin(t + 4.0f) * 0.5f + 0.5f;
        return new Color(r, g, b);
    }
}
