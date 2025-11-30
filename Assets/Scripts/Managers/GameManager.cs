using UnityEngine;
using System;

/// <summary>
/// GameManager - Central game controller that manages all game state, resources, and game loop.
/// Implements singleton pattern for global access.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Game state events for UI updates and other systems to subscribe to
    public event Action<int> OnCornChanged;
    public event Action<int> OnEggsChanged;
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnHelperCountChanged;
    public event Action<string, Vector3, Color> OnResourceGained; // For floating text feedback

    [Header("Game Configuration")]
    [SerializeField] private GameConfig config;

    [Header("Starting Resources (Used if no GameConfig)")]
    [SerializeField] private int startingCorn = 0;
    [SerializeField] private int startingEggs = 0;
    [SerializeField] private int startingCoins = 50;

    [Header("Base Prices (Used if no GameConfig)")]
    [SerializeField] private int eggSellPrice = 10;
    [SerializeField] private int helperCost = 100;

    // Expose config for other systems
    public GameConfig Config => config;

    [Header("Game References")]
    [SerializeField] private Transform cornFieldPosition;
    [SerializeField] private Transform chickenPosition;
    [SerializeField] private Transform storePosition;
    [SerializeField] private Transform helperSpawnPoint;

    [Header("Prefabs")]
    [SerializeField] private GameObject helperPrefab;
    [SerializeField] private GameObject coinParticlePrefab;
    [SerializeField] private GameObject sparkleParticlePrefab;

    // Current resource counts
    private int corn;
    private int eggs;
    private int coins;
    private int helperCount;

    // Upgrade multipliers
    private float cornMultiplier = 1f;
    private float eggMultiplier = 1f;
    private float priceMultiplier = 1f;
    private float speedMultiplier = 1f;

    // Properties for accessing resources
    public int Corn => corn;
    public int Eggs => eggs;
    public int Coins => coins;
    public int HelperCount => helperCount;
    public int EggSellPrice => Mathf.RoundToInt(GetEggSellPrice() * priceMultiplier);
    public int HelperCost => GetHelperBaseCost() + (helperCount * GetHelperCostIncrease());
    public float SpeedMultiplier => speedMultiplier;

    // Helper methods for config values with fallbacks
    private int GetEggSellPrice() => config != null ? config.eggSellPrice : eggSellPrice;
    private int GetHelperBaseCost() => config != null ? config.helperBaseCost : helperCost;
    private int GetHelperCostIncrease() => config != null ? config.helperCostIncrease : 50;
    private int GetStartingCorn() => config != null ? config.startingCorn : startingCorn;
    private int GetStartingEggs() => config != null ? config.startingEggs : startingEggs;
    private int GetStartingCoins() => config != null ? config.startingCoins : startingCoins;

    // Position getters for helpers and other systems
    public Transform CornFieldPosition => cornFieldPosition;
    public Transform ChickenPosition => chickenPosition;
    public Transform StorePosition => storePosition;

    private void Awake()
    {
        // Singleton setup with persistence
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize game with starting resources
    /// </summary>
    private void InitializeGame()
    {
        corn = GetStartingCorn();
        eggs = GetStartingEggs();
        coins = GetStartingCoins();
        helperCount = 0;

        // Trigger initial UI updates
        OnCornChanged?.Invoke(corn);
        OnEggsChanged?.Invoke(eggs);
        OnCoinsChanged?.Invoke(coins);
    }

    /// <summary>
    /// Add corn to inventory with optional animation trigger
    /// </summary>
    public void AddCorn(int amount, Vector3? worldPosition = null)
    {
        int actualAmount = Mathf.RoundToInt(amount * cornMultiplier);
        corn += actualAmount;
        OnCornChanged?.Invoke(corn);

        // Trigger floating text feedback
        if (worldPosition.HasValue)
        {
            Color cornColor = config != null ? config.cornColor : new Color(1f, 0.9f, 0.3f);
            OnResourceGained?.Invoke($"+{actualAmount} 🌽", worldPosition.Value, cornColor);
        }

        // Play collection sound
        AudioManager.Instance?.PlaySound("collect");
    }

    /// <summary>
    /// Add corn to inventory (legacy overload without position)
    /// </summary>
    public void AddCorn(int amount)
    {
        AddCorn(amount, null);
    }

    /// <summary>
    /// Use corn from inventory (returns false if not enough)
    /// </summary>
    public bool UseCorn(int amount)
    {
        if (corn >= amount)
        {
            corn -= amount;
            OnCornChanged?.Invoke(corn);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Add eggs to inventory
    /// </summary>
    public void AddEgg(int amount, Vector3? worldPosition = null)
    {
        int actualAmount = Mathf.RoundToInt(amount * eggMultiplier);
        eggs += actualAmount;
        OnEggsChanged?.Invoke(eggs);

        // Trigger floating text feedback
        if (worldPosition.HasValue)
        {
            Color eggColor = config != null ? config.eggColor : new Color(1f, 0.98f, 0.9f);
            OnResourceGained?.Invoke($"+{actualAmount} 🥚", worldPosition.Value, eggColor);
        }

        AudioManager.Instance?.PlaySound("egg");
    }

    /// <summary>
    /// Add eggs to inventory (legacy overload without position)
    /// </summary>
    public void AddEgg(int amount)
    {
        AddEgg(amount, null);
    }

    /// <summary>
    /// Sell an egg at the store (returns false if no eggs)
    /// </summary>
    public bool SellEgg(Vector3? worldPosition = null)
    {
        if (eggs > 0)
        {
            eggs--;
            OnEggsChanged?.Invoke(eggs);

            int salePrice = EggSellPrice;
            AddCoins(salePrice, worldPosition);

            // Spawn coin particles at store
            if (coinParticlePrefab != null && storePosition != null)
            {
                Instantiate(coinParticlePrefab, storePosition.position, Quaternion.identity);
            }

            AudioManager.Instance?.PlaySound("sell");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Sell an egg at the store (legacy overload without position)
    /// </summary>
    public bool SellEgg()
    {
        return SellEgg(null);
    }

    /// <summary>
    /// Add coins to player's balance
    /// </summary>
    public void AddCoins(int amount, Vector3? worldPosition = null)
    {
        coins += amount;
        OnCoinsChanged?.Invoke(coins);

        // Trigger floating text feedback
        if (worldPosition.HasValue)
        {
            Color coinColor = config != null ? config.coinColor : new Color(1f, 0.85f, 0.2f);
            OnResourceGained?.Invoke($"+{amount} 💰", worldPosition.Value, coinColor);
        }
    }

    /// <summary>
    /// Add coins to player's balance (legacy overload without position)
    /// </summary>
    public void AddCoins(int amount)
    {
        AddCoins(amount, null);
    }

    /// <summary>
    /// Spend coins (returns false if not enough)
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            OnCoinsChanged?.Invoke(coins);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Hire a new helper to automate the game loop
    /// </summary>
    public bool HireHelper()
    {
        int cost = HelperCost;
        if (SpendCoins(cost))
        {
            helperCount++;
            OnHelperCountChanged?.Invoke(helperCount);

            // Spawn the helper
            if (helperPrefab != null && helperSpawnPoint != null)
            {
                GameObject helper = Instantiate(helperPrefab, helperSpawnPoint.position, Quaternion.identity);
                
                // Spawn sparkle effect
                if (sparkleParticlePrefab != null)
                {
                    Instantiate(sparkleParticlePrefab, helperSpawnPoint.position, Quaternion.identity);
                }
            }

            AudioManager.Instance?.PlaySound("upgrade");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Apply an upgrade multiplier
    /// </summary>
    public void ApplyUpgrade(UpgradeType type, float multiplier)
    {
        switch (type)
        {
            case UpgradeType.CornField:
                cornMultiplier *= multiplier;
                break;
            case UpgradeType.ChickenProduction:
                eggMultiplier *= multiplier;
                break;
            case UpgradeType.EggPrice:
                priceMultiplier *= multiplier;
                break;
            case UpgradeType.Speed:
                speedMultiplier *= multiplier;
                break;
        }

        // Spawn sparkle effect
        if (sparkleParticlePrefab != null)
        {
            Instantiate(sparkleParticlePrefab, Vector3.zero, Quaternion.identity);
        }

        AudioManager.Instance?.PlaySound("upgrade");
    }

    /// <summary>
    /// Check if player can afford an amount
    /// </summary>
    public bool CanAfford(int amount)
    {
        return coins >= amount;
    }

    /// <summary>
    /// Try to spend coins with error feedback if insufficient
    /// </summary>
    public bool TrySpendCoins(int amount, out int shortfall)
    {
        shortfall = amount - coins;
        if (coins >= amount)
        {
            coins -= amount;
            OnCoinsChanged?.Invoke(coins);
            return true;
        }
        AudioManager.Instance?.PlaySound("error");
        return false;
    }

    /// <summary>
    /// Reset progress for prestige or new game
    /// </summary>
    public void ResetProgress()
    {
        corn = GetStartingCorn();
        eggs = GetStartingEggs();
        coins = GetStartingCoins();
        helperCount = 0;
        cornMultiplier = 1f;
        eggMultiplier = 1f;
        priceMultiplier = 1f;
        speedMultiplier = 1f;

        // Update UI
        OnCornChanged?.Invoke(corn);
        OnEggsChanged?.Invoke(eggs);
        OnCoinsChanged?.Invoke(coins);
        OnHelperCountChanged?.Invoke(helperCount);

        // Clear saved data
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Save game state to PlayerPrefs
    /// </summary>
    public void SaveGame()
    {
        PlayerPrefs.SetInt("Corn", corn);
        PlayerPrefs.SetInt("Eggs", eggs);
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Helpers", helperCount);
        PlayerPrefs.SetFloat("CornMultiplier", cornMultiplier);
        PlayerPrefs.SetFloat("EggMultiplier", eggMultiplier);
        PlayerPrefs.SetFloat("PriceMultiplier", priceMultiplier);
        PlayerPrefs.SetFloat("SpeedMultiplier", speedMultiplier);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load game state from PlayerPrefs
    /// </summary>
    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("Corn"))
        {
            corn = PlayerPrefs.GetInt("Corn");
            eggs = PlayerPrefs.GetInt("Eggs");
            coins = PlayerPrefs.GetInt("Coins");
            helperCount = PlayerPrefs.GetInt("Helpers");
            cornMultiplier = PlayerPrefs.GetFloat("CornMultiplier", 1f);
            eggMultiplier = PlayerPrefs.GetFloat("EggMultiplier", 1f);
            priceMultiplier = PlayerPrefs.GetFloat("PriceMultiplier", 1f);
            speedMultiplier = PlayerPrefs.GetFloat("SpeedMultiplier", 1f);

            // Update UI
            OnCornChanged?.Invoke(corn);
            OnEggsChanged?.Invoke(eggs);
            OnCoinsChanged?.Invoke(coins);
            OnHelperCountChanged?.Invoke(helperCount);
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }
}

/// <summary>
/// Enum for different upgrade types
/// </summary>
public enum UpgradeType
{
    CornField,
    ChickenProduction,
    EggPrice,
    Speed,
    StoreCapacity
}
