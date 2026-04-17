using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

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

    // Data-Driven Zone Management
    private List<FarmZoneController> activeZoneControllers = new List<FarmZoneController>();
    public List<FarmZoneController> ActiveZoneControllers => activeZoneControllers;
    private Dictionary<string, int> inventory = new Dictionary<string, int>();

    // Current resource counts (Maintained for UI backward compatibility)
    private int corn;
    private int eggs;
    private int coins;
    private int helperCount;

    // Expansion Flags (Free after first purchase)
    private bool hasPurchasedWheat = false;
    private bool hasPurchasedCow = false;

    // Upgrade multipliers
    private float cornMultiplier = 1f;
    private float eggMultiplier = 1f;
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
    public int EggSellPrice => Mathf.RoundToInt(GetEggSellPrice() * priceMultiplier);
    public int HelperCost => GetHelperBaseCost() + (helperCount * GetHelperCostIncrease());
    public float SpeedMultiplier => speedMultiplier;
    public float StoreEfficiencyMultiplier => storeEfficiencyMultiplier;
    public bool HasPurchasedWheat => hasPurchasedWheat;
    public bool HasPurchasedCow => hasPurchasedCow;
    public PlayerController Player => FindObjectOfType<PlayerController>();
    
    // Leveling State
    private int currentAct = 1;
    public int CurrentAct => currentAct;

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
    public Transform StorePosition => storePosition;

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
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            Debug.Log("[GameManager] Created missing EventSystem for UI support.");
        }
        else
        {
            // Resolve duplicate event systems in the scene
            EventSystem[] systems = FindObjectsOfType<EventSystem>();
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
        corn = GetStartingCorn();
        eggs = GetStartingEggs();
        coins = GetStartingCoins();
        helperCount = 0;
        currentAct = 1;

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

    public bool UseCorn(int amount) => UseItem("Corn", amount);
    public bool UseEggs(int amount) => UseItem("Egg", amount);
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
        // Null-safe search for existing controller
        FarmZoneController controller = activeZoneControllers.Find(c => c != null && c.template != null && c.template.id == zoneID);
        
        if (controller == null)
        {
            FarmZoneTemplate template = (config != null && config.zoneTemplates != null) 
                ? config.zoneTemplates.Find(t => t != null && t.id == zoneID) 
                : null;
            
            // Proper runtime initialization if template missing from config
            if (template == null)
            {
                template = CreateProperTemplate(zoneID);
            }

            GameObject obj = (zoneControllerPrefab != null) ? Instantiate(zoneControllerPrefab) : new GameObject("Zone_" + zoneID);
            controller = obj.GetComponent<FarmZoneController>();
            if (controller == null) controller = obj.AddComponent<FarmZoneController>();
            
            controller.Initialize(template);
            
            // Positioning Logic: Shifted strictly to the right (+X) to prevent overlaps
            float startX = -5f; // Start farm from -5.0
            if (activeZoneControllers.Count > 0)
            {
                var lastZone = activeZoneControllers[activeZoneControllers.Count - 1];
                startX = lastZone.transform.position.x + lastZone.template.GetTotalWidth() + 10.0f; // 10.0f buffer to prevent overlap
            }
            
            obj.transform.position = new Vector3(startX, -1f, 0f);
            activeZoneControllers.Add(controller);
            EnvironmentManager.Instance?.RefreshFences();
            
            Debug.Log($"[GameManager] Initialized Zone: {zoneID} with template: {template.id} at {obj.transform.position}");
        }
        return controller;
    }

    private FarmZoneTemplate CreateProperTemplate(string zoneID)
    {
        Debug.LogWarning($"[GameManager] Template for {zoneID} not found. Initializing proper runtime template.");
        FarmZoneTemplate template = ScriptableObject.CreateInstance<FarmZoneTemplate>();
        template.id = zoneID;
        template.maxSlots = 9; // Enforce 3x3 grid limit
        
        // Square Pens: Set itemsPerRow to 3 and spacing to be symmetric (2.0)
        template.itemsPerRow = 3;
        template.spacing = new Vector2(2.0f, 2.0f);
        template.zonePadding = 2.0f;
        
        // Decorations: Add Rock and Bush prefabs to the corners
        template.decorationPrefabs = new List<GameObject>();
        GameObject rock = Resources.Load<GameObject>("Env_Rock_01");
        GameObject bush = Resources.Load<GameObject>("Env_Bush_01");
        if (rock != null) template.decorationPrefabs.Add(rock);
        if (bush != null) template.decorationPrefabs.Add(bush);

        template.baseUnlockCost = 100;
        template.costPerAdditionalSlot = 50;

        // Initialize Items
        FarmItemDefinition corn = ScriptableObject.CreateInstance<FarmItemDefinition>();
        corn.id = "Corn";
        corn.displayName = "Corn";
        corn.basePrice = 5;

        FarmItemDefinition egg = ScriptableObject.CreateInstance<FarmItemDefinition>();
        egg.id = "Egg";
        egg.displayName = "Egg";
        egg.basePrice = 10;

        FarmItemDefinition wheat = ScriptableObject.CreateInstance<FarmItemDefinition>();
        wheat.id = "Wheat";
        wheat.displayName = "Wheat";
        wheat.basePrice = 15;

        FarmItemDefinition milk = ScriptableObject.CreateInstance<FarmItemDefinition>();
        milk.id = "Milk";
        milk.displayName = "Milk";
        milk.basePrice = 40;

        if (zoneID == "Chicken")
        {
            template.displayName = "Chicken Pen";
            template.zoneType = ZoneType.Animal;
            template.slotObjectResourcePath = "HappyHarvestChicken";
            template.inputItem = corn;
            template.outputItem = egg;
            template.baseProductionTime = 5.0f;
        }
        else if (zoneID == "Corn")
        {
            template.displayName = "Corn Field";
            template.zoneType = ZoneType.Crop;
            template.slotObjectResourcePath = "HappyHarvestCorn";
            template.outputItem = corn;
            template.baseProductionTime = 2.0f;
        }
        else if (zoneID == "Wheat")
        {
            template.displayName = "Wheat Field";
            template.zoneType = ZoneType.Crop;
            template.slotObjectResourcePath = "HappyHarvestWheat";
            template.outputItem = wheat;
            template.baseProductionTime = 4.0f;
        }
        else if (zoneID == "Cow")
        {
            template.displayName = "Cow Pen";
            template.zoneType = ZoneType.Animal;
            template.slotObjectResourcePath = "HappyHarvestCow";
            template.inputItem = wheat;
            template.outputItem = milk;
            template.baseProductionTime = 12.0f;
        }

        return template;
    }
    public void AddObjectToZone(string zoneID)
    {
        FarmZoneController zone = GetOrCreateZone(zoneID);
        if (zone == null) return;

        bool canAfford = false;
        
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
            int cost = (zone.CurrentCount == 0) ? zone.template.baseUnlockCost : zone.template.costPerAdditionalSlot;
            if (coins >= cost)
            {
                canAfford = UseCoins(cost);
            }
        }

        if (canAfford)
        {
            Vector3 pos = zone.GetNextSlotPosition();
            GameObject instance = null;

            if (zoneID == "Chicken")
                instance = SpawnChickenAt(pos, $"Chicken_{zone.CurrentCount}");
            else if (zoneID == "Corn")
                instance = SpawnCornFieldAt(pos, $"CornField_{zone.CurrentCount}");
            else if (zoneID == "Wheat")
                instance = SpawnCornFieldAt(pos, $"WheatField_{zone.CurrentCount}"); // Wheat Field Logic
            else if (zoneID == "Cow")
                instance = SpawnChickenAt(pos, $"Cow_{zone.CurrentCount}"); // Cow asset path handled in template
            else
            {
                string resourcePath = zone.template.slotObjectResourcePath;
                GameObject prefab = Resources.Load<GameObject>(resourcePath);
                if (prefab != null) instance = Instantiate(prefab, pos, Quaternion.identity);
                if (instance != null && zone.template.zoneType == ZoneType.Crop) AttachSoilVisual(instance.transform);
            }

            if (instance != null)
            {
                zone.AddSlot(instance.transform);
                OnZoneExpanded?.Invoke(zoneID, zone.CurrentCount);
                
                EnvironmentManager.Instance?.RefreshFences();
                UpdateExpansionButtons();
            }
        }
    }

    private void UpdateExpansionButtons()
    {
        // Tell UIManager to Refresh its expansion buttons
        UIManager.Instance?.UpdateExpansionButtons();
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
                eggMultiplier *= 1.5f; // Buffed from 1.2f
                break;
            case UpgradeType.EggPrice:
                priceMultiplier *= 1.5f; // Buffed from 1.2f
                break;
            case UpgradeType.Speed:
                speedMultiplier *= multiplier;
                break;
            case UpgradeType.StoreCapacity:
                storeEfficiencyMultiplier *= multiplier;
                break;
        }

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
        corn = GetStartingCorn();
        eggs = GetStartingEggs();
        coins = GetStartingCoins();
        helperCount = 0;
        cornMultiplier = 1f;
        eggMultiplier = 1f;
        priceMultiplier = 1f;
        speedMultiplier = 1f;
        storeEfficiencyMultiplier = 1f;

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
        PlayerPrefs.SetFloat("StoreEfficiencyMultiplier", storeEfficiencyMultiplier);
        PlayerPrefs.SetInt("HasPurchasedWheat", hasPurchasedWheat ? 1 : 0);
        PlayerPrefs.SetInt("HasPurchasedCow", hasPurchasedCow ? 1 : 0);
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
        cornMultiplier = PlayerPrefs.GetFloat("CornMultiplier", 1f);
        eggMultiplier = PlayerPrefs.GetFloat("EggMultiplier", 1f);
        priceMultiplier = PlayerPrefs.GetFloat("PriceMultiplier", 1f);
        speedMultiplier = PlayerPrefs.GetFloat("SpeedMultiplier", 1f);
        storeEfficiencyMultiplier = PlayerPrefs.GetFloat("StoreEfficiencyMultiplier", 1f);
        hasPurchasedWheat = PlayerPrefs.GetInt("HasPurchasedWheat", 0) == 1;
        hasPurchasedCow = PlayerPrefs.GetInt("HasPurchasedCow", 0) == 1;

        RestoreLoadedHelpers();

        // Update UI
        OnCornChanged?.Invoke(corn);
        OnEggsChanged?.Invoke(eggs);
        OnCoinsChanged?.Invoke(coins);
        OnHelperCountChanged?.Invoke(helperCount);
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
        if (cornFieldPositions.Count == 0)
        {
            HarvestableField field = FindObjectOfType<HarvestableField>();
            if (field != null)
            {
                cornFieldPositions.Add(field.transform);
            }
        }

        if (chickenPositions.Count == 0)
        {
            Chicken chicken = FindObjectOfType<Chicken>();
            if (chicken != null)
            {
                chickenPositions.Add(chicken.transform);
            }
        }

        if (storePosition == null)
        {
            StoreCounter storeCounter = FindObjectOfType<StoreCounter>();
            if (storeCounter != null)
            {
                storePosition = storeCounter.transform;
            }
        }

        if (helperSpawnPoint == null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                helperSpawnPoint = player.transform;
            }
        }
    }

    private void EnsureRuntimeStorySupport()
    {
        ResolveSceneReferences();
        EnsureCoreGameplayObjects();
        ResolveSceneReferences();

        if (FindObjectOfType<FloatingTextManager>() == null)
        {
            new GameObject("FloatingTextManager").AddComponent<FloatingTextManager>();
        }

        if (FindObjectOfType<EnvironmentAnimator>() == null)
        {
            EnvironmentAnimator environmentAnimator = new GameObject("EnvironmentAnimator").AddComponent<EnvironmentAnimator>();
            environmentAnimator.CreateAmbientParticles();
        }

        if (FindObjectOfType<EnvironmentManager>() == null)
        {
            new GameObject("EnvironmentManager").AddComponent<EnvironmentManager>();
        }

        if (FindObjectOfType<UIManager>() == null)
        {
            new GameObject("UIManager").AddComponent<UIManager>();
        }

        if (FindObjectOfType<DayNightCycle>() == null)
        {
            GameObject dayNightHost = Camera.main != null ? Camera.main.gameObject : new GameObject("DayNightCycle");
            DayNightCycle cycle = dayNightHost.GetComponent<DayNightCycle>();
            if (cycle == null)
            {
                cycle = dayNightHost.AddComponent<DayNightCycle>();
            }

            cycle.SetTimeOfDay(0.23f);
        }

        if (FindObjectOfType<TutorialManager>() == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            GameObject tutorialHost = canvas != null ? canvas.gameObject : gameObject;
            tutorialHost.AddComponent<TutorialManager>();
        }
        
        if (FindObjectOfType<TitleCardManager>() == null)
        {
            new GameObject("TitleCardManager").AddComponent<TitleCardManager>();
        }

        EnsureEnvironmentDecoration();

        // Add Global Light 2D if missing for URP 2D
        if (FindObjectOfType<UnityEngine.Rendering.Universal.Light2D>() == null)
        {
            GameObject lightHost = new GameObject("Global Light 2D");
            var light = lightHost.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
            light.intensity = 1.0f;
        }
    }

    private void EnsureCoreGameplayObjects()
    {
        EnsureChickenObjects();
        EnsureCornFieldObjects();
        EnsureStoreCounterObject();
        EnsureIncubatorObject();
        EnsureTitleCardManager();
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

    private GameObject SpawnCornFieldAt(Vector3 pos, string name = "CornField")
    {
        GameObject corn = new GameObject(name);
        corn.tag = "CornField";
        corn.layer = LayerMask.NameToLayer("Environment");
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

    private void AttachSoilVisual(Transform target)
    {
        // Load the Soil sprite from the Happy Harvest path
        Sprite soilSprite = Resources.Load<Sprite>("HappyHarvestSoil");
        if (soilSprite == null) 
        {
            // Fallback to specific user requested name (ensure it's in a Resources folder)
            soilSprite = Resources.Load<Sprite>("Sprite_Tiles_Soil");
        }

        if (soilSprite != null)
        {
            GameObject soilObj = new GameObject("SoilVisual");
            soilObj.transform.SetParent(target, false);
            soilObj.transform.localPosition = new Vector3(0, -0.2f, 0.01f); // Move closest to stalk while staying behind
            
            SpriteRenderer sr = soilObj.AddComponent<SpriteRenderer>();
            sr.sprite = soilSprite;
            sr.sortingOrder = -10; // Ensure it's behind the stalk
            
            // Adjust scale to fit the slot nicely
            soilObj.transform.localScale = Vector3.one * 1.5f; 
        }
        else
        {
            Debug.LogWarning("[GameManager] Could not find 'Sprite_Tiles_Soil' or 'HappyHarvestSoil' sprite in Resources.");
        }
    }

    private void EnsureIncubatorObject()
    {
        if (FindObjectOfType<Incubator>() != null) return;
        
        GameObject incubator = new GameObject("Incubator");
        incubator.transform.position = new Vector3(8f, 2f, 10f); // Positioned in background near market
        incubator.AddComponent<Incubator>();
    }

    private void EnsureTitleCardManager()
    {
        if (FindObjectOfType<TitleCardManager>() != null) return;

        GameObject manager = new GameObject("TitleCardManager");
    }

    // Purchase Handlers for Market
    public void RegisterWheatPurchase()
    {
        hasPurchasedWheat = true;
        OnZoneExpanded?.Invoke("Wheat", 0); // Trigger UI refresh
        SaveGame();
    }

    public void RegisterCowPurchase()
    {
        hasPurchasedCow = true;
        OnZoneExpanded?.Invoke("Cow", 0); // Trigger UI refresh
        SaveGame();
    }
    
    private void EnsureTitleCardManager()
    {
        if (FindObjectOfType<TitleCardManager>() != null) return;

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
        if (GameObject.Find("Environment_Decor") != null) return;
        
        GameObject decorRoot = new GameObject("Environment_Decor");
        decorRoot.transform.position = Vector3.zero;

        // 1. Distant Backdrops (Trees)
        string[] treePrefabs = { "Env_Tree_01", "Env_Tree_02", "Env_Tree_03", "Env_Tree_05" };
        for (int i = 0; i < 20; i++)
        {
            string prefabName = treePrefabs[UnityEngine.Random.Range(0, treePrefabs.Length)];
            GameObject treePrefab = Resources.Load<GameObject>(prefabName);
            if (treePrefab == null) continue;

            Vector3 pos = new Vector3(
                UnityEngine.Random.Range(-30f, 30f),
                UnityEngine.Random.Range(10f, 22f),
                UnityEngine.Random.Range(20f, 30f)
            );
            
            GameObject tree = Instantiate(treePrefab, pos, Quaternion.identity, decorRoot.transform);
            tree.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.15f, 0.25f);
            foreach (var r in tree.GetComponentsInChildren<SpriteRenderer>()) r.sortingOrder = -1500;
        }

        // 2. Mid-ground Accents (Flowers and Bushes)
        string[] accentPrefabs = { "Env_Flower_01", "Env_Flower_02", "Env_GrassPlant_02", "Env_Rock_02" };
        for (int i = 0; i < 40; i++)
        {
            string prefabName = accentPrefabs[UnityEngine.Random.Range(0, accentPrefabs.Length)];
            GameObject accentPrefab = Resources.Load<GameObject>(prefabName);
            if (accentPrefab == null) continue;

            Vector3 pos = new Vector3(
                UnityEngine.Random.Range(-20f, 20f),
                UnityEngine.Random.Range(-10f, 15f),
                12f
            );
            
            GameObject accent = Instantiate(accentPrefab, pos, Quaternion.identity, decorRoot.transform);
            accent.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.08f, 0.12f);
            foreach (var r in accent.GetComponentsInChildren<SpriteRenderer>()) r.sortingOrder = -900;
        }

        // 3. Ground Texture (Grass Clumps)
        GameObject grassPrefab = Resources.Load<GameObject>("Env_GrassPlant_05");
        if (grassPrefab != null)
        {
            for (int i = 0; i < 50; i++)
            {
                Vector3 pos = new Vector3(
                    UnityEngine.Random.Range(-18f, 18f),
                    UnityEngine.Random.Range(-12f, 12f),
                    10f
                );
                GameObject grass = Instantiate(grassPrefab, pos, Quaternion.identity, decorRoot.transform);
                grass.transform.localScale = Vector3.one * 0.08f;
                foreach (var r in grass.GetComponentsInChildren<SpriteRenderer>()) r.sortingOrder = -950;
            }
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

        int existingHelperCount = FindObjectsOfType<HelperAI>().Length;
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

        PlayerController player = FindObjectOfType<PlayerController>();
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

        PlayerController player = FindObjectOfType<PlayerController>();
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
        DayNightCycle cycle = FindObjectOfType<DayNightCycle>();
        return cycle != null ? cycle.GetTimeString() : "00:00";
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
}
