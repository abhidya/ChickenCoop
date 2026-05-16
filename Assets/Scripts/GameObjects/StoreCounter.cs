using UnityEngine;
using ChickenCoop.Managers;
using System.Collections;

/// <summary>
/// StoreCounter - Location where eggs can be sold for coins.
/// Triggers coin burst particles and satisfying sound effects.
/// Implements IInteractable for player interaction.
/// </summary>
public class StoreCounter : MonoBehaviour, IInteractable
{
    private const string RuntimeStoreVisualResourcePath = "HappyHarvestMarket";

    [Header("Store Settings")]
    [SerializeField] private float sellCooldown = 0.5f;

    [Header("Animation Settings")]
    [SerializeField] private float bounceAmount = 0.1f;
    [SerializeField] private Color activeColor = new Color(0.9f, 0.8f, 0.5f);
    [SerializeField] private Color inactiveColor = new Color(0.7f, 0.6f, 0.4f);

    [Header("Visual References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform coinSpawnPoint;
    [SerializeField] private GameObject coinParticlePrefab;
    [SerializeField] private GameObject storyVisualPrefab;

    // State
    private bool canSell = true;
    private Vector3 _originalScale;
    public Vector3 originalScale
    {
        get => _originalScale;
        set => _originalScale = value;
    }
    private Vector3 baseScale;
    private float bounceTimer = 0f;
    private SpriteRenderer[] storyRenderers;

    private void Start()
    {
        // Enforce the desired small scale for the market
        if (transform.localScale.x > 0.06f || transform.localScale.x < 0.04f) 
        {
            transform.localScale = new Vector3(0.05f, 0.05f, 1f);
        }
        
        // Only set _originalScale if it wasn't already set by GameManager or another initializer
        if (_originalScale == Vector3.zero)
        {
            _originalScale = transform.localScale;
        }
        baseScale = _originalScale;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        GameObject resolvedPrefab = storyVisualPrefab;
        if (resolvedPrefab == null)
        {
            resolvedPrefab = Resources.Load<GameObject>(RuntimeStoreVisualResourcePath);
        }

        if (resolvedPrefab != null)
        {
            GameObject visual = StoryVisualBinder.AttachVisualPrefab(transform, resolvedPrefab, spriteRenderer, true);
            if (visual != null)
            {
                storyRenderers = visual.GetComponentsInChildren<SpriteRenderer>(true);
                // Keep scale at 1.0 to respect root scale (0.05)
                visual.transform.localScale = Vector3.one;
                // Center the visual
                visual.transform.localPosition = Vector3.zero;
            }
        }

        // Enforce SortingGroup so characters consistently draw in front/behind based on Y-position
        var sGroup = gameObject.GetComponent<UnityEngine.Rendering.SortingGroup>();
        if (sGroup == null) sGroup = gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
        sGroup.sortingOrder = 0; // Standard world sorting

        UpdateVisual();
        VisualProgressionController.Instance?.ApplyCurrentStyleToStore(this);
    }

    private void Update()
    {
        UpdateAnimation();
        UpdateVisual();
    }

    /// <summary>
    /// Update idle animation
    /// </summary>
    private void UpdateAnimation()
    {
        bounceTimer += Time.deltaTime * 2f;

        // Gentle idle bounce
        float bounce = Mathf.Sin(bounceTimer) * bounceAmount * 0.3f;
        transform.localScale = originalScale + new Vector3(bounce, bounce, 0);
    }

    /// <summary>
    /// Update store visual based on whether player has eggs
    /// </summary>
    private void UpdateVisual()
    {
        if (spriteRenderer != null)
        {
            bool hasItems = GameManager.Instance != null && (GameManager.Instance.Eggs > 0 || 
                          GameManager.Instance.Corn > 0 || 
                          GameManager.Instance.GetItemCount("Wheat") > 0 || 
                          GameManager.Instance.GetItemCount("Milk") > 0 ||
                          GameManager.Instance.GetItemCount("Carrot") > 0 ||
                          GameManager.Instance.GetItemCount("Truffle") > 0);
            spriteRenderer.color = hasItems ? activeColor : inactiveColor;
        }

        bool itemsToSell = GameManager.Instance != null && (GameManager.Instance.Eggs > 0 || 
                          GameManager.Instance.Corn > 0 || 
                          GameManager.Instance.GetItemCount("Wheat") > 0 || 
                          GameManager.Instance.GetItemCount("Milk") > 0 ||
                          GameManager.Instance.GetItemCount("Carrot") > 0 ||
                          GameManager.Instance.GetItemCount("Truffle") > 0);
        ApplyStoryTint(itemsToSell ? activeColor : inactiveColor);
    }

    /// <summary>
    /// Player interaction - sell an egg
    /// </summary>
    public void Interact()
    {
        if (CanInteract())
        {
            SellAllEggs();
        }
    }

    /// <summary>
    /// Check if player can sell
    /// </summary>
    public bool CanInteract()
    {
        if (GameManager.Instance == null || !canSell) return false;
        return GameManager.Instance.Eggs > 0 || 
               GameManager.Instance.Corn > 0 || 
               GameManager.Instance.GetItemCount("Wheat") > 0 || 
               GameManager.Instance.GetItemCount("Milk") > 0 ||
               GameManager.Instance.GetItemCount("Carrot") > 0 ||
               GameManager.Instance.GetItemCount("Truffle") > 0;
    }

    /// <summary>
    /// Sell all items currently held by the player
    /// </summary>
    public void SellAllEggs()
    {
        if (!canSell || !CanInteract()) return;

        canSell = false;
        StartCoroutine(SellEverythingCoroutine());
    }

    private IEnumerator SellEverythingCoroutine()
    {
        Vector3 salePosition = coinSpawnPoint != null ? coinSpawnPoint.position : transform.position + new Vector3(0, 0.5f, 0);
        float burstInterval = 0.05f;

        // Sell Eggs
        int eggCount = GameManager.Instance.Eggs;
        for (int i = 0; i < eggCount; i++)
        {
            if (GameManager.Instance.SellEgg(salePosition))
            {
                if (i % 2 == 0) StartCoroutine(SaleAnimation());
                SpawnCoinBurst();
                yield return new WaitForSeconds(burstInterval);
            }
        }

        // Sell Corn (if player wants to sell it at market)
        int cornCount = GameManager.Instance.Corn;
        for (int i = 0; i < cornCount; i++)
        {
            if (GameManager.Instance.SpendCorn(1)) 
            {
                GameManager.Instance.AddCoins(2, salePosition); // Corn price
                SpawnCoinBurst();
                yield return new WaitForSeconds(burstInterval);
            }
        }

        // Sell Wheat
        int wheatCount = GameManager.Instance.GetItemCount("Wheat");
        for (int i = 0; i < wheatCount; i++)
        {
            GameManager.Instance.RemoveItem("Wheat", 1);
            GameManager.Instance.AddCoins(10, salePosition); // Wheat price
            SpawnCoinBurst();
            yield return new WaitForSeconds(burstInterval);
        }

        // Sell Milk
        int milkCount = GameManager.Instance.GetItemCount("Milk");
        for (int i = 0; i < milkCount; i++)
        {
            GameManager.Instance.RemoveItem("Milk", 1);
            GameManager.Instance.AddCoins(50, salePosition);
            SpawnCoinBurst();
            yield return new WaitForSeconds(burstInterval);
        }

        // Sell Carrot
        int carrotCount = GameManager.Instance.GetItemCount("Carrot");
        for (int i = 0; i < carrotCount; i++)
        {
            GameManager.Instance.RemoveItem("Carrot", 1);
            GameManager.Instance.AddCoins(15, salePosition);
            SpawnCoinBurst();
            yield return new WaitForSeconds(burstInterval);
        }

        // Sell Truffle
        int truffleCount = GameManager.Instance.GetItemCount("Truffle");
        for (int i = 0; i < truffleCount; i++)
        {
            GameManager.Instance.RemoveItem("Truffle", 1);
            GameManager.Instance.AddCoins(80, salePosition);
            SpawnCoinBurst();
            yield return new WaitForSeconds(burstInterval);
        }

        StartCoroutine(SellCooldown());
    }

    /// <summary>
    /// Sell an egg at the store (legacy support)
    /// </summary>
    public void SellEgg()
    {
        if (!canSell || GameManager.Instance.Eggs <= 0) return;

        canSell = false;
        
        // Perform sale
        Vector3 salePosition = coinSpawnPoint != null ? coinSpawnPoint.position : transform.position + new Vector3(0, 0.5f, 0);
        if (GameManager.Instance.SellEgg(salePosition))
        {
            // Play sale animation
            StartCoroutine(SaleAnimation());

            // Spawn coin burst
            SpawnCoinBurst();
        }

        // Cooldown
        StartCoroutine(SellCooldown());
    }

    /// <summary>
    /// Sale celebration animation
    /// </summary>
    private IEnumerator SaleAnimation()
    {
        // Pop scale
        Vector3 pop = originalScale * 1.3f;
        transform.localScale = pop;

        float t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            float bounce = 1f + Mathf.Sin(t * 30f) * 0.1f * (1f - t / 0.2f);
            transform.localScale = Vector3.Lerp(pop, originalScale, t / 0.2f) * bounce;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    /// <summary>
    /// Sell cooldown coroutine
    /// </summary>
    private IEnumerator SellCooldown()
    {
        float speedMultiplier = GameManager.Instance != null ? GameManager.Instance.SpeedMultiplier : 1f;
        float storeEfficiency = GameManager.Instance != null ? GameManager.Instance.StoreEfficiencyMultiplier : 1f;
        float sellRate = Mathf.Max(speedMultiplier * storeEfficiency, 0.01f);
        yield return new WaitForSeconds(sellCooldown / sellRate);
        canSell = true;
    }

    /// <summary>
    /// Spawn coin burst particle effect
    /// </summary>
    private void SpawnCoinBurst()
    {
        Vector3 spawnPos = coinSpawnPoint != null ? coinSpawnPoint.position : transform.position + new Vector3(0, 0.5f, 0);

        if (coinParticlePrefab != null)
        {
            Instantiate(coinParticlePrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // Create particle effect programmatically
            CreateCoinParticles(spawnPos);
        }
    }

    /// <summary>
    /// Create coin particle effect when no prefab is assigned
    /// </summary>
    private void CreateCoinParticles(Vector3 position)
    {
        GameObject particles = new GameObject("CoinBurst");
        particles.transform.position = position;

        ParticleSystem ps = particles.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startSize = 0.2f;
        main.startLifetime = 0.8f;
        main.startColor = new Color(1f, 0.85f, 0.2f, 1f); // Gold coin color
        main.startSpeed = 3f;
        main.gravityModifier = 1f;
        main.maxParticles = 15;
        main.duration = 0.1f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 45f;
        shape.radius = 0.1f;

        // Add size over lifetime for fade out
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        ps.Play();

        AudioManager.Instance?.PlaySound("coin");

        DestroyTemporaryObject(particles, 1.5f);
    }

    /// <summary>
    /// Upgrade store for better prices
    /// </summary>
    public void UpgradeStore()
    {
        GameManager.Instance.ApplyUpgrade(UpgradeType.BiggerStore, 1.2f);

        // Visual feedback
        StartCoroutine(UpgradeAnimation());
    }

    private IEnumerator UpgradeAnimation()
    {
        // Flash gold
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.yellow;

            yield return new WaitForSeconds(0.1f);

            spriteRenderer.color = originalColor;
        }

        // Scale pop
        Vector3 pop = originalScale * 1.4f;
        transform.localScale = pop;

        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(pop, originalScale, t / 0.3f);
            yield return null;
        }

        transform.localScale = originalScale;

        // Spawn sparkles
        SpawnUpgradeSparkles();
    }

    private void SpawnUpgradeSparkles()
    {
        GameObject sparkles = new GameObject("UpgradeSparkles");
        sparkles.transform.position = transform.position;

        ParticleSystem ps = sparkles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.15f;
        main.startLifetime = 0.6f;
        main.startColor = new Color(1f, 1f, 0.5f, 1f);
        main.startSpeed = 1.5f;
        main.gravityModifier = -0.3f;
        main.maxParticles = 20;
        main.duration = 0.2f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;

        ps.Play();
        DestroyTemporaryObject(sparkles, 1f);
    }

    private void ApplyStoryTint(Color tint)
    {
        if (storyRenderers == null)
        {
            return;
        }

        foreach (SpriteRenderer renderer in storyRenderers)
        {
            if (renderer != null)
            {
                renderer.color = tint;
            }
        }
    }

    public void ApplyVisualState(StoreVisualState state)
    {
        if (state == null)
        {
            return;
        }

        float scale = Mathf.Max(0.9f, state.localScale.x);
        Vector3 baseScaleLocal = baseScale == Vector3.zero ? transform.localScale : baseScale;
        originalScale = baseScaleLocal * scale;
        transform.localScale = originalScale;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(activeColor, state.tint, 0.5f);
        }

        ApplyStoryTint(Color.Lerp(activeColor, state.tint, 0.65f));

        string markerName = string.IsNullOrWhiteSpace(state.markerName) ? "StoreProgression" : state.markerName;
        Transform marker = transform.Find(markerName);
        if (marker == null)
        {
            GameObject markerObject = new GameObject(markerName);
            markerObject.transform.SetParent(transform, false);
            marker = markerObject.transform;
        }

        marker.localPosition = new Vector3(0f, 1.05f, 0f);
        marker.localScale = Vector3.one * 0.55f;

        SpriteRenderer markerRenderer = marker.GetComponent<SpriteRenderer>();
        if (markerRenderer == null)
        {
            markerRenderer = marker.gameObject.AddComponent<SpriteRenderer>();
        }

        Sprite sprite = Resources.Load<Sprite>("Sprite_coin_icon");
        if (sprite != null)
        {
            markerRenderer.sprite = sprite;
        }

        markerRenderer.color = state.tint;
        markerRenderer.sortingOrder = 12;

        Transform label = marker.Find("Label");
        if (label == null)
        {
            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(marker, false);
            label = labelObject.transform;
        }

        label.localPosition = new Vector3(0f, 0.42f, 0f);
        label.localScale = Vector3.one * 0.35f;

        TextMesh textMesh = label.GetComponent<TextMesh>();
        if (textMesh == null)
        {
            textMesh = label.gameObject.AddComponent<TextMesh>();
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 18;
        }

        textMesh.text = string.IsNullOrWhiteSpace(state.badgeText) ? "STORE" : state.badgeText;
        textMesh.color = Color.white;
        label.GetComponent<MeshRenderer>().sortingOrder = 13;
    }

    private static void DestroyTemporaryObject(Object target, float delay = 0f)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target, delay);
            return;
        }

        Object.DestroyImmediate(target);
    }
}
