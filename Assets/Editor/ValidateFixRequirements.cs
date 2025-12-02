using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor utility to validate that all fix requirements from the problem statement are met.
/// Adds menu item: Tools/Validate Fix Requirements
/// Checks for missing scripts, duplicates, player registration, and visual improvements.
/// </summary>
public class ValidateFixRequirements : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> validationResults = new List<string>();
    private int passCount = 0;
    private int failCount = 0;
    private int warnCount = 0;

    [MenuItem("Tools/Validate Fix Requirements")]
    static void ShowWindow()
    {
        GetWindow(typeof(ValidateFixRequirements), false, "Validate Fixes");
    }

    void OnGUI()
    {
        GUILayout.Label("Validate Fix Requirements", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Run All Validation Checks"))
        {
            RunValidation();
        }

        GUILayout.Space(10);

        if (validationResults.Count > 0)
        {
            GUILayout.Label($"Results: {passCount} Pass, {failCount} Fail, {warnCount} Warnings", EditorStyles.boldLabel);
            GUILayout.Space(5);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (string result in validationResults)
            {
                GUILayout.Label(result, EditorStyles.wordWrappedLabel);
            }
            GUILayout.EndScrollView();
        }
    }

    void RunValidation()
    {
        validationResults.Clear();
        passCount = 0;
        failCount = 0;
        warnCount = 0;

        AddHeader("=== VALIDATION STARTED ===");

        // Check 1: Missing Scripts
        CheckMissingScripts();

        // Check 2: Duplicate GameObjects
        CheckDuplicateGameObjects();

        // Check 3: Player Tag Registration
        CheckPlayerTagRegistration();

        // Check 4: Prefabs Exist
        CheckRequiredPrefabs();

        // Check 5: Sorting Layers Configured
        CheckSortingLayers();

        // Check 6: Scene Structure
        CheckSceneStructure();

        // Check 7: Sprite Assets (optional)
        CheckSpriteAssets();

        AddHeader("=== VALIDATION COMPLETE ===");
        AddResult($"Total: {passCount} checks passed, {failCount} failed, {warnCount} warnings");

        // Show summary dialog
        string message = $"Validation Complete!\n\nPassed: {passCount}\nFailed: {failCount}\nWarnings: {warnCount}\n\nSee Validation window for details.";
        EditorUtility.DisplayDialog("Validation Results", message, "OK");

        Repaint();
    }

    void CheckMissingScripts()
    {
        AddHeader("1. Checking for Missing Scripts...");

        int missingCount = 0;
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (missing > 0)
            {
                missingCount += missing;
                AddFail($"  '{go.name}' has {missing} missing script(s)");
            }
        }

        if (missingCount == 0)
        {
            AddPass("  ✓ No missing scripts found in scene");
        }
        else
        {
            AddFail($"  ✗ Found {missingCount} missing script references");
            AddFail("    Action: Run Tools > Remove All Missing Scripts");
        }
    }

    void CheckDuplicateGameObjects()
    {
        AddHeader("2. Checking for Duplicate GameObjects...");

        Dictionary<string, List<GameObject>> objectsByName = new Dictionary<string, List<GameObject>>();
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject obj in rootObjects)
        {
            if (!objectsByName.ContainsKey(obj.name))
            {
                objectsByName[obj.name] = new List<GameObject>();
            }
            objectsByName[obj.name].Add(obj);
        }

        var duplicates = objectsByName.Where(kvp => kvp.Value.Count > 1).ToList();

        if (duplicates.Count == 0)
        {
            AddPass("  ✓ No duplicate root GameObjects found");
        }
        else
        {
            foreach (var kvp in duplicates)
            {
                AddFail($"  ✗ '{kvp.Key}' appears {kvp.Value.Count} times");
            }
            AddFail("    Action: Delete duplicate instances manually");
        }
    }

    void CheckPlayerTagRegistration()
    {
        AddHeader("3. Checking Player Tag...");

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        var playersWithTag = allObjects.Where(go => go.CompareTag("Player")).ToList();

        if (playersWithTag.Count == 0)
        {
            AddWarn("  ⚠ No GameObject has the 'Player' tag");
            AddWarn("    Action: Assign 'Player' tag to player GameObject");
        }
        else if (playersWithTag.Count == 1)
        {
            AddPass($"  ✓ Exactly one GameObject has 'Player' tag: {playersWithTag[0].name}");
        }
        else
        {
            AddFail($"  ✗ {playersWithTag.Count} GameObjects have 'Player' tag:");
            foreach (GameObject player in playersWithTag)
            {
                AddFail($"    - {player.name}");
            }
            AddFail("    Action: Set extra GameObjects to 'Untagged'");
        }
    }

    void CheckRequiredPrefabs()
    {
        AddHeader("4. Checking Required Prefabs...");

        string[] requiredPrefabs = new string[]
        {
            "Assets/Prefabs/Player.prefab",
            "Assets/Prefabs/Chicken.prefab",
            "Assets/Prefabs/Egg.prefab",
            "Assets/Prefabs/Helper.prefab"
        };

        bool allExist = true;

        foreach (string prefabPath in requiredPrefabs)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                AddWarn($"  ⚠ Prefab not found: {prefabPath}");
                allExist = false;
            }
            else
            {
                // Check for missing scripts in prefab
                int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab);
                if (missing > 0)
                {
                    AddFail($"  ✗ Prefab has missing scripts: {prefabPath}");
                    allExist = false;
                }
                else
                {
                    AddPass($"  ✓ Prefab exists and valid: {System.IO.Path.GetFileName(prefabPath)}");
                }
            }
        }

        if (!allExist)
        {
            AddWarn("    Action: Create missing prefabs in Unity Editor");
        }
    }

    void CheckSortingLayers()
    {
        AddHeader("5. Checking Sorting Layers...");

        string[] requiredLayers = new string[] { "Background", "Foreground", "Characters", "UI" };
        var sortingLayers = GetSortingLayerNames();

        bool allExist = true;

        foreach (string layerName in requiredLayers)
        {
            if (sortingLayers.Contains(layerName))
            {
                AddPass($"  ✓ Sorting layer exists: {layerName}");
            }
            else
            {
                AddWarn($"  ⚠ Sorting layer missing: {layerName}");
                allExist = false;
            }
        }

        if (!allExist)
        {
            AddWarn("    Action: Add missing sorting layers in Edit > Project Settings > Tags and Layers");
        }
    }

    void CheckSceneStructure()
    {
        AddHeader("6. Checking Scene Structure...");

        string[] requiredObjects = new string[]
        {
            "GameManager",
            "Main Camera",
            "EventSystem"
        };

        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        var rootNames = rootObjects.Select(go => go.name).ToHashSet();

        foreach (string objName in requiredObjects)
        {
            if (rootNames.Contains(objName))
            {
                AddPass($"  ✓ Found: {objName}");
            }
            else
            {
                AddWarn($"  ⚠ Missing: {objName}");
            }
        }

        // Check for GameManager singleton
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            AddPass("  ✓ GameManager instance found in scene");
        }
        else
        {
            AddFail("  ✗ No GameManager instance found");
        }
    }

    void CheckSpriteAssets()
    {
        AddHeader("7. Checking Sprite Assets (Optional)...");

        string[] spritePaths = new string[]
        {
            "Assets/Sprites/Player/player_idle.png",
            "Assets/Sprites/Chicken/chicken_idle.png",
            "Assets/Sprites/Egg/egg.png"
        };

        int foundCount = 0;

        foreach (string spritePath in spritePaths)
        {
            if (AssetDatabase.LoadAssetAtPath<Sprite>(spritePath) != null)
            {
                AddPass($"  ✓ Sprite exists: {System.IO.Path.GetFileName(spritePath)}");
                foundCount++;
            }
        }

        if (foundCount == 0)
        {
            AddWarn("  ⚠ No sprite assets found (still using placeholders)");
            AddWarn("    Note: This is optional - game can work with colored circles");
        }
        else if (foundCount < spritePaths.Length)
        {
            AddWarn($"  ⚠ Only {foundCount} of {spritePaths.Length} key sprites found");
        }
    }

    // Helper methods
    void AddHeader(string header)
    {
        validationResults.Add("");
        validationResults.Add(header);
        Debug.Log(header);
    }

    void AddPass(string message)
    {
        validationResults.Add(message);
        passCount++;
        Debug.Log(message);
    }

    void AddFail(string message)
    {
        validationResults.Add(message);
        if (!message.StartsWith("    ")) // Only count actual failures, not action items
        {
            failCount++;
        }
        Debug.LogWarning(message);
    }

    void AddWarn(string message)
    {
        validationResults.Add(message);
        if (!message.StartsWith("    "))
        {
            warnCount++;
        }
        Debug.LogWarning(message);
    }

    void AddResult(string message)
    {
        validationResults.Add("");
        validationResults.Add(message);
        Debug.Log(message);
    }

    string[] GetSortingLayerNames()
    {
        System.Type internalEditorUtilityType = typeof(UnityEditorInternal.InternalEditorUtility);
        System.Reflection.PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        return (string[])sortingLayersProperty.GetValue(null, new object[0]);
    }
}
