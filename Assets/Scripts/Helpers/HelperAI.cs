using UnityEngine;
using ChickenCoop.Managers;
using ChickenCoop.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Automated helper character with prioritized task selection:
/// 1. Feed hungry animals (HIGHEST - always first)
/// 2. Harvest ready crops
/// 3. Collect ground items
/// 4. Sell at market
/// Uses weighted random selection with helper avoidance to spread work.
/// </summary>
public class HelperAI : MonoBehaviour
{
    public enum HelperState { Idle, Moving, Harvesting, Feeding, Selling, Waiting }
    private enum WorkStream { Feed, Harvest, Collect, Sell }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float waitTime = 1f;

    [Header("Animation Settings")]
    [SerializeField] private float bobAmount = 0.15f;
    [SerializeField] private float bobSpeed = 10f;
    [SerializeField] private Color helperColor = new Color(0.9f, 0.8f, 0.6f);

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool isMoving = false;
    private float bobTimer = 0f;
    private Vector3 originalScale;
    private Vector3 baseScale;
    private Transform happyHarvestVisualRoot;
    private HelperVisualState visualState = new HelperVisualState();
    private float lastStepDustTime = -999f;
    private static Sprite fallbackCircleSprite;

    private int helperId;
    private WorkStream assignedWorkStream;
    private static int helperCounter = 0;

