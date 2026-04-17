using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using ChickenCoop.Managers;
using ChickenCoop.Managers;

public static class CodexSceneAudit
{
    private const string ScenePath = "Assets/Scenes/MainGame.unity";
    private const string ReportPath = ".omx/state/codex-scene-audit.txt";

    public static void Run()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath)!);
        var sb = new StringBuilder();
        sb.AppendLine("Codex Scene Audit");
        sb.AppendLine($"ScenePath: {ScenePath}");

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        sb.AppendLine($"SceneLoaded: {scene.isLoaded}");

        AppendObject(sb, "GameManager", Object.FindFirstObjectByType<GameManager>()?.gameObject);
        AppendObject(sb, "UIManager", Object.FindFirstObjectByType<UIManager>()?.gameObject);
        AppendObject(sb, "TitleCardManager", Object.FindFirstObjectByType<TitleCardManager>()?.gameObject);
        AppendObject(sb, "Canvas", Object.FindFirstObjectByType<Canvas>()?.gameObject);
        AppendObject(sb, "EventSystem", Object.FindFirstObjectByType<EventSystem>()?.gameObject);

        AppendNamedObject(sb, "Chicken", GameObject.Find("Chicken"));
        AppendNamedObject(sb, "CornField", GameObject.Find("CornField"));
        AppendNamedObject(sb, "Store", GameObject.Find("Store"));
        AppendNamedObject(sb, "StoreCounter", GameObject.Find("StoreCounter"));

        var gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            sb.AppendLine($"GameManager.ConfigAssigned: {gm.Config != null}");
        }

        var ui = Object.FindFirstObjectByType<UIManager>();
        sb.AppendLine($"UIManager.Present: {ui != null}");

        var configGuids = AssetDatabase.FindAssets("t:GameConfig");
        sb.AppendLine($"GameConfigAssets: {configGuids.Length}");
        foreach (var guid in configGuids)
        {
            sb.AppendLine($"- {AssetDatabase.GUIDToAssetPath(guid)}");
        }

        var upgradeGuids = AssetDatabase.FindAssets("t:UpgradeData");
        sb.AppendLine($"UpgradeAssets: {upgradeGuids.Length}");
        foreach (var guid in upgradeGuids)
        {
            sb.AppendLine($"- {AssetDatabase.GUIDToAssetPath(guid)}");
        }

        var rootObjects = scene.GetRootGameObjects().Select(go => go.name).OrderBy(x => x).ToArray();
        sb.AppendLine("RootObjects:");
        foreach (var name in rootObjects)
        {
            sb.AppendLine($"- {name}");
        }

        File.WriteAllText(ReportPath, sb.ToString());
        Debug.Log(sb.ToString());
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }

    private static void AppendObject(StringBuilder sb, string label, GameObject go)
    {
        sb.AppendLine($"{label}: {(go != null ? go.name : "MISSING")}");
    }

    private static void AppendNamedObject(StringBuilder sb, string label, GameObject go)
    {
        sb.AppendLine($"NamedObject.{label}: {(go != null ? go.name : "MISSING")}");
    }
}
