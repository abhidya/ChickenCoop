using UnityEngine;
using System.Collections.Generic;

public enum ZoneType
{
    Crop,
    Animal
}

/// <summary>
/// FarmZoneTemplate - Defines a template for a harvestable area (e.g. Corn Field, Chicken Pen).
/// </summary>
[CreateAssetMenu(fileName = "NewZoneTemplate", menuName = "ChickenCoop/Zone Template")]
public class FarmZoneTemplate : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    public ZoneType zoneType;

    [Header("Assets")]
    public GameObject mainPrefab;
    public List<GameObject> decorationPrefabs;
    
    [Header("Production Cycle")]
    public FarmItemDefinition inputItem; // e.g. Corn needed to feed
    public int inputAmount = 1;
    public FarmItemDefinition outputItem; // e.g. Egg produced
    public int outputAmount = 1;
    public float baseProductionTime = 5.0f;

    [Header("Layout & Grid")]
    public Vector2 spacing = new Vector2(1.8f, 1.4f);
    public int itemsPerRow = 3;
    public int maxSlots = 6;
    public float zonePadding = 2.0f; // Buffer around the zone
    
    [Header("Economy")]
    public int baseUnlockCost = 100;
    public int costPerAdditionalSlot = 50;

    /// <summary>
    /// Calculates the width required for this zone when maxed out.
    /// </summary>
    public float GetTotalWidth()
    {
        int rows = Mathf.CeilToInt((float)maxSlots / itemsPerRow);
        int maxCols = rows > 1 ? itemsPerRow : maxSlots;
        
        return (maxCols - 1) * spacing.x + (zonePadding * 2);
    }
}
