using UnityEngine;

public class RainbowEffect : MonoBehaviour
{
    public float speed = 1.0f; // Speed of the color transition
    public float amplitude = 0.4f; // Amplitude of the color transition

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

        // Initialize time with a random value between 0 and 2π
        time = Random.Range(0f, 2 * Mathf.PI);
    }

    void Update()
    {
        if (spriteRenderer != null)
        {
            // Update the time variable
            time += Time.deltaTime * speed;

            // Generate a softened rainbow color
            Color color = SoftRainbowColor(time);

            // Apply the color to the SpriteRenderer component
            spriteRenderer.color = color;
        }
    }

    Color SoftRainbowColor(float t)
    {
        // Converts t into a color that transitions through a softer rainbow spectrum
        float r = Mathf.Sin(t * 0.5f) * amplitude + 0.5f;
        float g = Mathf.Sin(t * 0.5f + 2.0f) * amplitude + 0.5f;
        float b = Mathf.Sin(t * 0.5f + 4.0f) * amplitude + 0.5f;

        return new Color(r, g, b);
    }
}
