using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UIManager - Manages all UI elements including resource displays, upgrade buttons,
/// and visual feedback animations like number tweening and punch-scale effects.
/// </summary>
public class UIManager : MonoBehaviour
{
    public const int StoryUpgradeCount = 5;
    public static readonly int[] StoryUpgradeCosts = { 100, 200, 300, 500, 750 };
    public static readonly string[] StoryUpgradeNames =
    {
        "Better Seeds",
        "Healthier Chickens",
        "Premium Eggs",
        "Faster Operations",
        "Bigger Store"
    };

    public static UIManager Instance { get; private set; }

    [Header("Resource Displays")]
    [SerializeField] private TextMeshProUGUI cornCountText;
    [SerializeField] private TextMeshProUGUI eggsCountText;
    [SerializeField] private TextMeshProUGUI coinsCountText;
    [SerializeField] private TextMeshProUGUI helperCountText;
    [SerializeField] private TextMeshProUGUI incomeRateText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI storyLevelText;

    [Header("Resource Icons")]
    [SerializeField] private RectTransform cornIcon;
    [SerializeField] private RectTransform eggsIcon;
    [SerializeField] private RectTransform coinsIcon;

    [Header("Action Buttons")]
    [SerializeField] private Button harvestButton;
    [SerializeField] private Button feedButton;
    [SerializeField] private Button collectButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button hireHelperButton;
    [SerializeField] private Button incubateButton;
    [SerializeField] private Button plantButton;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Upgrade System")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private TextMeshProUGUI[] upgradeCostTexts;
    [SerializeField] private TextMeshProUGUI[] upgradeNameTexts;
    [SerializeField] private UpgradeData[] availableUpgrades;

    [Header("Goal Display")]
    [SerializeField] private TextMeshProUGUI nextGoalText;

    [Header("Progress Indicators")]
    [SerializeField] private Image cornProgressBar;
    [SerializeField] private Image eggProgressBar;

    [Header("Animation Settings")]
    [SerializeField] private float punchScale = 1.3f;
    [SerializeField] private float punchDuration = 0.2f;
    [SerializeField] private float numberTweenDuration = 0.5f;

    // Current displayed values for smooth tweening
    private float displayedCorn = 0;
    private float displayedEggs = 0;
    private float displayedCoins = 0;

    // Target values
    private int targetCorn = 0;
    private int targetEggs = 0;
    private int targetCoins = 0;

    // Track purchased upgrades
    private bool[] upgradesPurchased;

    public RectTransform HireHelperButtonTransform => hireHelperButton != null ? hireHelperButton.GetComponent<RectTransform>() : null;
    public bool AreAllUpgradesPurchased
    {
        get
        {
            if (upgradesPurchased == null || upgradesPurchased.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < upgradesPurchased.Length; i++)
            {
                if (!upgradesPurchased[i])
                {
                    return false;
                }
            }

            return true;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        EnsureRuntimeBindings();
        EnsureSerializedArrays();

        Instance = this;
        
        // Ensure Canvas is set to Screen Space - Overlay for maximum WebGL compatibility
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // Initialize upgrade tracking
        upgradesPurchased = new bool[availableUpgrades != null ? availableUpgrades.Length : 0];

        // Subscribe to game events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCornChanged += OnCornChanged;
            GameManager.Instance.OnEggsChanged += OnEggsChanged;
            GameManager.Instance.OnCoinsChanged += OnCoinsChanged;
            GameManager.Instance.OnHelperCountChanged += OnHelperCountChanged;
        }

        // Setup button listeners
        SetupButtons();
        SetupExpansionButtons();

        // Initialize displays
        UpdateAllDisplays();
        UpdateNextGoal();
        UpdateIncomeRate();
    }

    private void EnsureSerializedArrays()
    {
        if (upgradeButtons == null)
        {
            upgradeButtons = new Button[0];
        }

        if (upgradeCostTexts == null)
        {
            upgradeCostTexts = new TextMeshProUGUI[0];
        }

        if (upgradeNameTexts == null)
        {
            upgradeNameTexts = new TextMeshProUGUI[0];
        }

        if (availableUpgrades == null)
        {
            availableUpgrades = new UpgradeData[0];
        }
    }

