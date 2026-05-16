using System;
using System.Collections.Generic;
using UnityEngine;
using ChickenCoop.Managers;

[Serializable]
public class ZoneVisualState
{
    public string zoneId;
    public string markerName = "ProgressionMarker";
    public string resourcePath;
    public string label;
    public Color tint = Color.white;
    public Vector3 localOffset = Vector3.zero;
    public Vector3 localScale = Vector3.one;
    public float pulseStrength = 0f;
}

[Serializable]
public class ProductVisualState
{
    public string itemId;
    public string markerName = "ProductMarker";
    public string resourcePath;
    public Color tint = Color.white;
    public Vector3 localOffset = Vector3.zero;
    public Vector3 localScale = Vector3.one;
    public float glowStrength = 0f;
}

[Serializable]
public class HelperVisualState
{
    public Color tint = Color.white;
    public Vector3 localScale = Vector3.one;
    public bool showStepDust = false;
    public bool showAura = false;
    public string badgeText;
    public string auraMarkerName = "HelperAura";
}

[Serializable]
public class ChickenVisualState
{
    public Color tint = Color.white;
    public Vector3 localScale = Vector3.one;
    public string nestLabel;
    public string markerName = "ChickenVisualState";
    public float pulseStrength = 0f;
}

[Serializable]
public class PigVisualState
{
    public Color tint = Color.white;
    public Vector3 localScale = Vector3.one;
    public string mudLabel;
    public string markerName = "PigVisualState";
    public float pulseStrength = 0f;
}

[Serializable]
public class StoreVisualState
{
    public int tier = 0;
    public Color tint = Color.white;
    public Vector3 localScale = Vector3.one;
    public string markerName = "StoreProgression";
    public string[] resourcePaths = Array.Empty<string>();
    public string badgeText;
}

public enum VisualMutationKind
{
    ZoneSpawn,
    ZoneProp,
    ProductStyle,
    HelperStyle,
    StoreStyle,
    UiBadge
}

[Serializable]
public class UpgradeVisualMutation
{
    public UpgradeType upgradeType;
    public int requiredLevel = 1;
    public VisualMutationKind kind;
    public string targetId;
    public string resourcePath;
    public string label;
    public string markerName = "ProgressionMutation";
    public Color tint = Color.white;
    public Vector3 localOffset = Vector3.zero;
    public Vector3 localScale = Vector3.one;
    public float pulseStrength = 0f;
    public bool spawnZoneMember = false;
    public bool unlockResourceSlot = false;
    public string resourceSlotId;
}

[CreateAssetMenu(fileName = "UpgradeVisualMutationData", menuName = "ChickenCoop/Visual Progression Data")]
public class UpgradeVisualMutationData : ScriptableObject
{
    [SerializeField] private List<UpgradeVisualMutation> mutations = new List<UpgradeVisualMutation>();

    public IReadOnlyList<UpgradeVisualMutation> Mutations => mutations;

    public static UpgradeVisualMutationData CreateDefault()
    {
        UpgradeVisualMutationData data = CreateInstance<UpgradeVisualMutationData>();
        data.PopulateDefaultMutations();
        return data;
    }

