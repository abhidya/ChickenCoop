using UnityEngine;
using System.Collections.Generic;
using ChickenCoop.Managers;

/// <summary>
/// UpgradeData - ScriptableObject for configuring farm upgrades.
/// Allows easy modification of upgrade parameters without code changes.
/// </summary>
[CreateAssetMenu(fileName = "NewUpgrade", menuName = "ChickenCoop/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Upgrade Info")]
    [Tooltip("Display name for the upgrade")]
    public string upgradeName;

    [Tooltip("Description shown to player")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("Icon for the upgrade button")]
    public Sprite icon;

    [Header("Cost")]
    [Tooltip("Base cost in coins")]
    public int baseCost = 100;

    [Tooltip("Cost multiplier per level")]
    public float costMultiplier = 1.5f;

    [Tooltip("Maximum upgrade level (0 = unlimited)")]
    public int maxLevel = 5;

    [Header("Effect")]
    [Tooltip("Type of upgrade")]
    public UpgradeType upgradeType;

    [Tooltip("Multiplier applied per level")]
    public float effectMultiplier = 1.2f;

    [Tooltip("Flat bonus added per level")]
    public int flatBonus = 0;

    [Header("Tree Progression")]
    [Tooltip("The game act required to unlock this upgrade (1-4)")]
    public int requiredAct = 1;

    [Tooltip("Other upgrades that must be maxed before this appears")]
    public List<UpgradeData> prerequisiteUpgrades = new List<UpgradeData>();

    [Tooltip("If true, upgrade is removed from shop once maxed")]
    public bool hideWhenMaxed = true;

    [Header("Visual")]
    [Tooltip("Color tint for upgrade UI")]
    public Color tintColor = Color.white;

    [Tooltip("Particle effect prefab for upgrade")]
    public GameObject upgradeParticlePrefab;

    // Current level (not saved in ScriptableObject, tracked at runtime)
    [System.NonSerialized]
    public int currentLevel = 0;

    /// <summary>
    /// Calculate cost for next upgrade level
    /// </summary>
    public int GetCost()
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }

    /// <summary>
    /// Calculate total effect multiplier at current level
    /// </summary>
    public float GetTotalMultiplier()
    {
        return Mathf.Pow(effectMultiplier, currentLevel);
    }

    /// <summary>
    /// Calculate total flat bonus at current level
    /// </summary>
    public int GetTotalFlatBonus()
    {
        return flatBonus * currentLevel;
    }

    /// <summary>
    /// Checks if all prerequisite upgrades are maxed out
    /// </summary>
    public bool ArePrerequisitesMet()
    {
        if (prerequisiteUpgrades == null || prerequisiteUpgrades.Count == 0) return true;
        
        foreach (var pre in prerequisiteUpgrades)
        {
            if (pre == null) continue;
            if (pre.maxLevel > 0 && pre.currentLevel < pre.maxLevel) return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if this upgrade should be visible in the shop
    /// </summary>
    public bool IsVisible()
    {
        // Hide if maxed out and setting is enabled
        if (hideWhenMaxed && maxLevel > 0 && currentLevel >= maxLevel) return false;
        
        // Check Act requirement
        if (GameManager.Instance != null && GameManager.Instance.CurrentAct < requiredAct) return false;
        
        // Show if prerequisites are met
        return ArePrerequisitesMet();
    }

    /// <summary>
    /// Check if upgrade can be purchased
    /// </summary>
    public bool CanPurchase()
    {
        if (maxLevel > 0 && currentLevel >= maxLevel)
        {
            return false;
        }

        if (!ArePrerequisitesMet()) return false;
        if (GameManager.Instance != null && GameManager.Instance.CurrentAct < requiredAct) return false;

        return GameManager.Instance != null && GameManager.Instance.CanAfford(GetCost());
    }

    /// <summary>
    /// Purchase the upgrade
    /// </summary>
    public bool Purchase()
    {
        if (!CanPurchase()) return false;

        if (GameManager.Instance.SpendCoins(GetCost()))
        {
            currentLevel++;
            GameManager.Instance.ApplyUpgrade(upgradeType, effectMultiplier);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Reset upgrade level (for new game)
    /// </summary>
    public void Reset()
    {
        currentLevel = 0;
    }
}