    private void EnsureRuntimeBindings()
    {
        if (!(transform is RectTransform))
        {
            gameObject.AddComponent<RectTransform>();
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Ensure it's on top of everything

        if (GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        EnsureEventSystem();

        RectTransform canvasRect = transform as RectTransform;
        RectTransform resourcePanel = EnsurePanel("ResourcePanel", canvasRect, new Vector2(20f, -20f), new Vector2(380f, 175f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        RectTransform buttonPanel = EnsurePanel("ButtonPanel", canvasRect, new Vector2(0f, 85f), new Vector2(940f, 120f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        RectTransform upgradesRect = EnsurePanel("UpgradePanel", canvasRect, new Vector2(-20f, 0f), new Vector2(320f, 420f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));
        RectTransform expansionPanel = EnsurePanel("ExpansionPanel", canvasRect, new Vector2(0f, 240f), new Vector2(420f, 100f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));

        cornCountText = EnsureText(resourcePanel, "CornCountText", "Corn: 0", new Vector2(12f, -12f), new Vector2(160f, 36f), new Vector2(0f, 1f), new Vector2(0f, 1f), 26f);
        cornProgressBar = EnsureProgressBar(resourcePanel, "CornProgressBar", new Vector2(12f, -44f), new Vector2(150f, 6f));
        
        eggsCountText = EnsureText(resourcePanel, "EggsCountText", "Eggs: 0", new Vector2(12f, -52f), new Vector2(160f, 36f), new Vector2(0f, 1f), new Vector2(0f, 1f), 26f);
        eggProgressBar = EnsureProgressBar(resourcePanel, "EggProgressBar", new Vector2(12f, -84f), new Vector2(150f, 6f));
        
        coinsCountText = EnsureText(resourcePanel, "CoinsCountText", "Coins: 50", new Vector2(12f, -92f), new Vector2(180f, 36f), new Vector2(0f, 1f), new Vector2(0f, 1f), 26f);
        timeText = EnsureText(resourcePanel, "TimeText", "00:00", new Vector2(190f, -92f), new Vector2(170f, 36f), new Vector2(0f, 1f), new Vector2(0f, 1f), 24f); 
        helperCountText = EnsureText(resourcePanel, "HelperCountText", "Helpers: 0", new Vector2(190f, -12f), new Vector2(170f, 36f), new Vector2(0f, 1f), new Vector2(0f, 1f), 24f);
        incomeRateText = EnsureText(resourcePanel, "IncomeRateText", "Manual play", new Vector2(190f, -52f), new Vector2(170f, 36f), new Vector2(0f, 1f), new Vector2(0f, 1f), 24f);
        nextGoalText = EnsureText(resourcePanel, "NextGoalText", "Save coins to hire helper!", new Vector2(12f, -132f), new Vector2(350f, 32f), new Vector2(0f, 1f), new Vector2(0f, 1f), 20f);

        cornIcon = cornCountText != null ? cornCountText.rectTransform : cornIcon;
        eggsIcon = eggsCountText != null ? eggsCountText.rectTransform : eggsIcon;
        coinsIcon = coinsCountText != null ? coinsCountText.rectTransform : coinsIcon;

        harvestButton = EnsureButton(buttonPanel, "HarvestButton", "Harvest", new Vector2(-360f, 0f), new Vector2(150f, 56f));
        feedButton = EnsureButton(buttonPanel, "FeedButton", "Feed", new Vector2(-180f, 0f), new Vector2(150f, 56f));
        collectButton = EnsureButton(buttonPanel, "CollectButton", "Collect", new Vector2(0f, 0f), new Vector2(150f, 56f));
        sellButton = EnsureButton(buttonPanel, "SellButton", "Sell", new Vector2(180f, 0f), new Vector2(150f, 56f));
        hireHelperButton = EnsureButton(buttonPanel, "HireHelperButton", "Hire Helper", new Vector2(360f, 0f), new Vector2(170f, 56f));
        
        incubateButton = EnsureButton(expansionPanel, "IncubateButton", "Incubate Egg", new Vector2(-105f, 0f), new Vector2(180f, 64f));
        plantButton = EnsureButton(expansionPanel, "PlantButton", "Plant Corn", new Vector2(105f, 0f), new Vector2(180f, 64f));

        upgradePanel = upgradesRect != null ? upgradesRect.gameObject : upgradePanel;
        EnsureSerializedArrays();

        if (upgradeButtons == null || upgradeButtons.Length != StoryUpgradeCount)
        {
            upgradeButtons = new Button[StoryUpgradeCount];
        }

        if (upgradeCostTexts == null || upgradeCostTexts.Length != StoryUpgradeCount)
        {
            upgradeCostTexts = new TextMeshProUGUI[StoryUpgradeCount];
        }

        if (upgradeNameTexts == null || upgradeNameTexts.Length != StoryUpgradeCount)
        {
            upgradeNameTexts = new TextMeshProUGUI[StoryUpgradeCount];
        }

        for (int i = 0; i < StoryUpgradeCount; i++)
        {
            float y = -24f - (i * 72f);
            Button upgradeButton = EnsureButton(upgradesRect, $"UpgradeButton_{i + 1}", StoryUpgradeNames[i], new Vector2(0f, y), new Vector2(270f, 60f));
            TextMeshProUGUI[] texts = upgradeButton.GetComponentsInChildren<TextMeshProUGUI>(true);

            if (texts.Length == 0)
            {
                texts = new[] { EnsureButtonText(upgradeButton.transform, "Label", StoryUpgradeNames[i]) };
            }

            TextMeshProUGUI nameText = texts[0];
            RectTransform nameRect = nameText.rectTransform;
            nameRect.anchorMin = new Vector2(0f, 0.5f);
            nameRect.anchorMax = new Vector2(1f, 0.5f);
            nameRect.pivot = new Vector2(0.5f, 0.5f);
            nameRect.anchoredPosition = new Vector2(-20f, 10f);
            nameRect.sizeDelta = new Vector2(-12f, 24f);
            nameText.fontSize = 20f;

            TextMeshProUGUI costText = EnsureButtonSubtext(upgradeButton.transform, "CostText", $"💰{StoryUpgradeCosts[i]}", new Vector2(0f, -12f));

            upgradeButtons[i] = upgradeButton;
            upgradeNameTexts[i] = nameText;
            upgradeCostTexts[i] = costText;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCornChanged -= OnCornChanged;
            GameManager.Instance.OnEggsChanged -= OnEggsChanged;
            GameManager.Instance.OnCoinsChanged -= OnCoinsChanged;
            GameManager.Instance.OnHelperCountChanged -= OnHelperCountChanged;
        }
    }

    private void Update()
    {
        // Smooth number tweening
        UpdateNumberTweens();

        // Update button states
        UpdateButtonStates();

        // Update progress bars
        UpdateProgressBars();

        // Update time
        UpdateTimeDisplay();
    }

    private void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            DayNightCycle cycle = FindObjectOfType<DayNightCycle>();
            if (cycle != null)
            {
                timeText.text = cycle.GetTimeString();
            }
        }
    }

    private void UpdateProgressBars()
    {
        // Update Corn Progress
        if (cornProgressBar != null)
        {
            HarvestableField field = FindObjectOfType<HarvestableField>();
            if (field != null)
            {
                float progress = field.GetGrowthProgress();
                cornProgressBar.fillAmount = progress;
                // Fade out when full (1.0)
                cornProgressBar.transform.parent.gameObject.SetActive(progress < 1.0f);
            }
        }

        // Update Egg Progress
        if (eggProgressBar != null)
        {
            Chicken chicken = FindObjectOfType<Chicken>();
            if (chicken != null)
            {
                float progress = chicken.GetProductionProgress();
                eggProgressBar.fillAmount = progress;
                // Only show while laying egg
                eggProgressBar.transform.parent.gameObject.SetActive(progress > 0.01f);
            }
        }
    }

    /// <summary>
    /// Setup button click listeners
    /// </summary>
    private void SetupButtons()
    {
        EnsureSerializedArrays();

        if (harvestButton != null)
        {
            harvestButton.onClick.RemoveAllListeners();
            harvestButton.onClick.AddListener(OnHarvestClicked);
        }

        if (feedButton != null)
        {
            feedButton.onClick.RemoveAllListeners();
            feedButton.onClick.AddListener(OnFeedClicked);
        }

        if (collectButton != null)
        {
            collectButton.onClick.RemoveAllListeners();
            collectButton.onClick.AddListener(OnCollectClicked);
        }

        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(OnSellClicked);
        }

        if (hireHelperButton != null)
        {
            hireHelperButton.onClick.RemoveAllListeners();
            hireHelperButton.onClick.AddListener(OnHireHelperClicked);
        }

        // Setup upgrade buttons
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int index = i;
            if (upgradeButtons[i] != null)
            {
                upgradeButtons[i].onClick.RemoveAllListeners();
                upgradeButtons[i].onClick.AddListener(() => OnUpgradeClicked(index));
            }
        }
    }

    /// <summary>
    /// Update smooth number displays
    /// </summary>
    private void UpdateNumberTweens()
    {
        // Tween corn display
        if (Mathf.Abs(displayedCorn - targetCorn) > 0.1f)
        {
            displayedCorn = Mathf.Lerp(displayedCorn, targetCorn, Time.deltaTime / numberTweenDuration * 5f);
            if (cornCountText != null)
            {
                cornCountText.text = Mathf.RoundToInt(displayedCorn).ToString();
            }
        }
        else if (displayedCorn != targetCorn)
        {
            displayedCorn = targetCorn;
            if (cornCountText != null)
            {
                cornCountText.text = targetCorn.ToString();
            }
        }

        // Tween eggs display
        if (Mathf.Abs(displayedEggs - targetEggs) > 0.1f)
        {
            displayedEggs = Mathf.Lerp(displayedEggs, targetEggs, Time.deltaTime / numberTweenDuration * 5f);
            if (eggsCountText != null)
            {
                eggsCountText.text = Mathf.RoundToInt(displayedEggs).ToString();
            }
        }
        else if (displayedEggs != targetEggs)
        {
            displayedEggs = targetEggs;
            if (eggsCountText != null)
            {
                eggsCountText.text = targetEggs.ToString();
            }
        }

        // Tween coins display
        if (Mathf.Abs(displayedCoins - targetCoins) > 0.1f)
        {
            displayedCoins = Mathf.Lerp(displayedCoins, targetCoins, Time.deltaTime / numberTweenDuration * 5f);
            if (coinsCountText != null)
            {
                coinsCountText.text = Mathf.RoundToInt(displayedCoins).ToString();
            }
        }
        else if (displayedCoins != targetCoins)
        {
            displayedCoins = targetCoins;
            if (coinsCountText != null)
            {
                coinsCountText.text = targetCoins.ToString();
            }
        }
    }

    /// <summary>
    /// Update button interactability and visual states
    /// </summary>
    private void UpdateButtonStates()
    {
        if (GameManager.Instance == null) return;

        // Feed button - needs corn
        if (harvestButton != null)
        {
            HarvestableField field = FindObjectOfType<HarvestableField>();
            bool canHarvest = field == null || field.CanInteract();
            harvestButton.interactable = canHarvest;
            UpdateButtonVisual(harvestButton, canHarvest);
        }

        if (feedButton != null)
        {
            bool canFeed = GameManager.Instance.Corn > 0;
            feedButton.interactable = canFeed;
            UpdateButtonVisual(feedButton, canFeed);
        }

        if (collectButton != null)
        {
            bool canCollect = FindObjectsOfType<CollectibleEgg>().Length > 0;
            collectButton.interactable = canCollect;
            UpdateButtonVisual(collectButton, canCollect);
        }

        // Sell button - needs eggs
        if (sellButton != null)
        {
            bool canSell = GameManager.Instance.Eggs > 0;
            sellButton.interactable = canSell;
            UpdateButtonVisual(sellButton, canSell);
        }

        // Hire helper button - needs coins
        if (hireHelperButton != null)
        {
            bool canHire = GameManager.Instance.CanAfford(GameManager.Instance.HelperCost);
            hireHelperButton.interactable = canHire;
            UpdateButtonVisual(hireHelperButton, canHire);

            // Update helper cost text
            TextMeshProUGUI costText = hireHelperButton.GetComponentInChildren<TextMeshProUGUI>();
            if (costText != null)
            {
                costText.text = $"Hire Helper\n{GameManager.Instance.HelperCost} coins";
            }
        }

        // Update upgrade buttons
        UpdateUpgradeButtons();
    }

    /// <summary>
    /// Update button visual (brighten when available, darken when unaffordable)
    /// </summary>
    private void UpdateButtonVisual(Button button, bool available)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            Color targetColor = available ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// Update upgrade button visuals using UpgradeData or fallback to hardcoded values
    /// </summary>
    private void UpdateUpgradeButtons()
    {
        EnsureSerializedArrays();

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (upgradeButtons[i] == null) continue;

            // Skip if already purchased
            if (upgradesPurchased != null && i < upgradesPurchased.Length && upgradesPurchased[i])
            {
                upgradeButtons[i].interactable = false;
                UpdateButtonVisual(upgradeButtons[i], false);
                continue;
            }

            // Get cost from UpgradeData if available, otherwise use fallback
            int cost;
            string upgradeName;
            
            if (availableUpgrades != null && i < availableUpgrades.Length && availableUpgrades[i] != null)
            {
                cost = availableUpgrades[i].GetCost();
                upgradeName = availableUpgrades[i].upgradeName;
            }
            else
            {
                cost = i < StoryUpgradeCosts.Length ? StoryUpgradeCosts[i] : 100;
                upgradeName = i < StoryUpgradeNames.Length ? StoryUpgradeNames[i] : "Upgrade";
            }

            bool canAfford = GameManager.Instance.CanAfford(cost);
            upgradeButtons[i].interactable = canAfford;
            UpdateButtonVisual(upgradeButtons[i], canAfford);

            // Update cost text
            if (upgradeCostTexts != null && i < upgradeCostTexts.Length && upgradeCostTexts[i] != null)
            {
                upgradeCostTexts[i].text = $"💰{cost}";
                upgradeCostTexts[i].color = canAfford ? Color.white : Color.red;
            }

            // Update name text if available
            if (upgradeNameTexts != null && i < upgradeNameTexts.Length && upgradeNameTexts[i] != null)
            {
                upgradeNameTexts[i].text = upgradeName;
            }
        }
    }

    /// <summary>
    /// Update all displays immediately
    /// </summary>
    private void UpdateAllDisplays()
    {
        if (GameManager.Instance != null)
        {
            targetCorn = GameManager.Instance.Corn;
            targetEggs = GameManager.Instance.Eggs;
            targetCoins = GameManager.Instance.Coins;

            displayedCorn = targetCorn;
            displayedEggs = targetEggs;
            displayedCoins = targetCoins;

            if (cornCountText != null) cornCountText.text = targetCorn.ToString();
            if (eggsCountText != null) eggsCountText.text = targetEggs.ToString();
            if (coinsCountText != null) coinsCountText.text = targetCoins.ToString();
            if (helperCountText != null) helperCountText.text = GameManager.Instance.HelperCount.ToString();
            
            UpdateLevelDisplay();
            UpdateExpansionButtons();
        }
    }

    private void UpdateLevelDisplay()
    {
        if (levelText != null && GameManager.Instance != null)
        {
            levelText.text = $"Stage {GameManager.Instance.CurrentLevel}/3";
        }
    }

    private void UpdateExpansionButtons()
    {
        if (GameManager.Instance == null) return;
        
        bool canIncubate = GameManager.Instance.ChickenPositions.Count < 6 && GameManager.Instance.Eggs >= 10;
        bool canPlant = GameManager.Instance.CornFieldPositions.Count < 6 && GameManager.Instance.Corn >= 10;

        if (incubateButton != null) incubateButton.interactable = canIncubate;
        if (plantButton != null) plantButton.interactable = canPlant;
    }

    private void SetupExpansionButtons()
    {
        if (incubateButton != null)
        {
            incubateButton.onClick.RemoveAllListeners();
            incubateButton.onClick.AddListener(() => {
                 GameManager.Instance.AddChicken();
                 UpdateExpansionButtons();
            });
        }
        
        if (plantButton != null)
        {
            plantButton.onClick.RemoveAllListeners();
            plantButton.onClick.AddListener(() => {
                GameManager.Instance.AddCornField();
                UpdateExpansionButtons();
            });
        }
    }

    // Event handlers
    private void OnCornChanged(int newValue)
    {
        bool increased = newValue > targetCorn;
        targetCorn = newValue;

        if (increased && cornIcon != null)
        {
            PunchScale(cornIcon);
        }

        UpdateExpansionButtons();
    }

    private void OnEggsChanged(int newValue)
    {
        bool increased = newValue > targetEggs;
        targetEggs = newValue;

        if (increased && eggsIcon != null)
        {
            PunchScale(eggsIcon);
        }

        UpdateExpansionButtons();
    }

    private void OnCoinsChanged(int newValue)
    {
        bool increased = newValue > targetCoins;
        targetCoins = newValue;

        if (increased && coinsIcon != null)
        {
            PunchScale(coinsIcon);
        }

        UpdateNextGoal();
        UpdateIncomeRate();
    }

    private void OnHelperCountChanged(int newValue)
    {
        if (helperCountText != null)
        {
            helperCountText.text = newValue.ToString();
        }

        UpdateNextGoal();
        UpdateIncomeRate();
    }

    // --- Button click handlers ---
    // All actions route through PlayerController so the player walks to the target first.

    private PlayerController GetPlayer()
    {
        return FindObjectOfType<PlayerController>();
    }

    private void OnHarvestClicked()
    {
        try { AudioManager.Instance?.PlaySound("click"); } catch {}
        HarvestableField field = FindObjectOfType<HarvestableField>();
        if (field != null)
        {
            PlayerController player = GetPlayer();
            if (player != null)
                player.MoveToAndInteract(field.transform.position, field);
            else
                field.Harvest();
        }
        else
        {
            GameManager.Instance?.AddCorn(1);
        }
    }

    private void OnFeedClicked()
    {
        try { AudioManager.Instance?.PlaySound("click"); } catch {}
        Chicken chicken = FindObjectOfType<Chicken>();
        if (chicken != null)
        {
            if (chicken.CanInteract())
            {
                chicken.Feed();
            }
            else
            {
                PlayerController player = GetPlayer();
                if (player != null)
                    player.MoveTo(chicken.transform.position);
            }
        }
    }

    private void OnCollectClicked()
    {
        try { AudioManager.Instance?.PlaySound("click"); } catch {}
        CollectibleEgg[] eggs = FindObjectsOfType<CollectibleEgg>();
        foreach (var egg in eggs)
        {
            PlayerController player = GetPlayer();
            if (player != null)
                player.MoveToAndInteract(egg.transform.position, egg);
            else
                egg.Interact();
            break;
        }
    }

    private void OnSellClicked()
    {
        try { AudioManager.Instance?.PlaySound("click"); } catch {}
        StoreCounter store = FindObjectOfType<StoreCounter>();
        if (store != null)
        {
            PlayerController player = GetPlayer();
            if (player != null)
                player.MoveToAndInteract(store.transform.position, store);
            else if (store.CanInteract())
                store.SellEgg();
        }
        else
        {
            GameManager.Instance?.SellEgg();
        }
    }

    private void OnHireHelperClicked()
    {
        AudioManager.Instance?.PlaySound("click");
        int cost = GameManager.Instance.HelperCost;
        if (GameManager.Instance.CanAfford(cost))
        {
            if (GameManager.Instance.HireHelper())
            {
                // Play hire animation
                if (hireHelperButton != null)
                {
                    PunchScale(hireHelperButton.GetComponent<RectTransform>());
                }
                UpdateNextGoal();
                UpdateIncomeRate();
            }
        }
        else
        {
            // Show error feedback
            int shortfall = cost - GameManager.Instance.Coins;
            ShowCannotAfford(shortfall);
        }
    }

    private void OnUpgradeClicked(int upgradeIndex)
    {
        AudioManager.Instance?.PlaySound("click");
        // Use UpgradeData if available
        if (availableUpgrades != null && upgradeIndex < availableUpgrades.Length && availableUpgrades[upgradeIndex] != null)
        {
            UpgradeData upgrade = availableUpgrades[upgradeIndex];
            if (upgrade.Purchase())
            {
                // Mark as purchased
                if (upgradesPurchased != null && upgradeIndex < upgradesPurchased.Length)
                {
                    upgradesPurchased[upgradeIndex] = true;
                }

                // Visual feedback
                if (upgradeIndex < upgradeButtons.Length && upgradeButtons[upgradeIndex] != null)
                {
                    upgradeButtons[upgradeIndex].interactable = false;
                    PunchScale(upgradeButtons[upgradeIndex].GetComponent<RectTransform>());
                }

                ShowUpgradeNotification($"{upgrade.upgradeName} upgraded!");
                UpdateNextGoal();
                UpdateIncomeRate();
                TitleCardManager titleCardManager = FindObjectOfType<TitleCardManager>();
                if (titleCardManager != null)
                {
                    titleCardManager.EvaluateStoryProgress();
                }
            }
            else
            {
                int shortfall = upgrade.GetCost() - GameManager.Instance.Coins;
                ShowCannotAfford(shortfall);
            }
            return;
        }

        // Fallback to hardcoded upgrades
        UpgradeType[] upgradeTypes = {
            UpgradeType.CornField,
            UpgradeType.ChickenProduction,
            UpgradeType.EggPrice,
            UpgradeType.Speed,
            UpgradeType.StoreCapacity
        };

        if (upgradeIndex < StoryUpgradeCosts.Length)
        {
            if (GameManager.Instance.SpendCoins(StoryUpgradeCosts[upgradeIndex]))
            {
                GameManager.Instance.ApplyUpgrade(upgradeTypes[upgradeIndex], 1.2f);

                // Mark as purchased
                if (upgradesPurchased != null && upgradeIndex < upgradesPurchased.Length)
                {
                    upgradesPurchased[upgradeIndex] = true;
                }

                // Disable the button after purchase
                if (upgradeIndex < upgradeButtons.Length && upgradeButtons[upgradeIndex] != null)
                {
                    upgradeButtons[upgradeIndex].interactable = false;
                    PunchScale(upgradeButtons[upgradeIndex].GetComponent<RectTransform>());
                }

                ShowUpgradeNotification($"Upgrade purchased!");
                UpdateNextGoal();
                UpdateIncomeRate();
                TitleCardManager titleCardManager = FindObjectOfType<TitleCardManager>();
                if (titleCardManager != null)
                {
                    titleCardManager.EvaluateStoryProgress();
                }
            }
            else
            {
                int shortfall = StoryUpgradeCosts[upgradeIndex] - GameManager.Instance.Coins;
                ShowCannotAfford(shortfall);
            }
        }
    }

    /// <summary>
    /// Show cannot afford error feedback
    /// </summary>
    private void ShowCannotAfford(int coinsNeeded)
    {
        AudioManager.Instance?.PlaySound("error");
        ShowNotification($"Need {coinsNeeded} more coins!", Color.red);
        
        // Shake the coins icon
        if (coinsIcon != null)
        {
            StartCoroutine(ShakeCoroutine(coinsIcon));
        }
    }

    /// <summary>
    /// Update the next goal display
    /// </summary>
    private void UpdateNextGoal()
    {
        if (nextGoalText == null || GameManager.Instance == null) return;

        int coins = GameManager.Instance.Coins;
        int helperCost = GameManager.Instance.HelperCost;

        if (coins < helperCost)
        {
            int needed = helperCost - coins;
            nextGoalText.text = $"🎯 Save {needed} more coins to hire a helper!";
        }
        else
        {
            // Check for affordable upgrades
            bool hasAffordableUpgrade = false;
            string upgradeGoal = "";

            if (availableUpgrades != null)
            {
                for (int i = 0; i < availableUpgrades.Length; i++)
                {
                    if (availableUpgrades[i] != null && 
                        (upgradesPurchased == null || i >= upgradesPurchased.Length || !upgradesPurchased[i]))
                    {
                        if (!availableUpgrades[i].CanPurchase())
                        {
                            int needed = availableUpgrades[i].GetCost() - coins;
                            upgradeGoal = $"🎯 {needed} more for {availableUpgrades[i].upgradeName}!";
                            break;
                        }
                        else
                        {
                            hasAffordableUpgrade = true;
                        }
                    }
                }
            }

            if (hasAffordableUpgrade)
            {
                nextGoalText.text = "✨ You can afford an upgrade!";
            }
            else if (!string.IsNullOrEmpty(upgradeGoal))
            {
                nextGoalText.text = upgradeGoal;
            }
            else
            {
                nextGoalText.text = "👷 Hire more helpers to grow faster!";
            }
        }
    }

    /// <summary>
    /// Update income rate display
    /// </summary>
    private void UpdateIncomeRate()
    {
        if (incomeRateText == null || GameManager.Instance == null) return;

        int helpers = GameManager.Instance.HelperCount;
        if (helpers <= 0)
        {
            incomeRateText.text = "Manual play";
            return;
        }

        // Calculate approximate income
        // Each helper completes a loop in ~7-8 seconds, selling an egg for EggSellPrice
        float loopTime = 7.2f / (GameManager.Instance.SpeedMultiplier * GameManager.Instance.StoreEfficiencyMultiplier);
        float incomePerSecond = helpers * GameManager.Instance.EggSellPrice / loopTime;

        incomeRateText.text = $"+{incomePerSecond:F1} 💰/sec";
    }

    /// <summary>
    /// Shake coroutine for error feedback
    /// </summary>
    private IEnumerator ShakeCoroutine(RectTransform target)
    {
        Vector2 originalPos = target.anchoredPosition;
        float duration = 0.3f;
        float intensity = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float remaining = 1f - (elapsed / duration);
            float x = Random.Range(-intensity, intensity) * remaining;
            target.anchoredPosition = originalPos + new Vector2(x, 0);
            yield return null;
        }

        target.anchoredPosition = originalPos;
    }

