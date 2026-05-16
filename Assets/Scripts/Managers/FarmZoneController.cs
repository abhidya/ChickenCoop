using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ChickenCoop.Interfaces;

/// <summary>
/// FarmZoneController - Runtime manager for a specific farm zone (e.g., "Corn_Zone_1").
/// </summary>
public class FarmZoneController : MonoBehaviour
{
    public FarmZoneTemplate template;
    [SerializeField] private string zoneId;
    [SerializeField] private Transform slotRoot;
    [SerializeField] private Transform decorRoot;
    [SerializeField] private Transform fenceRoot;
    [SerializeField] private GameObject lockedVisual;
    [SerializeField] private GameObject unlockedVisual;
    [SerializeField] private Transform productRoot;
    [SerializeField] private List<Transform> authoredSlots = new List<Transform>();
    private List<Transform> slots = new List<Transform>();
    private List<IZoneMember> members = new List<IZoneMember>();
    private Vector3 baseScale = Vector3.one;
    private bool baseScaleCached;
    private bool rootSeededAsMember;
    private static Sprite fallbackMarkerSprite;

    public int CurrentCount => slots.Count;
    public List<Transform> Slots => slots;
    public string ZoneId => !string.IsNullOrWhiteSpace(zoneId) ? zoneId : template != null ? template.id : string.Empty;
    public Transform SlotRoot => slotRoot;
    public Transform DecorRoot => decorRoot;
    public Transform FenceRoot => fenceRoot;
    public Transform ProductRoot => productRoot;

    public void Initialize(FarmZoneTemplate template)
    {
        this.template = template;
        zoneId = template != null ? template.id : zoneId;
        gameObject.name = "Zone_" + ZoneId;
        CacheBaseScale();
        RefreshAuthoredSlots();
        SeedRootAsMemberIfNeeded();
    }

    private void Start()
    {
        SeedRootAsMemberIfNeeded();
    }

    private void OnValidate()
    {
        EnsureTemplate();
        RefreshAuthoredSlots();
        SeedRootAsMemberIfNeeded();
    }

    public void EnsureTemplate()
    {
        if (template != null || string.IsNullOrWhiteSpace(ZoneId))
        {
            return;
        }

        template = FarmZoneTemplate.CreateDefault(ZoneId);
    }

    public bool ZoneIdMatches(string candidate)
    {
        return !string.IsNullOrWhiteSpace(candidate) &&
               !string.IsNullOrWhiteSpace(ZoneId) &&
               string.Equals(ZoneId, candidate, System.StringComparison.OrdinalIgnoreCase);
    }

    public void RefreshAuthoredSlots()
    {
        EnsureTemplate();
        authoredSlots.RemoveAll(slot => slot == null);

        if (slotRoot == null)
        {
            slotRoot = transform.Find("SlotRoot") ?? transform.Find("Slots") ?? transform.Find("PlacementSlots");
        }

        if (decorRoot == null)
        {
            decorRoot = transform.Find("DecorRoot") ?? transform.Find("Decor") ?? transform.Find("DecorationRoot");
        }

        if (fenceRoot == null)
        {
            fenceRoot = transform.Find("FenceRoot") ?? transform.Find("Fences") ?? transform.Find("Fence");
        }

        if (productRoot == null)
        {
            productRoot = transform.Find("ProductRoot") ?? transform.Find("Products");
        }

        if (slotRoot == null)
        {
            return;
        }

        foreach (Transform child in slotRoot)
        {
            if (child == null || authoredSlots.Contains(child))
            {
                continue;
            }

            authoredSlots.Add(child);
        }
    }

    public void SeedRootAsMemberIfNeeded()
    {
        if (rootSeededAsMember || slots.Count > 0)
        {
            return;
        }

        rootSeededAsMember = true;
        slots.Add(transform);
    }

    public Transform GetNextAvailableAuthoredSlot()
    {
        RefreshAuthoredSlots();
        return authoredSlots.FirstOrDefault(slot => slot != null && !IsSlotOccupied(slot));
    }

    public bool HasAuthoredSlots()
    {
        RefreshAuthoredSlots();
        return authoredSlots.Count > 0;
    }

    private bool IsSlotOccupied(Transform slot)
    {
        if (slot == null)
        {
            return true;
        }

        return slots.Any(member => member != null && (member == slot || member.parent == slot));
    }

    public Vector3 GetNextSlotPosition()
    {
        EnsureTemplate();
        Transform authoredSlot = GetNextAvailableAuthoredSlot();
        if (authoredSlot != null)
        {
            return authoredSlot.position;
        }

        int index = slots.Count;
        int col = index % template.itemsPerRow;
        int row = index / template.itemsPerRow;
        
        // For 3 items per row (3x3 grid), col ranges 0-2
        // Grid spans (itemsPerRow - 1) * spacing centered on zone position
        int totalCols = Mathf.Min(template.itemsPerRow, Mathf.Max(template.maxSlots, 1));
        float totalWidth = (totalCols - 1) * template.spacing.x;
        
        // Calculate offset to center the grid
        float offsetX = col * template.spacing.x - (totalWidth * 0.5f);
        float offsetY = row * template.spacing.y;
        
        return transform.position + new Vector3(offsetX, offsetY, 0);
    }

    public void AddSlot(Transform slotTransform)
    {
        if (slotTransform == null)
        {
            return;
        }

        slots.Add(slotTransform);

        IZoneMember member = slotTransform.GetComponent<IZoneMember>();
        if (member != null)
        {
            member.Initialize(ZoneId, slots.Count - 1);
            members.Add(member);
        }
    }

