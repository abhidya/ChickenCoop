using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StorySceneVisualSetup
{
    private const string ScenePath = "Assets/Scenes/MainGame.unity";
    private const string PlayerPrefabPath = "Assets/HappyHarvest/Art/Characters/Prefab_character_base.prefab";
    private const string ChickenPrefabPath = "Assets/HappyHarvest/Art/Animals/Chicken/Prefab_Chicken.prefab";
    private const string CornPrefabPath = "Assets/HappyHarvest/Art/Crops/Corn/Prefabs/Prefab_Corn_04.prefab";
    private const string MarketPrefabPath = "Assets/HappyHarvest/Art/Environment/Market/Prefab_Market.prefab";

    [MenuItem("Tools/Story/Apply Happy Harvest Visuals")]
    public static void ApplyHappyHarvestVisualsMenu()
    {
        string report = ApplyHappyHarvestVisuals();
        Debug.Log(report);
        EditorUtility.DisplayDialog("Happy Harvest Visuals", report, "OK");
    }

    public static string ApplyHappyHarvestVisuals()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var lines = new List<string>();
        RemoveDuplicateRoots(lines);

        GameObject playerPrefab = LoadPrefab(PlayerPrefabPath, lines, "Player");
        GameObject chickenPrefab = LoadPrefab(ChickenPrefabPath, lines, "Chicken");
        GameObject cornPrefab = LoadPrefab(CornPrefabPath, lines, "Corn field");
        GameObject marketPrefab = LoadPrefab(MarketPrefabPath, lines, "Store");

        PlayerController player = Object.FindAnyObjectByType<PlayerController>();
        if (player != null && playerPrefab != null)
        {
            PrepareHost(player.transform, lines);
            SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
            GameObject visual = StoryVisualBinder.AttachVisualPrefabAsChild(player.transform, playerPrefab, renderer, "Visual");
            StoryVisualBinder.ApplySpriteLibrary(visual, "HappyHarvestFarmer");
            visual.transform.localScale = Vector3.one * 0.45f;
            visual.transform.localPosition = new Vector3(0f, -0.35f, 0f);
            lines.Add("Attached Happy Harvest farmer visual to Player.");
        }
        else
        {
            lines.Add("Player visual not updated.");
        }

        Chicken chicken = Object.FindAnyObjectByType<Chicken>();
        if (chicken != null && chickenPrefab != null)
        {
            PrepareHost(chicken.transform, lines);
            StoryVisualBinder.AttachVisualPrefabAsChild(chicken.transform, chickenPrefab, chicken.GetComponent<SpriteRenderer>(), "Visual");
            lines.Add("Attached Happy Harvest chicken visual.");
        }
        else
        {
            lines.Add("Chicken visual not updated.");
        }

        HarvestableField field = Object.FindAnyObjectByType<HarvestableField>();
        if (field != null && cornPrefab != null)
        {
            PrepareHost(field.transform, lines);
            StoryVisualBinder.AttachVisualPrefabAsChild(field.transform, cornPrefab, field.GetComponent<SpriteRenderer>(), "Visual");
            lines.Add("Attached Happy Harvest corn visual.");
        }
        else
        {
            lines.Add("Corn visual not updated.");
        }

        StoreCounter store = Object.FindAnyObjectByType<StoreCounter>();
        if (store != null && marketPrefab != null)
        {
            PrepareHost(store.transform, lines);
            StoryVisualBinder.AttachVisualPrefabAsChild(store.transform, marketPrefab, store.GetComponent<SpriteRenderer>(), "Visual");
            lines.Add("Attached Happy Harvest market visual.");
        }
        else
        {
            lines.Add("Store visual not updated.");
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        return string.Join("\n", lines);
    }

    private static void PrepareHost(Transform host, List<string> lines)
    {
        if (host == null)
        {
            return;
        }

        var toDelete = new List<GameObject>();
        for (int i = 0; i < host.childCount; i++)
        {
            Transform child = host.GetChild(i);
            string lowered = child.name.ToLowerInvariant();
            if (lowered == "visual" || lowered.EndsWith("_visual") || lowered.Contains("visual"))
            {
                toDelete.Add(child.gameObject);
                continue;
            }

            if (lowered == "eye" || lowered == "label")
            {
                toDelete.Add(child.gameObject);
            }
        }

        foreach (GameObject child in toDelete)
        {
            lines.Add($"Removed placeholder child '{child.name}' from {host.name}.");
            Object.DestroyImmediate(child);
        }
    }

    private static void RemoveDuplicateRoots(List<string> lines)
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        var byName = new Dictionary<string, List<GameObject>>();
        foreach (GameObject root in roots)
        {
            if (!byName.TryGetValue(root.name, out List<GameObject> list))
            {
                list = new List<GameObject>();
                byName[root.name] = list;
            }

            list.Add(root);
        }

        foreach (KeyValuePair<string, List<GameObject>> pair in byName)
        {
            if (pair.Value.Count <= 1)
            {
                continue;
            }

            List<GameObject> sorted = pair.Value
                .OrderByDescending(GetRootPriority)
                .ThenByDescending(root => root.GetComponents<Component>().Length)
                .ToList();

            for (int i = 1; i < sorted.Count; i++)
            {
                lines.Add($"Removed duplicate root '{sorted[i].name}'.");
                Object.DestroyImmediate(sorted[i]);
            }
        }

        GameObject legacyStore = roots.FirstOrDefault(root => root.name == "Store");
        GameObject storeCounter = roots.FirstOrDefault(root => root.name == "StoreCounter");
        if (legacyStore != null && storeCounter != null)
        {
            lines.Add("Removed legacy 'Store' root in favor of 'StoreCounter'.");
            Object.DestroyImmediate(legacyStore);
        }
    }

    private static int GetRootPriority(GameObject root)
    {
        if (root == null)
        {
            return 0;
        }

        if (root.GetComponent<PlayerController>() != null || root.GetComponent<Chicken>() != null || root.GetComponent<HarvestableField>() != null || root.GetComponent<StoreCounter>() != null)
        {
            return 100;
        }

        return root.GetComponents<Component>().Length;
    }

    private static GameObject LoadPrefab(string path, List<string> lines, string label)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            lines.Add($"Missing {label} visual prefab at {path}.");
        }

        return prefab;
    }
}
