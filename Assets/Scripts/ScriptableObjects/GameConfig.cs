using UnityEngine;

/// <summary>
/// GameConfig - ScriptableObject for game-wide configuration settings.
/// Centralizes game balance values for easy tuning.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "ChickenCoop/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Starting Resources")]
    public int startingCorn = 0;
    public int startingEggs = 0;
    public int startingCoins = 50;

    [Header("Production Values")]
    [Tooltip("Corn harvested per click")]
    public int cornPerHarvest = 1;

    [Tooltip("Time between harvests in seconds")]
    public float harvestCooldown = 2f;

    [Tooltip("Corn needed to feed chicken")]
    public int cornToFeed = 1;

    [Tooltip("Eggs produced per feeding")]
    public int eggsPerFeed = 1;

    [Header("Economy")]
    [Tooltip("Base sell price per egg")]
    public int eggSellPrice = 10;

    [Tooltip("Base cost to hire helper")]
    public int helperBaseCost = 100;

    [Tooltip("Additional cost per helper owned")]
    public int helperCostIncrease = 50;

    [Header("Helper Settings")]
    [Tooltip("Helper movement speed")]
    public float helperSpeed = 3f;

    [Tooltip("Time helper waits between tasks")]
    public float helperWaitTime = 0.5f;

    [Header("Animation Speeds")]
    public float tweenDuration = 0.3f;
    public float punchScale = 1.3f;
    public float bobSpeed = 2f;
    public float bobAmount = 0.1f;

    [Header("Colors")]
    public Color cornColor = new Color(1f, 0.9f, 0.3f);
    public Color eggColor = new Color(1f, 0.98f, 0.9f);
    public Color coinColor = new Color(1f, 0.85f, 0.2f);
    public Color readyColor = new Color(0.5f, 1f, 0.5f);
    public Color cooldownColor = new Color(0.7f, 0.7f, 0.7f);
}