    public Bounds GetZoneBounds()
    {
        EnsureTemplate();
        List<Transform> boundsPoints = new List<Transform>();
        boundsPoints.AddRange(slots.Where(slot => slot != null));
        foreach (Transform authoredSlot in authoredSlots)
        {
            if (authoredSlot != null && !boundsPoints.Contains(authoredSlot))
            {
                boundsPoints.Add(authoredSlot);
            }
        }

        bool hasFenceBounds = false;
        Bounds fenceBounds = new Bounds();
        if (fenceRoot != null)
        {
            Renderer[] fenceRenderers = fenceRoot.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer fenceRenderer in fenceRenderers)
            {
                if (fenceRenderer == null || !fenceRenderer.enabled)
                {
                    continue;
                }

                if (!hasFenceBounds)
                {
                    fenceBounds = fenceRenderer.bounds;
                    hasFenceBounds = true;
                }
                else
                {
                    fenceBounds.Encapsulate(fenceRenderer.bounds.min);
                    fenceBounds.Encapsulate(fenceRenderer.bounds.max);
                }
            }
        }

        if (boundsPoints.Count == 0)
        {
            if (hasFenceBounds)
            {
                float fencePad = Mathf.Max(template.spacing.x, template.spacing.y) * 0.15f;
                fenceBounds.Expand(new Vector3(fencePad, fencePad, 0f));
                return fenceBounds;
            }

            if (authoredSlots.Count > 0)
            {
                Bounds authoredBounds = new Bounds(authoredSlots[0].position, Vector3.zero);
                foreach (Transform slot in authoredSlots)
                {
                    if (slot != null)
                    {
                        authoredBounds.Encapsulate(slot.position);
                    }
                }

                authoredBounds.Expand(Vector3.one * template.zonePadding);
                return authoredBounds;
            }

            float width = template.spacing.x + template.zonePadding;
            float height = template.spacing.y + template.zonePadding;
            return new Bounds(transform.position, new Vector3(width, height, 1f));
        }

        Bounds bounds = new Bounds(boundsPoints[0].position, Vector3.zero);
        foreach (var slot in boundsPoints)
        {
            bounds.Encapsulate(slot.position);
        }

        if (hasFenceBounds)
        {
            bounds.Encapsulate(fenceBounds.min);
            bounds.Encapsulate(fenceBounds.max);
        }
        
        // Add minimal padding - only enough for fence, not full max grid
        float padX = Mathf.Max(template.spacing.x * 0.5f, template.zonePadding * 0.3f);
        float padY = Mathf.Max(template.spacing.y * 0.5f, template.zonePadding * 0.3f);
        bounds.Expand(new Vector3(padX, padY, 0));
        return bounds;
    }

    public void SetVisualRoots(Transform slotRoot, Transform decorRoot, Transform fenceRoot, Transform productRoot)
    {
        this.slotRoot = slotRoot;
        this.decorRoot = decorRoot;
        this.fenceRoot = fenceRoot;
        this.productRoot = productRoot;
        RefreshAuthoredSlots();
    }

    public void ApplyVisualState(ZoneVisualState state)
    {
        if (state == null)
        {
            return;
        }

        CacheBaseScale();
        transform.localScale = baseScale;

        if (!string.IsNullOrWhiteSpace(state.markerName))
        {
            Transform marker = transform.Find(state.markerName);
            if (marker == null)
            {
                GameObject markerObject = new GameObject(state.markerName);
                markerObject.transform.SetParent(transform, false);
                marker = markerObject.transform;
            }

            marker.localPosition = state.localOffset;
            marker.localScale = state.localScale;

            SpriteRenderer renderer = marker.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = marker.gameObject.AddComponent<SpriteRenderer>();
            }

            if (!string.IsNullOrWhiteSpace(state.resourcePath))
            {
                Sprite sprite = Resources.Load<Sprite>(state.resourcePath);
                if (sprite != null)
                {
                    renderer.sprite = sprite;
                }
            }

            if (renderer.sprite == null)
            {
                renderer.sprite = CreateFallbackMarkerSprite();
            }

            renderer.color = state.tint;
            renderer.sortingOrder = 990;
        }

        if (!string.IsNullOrWhiteSpace(state.label))
        {
            Transform label = transform.Find(state.label + "_Label");
            if (label == null)
            {
                GameObject labelObject = new GameObject(state.label + "_Label");
                labelObject.transform.SetParent(transform, false);
                label = labelObject.transform;
            }

            label.localPosition = state.localOffset + new Vector3(0f, 0.7f, 0f);
            label.localScale = Vector3.one * 0.45f;

            TextMesh textMesh = label.GetComponent<TextMesh>();
            if (textMesh == null)
            {
                textMesh = label.gameObject.AddComponent<TextMesh>();
                textMesh.fontSize = 24;
                textMesh.alignment = TextAlignment.Center;
                textMesh.anchor = TextAnchor.MiddleCenter;
            }

            textMesh.text = state.label;
            textMesh.color = state.tint;
            label.GetComponent<MeshRenderer>().sortingOrder = 995;
        }
    }

    private void CacheBaseScale()
    {
        if (baseScaleCached)
        {
            return;
        }

        baseScaleCached = true;
        baseScale = transform.localScale;
    }

    private static Sprite CreateFallbackMarkerSprite()
    {
        if (fallbackMarkerSprite != null)
        {
            return fallbackMarkerSprite;
        }

        Texture2D texture = new Texture2D(24, 24, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(11.5f, 11.5f);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float dx = (x - center.x) / 9f;
                float dy = (y - center.y) / 9f;
                texture.SetPixel(x, y, dx * dx + dy * dy <= 1f ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        fallbackMarkerSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 24f);
        return fallbackMarkerSprite;
    }
}
