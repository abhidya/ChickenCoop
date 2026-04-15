using UnityEngine;
using System;
using System.Collections;

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
            ResolveSceneReferences();
        }
        else
        {
            Destroy(gameObject);
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
                eggMultiplier *= multiplier;
                break;
            case UpgradeType.EggPrice:
                priceMultiplier *= multiplier;
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
        if (cornFieldPosition == null)
        {
            HarvestableField field = FindAnyObjectByType<HarvestableField>();
            if (field != null)
            {
                cornFieldPosition = field.transform;
            }
        }

        if (chickenPosition == null)
        {
            Chicken chicken = FindAnyObjectByType<Chicken>();
            if (chicken != null)
            {
                chickenPosition = chicken.transform;
            }
        }

        if (storePosition == null)
        {
            StoreCounter storeCounter = FindAnyObjectByType<StoreCounter>();
            if (storeCounter != null)
            {
                storePosition = storeCounter.transform;
            }
        }

        if (helperSpawnPoint == null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
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

        if (FindAnyObjectByType<FloatingTextManager>() == null)
        {
            new GameObject("FloatingTextManager").AddComponent<FloatingTextManager>();
        }

        if (FindAnyObjectByType<EnvironmentAnimator>() == null)
        {
            EnvironmentAnimator environmentAnimator = new GameObject("EnvironmentAnimator").AddComponent<EnvironmentAnimator>();
            environmentAnimator.CreateAmbientParticles();
        }

        if (FindAnyObjectByType<DayNightCycle>() == null)
        {
            GameObject dayNightHost = Camera.main != null ? Camera.main.gameObject : new GameObject("DayNightCycle");
            DayNightCycle cycle = dayNightHost.GetComponent<DayNightCycle>();
            if (cycle == null)
            {
                cycle = dayNightHost.AddComponent<DayNightCycle>();
            }

            cycle.SetTimeOfDay(0.23f);
        }

        if (FindAnyObjectByType<TutorialManager>() == null)
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            GameObject tutorialHost = canvas != null ? canvas.gameObject : gameObject;
            tutorialHost.AddComponent<TutorialManager>();
        }

        // Add Global Light 2D if missing for URP 2D
        if (FindAnyObjectByType<UnityEngine.Rendering.Universal.Light2D>() == null)
        {
            GameObject lightHost = new GameObject("Global Light 2D");
            var light = lightHost.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
            light.intensity = 1.0f;
        }
    }

    private void EnsureCoreGameplayObjects()
    {
        EnsureChickenObject();
        EnsureCornFieldObject();
        EnsureStoreCounterObject();
    }

    private void EnsureChickenObject()
    {
        if (FindAnyObjectByType<Chicken>() != null)
        {
            return;
        }

        GameObject chicken = new GameObject("Chicken");
        chicken.tag = "Chicken";
        chicken.transform.position = new Vector3(2f, 0f, 0f);

        SpriteRenderer renderer = chicken.AddComponent<SpriteRenderer>();
        renderer.enabled = false;
        renderer.sortingOrder = 10;

        CircleCollider2D collider = chicken.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        chicken.AddComponent<Chicken>();

        GameObject visualPrefab = Resources.Load<GameObject>(RuntimeChickenVisualResourcePath);
        if (visualPrefab != null)
        {
            GameObject visual = StoryVisualBinder.AttachVisualPrefabAsChild(chicken.transform, visualPrefab, renderer, "Visual", true);
            if (visual != null)
            {
                // Safely guarantee Chicken draws over environment geometry by applying a master SortingGroup.
                UnityEngine.Rendering.SortingGroup sGroup = chicken.GetComponent<UnityEngine.Rendering.SortingGroup>();
                if (sGroup == null) sGroup = chicken.AddComponent<UnityEngine.Rendering.SortingGroup>();
                sGroup.sortingOrder = 5000;
            }
        }
    }

    private void EnsureCornFieldObject()
    {
        if (FindAnyObjectByType<HarvestableField>() != null)
        {
            return;
        }

        GameObject cornField = new GameObject("CornField");
        cornField.transform.position = new Vector3(-4f, 0f, 0f);

        SpriteRenderer renderer = cornField.AddComponent<SpriteRenderer>();
        renderer.enabled = false;
        renderer.sortingOrder = 5;

        BoxCollider2D collider = cornField.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1.5f, 1.5f);

        cornField.AddComponent<HarvestableField>();

        GameObject visualPrefab = Resources.Load<GameObject>(RuntimeCornVisualResourcePath);
        if (visualPrefab != null)
        {
            StoryVisualBinder.AttachVisualPrefab(cornField.transform, visualPrefab, renderer, true);
        }
    }

    private void EnsureStoreCounterObject()
    {
        if (FindAnyObjectByType<StoreCounter>() != null)
        {
            return;
        }

        GameObject store = new GameObject("StoreCounter");
        store.tag = "Store";
        store.transform.position = new Vector3(4f, 0f, 0f);

        SpriteRenderer renderer = store.AddComponent<SpriteRenderer>();
        renderer.enabled = false;
        renderer.sortingOrder = 5;

        BoxCollider2D collider = store.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1.5f, 1.5f);

        store.AddComponent<StoreCounter>();

        GameObject visualPrefab = Resources.Load<GameObject>(RuntimeStoreVisualResourcePath);
        if (visualPrefab != null)
        {
            StoryVisualBinder.AttachVisualPrefab(store.transform, visualPrefab, renderer, true);
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

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            return player.transform.position + new Vector3(0.8f, -0.4f, 0f);
        }

        return transform.position + new Vector3(0f, -1f, 0f);
    }

    private GameObject CreateFallbackHelper(Vector3 spawnPosition)
    {
        GameObject helper = new GameObject($"Helper_{helperCount}");
        helper.transform.position = spawnPosition;
        helper.transform.localScale = Vector3.one * 0.9f;

        SpriteRenderer helperRenderer = helper.AddComponent<SpriteRenderer>();
        helperRenderer.sprite = GetHelperSprite();
        helperRenderer.color = StoryColorPalette.GetHelperColor(helperCount);
        helperRenderer.sortingOrder = 10;

        PlayerController player = FindAnyObjectByType<PlayerController>();
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
                helperRenderer.sortingOrder = playerRenderer.sortingOrder;
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

                UnityEngine.Rendering.SortingGroup sGroup = visualInstance.GetComponent<UnityEngine.Rendering.SortingGroup>();
                if (sGroup == null) sGroup = visualInstance.AddComponent<UnityEngine.Rendering.SortingGroup>();
                sGroup.sortingOrder = 5000;

                SpriteRenderer[] renderers = visualInstance.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (SpriteRenderer renderer in renderers)
                {
                    if (renderer != null)
                    {
                        renderer.color = Color.Lerp(renderer.color, StoryColorPalette.GetHelperColor(helperCount), 0.25f);
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
