using UnityEngine;
using System.Collections;
using ChickenCoop.Managers;
using ChickenCoop.Interfaces;

/// <summary>
/// Chicken - Represents a chicken that can be fed corn and produces eggs.
/// Includes cute animations: blinking, wiggling, eating, and egg laying.
/// Implements IInteractable, IFeedable, and IZoneMember interfaces.
/// </summary>
public class Chicken : MonoBehaviour, IInteractable, IFeedable, IZoneMember
{
    private const string RuntimeChickenVisualResourcePath = "HappyHarvestChicken";
    private static Sprite runtimeEggSprite;

    [Header("Production Settings")]
    [SerializeField] private int cornRequired = 1;

    [Header("Animation Settings")]
    [SerializeField] private float blinkInterval = 3f;
    [SerializeField] private float blinkDuration = 0.1f;
    [SerializeField] private float wiggleAmount = 5f;
    [SerializeField] private float wiggleSpeed = 3f;
    [SerializeField] private float bobAmount = 0.05f;

    [Header("Visual References")]
    [SerializeField] private SpriteRenderer bodySprite;
    [SerializeField] private SpriteRenderer eyeSprite;
    [SerializeField] private SpriteRenderer progressBarRenderer;
    [SerializeField] private Transform eggSpawnPoint;
    [SerializeField] private GameObject eggPrefab;
    [SerializeField] private GameObject storyVisualPrefab;
    [SerializeField] private float wanderInterval = 5f;
    [SerializeField] private float wanderSpeed = 1.25f;

    // State
    private bool isWandering = false;
    private bool isLayingEgg = false;
    private float blinkTimer = 0f;
    private float wiggleTimer = 0f;
    private Vector3 originalScale;
    private Vector3 baseScale;
    private Quaternion originalRotation;
    private Transform visualRoot;
    private Vector3 visualOriginalScale;
    private Quaternion visualOriginalRotation;
    private float productionProgress = 0f;
    private Transform progressBarPivot;
    private FarmZoneController zoneController;
    private ChickenVisualState visualState = new ChickenVisualState();

    public bool IsLayingEgg => isLayingEgg;

    private void Start()
    {
        originalScale = transform.localScale;
        baseScale = originalScale;
        originalRotation = transform.localRotation;

        GameConfig config = GameManager.Instance != null ? GameManager.Instance.Config : null;
        if (config != null)
        {
            cornRequired = config.cornToFeed;
        }

        zoneController = GetComponentInParent<FarmZoneController>();

        if (bodySprite == null)
        {
            bodySprite = GetComponent<SpriteRenderer>();
        }

        EnsureProgressBar();

        CircleCollider2D hitbox = GetComponent<CircleCollider2D>();
        if (hitbox == null)
        {
            hitbox = gameObject.AddComponent<CircleCollider2D>();
        }

        hitbox.isTrigger = true;
        if (hitbox.radius < 0.9f)
        {
            hitbox.radius = 0.9f;
        }

        GameObject resolvedPrefab = storyVisualPrefab;
        if (resolvedPrefab == null)
        {
            resolvedPrefab = Resources.Load<GameObject>(RuntimeChickenVisualResourcePath);
        }

        if (resolvedPrefab != null)
        {
            // NEW: Check if GameManager's SpawnChickenAt already attached a visual as a child
            Transform existingVisual = transform.Find("Visual");
            if (existingVisual != null)
            {
                if (bodySprite != null) bodySprite.enabled = false;
                visualRoot = existingVisual;
            }
            else
            {
                StoryVisualBinder.AttachVisualPrefab(transform, resolvedPrefab, bodySprite);
                visualRoot = StoryVisualBinder.FindAttachedVisualRoot(transform);
            }
        }

        EnsureVisualComponents();
        CacheMotionVisualRoot();

        // Randomize initial blink timer
        blinkTimer = Random.Range(0f, blinkInterval);

        StartCoroutine(WanderLoop());
    }

