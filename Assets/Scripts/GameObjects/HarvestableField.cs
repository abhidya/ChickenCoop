using UnityEngine;
using System.Collections;
using ChickenCoop.Managers;
using ChickenCoop.Interfaces;

/// <summary>
/// HarvestableField - Represents a corn field that can be harvested for corn.
/// Implements IInteractable, IHarvestable, and IZoneMember interfaces.
/// </summary>
public class HarvestableField : MonoBehaviour, IInteractable, IHarvestable, IZoneMember
{
    [Header("Harvest Settings")]
    [SerializeField] private int cornPerHarvest = 1;
    [SerializeField] private float harvestCooldown = 2f;
    [SerializeField] private string productId = "Corn";

    [Header("Animation Settings")]
    [SerializeField] private float bounceAmount = 0.1f;
    [SerializeField] private float bounceSpeed = 2f;
    [SerializeField] private Color readyColor = new Color(1f, 0.9f, 0.3f);
    [SerializeField] private Color cooldownColor = new Color(0.6f, 0.7f, 0.4f);

    [Header("Visual References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer soilRenderer;
    [SerializeField] private SpriteRenderer progressBarRenderer;
    [SerializeField] private Transform cornVisual;
    [SerializeField] private GameObject storyVisualPrefab;

    [Header("Assets")]
    [SerializeField] private Sprite drySoilSprite;
    [SerializeField] private Sprite wetSoilSprite;
    [SerializeField] private Sprite progressBarBgSprite;
    [SerializeField] private Sprite progressBarFillSprite;

    // State
    private bool canHarvest = true;
    private float cooldownTimer = 0f;
    private Vector3 originalScale;
    private float bounceTimer = 0f;
    private SpriteRenderer[] storyRenderers;
    private Transform progressBarPivot;
    private float visualScaleMultiplier = 1f;
    private Color visualTint = Color.white;
    private string visualBadgeLabel;

    private void Start()
    {
        originalScale = transform.localScale;

        GameConfig config = GameManager.Instance != null ? GameManager.Instance.Config : null;
        if (config != null)
        {
            cornPerHarvest = config.cornPerHarvest;
            harvestCooldown = config.harvestCooldown;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Set tag for tutorial/logic
        if (gameObject.CompareTag("Untagged"))
        {
            gameObject.tag = "CornField";
        }

        EnsureProgressBar();
        EnsureVisualComponents();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        Color color = canHarvest ? readyColor : cooldownColor;
        color = Color.Lerp(color, visualTint, 0.25f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }

        if (soilRenderer != null)
        {
            soilRenderer.sprite = canHarvest ? drySoilSprite : wetSoilSprite;
            // Fallback if sprites missing but we want darkening
            if (soilRenderer.sprite == null)
            {
                soilRenderer.color = canHarvest ? new Color(0.6f, 0.4f, 0.2f) : new Color(0.3f, 0.2f, 0.1f);
            }
            else
            {
                soilRenderer.color = Color.white;
            }
        }

        ApplyStoryTint(color);
    }

    private void EnsureVisualComponents()
    {
        // Soil Renderer
        if (soilRenderer == null)
        {
            GameObject soilObj = new GameObject("SoilVisual");
            soilObj.transform.SetParent(transform);
            soilObj.transform.localPosition = new Vector3(0, -0.4f, 0);
            soilRenderer = soilObj.AddComponent<SpriteRenderer>();
            soilRenderer.sortingOrder = -1;
            
            if (drySoilSprite == null) drySoilSprite = Resources.Load<Sprite>("HappyHarvestDrySoil"); // Example path, will adjust if needed
            if (wetSoilSprite == null) wetSoilSprite = Resources.Load<Sprite>("HappyHarvestWetSoil");
        }

        // Progress Bar (World Space)
        if (progressBarRenderer == null)
        {
            GameObject barContainer = new GameObject("ProgressBar");
            barContainer.transform.SetParent(transform);
            barContainer.transform.localPosition = new Vector3(0, 0.6f, 0);
            
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(barContainer.transform);
            bg.transform.localPosition = Vector3.zero;
            bg.transform.localScale = new Vector3(0.12f, 0.02f, 1f);
            var bgSr = bg.AddComponent<SpriteRenderer>();
            bgSr.color = new Color(0, 0, 0, 0.5f);
            bgSr.sortingOrder = 10;

            GameObject pivot = new GameObject("Pivot");
            pivot.transform.SetParent(barContainer.transform);
            pivot.transform.localPosition = new Vector3(-0.5f, 0, 0); // Left align
            progressBarPivot = pivot.transform;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(progressBarPivot);
            fill.transform.localPosition = new Vector3(0.5f, 0, 0);
            fill.transform.localScale = new Vector3(0.12f, 0.02f, 1f);
            progressBarRenderer = fill.AddComponent<SpriteRenderer>();
            progressBarRenderer.color = Color.green;
            progressBarRenderer.sortingOrder = 11;

            barContainer.SetActive(false);
        }

        // Trigger for Proximity
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.0f;

        // Resolve visual prefab — serialized field first, then Resources fallback
        GameObject resolvedPrefab = storyVisualPrefab;
        if (resolvedPrefab == null)
        {
            resolvedPrefab = Resources.Load<GameObject>("HappyHarvestCorn");
        }

        if (resolvedPrefab != null)
        {
            Transform existingVisual = transform.Find("Visual");
            if (existingVisual != null)
            {
                storyRenderers = existingVisual.GetComponentsInChildren<SpriteRenderer>(true);
                if (spriteRenderer != null) spriteRenderer.enabled = false;
                cornVisual = existingVisual;
            }
            else
            {
                GameObject visual = StoryVisualBinder.AttachVisualPrefab(
                    transform, resolvedPrefab, spriteRenderer, true);
                if (visual != null)
                {
                    storyRenderers = visual.GetComponentsInChildren<SpriteRenderer>(true);
                    cornVisual = visual.transform;
                }
            }
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
            float speedMult = GameManager.Instance != null ? GameManager.Instance.SpeedMultiplier : 1f;
            cooldownTimer -= Time.deltaTime * speedMult;
            
            // Update Progress Bar
            if (progressBarRenderer != null)
            {
                if (progressBarRenderer.transform.parent.parent != null && !progressBarRenderer.transform.parent.parent.gameObject.activeSelf)
                    progressBarRenderer.transform.parent.parent.gameObject.SetActive(true);
                
                float progress = 1f - Mathf.Clamp01(cooldownTimer / harvestCooldown);
                if (progressBarPivot != null)
                {
                    progressBarPivot.localScale = new Vector3(progress, 1, 1);
                }
            }

            if (cooldownTimer <= 0)
            {
                canHarvest = true;
                OnReadyToHarvest();
                UpdateVisuals(); // Restore dry soil when ready
                
                if (progressBarRenderer != null && progressBarRenderer.transform.parent.parent != null)
                    progressBarRenderer.transform.parent.parent.gameObject.SetActive(false);
            }
            else
            {
                // Keep updating soil color during cooldown so it shows wet soil correctly
                if (soilRenderer != null && soilRenderer.sprite == null)
                {
                    soilRenderer.color = new Color(0.3f, 0.2f, 0.1f); // wet/dark soil
                }
            }
        }
    }

    /// <summary>
    /// Returns 0-1 progress of corn growth.
    /// 1.0 means ready to harvest.
    /// </summary>
    public float GetGrowthProgress()
    {
        if (canHarvest) return 1f;
        if (harvestCooldown <= 0) return 1f;
        return 1f - Mathf.Clamp01(cooldownTimer / harvestCooldown);
    }

    /// <summary>
    private void UpdateAnimation()
    {
        bounceTimer += Time.deltaTime * bounceSpeed;

        // Corn visual bounces/pulses when ready to harvest (but no rotation sway)
        if (cornVisual != null)
        {
            // Keep rotation static - no swaying
            cornVisual.localRotation = Quaternion.identity;
            
            // Bounce/scale when ready to harvest
            if (canHarvest)
            {
                float bounce = 1f + Mathf.Sin(bounceTimer * 2f) * bounceAmount * 0.3f;
                cornVisual.localScale = Vector3.one * bounce * visualScaleMultiplier;
            }
            else
            {
                cornVisual.localScale = Vector3.one * 0.8f * visualScaleMultiplier;
            }
        }
        
        // Soil stays completely static - no rotation at all
        if (soilRenderer != null)
        {
            soilRenderer.transform.localRotation = Quaternion.identity;
            soilRenderer.transform.localScale = Vector3.one;
        }
    }

    public void ApplyVisualState(float scaleMultiplier, Color tint, string badgeLabel = null)
    {
        visualScaleMultiplier = Mathf.Max(0.9f, scaleMultiplier);
        visualTint = tint;
        visualBadgeLabel = badgeLabel;
        UpdateVisuals();

        if (string.IsNullOrWhiteSpace(visualBadgeLabel))
        {
            return;
        }

        Transform badge = transform.Find("FertilizerBadge");
        if (badge == null)
        {
            GameObject badgeObject = new GameObject("FertilizerBadge");
            badgeObject.transform.SetParent(transform, false);
            badge = badgeObject.transform;
        }

        badge.localPosition = new Vector3(0f, 0.7f, 0f);
        badge.localScale = Vector3.one * 0.4f;
        SpriteRenderer badgeRenderer = badge.GetComponent<SpriteRenderer>();
        if (badgeRenderer == null)
        {
            badgeRenderer = badge.gameObject.AddComponent<SpriteRenderer>();
        }

        Sprite sprite = Resources.Load<Sprite>("Sprite_Button_green");
        if (sprite != null)
        {
            badgeRenderer.sprite = sprite;
        }
        badgeRenderer.color = tint;
        badgeRenderer.sortingOrder = 35;

        Transform label = badge.Find("Label");
        if (label == null)
        {
            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(badge, false);
            label = labelObject.transform;
        }

        label.localPosition = Vector3.zero;
        label.localScale = Vector3.one * 0.5f;
        TextMesh textMesh = label.GetComponent<TextMesh>();
        if (textMesh == null)
        {
            textMesh = label.gameObject.AddComponent<TextMesh>();
            textMesh.fontSize = 20;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
        }

        textMesh.text = visualBadgeLabel;
        textMesh.color = Color.white;
        label.GetComponent<MeshRenderer>().sortingOrder = 36;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canHarvest)
        {
            Interact();
        }
    }

    /// <summary>
    /// Check if field can be harvested
    /// </summary>
    public bool CanInteract()
    {
        return canHarvest;
    }

    public bool IsReadyToHarvest()
    {
        return canHarvest;
    }

    /// <summary>
    /// Perform harvest action
    /// </summary>
    public void Harvest()
    {
        Debug.Log($"[HarvestableField] Harvest called. canHarvest={canHarvest}");
        if (!canHarvest) return;

        canHarvest = false;
        cooldownTimer = harvestCooldown;
        UpdateVisuals(); // Switch to wet soil immediately on harvest

        // Add product to inventory
        GameManager.Instance.AddItem(productId, cornPerHarvest, transform.position + Vector3.up * 0.6f);

        // Play harvest animation
        StartCoroutine(HarvestAnimation());

        // Update visual
        UpdateVisuals();

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
        Vector3 smallScale = original * 0.5f; // User said oversized, let's make it smaller when harvested
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
        UpdateVisuals();

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
        main.startColor = new Color(1f, 0.9f, 0.2f, 1f);
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
        DestroyTemporaryObject(particles, 1f);
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
        DestroyTemporaryObject(sparkle, 0.8f);
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

    private void EnsureProgressBar()
    {
        if (progressBarRenderer != null) return;

        // Try to find in children
        Transform bar = transform.Find("ProgressBar");
        if (bar != null)
        {
            progressBarRenderer = bar.Find("FillPivot/Fill")?.GetComponent<SpriteRenderer>();
            progressBarPivot = bar.Find("FillPivot")?.transform;
            return;
        }

        // Create dynamically
        GameObject root = new GameObject("ProgressBar");
        root.transform.SetParent(transform);
        root.transform.localPosition = new Vector3(0, 1.2f, 0);

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(root.transform);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(1.2f, 0.15f, 1);
        SpriteRenderer bgSr = bg.AddComponent<SpriteRenderer>();
        bgSr.color = new Color(0, 0, 0, 0.5f);
        bgSr.sortingOrder = 50;

        GameObject fillRoot = new GameObject("FillPivot");
        fillRoot.transform.SetParent(root.transform);
        fillRoot.transform.localPosition = new Vector3(-0.6f, 0, 0);
        progressBarPivot = fillRoot.transform;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillRoot.transform);
        fill.transform.localPosition = new Vector3(0.6f, 0, 0);
        fill.transform.localScale = new Vector3(1, 0.15f, 1);
        progressBarRenderer = fill.AddComponent<SpriteRenderer>();
        progressBarRenderer.color = new Color(1f, 0.8f, 0.2f); // Golden yellow for corn
        progressBarRenderer.sortingOrder = 51;

        root.SetActive(false);
    }

    // --- IZoneMember Implementation ---
    public void Initialize(string zoneID, int slotIndex)
    {
        // Simple mapping: if in Wheat zone, produce Wheat
        if (zoneID == "Wheat") productId = "Wheat";
        else productId = "Corn";
        
        // Match name for identification
        gameObject.name = $"{zoneID}_{slotIndex}";
    }
}
