using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIManager))]
public class UIManagerStoryUpgradeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Story Upgrade helpers keep ChickenCoop UI wiring in ChickenCoop-owned code and reduce manual drag/drop setup.",
            MessageType.Info);

        if (GUILayout.Button("Auto Wire Story UI + Title Card"))
        {
            string summary = StoryUpgradeSceneWiringUtility.AutoWireStoryScene();
            EditorUtility.DisplayDialog("Story Upgrade Auto-Wire", summary, "OK");
        }

        if (GUILayout.Button("Validate Story Wiring"))
        {
            string report = StoryUpgradeSceneWiringUtility.BuildValidationReport();
            Debug.Log(report);
            EditorUtility.DisplayDialog("Story Wiring Validation", report, "OK");
        }
    }
}

[CustomEditor(typeof(TitleCardManager))]
public class TitleCardManagerStoryUpgradeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TitleCardManager manager = (TitleCardManager)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Repair title-card references without editing MainGame.unity by hand. This keeps the cinematic story layer easy to rewire.",
            MessageType.Info);

        if (GUILayout.Button("Repair Title Card References"))
        {
            string summary = StoryUpgradeSceneWiringUtility.AutoWireTitleCardOnly();
            EditorUtility.DisplayDialog("Title Card Repair", summary, "OK");
        }

        if (GUILayout.Button("Reapply Story Defaults"))
        {
            Undo.RecordObject(manager, "Reapply Title Card Story Defaults");
            manager.ApplyStoryDefaults();
            EditorUtility.SetDirty(manager);
        }

        if (GUILayout.Button("Log Title Card Validation"))
        {
            Debug.Log(manager.BuildValidationSummary(), manager);
        }
    }
}
