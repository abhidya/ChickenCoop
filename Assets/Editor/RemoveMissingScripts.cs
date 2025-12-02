using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to batch remove all missing script references from scene GameObjects.
/// Adds menu item: Tools/Remove All Missing Scripts
/// </summary>
public class RemoveMissingScripts : EditorWindow
{
    [MenuItem("Tools/Remove All Missing Scripts")]
    static void RemoveAll()
    {
        if (!EditorUtility.DisplayDialog(
            "Remove Missing Scripts", 
            "This will remove all missing script references from the current scene. This cannot be undone. Continue?",
            "Yes, Remove Them",
            "Cancel"))
        {
            return;
        }

        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int removedCount = 0;
        int objectsAffected = 0;
        
        foreach (GameObject go in allObjects)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0)
            {
                removedCount += removed;
                objectsAffected++;
                Debug.Log($"Removed {removed} missing script(s) from '{go.name}'", go);
            }
        }
        
        if (removedCount > 0)
        {
            Debug.Log($"<color=green>✓</color> Removed {removedCount} missing scripts from {objectsAffected} GameObjects");
            EditorUtility.DisplayDialog(
                "Complete", 
                $"Removed {removedCount} missing scripts from {objectsAffected} GameObjects.\n\nSee Console for details.",
                "OK");
        }
        else
        {
            Debug.Log("No missing scripts found in scene.");
            EditorUtility.DisplayDialog("Complete", "No missing scripts found in scene.", "OK");
        }
    }
}
