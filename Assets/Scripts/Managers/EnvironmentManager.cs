using UnityEngine;
using System.Collections.Generic;
using ChickenCoop.Managers;

/// <summary>
/// EnvironmentManager - Handles world-space decorative elements like relative fencing
/// for chickens and corn fields. Replaces the old static perimeter fence.
/// Handles procedural decoration spawning based on zone templates.
/// </summary>
public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance { get; private set; }

    [Header("Fencing Settings")]
    [SerializeField] private string fenceResourcePath = "HappyHarvestFence";
    
    private GameObject fencePrefab;
    private Transform fenceContainer;
    private Dictionary<string, List<GameObject>> zoneDecorations = new Dictionary<string, List<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Create container for fences
        GameObject container = new GameObject("Environment_Root");
        fenceContainer = container.transform;
        fenceContainer.SetParent(transform);
    }

    private void Start()
    {
        LoadAssets();
        RefreshFences();
    }

    private void LoadAssets()
    {
        if (fencePrefab != null) return;
        fencePrefab = Resources.Load<GameObject>(fenceResourcePath);
        if (fencePrefab == null)
        {
            Debug.LogError($"[EnvironmentManager] FAILED to load fence prefab from Resources at '{fenceResourcePath}'. Procedural fences will not render.");
        }
    }

    /// <summary>
    /// Clears and rebuilds all pens based on current active zones in GameManager.
    /// </summary>
    public void RefreshFences()
    {
        ClearFences();

        if (GameManager.Instance == null) return;
        
        // Ensure prefab is loaded
        if (fencePrefab == null)
        {
            string path = GameManager.Instance.Config != null ? GameManager.Instance.Config.fenceResourcePath : "HappyHarvestFence";
            fencePrefab = Resources.Load<GameObject>(path);
            if (fencePrefab == null)
            {
                Debug.LogError($"[EnvironmentManager] Failed to load fence prefab from {path}");
                return;
            }
        }

        var zones = GameManager.Instance.ActiveZoneControllers;
        Debug.Log($"[EnvironmentManager] Refreshing fences for {zones.Count} zones.");

        foreach (var zone in zones)
        {
            Bounds bounds = zone.GetZoneBounds();
            if (bounds.size.magnitude < 0.1f) 
            {
                Debug.LogWarning($"[EnvironmentManager] Zone {zone.template.id} has invalid bounds {bounds.size}. Skipping fence.");
                continue;
            }

            BuildZoneFence(zone);
            SpawnZoneDecorations(zone);
        }
    }

    private void ClearFences()
    {
        if (fenceContainer == null) return;

        foreach (Transform child in fenceContainer)
        {
            if (Application.isPlaying) Object.Destroy(child.gameObject);
            else Object.DestroyImmediate(child.gameObject);
        }
        zoneDecorations.Clear();
    }

    private void BuildZoneFence(FarmZoneController zone)
    {
        Bounds b = zone.GetZoneBounds();
        CreateRectangularFence(
            new Vector2(b.min.x, b.min.y), 
            new Vector2(b.max.x, b.max.y), 
            $"{zone.template.id}_Fence"
        );
    }

    private void SpawnZoneDecorations(FarmZoneController zone)
    {
        if (zone.template.decorationPrefabs == null || zone.template.decorationPrefabs.Count == 0) return;

        Bounds b = zone.GetZoneBounds();
        GameObject decorRoot = new GameObject($"{zone.template.id}_Decorations");
        decorRoot.transform.SetParent(fenceContainer);

        // Procedural: place 2-3 decorations at random spots near corners
        int decorCount = Mathf.Min(zone.template.decorationPrefabs.Count, 3);
        for (int i = 0; i < decorCount; i++)
        {
            if (zone.template.decorationPrefabs[i] == null) continue;

            // Pick a corner-ish area but randomize slightly
            float randX = (i % 2 == 0) ? Random.Range(b.min.x + 0.5f, b.min.x + 1.5f) : Random.Range(b.max.x - 1.5f, b.max.x - 0.5f);
            float randY = (i < 2) ? Random.Range(b.min.y + 0.5f, b.min.y + 1.5f) : Random.Range(b.max.y - 1.5f, b.max.y - 0.5f);
            
            Vector3 pos = new Vector3(randX, randY, 0);

            GameObject decor = Instantiate(zone.template.decorationPrefabs[i], pos, Quaternion.identity, decorRoot.transform);
            decor.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-20f, 20f));
            
            // Ensure decorations are drawn behind characters
            var renderer = decor.GetComponentInChildren<SpriteRenderer>();
            if (renderer != null) renderer.sortingOrder = -50;
        }
    }

    private void CreateRectangularFence(Vector2 min, Vector2 max, string penName)
    {
        GameObject pen = new GameObject(penName);
        pen.transform.SetParent(fenceContainer);

        float step = 0.5f;

        // Top and Bottom rails
        for (float x = min.x; x <= max.x; x += step)
        {
            SpawnFencePost(new Vector3(x, min.y, 0), pen.transform);
            SpawnFencePost(new Vector3(x, max.y, 0), pen.transform);
        }

        // Left and Right rails
        for (float y = min.y + step; y < max.y; y += step)
        {
            SpawnFencePost(new Vector3(min.x, y, 0), pen.transform);
            SpawnFencePost(new Vector3(max.x, y, 0), pen.transform);
        }
    }

    private void SpawnFencePost(Vector3 position, Transform parent)
    {
        if (fencePrefab == null) return;
        
        GameObject post = Instantiate(fencePrefab, position, Quaternion.identity, parent);
        post.name = "Fence";
        
        // Ensure sorting behind characters (Y-sorted)
        var sGroup = post.GetComponent<UnityEngine.Rendering.SortingGroup>();
        if (sGroup == null) sGroup = post.AddComponent<UnityEngine.Rendering.SortingGroup>();
        sGroup.sortingOrder = -100;
    }
}
