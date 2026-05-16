using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ChickenCoop.Managers;

public sealed class VisualProgressionController : MonoBehaviour
{
    public static VisualProgressionController Instance { get; private set; }

    [SerializeField] private UpgradeVisualMutationData mutationData;

    private readonly HashSet<string> appliedMutations = new HashSet<string>();
    private readonly Dictionary<string, ZoneVisualState> zoneStates = new Dictionary<string, ZoneVisualState>();
    private readonly Dictionary<string, ProductVisualState> productStates = new Dictionary<string, ProductVisualState>();
    private readonly Dictionary<int, HelperVisualState> helperStates = new Dictionary<int, HelperVisualState>();
    private StoreVisualState storeState = new StoreVisualState();
    private bool boundToGameManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        EnsureMutationData();
        BindToGameManager();
        RefreshFromGameState();
    }

    private void OnDestroy()
    {
        UnbindFromGameManager();
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void BindToGameManager()
    {
        if (boundToGameManager || GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnUpgradeApplied += HandleUpgradeApplied;
        GameManager.Instance.OnZoneExpanded += HandleZoneExpanded;
        GameManager.Instance.OnHelperCountChanged += HandleHelperCountChanged;
        GameManager.Instance.OnCoinsChanged += HandleCoinsChanged;
        boundToGameManager = true;
    }

    private void UnbindFromGameManager()
    {
        if (!boundToGameManager || GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnUpgradeApplied -= HandleUpgradeApplied;
        GameManager.Instance.OnZoneExpanded -= HandleZoneExpanded;
        GameManager.Instance.OnHelperCountChanged -= HandleHelperCountChanged;
        GameManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
        boundToGameManager = false;
    }

    private void HandleUpgradeApplied(UpgradeType upgradeType, int level)
    {
        RefreshFromGameState();
    }

    private void HandleZoneExpanded(string zoneId, int newCount)
    {
        RefreshFromGameState();
    }

    private void HandleHelperCountChanged(int helperCount)
    {
        RefreshFromGameState();
    }

    private void HandleCoinsChanged(int coins)
    {
        // Keep UI badges/banners in sync if the controller is asked to surface them later.
    }

    public void RefreshFromGameState()
    {
        EnsureMutationData();
        BindToGameManager();

        if (GameManager.Instance == null || mutationData == null)
        {
            return;
        }

        appliedMutations.Clear();

        foreach (UpgradeVisualMutation mutation in mutationData.Mutations)
        {
            if (mutation == null)
            {
                continue;
            }

            int level = GameManager.Instance.GetUpgradeLevel(mutation.upgradeType);
            if (level < mutation.requiredLevel)
            {
                continue;
            }

            ApplyMutation(mutation);
        }

        RefreshEntityStyles();
    }

    public void NotifyUpgradeApplied(UpgradeType upgradeType)
    {
        RefreshFromGameState();
    }

    public void ApplyCurrentStyleToCollectible(CollectibleItem item)
    {
        if (item == null)
        {
            return;
        }

        if (productStates.TryGetValue(item.ItemId, out ProductVisualState state))
        {
            item.ApplyVisualState(state);
            return;
        }

        if (GameManager.Instance != null && string.Equals(item.ItemId, "Egg", StringComparison.OrdinalIgnoreCase) && GameManager.Instance.GetUpgradeLevel(UpgradeType.ChickenCare) > 0)
        {
            item.ApplyVisualTint(new Color(1f, 0.97f, 0.82f), 1.16f, "EggPremiumTier", null, Vector3.up * 0.05f);
        }
    }

    public void ApplyCurrentStyleToHelper(HelperAI helper)
    {
        if (helper == null)
        {
            return;
        }

        helper.ApplyVisualState(GetOrCreateHelperState());
    }

    public void ApplyCurrentStyleToStore(StoreCounter store)
    {
        if (store == null)
        {
            return;
        }

        store.ApplyVisualState(GetOrCreateStoreState());
    }

    public void ApplyCurrentStyleToZone(FarmZoneController zone)
    {
        if (zone == null || zone.template == null)
        {
            return;
        }

        if (zoneStates.TryGetValue(zone.template.id, out ZoneVisualState state))
        {
            zone.ApplyVisualState(state);
        }
    }

    private void ApplyMutation(UpgradeVisualMutation mutation)
    {
        string mutationKey = GetMutationKey(mutation);
        if (appliedMutations.Contains(mutationKey))
        {
            return;
        }

        appliedMutations.Add(mutationKey);

        switch (mutation.kind)
        {
            case VisualMutationKind.ZoneSpawn:
                ApplyZoneSpawnMutation(mutation);
                break;
            case VisualMutationKind.ZoneProp:
                ApplyZonePropMutation(mutation);
                break;
            case VisualMutationKind.ProductStyle:
                ApplyProductMutation(mutation);
                break;
            case VisualMutationKind.HelperStyle:
                ApplyHelperMutation(mutation);
                break;
            case VisualMutationKind.StoreStyle:
                ApplyStoreMutation(mutation);
                break;
            case VisualMutationKind.UiBadge:
                ApplyUiBadgeMutation(mutation);
                break;
        }
    }

    private void ApplyZoneSpawnMutation(UpgradeVisualMutation mutation)
    {
        if (GameManager.Instance == null || string.IsNullOrWhiteSpace(mutation.targetId))
        {
            return;
        }

        GameManager.Instance.EnsureZoneHasVisibleMember(mutation.targetId);
        FarmZoneController zone = FindZone(mutation.targetId);
        if (zone == null)
        {
            return;
        }

        ZoneVisualState state = GetOrCreateZoneState(mutation.targetId);
        state.zoneId = mutation.targetId;
        state.label = mutation.label;
        state.tint = mutation.tint;
        state.localOffset = mutation.localOffset;
        state.localScale = mutation.localScale;
        state.markerName = mutation.markerName;
        state.resourcePath = mutation.resourcePath;
        state.pulseStrength = mutation.pulseStrength;

        zone.ApplyVisualState(state);

        if (mutation.unlockResourceSlot && !string.IsNullOrWhiteSpace(mutation.resourceSlotId))
        {
            UIManager.Instance?.UnlockResourceSlot(mutation.resourceSlotId);
        }
    }

    private void ApplyZonePropMutation(UpgradeVisualMutation mutation)
    {
        if (GameManager.Instance == null || string.IsNullOrWhiteSpace(mutation.targetId))
        {
            return;
        }

        FarmZoneController zone = FindZone(mutation.targetId);
        if (zone == null)
        {
            return;
        }

        ZoneVisualState state = GetOrCreateZoneState(mutation.targetId);
        state.zoneId = mutation.targetId;
        state.label = mutation.label;
        state.tint = mutation.tint;
        state.localOffset = mutation.localOffset;
        state.localScale = mutation.localScale;
        state.markerName = mutation.markerName;
        state.resourcePath = mutation.resourcePath;
        state.pulseStrength = mutation.pulseStrength;

        zone.ApplyVisualState(state);

        foreach (HarvestableField field in zone.GetComponentsInChildren<HarvestableField>(true))
        {
            if (field == null)
            {
                continue;
            }

            if (mutation.upgradeType == UpgradeType.Fertilizer)
            {
                field.ApplyVisualState(1.12f, mutation.tint, mutation.label);
            }
        }

        foreach (Chicken chicken in zone.GetComponentsInChildren<Chicken>(true))
        {
            if (mutation.upgradeType == UpgradeType.ChickenCare)
            {
                chicken.ApplyVisualState(new ChickenVisualState
                {
                    tint = mutation.tint,
                    localScale = new Vector3(1.05f, 1.05f, 1f),
                    nestLabel = mutation.label
                });
            }
        }

        foreach (Pig pig in zone.GetComponentsInChildren<Pig>(true))
        {
            if (mutation.upgradeType == UpgradeType.PigPen)
            {
                pig.ApplyVisualState(new PigVisualState
                {
                    tint = mutation.tint,
                    localScale = new Vector3(1.05f, 1.05f, 1f),
                    mudLabel = mutation.label
                });
            }
        }

        foreach (AnimalProduct product in zone.GetComponentsInChildren<AnimalProduct>(true))
        {
            if (mutation.upgradeType == UpgradeType.CowFeed || mutation.upgradeType == UpgradeType.MilkProduction)
            {
                product.ApplyVisualState(new ProductVisualState
                {
                    itemId = "Milk",
                    tint = mutation.tint,
                    localScale = mutation.localScale,
                    markerName = mutation.markerName,
                    resourcePath = mutation.resourcePath
                });
            }
        }
    }

    private void ApplyProductMutation(UpgradeVisualMutation mutation)
    {
        if (string.IsNullOrWhiteSpace(mutation.targetId))
        {
            return;
        }

        ProductVisualState state = GetOrCreateProductState(mutation.targetId);
        state.itemId = mutation.targetId;
        state.tint = mutation.tint;
        state.localScale = mutation.localScale;
        state.localOffset = mutation.localOffset;
        state.markerName = mutation.markerName;
        state.resourcePath = mutation.resourcePath;
        state.glowStrength = mutation.pulseStrength;

        foreach (CollectibleItem item in FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None))
        {
            if (item == null)
            {
                continue;
            }

            if (string.Equals(item.ItemId, mutation.targetId, StringComparison.OrdinalIgnoreCase))
            {
                item.ApplyVisualState(state);
            }
        }
    }

    private void ApplyHelperMutation(UpgradeVisualMutation mutation)
    {
        HelperVisualState state = GetOrCreateHelperState();
        state.tint = mutation.tint;
        state.localScale = mutation.localScale;
        state.showAura = true;
        state.showStepDust = true;
        state.badgeText = mutation.label;

        foreach (HelperAI helper in FindObjectsByType<HelperAI>(FindObjectsSortMode.None))
        {
            if (helper == null)
            {
                continue;
            }

            helper.ApplyVisualState(state);
        }
    }

    private void ApplyStoreMutation(UpgradeVisualMutation mutation)
    {
        StoreVisualState state = GetOrCreateStoreState();
        state.tier = Mathf.Max(state.tier, 1);
        state.tint = mutation.tint;
        state.localScale = mutation.localScale;
        state.badgeText = mutation.label;

        StoreCounter store = FindFirstObjectByType<StoreCounter>();
        if (store != null)
        {
            store.ApplyVisualState(state);
        }
    }

    private void ApplyUiBadgeMutation(UpgradeVisualMutation mutation)
    {
        if (UIManager.Instance != null)
        {
            if (!string.IsNullOrWhiteSpace(mutation.resourceSlotId))
            {
                UIManager.Instance.UnlockResourceSlot(mutation.resourceSlotId);
            }

            if (!string.IsNullOrWhiteSpace(mutation.label))
            {
                UIManager.Instance.ShowUpgradeNotification(mutation.label);
            }
        }
    }

    private void RefreshEntityStyles()
    {
        foreach (CollectibleItem item in FindObjectsByType<CollectibleItem>(FindObjectsSortMode.None))
        {
            if (item == null)
            {
                continue;
            }

            if (productStates.TryGetValue(item.ItemId, out ProductVisualState state))
            {
                item.ApplyVisualState(state);
            }
        }

        foreach (HelperAI helper in FindObjectsByType<HelperAI>(FindObjectsSortMode.None))
        {
            if (helper == null)
            {
                continue;
            }

            helper.ApplyVisualState(GetOrCreateHelperState());
        }

        StoreCounter store = FindFirstObjectByType<StoreCounter>();
        if (store != null)
        {
            store.ApplyVisualState(GetOrCreateStoreState());
        }
    }

    private void PushUiState()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        UIManager.Instance.UpdateAllResourceText();
    }

    private FarmZoneController FindZone(string zoneId)
    {
        if (GameManager.Instance == null || string.IsNullOrWhiteSpace(zoneId))
        {
            return null;
        }

        return GameManager.Instance.ActiveZoneControllers.FirstOrDefault(zone => zone != null && zone.template != null && string.Equals(zone.template.id, zoneId, StringComparison.OrdinalIgnoreCase));
    }

    private ZoneVisualState GetOrCreateZoneState(string zoneId)
    {
        if (!zoneStates.TryGetValue(zoneId, out ZoneVisualState state))
        {
            state = new ZoneVisualState { zoneId = zoneId };
            zoneStates[zoneId] = state;
        }
        return state;
    }

    private ProductVisualState GetOrCreateProductState(string itemId)
    {
        if (!productStates.TryGetValue(itemId, out ProductVisualState state))
        {
            state = new ProductVisualState { itemId = itemId };
            productStates[itemId] = state;
        }
        return state;
    }

    private HelperVisualState GetOrCreateHelperState()
    {
        if (!helperStates.TryGetValue(0, out HelperVisualState state))
        {
            state = new HelperVisualState();
            helperStates[0] = state;
        }
        return state;
    }

    private StoreVisualState GetOrCreateStoreState()
    {
        if (storeState == null)
        {
            storeState = new StoreVisualState();
        }
        return storeState;
    }

    private void EnsureMutationData()
    {
        if (mutationData != null)
        {
            return;
        }

        mutationData = UpgradeVisualMutationData.CreateDefault();
    }

    private string GetMutationKey(UpgradeVisualMutation mutation)
    {
        return $"{mutation.upgradeType}:{mutation.requiredLevel}:{mutation.kind}:{mutation.targetId}:{mutation.markerName}:{mutation.resourcePath}";
    }
}
