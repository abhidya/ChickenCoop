using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public static class FixDuplicateRoots
{
    [MenuItem("Tools/Fix Duplicate Roots")]
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity", OpenSceneMode.Single);
        
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        var toDelete = new List<GameObject>();

        var byName = new Dictionary<string, List<GameObject>>();
        foreach (var root in roots)
        {
            if (!byName.ContainsKey(root.name)) byName[root.name] = new List<GameObject>();
            byName[root.name].Add(root);
        }

        foreach (var kvp in byName)
        {
            if (kvp.Value.Count > 1)
            {
                // Sort by component count (keep the one with more components, likely the real one)
                var sorted = kvp.Value.OrderByDescending(go => go.GetComponents<Component>().Length).ToList();
                for (int i = 1; i < sorted.Count; i++)
                {
                    Debug.Log("Deleting duplicate: " + sorted[i].name);
                    toDelete.Add(sorted[i]);
                }
            }
        }

        // Also check if there's both "Store" and "StoreCounter"
        var store = roots.FirstOrDefault(r => r.name == "Store");
        var storeCounter = roots.FirstOrDefault(r => r.name == "StoreCounter");
        if (store != null && storeCounter != null)
        {
            // StoreCounter is the real one from the guide
            toDelete.Add(store);
            Debug.Log("Deleting duplicate Store (keeping StoreCounter)");
        }

        foreach (var go in toDelete)
        {
            GameObject.DestroyImmediate(go);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Duplicates cleaned up.");
    }
}