    public void PopulateDefaultMutations()
    {
        mutations.Clear();

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.WheatField,
            requiredLevel = 1,
            kind = VisualMutationKind.ZoneSpawn,
            targetId = "Wheat",
            label = "Wheat field unlocked",
            resourcePath = "Sprite_Button_green",
            tint = new Color(0.98f, 0.92f, 0.45f),
            spawnZoneMember = true,
            unlockResourceSlot = true,
            resourceSlotId = "Wheat",
            pulseStrength = 0.16f
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.ChickenCare,
            requiredLevel = 1,
            kind = VisualMutationKind.ZoneProp,
            targetId = "Chicken",
            markerName = "ChickenNest",
            label = "Chicken care upgraded",
            tint = new Color(0.96f, 0.86f, 0.55f),
            localOffset = new Vector3(0f, -0.45f, 0f),
            localScale = new Vector3(1.4f, 0.6f, 1f),
            pulseStrength = 0.08f
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.ChickenCare,
            requiredLevel = 1,
            kind = VisualMutationKind.ProductStyle,
            targetId = "Egg",
            markerName = "EggPremiumTier",
            label = "Premium eggs",
            tint = new Color(1f, 0.97f, 0.82f),
            localScale = new Vector3(1.16f, 1.16f, 1f),
            pulseStrength = 0.18f
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.ChickenCare,
            requiredLevel = 1,
            kind = VisualMutationKind.UiBadge,
            targetId = "Egg",
            label = "Chicken care improved",
            tint = new Color(1f, 0.97f, 0.82f)
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.CowPen,
            requiredLevel = 1,
            kind = VisualMutationKind.ZoneSpawn,
            targetId = "Cow",
            label = "Cow pen unlocked",
            resourcePath = "Sprite_Button_Blue",
            tint = new Color(0.91f, 0.87f, 0.80f),
            spawnZoneMember = true,
            unlockResourceSlot = true,
            resourceSlotId = "Milk",
            pulseStrength = 0.16f
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.CowFeed,
            requiredLevel = 1,
            kind = VisualMutationKind.ZoneProp,
            targetId = "Cow",
            markerName = "CowTrough",
            label = "Cow feed upgraded",
            tint = new Color(0.92f, 0.84f, 0.65f),
            localOffset = new Vector3(0.45f, -0.5f, 0f),
            localScale = new Vector3(0.9f, 0.55f, 1f),
            pulseStrength = 0.06f
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.MilkProduction,
            requiredLevel = 1,
            kind = VisualMutationKind.ProductStyle,
            targetId = "Milk",
            markerName = "MilkPailTier",
            label = "Milk output upgraded",
            resourcePath = "Sprite_Button_Blue",
            tint = new Color(0.98f, 0.98f, 1f),
            localScale = new Vector3(1.12f, 1.12f, 1f),
            pulseStrength = 0.18f
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.MilkProduction,
            requiredLevel = 1,
            kind = VisualMutationKind.UiBadge,
            targetId = "Milk",
            label = "Milk output improved",
            tint = new Color(0.98f, 0.98f, 1f)
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.CarrotGarden,
            requiredLevel = 1,
            kind = VisualMutationKind.ZoneSpawn,
            targetId = "Carrot",
            label = "Carrot garden unlocked",
            resourcePath = "Sprite_Button_green",
            tint = new Color(1f, 0.60f, 0.22f),
            spawnZoneMember = true,
            unlockResourceSlot = true,
            resourceSlotId = "Carrot",
            pulseStrength = 0.16f
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.Fertilizer,
            requiredLevel = 1,
            kind = VisualMutationKind.ZoneProp,
            targetId = "Corn",
            markerName = "FertilizerCorn",
            label = "Fertilizer applied",
            tint = new Color(0.64f, 0.88f, 0.50f),
            localOffset = new Vector3(0f, -0.30f, 0f),
            localScale = new Vector3(1.5f, 0.55f, 1f),
            pulseStrength = 0.14f
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.Fertilizer,
            requiredLevel = 1,
            kind = VisualMutationKind.UiBadge,
            targetId = "Corn",
            label = "Fields look richer",
            tint = new Color(0.64f, 0.88f, 0.50f)
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.Fertilizer,
            requiredLevel = 1,
            kind = VisualMutationKind.ZoneProp,
            targetId = "Wheat",
            markerName = "FertilizerWheat",
            label = "Fertilizer applied",
            tint = new Color(0.64f, 0.88f, 0.50f),
            localOffset = new Vector3(0f, -0.30f, 0f),
            localScale = new Vector3(1.5f, 0.55f, 1f),
            pulseStrength = 0.14f
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.Fertilizer,
            requiredLevel = 1,
            kind = VisualMutationKind.ZoneProp,
            targetId = "Carrot",
            markerName = "FertilizerCarrot",
            label = "Fertilizer applied",
            tint = new Color(0.64f, 0.88f, 0.50f),
            localOffset = new Vector3(0f, -0.30f, 0f),
            localScale = new Vector3(1.5f, 0.55f, 1f),
            pulseStrength = 0.14f
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.PigPen,
            requiredLevel = 1,
            kind = VisualMutationKind.ZoneSpawn,
            targetId = "Pig",
            label = "Pig pen unlocked",
            resourcePath = "Sprite_Button_Blue",
            tint = new Color(0.88f, 0.79f, 0.72f),
            spawnZoneMember = true,
            unlockResourceSlot = true,
            resourceSlotId = "Truffle",
            pulseStrength = 0.16f
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.PigPen,
            requiredLevel = 1,
            kind = VisualMutationKind.ZoneProp,
            targetId = "Pig",
            markerName = "PigMudPatch",
            label = "Mud patch added",
            tint = new Color(0.48f, 0.34f, 0.22f),
            localOffset = new Vector3(0.25f, -0.52f, 0f),
            localScale = new Vector3(1.4f, 0.7f, 1f),
            pulseStrength = 0.06f
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.HelperSpeed,
            requiredLevel = 1,
            kind = VisualMutationKind.HelperStyle,
            targetId = "Helpers",
            markerName = "HelperSpeedAura",
            label = "Helpers boosted",
            tint = new Color(1f, 0.90f, 0.35f),
            localScale = new Vector3(1.06f, 1.06f, 1f),
            pulseStrength = 0.14f,
            unlockResourceSlot = false
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.BiggerStore,
            requiredLevel = 1,
            kind = VisualMutationKind.StoreStyle,
            targetId = "Store",
            markerName = "StoreTierBadge",
            label = "Store expanded",
            tint = new Color(1f, 0.86f, 0.34f),
            localScale = new Vector3(1.18f, 1.18f, 1f),
            pulseStrength = 0.18f
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.WheatField,
            requiredLevel = 1,
            kind = VisualMutationKind.UiBadge,
            targetId = "Wheat",
            label = "New crop unlocked",
            tint = new Color(0.98f, 0.93f, 0.45f)
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.CowPen,
            requiredLevel = 1,
            kind = VisualMutationKind.UiBadge,
            targetId = "Milk",
            label = "Cow pen unlocked",
            tint = new Color(0.91f, 0.87f, 0.80f)
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.CarrotGarden,
            requiredLevel = 1,
            kind = VisualMutationKind.UiBadge,
            targetId = "Carrot",
            label = "Carrot garden unlocked",
            tint = new Color(1f, 0.60f, 0.22f)
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.PigPen,
            requiredLevel = 1,
            kind = VisualMutationKind.UiBadge,
            targetId = "Truffle",
            label = "Pig pen unlocked",
            tint = new Color(0.88f, 0.79f, 0.72f)
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.HelperSpeed,
            requiredLevel = 1,
            kind = VisualMutationKind.UiBadge,
            targetId = "Helpers",
            label = "Helpers boosted",
            tint = new Color(1f, 0.90f, 0.35f)
        });

        mutations.Add(new UpgradeVisualMutation
        {
            upgradeType = UpgradeType.BiggerStore,
            requiredLevel = 1,
            kind = VisualMutationKind.UiBadge,
            targetId = "Store",
            label = "Store expanded",
            tint = new Color(1f, 0.86f, 0.34f)
        });
    }
}
