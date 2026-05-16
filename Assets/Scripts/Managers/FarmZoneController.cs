using UnityEngine;
using System.Collections.Generic;
using ChickenCoop.Interfaces;

/// <summary>
/// FarmZoneController - Runtime manager for a specific farm zone (e.g., "Corn_Zone_1").
/// </summary>
public class FarmZoneController : MonoBehaviour
{
    public FarmZoneTemplate template;
    private List<Transform> slots = new List<Transform>();
    private List<IZoneMember> members = new List<IZoneMember>();
    private Vector3 baseScale = Vector3.one;
    private bool baseScaleCached;
    private static Sprite fallbackMarkerSprite;

    public int CurrentCount => slots.Count;
    public List<Transform> Slots => slots;

    public void Initialize(FarmZoneTemplate template)
    {
        this.template = template;
        gameObject.name = "Zone_" + template.id;
        CacheBaseScale();
    }

    public Vector3 GetNextSlotPosition()
    {
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
        slots.Add(slotTransform);
        slotTransform.SetParent(transform);
        
        IZoneMember member = slotTransform.GetComponent<IZoneMember>();
        if (member != null)
        {
            member.Initialize(template.id, slots.Count - 1);
            members.Add(member);
        }
    }

    public Bounds GetZoneBounds()
    {
        if (slots.Count == 0)
        {
            // Empty zone: size based on current slot count potential (1 slot min)
            float width = template.spacing.x + template.zonePadding;
            float height = template.spacing.y + template.zonePadding;
            return new Bounds(transform.position, new Vector3(width, height, 1f));
        }
        
        Bounds bounds = new Bounds(slots[0].position, Vector3.zero);
        foreach (var slot in slots)
        {
            bounds.Encapsulate(slot.position);
        }
        
        // Add minimal padding - only enough for fence, not full max grid
        float padX = Mathf.Max(template.spacing.x * 0.5f, template.zonePadding * 0.3f);
        float padY = Mathf.Max(template.spacing.y * 0.5f, template.zonePadding * 0.3f);
        bounds.Expand(new Vector3(padX, padY, 0));
        return bounds;
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
