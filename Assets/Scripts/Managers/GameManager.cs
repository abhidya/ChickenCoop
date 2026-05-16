using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ChickenCoop.Managers
{
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
    public event Action<string, int> OnZoneExpanded; // zoneID, newCount
    public event Action<string, Vector3, Color> OnResourceGained; // For floating text feedback
    public event Action<UpgradeType, int> OnUpgradeApplied;

    [Header("Game Configuration")]
    [SerializeField] private GameConfig config;

    [Header("Starting Resources (Used if no GameConfig)")]
    [SerializeField] private int startingCorn = 5;
    [SerializeField] private int startingEggs = 5;
    [SerializeField] private int startingCoins = 150;

    [Header("Base Prices (Used if no GameConfig)")]
    [SerializeField] private int eggSellPrice = 10;
    [SerializeField] private int helperCost = 100;

    // Expose config for other systems
    public GameConfig Config => config;

    [Header("Game References")]
    [SerializeField] private List<Transform> cornFieldPositions = new List<Transform>();
    [SerializeField] private List<Transform> chickenPositions = new List<Transform>();
    [SerializeField] private Transform storePosition;
    [SerializeField] private Transform helperSpawnPoint;

    [Header("Prefabs")]
    [SerializeField] private GameObject helperPrefab;
    [SerializeField] private GameObject coinParticlePrefab;
    [SerializeField] private GameObject sparkleParticlePrefab;
    [SerializeField] private GameObject zoneControllerPrefab;
    [SerializeField] private SceneRegistry sceneRegistry;

    // Data-Driven Zone Management
    private List<FarmZoneController> activeZoneControllers = new List<FarmZoneController>();
    public List<FarmZoneController> ActiveZoneControllers => activeZoneControllers;
    private Dictionary<string, int> inventory = new Dictionary<string, int>();

    // Current resource counts (Maintained for UI backward compatibility)
    private int corn;
    private int eggs;
    private int coins;
    private int helperCount;

    // Robust tutorial tracking
    private int totalCornHarvested = 0;
    private int totalEggsProduced = 0;
    private int totalWheatHarvested = 0;

    // Expansion Flags (Free after first purchase)
    private bool hasPurchasedWheat = false;
    private bool hasPurchasedCow = false;

    // Upgrade unlock flags
    private bool hasUnlockedWheat = false;
    private bool hasUnlockedChicken = false;
    private bool hasUnlockedCow = false;
    private bool hasUnlockedCarrot = false;
    private bool hasUnlockedPig = false;

    // Upgrade progression state
    private readonly Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>();

    // Upgrade multipliers
    private float cornMultiplier = 1f;
    private float wheatMultiplier = 1f;
    private float carrotMultiplier = 1f;
    private float eggMultiplier = 1f;
    private float milkMultiplier = 1f;
    private float priceMultiplier = 1f;
    private float speedMultiplier = 1f;
    private float storeEfficiencyMultiplier = 1f;

    private static Sprite runtimeHelperSprite;
    private Camera cachedMainCamera;
    private float baseOrthoSize = 8f;

    private const string RuntimeChickenVisualResourcePath = "HappyHarvestChicken";
    private const string RuntimeCornVisualResourcePath = "HappyHarvestCorn";
    private const string RuntimeStoreVisualResourcePath = "HappyHarvestMarket";


    // Properties for accessing resources
    public int Corn => corn;
    public int Eggs => eggs;
    public int Coins => coins;
    public int HelperCount => helperCount;
    
    // Public accessors for tutorial tracking
    public int TotalCornHarvested => totalCornHarvested;
    public int TotalEggsProduced => totalEggsProduced;
    public int TotalWheatHarvested => totalWheatHarvested;
    public int EggSellPrice => Mathf.RoundToInt(GetEggSellPrice() * priceMultiplier);
    public int HelperCost => GetHelperBaseCost() + (helperCount * GetHelperCostIncrease());
    public float SpeedMultiplier => speedMultiplier;
    public float StoreEfficiencyMultiplier => storeEfficiencyMultiplier;
    public bool HasPurchasedWheat => hasPurchasedWheat;
    public bool HasPurchasedCow => hasPurchasedCow;
    public PlayerController Player => sceneRegistry != null ? sceneRegistry.Player : FindFirstObjectByType<PlayerController>();
    
    // Leveling State
    private int currentAct = 1;
    public int CurrentAct => currentAct;

    public int GetUpgradeLevel(UpgradeType type)
    {
        return upgradeLevels.TryGetValue(type, out int level) ? level : 0;
    }

    public bool HasUpgrade(UpgradeType type)
    {
        return GetUpgradeLevel(type) > 0;
    }

    private void SetUpgradeLevel(UpgradeType type, int level)
    {
        upgradeLevels[type] = Mathf.Max(0, level);
    }

    private void IncrementUpgradeLevel(UpgradeType type)
    {
        SetUpgradeLevel(type, GetUpgradeLevel(type) + 1);
    }

    public void AdvanceAct()
    {
        currentAct = Mathf.Min(currentAct + 1, 4);
        Debug.Log($"[GameManager] Advanced to Act {currentAct}!");
        
        // Refresh UI and environment for the new Act
        UIManager.Instance?.ShowActTitle(currentAct);
        EnvironmentManager.Instance?.RefreshFences();
    }

    // Helper methods for config values with fallbacks
    private int GetEggSellPrice() => config != null ? config.eggSellPrice : eggSellPrice;
    private int GetHelperBaseCost() => config != null ? config.helperBaseCost : helperCost;
    private int GetHelperCostIncrease() => config != null ? config.helperCostIncrease : 50;
    private int GetStartingCorn() => config != null ? config.startingCorn : startingCorn;
    private int GetStartingEggs() => config != null ? config.startingEggs : startingEggs;
    private int GetStartingCoins() => config != null ? config.startingCoins : startingCoins;

    // Position getters for helpers and other systems
    public List<Transform> CornFieldPositions => cornFieldPositions;
    public List<Transform> ChickenPositions => chickenPositions;
    public Transform StorePosition => sceneRegistry != null && sceneRegistry.Store != null ? sceneRegistry.Store.transform : storePosition;

    private void Awake()
    {
        // Singleton setup with persistence
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
            ResolveSceneReferences();
            EnsureEventSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Ensures a standard EventSystem exists in the scene for UI interactivity.
    /// Crucial for WebGL environments.
    /// </summary>
    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            Debug.Log("[GameManager] Created missing EventSystem for UI support.");
        }
        else
        {
            // Resolve duplicate event systems in the scene
            EventSystem[] systems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            if (systems.Length > 1)
            {
                Debug.LogWarning($"[GameManager] Purging {systems.Length - 1} duplicate EventSystems.");
                for (int i = 1; i < systems.Length; i++)
                {
                    Destroy(systems[i].gameObject);
                }
            }
        }
    }

    private void Start()
    {
        cachedMainCamera = Camera.main;
        if (cachedMainCamera != null) baseOrthoSize = cachedMainCamera.orthographicSize;
        StartCoroutine(BootstrapStorySupport());
    }
    private void Update()
    {
        UpdateCameraForOrientation();
    }

    private void UpdateCameraForOrientation()
    {
        if (cachedMainCamera == null) cachedMainCamera = Camera.main;
        if (cachedMainCamera == null) return;

        float aspectRatio = (float)Screen.width / Screen.height;
        
        // If aspect ratio < 1 (Portrait), we need to zoom out to fit the farm side-to-side
        if (aspectRatio < 1.1f)
        {
            // Simple heuristic to keep about 16 units of horizontal view
            // horizontal_units = ortho_size * aspect_ratio * 2
            // ortho_size = horizontal_units / (aspect_ratio * 2)
            float targetOrthoHeight = 12f / aspectRatio;
            cachedMainCamera.orthographicSize = Mathf.Lerp(cachedMainCamera.orthographicSize, Mathf.Max(baseOrthoSize, targetOrthoHeight / 2f), Time.deltaTime * 5f);
        }
        else
        {
            cachedMainCamera.orthographicSize = Mathf.Lerp(cachedMainCamera.orthographicSize, baseOrthoSize, Time.deltaTime * 5f);
        }
    }


    /// <summary>
    /// Initialize game with starting resources
    /// </summary>
    private void InitializeGame()
    {
        inventory.Clear();
        corn = GetStartingCorn();
        eggs = GetStartingEggs();
        coins = GetStartingCoins();
        helperCount = 0;
        currentAct = 1;
        upgradeLevels.Clear();
        hasPurchasedWheat = false;
        hasPurchasedCow = false;
        hasUnlockedWheat = false;
        hasUnlockedChicken = false;
        hasUnlockedCow = false;
        hasUnlockedCarrot = false;
        hasUnlockedPig = false;

        // Initialize generic inventory
        inventory["Corn"] = corn;
        inventory["Egg"] = eggs;

        // Trigger initial UI updates
        OnCornChanged?.Invoke(corn);
        OnEggsChanged?.Invoke(eggs);
        OnCoinsChanged?.Invoke(coins);
        OnHelperCountChanged?.Invoke(helperCount);
    }

    private bool HasSavedProgress()
    {
        return PlayerPrefs.HasKey("Corn");
    }

    /// <summary>
    /// Add corn to inventory with optional animation trigger
    /// </summary>
    public void AddCorn(int amount, Vector3? worldPosition = null)
    {
        int actualAmount = Mathf.CeilToInt(amount * cornMultiplier);
        corn += actualAmount;
        totalCornHarvested += actualAmount;
        
        // Sync with generic inventory
        inventory["Corn"] = corn;
        
        OnCornChanged?.Invoke(corn);

        // Trigger floating text feedback
        if (worldPosition.HasValue)
        {
            Color cornColor = config != null ? config.cornColor : new Color(1f, 0.9f, 0.3f);
            OnResourceGained?.Invoke($"+{actualAmount} CORN", worldPosition.Value, cornColor);
        }

        // Play collection sound
        AudioManager.Instance?.PlaySound("collect");
    }

    /// <summary>
    /// Generic method to add any item to inventory
    /// </summary>
    public void AddItem(string itemId, int amount, Vector3? worldPosition = null)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        if (itemId == "Corn") { AddCorn(amount, worldPosition); return; }
        if (itemId == "Egg") { AddEgg(amount, worldPosition); return; }

        if (!inventory.ContainsKey(itemId)) inventory[itemId] = 0;
        inventory[itemId] += amount;

        // Track totals for tutorial
        if (itemId == "Wheat") totalWheatHarvested += amount;

        // Feedback
        if (worldPosition.HasValue)
        {
            Color color = Color.white;
            if (itemId == "Wheat") color = new Color(0.96f, 0.87f, 0.47f);
            OnResourceGained?.Invoke($"+{amount} {itemId.ToUpper()}", worldPosition.Value, color);
        }

        UIManager.Instance?.UpdateAllResourceText();
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
    /// Add eggs to inventory
    /// </summary>
    public void AddEgg(int amount, Vector3? worldPosition = null)
    {
        int actualAmount = Mathf.CeilToInt(amount * eggMultiplier);
        eggs += actualAmount;
        totalEggsProduced += actualAmount;
        
        // Sync with generic inventory
        inventory["Egg"] = eggs;
        
        OnEggsChanged?.Invoke(eggs);

        // Trigger floating text feedback
        if (worldPosition.HasValue)
        {
            Color eggColor = config != null ? config.eggColor : new Color(1f, 0.98f, 0.9f);
            OnResourceGained?.Invoke($"+{actualAmount} EGG", worldPosition.Value, eggColor);
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

    // Use generic item from inventory
    public bool UseItem(string itemId, int amount)
    {
        int currentCount = GetItemCount(itemId);
        if (currentCount >= amount)
        {
            inventory[itemId] = currentCount - amount;
            
            // Sync legacy counts for now
            if (itemId == "Corn") corn = inventory[itemId];
            if (itemId == "Egg") eggs = inventory[itemId];
            
            NotifyResourcesChanged(itemId);
            UIManager.Instance?.UpdateAllResourceText();
            return true;
        }
        return false;
    }

    public int GetItemCount(string itemId)
    {
        if (inventory.ContainsKey(itemId)) return inventory[itemId];
        return 0;
    }

    private void NotifyResourcesChanged(string itemId)
    {
        if (itemId == "Corn") OnCornChanged?.Invoke(corn);
        if (itemId == "Egg") OnEggsChanged?.Invoke(eggs);
        if (itemId == "Coins") OnCoinsChanged?.Invoke(coins);
    }

    public bool SpendCorn(int amount) => UseCorn(amount);
    public bool UseCorn(int amount) => UseItem("Corn", amount);
    public bool UseEggs(int amount) => UseItem("Egg", amount);

    public void RefundEggs(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        eggs += amount;
        inventory["Egg"] = eggs;
        OnEggsChanged?.Invoke(eggs);
        UIManager.Instance?.UpdateAllResourceText();
    }

    public bool RemoveItem(string itemId, int amount) => UseItem(itemId, amount);
    public bool UseCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            OnCoinsChanged?.Invoke(coins);
            return true;
        }
        return false;
    }

    public FarmZoneController GetOrCreateZone(string zoneID)
    {
        return GetZone(zoneID);
    }

    private FarmZoneTemplate CreateProperTemplate(string zoneID)
    {
        Debug.LogWarning($"[GameManager] Template for {zoneID} not found. Using factory method.");
        return FarmZoneTemplate.CreateDefault(zoneID);
    }

    public void AddObjectToZone(string zoneID)
    {
        TryAddObjectToZone(zoneID, true);
    }

    public bool TryAddObjectToZoneWithoutCost(string zoneID)
    {
        return TryAddObjectToZone(zoneID, false);
    }

    public bool TryPlaceChickenInNextSlot()
    {
        return TryAddObjectToZoneWithoutCost("Chicken");
    }

    public bool TryHatchChicken(int eggCost)
    {
        if (eggCost <= 0)
        {
            eggCost = 1;
        }

        FarmZoneController zone = GetZone("Chicken");
        if (zone == null)
        {
            Debug.LogWarning("[GameManager] Cannot hatch chicken: Chicken zone missing.");
            return false;
        }

        zone.RefreshAuthoredSlots();
        if (zone.GetNextAvailableAuthoredSlot() == null)
        {
            Debug.LogWarning("[GameManager] Cannot hatch chicken: Chicken zone full.");
            return false;
        }

        if (Eggs < eggCost)
        {
            Debug.LogWarning($"[GameManager] Cannot hatch chicken: need {eggCost} eggs, have {Eggs}.");
            return false;
        }

        if (!UseEggs(eggCost))
        {
            return false;
        }

        if (!TryAddObjectToZoneWithoutCost("Chicken"))
        {
            RefundEggs(eggCost);
            Debug.LogWarning("[GameManager] Chicken hatch failed after cost; eggs refunded.");
            return false;
        }

        return true;
    }

    private bool TryAddObjectToZone(string zoneID, bool chargeCost)
    {
        Debug.Log($"[GameManager] AddObjectToZone called with zoneID={zoneID}");
        FarmZoneController zone = GetOrCreateZone(zoneID);
        if (zone == null) {
            Debug.LogError($"[GameManager] Authored zone missing for {zoneID}");
            return false;
        }

        zone.RefreshAuthoredSlots();
        Debug.Log($"[GameManager] Zone {zoneID}: CurrentCount={zone.CurrentCount}, maxSlots={(zone.template != null ? zone.template.maxSlots : -1)}");

        Transform slot = zone.GetNextAvailableAuthoredSlot();
        if (slot == null)
        {
            Debug.LogError($"[GameManager] No authored slots available for {zoneID}");
            return false;
        }

        if (chargeCost)
        {
            bool canAfford = false;

            Debug.Log($"[GameManager] Checking affordability. Corn={Corn}, Eggs={Eggs}");

            // 1-to-1 Resource Barter for basic expansion
            // Market purchase logic would be handled by a separate Shop method, but here we enforce resource growth for Plant/Incubate
            if (zoneID == "Corn")
            {
                if (Corn >= 1)
                {
                    UseCorn(1);
                    canAfford = true;
                }
            }
            else if (zoneID == "Chicken")
            {
                if (Eggs >= 1)
                {
                    UseEggs(1);
                    canAfford = true;
                }
            }
            else if (zoneID == "Wheat")
            {
                // Free after first seed purchase
                if (hasPurchasedWheat)
                {
                    canAfford = true;
                }
                else if (GetItemCount("Wheat") >= 1)
                {
                    UseItem("Wheat", 1);
                    canAfford = true;
                }
            }
            else if (zoneID == "Cow")
            {
                // Free after first cow purchase
                if (hasPurchasedCow)
                {
                    canAfford = true;
                }
                else if (GetItemCount("Milk") >= 1)
                {
                    UseItem("Milk", 1);
                    canAfford = true;
                }
            }
            else
            {
                // Generic fallback to coins
                int cost = (zone.CurrentCount == 0 && zone.template != null) ? zone.template.baseUnlockCost : zone.template != null ? zone.template.costPerAdditionalSlot : 0;
                if (coins >= cost)
                {
                    canAfford = UseCoins(cost);
                }
            }

            if (!canAfford)
            {
                Debug.LogWarning($"[GameManager] Cannot afford to add {zoneID}. Corn={Corn}, Eggs={Eggs}");
                return false;
            }
        }

        GameObject instance = SpawnFromTemplate(zone, slot, $"{zoneID}_{zone.CurrentCount}");

        if (instance != null)
        {
            zone.AddSlot(instance.transform);
            Debug.Log($"[GameManager] Added instance to zone. Triggering OnZoneExpanded({zoneID}, {zone.CurrentCount})");
            OnZoneExpanded?.Invoke(zoneID, zone.CurrentCount);

            // Spawn decorations for this zone
            SpawnZoneDecorations(zone, instance.transform);

            EnvironmentManager.Instance?.RefreshFences();
            UpdateExpansionButtons();
            return true;
        }

        Debug.LogError($"[GameManager] Failed to spawn instance for zone {zoneID}");
        return false;
    }

    private void UpdateExpansionButtons()
    {
        // Tell UIManager to Refresh its expansion buttons
        UIManager.Instance?.UpdateExpansionButtons();
    }

    public void EnsureZoneHasVisibleMember(string zoneID)
    {
        if (string.IsNullOrWhiteSpace(zoneID))
        {
            return;
        }

        FarmZoneController zone = GetOrCreateZone(zoneID);
        if (zone == null || zone.CurrentCount > 0)
        {
            return;
        }

        Transform slot = zone.GetNextAvailableAuthoredSlot();
        if (slot == null)
        {
            Debug.LogWarning($"[GameManager] No authored slot available for {zoneID}");
            return;
        }

        GameObject instance = SpawnFromTemplate(zone, slot, $"{zoneID}_{zone.CurrentCount}");
        if (instance != null)
        {
            zone.AddSlot(instance.transform);
            OnZoneExpanded?.Invoke(zoneID, zone.CurrentCount);
            SpawnZoneDecorations(zone, instance.transform);
            EnvironmentManager.Instance?.RefreshFences();
        }
    }

    // Legacy Support for UI buttons
    public void AddChicken() => AddObjectToZone("Chicken");
    public void AddCornField() => AddObjectToZone("Corn");

    private Vector3 GetNextGridPosition(int index, Vector3 origin, float spacingX, float spacingY, bool growRight)
    {
        int col = index % 3;
        int row = index / 3;
        // Corn expands left (-X), Chickens expand right (+X), both grow to background (+Y)
        float directionMultiplier = growRight ? 1f : -1f;
        return origin + new Vector3(col * spacingX * directionMultiplier, row * spacingY, 0);
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
            OnResourceGained?.Invoke($"+{amount} GOLD", worldPosition.Value, coinColor);
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

            GameObject helper = SpawnHelperInstance();
            Vector3 effectPosition = helper != null ? helper.transform.position : GetHelperSpawnPosition();

            if (sparkleParticlePrefab != null)
            {
                Instantiate(sparkleParticlePrefab, effectPosition, Quaternion.identity);
            }

            AudioManager.Instance?.PlaySound("upgrade");
            return true;
        }
        return false;
    }

    public void ApplyUpgrade(UpgradeType type, float multiplier)
    {
        IncrementUpgradeLevel(type);

        switch (type)
        {
            case UpgradeType.WheatField:
                hasUnlockedWheat = true;
                hasPurchasedWheat = true;
                UIManager.Instance?.UnlockResourceSlot("Wheat");
                EnsureZoneHasVisibleMember("Wheat");
                break;
            case UpgradeType.ChickenCare:
                hasUnlockedChicken = true;
                eggMultiplier *= multiplier;
                break;
            case UpgradeType.CowPen:
                hasUnlockedCow = true;
                hasPurchasedCow = true;
                UIManager.Instance?.UnlockResourceSlot("Milk");
                EnsureZoneHasVisibleMember("Cow");
                break;
            case UpgradeType.CowFeed:
                wheatMultiplier *= multiplier;
                break;
            case UpgradeType.MilkProduction:
                milkMultiplier *= multiplier;
                UIManager.Instance?.UnlockResourceSlot("Milk");
                break;
            case UpgradeType.CarrotGarden:
                hasUnlockedCarrot = true;
                UIManager.Instance?.UnlockResourceSlot("Carrot");
                EnsureZoneHasVisibleMember("Carrot");
                break;
            case UpgradeType.Fertilizer:
                cornMultiplier *= multiplier;
                wheatMultiplier *= multiplier;
                carrotMultiplier *= multiplier;
                break;
            case UpgradeType.PigPen:
                hasUnlockedPig = true;
                UIManager.Instance?.UnlockResourceSlot("Truffle");
                EnsureZoneHasVisibleMember("Pig");
                break;
            case UpgradeType.HelperSpeed:
                speedMultiplier *= multiplier;
                break;
            case UpgradeType.BiggerStore:
                storeEfficiencyMultiplier *= multiplier;
                break;
        }

        OnUpgradeApplied?.Invoke(type, GetUpgradeLevel(type));

        // Spawn sparkle effect
        if (sparkleParticlePrefab != null)
        {
            Instantiate(sparkleParticlePrefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            SpawnGlobalSparkle();
        }

        AudioManager.Instance?.PlaySound("upgrade");
        SaveGame();
    }

    /// <summary>
    /// Creates a programmatic sparkle effect when no prefab is available.
    /// Ensures upgrades always feel satisfying and premium.
    /// </summary>
    private void SpawnGlobalSparkle()
    {
        GameObject sparkle = new GameObject("UpgradeSparkle");
        sparkle.transform.position = Vector3.zero;

        ParticleSystem ps = sparkle.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.3f;
        main.startLifetime = 1.0f;
        main.startColor = StoryColorPalette.CoinGold;
        main.startSpeed = 3f;
        main.gravityModifier = 0.2f;
        main.maxParticles = 50;
        main.duration = 0.2f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 40) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 2.0f;

        ps.Play();
        Destroy(sparkle, 1.5f);
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
        inventory.Clear();
        corn = GetStartingCorn();
        eggs = GetStartingEggs();
        coins = GetStartingCoins();
        helperCount = 0;
        currentAct = 1;
        cornMultiplier = 1f;
        eggMultiplier = 1f;
        priceMultiplier = 1f;
        speedMultiplier = 1f;
        storeEfficiencyMultiplier = 1f;
        upgradeLevels.Clear();
        hasPurchasedWheat = false;
        hasPurchasedCow = false;
        hasUnlockedWheat = false;
        hasUnlockedChicken = false;
        hasUnlockedCow = false;
        hasUnlockedCarrot = false;
        hasUnlockedPig = false;

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
        PlayerPrefs.SetInt("CurrentAct", currentAct);
        PlayerPrefs.SetFloat("CornMultiplier", cornMultiplier);
        PlayerPrefs.SetFloat("EggMultiplier", eggMultiplier);
        PlayerPrefs.SetFloat("PriceMultiplier", priceMultiplier);
        PlayerPrefs.SetFloat("SpeedMultiplier", speedMultiplier);
        PlayerPrefs.SetFloat("StoreEfficiencyMultiplier", storeEfficiencyMultiplier);
        PlayerPrefs.SetInt("HasPurchasedWheat", hasPurchasedWheat ? 1 : 0);
        PlayerPrefs.SetInt("HasPurchasedCow", hasPurchasedCow ? 1 : 0);
        PlayerPrefs.SetInt("HasUnlockedWheat", hasUnlockedWheat ? 1 : 0);
        PlayerPrefs.SetInt("HasUnlockedChicken", hasUnlockedChicken ? 1 : 0);
        PlayerPrefs.SetInt("HasUnlockedCow", hasUnlockedCow ? 1 : 0);
        PlayerPrefs.SetInt("HasUnlockedCarrot", hasUnlockedCarrot ? 1 : 0);
        PlayerPrefs.SetInt("HasUnlockedPig", hasUnlockedPig ? 1 : 0);

        List<string> savedInventoryKeys = new List<string>(inventory.Keys);
        PlayerPrefs.SetString("InventoryKeys", string.Join("|", savedInventoryKeys));
        foreach (string key in savedInventoryKeys)
        {
            PlayerPrefs.SetInt($"Inventory_{key}", GetItemCount(key));
        }

        foreach (UpgradeType upgradeType in Enum.GetValues(typeof(UpgradeType)))
        {
            PlayerPrefs.SetInt($"UpgradeLevel_{upgradeType}", GetUpgradeLevel(upgradeType));
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load game state from PlayerPrefs
    /// </summary>
    public void LoadGame()
    {
        if (!HasSavedProgress())
        {
            return;
        }

        corn = PlayerPrefs.GetInt("Corn");
        eggs = PlayerPrefs.GetInt("Eggs");
        coins = PlayerPrefs.GetInt("Coins");
        helperCount = PlayerPrefs.GetInt("Helpers");
        currentAct = PlayerPrefs.GetInt("CurrentAct", 1);
        cornMultiplier = PlayerPrefs.GetFloat("CornMultiplier", 1f);
        eggMultiplier = PlayerPrefs.GetFloat("EggMultiplier", 1f);
        priceMultiplier = PlayerPrefs.GetFloat("PriceMultiplier", 1f);
        speedMultiplier = PlayerPrefs.GetFloat("SpeedMultiplier", 1f);
        storeEfficiencyMultiplier = PlayerPrefs.GetFloat("StoreEfficiencyMultiplier", 1f);
        hasPurchasedWheat = PlayerPrefs.GetInt("HasPurchasedWheat", 0) == 1;
        hasPurchasedCow = PlayerPrefs.GetInt("HasPurchasedCow", 0) == 1;
        hasUnlockedWheat = PlayerPrefs.GetInt("HasUnlockedWheat", 0) == 1;
        hasUnlockedChicken = PlayerPrefs.GetInt("HasUnlockedChicken", 0) == 1;
        hasUnlockedCow = PlayerPrefs.GetInt("HasUnlockedCow", 0) == 1;
        hasUnlockedCarrot = PlayerPrefs.GetInt("HasUnlockedCarrot", 0) == 1;
        hasUnlockedPig = PlayerPrefs.GetInt("HasUnlockedPig", 0) == 1;

        inventory.Clear();
        string inventoryKeyCsv = PlayerPrefs.GetString("InventoryKeys", string.Empty);
        if (!string.IsNullOrWhiteSpace(inventoryKeyCsv))
        {
            string[] keys = inventoryKeyCsv.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string key in keys)
            {
                int value = PlayerPrefs.GetInt($"Inventory_{key}", 0);
                inventory[key] = value;
            }
        }

        inventory["Corn"] = corn;
        inventory["Egg"] = eggs;

        upgradeLevels.Clear();
        foreach (UpgradeType upgradeType in Enum.GetValues(typeof(UpgradeType)))
        {
            SetUpgradeLevel(upgradeType, PlayerPrefs.GetInt($"UpgradeLevel_{upgradeType}", 0));
        }

        RestoreLoadedHelpers();

        // Update UI
        OnCornChanged?.Invoke(corn);
        OnEggsChanged?.Invoke(eggs);
        OnCoinsChanged?.Invoke(coins);
        OnHelperCountChanged?.Invoke(helperCount);

        UIManager.Instance?.SyncUpgradeLevelsFromGameManager();
        VisualProgressionController.Instance?.RefreshFromGameState();
    }

    private IEnumerator BootstrapStorySupport()
    {
        yield return null;
        EnsureRuntimeStorySupport();

        if (HasSavedProgress())
        {
            LoadGame();
        }
    }

    private void ResolveSceneReferences()
    {
        sceneRegistry = sceneRegistry != null ? sceneRegistry : FindFirstObjectByType<SceneRegistry>();
        if (sceneRegistry != null)
        {
            sceneRegistry.RefreshCache();
            activeZoneControllers.Clear();
            activeZoneControllers.AddRange(sceneRegistry.Zones.Where(zone => zone != null));

            if (storePosition == null && sceneRegistry.Store != null)
            {
                storePosition = sceneRegistry.Store.transform;
            }

            if (helperSpawnPoint == null && sceneRegistry.HelperSpawn != null)
            {
                helperSpawnPoint = sceneRegistry.HelperSpawn;
            }
        }

        if (activeZoneControllers.Count == 0)
        {
            activeZoneControllers.Clear();
            activeZoneControllers.AddRange(FindObjectsByType<FarmZoneController>(FindObjectsSortMode.None).Where(zone => zone != null));
        }

        if (cornFieldPositions.Count == 0 && sceneRegistry == null)
        {
            HarvestableField field = FindFirstObjectByType<HarvestableField>();
            if (field != null)
            {
                cornFieldPositions.Add(field.transform);
            }
        }

        if (chickenPositions.Count == 0 && sceneRegistry == null)
        {
            Chicken chicken = FindFirstObjectByType<Chicken>();
            if (chicken != null)
            {
                chickenPositions.Add(chicken.transform);
            }
        }

        if (storePosition == null && sceneRegistry == null)
        {
            StoreCounter storeCounter = FindFirstObjectByType<StoreCounter>();
            if (storeCounter != null)
            {
                storePosition = storeCounter.transform;
            }
        }

        if (helperSpawnPoint == null && sceneRegistry == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                helperSpawnPoint = player.transform;
            }
        }
    }

    private void EnsureRuntimeStorySupport()
    {
        ResolveSceneReferences();

        if (FindFirstObjectByType<FloatingTextManager>() == null)
        {
            Debug.LogWarning("[GameManager] FloatingTextManager missing from authored scene.");
        }

        if (FindFirstObjectByType<EnvironmentAnimator>() == null)
        {
            Debug.LogWarning("[GameManager] EnvironmentAnimator missing from authored scene.");
        }

        if (FindFirstObjectByType<EnvironmentManager>() == null)
        {
            Debug.LogWarning("[GameManager] EnvironmentManager missing from authored scene.");
        }

        EnsureRuntimeUIRoot();

        if (FindFirstObjectByType<VisualProgressionController>() == null)
        {
            Debug.LogWarning("[GameManager] VisualProgressionController missing from authored scene.");
        }

        if (FindFirstObjectByType<DayNightCycle>() == null)
        {
            Debug.LogWarning("[GameManager] DayNightCycle missing from authored scene.");
        }

        if (FindFirstObjectByType<TutorialManager>() == null)
        {
            Debug.LogWarning("[GameManager] TutorialManager missing from authored scene.");
        }
        
        if (FindFirstObjectByType<TitleCardManager>() == null)
        {
            Debug.LogWarning("[GameManager] TitleCardManager missing from authored scene.");
        }

        EnsureEnvironmentDecoration();

        // Add Global Light 2D if missing for URP 2D
        if (FindFirstObjectByType<UnityEngine.Rendering.Universal.Light2D>() == null)
        {
            Debug.LogWarning("[GameManager] Global Light 2D missing from authored scene.");
        }
    }

    private void EnsureCoreGameplayObjects()
    {
        Debug.LogWarning("[GameManager] EnsureCoreGameplayObjects is legacy bootstrap only; authored scene should supply core gameplay roots.");
        EnsureChickenObjects();
        EnsureCornFieldObjects();
        EnsureStoreCounterObject();
        EnsureIncubatorObject();
        EnsureTitleCardManager();
    }

    private GameObject EnsureRuntimeUIRoot()
    {
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        Canvas canvas = FindFirstObjectByType<Canvas>();
        GameObject uiRoot = uiManager != null ? uiManager.gameObject : canvas != null ? canvas.gameObject : null;

        if (uiRoot == null)
        {
            Debug.LogWarning("[GameManager] Authored Canvas/UIManager missing from scene.");
            return null;
        }

        Canvas rootCanvas = uiRoot.GetComponent<Canvas>();
        if (rootCanvas == null)
        {
            rootCanvas = uiRoot.AddComponent<Canvas>();
        }
        rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        rootCanvas.sortingOrder = 999;

        CanvasScaler scaler = uiRoot.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = uiRoot.AddComponent<CanvasScaler>();
        }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (uiRoot.GetComponent<GraphicRaycaster>() == null)
        {
            uiRoot.AddComponent<GraphicRaycaster>();
        }

        if (uiManager == null)
        {
            Debug.LogWarning("[GameManager] UIManager missing from authored scene.");
        }

        EnsureEventSystem();
        return uiRoot;
    }

    private void EnsureChickenObjects()
    {
        if (chickenPositions.Count == 0) ResolveSceneReferences();
        
        FarmZoneController zone = GetOrCreateZone("Chicken");
        if (zone == null)
        {
            Debug.LogWarning("[GameManager] Chicken zone template not found in GameConfig. Fences will not render.");
            return;
        }

        for (int i = 0; i < chickenPositions.Count; i++)
        {
            // Simple overlap check to avoid double-spawn on reload
            Collider2D hit = Physics2D.OverlapPoint(chickenPositions[i].position);
            if (hit != null && hit.GetComponent<Chicken>() != null)
            {
                zone.AddSlot(hit.transform);
                continue;
            }

            GameObject chicken = SpawnChickenAt(chickenPositions[i].position, "Chicken_" + i);
            if (chicken != null) zone.AddSlot(chicken.transform);
        }

        // Trigger fence refresh AFTER slots are added
        EnvironmentManager.Instance?.RefreshFences();
    }

    private void EnsureCornFieldObjects()
    {
        if (cornFieldPositions.Count == 0) ResolveSceneReferences();

        FarmZoneController zone = GetOrCreateZone("Corn");
        if (zone == null)
        {
            Debug.LogWarning("[GameManager] Corn zone template not found in GameConfig. Fences will not render.");
            return;
        }

        for (int i = 0; i < cornFieldPositions.Count; i++)
        {
            Collider2D hit = Physics2D.OverlapPoint(cornFieldPositions[i].position, 1 << LayerMask.NameToLayer("Environment"));
            if (hit != null && hit.GetComponent<HarvestableField>() != null)
            {
                zone.AddSlot(hit.transform);
                continue;
            }

            GameObject corn = SpawnCornFieldAt(cornFieldPositions[i].position, "CornField_" + i);
            if (corn != null) zone.AddSlot(corn.transform);
        }

        // Trigger fence refresh AFTER slots are added
        EnvironmentManager.Instance?.RefreshFences();
    }

    private GameObject SpawnChickenAt(Vector3 pos, string name = "Chicken")
    {
        GameObject chicken = new GameObject(name);
        chicken.tag = "Chicken";
        chicken.transform.position = pos;

        SpriteRenderer renderer = chicken.AddComponent<SpriteRenderer>();
        renderer.enabled = false;

        CircleCollider2D collider = chicken.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        chicken.AddComponent<Chicken>();

        // NEW: Check for dynamic resource path from zone template fallback
        string resourcePath = RuntimeChickenVisualResourcePath;
        FarmZoneTemplate template = config != null ? config.zoneTemplates.Find(z => z.id == "Chicken") : null;
        if (template != null && !string.IsNullOrEmpty(template.slotObjectResourcePath)) 
            resourcePath = template.slotObjectResourcePath;

        GameObject visualPrefab = Resources.Load<GameObject>(resourcePath);
        if (visualPrefab != null)
        {
            StoryVisualBinder.AttachVisualPrefabAsChild(chicken.transform, visualPrefab, renderer, "Visual", true);
        }
        return chicken;
    }

    private void AttachSoilVisual(Transform target)
    {
        Sprite soilSprite = Resources.Load<Sprite>("HappyHarvestSoil");
        if (soilSprite == null) 
        {
            soilSprite = Resources.Load<Sprite>("Sprite_Tiles_Soil");
        }

        if (soilSprite != null)
        {
            GameObject soilObj = new GameObject("SoilVisual");
            soilObj.transform.SetParent(target, false);
            soilObj.transform.localPosition = new Vector3(0, -0.25f, 0.01f);
            
            SpriteRenderer sr = soilObj.AddComponent<SpriteRenderer>();
            sr.sprite = soilSprite;
            sr.sortingOrder = 2;
            
            soilObj.transform.localScale = Vector3.one * 0.8f; 
        }
        else
        {
            Debug.LogWarning("[GameManager] Could not find 'Sprite_Tiles_Soil' or 'HappyHarvestSoil' sprite in Resources.");
        }
    }

    public FarmZoneController GetZone(string zoneID)
    {
        if (string.IsNullOrWhiteSpace(zoneID))
        {
            return null;
        }

        if (sceneRegistry != null)
        {
            FarmZoneController registryZone = sceneRegistry.GetZone(zoneID);
            if (registryZone != null)
            {
                return registryZone;
            }
        }

        return activeZoneControllers.FirstOrDefault(c => c != null && c.ZoneIdMatches(zoneID));
    }

    private GameObject SpawnFromTemplate(FarmZoneController zone, Transform slot, string name)
    {
        if (zone == null || zone.template == null || string.IsNullOrEmpty(zone.template.slotObjectResourcePath) || slot == null)
        {
            Debug.LogError($"[GameManager] Template missing resource path for zone");
            return null;
        }

        GameObject prefab = Resources.Load<GameObject>(zone.template.slotObjectResourcePath);
        if (prefab == null)
        {
            Debug.LogError($"[GameManager] Could not load prefab: {zone.template.slotObjectResourcePath}");
            return null;
        }

        GameObject instance = Instantiate(prefab, slot.position, Quaternion.identity, slot);
        instance.name = name;

        // Attach appropriate component based on zone type and ID
        if (zone.template.zoneType == ZoneType.Crop)
        {
            if (instance.GetComponent<HarvestableField>() == null)
                instance.AddComponent<HarvestableField>();
            AttachSoilVisual(instance.transform);
            instance.tag = zone.template.id == "Corn" ? "CornField" : zone.template.id;
            
            // Set layer safely
            int envLayer = LayerMask.NameToLayer("Environment");
            if (envLayer >= 0) instance.layer = envLayer;
            
            BoxCollider2D col = instance.GetComponent<BoxCollider2D>();
            if (col == null)
            {
                col = instance.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1f, 1f);
            }
        }
        else if (zone.template.zoneType == ZoneType.Animal)
        {
            // Chickens use Chicken component, other animals just get basic interaction
            if (zone.template.id == "Chicken")
            {
                if (instance.GetComponent<Chicken>() == null)
                    instance.AddComponent<Chicken>();
            }
            else
            {
                // For Cow and other animals, use basic AnimalProduct component
                AnimalProduct animal = instance.GetComponent<AnimalProduct>();
                if (animal == null)
                {
                    animal = instance.AddComponent<AnimalProduct>();
                    animal.Initialize(zone.template.outputItem.id, zone.template.baseProductionTime);
                }
            }
            
            instance.tag = zone.template.id;
            
            CircleCollider2D col = instance.GetComponent<CircleCollider2D>();
            if (col == null)
            {
                col = instance.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.5f;
            }
            
            SpriteRenderer renderer = instance.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.enabled = false;
        }

        return instance;
    }
    
    private GameObject SpawnCornFieldAt(Vector3 pos, string name = "CornField")
    {
        GameObject corn = new GameObject(name);
        corn.tag = "CornField";
        
        // Set layer safely
        int envLayer = LayerMask.NameToLayer("Environment");
        if (envLayer >= 0) corn.layer = envLayer;
        
        corn.transform.position = pos;

        corn.AddComponent<HarvestableField>();
        
        BoxCollider2D collider = corn.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1f);

        SpriteRenderer renderer = corn.AddComponent<SpriteRenderer>();
        renderer.enabled = false;

        // NEW: Check for dynamic resource path
        string resourcePath = RuntimeCornVisualResourcePath;
        FarmZoneTemplate template = config != null ? config.zoneTemplates.Find(z => z.id == "Corn") : null;
        if (template != null && !string.IsNullOrEmpty(template.slotObjectResourcePath)) 
            resourcePath = template.slotObjectResourcePath;

        GameObject visualPrefab = Resources.Load<GameObject>(resourcePath);
        if (visualPrefab != null)
        {
            StoryVisualBinder.AttachVisualPrefabAsChild(corn.transform, visualPrefab, renderer, "Visual", true);
        }

        // Add soil visual under the corn
        AttachSoilVisual(corn.transform);

        return corn;
    }

    private void SpawnZoneDecorations(FarmZoneController zone, Transform anchor)
    {
        if (zone.template == null || zone.template.decorationPrefabs == null || zone.template.decorationPrefabs.Count == 0)
            return;

        // Spawn decorations only for the FIRST item in zone (once per zone, not per slot)
        if (zone.CurrentCount > 1)
            return;

        foreach (var decoPrefab in zone.template.decorationPrefabs)
        {
            if (decoPrefab == null) continue;

            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-0.5f, 0.5f),
                0.1f
            );

            GameObject deco = Instantiate(decoPrefab, anchor.position + offset, Quaternion.identity);
            deco.transform.localScale *= 0.5f;
            
            // Set appropriate sorting order
            foreach (var sr in deco.GetComponentsInChildren<SpriteRenderer>())
            {
                sr.sortingOrder = -5;
            }
        }
    }

    private void EnsureIncubatorObject()
    {
        if (FindFirstObjectByType<Incubator>() != null) return;
        
        GameObject incubator = new GameObject("Incubator");
        incubator.transform.position = new Vector3(8f, 2f, 10f); // Positioned in background near market
        incubator.AddComponent<Incubator>();
    }

    // Purchase Handlers for Market
    public void RegisterWheatPurchase()
    {
        ApplyUpgrade(UpgradeType.WheatField, 1f);
    }

    public void RegisterCowPurchase()
    {
        ApplyUpgrade(UpgradeType.CowPen, 1f);
    }
    
    private void EnsureTitleCardManager()
    {
        if (FindFirstObjectByType<TitleCardManager>() != null) return;

        GameObject manager = new GameObject("TitleCardManager");
        manager.AddComponent<TitleCardManager>();
    }

    // Singular helpers removed in favor of plural EnsureChickenObjects and EnsureCornFieldObjects

    private void EnsureStoreCounterObject()
    {
        StoreCounter existing = FindFirstObjectByType<StoreCounter>();
        if (existing != null)
        {
            existing.transform.position = new Vector3(-10f, 4f, 15f);
            existing.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            existing.originalScale = existing.transform.localScale;
            return;
        }

        GameObject store = new GameObject("StoreCounter");
        store.tag = "Store";
        store.transform.position = new Vector3(-10f, 4f, 15f); // Background
        store.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        
        StoreCounter sc = store.AddComponent<StoreCounter>();
        sc.originalScale = store.transform.localScale;

        SpriteRenderer renderer = store.AddComponent<SpriteRenderer>();
        renderer.enabled = false;
        
        SortingGroup sortingGroup = store.AddComponent<SortingGroup>();
        sortingGroup.sortingOrder = -500; 

        store.AddComponent<StoreCounter>();

        GameObject visualPrefab = Resources.Load<GameObject>(RuntimeStoreVisualResourcePath);
        if (visualPrefab != null)
        {
            // NEW: Use AttachVisualPrefabAsChild for static background objects to ensure 0.05 scale is preserved
            StoryVisualBinder.AttachVisualPrefabAsChild(store.transform, visualPrefab, renderer, "Visual", true);
        }
    }

    private void EnsureEnvironmentDecoration()
    {
        if (GameObject.Find("Environment_Decor") == null)
        {
            Debug.LogWarning("[GameManager] Environment_Decor missing from authored scene.");
        }
    }

    private GameObject SpawnHelperInstance()
    {
        Vector3 spawnPosition = GetHelperSpawnPosition();

        if (helperPrefab != null)
        {
            return Instantiate(helperPrefab, spawnPosition, Quaternion.identity);
        }

        return CreateFallbackHelper(spawnPosition);
    }

    private void RestoreLoadedHelpers()
    {
        if (helperCount <= 0)
        {
            return;
        }

        int existingHelperCount = FindObjectsByType<HelperAI>(FindObjectsSortMode.None).Length;
        if (existingHelperCount >= helperCount)
        {
            return;
        }

        int savedHelperCount = helperCount;
        for (int i = existingHelperCount; i < savedHelperCount; i++)
        {
            helperCount = i + 1;
            SpawnHelperInstance();
        }

        helperCount = savedHelperCount;
    }

    private Vector3 GetHelperSpawnPosition()
    {
        if (helperSpawnPoint != null)
        {
            return helperSpawnPoint.position;
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            return player.transform.position + new Vector3(0.8f, -0.4f, 0f);
        }

        return transform.position + new Vector3(0f, -1f, 0f);
    }

    private GameObject CreateFallbackHelper(Vector3 spawnPosition)
    {
        GameObject helper = new GameObject($"Helper_{helperCount}");
        // Push slightly in front to avoid background clipping (Z = -1)
        helper.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, -1f);
        helper.transform.localScale = Vector3.one * 0.9f;

        SpriteRenderer helperRenderer = helper.AddComponent<SpriteRenderer>();
        helperRenderer.sprite = GetHelperSprite();
        helperRenderer.color = StoryColorPalette.GetHelperColor(helperCount);
        helperRenderer.sortingOrder = 5000;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            helper.layer = player.gameObject.layer;
            helper.transform.localScale = player.transform.localScale * 0.9f;

            SpriteRenderer playerRenderer = null;
            SpriteRenderer[] playerRenderers = player.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer candidate in playerRenderers)
            {
                if (candidate != null && candidate.enabled && candidate.sprite != null)
                {
                    playerRenderer = candidate;
                    break;
                }
            }

            if (playerRenderer == null)
            {
                playerRenderer = player.GetComponent<SpriteRenderer>();
            }

            if (playerRenderer != null)
            {
                helperRenderer.sortingLayerID = playerRenderer.sortingLayerID;
                helperRenderer.sortingOrder = 5000; 
                if (playerRenderer.sprite != null)
                {
                    helperRenderer.sprite = playerRenderer.sprite;
                }
            }
        }

        // Ensure helper is always visible with a SortingGroup
        var sGroup = helper.GetComponent<UnityEngine.Rendering.SortingGroup>();
        if (sGroup == null) sGroup = helper.AddComponent<UnityEngine.Rendering.SortingGroup>();
        sGroup.sortingOrder = 10; // In front of standard world objects

        GameObject happyHarvestCharacter = Resources.Load<GameObject>("Character");
        if (happyHarvestCharacter != null)
        {
            GameObject visualInstance = StoryVisualBinder.AttachVisualPrefab(
                helper.transform, happyHarvestCharacter, helperRenderer, true);
            if (visualInstance != null)
            {
                StoryVisualBinder.ApplySpriteLibrary(visualInstance, "HappyHarvestFarmer");
                visualInstance.transform.localScale = Vector3.one * 0.4f;
                StoryVisualFollower follower = visualInstance.GetComponent<StoryVisualFollower>();
                if (follower != null) follower.offset = new Vector3(0f, -0.35f, 0f);

                // Add SortingGroup and set high order to stay in front of background
                UnityEngine.Rendering.SortingGroup visualSGroup = visualInstance.GetComponent<UnityEngine.Rendering.SortingGroup>();
                if (visualSGroup == null) visualSGroup = visualInstance.AddComponent<UnityEngine.Rendering.SortingGroup>();
                visualSGroup.sortingOrder = 5000;

                // Ensure all renderers are enabled and correctly colored
                SpriteRenderer[] renderers = visualInstance.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (SpriteRenderer r in renderers)
                {
                    if (r != null)
                    {
                        r.enabled = true;
                        r.color = Color.Lerp(r.color, StoryColorPalette.GetHelperColor(helperCount), 0.25f);
                    }
                }
            }
        }

        helper.AddComponent<HelperAI>();
        return helper;
    }

    private Sprite GetHelperSprite()
    {
        if (runtimeHelperSprite != null)
        {
            return runtimeHelperSprite;
        }

        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        runtimeHelperSprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            1f);

        return runtimeHelperSprite;
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

    /// <summary>
    /// Returns the current game time as a formatted string (e.g. 12:00)
    /// </summary>
    public string CurrentTimeAsString()
    {
        DayNightCycle cycle = FindFirstObjectByType<DayNightCycle>();
        return cycle != null ? cycle.GetTimeString() : "00:00";
    }
}

    public enum UpgradeType
    {
        WheatField,
        ChickenCare,
        CowPen,
        CowFeed,
        MilkProduction,
        CarrotGarden,
        Fertilizer,
        PigPen,
        HelperSpeed,
        BiggerStore
    }
}
