using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to help set up the game according to the Story Upgrade requirements.
/// Provides menu commands for creating ScriptableObject assets with correct values.
/// </summary>
public class StoryUpgradeSetup : EditorWindow
{
    [MenuItem("Tools/Story Upgrade/Setup Game Config")]
    public static void SetupGameConfig()
    {
        // Check if Resources folder exists
        string resourcesPath = "Assets/Resources";
        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        // Check if ScriptableObjects folder exists
        string soPath = "Assets/ScriptableObjects";
        if (!AssetDatabase.IsValidFolder(soPath))
        {
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        }

        // Create GameConfig asset
        GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
        
        // Set values according to story requirements
        config.startingCorn = 0;
        config.startingEggs = 0;
        config.startingCoins = 50;
        config.cornPerHarvest = 1;
        config.harvestCooldown = 2.0f;
        config.cornToFeed = 1;
        config.eggsPerFeed = 1;
        config.eggSellPrice = 10;
        config.helperBaseCost = 100;
        config.helperCostIncrease = 50;
        config.helperSpeed = 3.0f;
        config.helperWaitTime = 0.5f;
        config.tweenDuration = 0.3f;
        config.punchScale = 1.3f;
        config.bobSpeed = 2.0f;
        config.bobAmount = 0.1f;
        
        // Set colors
        config.cornColor = new Color(1f, 0.9f, 0.3f);
        config.eggColor = new Color(1f, 0.98f, 0.9f);
        config.coinColor = new Color(1f, 0.85f, 0.2f);
        config.readyColor = new Color(0.5f, 1f, 0.5f);
        config.cooldownColor = new Color(0.7f, 0.7f, 0.7f);

        string configPath = soPath + "/GameConfig_Story.asset";
        AssetDatabase.CreateAsset(config, configPath);
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Success", 
            "GameConfig_Story.asset created at:\n" + configPath + 
            "\n\nValues set according to story requirements:\n" +
            "- Starting coins: 50\n" +
            "- Egg sell price: 10\n" +
            "- Helper cost: 100 + (count × 50)\n\n" +
            "Assign this to your GameManager in the scene!", 
            "OK");
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = config;
    }

    [MenuItem("Tools/Story Upgrade/Create All Upgrade Assets")]
    public static void CreateAllUpgrades()
    {
        string soPath = "Assets/ScriptableObjects";
        if (!AssetDatabase.IsValidFolder(soPath))
        {
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        }
        
        string upgradesPath = soPath + "/Upgrades";
        if (!AssetDatabase.IsValidFolder(upgradesPath))
        {
            AssetDatabase.CreateFolder(soPath, "Upgrades");
        }

        // Create 5 upgrades according to story
        CreateUpgrade(upgradesPath, "BetterSeeds", "Better Seeds", 
            "Improved seeds yield 20% more corn per harvest", 
            100, UpgradeType.CornField, 1.2f);
        
        CreateUpgrade(upgradesPath, "HealthierChickens", "Healthier Chickens", 
            "Healthier chickens produce 20% more eggs", 
            200, UpgradeType.ChickenProduction, 1.2f);
        
        CreateUpgrade(upgradesPath, "PremiumEggs", "Premium Eggs", 
            "Premium eggs sell for 20% higher prices", 
            300, UpgradeType.EggPrice, 1.2f);
        
        CreateUpgrade(upgradesPath, "FasterOperations", "Faster Operations", 
            "All farm operations are 20% faster", 
            500, UpgradeType.Speed, 1.2f);
        
        CreateUpgrade(upgradesPath, "BiggerStore", "Bigger Store", 
            "Expanded store increases efficiency by 20%", 
            750, UpgradeType.StoreCapacity, 1.2f);

        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog("Success", 
            "Created 5 upgrade assets in:\n" + upgradesPath + 
            "\n\n1. Better Seeds (100 coins)\n" +
            "2. Healthier Chickens (200 coins)\n" +
            "3. Premium Eggs (300 coins)\n" +
            "4. Faster Operations (500 coins)\n" +
            "5. Bigger Store (750 coins)\n\n" +
            "Assign these to your UIManager!", 
            "OK");
        
        EditorUtility.FocusProjectWindow();
    }

    private static void CreateUpgrade(string path, string fileName, string upgradeName, 
        string description, int cost, UpgradeType type, float multiplier)
    {
        UpgradeData upgrade = ScriptableObject.CreateInstance<UpgradeData>();
        upgrade.upgradeName = upgradeName;
        upgrade.description = description;
        upgrade.baseCost = cost;
        upgrade.costMultiplier = 1.0f; // No scaling, one-time purchase
        upgrade.maxLevel = 1; // One-time purchase only
        upgrade.upgradeType = type;
        upgrade.effectMultiplier = multiplier;
        upgrade.flatBonus = 0;
        upgrade.tintColor = Color.white;

        string assetPath = path + "/" + fileName + ".asset";
        AssetDatabase.CreateAsset(upgrade, assetPath);
        
        Debug.Log("Created upgrade: " + upgradeName + " at " + assetPath);
    }

    [MenuItem("Tools/Story Upgrade/Open Documentation")]
    public static void OpenDocumentation()
    {
        string[] docs = new string[]
        {
            "Assets/../Documentation/ASSET_INTEGRATION_GUIDE.md",
            "Assets/../Documentation/STORY_UPGRADE_REQUIREMENTS.md",
            "Assets/../Documentation/GAME_STORY.md",
            "Assets/../Documentation/UI_SETUP.md"
        };

        foreach (string doc in docs)
        {
            string fullPath = Path.GetFullPath(doc);
            if (File.Exists(fullPath))
            {
                EditorUtility.OpenWithDefaultApp(fullPath);
            }
        }

        EditorUtility.DisplayDialog("Documentation", 
            "Opening story upgrade documentation:\n\n" +
            "- ASSET_INTEGRATION_GUIDE.md\n" +
            "- STORY_UPGRADE_REQUIREMENTS.md\n" +
            "- GAME_STORY.md\n" +
            "- UI_SETUP.md\n\n" +
            "Check your default markdown viewer!", 
            "OK");
    }

    [MenuItem("Tools/Story Upgrade/Show Asset Locations")]
    public static void ShowAssetLocations()
    {
        string message = "Asset Locations for Story Integration:\n\n";
        
        message += "CHICKENS:\n";
        message += "→ Assets/HappyHarvest/Art/Animals/Chicken/\n\n";
        
        message += "CORN:\n";
        message += "→ Assets/HappyHarvest/Art/Crops/Corn/Sprites/\n\n";
        
        message += "MARKET/STORE:\n";
        message += "→ Assets/HappyHarvest/Art/Environment/Market/\n\n";
        
        message += "UI ELEMENTS:\n";
        message += "→ Assets/HappyHarvest/Art/UI/\n";
        message += "  - Sprite_Button_green.png\n";
        message += "  - Sprite_Button_Blue.png\n";
        message += "  - Sprite_coin_icon.png\n\n";
        
        message += "PARTICLES:\n";
        message += "→ Assets/HappyHarvest/VFX/\n";
        message += "  - Tools/VFX_Harvest_Corn.vfx\n";
        message += "  - StepDust/P_VFX_Step_Dust.prefab\n";
        message += "  - Ambient Dust/ (for atmosphere)\n\n";
        
        message += "ENVIRONMENT:\n";
        message += "→ Assets/HappyHarvest/Art/Environment/\n";
        message += "  - Bush/, Flowers/, Pinetree/, Grass/\n\n";
        
        message += "See ASSET_INTEGRATION_GUIDE.md for full details!";
        
        EditorUtility.DisplayDialog("Asset Locations", message, "OK");
    }

    [MenuItem("Tools/Story Upgrade/Validate Scene Setup")]
    public static void ValidateSceneSetup()
    {
        string report = "Scene Validation Report:\n\n";
        bool hasIssues = false;

        // Check for GameManager
        GameManager gm = GameObject.FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            report += "❌ GameManager not found in scene!\n";
            hasIssues = true;
        }
        else
        {
            report += "✓ GameManager found\n";
            if (gm.Config == null)
            {
                report += "  ⚠️ GameConfig not assigned\n";
                hasIssues = true;
            }
            else
            {
                report += "  ✓ GameConfig assigned\n";
            }
        }

        // Check for UIManager
        UIManager ui = GameObject.FindAnyObjectByType<UIManager>();
        if (ui == null)
        {
            report += "❌ UIManager not found in scene!\n";
            hasIssues = true;
        }
        else
        {
            report += "✓ UIManager found\n";
        }

        // Check for Canvas
        Canvas canvas = GameObject.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            report += "❌ Canvas not found in scene!\n";
            report += "  → Create one: GameObject > UI > Canvas\n";
            hasIssues = true;
        }
        else
        {
            report += "✓ Canvas found\n";
        }

        // Check for EventSystem
        UnityEngine.EventSystems.EventSystem es = GameObject.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
        {
            report += "❌ EventSystem not found!\n";
            report += "  → Create one: GameObject > UI > Event System\n";
            hasIssues = true;
        }
        else
        {
            report += "✓ EventSystem found\n";
        }

        // Check for main game objects
        GameObject chicken = GameObject.Find("Chicken");
        GameObject cornField = GameObject.Find("CornField");
        GameObject store = GameObject.Find("Store");

        if (chicken == null)
        {
            report += "⚠️ Chicken GameObject not found\n";
            hasIssues = true;
        }
        else
        {
            report += "✓ Chicken found\n";
        }

        if (cornField == null)
        {
            report += "⚠️ CornField GameObject not found\n";
            hasIssues = true;
        }
        else
        {
            report += "✓ CornField found\n";
        }

        if (store == null)
        {
            report += "⚠️ Store GameObject not found\n";
            hasIssues = true;
        }
        else
        {
            report += "✓ Store found\n";
        }

        if (!hasIssues)
        {
            report += "\n✅ Scene setup looks good!\n";
            report += "\nNext steps:\n";
            report += "1. Assign GameConfig to GameManager\n";
            report += "2. Wire UI elements to UIManager\n";
            report += "3. Replace placeholder sprites\n";
            report += "4. Add particle effects\n";
        }
        else
        {
            report += "\n⚠️ Scene needs setup work\n";
            report += "\nRefer to UI_SETUP.md for instructions!";
        }

        EditorUtility.DisplayDialog("Scene Validation", report, "OK");
    }
}
