using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class StoryUpgradeBatchRunner
{
    private const string ScenePath = "Assets/Scenes/MainGame.unity";
    private const string ReportPath = ".omx/state/story-upgrade-batch-report.txt";

    public static void AutoWireScene()
    {
        RunWithReport(() =>
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            string summary = StoryUpgradeSceneWiringUtility.AutoWireStoryScene();
            EditorSceneManager.SaveOpenScenes();
            return summary;
        });
    }

    public static void ValidateScene()
    {
        RunWithReport(() =>
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            return StoryUpgradeSceneWiringUtility.BuildValidationReport();
        });
    }

    private static void RunWithReport(Func<string> action)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? ".omx/state");
            string report = action();
            File.WriteAllText(ReportPath, report + Environment.NewLine);
            Debug.Log(report);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? ".omx/state");
            File.WriteAllText(ReportPath, "FAILED\n" + ex + Environment.NewLine);
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }
}
