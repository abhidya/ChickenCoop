using UnityEngine;
using System.Collections;

/// <summary>
/// StoreCounter - Location where eggs can be sold for coins.
/// Triggers coin burst particles and satisfying sound effects.
/// Implements IInteractable for player interaction.
/// </summary>
public class StoreCounter : MonoBehaviour, IInteractable
{
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

    // State
    private bool canSell = true;
    private Vector3 originalScale;
    private float bounceTimer = 0f;

    private void Start()
    {
        originalScale = transform.localScale;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        UpdateVisual();
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
            bool hasEggs = GameManager.Instance.Eggs > 0;
            spriteRenderer.color = hasEggs ? activeColor : inactiveColor;
        }
    }

    /// <summary>
    /// Player interaction - sell an egg
    /// </summary>
    public void Interact()
    {
        if (CanInteract())
        {
            SellEgg();
        }
    }

    /// <summary>
    /// Check if player can sell
    /// </summary>
    public bool CanInteract()
    {
        return canSell && GameManager.Instance.Eggs > 0;
    }

    /// <summary>
    /// Sell an egg at the store
    /// </summary>
    public void SellEgg()
    {
        if (!canSell || GameManager.Instance.Eggs <= 0) return;

        canSell = false;

        // Perform sale
        if (GameManager.Instance.SellEgg())
        {
            // Play sale animation
            StartCoroutine(SaleAnimation());

            // Spawn coin burst
            SpawnCoinBurst();

            AudioManager.Instance?.PlaySound("sell");
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
        yield return new WaitForSeconds(sellCooldown / GameManager.Instance.SpeedMultiplier);
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

        Destroy(particles, 1.5f);
    }

    /// <summary>
    /// Upgrade store for better prices
    /// </summary>
    public void UpgradeStore()
    {
        GameManager.Instance.ApplyUpgrade(UpgradeType.EggPrice, 1.2f);

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
        Destroy(sparkles, 1f);
    }
}
