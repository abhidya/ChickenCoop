using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor utility to find duplicate GameObjects by name in the scene hierarchy.
/// Adds menu item: Tools/Find Duplicate GameObjects
/// </summary>
public class FindDuplicateGameObjects : EditorWindow
{
    [MenuItem("Tools/Find Duplicate GameObjects")]
    static void ShowWindow()
    {
        GetWindow(typeof(FindDuplicateGameObjects), false, "Find Duplicates");
    }

    void OnGUI()
    {
        GUILayout.Label("Find Duplicate GameObjects", EditorStyles.boldLabel);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Scan for Duplicates in Scene"))
        {
            FindDuplicates();
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label("This will scan for root GameObjects with identical names.", EditorStyles.wordWrappedLabel);
    }

    static void FindDuplicates()
    {
        Dictionary<string, List<GameObject>> objectsByName = new Dictionary<string, List<GameObject>>();
        GameObject[] allObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        
        foreach (GameObject obj in allObjects)
        {
            if (!objectsByName.ContainsKey(obj.name))
            {
                objectsByName[obj.name] = new List<GameObject>();
            }
            objectsByName[obj.name].Add(obj);
        }
        
        var duplicates = objectsByName.Where(kvp => kvp.Value.Count > 1).ToList();
        
        if (duplicates.Count > 0)
        {
            Debug.LogWarning($"<color=yellow>⚠</color> Found {duplicates.Count} sets of duplicate GameObjects:");
            
            foreach (var kvp in duplicates)
            {
                Debug.LogWarning($"  '{kvp.Key}' appears {kvp.Value.Count} times:");
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    GameObject obj = kvp.Value[i];
                    string components = GetComponentSummary(obj);
                    Debug.Log($"    [{i + 1}] {obj.name} at {obj.transform.position} - {components}", obj);
                }
            }
            
            EditorUtility.DisplayDialog(
                "Duplicates Found", 
                $"Found {duplicates.Count} sets of duplicate GameObjects.\n\nSee Console for details.\n\nClick on the log entries to select objects in Hierarchy.",
                "OK");
        }
        else
        {
            Debug.Log("<color=green>✓</color> No duplicate root GameObjects found!");
            EditorUtility.DisplayDialog("No Duplicates", "No duplicate root GameObjects found in scene.", "OK");
        }
    }

    static string GetComponentSummary(GameObject go)
    {
        var components = go.GetComponents<Component>();
        var names = components.Where(c => c != null && !(c is Transform))
                              .Select(c => c.GetType().Name)
                              .Take(3);
        
        string summary = string.Join(", ", names);
        if (components.Length > 4)
        {
            summary += $" (+{components.Length - 4} more)";
        }
        
        return summary.Length > 0 ? summary : "No components";
    }
}
