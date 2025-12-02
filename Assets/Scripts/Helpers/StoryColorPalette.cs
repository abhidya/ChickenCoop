using UnityEngine;

/// <summary>
/// StoryColorPalette - Defines the pastel color scheme for the story upgrade.
/// Use these colors for consistent visual styling throughout the game.
/// Based on "soft pastel, cartoony, cute, Happy Harvest vibe" requirements.
/// </summary>
public static class StoryColorPalette
{
    // ========== PRIMARY COLORS ==========
    
    /// <summary>Sky blue for backgrounds and sky elements</summary>
    public static readonly Color SkyBlue = new Color(0.72f, 0.83f, 0.91f); // #B8D4E8
    
    /// <summary>Grass green for ground and nature elements</summary>
    public static readonly Color GrassGreen = new Color(0.78f, 0.90f, 0.79f); // #C8E6C9
    
    /// <summary>Corn yellow for corn and gold elements</summary>
    public static readonly Color CornYellow = new Color(1.0f, 0.96f, 0.62f); // #FFF59D
    
    /// <summary>Warm brown for wood and earth elements</summary>
    public static readonly Color WarmBrown = new Color(0.84f, 0.80f, 0.78f); // #D7CCC8
    
    /// <summary>Soft white for highlights and clean elements</summary>
    public static readonly Color SoftWhite = new Color(0.98f, 0.98f, 0.98f); // #FAFAFA
    
    // ========== ACCENT COLORS ==========
    
    /// <summary>Chicken orange for chicken character</summary>
    public static readonly Color ChickenOrange = new Color(1.0f, 0.88f, 0.70f); // #FFE0B2
    
    /// <summary>Coin gold for coins and money</summary>
    public static readonly Color CoinGold = new Color(1.0f, 0.84f, 0.31f); // #FFD54F
    
    /// <summary>Egg cream for eggs</summary>
    public static readonly Color EggCream = new Color(1.0f, 0.97f, 0.88f); // #FFF8E1
    
    /// <summary>Button green for positive action buttons</summary>
    public static readonly Color ButtonGreen = new Color(0.65f, 0.84f, 0.66f); // #A5D6A7
    
    /// <summary>Button blue for secondary action buttons</summary>
    public static readonly Color ButtonBlue = new Color(0.56f, 0.79f, 0.98f); // #90CAF9
    
    // ========== UI COLORS ==========
    
    /// <summary>Text color - dark brown for readability</summary>
    public static readonly Color TextDark = new Color(0.24f, 0.15f, 0.14f); // #3E2723
    
    /// <summary>UI background - light cream</summary>
    public static readonly Color UIBackground = new Color(1.0f, 0.98f, 0.77f); // #FFF9C4
    
    /// <summary>Panel background - light gray</summary>
    public static readonly Color PanelBackground = new Color(0.96f, 0.96f, 0.96f); // #F5F5F5
    
    // ========== SEMANTIC COLORS ==========
    
    /// <summary>Success/Ready state - bright green</summary>
    public static readonly Color Ready = new Color(0.5f, 1.0f, 0.5f);
    
    /// <summary>Cooldown/Disabled state - gray</summary>
    public static readonly Color Cooldown = new Color(0.7f, 0.7f, 0.7f);
    
    /// <summary>Error/Cannot afford - soft red</summary>
    public static readonly Color Error = new Color(1.0f, 0.6f, 0.6f);
    
    /// <summary>Upgrade/Special - golden</summary>
    public static readonly Color Special = new Color(1.0f, 0.90f, 0.3f);
    
    // ========== HELPER METHODS ==========
    
    /// <summary>
    /// Get a pastel color for a helper based on their ID.
    /// Generates unique but consistent colors for each helper.
    /// </summary>
    public static Color GetHelperColor(int helperId)
    {
        // Base color is a warm beige/tan
        Color baseColor = WarmBrown;
        
        // Add hue offset based on helper ID
        float hueOffset = (helperId * 0.15f) % 1.0f;
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);
        
        // Keep saturation and value in pastel range
        s = Mathf.Clamp(s + 0.1f, 0.2f, 0.4f); // Gentle saturation
        v = Mathf.Clamp(v, 0.8f, 0.95f); // Keep bright
        
        return Color.HSVToRGB((h + hueOffset) % 1.0f, s, v);
    }
    
    /// <summary>
    /// Get a color with adjusted alpha for transparency
    /// </summary>
    public static Color WithAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
    
    /// <summary>
    /// Get a lighter version of a color (pastel-ify)
    /// </summary>
    public static Color Lighten(Color color, float amount = 0.3f)
    {
        return Color.Lerp(color, Color.white, amount);
    }
    
    /// <summary>
    /// Get a darker version of a color
    /// </summary>
    public static Color Darken(Color color, float amount = 0.3f)
    {
        return Color.Lerp(color, Color.black, amount);
    }
    
    /// <summary>
    /// Apply the color palette to a GameConfig asset
    /// </summary>
    public static void ApplyToGameConfig(GameConfig config)
    {
        if (config == null) return;
        
        config.cornColor = CornYellow;
        config.eggColor = EggCream;
        config.coinColor = CoinGold;
        config.readyColor = Ready;
        config.cooldownColor = Cooldown;
    }
}
