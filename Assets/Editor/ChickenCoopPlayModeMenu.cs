using UnityEditor;

public static class ChickenCoopPlayModeMenu
{
    [MenuItem("ChickenCoop/Exit Play Mode")]
    public static void ExitPlayMode()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
    }
}
