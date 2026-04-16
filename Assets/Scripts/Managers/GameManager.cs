using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

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
    [SerializeField] private List<Transform> cornFieldPositions = new List<Transform>();
    [SerializeField] private List<Transform> chickenPositions = new List<Transform>();
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
    private float storeEfficiencyMultiplier = 1f;

    private static Sprite runtimeHelperSprite;

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
    public PlayerController Player => FindObjectOfType<PlayerController>();
    
    // Leveling State
    private int currentLevel = 1;
    public int CurrentLevel => currentLevel;

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
    }

    private void Start()
    {
        StartCoroutine(BootstrapStorySupport());
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
        currentLevel = 1;

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
        int actualAmount = Mathf.CeilToInt(amount * eggMultiplier);
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

    public bool UseEggs(int amount)
    {
        if (eggs >= amount)
        {
            eggs -= amount;
            OnEggsChanged?.Invoke(eggs);
            return true;
        }
        return false;
    }

    public void AddChicken()
    {
        if (chickenPositions.Count < 6 && eggs >= 1)
        {
            if (UseEggs(1))
            {
                Vector3 origin = chickenPositions[0].position;
                Vector3 nextGridPos = GetNextGridPosition(chickenPositions.Count, origin, 1.8f, 1.4f);
                
                GameObject newPosObj = new GameObject("ChickenPos_" + chickenPositions.Count);
                newPosObj.transform.position = nextGridPos;
                chickenPositions.Add(newPosObj.transform);
                
                SpawnChickenAt(newPosObj.transform.position, "Chicken_" + (chickenPositions.Count - 1));
                
                if (chickenPositions.Count == 2) currentLevel = 2;
                if (chickenPositions.Count >= 3) currentLevel = 3;
            }
        }
    }

    public void AddCornField()
    {
        if (cornFieldPositions.Count < 6 && corn >= 1)
        {
            if (UseCorn(1))
            {
                Vector3 origin = cornFieldPositions[0].position;
                Vector3 nextGridPos = GetNextGridPosition(cornFieldPositions.Count, origin, 1.8f, 1.4f);
                
                GameObject newPosObj = new GameObject("CornPos_" + cornFieldPositions.Count);
                newPosObj.transform.position = nextGridPos;
                cornFieldPositions.Add(newPosObj.transform);
                
                SpawnCornFieldAt(newPosObj.transform.position, "CornField_" + (cornFieldPositions.Count - 1));
            }
        }
    }

    private Vector3 GetNextGridPosition(int index, Vector3 origin, float spacingX, float spacingY)
    {
        int col = index % 3;
        int row = index / 3;
        // Expand left (-X) and up (+Y) into the background
        return origin + new Vector3(-col * spacingX, row * spacingY, 0);
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
        // Remove existing if any to refresh from list (or avoid duplicates if we want persistent ones)
        // For simplicity in this self-healing script, we'll ensure at least one per position
        for (int i = 0; i < chickenPositions.Count; i++)
        {
            SpawnChickenAt(chickenPositions[i].position, "Chicken_" + i);
        }
    }

    private void EnsureCornFieldObjects()
    {
        for (int i = 0; i < cornFieldPositions.Count; i++)
        {
            SpawnCornFieldAt(cornFieldPositions[i].position, "CornField_" + i);
        }
    }

    private void SpawnChickenAt(Vector3 pos, string name = "Chicken")
    {
        // Simple overlap check
        Collider2D hit = Physics2D.OverlapPoint(pos);
        if (hit != null && hit.GetComponent<Chicken>() != null) return;

        GameObject chicken = new GameObject(name);
        chicken.tag = "Chicken";
        chicken.transform.position = pos;

        SpriteRenderer renderer = chicken.AddComponent<SpriteRenderer>();
        renderer.enabled = false;

        CircleCollider2D collider = chicken.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        chicken.AddComponent<Chicken>();

        GameObject visualPrefab = Resources.Load<GameObject>(RuntimeChickenVisualResourcePath);
        if (visualPrefab != null)
        {
            StoryVisualBinder.AttachVisualPrefabAsChild(chicken.transform, visualPrefab, renderer, "Visual", true);
        }
    }

    private void SpawnCornFieldAt(Vector3 pos, string name = "CornField")
    {
        Collider2D hit = Physics2D.OverlapPoint(pos);
        if (hit != null && hit.GetComponent<HarvestableField>() != null) return;

        GameObject corn = new GameObject(name);
        corn.tag = "CornField";
        corn.transform.position = pos;

        SpriteRenderer renderer = corn.AddComponent<SpriteRenderer>();
        renderer.enabled = false;

        BoxCollider2D collider = corn.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1f);

        corn.AddComponent<HarvestableField>();

        GameObject visualPrefab = Resources.Load<GameObject>(RuntimeCornVisualResourcePath);
        if (visualPrefab != null)
        {
            StoryVisualBinder.AttachVisualPrefabAsChild(corn.transform, visualPrefab, renderer, "Visual", true);
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
        manager.AddComponent<TitleCardManager>();
    }

    // Singular helpers removed in favor of plural EnsureChickenObjects and EnsureCornFieldObjects

    private void EnsureStoreCounterObject()
    {
        StoreCounter existing = FindFirstObjectByType<StoreCounter>();
        if (existing != null)
        {
            existing.transform.position = new Vector3(-10f, 4f, 15f);
            existing.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
            return;
        }

        GameObject store = new GameObject("StoreCounter");
        store.tag = "Store";
        store.transform.position = new Vector3(-10f, 4f, 15f); // Background
        store.transform.localScale = new Vector3(0.1f, 0.1f, 1f);

        SpriteRenderer renderer = store.AddComponent<SpriteRenderer>();
        renderer.enabled = false;
        
        SortingGroup sortingGroup = store.AddComponent<SortingGroup>();
        sortingGroup.sortingOrder = -500; 

        store.AddComponent<StoreCounter>();

        GameObject visualPrefab = Resources.Load<GameObject>(RuntimeStoreVisualResourcePath);
        if (visualPrefab != null)
        {
            StoryVisualBinder.AttachVisualPrefab(store.transform, visualPrefab, renderer, true);
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
                // Force helpers to stay in front (5000) regardless of player's relative order
                helperRenderer.sortingOrder = 5000; 
                if (playerRenderer.sprite != null)
                {
                    helperRenderer.sprite = playerRenderer.sprite;
                }
            }
        }

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
                UnityEngine.Rendering.SortingGroup sGroup = visualInstance.GetComponent<UnityEngine.Rendering.SortingGroup>();
                if (sGroup == null) sGroup = visualInstance.AddComponent<UnityEngine.Rendering.SortingGroup>();
                sGroup.sortingOrder = 5000;

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