    private void Start()
    {
        helperId = helperCounter++;
        assignedWorkStream = (WorkStream)(helperId % 4);
        baseScale = transform.localScale;
        originalScale = baseScale;

        GameConfig config = GameManager.Instance != null ? GameManager.Instance.Config : null;
        if (config != null)
        {
            moveSpeed = config.helperSpeed;
            waitTime = config.helperWaitTime;
        }

        spriteRenderer = spriteRenderer ?? GetComponent<SpriteRenderer>();
        happyHarvestVisualRoot = StoryVisualBinder.FindAttachedVisualRoot(transform);

        if (spriteRenderer != null)
        {
            float hueOffset = (helperId * 0.15f) % 1f;
            Color.RGBToHSV(helperColor, out float h, out float s, out float v);
            spriteRenderer.color = Color.HSVToRGB((h + hueOffset) % 1f, s, v);
        }

        VisualProgressionController.Instance?.ApplyCurrentStyleToHelper(this);
        StartCoroutine(StartHelperLoop());
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private IEnumerator StartHelperLoop()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 2f));

        while (true)
        {
            yield return StartCoroutine(PerformNextTask());
            float speedMultiplier = GameManager.Instance != null ? GameManager.Instance.SpeedMultiplier : 1f;
            float wait = waitTime / Mathf.Max(speedMultiplier, 0.01f);
            yield return new WaitForSeconds(wait);
        }
    }

    private IEnumerator PerformNextTask()
    {
        if (TryGetTaskRoutine(assignedWorkStream, out IEnumerator routine))
        {
            yield return StartCoroutine(routine);
            yield break;
        }

        foreach (WorkStream stream in new[] { WorkStream.Feed, WorkStream.Harvest, WorkStream.Collect, WorkStream.Sell })
        {
            if (stream == assignedWorkStream)
            {
                continue;
            }

            if (TryGetTaskRoutine(stream, out routine))
            {
                yield return StartCoroutine(routine);
                yield break;
            }
        }

        yield return new WaitForSeconds(1.0f);
    }

    private bool TryGetTaskRoutine(WorkStream stream, out IEnumerator routine)
    {
        routine = null;

        switch (stream)
        {
            case WorkStream.Feed:
                if (TryBuildFeedRoutine(out routine)) return true;
                break;
            case WorkStream.Harvest:
                if (TryBuildHarvestRoutine(out routine)) return true;
                break;
            case WorkStream.Collect:
                if (TryBuildCollectRoutine(out routine)) return true;
                break;
            case WorkStream.Sell:
                if (TryBuildSellRoutine(out routine)) return true;
                break;
        }

        return false;
    }

    private bool TryBuildFeedRoutine(out IEnumerator routine)
    {
        routine = null;
        var feedable = FindRandomFeedable();
        if (feedable == null) return false;

        MonoBehaviour mono = feedable as MonoBehaviour;
        if (mono == null) return false;

        string foodId = ResolveFoodFor(feedable);
        if (string.IsNullOrEmpty(foodId)) return false;
        if (!HasFoodFor(foodId)) return false;
        if (!feedable.CanAcceptFood(foodId)) return false;

        routine = MoveAndAction(mono.transform.position, "FEED", () => feedable.Feed(foodId));
        return true;
    }

    private bool TryBuildHarvestRoutine(out IEnumerator routine)
    {
        routine = null;
        var harvestable = FindRandomHarvestable();
        if (harvestable == null) return false;

        MonoBehaviour mono = harvestable as MonoBehaviour;
        if (mono == null) return false;

        routine = MoveAndAction(mono.transform.position, "WORK", harvestable.Harvest);
        return true;
    }

    private bool TryBuildCollectRoutine(out IEnumerator routine)
    {
        routine = null;
        var collectible = FindRandomCollectible<CollectibleItem>();
        if (collectible == null) return false;

        routine = MoveAndAction(collectible.transform.position, "ITEM", collectible.Interact);
        return true;
    }

    private bool TryBuildSellRoutine(out IEnumerator routine)
    {
        routine = null;
        if (GameManager.Instance == null || GameManager.Instance.StorePosition == null)
        {
            return false;
        }

        if (GameManager.Instance.Eggs <= 0 &&
            GameManager.Instance.Corn <= 0 &&
            GameManager.Instance.GetItemCount("Wheat") <= 0 &&
            GameManager.Instance.GetItemCount("Milk") <= 0 &&
            GameManager.Instance.GetItemCount("Carrot") <= 0 &&
            GameManager.Instance.GetItemCount("Truffle") <= 0)
        {
            return false;
        }

        routine = MoveAndAction(GameManager.Instance.StorePosition.position, "GOLD", () =>
        {
            StoreCounter store = FindFirstObjectByType<StoreCounter>();
            if (store != null) store.SellEgg();
            else GameManager.Instance.SellEgg(transform.position);
        });
        return true;
    }

    private bool HasFoodFor(string foodId)
    {
        if (GameManager.Instance == null) return false;
        return GameManager.Instance.GetItemCount(foodId) > 0 || (foodId == "Corn" && GameManager.Instance.Corn > 0);
    }

    private string ResolveFoodFor(IFeedable feedable)
    {
        string[] knownFoods = { "Corn", "Carrot", "Wheat", "Milk" };
        foreach (string food in knownFoods)
        {
            if (HasFoodFor(food) && feedable.CanAcceptFood(food))
            {
                return food;
            }
        }

        return feedable.CanAcceptFood("Generic") ? "Generic" : string.Empty;
    }

    private IEnumerator MoveAndAction(Vector3 pos, string bubbleText, System.Action action)
    {
        SpawnTaskBubble(bubbleText);
        yield return StartCoroutine(MoveTo(pos));
        PlaySquashStretch();
        float actionSpeedMultiplier = GameManager.Instance != null ? GameManager.Instance.SpeedMultiplier : 1f;
        yield return new WaitForSeconds(0.4f / Mathf.Max(actionSpeedMultiplier, 0.01f));
        action?.Invoke();
        yield return new WaitForSeconds(0.2f / Mathf.Max(actionSpeedMultiplier, 0.01f));
    }

    private IHarvestable FindRandomHarvestable()
    {
        var harvestables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IHarvestable>()
            .Where(h => h.IsReadyToHarvest())
            .ToList();
        
        if (harvestables.Count == 0) return null;
        
        // Score by distance AND helper avoidance, pick weighted random
        var scored = harvestables.Select(h => {
            MonoBehaviour mono = (MonoBehaviour)h;
            float dist = Vector3.Distance(transform.position, mono.transform.position);
            float helperPenalty = GetMinDistanceToOtherHelpers(mono.transform.position) * 0.5f;
            float score = dist + helperPenalty + Random.Range(-2f, 2f); // Add randomness
            return (item: h, score: score);
        }).OrderBy(x => x.score).ToList();
        
        return scored.First().item;
    }

    private IFeedable FindRandomFeedable()
    {
        // Prioritize animals that NEED feeding (not just can interact)
        var feedables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IFeedable>()
            .Where(f => f.NeedsFeeding()) // Only hungry animals
            .ToList();
        
        if (feedables.Count == 0) return null;
        
        // Score by distance AND helper avoidance, pick weighted random
        var scored = feedables.Select(f => {
            MonoBehaviour mono = (MonoBehaviour)f;
            float dist = Vector3.Distance(transform.position, mono.transform.position);
            float helperPenalty = GetMinDistanceToOtherHelpers(mono.transform.position) * 0.5f;
            float score = dist + helperPenalty + Random.Range(-2f, 2f); // Add randomness
            return (item: f, score: score);
        }).OrderBy(x => x.score).ToList();
        
        return scored.First().item;
    }

    private T FindRandomCollectible<T>() where T : MonoBehaviour, IInteractable
    {
        var collectibles = FindObjectsByType<T>(FindObjectsSortMode.None)
            .Where(i => i.CanInteract())
            .ToList();
        
        if (collectibles.Count == 0) return null;
        
        // Score by distance AND helper avoidance, pick weighted random
        var scored = collectibles.Select(i => {
            float dist = Vector3.Distance(transform.position, i.transform.position);
            float helperPenalty = GetMinDistanceToOtherHelpers(i.transform.position) * 0.5f;
            float score = dist + helperPenalty + Random.Range(-1f, 1f);
            return (item: i, score: score);
        }).OrderBy(x => x.score).ToList();
        
        return scored.First().item;
    }

    private float GetMinDistanceToOtherHelpers(Vector3 position)
    {
        float minDist = float.MaxValue;
        var otherHelpers = FindObjectsByType<HelperAI>(FindObjectsSortMode.None).Where(h => h != this);
        foreach (var helper in otherHelpers)
        {
            float dist = Vector3.Distance(position, helper.transform.position);
            if (dist < minDist) minDist = dist;
        }
        return minDist;
    }

    private IEnumerator MoveTo(Vector3 position)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        position.z = startPos.z;

        if (spriteRenderer != null)
        {
            bool faceLeft = position.x < startPos.x;
            spriteRenderer.flipX = faceLeft;
            StoryVisualBinder.SetFacing(happyHarvestVisualRoot, faceLeft);
        }

        float distance = Vector3.Distance(startPos, position);
        float speedMultiplierMove = GameManager.Instance != null ? GameManager.Instance.SpeedMultiplier : 1f;
        float duration = distance / Mathf.Max(moveSpeed * speedMultiplierMove, 0.1f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, position, elapsed / duration);
            if (visualState.showStepDust && Time.time - lastStepDustTime > 0.14f)
            {
                lastStepDustTime = Time.time;
                SpawnStepDust(transform.position - new Vector3(0f, 0.4f, 0f));
            }
            yield return null;
        }

        transform.position = position;
        isMoving = false;
    }

    private void UpdateAnimation()
    {
        bobTimer += Time.deltaTime * bobSpeed;
        float bob = (isMoving ? Mathf.Abs(Mathf.Sin(bobTimer * 2f)) : Mathf.Sin(bobTimer) * 0.3f) * bobAmount;
        transform.localScale = (originalScale * Mathf.Max(0.9f, visualState.localScale.x)) + new Vector3(0, bob, 0);
    }

    public void ApplyVisualState(HelperVisualState state)
    {
        if (state == null)
        {
            return;
        }

        visualState = state;
        Vector3 resolvedBase = baseScale == Vector3.zero ? transform.localScale : baseScale;
        baseScale = resolvedBase;
        originalScale = resolvedBase * Mathf.Max(0.9f, state.localScale.x);
        transform.localScale = originalScale;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(helperColor, state.tint, 0.65f);
        }

        string markerName = string.IsNullOrWhiteSpace(state.auraMarkerName) ? "HelperAura" : state.auraMarkerName;
        Transform aura = transform.Find(markerName);
        if (state.showAura)
        {
            if (aura == null)
            {
                GameObject auraObject = new GameObject(markerName);
                auraObject.transform.SetParent(transform, false);
                aura = auraObject.transform;
            }

            aura.localPosition = new Vector3(0f, 0.1f, 0f);
            aura.localScale = Vector3.one * 1.1f;

            SpriteRenderer auraRenderer = aura.GetComponent<SpriteRenderer>();
            if (auraRenderer == null)
            {
                auraRenderer = aura.gameObject.AddComponent<SpriteRenderer>();
            }

            Sprite auraSprite = Resources.Load<Sprite>("Sprite_Circle");
            if (auraSprite == null)
            {
                auraSprite = CreateFallbackCircleSprite();
            }
            if (auraSprite != null)
            {
                auraRenderer.sprite = auraSprite;
            }
            auraRenderer.color = new Color(state.tint.r, state.tint.g, state.tint.b, 0.25f);
            auraRenderer.sortingOrder = 5;
        }
        else if (aura != null)
        {
            aura.gameObject.SetActive(false);
        }

        if (!string.IsNullOrWhiteSpace(state.badgeText))
        {
            Transform badge = transform.Find("HelperBadge");
            if (badge == null)
            {
                GameObject badgeObject = new GameObject("HelperBadge");
                badgeObject.transform.SetParent(transform, false);
                badge = badgeObject.transform;
            }

            badge.localPosition = new Vector3(0f, 1.0f, 0f);
            badge.localScale = Vector3.one * 0.35f;
            TextMesh tm = badge.GetComponent<TextMesh>();
            if (tm == null)
            {
                tm = badge.gameObject.AddComponent<TextMesh>();
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                tm.fontSize = 18;
            }

            tm.text = state.badgeText;
            tm.color = state.tint;
            badge.GetComponent<MeshRenderer>().sortingOrder = 20;
        }
    }

    private void SpawnStepDust(Vector3 position)
    {
        GameObject dust = new GameObject("HelperStepDust");
        dust.transform.position = position;
        ParticleSystem ps = dust.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.08f;
        main.startLifetime = 0.35f;
        main.startColor = Color.Lerp(helperColor, Color.white, 0.5f);
        main.startSpeed = 0.45f;
        main.gravityModifier = -0.05f;
        main.maxParticles = 4;
        main.duration = 0.08f;
        main.loop = false;
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 4) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.12f;
        ps.Play();
        Destroy(dust, 0.8f);
    }

    private static Sprite CreateFallbackCircleSprite()
    {
        if (fallbackCircleSprite != null)
        {
            return fallbackCircleSprite;
        }

        Texture2D texture = new Texture2D(24, 24, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(11.5f, 11.5f);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float dx = (x - center.x) / 9f;
                float dy = (y - center.y) / 9f;
                texture.SetPixel(x, y, dx * dx + dy * dy <= 1f ? Color.white : Color.clear);
            }
        }
        texture.Apply();
        fallbackCircleSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 24f);
        return fallbackCircleSprite;
    }

    private void PlaySquashStretch() => StartCoroutine(SquashStretchAnimation());

    private IEnumerator SquashStretchAnimation()
    {
        Vector3 original = originalScale;
        Vector3 squash = new Vector3(original.x * 1.3f, original.y * 0.7f, original.z);
        Vector3 stretch = new Vector3(original.x * 0.85f, original.y * 1.15f, original.z);

        float t = 0;
        while (t < 0.1f) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(original, squash, t / 0.1f); yield return null; }
        t = 0;
        while (t < 0.1f) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(squash, stretch, t / 0.1f); yield return null; }
        t = 0;
        while (t < 0.1f) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(stretch, original, t / 0.1f); yield return null; }
        transform.localScale = original;
    }

    private void SpawnTaskBubble(string icon)
    {
        GameObject bubble = new GameObject("TaskBubble");
        bubble.transform.position = transform.position + Vector3.up * 1.5f;
        var tm = bubble.AddComponent<TextMesh>();
        tm.text = icon;
        tm.fontSize = 32;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.characterSize = 0.1f;
        Destroy(bubble, 1.0f);
    }
}
