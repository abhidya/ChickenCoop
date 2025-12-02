using UnityEngine;

/// <summary>
/// Utility class to generate simple placeholder sprites for the game.
/// This provides basic colored sprites until proper art assets are added.
/// </summary>
public static class PlaceholderSpriteGenerator
{
    /// <summary>
    /// Create a simple colored square sprite
    /// </summary>
    public static Sprite CreateSquareSprite(Color color, int size = 32)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    /// <summary>
    /// Create a simple colored circle sprite
    /// </summary>
    public static Sprite CreateCircleSprite(Color color, int size = 32)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    pixels[y * size + x] = color;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    /// <summary>
    /// Create a simple egg-shaped sprite
    /// </summary>
    public static Sprite CreateEggSprite(Color color, int size = 32)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normalizedX = (x - center.x) / (size / 2f);
                float normalizedY = (y - center.y) / (size / 2f);
                
                // Egg shape formula
                float eggShape = (normalizedX * normalizedX) + (normalizedY * normalizedY * (1.0f + 0.3f * normalizedY));
                
                if (eggShape <= 1.0f)
                {
                    pixels[y * size + x] = color;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    /// <summary>
    /// Create a simple triangle sprite (for corn/vegetation)
    /// </summary>
    public static Sprite CreateTriangleSprite(Color color, int size = 32)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Triangle pointing up
                float normalizedX = (x - size / 2f) / (size / 2f);
                float normalizedY = (y) / (float)size;
                
                if (Mathf.Abs(normalizedX) <= (1.0f - normalizedY))
                {
                    pixels[y * size + x] = color;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
