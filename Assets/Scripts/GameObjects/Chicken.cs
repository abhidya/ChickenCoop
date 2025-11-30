using UnityEngine;
using System.Collections;

/// <summary>
/// Chicken - Represents a chicken that can be fed corn and produces eggs.
/// Includes cute animations: blinking, wiggling, eating, and egg laying.
/// Implements IInteractable for player interaction.
/// </summary>
public class Chicken : MonoBehaviour, IInteractable
{
    [Header("Production Settings")]
    [SerializeField] private int cornRequired = 1;
    [SerializeField] private float eggLayDelay = 1.5f;

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

    // State
    private bool isFed = false;
    private bool isLayingEgg = false;
    private float blinkTimer = 0f;
    private float wiggleTimer = 0f;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;

        if (bodySprite == null)
        {
            bodySprite = GetComponent<SpriteRenderer>();
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

    /// <summary>
    /// Player interaction - feed the chicken
    /// </summary>
    public void Interact()
    {
        if (CanInteract())
        {
            Feed();
        }
    }

    /// <summary>
    /// Check if chicken can be interacted with
    /// </summary>
    public bool CanInteract()
    {
        return !isLayingEgg && GameManager.Instance.Corn >= cornRequired;
    }

    /// <summary>
    /// Feed the chicken corn to start egg production
    /// </summary>
    public void Feed()
    {
        if (isLayingEgg) return;

        if (GameManager.Instance.UseCorn(cornRequired))
        {
            StartCoroutine(FeedAndLayEgg());
        }
    }

    /// <summary>
    /// Feeding and egg laying sequence
    /// </summary>
    private IEnumerator FeedAndLayEgg()
    {
        isLayingEgg = true;

        // Eating animation - pecking motion
        yield return StartCoroutine(EatingAnimation());

        AudioManager.Instance?.PlaySound("eat");

        // Short pause
        yield return new WaitForSeconds(0.5f / GameManager.Instance.SpeedMultiplier);

        // Egg laying animation
        yield return StartCoroutine(LayEggAnimation());

        // Spawn egg
        SpawnEgg();

        isLayingEgg = false;
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
        sr.color = new Color(1f, 0.98f, 0.9f); // Off-white egg color
        sr.sortingLayerName = "Characters";
        sr.sortingOrder = 1;

        // Add collider for collection
        CircleCollider2D col = egg.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;
        col.isTrigger = true;

        // Add egg collector component
        egg.AddComponent<CollectibleEgg>();

        StartCoroutine(EggBounceAnimation(egg.transform, position));
    }

    /// <summary>
    /// Egg spawn bounce animation
    /// </summary>
    private IEnumerator EggBounceAnimation(Transform egg, Vector3 targetPos)
    {
        Vector3 startPos = targetPos + new Vector3(0, 0.3f, 0);
        egg.position = startPos;
        egg.localScale = Vector3.zero;

        // Pop in with scale
        float t = 0;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            float scale = Mathf.Sin(t / 0.15f * Mathf.PI / 2f) * 1.2f;
            egg.localScale = Vector3.one * Mathf.Min(scale, 1f);
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
        egg.localScale = Vector3.one;
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

/// <summary>
/// CollectibleEgg - Egg that can be clicked to collect into inventory
/// </summary>
public class CollectibleEgg : MonoBehaviour, IInteractable
{
    private bool isCollected = false;

    public void Interact()
    {
        if (!isCollected)
        {
            Collect();
        }
    }

    public bool CanInteract()
    {
        return !isCollected;
    }

    private void Collect()
    {
        isCollected = true;
        GameManager.Instance.AddEgg(1);

        // Fly to UI animation
        StartCoroutine(CollectAnimation());
    }

    private System.Collections.IEnumerator CollectAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        // Spawn glow effect
        SpawnGlowEffect();

        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float progress = t / 0.3f;

            // Scale down and move up
            transform.localScale = startScale * (1f - progress);
            transform.position = startPos + new Vector3(0, progress * 2f, 0);

            yield return null;
        }

        Destroy(gameObject);
    }

    private void SpawnGlowEffect()
    {
        GameObject glow = new GameObject("GlowEffect");
        glow.transform.position = transform.position;

        ParticleSystem ps = glow.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.15f;
        main.startLifetime = 0.4f;
        main.startColor = new Color(1f, 1f, 0.6f, 0.8f);
        main.startSpeed = 0.8f;
        main.gravityModifier = -0.5f;
        main.maxParticles = 12;
        main.duration = 0.1f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 12) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;

        ps.Play();
        Destroy(glow, 0.8f);
    }

    private void OnMouseDown()
    {
        Interact();
    }
}
