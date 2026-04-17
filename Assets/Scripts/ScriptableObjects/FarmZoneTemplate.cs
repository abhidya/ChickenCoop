using UnityEngine;
using System.Collections.Generic;

public enum ZoneType
{
    Crop,
    Animal
}

[CreateAssetMenu(fileName = "NewZoneTemplate", menuName = "ChickenCoop/Zone Template")]
public class FarmZoneTemplate : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    public ZoneType zoneType;

    [Header("Assets")]
    public string slotObjectResourcePath;
    public List<GameObject> decorationPrefabs = new List<GameObject>();
    
    [Header("Production")]
    public FarmItemDefinition inputItem;
    public int inputAmount = 1;
    public FarmItemDefinition outputItem;
    public int outputAmount = 1;
    public float baseProductionTime = 5.0f;

    [Header("Grid Layout")]
    public Vector2 spacing = new Vector2(2.8f, 2.5f);
    public int itemsPerRow = 3;
    public int maxSlots = 9;
    public float zonePadding = 1.5f;
    
    [Header("Economy")]
    public int baseUnlockCost = 100;
    public int costPerAdditionalSlot = 50;

    public float GetTotalWidth()
    {
        int rows = Mathf.CeilToInt((float)maxSlots / itemsPerRow);
        int maxCols = rows > 1 ? itemsPerRow : maxSlots;
        return (maxCols - 1) * spacing.x + (zonePadding * 2);
    }

    public static FarmZoneTemplate CreateDefault(string zoneId)
    {
        var template = ScriptableObject.CreateInstance<FarmZoneTemplate>();
        template.id = zoneId;
        template.maxSlots = 9;
        template.itemsPerRow = 3;
        template.zonePadding = 2.0f;

        FarmItemDefinition corn = ScriptableObject.CreateInstance<FarmItemDefinition>();
        corn.id = "Corn"; corn.displayName = "Corn"; corn.basePrice = 5;
        FarmItemDefinition egg = ScriptableObject.CreateInstance<FarmItemDefinition>();
        egg.id = "Egg"; egg.displayName = "Egg"; egg.basePrice = 10;
        FarmItemDefinition wheat = ScriptableObject.CreateInstance<FarmItemDefinition>();
        wheat.id = "Wheat"; wheat.displayName = "Wheat"; wheat.basePrice = 15;
        FarmItemDefinition milk = ScriptableObject.CreateInstance<FarmItemDefinition>();
        milk.id = "Milk"; milk.displayName = "Milk"; milk.basePrice = 40;

        switch (zoneId)
        {
            case "Chicken":
                template.displayName = "Chicken Pen";
                template.zoneType = ZoneType.Animal;
                template.spacing = new Vector2(3.5f, 3.0f);
                template.slotObjectResourcePath = "HappyHarvestChicken";
                template.inputItem = corn;
                template.outputItem = egg;
                template.baseProductionTime = 5f;
                template.decorationPrefabs = LoadDecorations(new[] { "Env_Barrel", "Env_Bush" });
                break;
            case "Corn":
                template.displayName = "Corn Field";
                template.zoneType = ZoneType.Crop;
                template.spacing = new Vector2(2.8f, 2.5f);
                template.slotObjectResourcePath = "HappyHarvestCorn";
                template.outputItem = corn;
                template.baseProductionTime = 2f;
                template.decorationPrefabs = LoadDecorations(new[] { "Env_Rock_01", "Env_Flower_01" });
                break;
            case "Wheat":
                template.displayName = "Wheat Field";
                template.zoneType = ZoneType.Crop;
                template.spacing = new Vector2(2.8f, 2.5f);
                template.slotObjectResourcePath = "HappyHarvestWheat";
                template.outputItem = wheat;
                template.baseProductionTime = 4f;
                template.decorationPrefabs = LoadDecorations(new[] { "Prefab_Tool_Hoe", "Env_Scarecrow" });
                break;
            case "Cow":
                template.displayName = "Cow Pen";
                template.zoneType = ZoneType.Animal;
                template.spacing = new Vector2(4.0f, 3.5f);
                template.slotObjectResourcePath = "HappyHarvestCow";
                template.inputItem = wheat;
                template.outputItem = milk;
                template.baseProductionTime = 12f;
                template.decorationPrefabs = LoadDecorations(new[] { "Prop_Bucket_01", "Env_Barrel" });
                break;
        }

        return template;
    }

    private static List<GameObject> LoadDecorations(string[] prefabNames)
    {
        var list = new List<GameObject>();
        foreach (var name in prefabNames)
        {
            var prefab = Resources.Load<GameObject>(name);
            if (prefab != null) list.Add(prefab);
        }
        return list;
    }
}
