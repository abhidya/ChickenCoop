using UnityEngine;
using System.Collections.Generic;
using ChickenCoop.Managers;

namespace ChickenCoop.Managers
{
    /// <summary>
    /// EnvironmentManager - Handles world-space decorative elements like relative fencing
    /// for chickens and corn fields. Replaces the old static perimeter fence.
    /// </summary>
    public class EnvironmentManager : MonoBehaviour
    {
        public static EnvironmentManager Instance { get; private set; }

        public Bounds ChickenPenBounds { get; private set; }

        [Header("Fencing Settings")]
        [SerializeField] private string fenceResourcePath = "HappyHarvestFence";
        [SerializeField] private float chickenPenPadding = 2f;
        [SerializeField] private float cornFieldPadding = 1.5f;
        
        private GameObject fencePrefab;
        private Transform fenceContainer;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Create container for fences
            GameObject container = new GameObject("FenceContainer");
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
            if (fencePrefab == null)
            {
                Debug.LogWarning("[EnvironmentManager] Could not find HappyHarvestFence in Resources.");
            }
        }

        /// <summary>
        /// Clears and rebuilds all pens based on current game state
        /// </summary>
        public void RefreshFences()
        {
            if (fenceContainer == null) return;

            // Clear existing
            foreach (Transform child in fenceContainer)
            {
                if (Application.isPlaying) Object.Destroy(child.gameObject);
                else Object.DestroyImmediate(child.gameObject);
            }

            BuildChickenPen();
            BuildCornPens();
        }

        private void BuildChickenPen()
        {
            Chicken[] chickens = Object.FindObjectsOfType<Chicken>();
            if (chickens.Length == 0) return;

            // Calculate bounding box of all chickens
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            foreach (var c in chickens)
            {
                Vector3 pos = c.transform.position;
                minX = Mathf.Min(minX, pos.x);
                maxX = Mathf.Max(maxX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxY = Mathf.Max(maxY, pos.y);
            }

            // Add padding based on chicken count (pen grows with more chickens)
            float dynamicPadding = chickenPenPadding + (chickens.Length * 0.2f);
            
            Vector2 min = new Vector2(minX - dynamicPadding, minY - dynamicPadding);
            Vector2 max = new Vector2(maxX + dynamicPadding, maxY + dynamicPadding);

            ChickenPenBounds = new Bounds(
                new Vector3((min.x + max.x) / 2f, (min.y + max.y) / 2f, 0),
                new Vector3(max.x - min.x, max.y - min.y, 10f)
            );

            CreateRectangularFence(min, max, "ChickenPen");
        }

        private void BuildCornPens()
        {
            HarvestableField[] fields = Object.FindObjectsOfType<HarvestableField>();
            if (fields.Length == 0) return;

            // Group adjacent fields? For now, buffered boxes per user request:
            // "boxed in adjacently but with a buffer"
            // If they are adjacent, we can treat them as one cluster.
            
            // Simplest: Find clusters or just box the whole corn area if they are close.
            // Let's box the whole corn area for stability unless they are far apart.
            
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;

            foreach (var f in fields)
            {
                Vector3 pos = f.transform.position;
                minX = Mathf.Min(minX, pos.x);
                maxX = Mathf.Max(maxX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxY = Mathf.Max(maxY, pos.y);
            }

            CreateRectangularFence(
                new Vector2(minX - cornFieldPadding, minY - cornFieldPadding),
                new Vector2(maxX + cornFieldPadding, maxY + cornFieldPadding),
                "CornRegionPen"
            );
        }

        private void CreateRectangularFence(Vector2 min, Vector2 max, string penName)
        {
            GameObject pen = new GameObject(penName);
            pen.transform.SetParent(fenceContainer);

            // Top and Bottom
            for (float x = min.x; x <= max.x; x += 0.5f)
            {
                SpawnFencePost(new Vector3(x, min.y, 0), pen.transform);
                SpawnFencePost(new Vector3(x, max.y, 0), pen.transform);
            }

            // Left and Right
            for (float y = min.y + 0.5f; y < max.y; y += 0.5f)
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
            
            // Ensure sorting
            var sGroup = post.GetComponent<UnityEngine.Rendering.SortingGroup>();
            if (sGroup == null) sGroup = post.AddComponent<UnityEngine.Rendering.SortingGroup>();
            sGroup.sortingOrder = -100; // Behind players usually, but Y-sorted
        }
    }
}
