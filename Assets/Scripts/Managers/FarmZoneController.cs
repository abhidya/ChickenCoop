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
        
        // Calculate grid dimensions for centering
        // For a max of 9 in 3 rows, we use 3x3 as the target bounding box
        float offsetX = (template.itemsPerRow - 1) * template.spacing.x * 0.5f;
        float offsetY = (Mathf.CeilToInt((float)template.maxSlots / template.itemsPerRow) - 1) * template.spacing.y * 0.5f;
        
        return transform.position + new Vector3((col * template.spacing.x) - offsetX, (row * template.spacing.y) - offsetY, 0);
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
        if (slots.Count == 0) return new Bounds(transform.position, Vector3.one * 2f);
        
        Bounds bounds = new Bounds(slots[0].position, Vector3.zero);
        foreach (var slot in slots)
        {
            bounds.Encapsulate(slot.position);
        }
        
        // Add padding from template
        bounds.Expand(template.zonePadding);
        return bounds;
    }
}
