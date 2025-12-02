using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor window for creating visual GameObjects with colored sprites.
/// These objects can have their sprites easily replaced with proper art assets.
/// </summary>
public class GameObjectCreatorWindow : EditorWindow
{
    [MenuItem("Tools/Game Objects/Create Visual GameObjects")]
    public static void ShowWindow()
    {
        GetWindow<GameObjectCreatorWindow>("Visual GameObject Creator");
    }
    
    [MenuItem("Tools/Game Objects/Create Player")]
    public static void CreatePlayer()
    {
        GameObject go = FindObjectOfType<VisualGameObjectCreator>()?.CreatePlayerVisual();
        if (go == null)
        {
            // Create temporary creator
            GameObject temp = new GameObject("TempCreator");
            var creator = temp.AddComponent<VisualGameObjectCreator>();
            go = creator.CreatePlayerVisual();
            DestroyImmediate(temp);
        }
        
        if (go != null)
        {
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
        }
    }
    
    [MenuItem("Tools/Game Objects/Create Chicken")]
    public static void CreateChicken()
    {
        GameObject go = null;
        GameObject temp = new GameObject("TempCreator");
        var creator = temp.AddComponent<VisualGameObjectCreator>();
        go = creator.CreateChickenVisual();
        DestroyImmediate(temp);
        
        if (go != null)
        {
            Selection.activeGameObject = go;
            EditorGUIUtility.PingObject(go);
        }
    }
    
    [MenuItem("Tools/Game Objects/Create Environment")]
    public static void CreateEnvironment()
    {
        GameObject temp = new GameObject("TempCreator");
        var creator = temp.AddComponent<VisualGameObjectCreator>();
        creator.CreateEnvironmentVisuals();
        DestroyImmediate(temp);
        
        EditorUtility.DisplayDialog("Success", 
            "Environment GameObjects created!\n\n" +
            "Created:\n" +
            "- Background (green grass)\n" +
            "- Corn Field (with label)\n" +
            "- Store (with label)\n\n" +
            "Replace their sprites in the Inspector with proper art.", 
            "OK");
    }
    
    [MenuItem("Tools/Game Objects/Create Egg Prefab")]
    public static void CreateEggPrefab()
    {
        GameObject temp = new GameObject("TempCreator");
        var creator = temp.AddComponent<VisualGameObjectCreator>();
        GameObject egg = creator.CreateEggPrefab();
        DestroyImmediate(temp);
        
        if (egg != null)
        {
            // Save as prefab
            string prefabPath = "Assets/Prefabs/Egg.prefab";
            bool success = false;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(egg, prefabPath, out success);
            
            if (success)
            {
                Debug.Log($"Egg prefab saved to {prefabPath}");
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
            
            // Clean up scene instance
            DestroyImmediate(egg);
        }
    }
    
    [MenuItem("Tools/Game Objects/Create All Visuals")]
    public static void CreateAllVisuals()
    {
        GameObject temp = new GameObject("TempCreator");
        var creator = temp.AddComponent<VisualGameObjectCreator>();
        creator.CreateAllVisuals();
        creator.CreateEggPrefab();
        DestroyImmediate(temp);
        
        EditorUtility.DisplayDialog("Success", 
            "All visual GameObjects created!\n\n" +
            "Created:\n" +
            "- Player (blue circle)\n" +
            "- Chicken (yellow circle with eye)\n" +
            "- Environment (background, corn field, store)\n" +
            "- Egg prefab\n\n" +
            "All objects have SpriteRenderers.\n" +
            "Simply assign proper sprites in the Inspector to replace the colored circles!", 
            "OK");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Visual GameObject Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Create GameObjects with colored sprite placeholders.\n\n" +
            "Each GameObject has a SpriteRenderer with a colored circle sprite.\n" +
            "Replace the 'Sprite' field in the Inspector with proper art assets when ready.",
            MessageType.Info);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create All Visuals", GUILayout.Height(40)))
        {
            CreateAllVisuals();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Create Individual Objects:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Player"))
        {
            CreatePlayer();
        }
        
        if (GUILayout.Button("Create Chicken"))
        {
            CreateChicken();
        }
        
        if (GUILayout.Button("Create Environment"))
        {
            CreateEnvironment();
        }
        
        if (GUILayout.Button("Create Egg Prefab"))
        {
            CreateEggPrefab();
        }
        
        GUILayout.Space(20);
        
        EditorGUILayout.HelpBox(
            "How to replace sprites:\n" +
            "1. Select the GameObject in the Hierarchy\n" +
            "2. Find 'Sprite Renderer' component in Inspector\n" +
            "3. Drag your sprite into the 'Sprite' field\n" +
            "4. Adjust 'Pixels Per Unit' if needed",
            MessageType.None);
    }
}
