# Upgrade System Documentation 📈

This document details the upgrade and progression systems in Chicken Coop.

---

## Table of Contents

1. [Upgrade Types](#upgrade-types)
2. [Upgrade Data ScriptableObject](#upgrade-data-scriptableobject)
3. [Economy Balance](#economy-balance)
4. [Helper System](#helper-system)
5. [Progression Tables](#progression-tables)

---

## Upgrade Types

### Enum Definition

```csharp
public enum UpgradeType
{
    CornField,           // Increases corn per harvest
    ChickenProduction,   // Increases eggs per feeding
    EggPrice,            // Increases sell price
    Speed,               // Increases all action speeds
    StoreCapacity        // Reserved for future use
}
```

### Upgrade Effects

| Type | Effect | Multiplier | Description |
|------|--------|------------|-------------|
| **CornField** | `cornMultiplier` | 1.2x | More corn per harvest |
| **ChickenProduction** | `eggMultiplier` | 1.2x | More eggs per feeding |
| **EggPrice** | `priceMultiplier` | 1.2x | Higher egg sell price |
| **Speed** | `speedMultiplier` | 1.2x | Faster cooldowns & movement |
| **StoreCapacity** | - | - | Placeholder for future |

---

## Upgrade Data ScriptableObject

**Location:** `Assets/Scripts/ScriptableObjects/UpgradeData.cs`

### Purpose
Configurable data container for individual upgrades. Create instances via Asset Menu.

### Creating New Upgrades

```
Unity Menu: Assets → Create → ChickenCoop → Upgrade Data
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `upgradeName` | string | Display name |
| `description` | string | Player-facing description |
| `icon` | Sprite | UI icon |
| `baseCost` | int | Starting cost |
| `costMultiplier` | float | Cost increase per level |
| `maxLevel` | int | Maximum purchases (0 = unlimited) |
| `upgradeType` | UpgradeType | Effect category |
| `effectMultiplier` | float | Per-level multiplier |
| `flatBonus` | int | Per-level flat bonus |
| `tintColor` | Color | UI color |
| `upgradeParticlePrefab` | GameObject | Effect on purchase |

### Methods

#### `GetCost()`
```csharp
public int GetCost()
{
    return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
}
```

#### `GetTotalMultiplier()`
```csharp
public float GetTotalMultiplier()
{
    return Mathf.Pow(effectMultiplier, currentLevel);
}
```

#### `CanPurchase()`
```csharp
public bool CanPurchase()
{
    if (maxLevel > 0 && currentLevel >= maxLevel) return false;
    return GameManager.Instance.CanAfford(GetCost());
}
```

#### `Purchase()`
```csharp
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
```

---

## Economy Balance

### Starting Resources

| Resource | Amount |
|----------|--------|
| Coins | 50 |
| Corn | 0 |
| Eggs | 0 |
| Helpers | 0 |

### Base Values

| Item | Value |
|------|-------|
| Egg Sell Price | 10 coins |
| Corn per Harvest | 1 |
| Eggs per Feed | 1 |
| Corn to Feed | 1 |

### Current Upgrade Costs (Hardcoded)

| Upgrade Index | Cost | Type |
|---------------|------|------|
| 0 | 100 | CornField |
| 1 | 200 | ChickenProduction |
| 2 | 300 | EggPrice |
| 3 | 500 | Speed |
| 4 | 750 | StoreCapacity |

---

## Helper System

### Cost Formula

```csharp
public int HelperCost => helperBaseCost + (helperCount * helperCostIncrease);
// helperBaseCost = 100
// helperCostIncrease = 50
```

### Helper Cost Progression

| Helper # | Cost | Total Investment |
|----------|------|------------------|
| 1st | 100 | 100 |
| 2nd | 150 | 250 |
| 3rd | 200 | 450 |
| 4th | 250 | 700 |
| 5th | 300 | 1,000 |
| 6th | 350 | 1,350 |
| 7th | 400 | 1,750 |
| 8th | 450 | 2,200 |
| 9th | 500 | 2,700 |
| 10th | 550 | 3,250 |

### Helper Efficiency

```
Base Loop Time: ~7-8 seconds
Production per Helper per Loop:
├── Corn: +1 (consumed)
├── Eggs: +1 (consumed)
└── Coins: +10 (net gain)

Effective Rate: ~10 coins every 8 seconds = 1.25 coins/second/helper
```

### ROI Analysis

| Helper # | Cost | Time to ROI (seconds) |
|----------|------|----------------------|
| 1st | 100 | 80s |
| 2nd | 150 | 120s |
| 3rd | 200 | 160s |
| 4th | 250 | 200s |
| 5th | 300 | 240s |

---

## Progression Tables

### Multiplier Stacking

All multipliers stack multiplicatively:

```csharp
// Example: 3 CornField upgrades
cornMultiplier = 1.0 × 1.2 × 1.2 × 1.2 = 1.728
// Result: 1.728 corn per harvest (rounds to 2)
```

### Upgrade Level Progression (with 1.5x cost multiplier)

| Level | Base Cost | Actual Cost | Total Multiplier |
|-------|-----------|-------------|------------------|
| 0 | 100 | 100 | 1.0x |
| 1 | 100 | 100 | 1.2x |
| 2 | 100 | 150 | 1.44x |
| 3 | 100 | 225 | 1.73x |
| 4 | 100 | 338 | 2.07x |
| 5 | 100 | 507 | 2.49x |

### Income Projection

**Without Upgrades (1 Helper):**
```
Time: 0s    → Coins: 50
Time: 8s    → Coins: 60
Time: 16s   → Coins: 70
Time: 80s   → Coins: 150
Time: 160s  → Coins: 250
```

**With Speed Upgrade (1.2x):**
```
Loop time: 8s ÷ 1.2 = 6.67s
Production: 1.5 coins/second/helper
```

**With Price Upgrade (1.2x):**
```
Sell price: 10 × 1.2 = 12 coins
Production: 1.5 coins/second/helper
```

### Recommended Upgrade Path

```
Early Game (0-200 coins):
├── Focus on manual gameplay
├── Save for first helper (100 coins)
└── Build to 150 coins for second helper

Mid Game (200-1000 coins):
├── Buy 2-3 helpers
├── Purchase CornField upgrade (100)
├── Purchase EggPrice upgrade (300)
└── Consider Speed upgrade for faster loops

Late Game (1000+ coins):
├── Max out helpers (5-10)
├── Stack all multiplier upgrades
└── Optimize for idle play
```

---

## GameConfig ScriptableObject

**Location:** `Assets/Scripts/ScriptableObjects/GameConfig.cs`

### Purpose
Centralized configuration for all game balance values.

### Creating Config

```
Unity Menu: Assets → Create → ChickenCoop → Game Config
```

### Categories

#### Starting Resources
```csharp
public int startingCorn = 0;
public int startingEggs = 0;
public int startingCoins = 50;
```

#### Production Values
```csharp
public int cornPerHarvest = 1;
public float harvestCooldown = 2f;
public int cornToFeed = 1;
public int eggsPerFeed = 1;
```

#### Economy
```csharp
public int eggSellPrice = 10;
public int helperBaseCost = 100;
public int helperCostIncrease = 50;
```

#### Helper Settings
```csharp
public float helperSpeed = 3f;
public float helperWaitTime = 0.5f;
```

#### Animation Speeds
```csharp
public float tweenDuration = 0.3f;
public float punchScale = 1.3f;
public float bobSpeed = 2f;
public float bobAmount = 0.1f;
```

#### Colors
```csharp
public Color cornColor = new Color(1f, 0.9f, 0.3f);
public Color eggColor = new Color(1f, 0.98f, 0.9f);
public Color coinColor = new Color(1f, 0.85f, 0.2f);
public Color readyColor = new Color(0.5f, 1f, 0.5f);
public Color cooldownColor = new Color(0.7f, 0.7f, 0.7f);
```

---

## Visual: Upgrade System Flow

```
┌───────────────────────────────────────────────────────────────────┐
│                    UPGRADE SYSTEM FLOW                            │
├───────────────────────────────────────────────────────────────────┤
│                                                                    │
│  ┌──────────────┐                                                  │
│  │  UI Manager  │                                                  │
│  │ (Buttons)    │                                                  │
│  └──────┬───────┘                                                  │
│         │                                                          │
│         │ OnUpgradeClicked(index)                                  │
│         ▼                                                          │
│  ┌──────────────┐                                                  │
│  │ Check Cost   │                                                  │
│  │ CanAfford()? │                                                  │
│  └──────┬───────┘                                                  │
│         │ Yes                                                      │
│         ▼                                                          │
│  ┌──────────────┐                                                  │
│  │ SpendCoins() │                                                  │
│  └──────┬───────┘                                                  │
│         │                                                          │
│         ▼                                                          │
│  ┌──────────────┐     ┌──────────────────────┐                    │
│  │ ApplyUpgrade │────►│ Update Multiplier    │                    │
│  │ (type, 1.2x) │     │ • cornMultiplier     │                    │
│  └──────┬───────┘     │ • eggMultiplier      │                    │
│         │             │ • priceMultiplier    │                    │
│         │             │ • speedMultiplier    │                    │
│         ▼             └──────────────────────┘                    │
│  ┌──────────────┐                                                  │
│  │ Visual FX    │                                                  │
│  │ • Sparkles   │                                                  │
│  │ • Sound      │                                                  │
│  │ • UI Pop     │                                                  │
│  └──────────────┘                                                  │
│                                                                    │
└───────────────────────────────────────────────────────────────────┘
```

---

## Save System Integration

### Saved Values (PlayerPrefs)

```csharp
// Resources
PlayerPrefs.SetInt("Corn", corn);
PlayerPrefs.SetInt("Eggs", eggs);
PlayerPrefs.SetInt("Coins", coins);
PlayerPrefs.SetInt("Helpers", helperCount);

// Multipliers
PlayerPrefs.SetFloat("CornMultiplier", cornMultiplier);
PlayerPrefs.SetFloat("EggMultiplier", eggMultiplier);
PlayerPrefs.SetFloat("PriceMultiplier", priceMultiplier);
PlayerPrefs.SetFloat("SpeedMultiplier", speedMultiplier);

// Audio Settings
PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
PlayerPrefs.SetFloat("MusicVolume", musicVolume);
```

### Load Trigger

```csharp
public void LoadGame()
{
    if (PlayerPrefs.HasKey("Corn"))
    {
        // Load all values
        // Trigger UI updates via events
    }
}
```

### Save Triggers
- `OnApplicationQuit()`
- `OnApplicationPause(true)`
