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
    public const int StoryUpgradeCount = 10;
    public static readonly int[] StoryUpgradeCosts = {
        100, 150, 200, 175, 225, 300, 200, 400, 350, 500
    };
    public static readonly string[] StoryUpgradeNames = {
        "Wheat Field",
        "Chicken Care",
        "Build Barn",
        "Cow Feed",
        "Dairy Care",
        "Carrot Garden",
        "Fertilizer",
        "Pig Pen",
        "Speed Boots",
        "Bigger Store"
    };

    public static UIManager Instance { get; private set; }

    public event System.Action OnManagementOpened;

    [Header("Resource Displays")]
    [SerializeField] private TextMeshProUGUI cornCountText;
    [SerializeField] private TextMeshProUGUI eggsCountText;
    [SerializeField] private TextMeshProUGUI coinsCountText;
    [SerializeField] private TextMeshProUGUI helperCountText;
    [SerializeField] private TextMeshProUGUI incomeRateText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI storyLevelText;

    [SerializeField] private RectTransform coinsIcon;

    [Header("Inventory Sidebar (Left)")]
    private RectTransform inventorySidebar;
    private RectTransform sidebarContent;
    private Dictionary<string, TextMeshProUGUI> resourceCounts = new Dictionary<string, TextMeshProUGUI>();
    private Dictionary<string, float> displayedResourceValues = new Dictionary<string, float>();
    private Dictionary<string, GameObject> resourceSlots = new Dictionary<string, GameObject>(); // For gating Wheat/Milk slots

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
    public RectTransform IncubateButtonTransform => incubateButton != null ? incubateButton.GetComponent<RectTransform>() : null;
    public RectTransform PlantButtonTransform => plantButton != null ? plantButton.GetComponent<RectTransform>() : null;
    public RectTransform ShopButtonTransform => managementToggleButton != null ? managementToggleButton.GetComponent<RectTransform>() : null;

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

        CreateInventorySidebar();

        // Initialize upgrade tracking
        upgradesPurchased = new bool[availableUpgrades != null ? availableUpgrades.Length : 0];
        SyncUpgradeLevelsFromGameManager();

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

        if (EnvironmentManager.Instance == null)
        {
            Debug.LogWarning("[UIManager] EnvironmentManager missing from scene. Expected authored scene object, not runtime bootstrap.");
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

                VisualFeedbackManager drawerVfx = VisualFeedbackManager.Instance ?? FindFirstObjectByType<VisualFeedbackManager>();
                drawerVfx?.SlideIn(rect, new Vector2(0, -1000f), 0.4f);
                
                OnManagementOpened?.Invoke();
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
        
        VisualFeedbackManager globalVfx = VisualFeedbackManager.Instance ?? FindFirstObjectByType<VisualFeedbackManager>();
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
        
        // HUD: Removed top-level inventory counters in favor of sidebar as requested by user.
        // Keeping only essential top-level info like Time and Act title if needed.

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

        harvestButton  = EnsureHarvestButton(actionBar, "Btn1", "Corn\nHarvest", new Vector2(-btnSpacing * 2f, btnY), btnSize, new Color(0.56f, 0.76f, 0.36f));
        feedButton     = EnsureHarvestButton(actionBar, "Btn2", "Corn\nFeed",    new Vector2(-btnSpacing,      btnY), btnSize, new Color(0.56f, 0.76f, 0.36f));
        collectButton  = EnsureHarvestButton(actionBar, "Btn3", "Egg\nCollect", new Vector2(0,               btnY), btnSize, new Color(0.56f, 0.76f, 0.36f));
        sellButton     = EnsureHarvestButton(actionBar, "Btn4", "Coins\nSell",    new Vector2(btnSpacing,       btnY), btnSize, new Color(0.89f, 0.65f, 0.22f));
        
        // MANAGEMENT TOGGLE BUTTON
        managementToggleButton = EnsureHarvestButton(actionBar, "BtnShop", "Shop\nMenu", new Vector2(btnSpacing * 2f, btnY), btnSize, new Color(0.68f, 0.44f, 0.22f));
        managementToggleButton.onClick.RemoveAllListeners();
        managementToggleButton.onClick.AddListener(ToggleManagement);

        upgradePanel = upgradesSub != null ? upgradesSub.gameObject : upgradePanel;

        // Specific Hire Helper button update - Removing it from Horizontal group by not making it a direct child if we want it below, 
        // but for now let's just use it as part of the list or handle it after
        hireHelperButton = EnsureButton(expansionsSub, "HireBtn", "Hire Helper", Vector2.zero, new Vector2(250f, 60f));

        // Unlock Wheat/Cow Buttons in Shop
        Button unlockWheatBtn = EnsureButton(expansionsSub, "UnlockWheatBtn", "Unlock Wheat (50 Gold)", Vector2.zero, new Vector2(250f, 60f));
        unlockWheatBtn.onClick.AddListener(() => {
            if (GameManager.Instance.SpendCoins(50)) {
                GameManager.Instance.RegisterWheatPurchase();
                unlockWheatBtn.gameObject.SetActive(false);
            } else {
                ShowCannotAfford(50 - GameManager.Instance.Coins);
            }
        });

        Button unlockCowBtn = EnsureButton(expansionsSub, "UnlockCowBtn", "Unlock Cow (500 Gold)", Vector2.zero, new Vector2(250f, 60f));
        unlockCowBtn.onClick.AddListener(() => {
            if (GameManager.Instance.SpendCoins(500)) {
                GameManager.Instance.RegisterCowPurchase();
                unlockCowBtn.gameObject.SetActive(false);
            } else {
                ShowCannotAfford(500 - GameManager.Instance.Coins);
            }
        });

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
            DayNightCycle cycle = FindFirstObjectByType<DayNightCycle>();
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
            HarvestableField field = FindFirstObjectByType<HarvestableField>();
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
            Chicken chicken = FindFirstObjectByType<Chicken>();
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
        // Tween legacy hardcoded counters
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

        // Tween Sidebar Counters
        if (resourceCounts == null) return;

        foreach (var pair in resourceCounts)
        {
            string id = pair.Key;
            TextMeshProUGUI tmp = pair.Value;
            
            int target = GetTargetValueFor(id);
            float current = displayedResourceValues.ContainsKey(id) ? displayedResourceValues[id] : 0f;

            if (Mathf.Abs(current - target) > 0.1f)
            {
                current = Mathf.Lerp(current, target, Time.deltaTime / numberTweenDuration * 5f);
                displayedResourceValues[id] = current;
                tmp.text = Mathf.RoundToInt(current).ToString();
            }
            else if (current != target)
            {
                current = target;
                displayedResourceValues[id] = current;
                tmp.text = target.ToString();
            }
        }
    }

    private int GetTargetValueFor(string id)
    {
        if (GameManager.Instance == null) return 0;
        
        if (id == "Corn") return GameManager.Instance.Corn;
        if (id == "Egg") return GameManager.Instance.Eggs;
        if (id == "Coins") return GameManager.Instance.Coins;
        if (id == "Wheat") return GameManager.Instance.GetItemCount("Wheat");
        if (id == "Milk") return GameManager.Instance.GetItemCount("Milk");
        
        return 0;
    }

    /// <summary>
    /// Forces an immediate update of all resource text labels in the sidebar
    /// </summary>
    public void UpdateAllResourceText()
    {
        if (resourceCounts == null) return;
        
        foreach (var pair in resourceCounts)
        {
            int targetValue = GetTargetValueFor(pair.Key);
            pair.Value.text = targetValue.ToString();
            displayedResourceValues[pair.Key] = (float)targetValue;
        }
        
        // Also update button states as they might depend on these values
        UpdateButtonStates();
    }

    /// <summary>
    /// Unlocks and shows a resource slot in the sidebar (e.g., Wheat or Milk after purchase)
    /// </summary>
    public void UnlockResourceSlot(string slotId)
    {
        if (sidebarContent == null)
        {
            CreateInventorySidebar();
        }

        if (!resourceSlots.ContainsKey(slotId) || resourceSlots[slotId] == null)
        {
            CreateResourceSlot(slotId, GetFallbackIconPathForSlot(slotId));
        }

        if (resourceSlots.ContainsKey(slotId) && resourceSlots[slotId] != null)
        {
            resourceSlots[slotId].SetActive(true);
        }
    }

    private string GetFallbackIconPathForSlot(string slotId)
    {
        switch (slotId)
        {
            case "Corn":
                return "Sprite_Corn_icon";
            case "Egg":
                return "egg_icon";
            case "Wheat":
                return "Sprite_Wheat_icon";
            case "Milk":
                return "Sprite_coin_icon";
            case "Carrot":
                return "Sprite_Corn_icon";
            case "Truffle":
                return "Sprite_coin_icon";
            default:
                return "Sprite_Button_Blue";
        }
    }

    public void SyncUpgradeLevelsFromGameManager()
    {
        if (GameManager.Instance == null || availableUpgrades == null)
        {
            return;
        }

        for (int i = 0; i < availableUpgrades.Length; i++)
        {
            UpgradeData data = availableUpgrades[i];
            if (data == null)
            {
                continue;
            }

            data.currentLevel = GameManager.Instance.GetUpgradeLevel(data.upgradeType);
        }
    }

    private void CreateInventorySidebar()
    {
        if (sidebarContent != null)
        {
            return;
        }

        // Create the sidebar root anchored to the Left Middle - compact size
        GameObject sidebarObj = new GameObject("InventorySidebar");
        sidebarObj.transform.SetParent(transform, false);
        inventorySidebar = sidebarObj.AddComponent<RectTransform>();
        inventorySidebar.anchorMin = new Vector2(0, 0.5f);
        inventorySidebar.anchorMax = new Vector2(0, 0.5f);
        inventorySidebar.pivot = new Vector2(0, 0.5f);
        inventorySidebar.sizeDelta = new Vector2(180, 280); // Compact: width 180, height 280
        inventorySidebar.anchoredPosition = new Vector3(15, 0, 0);

        // Add a background panel
        Image bg = sidebarObj.AddComponent<Image>();
        bg.sprite = Resources.Load<Sprite>("Sprite_Button_Blue");
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0, 0, 0, 0.6f); // Slightly transparent
        
        // Add ScrollRect
        ScrollRect scroll = sidebarObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.viewport = sidebarObj.GetComponent<RectTransform>();
        
        // Add Mask for clipping
        sidebarObj.AddComponent<CanvasRenderer>();
        sidebarObj.AddComponent<RectMask2D>();

        // Content Container
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(sidebarObj.transform, false);
        sidebarContent = contentObj.AddComponent<RectTransform>();
        sidebarContent.anchorMin = new Vector2(0, 1);
        sidebarContent.anchorMax = new Vector2(1, 1);
        sidebarContent.pivot = new Vector2(0.5f, 1);
        sidebarContent.sizeDelta = new Vector2(0, 0);

        scroll.content = sidebarContent;

        VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 6;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Populate Slots with Happy Harvest Names
        CreateResourceSlot("Coins", "Sprite_coin_icon");
        CreateResourceSlot("Corn", "Sprite_Corn_icon");
        CreateResourceSlot("Egg", "egg_icon");
        CreateResourceSlot("Wheat", "Sprite_Wheat_icon");
        CreateResourceSlot("Milk", "milk_bottle_icon");
        CreateResourceSlot("Carrot", "Sprite_Corn_icon");
        CreateResourceSlot("Truffle", "Sprite_coin_icon");
        
        // Gate Wheat/Milk slots until unlocked
        if (resourceSlots.ContainsKey("Wheat"))
            resourceSlots["Wheat"]?.SetActive(false);
        if (resourceSlots.ContainsKey("Milk"))
            resourceSlots["Milk"]?.SetActive(false); 
        if (resourceSlots.ContainsKey("Carrot"))
            resourceSlots["Carrot"]?.SetActive(false);
        if (resourceSlots.ContainsKey("Truffle"))
            resourceSlots["Truffle"]?.SetActive(false);
    }

    private void CreateResourceSlot(string id, string iconPath)
    {
        GameObject slotObj = new GameObject("Slot_" + id);
        slotObj.transform.SetParent(sidebarContent, false);
        RectTransform rt = slotObj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 50); // Compact slot height

        // Background for slot
        Image bg = slotObj.AddComponent<Image>();
        bg.sprite = Resources.Load<Sprite>("Sprite_Button_Blue");
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.4f); // Subtle, less opaque

        // Horizontal Layout for Slot Content
        HorizontalLayoutGroup layout = slotObj.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 4, 4);
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;

        // Icon - smaller
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(slotObj.transform, false);
        RectTransform iconRT = iconObj.AddComponent<RectTransform>();
        iconRT.sizeDelta = new Vector2(36, 36); // Compact icon

        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.sprite = Resources.Load<Sprite>(iconPath);
        if (iconImg.sprite == null)
        {
            iconImg.sprite = Resources.Load<Sprite>("Sprite_Button_Blue");
        }
        iconImg.preserveAspect = true;

        // Name/Label
        GameObject nameObj = new GameObject("Label");
        nameObj.transform.SetParent(slotObj.transform, false);
        RectTransform nameRT = nameObj.AddComponent<RectTransform>();
        nameRT.sizeDelta = new Vector2(50, 36);

        TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
        nameTmp.text = id;
        nameTmp.fontSize = 14;
        nameTmp.font = cornCountText != null ? cornCountText.font : null;
        nameTmp.alignment = TextAlignmentOptions.Left;
        nameTmp.color = Color.white;

        // Count Text
        GameObject textObj = new GameObject("Count");
        textObj.transform.SetParent(slotObj.transform, false);
        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.sizeDelta = new Vector2(45, 36);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 16;
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.text = "0";
        tmp.color = Color.white;
        tmp.font = cornCountText != null ? cornCountText.font : null;

        resourceCounts[id] = tmp;
        displayedResourceValues[id] = 0f;
        resourceSlots[id] = slotObj;
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
            HarvestableField field = FindFirstObjectByType<HarvestableField>();
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
            bool canCollect = FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None).Length > 0;
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
        TitleCardManager tcm = FindFirstObjectByType<TitleCardManager>();
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
            
            // Sidebar sync
            foreach(var id in resourceCounts.Keys)
            {
                int val = GetTargetValueFor(id);
                displayedResourceValues[id] = val;
                resourceCounts[id].text = val.ToString();
            }

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

    public void UpdateExpansionButtons()
    {
        if (GameManager.Instance == null) return;
        
        // Find counts from active zones
        FarmZoneController chickenZone = GameManager.Instance.ActiveZoneControllers.Find(z => z.template.id == "Chicken");
        FarmZoneController cornZone = GameManager.Instance.ActiveZoneControllers.Find(z => z.template.id == "Corn");

        if (chickenZone == null || cornZone == null)
        {
            string activeIDs = string.Join(", ", GameManager.Instance.ActiveZoneControllers.ConvertAll(z => z.template.id));
            Debug.LogWarning($"[UIManager] Zone mismatch detected. Active IDs: [{activeIDs}]. Needed: [Chicken, Corn]");
        }

        int chickenCount = chickenZone != null ? chickenZone.CurrentCount : 0;
        int cornCount = cornZone != null ? cornZone.CurrentCount : 0;
        int chickenMax = chickenZone != null ? chickenZone.template.maxSlots : 9;
        int cornMax = cornZone != null ? cornZone.template.maxSlots : 9;

        bool chickenAtMax = chickenCount >= chickenMax;
        bool cornAtMax = cornCount >= cornMax;

        if (incubateButton != null) 
        {
            bool canIncubate = !chickenAtMax && GameManager.Instance.Eggs >= 1;
            incubateButton.interactable = canIncubate;
            incubateButton.gameObject.SetActive(!chickenAtMax);
            TextMeshProUGUI label = incubateButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = chickenAtMax ? "Chicken Max" : "Incubate (1 Egg)";
            UpdateButtonVisual(incubateButton, canIncubate);
        }

        if (plantButton != null) 
        {
            bool canPlant = !cornAtMax && GameManager.Instance.Corn >= 1;
            plantButton.interactable = canPlant;
            plantButton.gameObject.SetActive(!cornAtMax);
            TextMeshProUGUI label = plantButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = cornAtMax ? "Corn Max" : "Plant (1 Corn)";
            UpdateButtonVisual(plantButton, canPlant);
        }
    }

    private void SetupExpansionButtons()
    {
        Sprite greenBtn = Resources.Load<Sprite>("Sprite_Button_green");
        Sprite blueBtn = Resources.Load<Sprite>("Sprite_Button_Blue");
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("LiberationSans SDF");
        if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        if (incubateButton != null)
        {
            if (blueBtn != null) incubateButton.image.sprite = blueBtn;
            incubateButton.image.type = Image.Type.Sliced;
            
            TextMeshProUGUI label = incubateButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) {
                if (font != null) label.font = font;
                label.color = Color.white;
                label.fontStyle = FontStyles.Bold;
            }

            incubateButton.onClick.RemoveAllListeners();
            incubateButton.onClick.AddListener(OnIncubateClicked);
        }
        else
        {
            Debug.LogWarning("[UIManager] incubateButton is NULL in SetupExpansionButtons!");
        }
        
        if (plantButton != null)
        {
            if (greenBtn != null) plantButton.image.sprite = greenBtn;
            plantButton.image.type = Image.Type.Sliced;

            TextMeshProUGUI label = plantButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) {
                if (font != null) label.font = font;
                label.color = Color.white;
                label.fontStyle = FontStyles.Bold;
            }

            plantButton.onClick.RemoveAllListeners();
            plantButton.onClick.AddListener(OnPlantClicked);
        }
        else
        {
            Debug.LogWarning("[UIManager] plantButton is NULL in SetupExpansionButtons!");
        }
    }

    private void OnIncubateClicked()
    {
        Debug.Log("[UIManager] Incubate button clicked!");
        if (GameManager.Instance != null)
        {
            Debug.Log($"[UIManager] Calling AddChicken(). Current Eggs: {GameManager.Instance.Eggs}");
            GameManager.Instance.AddChicken();
        }
        else
        {
            Debug.LogError("[UIManager] GameManager.Instance is NULL!");
        }
        UpdateExpansionButtons();
    }

    private void OnPlantClicked()
    {
        Debug.Log("[UIManager] Plant button clicked!");
        if (GameManager.Instance != null)
        {
            Debug.Log($"[UIManager] Calling AddCornField(). Current Corn: {GameManager.Instance.Corn}");
            GameManager.Instance.AddCornField();
        }
        else
        {
            Debug.LogError("[UIManager] GameManager.Instance is NULL!");
        }
        UpdateExpansionButtons();
    }

    // Event handlers
    private void OnCornChanged(int newValue)
    {
        bool increased = newValue > targetCorn;
        targetCorn = newValue;

        // Sidebar UI
        if (resourceCounts.ContainsKey("Corn") && increased)
        {
            PunchScale(resourceCounts["Corn"].rectTransform);
        }
    }

    private void OnEggsChanged(int newValue)
    {
        bool increased = newValue > targetEggs;
        targetEggs = newValue;

        // Sidebar UI
        if (resourceCounts.ContainsKey("Egg") && increased)
        {
            PunchScale(resourceCounts["Egg"].rectTransform);
        }
    }

    private void OnCoinsChanged(int newValue)
    {
        bool increased = newValue > targetCoins;
        targetCoins = newValue;

        // Legacy UI
        if (increased && coinsIcon != null)
        {
            PunchScale(coinsIcon);
        }

        // Sidebar UI
        if (resourceCounts.ContainsKey("Coins") && increased)
        {
            PunchScale(resourceCounts["Coins"].rectTransform);
        }

        UpdateNextGoal();
        UpdateIncomeRate();
        UpdateExpansionButtons();
    }

    // Generic handler for sidebar resources (Wheat, Milk, etc.)
    public void UpdateResourceUI(string id)
    {
        if (resourceCounts.ContainsKey(id))
        {
            PunchScale(resourceCounts[id].rectTransform);
        }
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
        return FindFirstObjectByType<PlayerController>();
    }

    private void OnHarvestClicked()
    {
        try { AudioManager.Instance?.PlaySound("click"); } catch {}
        HarvestableField field = FindFirstObjectByType<HarvestableField>();
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
        Chicken chicken = FindFirstObjectByType<Chicken>();
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
        CollectibleItem[] items = FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None);
        foreach (var egg in items)
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
        StoreCounter store = FindFirstObjectByType<StoreCounter>();
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
            UpgradeType.WheatField,
            UpgradeType.ChickenCare,
            UpgradeType.CowPen,
            UpgradeType.CowFeed,
            UpgradeType.MilkProduction,
            UpgradeType.CarrotGarden,
            UpgradeType.Fertilizer,
            UpgradeType.PigPen,
            UpgradeType.HelperSpeed,
            UpgradeType.BiggerStore
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
                TitleCardManager titleCardManager = FindFirstObjectByType<TitleCardManager>();
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
        EventSystem[] systems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
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
        EventSystem[] systems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
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
