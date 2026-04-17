using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using ChickenCoop.Managers;

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

    [Header("Management Toggle")]
    [SerializeField] private Button managementToggleButton;
    [SerializeField] private GameObject managementDrawer;
    private bool isManagementOpen = false;

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
        
        // Ensure only one EventSystem
        CleanupDuplicateEventSystems();

        // Initialize Background Systems
        if (EnvironmentManager.Instance == null)
        {
            GameObject envObj = new GameObject("EnvironmentManager");
            envObj.AddComponent<EnvironmentManager>();
        }

        // Initialize displays
        UpdateAllDisplays();
        UpdateNextGoal();
        UpdateIncomeRate();
    }

    private float lastAspectRatio = 0f;
    private void UpdateAdaptiveLayout()
    {
        float ratio = (float)Screen.width / Screen.height;
        if (Mathf.Abs(ratio - lastAspectRatio) < 0.01f)
        {
            // Even if ratio is same, handle drawer state animations here if needed
            return;
        }
        lastAspectRatio = ratio;
        
        EnsureRuntimeBindings(); // Re-apply anchors/pivots based on ratio
    }

    public void ToggleManagement()
    {
        isManagementOpen = !isManagementOpen;
        if (managementDrawer != null)
        {
            RectTransform rect = managementDrawer.GetComponent<RectTransform>();
            if (isManagementOpen)
            {
                SetPanelActiveWithShadow(managementDrawer, true);

                VisualFeedbackManager drawerVfx = VisualFeedbackManager.Instance ?? FindObjectOfType<VisualFeedbackManager>();
                drawerVfx?.SlideIn(rect, new Vector2(0, -1000f), 0.4f);
            }
            else
            {
                SetPanelActiveWithShadow(managementDrawer, false);
            }
        }
        
        if (managementToggleButton != null)
        {
            TextMeshProUGUI label = managementToggleButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = isManagementOpen ? "Close Shop" : "Shop / Build";
        }
        
        VisualFeedbackManager globalVfx = VisualFeedbackManager.Instance ?? FindObjectOfType<VisualFeedbackManager>();
        globalVfx?.ShakeCamera(0.1f, 0.05f);
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

        float aspectRatio = (float)Screen.width / Screen.height;
        bool isPortrait = aspectRatio < 1.1f;

        if (GetComponent<CanvasScaler>() == null)
        {
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = isPortrait ? new Vector2(1080f, 1920f) : new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = isPortrait ? 0f : 0.5f; // Match width in portrait to ensure content fits
        }
        else
        {
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            scaler.referenceResolution = isPortrait ? new Vector2(1080f, 1920f) : new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = isPortrait ? 0f : 0.5f;
        }

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        EnsureEventSystem();

        RectTransform canvasRect = transform as RectTransform;
        
        // ----- HUD: DYNAMIC ICON INVENTORY BAR -----
        Vector2 resourcePos = isPortrait ? new Vector2(0f, -10f) : new Vector2(0f, -10f);
        Vector2 resourceAnchor = new Vector2(0.5f, 1f);
        RectTransform hudPanel = EnsurePanelWithShadow("HUDPanel", canvasRect, resourcePos, new Vector2(isPortrait ? 900f : 800f, 80f), resourceAnchor, resourceAnchor);
        // Style: warm parchment
        hudPanel.GetComponent<Image>().color = new Color(0.95f, 0.87f, 0.60f, 0.93f);

        // Static icons for the three primary resources (corn/eggs/coins)
        // plus dynamic slots generated by RefreshInventoryHUD()
        cornCountText = EnsureIconCounterSlot(hudPanel, "CornCounter", "🌽", "0", new Vector2(isPortrait ? -300f : -260f, 0f));
        eggsCountText = EnsureIconCounterSlot(hudPanel, "EggCounter", "🥚", "0", new Vector2(isPortrait ? -100f : -87f, 0f));
        coinsCountText = EnsureIconCounterSlot(hudPanel, "CoinCounter", "🪙", "0", new Vector2(isPortrait ? 100f : 87f, 0f));
        helperCountText = EnsureIconCounterSlot(hudPanel, "HelperCounter", "👷", "0", new Vector2(isPortrait ? 300f : 260f, 0f));

        // Thin grow bars below icon counters
        cornProgressBar = EnsureProgressBar(hudPanel, "CornBar", new Vector2(isPortrait ? -300f : -260f, -46f), new Vector2(130f, 5f));
        eggProgressBar = EnsureProgressBar(hudPanel, "EggBar", new Vector2(isPortrait ? -100f : -87f, -46f), new Vector2(130f, 5f));

        // --- MANAGEMENT DRAWER: CONSOLIDATED TOOLS (Hidden by default) ---
        Vector2 drawerSize = isPortrait ? new Vector2(900f, 1100f) : new Vector2(700f, 750f);
        RectTransform drawer = EnsurePanelWithShadow("ManagementDrawer", canvasRect, new Vector2(0, 0), drawerSize, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        managementDrawer = drawer.gameObject;
        if (!isManagementOpen) SetPanelActiveWithShadow(managementDrawer, false);

        // Management Content: Header
        EnsureText(drawer, "ShopHeader", "Farm Management", new Vector2(0f, -10f), new Vector2(500f, 60f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), 42f).alignment = TextAlignmentOptions.Center;
        
        // Stats in Management
        helperCountText = EnsureText(drawer, "HelperStats", "Helpers: 0", new Vector2(30f, -90f), new Vector2(300f, 36f), new Vector2(0f, 1f), new Vector2(0f, 1f), 24f); 
        incomeRateText = EnsureText(drawer, "IncomeStats", "Profit/hr: 0", new Vector2(drawerSize.x - 30f, -90f), new Vector2(300f, 36f), new Vector2(1f, 1f), new Vector2(1f, 1f), 24f);
        incomeRateText.alignment = TextAlignmentOptions.Right;

        // Sub-Panels inside Drawer
        RectTransform upgradesSub = EnsurePanel("UpgradesArea", drawer, new Vector2(0, -150f), new Vector2(drawerSize.x - 60f, 450f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        upgradesSub.GetComponent<Image>().color = new Color(0,0,0,0.1f);
        upgradePanel = upgradesSub.gameObject;

        // Ensure Vertical Layout for Upgrades
        VerticalLayoutGroup vlg = upgradesSub.GetComponent<VerticalLayoutGroup>() ?? upgradesSub.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(20, 20, 20, 20);
        vlg.spacing = 15f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;

        RectTransform expansionsSub = EnsurePanel("ExpansionsArea", drawer, new Vector2(0, -650f), new Vector2(drawerSize.x - 60f, 200f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        expansionsSub.GetComponent<Image>().color = new Color(0,0,0,0.1f);

        // Ensure Horizontal Layout for common expansion buttons (Incubate, Plant)
        // We'll use a container for just these two if needed, but let's try direct layout first
        HorizontalLayoutGroup hlg = expansionsSub.GetComponent<HorizontalLayoutGroup>() ?? expansionsSub.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(10, 10, 10, 10);
        hlg.spacing = 30f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        // Expansion Buttons
        incubateButton = EnsureButton(expansionsSub, "IncubateBtn", "Incubate", Vector2.zero, new Vector2(200f, 80f));
        plantButton = EnsureButton(expansionsSub, "PlantBtn", "Plant", Vector2.zero, new Vector2(200f, 80f));

        nextGoalText = EnsureText(drawer, "GoalText", "Next Goal...", new Vector2(0f, 50f), new Vector2(drawerSize.x - 100f, 40f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), 22f);
        nextGoalText.alignment = TextAlignmentOptions.Center;

        // --- ACTION BAR: PRIMARY CLICKING AREA (Always Bottom) ---
        Vector2 buttonPanelPos = isPortrait ? new Vector2(0f, 10f) : new Vector2(0f, 10f);
        Vector2 buttonPanelSize = isPortrait ? new Vector2(1020f, 200f) : new Vector2(1200f, 120f);
        RectTransform actionBar = EnsurePanel("ActionBar", canvasRect, buttonPanelPos, buttonPanelSize, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        actionBar.GetComponent<Image>().color = new Color(0.9f, 0.78f, 0.45f, 0.85f); // Warm harvest gold strip

        float btnSpacing = isPortrait ? 200f : 220f;
        float btnY = 0f;
        Vector2 btnSize = isPortrait ? new Vector2(175f, 140f) : new Vector2(190f, 90f);

        harvestButton  = EnsureHarvestButton(actionBar, "Btn1", "🌽\nHarvest", new Vector2(-btnSpacing * 2f, btnY), btnSize, new Color(0.56f, 0.76f, 0.36f));
        feedButton     = EnsureHarvestButton(actionBar, "Btn2", "🌾\nFeed",    new Vector2(-btnSpacing,      btnY), btnSize, new Color(0.56f, 0.76f, 0.36f));
        collectButton  = EnsureHarvestButton(actionBar, "Btn3", "🥚\nCollect", new Vector2(0,               btnY), btnSize, new Color(0.56f, 0.76f, 0.36f));
        sellButton     = EnsureHarvestButton(actionBar, "Btn4", "💰\nSell",    new Vector2(btnSpacing,       btnY), btnSize, new Color(0.89f, 0.65f, 0.22f));
        
        // MANAGEMENT TOGGLE BUTTON
        managementToggleButton = EnsureHarvestButton(actionBar, "BtnShop", "🏪\nShop", new Vector2(btnSpacing * 2f, btnY), btnSize, new Color(0.68f, 0.44f, 0.22f));
        managementToggleButton.onClick.RemoveAllListeners();
        managementToggleButton.onClick.AddListener(ToggleManagement);

        upgradePanel = upgradesSub != null ? upgradesSub.gameObject : upgradePanel;

        // Specific Hire Helper button update - Removing it from Horizontal group by not making it a direct child if we want it below, 
        // but for now let's just use it as part of the list or handle it after
        hireHelperButton = EnsureButton(expansionsSub, "HireBtn", "Hire Helper", Vector2.zero, new Vector2(250f, 60f));

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
            RectTransform upgradeArea = upgradePanel != null ? upgradePanel.GetComponent<RectTransform>() : canvasRect;
            Button upgradeButton = EnsureButton(upgradeArea, $"UpgradeButton_{i + 1}", StoryUpgradeNames[i], Vector2.zero, new Vector2(300f, 70f));
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

            TextMeshProUGUI costText = EnsureButtonSubtext(upgradeButton.transform, "CostText", $"Gold {StoryUpgradeCosts[i]}", new Vector2(0f, -12f));

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
        UpdateAdaptiveLayout();

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

    // Updates corn text to pure number (icon already in slot container label)
    private void UpdateNumberTweens()
    {
        // Tween corn display
        if (Mathf.Abs(displayedCorn - targetCorn) > 0.1f)
        {
            displayedCorn = Mathf.Lerp(displayedCorn, targetCorn, Time.deltaTime / numberTweenDuration * 5f);
            if (cornCountText != null) cornCountText.text = Mathf.RoundToInt(displayedCorn).ToString();
        }
        else if (displayedCorn != targetCorn)
        {
            displayedCorn = targetCorn;
            if (cornCountText != null) cornCountText.text = targetCorn.ToString();
        }

        // Tween eggs display
        if (Mathf.Abs(displayedEggs - targetEggs) > 0.1f)
        {
            displayedEggs = Mathf.Lerp(displayedEggs, targetEggs, Time.deltaTime / numberTweenDuration * 5f);
            if (eggsCountText != null) eggsCountText.text = Mathf.RoundToInt(displayedEggs).ToString();
        }
        else if (displayedEggs != targetEggs)
        {
            displayedEggs = targetEggs;
            if (eggsCountText != null) eggsCountText.text = targetEggs.ToString();
        }

        // Tween coins display
        if (Mathf.Abs(displayedCoins - targetCoins) > 0.1f)
        {
            displayedCoins = Mathf.Lerp(displayedCoins, targetCoins, Time.deltaTime / numberTweenDuration * 5f);
            if (coinsCountText != null) coinsCountText.text = Mathf.RoundToInt(displayedCoins).ToString();
        }
        else if (displayedCoins != targetCoins)
        {
            displayedCoins = targetCoins;
            if (coinsCountText != null) coinsCountText.text = targetCoins.ToString();
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
    /// Update upgrade button visuals using UpgradeData tree logic
    /// </summary>
    private void UpdateUpgradeButtons()
    {
        EnsureSerializedArrays();

        if (availableUpgrades == null || availableUpgrades.Length == 0) return;

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i >= availableUpgrades.Length || upgradeButtons[i] == null) continue;

            UpgradeData data = availableUpgrades[i];
            
            // Check visibility based on Act and Prerequisites
            bool isVisible = data.IsVisible();
            upgradeButtons[i].gameObject.SetActive(isVisible);

            if (!isVisible) continue;

            int cost = data.GetCost();
            bool canAfford = data.CanPurchase();
            
            upgradeButtons[i].interactable = canAfford;
            UpdateButtonVisual(upgradeButtons[i], canAfford);

            // Update cost text
            if (upgradeCostTexts != null && i < upgradeCostTexts.Length && upgradeCostTexts[i] != null)
            {
                upgradeCostTexts[i].text = $"Gold {cost}";
                upgradeCostTexts[i].color = canAfford ? Color.white : Color.red;
            }

            // Update name text
            if (upgradeNameTexts != null && i < upgradeNameTexts.Length && upgradeNameTexts[i] != null)
            {
                upgradeNameTexts[i].text = data.upgradeName;
            }
        }
    }

    /// <summary>
    /// Shows the large cinematic title card for a new Act
    /// </summary>
    public void ShowActTitle(int actIndex)
    {
        TitleCardManager tcm = FindObjectOfType<TitleCardManager>();
        if (tcm != null)
        {
            tcm.ShowTitleCard(actIndex);
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
            levelText.text = $"Act {GameManager.Instance.CurrentAct}/4";
        }
    }

    private void UpdateExpansionButtons()
    {
        if (GameManager.Instance == null) return;
        
        int chickenCount = GameManager.Instance.ChickenPositions.Count;
        int cornCount = GameManager.Instance.CornFieldPositions.Count;

        bool canIncubate = chickenCount < 6 && GameManager.Instance.Eggs >= 1;
        bool canPlant = cornCount < 6 && GameManager.Instance.Corn >= 1;

        if (incubateButton != null) 
        {
            incubateButton.interactable = canIncubate;
            TextMeshProUGUI label = incubateButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = chickenCount >= 6 ? "MAX" : "Incubate";
            UpdateButtonVisual(incubateButton, canIncubate);
        }

        if (plantButton != null) 
        {
            plantButton.interactable = canPlant;
            TextMeshProUGUI label = plantButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = cornCount >= 6 ? "MAX" : "Plant";
            UpdateButtonVisual(plantButton, canPlant);
        }
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
                // Visual feedback
                if (upgradeIndex < upgradeButtons.Length && upgradeButtons[upgradeIndex] != null)
                {
                    PunchScale(upgradeButtons[upgradeIndex].GetComponent<RectTransform>());
                    
                    // Only disable/hide if it's maxed or shouldn't be visible anymore
                    bool shouldStay = upgrade.IsVisible();
                    upgradeButtons[upgradeIndex].gameObject.SetActive(shouldStay);
                    if (shouldStay)
                    {
                        upgradeButtons[upgradeIndex].interactable = upgrade.CanPurchase();
                    }
                }

                ShowUpgradeNotification($"{upgrade.upgradeName} level {upgrade.currentLevel}!");
                UpdateUpgradeButtons(); // Refresh all buttons for prerequisites
                UpdateNextGoal();
                UpdateIncomeRate();
                
                // Refresh environment in case this upgrade affects fencing/pens
                EnvironmentManager.Instance?.RefreshFences();
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
                nextGoalText.text = "Hire more helpers to grow faster!";
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

        incomeRateText.text = $"+{incomePerSecond:F1} Gold/sec";
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
        EventSystem[] systems = FindObjectsOfType<EventSystem>();
        if (systems.Length > 1)
        {
            for (int i = 1; i < systems.Length; i++) Destroy(systems[i].gameObject);
            return;
        }

        if (systems.Length == 0)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
    }

    private void CleanupDuplicateEventSystems()
    {
        EventSystem[] systems = FindObjectsOfType<EventSystem>();
        if (systems.Length > 1)
        {
            for (int i = 1; i < systems.Length; i++) 
            {
                // Silence cleanup log to avoid spamming console
                Destroy(systems[i].gameObject);
            }
        }
    }

    private RectTransform EnsurePanelWithShadow(string name, RectTransform parent, Vector2 pos, Vector2 size, Vector2 anchor, Vector2 pivot)
    {
        // Shadow first
        string shadowName = name + "_Shadow";
        Transform existingShadow = parent.Find(shadowName);
        RectTransform shadowRect;
        if (existingShadow == null)
        {
            GameObject obj = new GameObject(shadowName);
            shadowRect = obj.AddComponent<RectTransform>();
            shadowRect.SetParent(parent, false);
            obj.AddComponent<CanvasRenderer>();
            Image img = obj.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.4f); // Semi-transparent black
        }
        else
        {
            shadowRect = existingShadow as RectTransform;
        }

        // Apply same properties to shadow but offset
        shadowRect.anchorMin = anchor;
        shadowRect.anchorMax = anchor;
        shadowRect.pivot = pivot;
        shadowRect.sizeDelta = size;
        shadowRect.anchoredPosition = pos + new Vector2(8f, -8f);

        // Main Panel
        return EnsurePanel(name, parent, pos, size, anchor, pivot);
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

    // ----------------------------------------------------------------
    // HAPPY HARVEST THEMED BUTTON - icon on top, label below
    // ----------------------------------------------------------------
    private Button EnsureHarvestButton(RectTransform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, Color baseColor)
    {
        Transform existing = parent.Find(name);
        GameObject buttonObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        if (buttonObject.transform.parent != parent) buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = baseColor;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor    = baseColor;
        colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.35f);
        colors.pressedColor   = Color.Lerp(baseColor, Color.black, 0.25f);
        colors.disabledColor  = new Color(0.55f, 0.55f, 0.55f, 0.6f);
        colors.fadeDuration   = 0.08f;
        button.colors = colors;

        // Outer rounded feel: add a thin darker border image child
        Transform borderT = buttonObject.transform.Find("Border");
        if (borderT == null)
        {
            GameObject border = new GameObject("Border", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(buttonObject.transform, false);
            border.transform.SetAsFirstSibling();
            RectTransform bRect = border.GetComponent<RectTransform>();
            bRect.anchorMin = Vector2.zero; bRect.anchorMax = Vector2.one;
            bRect.offsetMin = new Vector2(-3f, -3f); bRect.offsetMax = new Vector2(3f, 3f);
            border.GetComponent<Image>().color = Color.Lerp(baseColor, Color.black, 0.3f);
        }

        TextMeshProUGUI text = EnsureButtonText(buttonObject.transform, "Label", label);
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = true;
        text.fontSizeMin = 14f;
        text.fontSizeMax = 26f;

        return button;
    }

    // ----------------------------------------------------------------
    // ICON COUNTER SLOT  (emoji icon above, number below)
    // ----------------------------------------------------------------
    private TextMeshProUGUI EnsureIconCounterSlot(RectTransform parent, string name, string icon, string initialCount, Vector2 anchoredPosition)
    {
        Transform existing = parent.Find(name);
        GameObject slotObj = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        if (slotObj.transform.parent != parent) slotObj.transform.SetParent(parent, false);

        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0.5f, 0.5f);
        slotRect.anchorMax = new Vector2(0.5f, 0.5f);
        slotRect.pivot = new Vector2(0.5f, 0.5f);
        slotRect.anchoredPosition = anchoredPosition;
        slotRect.sizeDelta = new Vector2(160f, 70f);

        // Icon label (top half)
        Transform iconT = slotObj.transform.Find("Icon");
        if (iconT == null)
        {
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform));
            iconObj.transform.SetParent(slotObj.transform, false);
            RectTransform iRect = iconObj.GetComponent<RectTransform>();
            iRect.anchorMin = new Vector2(0f, 0.5f); iRect.anchorMax = new Vector2(1f, 1f);
            iRect.offsetMin = Vector2.zero; iRect.offsetMax = Vector2.zero;
            TextMeshProUGUI iconTxt = iconObj.AddComponent<TextMeshProUGUI>();
            iconTxt.text = icon;
            iconTxt.fontSize = 28f;
            iconTxt.alignment = TextAlignmentOptions.Center;
            iconTxt.color = Color.white;
            iconTxt.raycastTarget = false;
        }

        // Count label (bottom half)
        Transform countT = slotObj.transform.Find("Count");
        TextMeshProUGUI countTxt;
        if (countT == null)
        {
            GameObject countObj = new GameObject("Count", typeof(RectTransform));
            countObj.transform.SetParent(slotObj.transform, false);
            RectTransform cRect = countObj.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0f, 0f); cRect.anchorMax = new Vector2(1f, 0.5f);
            cRect.offsetMin = Vector2.zero; cRect.offsetMax = Vector2.zero;
            countTxt = countObj.AddComponent<TextMeshProUGUI>();
            countTxt.text = initialCount;
            countTxt.fontSize = 22f;
            countTxt.fontStyle = FontStyles.Bold;
            countTxt.alignment = TextAlignmentOptions.Center;
            countTxt.color = new Color(0.15f, 0.08f, 0.02f);
            countTxt.raycastTarget = false;
        }
        else
        {
            countTxt = countT.GetComponent<TextMeshProUGUI>();
        }
        return countTxt;
    }

    // ----------------------------------------------------------------
    // EnsureButton (legacy – used for upgrade buttons inside drawer)
    // ----------------------------------------------------------------
    private Button EnsureButton(RectTransform parent, string name, string label, Vector2 anchoredPosition, Vector2 size)
    {
        return EnsureHarvestButton(parent, name, label, anchoredPosition, size, StoryColorPalette.ButtonGreen);
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

    private void SetPanelActiveWithShadow(GameObject panel, bool active)
    {
        if (panel == null) return;
        panel.SetActive(active);

        // Find sibling shadow in parent
        Transform parent = panel.transform.parent;
        if (parent != null)
        {
            Transform shadow = parent.Find(panel.name + "_Shadow");
            if (shadow != null) shadow.gameObject.SetActive(active);
        }
    }
}

/// <summary>
/// VisualFeedbackManager - Global utility for "Juice" and visual polish.
/// Handles Screen Shake, Scale Punches, and UI Transitions.
/// </summary>
public class VisualFeedbackManager : MonoBehaviour
{
    public static VisualFeedbackManager Instance { get; private set; }

    private Camera mainCamera;
    private Vector3 cameraOrigin;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            mainCamera = Camera.main;
            if (mainCamera != null) cameraOrigin = mainCamera.transform.position;
        }
    }

    public void ShakeCamera(float duration = 0.2f, float intensity = 0.1f)
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(DoShake(duration, intensity));
    }

    private IEnumerator DoShake(float duration, float intensity)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = intensity * (1f - elapsed / duration);
            mainCamera.transform.position = cameraOrigin + (Vector3)Random.insideUnitCircle * strength;
            yield return null;
        }
        mainCamera.transform.position = cameraOrigin;
        shakeCoroutine = null;
    }

    public void SlideIn(RectTransform target, Vector2 fromOffset, float duration = 0.5f)
    {
        if (target == null) return;
        StartCoroutine(DoSlideIn(target, fromOffset, duration));
    }

    private IEnumerator DoSlideIn(RectTransform target, Vector2 fromOffset, float duration)
    {
        Vector2 originalPos = target.anchoredPosition;
        Vector2 startPos = originalPos + fromOffset;
        target.anchoredPosition = startPos;

        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            progress = 1f - Mathf.Pow(1f - progress, 3f);
            target.anchoredPosition = Vector2.Lerp(startPos, originalPos, progress);
            yield return null;
        }
        target.anchoredPosition = originalPos;
    }

}
