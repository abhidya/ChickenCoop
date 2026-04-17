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

    public int CurrentCount => slots.Count;
    public List<Transform> Slots => slots;

    public void Initialize(FarmZoneTemplate template)
    {
        this.template = template;
        gameObject.name = "Zone_" + template.id;
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
}
