using UnityEngine;
using System.Collections;

/// <summary>
/// HarvestableField - Represents a corn field that can be harvested for corn.
/// Implements IInteractable for player interaction.
/// Includes growth animation and harvest effects.
/// </summary>
public class HarvestableField : MonoBehaviour, IInteractable
{
    [Header("Harvest Settings")]
    [SerializeField] private int cornPerHarvest = 1;
    [SerializeField] private float harvestCooldown = 2f;

    [Header("Animation Settings")]
    [SerializeField] private float bounceAmount = 0.1f;
    [SerializeField] private float bounceSpeed = 2f;
    [SerializeField] private Color readyColor = new Color(1f, 0.9f, 0.3f);
    [SerializeField] private Color cooldownColor = new Color(0.6f, 0.7f, 0.4f);

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform cornVisual;

    // State
    private bool canHarvest = true;
    private float cooldownTimer = 0f;
    private Vector3 originalScale;
    private float bounceTimer = 0f;

    private void Start()
    {
        originalScale = transform.localScale;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = readyColor;
        }
    }

    private void Update()
    {
        UpdateCooldown();
        UpdateAnimation();
    }

    /// <summary>
    /// Update harvest cooldown timer
    /// </summary>
    private void UpdateCooldown()
    {
        if (!canHarvest)
        {
            cooldownTimer -= Time.deltaTime * GameManager.Instance.SpeedMultiplier;
            if (cooldownTimer <= 0)
            {
                canHarvest = true;
                OnReadyToHarvest();
            }
        }
    }

    /// <summary>
    /// Update ambient bounce animation
    /// </summary>
    private void UpdateAnimation()
    {
        bounceTimer += Time.deltaTime * bounceSpeed;

        // Gentle swaying animation for corn stalks
        float sway = Mathf.Sin(bounceTimer) * bounceAmount * 0.5f;
        
        if (cornVisual != null)
        {
            cornVisual.localRotation = Quaternion.Euler(0, 0, sway * 10f);
        }

        // Scale bounce when ready
        if (canHarvest)
        {
            float bounce = 1f + Mathf.Sin(bounceTimer * 2f) * bounceAmount * 0.3f;
            transform.localScale = originalScale * bounce;
        }
    }

    /// <summary>
    /// Called when player interacts with the field
    /// </summary>
    public void Interact()
    {
        if (canHarvest)
        {
            Harvest();
        }
    }

    /// <summary>
    /// Check if field can be harvested
    /// </summary>
    public bool CanInteract()
    {
        return canHarvest;
    }

    /// <summary>
    /// Perform harvest action
    /// </summary>
    public void Harvest()
    {
        if (!canHarvest) return;

        canHarvest = false;
        cooldownTimer = harvestCooldown;

        // Add corn to inventory
        GameManager.Instance.AddCorn(cornPerHarvest);

        // Play harvest animation
        StartCoroutine(HarvestAnimation());

        // Update visual
        if (spriteRenderer != null)
        {
            spriteRenderer.color = cooldownColor;
        }

        // Spawn pop particle effect
        SpawnHarvestParticles();

        AudioManager.Instance?.PlaySound("harvest");
    }

    /// <summary>
    /// Squash and stretch harvest animation
    /// </summary>
    private IEnumerator HarvestAnimation()
    {
        Vector3 original = originalScale;
        Vector3 squash = new Vector3(original.x * 1.4f, original.y * 0.6f, original.z);
        Vector3 stretch = new Vector3(original.x * 0.8f, original.y * 1.2f, original.z);

        // Squash down
        float t = 0;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(original, squash, t / 0.1f);
            yield return null;
        }

        // Stretch up (corn pops out)
        t = 0;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(squash, stretch, t / 0.1f);
            yield return null;
        }

        // Return to normal (smaller for cooldown)
        Vector3 smallScale = original * 0.8f;
        t = 0;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(stretch, smallScale, t / 0.15f);
            yield return null;
        }

        transform.localScale = smallScale;

        // Gradually grow back during cooldown
        StartCoroutine(GrowBackAnimation(smallScale, original));
    }

    /// <summary>
    /// Grow back animation during cooldown
    /// </summary>
    private IEnumerator GrowBackAnimation(Vector3 from, Vector3 to)
    {
        float growDuration = harvestCooldown * 0.9f;
        float elapsed = 0f;

        while (elapsed < growDuration && !canHarvest)
        {
            elapsed += Time.deltaTime * GameManager.Instance.SpeedMultiplier;
            float t = elapsed / growDuration;
            transform.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        if (canHarvest)
        {
            transform.localScale = to;
        }
    }

    /// <summary>
    /// Called when corn is ready to harvest again
    /// </summary>
    private void OnReadyToHarvest()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = readyColor;
        }

        // Pop animation
        StartCoroutine(ReadyPopAnimation());

        // Spawn ready sparkle
        SpawnReadySparkle();
    }

    private IEnumerator ReadyPopAnimation()
    {
        Vector3 target = originalScale;
        Vector3 pop = originalScale * 1.2f;

        transform.localScale = pop;

        float t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(pop, target, t / 0.2f);
            yield return null;
        }

        transform.localScale = target;
    }

    /// <summary>
    /// Spawn harvest particle effects
    /// </summary>
    private void SpawnHarvestParticles()
    {
        GameObject particles = new GameObject("HarvestParticles");
        particles.transform.position = transform.position + new Vector3(0, 0.5f, 0);

        ParticleSystem ps = particles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.2f;
        main.startLifetime = 0.6f;
        main.startColor = new Color(1f, 0.9f, 0.2f, 1f); // Golden yellow corn color
        main.startSpeed = 3f;
        main.gravityModifier = 0.5f;
        main.maxParticles = 10;
        main.duration = 0.1f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;
        shape.radius = 0.1f;

        ps.Play();
        Destroy(particles, 1f);
    }

    /// <summary>
    /// Spawn sparkle when ready to harvest
    /// </summary>
    private void SpawnReadySparkle()
    {
        GameObject sparkle = new GameObject("ReadySparkle");
        sparkle.transform.position = transform.position + new Vector3(0, 0.3f, 0);

        ParticleSystem ps = sparkle.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.15f;
        main.startLifetime = 0.4f;
        main.startColor = new Color(1f, 1f, 0.5f, 1f);
        main.startSpeed = 0.5f;
        main.gravityModifier = -0.3f;
        main.maxParticles = 5;
        main.duration = 0.1f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 5) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        ps.Play();
        Destroy(sparkle, 0.8f);
    }

    /// <summary>
    /// Upgrade the corn field to produce more corn
    /// </summary>
    public void UpgradeField(int additionalCorn)
    {
        cornPerHarvest += additionalCorn;
        
        // Visual upgrade effect
        StartCoroutine(UpgradeAnimation());
    }

    private IEnumerator UpgradeAnimation()
    {
        Vector3 pop = originalScale * 1.3f;
        transform.localScale = pop;

        yield return new WaitForSeconds(0.1f);

        float t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(pop, originalScale, t / 0.2f);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}
