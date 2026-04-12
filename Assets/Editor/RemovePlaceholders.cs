using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RemovePlaceholders
{
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainGame.unity", OpenSceneMode.Single);
        
        // Find duplicate roots
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        int removed = 0;

        foreach (var root in roots)
        {
            if (root.name == "Background" && root.GetComponents<Component>().Length <= 3) 
            {
                // Delete if it's the placeholder one
                if (root.transform.position == Vector3.zero || root.GetComponent<SpriteRenderer>() == null || root.GetComponent<SpriteRenderer>().sprite == null || root.GetComponent<SpriteRenderer>().sprite.name.Contains("placeholder") || root.GetComponent<SpriteRenderer>().sprite.name == "") 
                {
                    // Actually wait, some might just be the old placeholders.
                    // Let's just use the known names that are duplicated and look for the ones with fewer components or placeholder sprites
                }
            }
        }
    }
}
