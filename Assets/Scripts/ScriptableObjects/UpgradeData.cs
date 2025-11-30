using UnityEngine;

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
    /// Check if upgrade can be purchased
    /// </summary>
    public bool CanPurchase()
    {
        if (maxLevel > 0 && currentLevel >= maxLevel)
        {
            return false;
        }
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
