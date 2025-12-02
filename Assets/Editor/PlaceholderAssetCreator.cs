using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor tool to create placeholder sprites for the game.
/// Creates simple colored sprites for all game objects until proper art is added.
/// </summary>
public class PlaceholderAssetCreator : EditorWindow
{
    [MenuItem("Tools/Create Placeholder Sprites")]
    public static void CreatePlaceholderSprites()
    {
        string basePath = "Assets/Art/Sprites";
        
        // Ensure directories exist
        EnsureDirectoryExists($"{basePath}/Characters");
        EnsureDirectoryExists($"{basePath}/Objects");
        EnsureDirectoryExists($"{basePath}/UI");
        EnsureDirectoryExists($"{basePath}/Environment");
        
        // Create character sprites
        CreateAndSaveSprite("farmer", new Color(0.4f, 0.6f, 0.9f), 64, $"{basePath}/Characters", SpriteType.Circle);
        CreateAndSaveSprite("helper", new Color(0.9f, 0.8f, 0.6f), 48, $"{basePath}/Characters", SpriteType.Circle);
        CreateAndSaveSprite("chicken", new Color(1f, 0.9f, 0.3f), 56, $"{basePath}/Characters", SpriteType.Circle);
        CreateAndSaveSprite("chicken_eye", new Color(0.1f, 0.1f, 0.1f), 8, $"{basePath}/Characters", SpriteType.Circle);
        
        // Create object sprites
        CreateAndSaveSprite("egg", new Color(0.95f, 0.9f, 0.85f), 32, $"{basePath}/Objects", SpriteType.Egg);
        CreateAndSaveSprite("corn", new Color(1f, 0.85f, 0.2f), 32, $"{basePath}/Objects", SpriteType.Triangle);
        CreateAndSaveSprite("coin", new Color(1f, 0.84f, 0f), 24, $"{basePath}/Objects", SpriteType.Circle);
        
        // Create UI sprites
        CreateAndSaveSprite("ui_corn_icon", new Color(1f, 0.85f, 0.2f), 32, $"{basePath}/UI", SpriteType.Square);
        CreateAndSaveSprite("ui_egg_icon", new Color(0.95f, 0.9f, 0.85f), 32, $"{basePath}/UI", SpriteType.Square);
        CreateAndSaveSprite("ui_coin_icon", new Color(1f, 0.84f, 0f), 32, $"{basePath}/UI", SpriteType.Square);
        CreateAndSaveSprite("ui_button", new Color(0.3f, 0.7f, 0.3f), 128, $"{basePath}/UI", SpriteType.Square);
        CreateAndSaveSprite("ui_panel", new Color(0.2f, 0.2f, 0.25f, 0.8f), 256, $"{basePath}/UI", SpriteType.Square);
        
        // Create environment sprites
        CreateAndSaveSprite("grass", new Color(0.4f, 0.7f, 0.3f), 64, $"{basePath}/Environment", SpriteType.Square);
        CreateAndSaveSprite("store", new Color(0.6f, 0.4f, 0.3f), 96, $"{basePath}/Environment", SpriteType.Square);
        CreateAndSaveSprite("cornfield", new Color(0.5f, 0.8f, 0.2f), 96, $"{basePath}/Environment", SpriteType.Square);
        
        AssetDatabase.Refresh();
        
        Debug.Log("✅ Placeholder sprites created successfully!");
        Debug.Log("📁 Location: Assets/Art/Sprites/");
        Debug.Log("ℹ️ These are simple colored shapes. Replace with proper art from Kenney.nl for better visuals.");
        
        EditorUtility.DisplayDialog("Success", 
            "Placeholder sprites created!\n\n" +
            "Location: Assets/Art/Sprites/\n\n" +
            "These are simple colored shapes as placeholders.\n" +
            "Replace with proper art from Kenney.nl (CC0 license) for production quality.", 
            "OK");
    }
    
    private enum SpriteType
    {
        Square,
        Circle,
        Egg,
        Triangle
    }
    
    private static void CreateAndSaveSprite(string name, Color color, int size, string path, SpriteType type)
    {
        Sprite sprite = null;
        
        switch (type)
        {
            case SpriteType.Square:
                sprite = PlaceholderSpriteGenerator.CreateSquareSprite(color, size);
                break;
            case SpriteType.Circle:
                sprite = PlaceholderSpriteGenerator.CreateCircleSprite(color, size);
                break;
            case SpriteType.Egg:
                sprite = PlaceholderSpriteGenerator.CreateEggSprite(color, size);
                break;
            case SpriteType.Triangle:
                sprite = PlaceholderSpriteGenerator.CreateTriangleSprite(color, size);
                break;
        }
        
        if (sprite == null)
        {
            Debug.LogError($"Failed to create sprite: {name}");
            return;
        }
        
        // Save texture as PNG
        string filePath = $"{path}/{name}.png";
        Texture2D texture = sprite.texture;
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
        
        // Import and configure as sprite
        AssetDatabase.ImportAsset(filePath);
        TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = size;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        
        Debug.Log($"Created sprite: {name} at {filePath}");
    }
    
    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
