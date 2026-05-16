using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ChickenCoop.Managers;

public static class StoryUpgradeSceneWiringUtility
{
    private static readonly string[] UpgradeAssetNames =
    {
        "BetterSeeds",
        "HealthierChickens",
        "PremiumEggs",
        "FasterOperations",
        "BiggerStore"
    };

    public static string AutoWireStoryScene()
    {
        var report = new StoryUpgradeWiringReport();
        CleanDuplicateRoots(report);

        Canvas canvas = EnsureCanvas(report);
        EnsureEventSystem(report);

        if (canvas == null)
        {
            report.Missing.Add("Canvas could not be created or found.");
            return report.ToSummary();
        }

        RectTransform resourcePanel = EnsurePanel(canvas.transform, "ResourcePanel", report);
        ConfigureTopPanel(resourcePanel);

        RectTransform buttonPanel = EnsurePanel(canvas.transform, "ButtonPanel", report);
        ConfigureBottomPanel(buttonPanel);

        TextMeshProUGUI cornCountText = EnsureStatusText(resourcePanel, "CornCountText", "Corn: 0", report);
        TextMeshProUGUI eggsCountText = EnsureStatusText(resourcePanel, "EggsCountText", "Eggs: 0", report);
        TextMeshProUGUI coinsCountText = EnsureStatusText(resourcePanel, "CoinsCountText", "Coins: 50", report);
        TextMeshProUGUI helperCountText = EnsureStatusText(resourcePanel, "HelperCountText", "Helpers: 0", report);
        TextMeshProUGUI incomeRateText = EnsureStatusText(resourcePanel, "IncomeRateText", "Manual play", report);
        TextMeshProUGUI nextGoalText = EnsureGoalText(canvas.transform, report);

        Button harvestButton = EnsureActionButton(buttonPanel, "HarvestButton", "Harvest", report);
        Button feedButton = EnsureActionButton(buttonPanel, "FeedButton", "Feed", report);
        Button collectButton = EnsureActionButton(buttonPanel, "CollectButton", "Collect", report);
        Button sellButton = EnsureActionButton(buttonPanel, "SellButton", "Sell", report);
        Button hireHelperButton = EnsureActionButton(buttonPanel, "HireHelperButton", "Hire Helper", report);

        RectTransform upgradePanel = EnsurePanel(canvas.transform, "UpgradePanel", report);
        ConfigureUpgradePanel(upgradePanel);

        Button[] upgradeButtons = new Button[UIManager.StoryUpgradeCount];
        TextMeshProUGUI[] upgradeNameTexts = new TextMeshProUGUI[UIManager.StoryUpgradeCount];
        TextMeshProUGUI[] upgradeCostTexts = new TextMeshProUGUI[UIManager.StoryUpgradeCount];

        for (int i = 0; i < UIManager.StoryUpgradeCount; i++)
        {
            UpgradeEntry entry = EnsureUpgradeEntry(upgradePanel, i, report);
            upgradeButtons[i] = entry.Button;
            upgradeNameTexts[i] = entry.NameText;
            upgradeCostTexts[i] = entry.CostText;
        }

        UpgradeData[] upgrades = FindUpgradeAssets(report);

        TitleCardManager titleCardManager = EnsureTitleCard(canvas.transform, report);
        if (titleCardManager != null)
        {
            titleCardManager.ApplyStoryDefaults();
            EditorUtility.SetDirty(titleCardManager);
        }

        UIManager uiManager = EnsureUIManager(report);
        if (uiManager != null)
        {
            WireUIManager(
                uiManager,
                cornCountText,
                eggsCountText,
                coinsCountText,
                helperCountText,
                incomeRateText,
                harvestButton,
                feedButton,
                collectButton,
                sellButton,
                hireHelperButton,
                upgradePanel.gameObject,
                upgradeButtons,
                upgradeCostTexts,
                upgradeNameTexts,
                upgrades,
                nextGoalText,
                report);
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        return report.ToSummary();
    }

    public static string AutoWireTitleCardOnly()
    {
        var report = new StoryUpgradeWiringReport();
        Canvas canvas = EnsureCanvas(report);
        EnsureEventSystem(report);

        if (canvas == null)
        {
            report.Missing.Add("Canvas could not be created or found.");
            return report.ToSummary();
        }

        TitleCardManager titleCardManager = EnsureTitleCard(canvas.transform, report);
        if (titleCardManager != null)
        {
            titleCardManager.ApplyStoryDefaults();
            EditorUtility.SetDirty(titleCardManager);
            report.Wired.Add("TitleCardManager story defaults");
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        return report.ToSummary();
    }

    public static string BuildValidationReport()
    {
        var lines = new List<string>();
        lines.Add("Story Upgrade Wiring Validation");
        lines.Add(string.Empty);
        
        var duplicates = GetDuplicateRoots();
        if (duplicates.Count > 0)
        {
            lines.Add($"[WARNING] Found {duplicates.Count} duplicate roots. Auto Wiring will clean them.");
            lines.Add(string.Empty);
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        lines.Add("Canvas: " + DescribeObject(canvas));

        EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
        lines.Add("EventSystem: " + DescribeObject(eventSystem));

        UIManager uiManager = Object.FindAnyObjectByType<UIManager>();
        lines.Add("UIManager: " + DescribeObject(uiManager));

        GameManager gameManager = Object.FindAnyObjectByType<GameManager>();
        lines.Add("GameManager: " + DescribeObject(gameManager));
        lines.Add("GameManager.Config: " + (gameManager != null && gameManager.Config != null ? "Assigned" : "Missing"));

        TitleCardManager titleCardManager = Object.FindAnyObjectByType<TitleCardManager>();
        lines.Add("TitleCardManager: " + DescribeObject(titleCardManager));
        if (titleCardManager != null)
        {
            lines.Add(titleCardManager.BuildValidationSummary());
        }

        if (uiManager != null)
        {
            SerializedObject serializedObject = new SerializedObject(uiManager);
            AppendReferenceStatus(lines, serializedObject, "cornCountText");
            AppendReferenceStatus(lines, serializedObject, "eggsCountText");
            AppendReferenceStatus(lines, serializedObject, "coinsCountText");
            AppendReferenceStatus(lines, serializedObject, "helperCountText");
            AppendReferenceStatus(lines, serializedObject, "incomeRateText");
            AppendReferenceStatus(lines, serializedObject, "harvestButton");
            AppendReferenceStatus(lines, serializedObject, "feedButton");
            AppendReferenceStatus(lines, serializedObject, "collectButton");
            AppendReferenceStatus(lines, serializedObject, "sellButton");
            AppendReferenceStatus(lines, serializedObject, "hireHelperButton");
            AppendReferenceStatus(lines, serializedObject, "upgradePanel");
            AppendReferenceStatus(lines, serializedObject, "nextGoalText");

            AppendArrayStatus(lines, serializedObject, "upgradeButtons", UIManager.StoryUpgradeCount);
            AppendArrayStatus(lines, serializedObject, "upgradeNameTexts", UIManager.StoryUpgradeCount);
            AppendArrayStatus(lines, serializedObject, "upgradeCostTexts", UIManager.StoryUpgradeCount);
            AppendArrayStatus(lines, serializedObject, "availableUpgrades", UIManager.StoryUpgradeCount);
        }

        UpgradeData[] upgrades = FindUpgradeAssets(null);
        int assignedUpgradeCount = 0;
        foreach (UpgradeData upgrade in upgrades)
        {
            if (upgrade != null)
            {
                assignedUpgradeCount++;
            }
        }

        lines.Add("Upgrade assets found: " + assignedUpgradeCount + "/" + UIManager.StoryUpgradeCount);
        foreach (UpgradeData upgrade in upgrades)
        {
            if (upgrade != null)
            {
                lines.Add("- " + AssetDatabase.GetAssetPath(upgrade));
            }
        }

        return string.Join("\n", lines.ToArray());
    }

    private static List<GameObject> GetDuplicateRoots()
    {
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        var duplicates = new HashSet<GameObject>();
        var byName = new System.Collections.Generic.Dictionary<string, List<GameObject>>();
        foreach (var root in roots)
        {
            if (!byName.ContainsKey(root.name)) byName[root.name] = new List<GameObject>();
            byName[root.name].Add(root);
        }

        foreach (var kvp in byName)
        {
            if (kvp.Value.Count > 1)
            {
                var sorted = new List<GameObject>(kvp.Value);
                sorted.Sort((a, b) => b.GetComponents<Component>().Length.CompareTo(a.GetComponents<Component>().Length));
                for (int i = 1; i < sorted.Count; i++)
                {
                    duplicates.Add(sorted[i]);
                }
            }
        }
        
        var store = roots.FirstOrDefault(r => r.name == "Store");
        var storeCounter = roots.FirstOrDefault(r => r.name == "StoreCounter");
        if (store != null && storeCounter != null)
        {
            duplicates.Add(store);
        }

        return duplicates.ToList();
    }

    private static void CleanDuplicateRoots(StoryUpgradeWiringReport report)
    {
        var toDelete = GetDuplicateRoots();
        foreach (var go in toDelete)
        {
            report.Wired.Add($"Removed duplicate root: {go.name}");
            GameObject.DestroyImmediate(go);
        }
    }

    private static Canvas EnsureCanvas(StoryUpgradeWiringReport report)
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            EnsureCanvasComponents(canvas.gameObject, report);
            return canvas;
        }

        GameObject canvasObject = new GameObject("Canvas");
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Story Upgrade Canvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.layer = LayerMask.NameToLayer("UI");
        report.Created.Add("Canvas");

        EnsureCanvasComponents(canvasObject, report);
        return canvas;
    }

    private static void EnsureCanvasComponents(GameObject canvasObject, StoryUpgradeWiringReport report)
    {
        Canvas canvas = GetOrAddComponent<Canvas>(canvasObject, report);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = GetOrAddComponent<CanvasScaler>(canvasObject, report);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GetOrAddComponent<GraphicRaycaster>(canvasObject, report);
    }

    private static void EnsureEventSystem(StoryUpgradeWiringReport report)
    {
        EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem != null)
        {
            GetOrAddComponent<StandaloneInputModule>(eventSystem.gameObject, report);
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create Event System");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
        report.Created.Add("EventSystem");
    }

    private static RectTransform EnsurePanel(Transform parent, string name, StoryUpgradeWiringReport report)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            RectTransform rectTransform = existing as RectTransform;
            if (rectTransform != null)
            {
                return rectTransform;
            }

            rectTransform = existing.gameObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                return rectTransform;
            }
        }

        GameObject panelObject = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(panelObject, "Create " + name);
        panelObject.transform.SetParent(parent, false);
        panelObject.layer = LayerMask.NameToLayer("UI");

        RectTransform resultRectTransform = panelObject.AddComponent<RectTransform>();
        panelObject.AddComponent<CanvasRenderer>();
        Image image = panelObject.AddComponent<Image>();
        image.color = StoryColorPalette.WithAlpha(StoryColorPalette.UIBackground, 0.88f);

        report.Created.Add(name);
        return resultRectTransform;
    }

    private static void ConfigureTopPanel(RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, -24f);
        rectTransform.sizeDelta = new Vector2(1240f, 110f);

        HorizontalLayoutGroup layout = GetOrAddComponent<HorizontalLayoutGroup>(rectTransform.gameObject, null);
        layout.padding = new RectOffset(24, 24, 18, 18);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private static void ConfigureBottomPanel(RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = new Vector2(0f, 24f);
        rectTransform.sizeDelta = new Vector2(1240f, 120f);

        HorizontalLayoutGroup layout = GetOrAddComponent<HorizontalLayoutGroup>(rectTransform.gameObject, null);
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private static void ConfigureUpgradePanel(RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(1f, 0.5f);
        rectTransform.anchorMax = new Vector2(1f, 0.5f);
        rectTransform.pivot = new Vector2(1f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(-24f, 0f);
        rectTransform.sizeDelta = new Vector2(360f, 420f);

        VerticalLayoutGroup layout = GetOrAddComponent<VerticalLayoutGroup>(rectTransform.gameObject, null);
        layout.padding = new RectOffset(18, 18, 18, 18);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    private static TextMeshProUGUI EnsureStatusText(Transform parent, string name, string defaultText, StoryUpgradeWiringReport report)
    {
        TextMeshProUGUI text = FindText(parent, name);
        if (text == null)
        {
            GameObject textObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(textObject, "Create " + name);
            textObject.transform.SetParent(parent, false);
            textObject.layer = LayerMask.NameToLayer("UI");
            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(180f, 48f);

            text = textObject.AddComponent<TextMeshProUGUI>();
            report.Created.Add(name);
        }

        ConfigureLabel(text, defaultText, 28f, TextAlignmentOptions.MidlineLeft);
        LayoutElement layoutElement = GetOrAddComponent<LayoutElement>(text.gameObject, report);
        layoutElement.preferredWidth = 180f;
        layoutElement.preferredHeight = 48f;
        return text;
    }

    private static TextMeshProUGUI EnsureGoalText(Transform parent, StoryUpgradeWiringReport report)
    {
        TextMeshProUGUI text = FindText(parent, "NextGoalText");
        if (text == null)
        {
            GameObject textObject = new GameObject("NextGoalText");
            Undo.RegisterCreatedObjectUndo(textObject, "Create NextGoalText");
            textObject.transform.SetParent(parent, false);
            textObject.layer = LayerMask.NameToLayer("UI");

            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(24f, -144f);
            rectTransform.sizeDelta = new Vector2(600f, 60f);

            text = textObject.AddComponent<TextMeshProUGUI>();
            report.Created.Add("NextGoalText");
        }

        ConfigureLabel(text, "Save 50 more coins to hire a helper!", 30f, TextAlignmentOptions.Left);
        text.color = StoryColorPalette.TextDark;
        return text;
    }

    private static Button EnsureActionButton(Transform parent, string name, string label, StoryUpgradeWiringReport report)
    {
        Button button = FindButton(parent, name);
        if (button == null)
        {
            GameObject buttonObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(buttonObject, "Create " + name);
            buttonObject.transform.SetParent(parent, false);
            buttonObject.layer = LayerMask.NameToLayer("UI");
            buttonObject.AddComponent<CanvasRenderer>();
            buttonObject.AddComponent<Image>();
            button = buttonObject.AddComponent<Button>();

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(180f, 72f);

            report.Created.Add(name);
        }

        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = StoryColorPalette.ButtonGreen;
        }

        LayoutElement layoutElement = GetOrAddComponent<LayoutElement>(button.gameObject, report);
        layoutElement.preferredWidth = 180f;
        layoutElement.preferredHeight = 72f;

        TextMeshProUGUI text = EnsureButtonLabel(button.transform, label, report);
        ConfigureLabel(text, label, 30f, TextAlignmentOptions.Center);
        return button;
    }

    private static TextMeshProUGUI EnsureButtonLabel(Transform buttonTransform, string label, StoryUpgradeWiringReport report)
    {
        TextMeshProUGUI text = FindText(buttonTransform, "Label");
        if (text == null)
        {
            GameObject textObject = new GameObject("Label");
            Undo.RegisterCreatedObjectUndo(textObject, "Create Button Label");
            textObject.transform.SetParent(buttonTransform, false);
            textObject.layer = LayerMask.NameToLayer("UI");

            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(12f, 8f);
            rectTransform.offsetMax = new Vector2(-12f, -8f);

            text = textObject.AddComponent<TextMeshProUGUI>();
            report.Created.Add(buttonTransform.name + "/Label");
        }

        ConfigureLabel(text, label, 30f, TextAlignmentOptions.Center);
        return text;
    }

    private static UpgradeEntry EnsureUpgradeEntry(Transform parent, int index, StoryUpgradeWiringReport report)
    {
        string entryName = "UpgradeButton_" + (index + 1);
        Button button = FindButton(parent, entryName);
        if (button == null)
        {
            GameObject buttonObject = new GameObject(entryName);
            Undo.RegisterCreatedObjectUndo(buttonObject, "Create " + entryName);
            buttonObject.transform.SetParent(parent, false);
            buttonObject.layer = LayerMask.NameToLayer("UI");
            buttonObject.AddComponent<CanvasRenderer>();
            buttonObject.AddComponent<Image>();
            button = buttonObject.AddComponent<Button>();

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(0f, 62f);

            VerticalLayoutGroup layout = buttonObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 10, 10);
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            report.Created.Add(entryName);
        }

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = StoryColorPalette.ButtonBlue;
        }

        LayoutElement layoutElement = GetOrAddComponent<LayoutElement>(button.gameObject, report);
        layoutElement.preferredHeight = 62f;
        layoutElement.flexibleWidth = 1f;

        string upgradeName = index < UIManager.StoryUpgradeNames.Length
            ? UIManager.StoryUpgradeNames[index]
            : "Upgrade " + (index + 1);
        string upgradeCost = index < UIManager.StoryUpgradeCosts.Length
            ? "Gold " + UIManager.StoryUpgradeCosts[index]
            : "Gold 100";

        TextMeshProUGUI nameText = FindText(button.transform, "UpgradeName");
        if (nameText == null)
        {
            nameText = CreateChildText(button.transform, "UpgradeName", report);
        }
        ConfigureLabel(nameText, upgradeName, 24f, TextAlignmentOptions.MidlineLeft);

        TextMeshProUGUI costText = FindText(button.transform, "UpgradeCost");
        if (costText == null)
        {
            costText = CreateChildText(button.transform, "UpgradeCost", report);
        }
        ConfigureLabel(costText, upgradeCost, 22f, TextAlignmentOptions.MidlineLeft);
        costText.color = StoryColorPalette.Special;

        return new UpgradeEntry
        {
            Button = button,
            NameText = nameText,
            CostText = costText
        };
    }

    private static TitleCardManager EnsureTitleCard(Transform canvasTransform, StoryUpgradeWiringReport report)
    {
        Transform existingPanel = canvasTransform.Find("TitleCardPanel");
        GameObject titleCardPanel;
        if (existingPanel == null)
        {
            titleCardPanel = new GameObject("TitleCardPanel");
            Undo.RegisterCreatedObjectUndo(titleCardPanel, "Create Title Card Panel");
            titleCardPanel.transform.SetParent(canvasTransform, false);
            titleCardPanel.layer = LayerMask.NameToLayer("UI");

            RectTransform rectTransform = titleCardPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            titleCardPanel.AddComponent<CanvasRenderer>();
            Image image = titleCardPanel.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.72f);
            titleCardPanel.AddComponent<CanvasGroup>();
            titleCardPanel.SetActive(false);
            report.Created.Add("TitleCardPanel");
        }
        else
        {
            titleCardPanel = existingPanel.gameObject;
            GetOrAddComponent<CanvasGroup>(titleCardPanel, report);
            GetOrAddComponent<Image>(titleCardPanel, report);
        }

        TextMeshProUGUI titleText = FindText(titleCardPanel.transform, "TitleText");
        if (titleText == null)
        {
            titleText = CreateChildText(titleCardPanel.transform, "TitleText", report);
            RectTransform rectTransform = titleText.rectTransform;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(920f, 220f);
            report.Created.Add("TitleCardPanel/TitleText");
        }
        ConfigureLabel(titleText, "Chicken Coop – Act 1: Dawn on the Farm", 46f, TextAlignmentOptions.Center);
        titleText.color = Color.white;

        TitleCardManager manager = Object.FindAnyObjectByType<TitleCardManager>();
        if (manager == null)
        {
            GameObject managerObject = new GameObject("TitleCardManager");
            Undo.RegisterCreatedObjectUndo(managerObject, "Create TitleCardManager");
            manager = managerObject.AddComponent<TitleCardManager>();
            report.Created.Add("TitleCardManager");
        }

        manager.titleText = titleText;
        manager.titleCanvasGroup = titleCardPanel.GetComponent<CanvasGroup>();
        manager.titleCardPanel = titleCardPanel;

        return manager;
    }

    private static UIManager EnsureUIManager(StoryUpgradeWiringReport report)
    {
        UIManager uiManager = Object.FindAnyObjectByType<UIManager>();
        if (uiManager != null)
        {
            return uiManager;
        }

        GameObject managerObject = new GameObject("UIManager");
        Undo.RegisterCreatedObjectUndo(managerObject, "Create UIManager");
        uiManager = managerObject.AddComponent<UIManager>();
        report.Created.Add("UIManager");
        return uiManager;
    }

    private static void WireUIManager(
        UIManager uiManager,
        TextMeshProUGUI cornCountText,
        TextMeshProUGUI eggsCountText,
        TextMeshProUGUI coinsCountText,
        TextMeshProUGUI helperCountText,
        TextMeshProUGUI incomeRateText,
        Button harvestButton,
        Button feedButton,
        Button collectButton,
        Button sellButton,
        Button hireHelperButton,
        GameObject upgradePanel,
        Button[] upgradeButtons,
        TextMeshProUGUI[] upgradeCostTexts,
        TextMeshProUGUI[] upgradeNameTexts,
        UpgradeData[] upgrades,
        TextMeshProUGUI nextGoalText,
        StoryUpgradeWiringReport report)
    {
        SerializedObject serializedObject = new SerializedObject(uiManager);
        SetObjectReference(serializedObject, "cornCountText", cornCountText);
        SetObjectReference(serializedObject, "eggsCountText", eggsCountText);
        SetObjectReference(serializedObject, "coinsCountText", coinsCountText);
        SetObjectReference(serializedObject, "helperCountText", helperCountText);
        SetObjectReference(serializedObject, "incomeRateText", incomeRateText);

        SetObjectReference(serializedObject, "harvestButton", harvestButton);
        SetObjectReference(serializedObject, "feedButton", feedButton);
        SetObjectReference(serializedObject, "collectButton", collectButton);
        SetObjectReference(serializedObject, "sellButton", sellButton);
        SetObjectReference(serializedObject, "hireHelperButton", hireHelperButton);

        SetObjectReference(serializedObject, "upgradePanel", upgradePanel);
        SetArrayReferences(serializedObject, "upgradeButtons", upgradeButtons);
        SetArrayReferences(serializedObject, "upgradeCostTexts", upgradeCostTexts);
        SetArrayReferences(serializedObject, "upgradeNameTexts", upgradeNameTexts);
        SetArrayReferences(serializedObject, "availableUpgrades", upgrades);

        SetObjectReference(serializedObject, "nextGoalText", nextGoalText);
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(uiManager);
        report.Wired.Add("UIManager serialized references");
    }

    private static UpgradeData[] FindUpgradeAssets(StoryUpgradeWiringReport report)
    {
        var upgrades = new UpgradeData[UIManager.StoryUpgradeCount];

        for (int i = 0; i < UpgradeAssetNames.Length; i++)
        {
            string preferredPath = "Assets/ScriptableObjects/Upgrades/" + UpgradeAssetNames[i] + ".asset";
            UpgradeData asset = AssetDatabase.LoadAssetAtPath<UpgradeData>(preferredPath);

            if (asset == null)
            {
                string[] matches = AssetDatabase.FindAssets(UpgradeAssetNames[i] + " t:UpgradeData");
                if (matches.Length > 0)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(matches[0]);
                    asset = AssetDatabase.LoadAssetAtPath<UpgradeData>(assetPath);
                }
            }

            upgrades[i] = asset;

            if (report != null && asset == null)
            {
                report.Warnings.Add("Missing upgrade asset: " + UpgradeAssetNames[i] + ". Run 'Create All Upgrade Assets' first.");
            }
        }

        return upgrades;
    }

    private static void SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetArrayReferences(SerializedObject serializedObject, string propertyName, Object[] values)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.arraySize = values != null ? values.Length : 0;
        if (values == null)
        {
            return;
        }

        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static void AppendReferenceStatus(List<string> lines, SerializedObject serializedObject, string propertyName)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        string status = property != null && property.objectReferenceValue != null ? "OK" : "Missing";
        lines.Add(propertyName + ": " + status);
    }

    private static void AppendArrayStatus(List<string> lines, SerializedObject serializedObject, string propertyName, int expectedSize)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null || !property.isArray)
        {
            lines.Add(propertyName + ": Missing array");
            return;
        }

        int assigned = 0;
        for (int i = 0; i < property.arraySize; i++)
        {
            if (property.GetArrayElementAtIndex(i).objectReferenceValue != null)
            {
                assigned++;
            }
        }

        lines.Add(propertyName + ": " + assigned + "/" + expectedSize + " assigned");
    }

    private static string DescribeObject(Object value)
    {
        return value != null ? value.name : "Missing";
    }

    private static TextMeshProUGUI FindText(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
    }

    private static Button FindButton(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        return child != null ? child.GetComponent<Button>() : null;
    }

    private static TextMeshProUGUI CreateChildText(Transform parent, string name, StoryUpgradeWiringReport report)
    {
        GameObject textObject = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(textObject, "Create " + name);
        textObject.transform.SetParent(parent, false);
        textObject.layer = LayerMask.NameToLayer("UI");

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0f, 26f);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        report.Created.Add(parent.name + "/" + name);
        return text;
    }

    private static void ConfigureLabel(TextMeshProUGUI text, string value, float fontSize, TextAlignmentOptions alignment)
    {
        if (text == null)
        {
            return;
        }

        text.text = value;
        text.fontSize = fontSize;
        text.color = StoryColorPalette.TextDark;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
    }

    private static T GetOrAddComponent<T>(GameObject gameObject, StoryUpgradeWiringReport report) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        component = gameObject.AddComponent<T>();
        if (report != null)
        {
            report.Wired.Add("Added " + typeof(T).Name + " to " + gameObject.name);
        }
        return component;
    }

    private sealed class UpgradeEntry
    {
        public Button Button;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI CostText;
    }

    private sealed class StoryUpgradeWiringReport
    {
        public readonly List<string> Created = new List<string>();
        public readonly List<string> Wired = new List<string>();
        public readonly List<string> Warnings = new List<string>();
        public readonly List<string> Missing = new List<string>();

        public string ToSummary()
        {
            var lines = new List<string>();
            lines.Add("Story Upgrade Auto-Wire");
            lines.Add(string.Empty);

            if (Created.Count > 0)
            {
                lines.Add("Created:");
                foreach (string item in Created)
                {
                    lines.Add("- " + item);
                }
            }

            if (Wired.Count > 0)
            {
                if (lines.Count > 0) lines.Add(string.Empty);
                lines.Add("Wired:");
                foreach (string item in Wired)
                {
                    lines.Add("- " + item);
                }
            }

            if (Warnings.Count > 0)
            {
                if (lines.Count > 0) lines.Add(string.Empty);
                lines.Add("Warnings:");
                foreach (string item in Warnings)
                {
                    lines.Add("- " + item);
                }
            }

            if (Missing.Count > 0)
            {
                if (lines.Count > 0) lines.Add(string.Empty);
                lines.Add("Missing:");
                foreach (string item in Missing)
                {
                    lines.Add("- " + item);
                }
            }

            if (Created.Count == 0 && Wired.Count == 0 && Warnings.Count == 0 && Missing.Count == 0)
            {
                lines.Add("No changes were required.");
            }

            return string.Join("\n", lines.ToArray());
        }
    }
}