    /// <summary>
    /// Punch scale animation for UI elements
    /// </summary>
    public void PunchScale(RectTransform target)
    {
        if (target != null)
        {
            StartCoroutine(PunchScaleCoroutine(target));
        }
    }

    private IEnumerator PunchScaleCoroutine(RectTransform target)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 punchScaleVec = originalScale * punchScale;

        // Punch up
        float t = 0;
        while (t < punchDuration * 0.3f)
        {
            t += Time.deltaTime;
            target.localScale = Vector3.Lerp(originalScale, punchScaleVec, t / (punchDuration * 0.3f));
            yield return null;
        }

        // Return with bounce
        t = 0;
        while (t < punchDuration * 0.7f)
        {
            t += Time.deltaTime;
            float bounce = 1f + Mathf.Sin(t / (punchDuration * 0.7f) * Mathf.PI * 2f) * 0.1f;
            target.localScale = Vector3.Lerp(punchScaleVec, originalScale, t / (punchDuration * 0.7f)) * bounce;
            yield return null;
        }

        target.localScale = originalScale;
    }

    /// <summary>
    /// Show floating notification text
    /// </summary>
    public void ShowUpgradeNotification(string message)
    {
        StartCoroutine(ShowNotificationCoroutine(message, new Color(1f, 0.9f, 0.3f)));
    }

    /// <summary>
    /// Show a notification with custom color
    /// </summary>
    public void ShowNotification(string message, Color color)
    {
        StartCoroutine(ShowNotificationCoroutine(message, color));
    }

    private IEnumerator ShowNotificationCoroutine(string message, Color color)
    {
        // Create notification text
        GameObject notificationObj = new GameObject("Notification");
        notificationObj.transform.SetParent(transform, false);

        TextMeshProUGUI text = notificationObj.AddComponent<TextMeshProUGUI>();
        text.text = message;
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;

        RectTransform rt = notificationObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(300, 50);

        // Animate
        float duration = 1.5f;
        float t = 0;
        Vector2 startPos = Vector2.zero;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            // Move up and fade out
            rt.anchoredPosition = startPos + new Vector2(0, progress * 100f);
            text.alpha = 1f - progress;

            yield return null;
        }

        Destroy(notificationObj);
    }

    /// <summary>
    /// Show upgrade panel with bounce animation
    /// </summary>
    public void ShowUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
            StartCoroutine(BounceIn(upgradePanel.GetComponent<RectTransform>()));
        }
    }

    public void HideUpgradePanel()
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }
    }

    private IEnumerator BounceIn(RectTransform target)
    {
        if (target == null) yield break;

        Vector3 originalScale = Vector3.one;
        target.localScale = Vector3.zero;

        float t = 0;
        float duration = 0.3f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            // Overshoot then settle
            float bounce = Mathf.Sin(progress * Mathf.PI) * 0.2f;
            target.localScale = originalScale * (progress + bounce);

            yield return null;
        }

        target.localScale = originalScale;
    }

    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        if (eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
    }

    private RectTransform EnsurePanel(string name, RectTransform parent, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
    {
        Transform existing = parent.Find(name);
        GameObject panelObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
        if (panelObject.transform.parent != parent)
        {
            panelObject.transform.SetParent(parent, false);
        }

        Image image = panelObject.GetComponent<Image>();
        if (image == null)
        {
            image = panelObject.AddComponent<Image>();
        }

        image.color = new Color(1f, 0.98f, 0.77f, 0.82f);

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMax;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
        return rect;
    }

    private TextMeshProUGUI EnsureText(RectTransform parent, string name, string content, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, float fontSize)
    {
        Transform existing = parent.Find(name);
        GameObject textObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        if (textObject.transform.parent != parent)
        {
            textObject.transform.SetParent(parent, false);
        }

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMax;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = textObject.AddComponent<TextMeshProUGUI>();
        }

        text.text = content;
        text.fontSize = fontSize;
        text.color = StoryColorPalette.TextDark;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        return text;
    }

    private Button EnsureButton(RectTransform parent, string name, string label, Vector2 anchoredPosition, Vector2 size)
    {
        Transform existing = parent.Find(name);
        GameObject buttonObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        if (buttonObject.transform.parent != parent)
        {
            buttonObject.transform.SetParent(parent, false);
        }

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = StoryColorPalette.ButtonGreen;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;

        // Implementation of premium highlighting
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = StoryColorPalette.AccentGold;
        colors.pressedColor = Color.Lerp(image.color, Color.black, 0.2f);
        colors.selectedColor = StoryColorPalette.AccentGold;
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        TextMeshProUGUI text = EnsureButtonText(buttonObject.transform, "Label", label);
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 22f;

        return button;
    }

    private TextMeshProUGUI EnsureButtonText(Transform parent, string name, string label)
    {
        Transform existing = parent.Find(name);
        GameObject textObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        if (textObject.transform.parent != parent)
        {
            textObject.transform.SetParent(parent, false);
        }

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(10f, 10f);
        rect.offsetMax = new Vector2(-10f, -10f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = textObject.AddComponent<TextMeshProUGUI>();
        }

        text.text = label;
        text.color = StoryColorPalette.TextDark;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.alignment = TextAlignmentOptions.Center;
        return text;
    }

    private TextMeshProUGUI EnsureButtonSubtext(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        Transform existing = parent.Find(name);
        GameObject textObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        if (textObject.transform.parent != parent)
        {
            textObject.transform.SetParent(parent, false);
        }

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(220f, 18f);
        rect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = textObject.AddComponent<TextMeshProUGUI>();
        }

        text.text = label;
        text.fontSize = 16f;
        text.color = StoryColorPalette.TextDark;
        text.alignment = TextAlignmentOptions.Center;
        return text;
    }

    private Image EnsureProgressBar(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
    {
        // Background
        Transform existingBg = parent.Find(name + "_Bg");
        GameObject bgObject = existingBg != null ? existingBg.gameObject : new GameObject(name + "_Bg", typeof(RectTransform));
        if (bgObject.transform.parent != parent) bgObject.transform.SetParent(parent, false);

        RectTransform bgRect = bgObject.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 1f);
        bgRect.anchorMax = new Vector2(0f, 1f);
        bgRect.pivot = new Vector2(0f, 1f);
        bgRect.anchoredPosition = anchoredPosition;
        bgRect.sizeDelta = size;

        Image bgImage = bgObject.GetComponent<Image>();
        if (bgImage == null) bgImage = bgObject.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.4f);

        // Fill
        Transform existingFill = bgObject.transform.Find("Fill");
        GameObject fillObject = existingFill != null ? existingFill.gameObject : new GameObject("Fill", typeof(RectTransform));
        if (fillObject.transform.parent != bgObject.transform) fillObject.transform.SetParent(bgObject.transform, false);

        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fillObject.GetComponent<Image>();
        if (fillImage == null) fillImage = fillObject.AddComponent<Image>();
        fillImage.color = StoryColorPalette.CoinGold;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 0.5f;

        return fillImage;
    }
}