    private void EnsureVisualComponents()
    {
        // Progress Bar (World Space)
        if (progressBarRenderer == null)
        {
            GameObject barContainer = new GameObject("ProgressBar");
            barContainer.transform.SetParent(transform);
            barContainer.transform.localPosition = new Vector3(0, 0.5f, 0); // Above chicken
            
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(barContainer.transform);
            bg.transform.localPosition = Vector3.zero;
            bg.transform.localScale = new Vector3(0.08f, 0.015f, 1f);
            var bgSr = bg.AddComponent<SpriteRenderer>();
            bgSr.color = new Color(0, 0, 0, 0.5f);
            bgSr.sortingOrder = 30;

            GameObject pivot = new GameObject("Pivot");
            pivot.transform.SetParent(barContainer.transform);
            pivot.transform.localPosition = new Vector3(-0.35f, 0, 0); // Left align
            progressBarPivot = pivot.transform;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(progressBarPivot);
            fill.transform.localPosition = new Vector3(0.35f, 0, 0);
            fill.transform.localScale = new Vector3(0.08f, 0.015f, 1f);
            progressBarRenderer = fill.AddComponent<SpriteRenderer>();
            progressBarRenderer.color = new Color(1f, 0.8f, 0f); // Egg yellow
            progressBarRenderer.sortingOrder = 31;

            barContainer.SetActive(false);
        }

        // Trigger for Proximity
        CircleCollider2D hitbox = GetComponent<CircleCollider2D>();
        if (hitbox == null) hitbox = gameObject.AddComponent<CircleCollider2D>();
        hitbox.isTrigger = true;
        hitbox.radius = 1.0f;
    }

    public void ApplyVisualState(ChickenVisualState state)
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

        if (bodySprite != null)
        {
            bodySprite.color = Color.Lerp(Color.white, state.tint, 0.65f);
        }

        if (eyeSprite != null)
        {
            eyeSprite.color = Color.Lerp(eyeSprite.color, Color.white, 0.35f);
        }

        if (!string.IsNullOrWhiteSpace(state.nestLabel))
        {
            Transform nest = transform.Find("ChickenNest");
            if (nest == null)
            {
                GameObject nestObject = new GameObject("ChickenNest");
                nestObject.transform.SetParent(transform, false);
                nest = nestObject.transform;
            }

            nest.localPosition = new Vector3(0f, -0.45f, 0f);
            nest.localScale = Vector3.one * 0.45f;
            SpriteRenderer renderer = nest.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = nest.gameObject.AddComponent<SpriteRenderer>();
            }

