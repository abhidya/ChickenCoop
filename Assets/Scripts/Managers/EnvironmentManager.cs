using UnityEngine;
using System.Collections.Generic;

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
        fencePrefab = Resources.Load<GameObject>(fenceResourcePath);
    }

    /// <summary>
    /// Clears and rebuilds all pens based on current active zones in GameManager.
    /// </summary>
    public void RefreshFences()
    {
        if (fenceContainer == null || GameManager.Instance == null) return;

        // Clear existing
        foreach (Transform child in fenceContainer)
        {
            if (Application.isPlaying) Object.Destroy(child.gameObject);
            else Object.DestroyImmediate(child.gameObject);
        }
        zoneDecorations.Clear();

        // Build individual fences for each zone controller
        foreach (var zone in GameManager.Instance.ActiveZoneControllers)
        {
            BuildZoneFence(zone);
            SpawnZoneDecorations(zone);
        }
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

        // Procedural: place decorations at corners
        int decorCount = Mathf.Min(zone.template.decorationPrefabs.Count, 3);
        for (int i = 0; i < decorCount; i++)
        {
            Vector3 pos;
            if (i == 0) pos = new Vector3(b.min.x + 0.5f, b.max.y - 0.5f, 0);      // Top Left
            else if (i == 1) pos = new Vector3(b.max.x - 0.5f, b.min.y + 0.5f, 0); // Bottom Right
            else pos = new Vector3(b.max.x - 0.5f, b.max.y - 0.5f, 0);              // Top Right

            GameObject decor = Instantiate(zone.template.decorationPrefabs[i], pos, Quaternion.identity, decorRoot.transform);
            decor.transform.rotation = Quaternion.Euler(0, 0, (i % 2 == 0) ? 15f : -15f);
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
