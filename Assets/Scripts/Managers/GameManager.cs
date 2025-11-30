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

    [Header("Starting Resources")]
    [SerializeField] private int startingCorn = 0;
    [SerializeField] private int startingEggs = 0;
    [SerializeField] private int startingCoins = 50;

    [Header("Base Prices")]
    [SerializeField] private int eggSellPrice = 10;
    [SerializeField] private int helperCost = 100;

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
    public int EggSellPrice => Mathf.RoundToInt(eggSellPrice * priceMultiplier);
    public int HelperCost => helperCost + (helperCount * 50);
    public float SpeedMultiplier => speedMultiplier;

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
        corn = startingCorn;
        eggs = startingEggs;
        coins = startingCoins;
        helperCount = 0;

        // Trigger initial UI updates
        OnCornChanged?.Invoke(corn);
        OnEggsChanged?.Invoke(eggs);
        OnCoinsChanged?.Invoke(coins);
    }

    /// <summary>
    /// Add corn to inventory with optional animation trigger
    /// </summary>
    public void AddCorn(int amount)
    {
        int actualAmount = Mathf.RoundToInt(amount * cornMultiplier);
        corn += actualAmount;
        OnCornChanged?.Invoke(corn);

        // Play collection sound
        AudioManager.Instance?.PlaySound("collect");
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
    public void AddEgg(int amount)
    {
        int actualAmount = Mathf.RoundToInt(amount * eggMultiplier);
        eggs += actualAmount;
        OnEggsChanged?.Invoke(eggs);

        AudioManager.Instance?.PlaySound("egg");
    }

    /// <summary>
    /// Sell an egg at the store (returns false if no eggs)
    /// </summary>
    public bool SellEgg()
    {
        if (eggs > 0)
        {
            eggs--;
            OnEggsChanged?.Invoke(eggs);

            int salePrice = EggSellPrice;
            AddCoins(salePrice);

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
    /// Add coins to player's balance
    /// </summary>
    public void AddCoins(int amount)
    {
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
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
