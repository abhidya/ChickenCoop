using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor utility: Scans the open scenes and project prefabs to list GameObjects that have missing MonoBehaviour references.
/// Adds a menu item: Tools/Find Missing Scripts in Scene
/// </summary>
public class FindMissingScripts : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts in Scene")]
    static void ShowWindow()
    {
        GetWindow(typeof(FindMissingScripts), false, "Missing Scripts Finder");
    }

    void OnGUI()
    {
        GUILayout.Label("Search for missing MonoBehaviour references", EditorStyles.boldLabel);
        if (GUILayout.Button("Scan Open Scenes and Prefabs"))
        {
            Scan();
        }
    }

    static void Scan()
    {
        int gameObjectCount = 0;
        int missingCount = 0;
        var results = new List<string>();

        // Scan all root objects in all open scenes
        for (int s = 0; s < UnityEngine.SceneManagement.SceneManager.sceneCount; s++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(s);
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                ScanGameObject(root, ref gameObjectCount, ref missingCount, results);
            }
        }

        // Scan prefabs under Assets (optional): finds prefab assets with missing scripts
        string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in allPrefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                ScanGameObject(prefab, ref gameObjectCount, ref missingCount, results, isAsset:true, assetPath:path);
            }
        }

        // Report
        Debug.Log($"Missing Scripts Scan: scanned {gameObjectCount} GameObjects, found {missingCount} missing script references.");
        foreach (var r in results)
        {
            Debug.Log(r);
        }

        EditorUtility.DisplayDialog("Missing Scripts Scan Complete", $"Scanned {gameObjectCount} GameObjects.\nMissing references: {missingCount}\nSee Console for details.", "OK");
    }

    static void ScanGameObject(GameObject go, ref int gameObjectCount, ref int missingCount, List<string> results, bool isAsset = false, string assetPath = "")
    {
        gameObjectCount++;
        var components = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
        if (components > 0)
        {
            missingCount += components;
            string location = isAsset ? $"Prefab: {assetPath}" : $"SceneObject: {go.scene.name}";
            results.Add($"{location} -> GameObject '{GetFullPath(go)}' has {components} missing scripts.");
        }

        foreach (Transform child in go.transform)
        {
            ScanGameObject(child.gameObject, ref gameObjectCount, ref missingCount, results, isAsset, assetPath);
        }
    }

    static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }
}
