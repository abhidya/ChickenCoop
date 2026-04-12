using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public static class AutoCleanup
{
    static AutoCleanup()
    {
        EditorApplication.delayCall += RunCleanup;
    }

    private static void RunCleanup()
    {
        EditorApplication.delayCall -= RunCleanup;
        if (!SceneManager.GetActiveScene().isLoaded) return;
        
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
                var sorted = kvp.Value.OrderByDescending(go => go.GetComponents<Component>().Length).ToList();
                for (int i = 1; i < sorted.Count; i++)
                {
                    toDelete.Add(sorted[i]);
                }
            }
        }

        var store = roots.FirstOrDefault(r => r.name == "Store");
        var storeCounter = roots.FirstOrDefault(r => r.name == "StoreCounter");
        if (store != null && storeCounter != null)
        {
            toDelete.Add(store);
        }

        if (toDelete.Count > 0)
        {
            foreach (var go in toDelete)
            {
                GameObject.DestroyImmediate(go);
            }
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"Cleaned {toDelete.Count} duplicates.");
        }
    }
}
