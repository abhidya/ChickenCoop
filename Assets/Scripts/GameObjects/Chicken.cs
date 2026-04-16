using UnityEngine;
using System.Collections;

/// <summary>
/// Chicken - Represents a chicken that can be fed corn and produces eggs.
/// Includes cute animations: blinking, wiggling, eating, and egg laying.
/// Implements IInteractable for player interaction.
/// </summary>
public class Chicken : MonoBehaviour, IInteractable
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
    [SerializeField] private Transform eggSpawnPoint;
    [SerializeField] private GameObject eggPrefab;
    [SerializeField] private GameObject storyVisualPrefab;

    // State
    private bool isLayingEgg = false;
    private float blinkTimer = 0f;
    private float wiggleTimer = 0f;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private float productionProgress = 0f;

    public bool IsLayingEgg => isLayingEgg;

    private void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;

        GameConfig config = GameManager.Instance != null ? GameManager.Instance.Config : null;
        if (config != null)
        {
            cornRequired = config.cornToFeed;
        }

        if (bodySprite == null)
        {
            bodySprite = GetComponent<SpriteRenderer>();
        }

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
            StoryVisualBinder.AttachVisualPrefab(transform, resolvedPrefab, bodySprite);
        }

        // Randomize initial blink timer
        blinkTimer = Random.Range(0f, blinkInterval);
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
        transform.localRotation = originalRotation * Quaternion.Euler(0, 0, wiggle);
    }

    /// <summary>
    /// Update idle bobbing
    /// </summary>
    private void UpdateIdleBob()
    {
        if (!isLayingEgg)
        {
            float bob = Mathf.Sin(Time.time * 2f) * bobAmount;
            transform.localScale = originalScale + new Vector3(0, bob, 0);
        }
    }

    public void Interact()
    {
        if (CanInteract())
        {
            Feed();
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

    /// <summary>
    /// Check if chicken can be interacted with
    /// </summary>
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
        yield return StartCoroutine(EatingAnimation());
        productionProgress = 0.4f;

        AudioManager.Instance?.PlaySound("eat");

        // Short pause
        yield return new WaitForSeconds(0.5f / GameManager.Instance.SpeedMultiplier);
        productionProgress = 0.6f;

        // Egg laying animation
        yield return StartCoroutine(LayEggAnimation());
        productionProgress = 1.0f;

        // Spawn egg
        SpawnEgg();

        isLayingEgg = false;
        productionProgress = 0f;
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
                transform.localRotation = Quaternion.Lerp(originalRotation, Quaternion.Euler(peckRotation), t / 0.1f);
                yield return null;
            }

            SpawnPeckDust();

            // Return
            t = 0;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                transform.localRotation = Quaternion.Lerp(Quaternion.Euler(peckRotation), originalRotation, t / 0.1f);
                yield return null;
            }

            yield return new WaitForSeconds(0.1f / GameManager.Instance.SpeedMultiplier);
        }
    }

    /// <summary>
    /// Egg laying animation with squash and stretch
    /// </summary>
    private IEnumerator LayEggAnimation()
    {
        AudioManager.Instance?.PlaySound("egg");

        // Build up - squash wider
        Vector3 squash = new Vector3(originalScale.x * 1.3f, originalScale.y * 0.7f, originalScale.z);
        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, squash, t / 0.3f);
            yield return null;
        }

        // Pop! - stretch tall
        Vector3 stretch = new Vector3(originalScale.x * 0.8f, originalScale.y * 1.2f, originalScale.z);
        t = 0;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(squash, stretch, t / 0.1f);
            yield return null;
        }

        // Return to normal with bounce
        t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            float bounce = 1f + Mathf.Sin(t * 20f) * 0.1f * (1f - t / 0.2f);
            transform.localScale = Vector3.Lerp(stretch, originalScale, t / 0.2f) * bounce;
            yield return null;
        }

        transform.localScale = originalScale;
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
        Vector3 startPos = targetPos + new Vector3(0, 0.3f, 0);
        egg.position = startPos;
        egg.localScale = Vector3.zero;

        float targetScale = 0.65f; // reduced from 1.0f

        // Pop in with scale
        float t = 0;
        while (t < 0.15f)
        {
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
            Vector3 upPos = targetPos + new Vector3(0, bounceHeight, 0);
            t = 0;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                egg.position = Vector3.Lerp(targetPos, upPos, Mathf.Sin(t / 0.1f * Mathf.PI));
                yield return null;
            }
            bounceHeight *= 0.5f;
        }

        egg.position = targetPos;
        egg.localScale = Vector3.one * targetScale;
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
            transform.localRotation = originalRotation * Quaternion.Euler(0, 0, wiggle);
            yield return null;
        }
        transform.localRotation = originalRotation;
    }
}