            Sprite sprite = Resources.Load<Sprite>("Sprite_Button_green");
            if (sprite != null)
            {
                renderer.sprite = sprite;
            }
            renderer.color = state.tint;
            renderer.sortingOrder = 4;
        }

        if (state.pulseStrength > 0f && progressBarRenderer != null)
        {
            progressBarRenderer.color = Color.Lerp(progressBarRenderer.color, StoryColorPalette.CoinGold, 0.35f);
        }
    }

    private void Update()
    {
        UpdateBlinking();
        UpdateWiggle();
        UpdateIdleBob();
    }

    /// <summary>
    /// Update eye blinking animation
    /// </summary>
    private void UpdateBlinking()
    {
        if (eyeSprite == null) return;

        blinkTimer -= Time.deltaTime;
        if (blinkTimer <= 0)
        {
            StartCoroutine(Blink());
            blinkTimer = blinkInterval + Random.Range(-0.5f, 0.5f);
        }
    }

    private IEnumerator Blink()
    {
        if (eyeSprite != null)
        {
            Vector3 originalEyeScale = eyeSprite.transform.localScale;
            eyeSprite.transform.localScale = new Vector3(originalEyeScale.x, 0.1f, originalEyeScale.z);
            yield return new WaitForSeconds(blinkDuration);
            eyeSprite.transform.localScale = originalEyeScale;
        }
    }

    /// <summary>
    /// Update cute wiggle animation
    /// </summary>
    private void UpdateWiggle()
    {
        wiggleTimer += Time.deltaTime * wiggleSpeed;
        float wiggle = Mathf.Sin(wiggleTimer) * wiggleAmount * 0.3f;
        Transform target = GetMotionVisualRoot();
        if (target != null)
        {
            target.localRotation = visualOriginalRotation * Quaternion.Euler(0, 0, wiggle);
        }
        else
        {
            transform.localRotation = originalRotation * Quaternion.Euler(0, 0, wiggle);
        }
    }

    /// <summary>
    /// Update progress bar and handle chicken movement
    /// </summary>
    private void UpdateProgressBar()
    {
        if (progressBarRenderer != null && isLayingEgg)
        {
            if (progressBarRenderer.transform.parent.parent != null && !progressBarRenderer.transform.parent.parent.gameObject.activeSelf)
                progressBarRenderer.transform.parent.parent.gameObject.SetActive(true);

            if (progressBarPivot != null)
            {
                progressBarPivot.localScale = new Vector3(productionProgress, 1, 1);
            }
        }
    }

    private IEnumerator WanderLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(wanderInterval * 0.5f, wanderInterval * 1.5f);
            yield return new WaitForSeconds(waitTime);

            if (!isLayingEgg && !isWandering)
            {
                if (zoneController != null)
                {
                    Bounds bounds = zoneController.GetZoneBounds();
                    // Ensure chickens stay strictly within their zone by shrinking the wander area by 0.7 units on each side
                    bounds.Expand(-1.4f); 
                    if (bounds.size.x < 1.0f || bounds.size.y < 1.0f) bounds = zoneController.GetZoneBounds(); // Safety fallback

                    Vector3 dest = new Vector3(
                        Random.Range(bounds.min.x, bounds.max.x),
                        Random.Range(bounds.min.y, bounds.max.y),
                        transform.position.z
                    );
                    yield return StartCoroutine(WanderTo(dest));
                }
            }
        }
    }

    private IEnumerator WanderTo(Vector3 destination)
    {
        isWandering = true;
        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, destination);
        float duration = distance / wanderSpeed;
        float elapsed = 0f;

        // Facing
        Transform visualRoot = StoryVisualBinder.FindAttachedVisualRoot(transform);
        if (visualRoot == null)
        {
            visualRoot = GetMotionVisualRoot();
        }
        if (visualRoot != null)
        {
            StoryVisualBinder.SetFacing(visualRoot, destination.x < startPos.x);
        }

        while (elapsed < duration)
        {
            if (isLayingEgg) break; // Cancel wander if fed
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, destination, elapsed / duration);
            yield return null;
        }

        isWandering = false;
    }

    /// <summary>
    /// Update idle bobbing
    /// </summary>
    private void UpdateIdleBob()
    {
        if (!isLayingEgg)
        {
            float bob = Mathf.Sin(Time.time * 2f) * bobAmount;
            Transform target = GetMotionVisualRoot();
            if (target != null)
            {
                target.localScale = visualOriginalScale + new Vector3(0, bob, 0);
            }
            else
            {
                transform.localScale = originalScale + new Vector3(0, bob, 0);
            }
        }
    }

    /// <summary>
    /// Returns 0-1 progress of egg production
    /// </summary>
    public float GetProductionProgress()
    {
        if (!isLayingEgg) return 0f;
        return productionProgress;
    }

    public void Interact()
    {
        if (CanInteract())
        {
            Feed();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && CanInteract())
        {
            Interact();
        }
    }

    public bool CanInteract()
    {
        return !isLayingEgg && GameManager.Instance != null && GameManager.Instance.Corn >= cornRequired;
    }

    /// <summary>
    /// Feed the chicken corn to start egg production
    /// </summary>
    public void Feed()
    {
        TryFeed(true);
    }

    public bool FeedWithCorn()
    {
        return TryFeed(true);
    }

    private bool TryFeed(bool consumeCorn)
    {
        if (isLayingEgg || GameManager.Instance == null)
        {
            return false;
        }

        if (consumeCorn && !GameManager.Instance.UseCorn(cornRequired))
        {
            return false;
        }

        StartCoroutine(FeedAndLayEgg());
        return true;
    }

    /// <summary>
    /// Feeding and egg laying sequence
    /// </summary>
    private IEnumerator FeedAndLayEgg()
    {
        isLayingEgg = true;
        productionProgress = 0.1f;

        // Eating animation - pecking motion
        PlayFeedingEffect();
        SpawnHeartEffect();
        
        UpdateProgressBar();
        yield return StartCoroutine(EatingAnimation());
        productionProgress = 0.4f;
        UpdateProgressBar();

        AudioManager.Instance?.PlaySound("eat");

        // Short pause
        float speedMultiplier = GameManager.Instance != null ? GameManager.Instance.SpeedMultiplier : 1f;
        yield return new WaitForSeconds(0.5f / Mathf.Max(speedMultiplier, 0.01f));
        productionProgress = 0.6f;
        UpdateProgressBar();

        // Egg laying animation
        yield return StartCoroutine(LayEggAnimation());
        productionProgress = 1.0f;
        UpdateProgressBar();

        // Spawn egg
        SpawnEgg();

        // Hide progress bar
        if (progressBarRenderer != null && progressBarRenderer.transform.parent != null && progressBarRenderer.transform.parent.parent != null)
            progressBarRenderer.transform.parent.parent.gameObject.SetActive(false);

        isLayingEgg = false;
        productionProgress = 0f;
    }

    private void EnsureProgressBar()
    {
        if (progressBarRenderer != null) return;

        // Try to find in children first
        Transform bar = transform.Find("ProgressBar");
        if (bar != null)
        {
            progressBarRenderer = bar.Find("Fill")?.GetComponent<SpriteRenderer>();
            progressBarPivot = bar.Find("Fill")?.transform;
            return;
        }

        // Create dynamically
        GameObject root = new GameObject("ProgressBar");
        root.transform.SetParent(transform);
        root.transform.localPosition = new Vector3(0, 0.8f, 0);

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(root.transform);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(1, 0.15f, 1);
        SpriteRenderer bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.color = new Color(0, 0, 0, 0.5f);
        bgSr.sortingOrder = 50;

        GameObject fillRoot = new GameObject("FillPivot");
        fillRoot.transform.SetParent(root.transform);
        fillRoot.transform.localPosition = new Vector3(-0.5f, 0, 0);
        progressBarPivot = fillRoot.transform;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillRoot.transform);
        fill.transform.localPosition = new Vector3(0.5f, 0, 0);
        fill.transform.localScale = new Vector3(1, 0.15f, 1);
        progressBarRenderer = fill.AddComponent<SpriteRenderer>();
        progressBarRenderer.color = Color.green;
        progressBarRenderer.sortingOrder = 51;

        // Use a simple white pixel sprite if available, otherwise just use color
        root.SetActive(false);
    }


    /// <summary>
    /// Eating/pecking animation
    /// </summary>
    private IEnumerator EatingAnimation()
    {
        int pecks = 3;
        for (int i = 0; i < pecks; i++)
        {
            // Peck down
            Vector3 peckRotation = new Vector3(0, 0, 15f);
            float t = 0;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                Transform target = GetMotionVisualRoot();
                if (target != null)
                {
                    target.localRotation = Quaternion.Lerp(visualOriginalRotation, Quaternion.Euler(peckRotation), t / 0.1f);
                }
                else
                {
                    transform.localRotation = Quaternion.Lerp(originalRotation, Quaternion.Euler(peckRotation), t / 0.1f);
                }
                yield return null;
            }

            SpawnPeckDust();

            // Return
            t = 0;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                Transform target = GetMotionVisualRoot();
                if (target != null)
                {
                    target.localRotation = Quaternion.Lerp(Quaternion.Euler(peckRotation), visualOriginalRotation, t / 0.1f);
                }
                else
                {
                    transform.localRotation = Quaternion.Lerp(Quaternion.Euler(peckRotation), originalRotation, t / 0.1f);
                }
                yield return null;
            }

            float speedMultiplier = GameManager.Instance != null ? GameManager.Instance.SpeedMultiplier : 1f;
            yield return new WaitForSeconds(0.1f / Mathf.Max(speedMultiplier, 0.01f));
        }
    }

    /// <summary>
    /// Egg laying animation with squash and stretch
    /// </summary>
    private IEnumerator LayEggAnimation()
    {
        AudioManager.Instance?.PlaySound("egg");

        Transform target = GetMotionVisualRoot();
        Vector3 baseScaleForAnimation = target != null ? visualOriginalScale : originalScale;

        // Build up - squash wider
        Vector3 squash = new Vector3(baseScaleForAnimation.x * 1.3f, baseScaleForAnimation.y * 0.7f, baseScaleForAnimation.z);
        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            if (target != null)
            {
                target.localScale = Vector3.Lerp(baseScaleForAnimation, squash, t / 0.3f);
            }
            else
            {
                transform.localScale = Vector3.Lerp(originalScale, squash, t / 0.3f);
            }
            yield return null;
        }

        // Pop! - stretch tall
        Vector3 stretch = new Vector3(baseScaleForAnimation.x * 0.8f, baseScaleForAnimation.y * 1.2f, baseScaleForAnimation.z);
        t = 0;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            if (target != null)
            {
                target.localScale = Vector3.Lerp(squash, stretch, t / 0.1f);
            }
            else
            {
                transform.localScale = Vector3.Lerp(squash, stretch, t / 0.1f);
            }
            yield return null;
        }

        // Return to normal with bounce
        t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            float bounce = 1f + Mathf.Sin(t * 20f) * 0.1f * (1f - t / 0.2f);
            Transform bounceTarget = GetMotionVisualRoot();
            if (bounceTarget != null)
            {
                bounceTarget.localScale = Vector3.Lerp(stretch, baseScaleForAnimation, t / 0.2f) * bounce;
            }
            else
            {
                transform.localScale = Vector3.Lerp(stretch, originalScale, t / 0.2f) * bounce;
            }
            yield return null;
        }

        Transform finalTarget = GetMotionVisualRoot();
        if (finalTarget != null)
        {
            finalTarget.localScale = visualOriginalScale;
        }
        else
        {
            transform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Spawn an egg at the spawn point with bounce animation
    /// </summary>
    private void SpawnEgg()
    {
        Vector3 spawnPos = eggSpawnPoint != null ? eggSpawnPoint.position : transform.position - new Vector3(0, 0.5f, 0);

        if (eggPrefab != null)
        {
            GameObject egg = Instantiate(eggPrefab, spawnPos, Quaternion.identity);
            StartCoroutine(EggBounceAnimation(egg.transform, spawnPos));
        }
        else
        {
            // Create simple egg visual if no prefab
            CreateSimpleEgg(spawnPos);
        }

        // Spawn particles
        SpawnEggParticles(spawnPos);
    }

    /// <summary>
    /// Create a simple egg visual when no prefab is assigned
    /// </summary>
    private void CreateSimpleEgg(Vector3 position)
    {
        GameObject egg = new GameObject("Egg");
        egg.transform.position = position;

        SpriteRenderer sr = egg.AddComponent<SpriteRenderer>();
        sr.sprite = GetRuntimeEggSprite();
        sr.color = new Color(1f, 0.98f, 0.9f); // Off-white egg color
        sr.sortingLayerName = "Characters";
        sr.sortingOrder = 25;

        // Add collider for collection
        CircleCollider2D col = egg.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;
        col.isTrigger = true;

        // Add egg collector component
        egg.AddComponent<CollectibleEgg>();

        StartCoroutine(EggBounceAnimation(egg.transform, position));
    }

    /// <summary>
    /// Visual feedback for being fed: corn particles flying towards the chicken
    /// </summary>
    public void PlayFeedingEffect()
    {
        GameObject particles = new GameObject("FeedingCornParticles");
        particles.transform.position = transform.position + Vector3.up * 0.8f;

        ParticleSystem ps = particles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.08f;
        main.startLifetime = 0.4f;
        main.startColor = Color.yellow; // Explicitly use bright yellow
        main.startSpeed = 2f;
        main.gravityModifier = 1f;
        main.maxParticles = 10;
        main.duration = 0.2f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.1f;
        particles.transform.rotation = Quaternion.Euler(90, 0, 0); // Point downwards
        
        // Ensure material is not blue-missing-fallback
        ParticleSystemRenderer psr = particles.GetComponent<ParticleSystemRenderer>();
        if (psr != null) {
            psr.material = new Material(Shader.Find("Sprites/Default"));
        }

        ps.Play();
        Destroy(particles, 1f);

        AudioManager.Instance?.PlaySound("eat");
    }

    private static Sprite GetRuntimeEggSprite()
    {
        if (runtimeEggSprite != null)
        {
            return runtimeEggSprite;
        }

        Texture2D texture = new Texture2D(32, 40, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2(15.5f, 19f);
        float radiusX = 12f;
        float radiusY = 16f;

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float normalizedX = (x - center.x) / radiusX;
                float normalizedY = (y - center.y) / radiusY;
                float shape = normalizedX * normalizedX + normalizedY * normalizedY;
                texture.SetPixel(x, y, shape <= 1f ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        runtimeEggSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            32f);
        runtimeEggSprite.name = "RuntimeEggSprite";
        return runtimeEggSprite;
    }

    /// <summary>
    /// Egg spawn bounce animation
    /// </summary>
    private IEnumerator EggBounceAnimation(Transform egg, Vector3 targetPos)
    {
        if (egg == null) yield break;

        Vector3 startPos = targetPos + new Vector3(0, 0.3f, 0);
        egg.position = startPos;
        egg.localScale = Vector3.zero;

        float targetScale = 0.65f; // reduced from 1.0f

        // Pop in with scale
        float t = 0;
        while (t < 0.15f)
        {
            if (egg == null) yield break;
            t += Time.deltaTime;
            float scale = Mathf.Sin(t / 0.15f * Mathf.PI / 2f) * (targetScale * 1.2f);
            egg.localScale = Vector3.one * Mathf.Min(scale, targetScale);
            egg.position = Vector3.Lerp(startPos, targetPos, t / 0.15f);
            yield return null;
        }

        // Bounce settle
        int bounces = 2;
        float bounceHeight = 0.1f;
        for (int i = 0; i < bounces; i++)
        {
            if (egg == null) yield break;
            Vector3 upPos = targetPos + new Vector3(0, bounceHeight, 0);
            t = 0;
            while (t < 0.1f)
            {
                if (egg == null) yield break;
                t += Time.deltaTime;
                egg.position = Vector3.Lerp(targetPos, upPos, Mathf.Sin(t / 0.1f * Mathf.PI));
                yield return null;
            }
            bounceHeight *= 0.5f;
        }

        if (egg != null)
        {
            egg.position = targetPos;
            egg.localScale = Vector3.one * targetScale;
        }
    }

    /// <summary>
    /// Spawn egg particles
    /// </summary>
    private void SpawnEggParticles(Vector3 position)
    {
        GameObject particles = new GameObject("EggParticles");
        particles.transform.position = position;

        ParticleSystem ps = particles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.1f;
        main.startLifetime = 0.5f;
        main.startColor = new Color(1f, 1f, 0.8f, 1f); // Light yellow glow
        main.startSpeed = 1f;
        main.gravityModifier = 0.2f;
        main.maxParticles = 8;
        main.duration = 0.1f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 8) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;

        ps.Play();
        Destroy(particles, 1f);
    }

    private void SpawnPeckDust()
    {
        Vector3 dustPosition = transform.position + new Vector3(0f, -0.35f, 0f);
        GameObject dust = new GameObject("PeckDust");
        dust.transform.position = dustPosition;

        ParticleSystem ps = dust.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.08f;
        main.startLifetime = 0.25f;
        main.startColor = new Color(0.84f, 0.80f, 0.78f, 0.8f);
        main.startSpeed = 0.3f;
        main.gravityModifier = -0.05f;
        main.maxParticles = 5;
        main.duration = 0.1f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 4) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.12f;

        ps.Play();
        Destroy(dust, 0.5f);
    }

    private void CacheMotionVisualRoot()
    {
        if (visualRoot == null)
        {
            visualRoot = StoryVisualBinder.FindAttachedVisualRoot(transform);
        }

        if (visualRoot == null)
        {
            visualRoot = transform.Find("Visual");
        }

        if (visualRoot != null)
        {
            visualOriginalScale = visualRoot.localScale;
            visualOriginalRotation = visualRoot.localRotation;
        }
    }

    private Transform GetMotionVisualRoot()
    {
        if (visualRoot == null)
        {
            CacheMotionVisualRoot();
        }

        return visualRoot;
    }

    /// <summary>
    /// Happy wiggle when fed
    /// </summary>
    public void HappyWiggle()
    {
        StartCoroutine(HappyWiggleAnimation());
    }

    private IEnumerator HappyWiggleAnimation()
    {
        float duration = 0.5f;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float wiggle = Mathf.Sin(t * 30f) * wiggleAmount * (1f - t / duration);
            Transform target = GetMotionVisualRoot();
            if (target != null)
            {
                target.localRotation = visualOriginalRotation * Quaternion.Euler(0, 0, wiggle);
            }
            else
            {
                transform.localRotation = originalRotation * Quaternion.Euler(0, 0, wiggle);
            }
            yield return null;
        }
        Transform finalTarget = GetMotionVisualRoot();
        if (finalTarget != null)
        {
            finalTarget.localRotation = visualOriginalRotation;
        }
        else
        {
            transform.localRotation = originalRotation;
        }
    }

    /// <summary>
    /// Spawn cute heart particles when fed
    /// </summary>
    private void SpawnHeartEffect()
    {
        GameObject hearts = new GameObject("FeedHearts");
        hearts.transform.position = transform.position + new Vector3(0, 0.5f, 0);

        ParticleSystem ps = hearts.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.2f;
        main.startLifetime = 1.0f;
        main.startColor = new Color(1f, 0.4f, 0.6f, 1f); // Pink/Rose
        main.startSpeed = 0.8f;
        main.gravityModifier = -0.2f; 
        main.maxParticles = 5;
        main.duration = 0.5f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 3) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        ps.Play();
        Destroy(hearts, 1.5f);
    }

    // --- IFeedable Implementation ---
    public bool NeedsFeeding() => !isLayingEgg;
    
    public void Feed(string itemID)
    {
        if (!string.Equals(itemID, "Corn", System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        TryFeed(true);
    }

    public bool CanAcceptFood(string itemID)
    {
        return !isLayingEgg && string.Equals(itemID, "Corn", System.StringComparison.OrdinalIgnoreCase);
    }

    // GetProductionProgress() and other methods are already satisfied by existing public methods

    // --- IZoneMember Implementation ---
    public void Initialize(string zoneID, int slotIndex)
    {
        // Store for future specialized logic
        gameObject.name = $"{zoneID}_{slotIndex}";
    }
}
