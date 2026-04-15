using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Quick WebGL build utility - builds to the build/ folder in the project root.
/// Access via: Tools > Build WebGL (Quick)
/// </summary>
public static class QuickWebGLBuilder
{
    [MenuItem("Tools/Build WebGL (Quick)", priority = 1)]
    public static void BuildWebGL()
    {
        string buildPath = Path.Combine(Application.dataPath, "../build");
        buildPath = Path.GetFullPath(buildPath);

        Debug.Log($"[QuickWebGLBuilder] Starting WebGL build to: {buildPath}");

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/MainGame.unity" },
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(opts);
        var summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[QuickWebGLBuilder] Build succeeded! Size: {summary.totalSize / 1024 / 1024} MB");
        }
        else
        {
            Debug.LogError($"[QuickWebGLBuilder] Build FAILED: {summary.result}");
        }
    }

    [MenuItem("Tools/Build WebGL (Quick)", true)]
    public static bool BuildWebGLValidate()
    {
        return !EditorApplication.isPlaying && !EditorApplication.isCompiling;
    }
}
