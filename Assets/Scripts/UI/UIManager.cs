using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UIManager - Manages all UI elements including resource displays, upgrade buttons,
/// and visual feedback animations like number tweening and punch-scale effects.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Resource Displays")]
    [SerializeField] private TextMeshProUGUI cornCountText;
    [SerializeField] private TextMeshProUGUI eggsCountText;
    [SerializeField] private TextMeshProUGUI coinsCountText;
    [SerializeField] private TextMeshProUGUI helperCountText;

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

    [Header("Upgrade Panel")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private TextMeshProUGUI[] upgradeCostTexts;

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

        // Initialize displays
        UpdateAllDisplays();
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
    }

    /// <summary>
    /// Setup button click listeners
    /// </summary>
    private void SetupButtons()
    {
        if (harvestButton != null)
        {
            harvestButton.onClick.AddListener(OnHarvestClicked);
        }

        if (feedButton != null)
        {
            feedButton.onClick.AddListener(OnFeedClicked);
        }

        if (collectButton != null)
        {
            collectButton.onClick.AddListener(OnCollectClicked);
        }

        if (sellButton != null)
        {
            sellButton.onClick.AddListener(OnSellClicked);
        }

        if (hireHelperButton != null)
        {
            hireHelperButton.onClick.AddListener(OnHireHelperClicked);
        }

        // Setup upgrade buttons
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int index = i;
            if (upgradeButtons[i] != null)
            {
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
        if (feedButton != null)
        {
            bool canFeed = GameManager.Instance.Corn > 0;
            feedButton.interactable = canFeed;
            UpdateButtonVisual(feedButton, canFeed);
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
    /// Update upgrade button visuals
    /// </summary>
    private void UpdateUpgradeButtons()
    {
        int[] upgradeCosts = { 100, 200, 300, 500, 750 };

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (upgradeButtons[i] != null && i < upgradeCosts.Length)
            {
                bool canAfford = GameManager.Instance.CanAfford(upgradeCosts[i]);
                upgradeButtons[i].interactable = canAfford;
                UpdateButtonVisual(upgradeButtons[i], canAfford);

                if (i < upgradeCostTexts.Length && upgradeCostTexts[i] != null)
                {
                    upgradeCostTexts[i].text = $"{upgradeCosts[i]}";
                }
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
    }

    private void OnEggsChanged(int newValue)
    {
        bool increased = newValue > targetEggs;
        targetEggs = newValue;

        if (increased && eggsIcon != null)
        {
            PunchScale(eggsIcon);
        }
    }

    private void OnCoinsChanged(int newValue)
    {
        bool increased = newValue > targetCoins;
        targetCoins = newValue;

        if (increased && coinsIcon != null)
        {
            PunchScale(coinsIcon);
        }
    }

    private void OnHelperCountChanged(int newValue)
    {
        if (helperCountText != null)
        {
            helperCountText.text = newValue.ToString();
        }
    }

    // Button click handlers
    private void OnHarvestClicked()
    {
        HarvestableField field = FindObjectOfType<HarvestableField>();
        if (field != null)
        {
            field.Harvest();
        }
    }

    private void OnFeedClicked()
    {
        Chicken chicken = FindObjectOfType<Chicken>();
        if (chicken != null && chicken.CanInteract())
        {
            chicken.Feed();
        }
    }

    private void OnCollectClicked()
    {
        // Note: FindObjectsOfType is acceptable here as this is a simple idle game
        // with typically very few eggs in the scene at once
        CollectibleEgg[] eggs = FindObjectsOfType<CollectibleEgg>();
        foreach (var egg in eggs)
        {
            egg.Interact();
            break; // Collect one at a time
        }
    }

    private void OnSellClicked()
    {
        StoreCounter store = FindObjectOfType<StoreCounter>();
        if (store != null && store.CanInteract())
        {
            store.SellEgg();
        }
    }

    private void OnHireHelperClicked()
    {
        if (GameManager.Instance.HireHelper())
        {
            // Play hire animation
            if (hireHelperButton != null)
            {
                PunchScale(hireHelperButton.GetComponent<RectTransform>());
            }
        }
    }

    private void OnUpgradeClicked(int upgradeIndex)
    {
        int[] upgradeCosts = { 100, 200, 300, 500, 750 };
        UpgradeType[] upgradeTypes = {
            UpgradeType.CornField,
            UpgradeType.ChickenProduction,
            UpgradeType.EggPrice,
            UpgradeType.Speed,
            UpgradeType.StoreCapacity
        };

        if (upgradeIndex < upgradeCosts.Length)
        {
            if (GameManager.Instance.SpendCoins(upgradeCosts[upgradeIndex]))
            {
                GameManager.Instance.ApplyUpgrade(upgradeTypes[upgradeIndex], 1.2f);

                // Disable the button after purchase
                if (upgradeIndex < upgradeButtons.Length && upgradeButtons[upgradeIndex] != null)
                {
                    upgradeButtons[upgradeIndex].interactable = false;
                    PunchScale(upgradeButtons[upgradeIndex].GetComponent<RectTransform>());
                }

                ShowUpgradeNotification($"Upgrade purchased!");
            }
        }
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
        StartCoroutine(ShowNotification(message));
    }

    private IEnumerator ShowNotification(string message)
    {
        // Create notification text
        GameObject notificationObj = new GameObject("Notification");
        notificationObj.transform.SetParent(transform, false);

        TextMeshProUGUI text = notificationObj.AddComponent<TextMeshProUGUI>();
        text.text = message;
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.9f, 0.3f);

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
}
