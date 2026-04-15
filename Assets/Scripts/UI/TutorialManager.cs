using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// TutorialManager - Guides new players through the core game loop with
/// step-by-step instructions and visual indicators.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    /// <summary>
    /// Tutorial steps that guide the player through the game loop
    /// </summary>
    public enum TutorialStep
    {
        Welcome,
        HarvestCorn,
        FeedChicken,
        CollectEgg,
        SellEgg,
        HireHelper,
        Complete
    }

    [Header("Tutorial UI")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private RectTransform arrowIndicator;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button nextButton;

    [Header("Tutorial Messages")]
    [SerializeField] private string welcomeMessage = "Welcome to Chicken Coop! 🐔\nLet's learn how to run your farm.";
    [SerializeField] private string harvestMessage = "Tap the corn field to harvest corn! 🌽";
    [SerializeField] private string feedMessage = "Great! Now feed the chicken by tapping on it. 🐔";
    [SerializeField] private string collectMessage = "Collect the egg the chicken laid! 🥚";
    [SerializeField] private string sellMessage = "Sell your egg at the store for coins! 💰";
    [SerializeField] private string hireMessage = "You've earned enough coins! Hire a helper to automate. 👷";
    [SerializeField] private string completeMessage = "Excellent! You've mastered the basics!\nKeep farming to grow your empire! 🏆";

    [Header("Target References")]
    [SerializeField] private Transform cornFieldTarget;
    [SerializeField] private Transform chickenTarget;
    [SerializeField] private Transform storeTarget;
    [SerializeField] private RectTransform hireButtonTarget;

    [Header("Settings")]
    [SerializeField] private float arrowBobSpeed = 2f;
    [SerializeField] private float arrowBobAmount = 10f;
    [SerializeField] private bool showOnFirstPlay = true;

    private TutorialStep currentStep = TutorialStep.Welcome;
    private bool tutorialActive = false;
    private bool tutorialCompleted = false;
    private Vector3 arrowBasePosition;

    // Track actions for step completion
    private int cornHarvestedDuringTutorial = 0;
    private int eggsCollectedDuringTutorial = 0;

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
        EnsureRuntimeReferences();
        ResolveTargets();

        // Check if tutorial was already completed
        tutorialCompleted = PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;

        // Setup button listeners
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipTutorial);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextClicked);
        }

        // Start tutorial on first play
        if (showOnFirstPlay && !tutorialCompleted)
        {
            StartCoroutine(StartTutorialDelayed());
        }

        // Subscribe to game events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCornChanged += OnCornChanged;
            GameManager.Instance.OnEggsChanged += OnEggsChanged;
            GameManager.Instance.OnCoinsChanged += OnCoinsChanged;
            GameManager.Instance.OnHelperCountChanged += OnHelperHired;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCornChanged -= OnCornChanged;
            GameManager.Instance.OnEggsChanged -= OnEggsChanged;
            GameManager.Instance.OnCoinsChanged -= OnCoinsChanged;
            GameManager.Instance.OnHelperCountChanged -= OnHelperHired;
        }
    }

    private void Update()
    {
        if (tutorialActive && arrowIndicator != null && arrowIndicator.gameObject.activeSelf)
        {
            // Bob the arrow up and down
            float bob = Mathf.Sin(Time.time * arrowBobSpeed) * arrowBobAmount;
            arrowIndicator.anchoredPosition = arrowBasePosition + new Vector3(0, bob, 0);
        }

        if (tutorialActive && currentStep == TutorialStep.FeedChicken)
        {
            CollectibleEgg egg = FindAnyObjectByType<CollectibleEgg>();
            if (egg != null)
            {
                AdvanceToStep(TutorialStep.CollectEgg);
            }
        }
    }

    private IEnumerator StartTutorialDelayed()
    {
        yield return new WaitForSeconds(1f);
        StartTutorial();
    }

    /// <summary>
    /// Start the tutorial sequence
    /// </summary>
    public void StartTutorial()
    {
        ResolveTargets();
        tutorialActive = true;
        currentStep = TutorialStep.Welcome;
        
        if (tutorialPanel != null)
        {
            tutorialPanel.transform.SetAsLastSibling();
            tutorialPanel.SetActive(true);
        }

        ShowCurrentStep();
    }

    /// <summary>
    /// Skip the tutorial entirely
    /// </summary>
    public void SkipTutorial()
    {
        tutorialActive = false;
        tutorialCompleted = true;
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        if (arrowIndicator != null)
        {
            arrowIndicator.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Handle Next button click
    /// </summary>
    private void OnNextClicked()
    {
        if (currentStep == TutorialStep.Welcome)
        {
            AdvanceToStep(TutorialStep.HarvestCorn);
        }
        else if (currentStep == TutorialStep.Complete)
        {
            CompleteTutorial();
        }
    }

    /// <summary>
    /// Display the current tutorial step
    /// </summary>
    private void ShowCurrentStep()
    {
        if (instructionText == null) return;
        ResolveTargets();

        // Hide arrow by default
        if (arrowIndicator != null)
        {
            arrowIndicator.gameObject.SetActive(false);
        }

        // Show/hide next button based on step
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(currentStep == TutorialStep.Welcome || currentStep == TutorialStep.Complete);
        }

        switch (currentStep)
        {
            case TutorialStep.Welcome:
                instructionText.text = welcomeMessage;
                break;

            case TutorialStep.HarvestCorn:
                instructionText.text = harvestMessage;
                PointArrowAt(cornFieldTarget);
                break;

            case TutorialStep.FeedChicken:
                instructionText.text = feedMessage;
                PointArrowAt(chickenTarget);
                break;

            case TutorialStep.CollectEgg:
                instructionText.text = collectMessage;
                PointArrowAt(chickenTarget); // Egg spawns near chicken
                break;

            case TutorialStep.SellEgg:
                instructionText.text = sellMessage;
                PointArrowAt(storeTarget);
                break;

            case TutorialStep.HireHelper:
                instructionText.text = hireMessage;
                PointArrowAtUI(hireButtonTarget);
                break;

            case TutorialStep.Complete:
                instructionText.text = completeMessage;
                break;
        }
    }

    /// <summary>
    /// Point the arrow indicator at a world target
    /// </summary>
    private void PointArrowAt(Transform target)
    {
        if (arrowIndicator == null || target == null) return;

        // Convert world position to screen position
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);
        
        // Convert to canvas position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            arrowIndicator.parent as RectTransform,
            screenPos,
            null,
            out Vector2 localPoint
        );

        arrowBasePosition = localPoint + new Vector2(0, 50f); // Offset above target
        arrowIndicator.anchoredPosition = arrowBasePosition;
        arrowIndicator.gameObject.SetActive(true);
    }

    /// <summary>
    /// Point the arrow indicator at a UI element
    /// </summary>
    private void PointArrowAtUI(RectTransform target)
    {
        if (arrowIndicator == null || target == null) return;

        // Get target's position in parent space
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 center = (corners[0] + corners[2]) / 2f;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            arrowIndicator.parent as RectTransform,
            center,
            null,
            out Vector2 localPoint
        );

        arrowBasePosition = localPoint + new Vector2(0, 50f);
        arrowIndicator.anchoredPosition = arrowBasePosition;
        arrowIndicator.gameObject.SetActive(true);
    }

    /// <summary>
    /// Advance to the next tutorial step
    /// </summary>
    private void AdvanceToStep(TutorialStep nextStep)
    {
        currentStep = nextStep;
        ShowCurrentStep();

        // Play a sound for step advancement
        AudioManager.Instance?.PlaySound("click");
    }

    /// <summary>
    /// Complete the tutorial
    /// </summary>
    private void CompleteTutorial()
    {
        tutorialActive = false;
        tutorialCompleted = true;
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        if (arrowIndicator != null)
        {
            arrowIndicator.gameObject.SetActive(false);
        }

        AudioManager.Instance?.PlaySound("upgrade");
    }

    // Event handlers to detect player actions
    private void OnCornChanged(int newValue)
    {
        if (!tutorialActive) return;

        if (currentStep == TutorialStep.HarvestCorn)
        {
            int previousCorn = cornHarvestedDuringTutorial;
            cornHarvestedDuringTutorial = newValue;
            
            if (newValue > previousCorn)
            {
                AdvanceToStep(TutorialStep.FeedChicken);
            }
        }
    }

    private void OnEggsChanged(int newValue)
    {
        if (!tutorialActive) return;

        int previousEggs = eggsCollectedDuringTutorial;
        eggsCollectedDuringTutorial = newValue;

        if (currentStep == TutorialStep.CollectEgg && newValue > previousEggs)
        {
            // Egg was collected
            AdvanceToStep(TutorialStep.SellEgg);
        }
    }

    private void OnCoinsChanged(int newValue)
    {
        if (!tutorialActive) return;

        if (currentStep == TutorialStep.SellEgg)
        {
            int helperCost = GameManager.Instance != null ? GameManager.Instance.HelperCost : 100;
            if (newValue >= helperCost)
            {
                AdvanceToStep(TutorialStep.HireHelper);
            }
            else if (instructionText != null)
            {
                int remaining = helperCost - newValue;
                instructionText.text = $"Sell eggs until you reach {helperCost} coins! {remaining} more to go. 💰";
            }
        }
    }

    private void OnHelperHired(int newValue)
    {
        if (!tutorialActive) return;

        if (currentStep == TutorialStep.HireHelper && newValue >= 1)
        {
            AdvanceToStep(TutorialStep.Complete);
        }
    }

    /// <summary>
    /// Check if tutorial is currently active
    /// </summary>
    public bool IsTutorialActive()
    {
        return tutorialActive;
    }

    /// <summary>
    /// Reset tutorial for replay
    /// </summary>
    public void ResetTutorial()
    {
        tutorialCompleted = false;
        PlayerPrefs.SetInt("TutorialCompleted", 0);
        PlayerPrefs.Save();
        
        cornHarvestedDuringTutorial = 0;
        eggsCollectedDuringTutorial = 0;
    }

    private void EnsureRuntimeReferences()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        if (eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }

        if (tutorialPanel == null)
        {
            tutorialPanel = new GameObject("TutorialPanel", typeof(RectTransform), typeof(Image));
            tutorialPanel.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = tutorialPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -210f);
            panelRect.sizeDelta = new Vector2(520f, 190f);

            Image panelImage = tutorialPanel.GetComponent<Image>();
            panelImage.color = new Color(1f, 0.98f, 0.77f, 0.92f);
            panelImage.raycastTarget = false;
        }

        if (instructionText == null)
        {
            GameObject textObject = new GameObject("InstructionText", typeof(RectTransform));
            textObject.transform.SetParent(tutorialPanel.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.65f);
            textRect.anchorMax = new Vector2(0.5f, 0.65f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0f, 10f);
            textRect.sizeDelta = new Vector2(460f, 90f);

            instructionText = textObject.AddComponent<TextMeshProUGUI>();
            instructionText.alignment = TextAlignmentOptions.Center;
            instructionText.fontSize = 24f;
            instructionText.color = StoryColorPalette.TextDark;
            instructionText.textWrappingMode = TextWrappingModes.Normal;
            instructionText.raycastTarget = false;
        }

        if (skipButton == null)
        {
            skipButton = CreateRuntimeButton(tutorialPanel.transform, "SkipButton", "Skip", new Vector2(-120f, -62f));
        }

        if (nextButton == null)
        {
            nextButton = CreateRuntimeButton(tutorialPanel.transform, "NextButton", "Next", new Vector2(120f, -62f));
        }

        if (arrowIndicator == null)
        {
            GameObject arrowObject = new GameObject("TutorialArrow", typeof(RectTransform));
            arrowObject.transform.SetParent(canvas.transform, false);
            arrowIndicator = arrowObject.GetComponent<RectTransform>();
            arrowIndicator.sizeDelta = new Vector2(60f, 60f);

            TextMeshProUGUI arrowText = arrowObject.AddComponent<TextMeshProUGUI>();
            arrowText.text = "▼";
            arrowText.fontSize = 44f;
            arrowText.alignment = TextAlignmentOptions.Center;
            arrowText.color = StoryColorPalette.Special;
        }
    }

    private void ResolveTargets()
    {
        if (cornFieldTarget == null)
        {
            HarvestableField field = FindAnyObjectByType<HarvestableField>();
            if (field != null)
            {
                cornFieldTarget = field.transform;
            }
        }

        if (chickenTarget == null)
        {
            Chicken chicken = FindAnyObjectByType<Chicken>();
            if (chicken != null)
            {
                chickenTarget = chicken.transform;
            }
        }

        if (storeTarget == null)
        {
            StoreCounter store = FindAnyObjectByType<StoreCounter>();
            if (store != null)
            {
                storeTarget = store.transform;
            }
        }

        if (hireButtonTarget == null && UIManager.Instance != null)
        {
            hireButtonTarget = UIManager.Instance.HireHelperButtonTransform;
        }
    }

    private Button CreateRuntimeButton(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(150f, 42f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = StoryColorPalette.ButtonBlue;
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        GameObject textObject = new GameObject("Label", typeof(RectTransform));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 20f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = StoryColorPalette.TextDark;
        text.raycastTarget = false;

        return button;
    }
}
