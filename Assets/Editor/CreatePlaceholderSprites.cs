using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to create simple placeholder sprites for rapid prototyping.
/// Adds menu item: Tools/Create Placeholder Sprites
/// Creates colored circles representing different game entities.
/// </summary>
public class CreatePlaceholderSprites : EditorWindow
{
    [MenuItem("Tools/Create Placeholder Sprites")]
    static void ShowWindow()
    {
        GetWindow(typeof(CreatePlaceholderSprites), false, "Create Placeholders");
    }

    void OnGUI()
    {
        GUILayout.Label("Create Placeholder Sprites", EditorStyles.boldLabel);
        
        GUILayout.Space(10);
        GUILayout.Label("This will create simple colored circle sprites\nfor rapid prototyping.", EditorStyles.wordWrappedLabel);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create All Placeholder Sprites"))
        {
            CreateAllPlaceholders();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Player Only"))
        {
            CreateSprite("Player/player_idle", 64, new Color(0.5f, 1f, 0.5f), "Light Green Circle");
        }
        
        if (GUILayout.Button("Create Chicken Only"))
        {
            CreateSprite("Chicken/chicken_idle", 48, new Color(0.3f, 0.6f, 1f), "Blue Circle");
        }
        
        if (GUILayout.Button("Create Egg Only"))
        {
            CreateSprite("Egg/egg", 32, new Color(1f, 0.98f, 0.9f), "Off-White Circle");
        }
    }

    static void CreateAllPlaceholders()
    {
        if (!EditorUtility.DisplayDialog(
            "Create Placeholder Sprites",
            "This will create simple colored circles in Assets/Sprites/.\n\nThese are temporary placeholders until proper art is created.\n\nContinue?",
            "Yes, Create",
            "Cancel"))
        {
            return;
        }

        int created = 0;

        // Player sprites
        created += CreateSprite("Player/player_idle", 64, new Color(0.5f, 1f, 0.5f), "Light Green Circle") ? 1 : 0;
        created += CreateSprite("Player/player_run_0", 64, new Color(0.5f, 1f, 0.5f), "Light Green Circle") ? 1 : 0;
        created += CreateSprite("Player/player_run_1", 64, new Color(0.5f, 1f, 0.5f), "Light Green Circle") ? 1 : 0;

        // Chicken sprites
        created += CreateSprite("Chicken/chicken_idle", 48, new Color(0.3f, 0.6f, 1f), "Blue Circle") ? 1 : 0;
        created += CreateSprite("Chicken/chicken_walk_0", 48, new Color(0.3f, 0.6f, 1f), "Blue Circle") ? 1 : 0;
        created += CreateSprite("Chicken/chicken_walk_1", 48, new Color(0.3f, 0.6f, 1f), "Blue Circle") ? 1 : 0;

        // Egg sprite
        created += CreateSprite("Egg/egg", 32, new Color(1f, 0.98f, 0.9f), "Off-White Circle") ? 1 : 0;

        // Environment sprites
        created += CreateSprite("Environment/corn_stalk", 32, new Color(1f, 0.84f, 0.34f), "Yellow Circle") ? 1 : 0;
        created += CreateSprite("Environment/platform", 512, new Color(0.34f, 0.54f, 0.18f), "Dark Green Oval") ? 1 : 0;
        created += CreateSprite("Environment/corn_field_tile", 64, new Color(0.43f, 0.26f, 0.16f), "Brown Square") ? 1 : 0;

        // UI icons
        created += CreateSprite("UI/coin_icon", 32, new Color(1f, 0.84f, 0f), "Gold Circle") ? 1 : 0;
        created += CreateSprite("UI/corn_icon", 32, new Color(1f, 0.84f, 0.34f), "Yellow Circle") ? 1 : 0;
        created += CreateSprite("UI/egg_icon", 32, new Color(1f, 0.98f, 0.9f), "Off-White Circle") ? 1 : 0;

        AssetDatabase.Refresh();
        
        Debug.Log($"<color=green>✓</color> Created {created} placeholder sprites in Assets/Sprites/");
        EditorUtility.DisplayDialog(
            "Complete",
            $"Created {created} placeholder sprites.\n\nThese are simple colored circles for prototyping.\n\nReplace with proper art when available.",
            "OK");
    }

    static bool CreateSprite(string name, int size, Color color, string description)
    {
        string path = $"Assets/Sprites/{name}.png";
        
        // Check if already exists
        if (File.Exists(path))
        {
            Debug.LogWarning($"Sprite already exists: {path}");
            return false;
        }

        // Create texture
        Texture2D tex = new Texture2D(size, size);
        int center = size / 2;
        float radius = size / 2f - 2f;

        // Fill with transparent
        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        tex.SetPixels(pixels);

        // Draw circle
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                
                if (dist < radius)
                {
                    // Inside circle - solid color
                    tex.SetPixel(x, y, color);
                }
                else if (dist < radius + 2f)
                {
                    // Edge - anti-aliased
                    float alpha = 1f - (dist - radius) / 2f;
                    tex.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                }
                // else - stays transparent
            }
        }

        tex.Apply();

        // Ensure directory exists
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // Save as PNG
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        Debug.Log($"Created placeholder sprite: {path} ({description})");
        
        return true;
    }
}
