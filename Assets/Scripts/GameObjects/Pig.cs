using UnityEngine;
using System.Collections;
using ChickenCoop.Managers;
using ChickenCoop.Interfaces;

public class Pig : MonoBehaviour, IInteractable, IFeedable, IZoneMember
{
    private const string RuntimePigVisualResourcePath = "HappyHarvestPig";

    [Header("Production")]
    [SerializeField] private int carrotsRequired = 2;
    [SerializeField] private float productionTime = 15f;

    [Header("Animation")]
    [SerializeField] private float wanderInterval = 6f;
    [SerializeField] private float wanderSpeed = 0.8f;
    [SerializeField] private float wiggleAmount = 3f;
    [SerializeField] private float wiggleSpeed = 2f;
    [SerializeField] private float bobAmount = 0.04f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer bodySprite;
    [SerializeField] private SpriteRenderer progressBarRenderer;

    private bool isProducing = false;
    private float productionTimer = 0f;
    private float productionProgress = 0f;
    private bool isWandering = false;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private float wiggleTimer = 0f;
    private Transform progressBarPivot;
    private FarmZoneController zoneController;

    private void Start()
    {
        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
        zoneController = GetComponentInParent<FarmZoneController>();

        if (bodySprite == null)
            bodySprite = GetComponent<SpriteRenderer>();

        CircleCollider2D hitbox = GetComponent<CircleCollider2D>();
        if (hitbox == null)
            hitbox = gameObject.AddComponent<CircleCollider2D>();
        hitbox.isTrigger = true;
        if (hitbox.radius < 0.9f)
            hitbox.radius = 0.9f;

        Transform existingVisual = transform.Find("Visual");
        if (existingVisual == null)
        {
            GameObject visualPrefab = Resources.Load<GameObject>(RuntimePigVisualResourcePath);
            if (visualPrefab != null)
            {
                StoryVisualBinder.AttachVisualPrefab(transform, visualPrefab, bodySprite);
            }
        }
        else if (bodySprite != null)
        {
            bodySprite.enabled = false;
        }

        EnsureProgressBar();
        StartCoroutine(WanderLoop());
    }

    private void Update()
    {
        wiggleTimer += Time.deltaTime * wiggleSpeed;
        float wiggle = Mathf.Sin(wiggleTimer) * wiggleAmount * 0.3f;
        transform.localRotation = originalRotation * Quaternion.Euler(0, 0, wiggle);

        if (!isProducing)
        {
            float bob = Mathf.Sin(Time.time * 1.5f) * bobAmount;
            transform.localScale = originalScale + new Vector3(0, bob, 0);
        }

        if (isProducing)
        {
            float speed = GameManager.Instance != null ? GameManager.Instance.SpeedMultiplier : 1f;
            productionTimer -= Time.deltaTime * speed;
            productionProgress = 1f - (productionTimer / productionTime);
            UpdateProgressBar();

            if (productionTimer <= 0)
            {
                productionTimer = 0;
                productionProgress = 0f;
                isProducing = false;
                SpawnTruffle();
                HideProgressBar();
            }
        }
    }

    public void Interact()
    {
        if (CanInteract())
            Feed();
    }

    public bool CanInteract()
    {
        return !isProducing && GameManager.Instance != null &&
               GameManager.Instance.GetItemCount("Carrot") >= carrotsRequired;
    }

    public void Feed()
    {
        if (isProducing || GameManager.Instance == null) return;

        if (GameManager.Instance.GetItemCount("Carrot") < carrotsRequired) return;
        GameManager.Instance.RemoveItem("Carrot", carrotsRequired);

        isProducing = true;
        productionTimer = productionTime;
        productionProgress = 0.1f;
        ShowProgressBar();

        SpawnHeartEffect();
        AudioManager.Instance?.PlaySound("eat");
    }

    public void Feed(string itemID)
    {
        Feed();
    }

    public bool NeedsFeeding() => !isProducing;
    public bool CanAcceptFood(string itemID) => !isProducing && itemID == "Carrot";
    public float GetProductionProgress() => productionProgress;

    public void Initialize(string zoneID, int slotIndex)
    {
        gameObject.name = $"{zoneID}_{slotIndex}";
    }

    private void SpawnTruffle()
    {
        Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), -0.3f, 0);

        GameObject truffle = new GameObject("Truffle");
        truffle.transform.position = spawnPos;

        SpriteRenderer sr = truffle.AddComponent<SpriteRenderer>();
        sr.sprite = CreateTruffleSprite();
        sr.color = new Color(0.4f, 0.3f, 0.2f);
        sr.sortingOrder = 25;

        CircleCollider2D col = truffle.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;
        col.isTrigger = true;

        CollectibleEgg collector = truffle.AddComponent<CollectibleEgg>();
        collector.SetItemId("Truffle");

        StartCoroutine(BounceIn(truffle.transform, spawnPos));
        SpawnTruffleParticles(spawnPos);
    }

    private static Sprite CreateTruffleSprite()
    {
        Texture2D tex = new Texture2D(24, 24, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(11.5f, 11.5f);
        float radius = 9f;
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                float dx = (x - center.x) / radius;
                float dy = (y - center.y) / radius;
                float d = dx * dx + dy * dy;
                tex.SetPixel(x, y, d <= 1f ? new Color(0.35f, 0.25f, 0.15f) : Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 24f);
    }

    private IEnumerator BounceIn(Transform target, Vector3 pos)
    {
        if (target == null) yield break;
        Vector3 start = pos + Vector3.up * 0.3f;
        target.position = start;
        target.localScale = Vector3.zero;
        float scale = 0.5f;
        float t = 0;
        while (t < 0.15f)
        {
            if (target == null) yield break;
            t += Time.deltaTime;
            target.localScale = Vector3.one * Mathf.Min(Mathf.Sin(t / 0.15f * Mathf.PI / 2f) * scale * 1.2f, scale);
            target.position = Vector3.Lerp(start, pos, t / 0.15f);
            yield return null;
        }
        if (target != null) target.localScale = Vector3.one * scale;
    }

    private void SpawnTruffleParticles(Vector3 pos)
    {
        GameObject ps = new GameObject("TruffleParticles");
        ps.transform.position = pos;
        ParticleSystem particles = ps.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startSize = 0.08f;
        main.startLifetime = 0.4f;
        main.startColor = new Color(0.6f, 0.4f, 0.2f);
        main.startSpeed = 1f;
        main.gravityModifier = 0.3f;
        main.maxParticles = 6;
        main.duration = 0.1f;
        main.loop = false;
        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 6) });
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.15f;
        ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();
        if (psr != null) psr.material = new Material(Shader.Find("Sprites/Default"));
        particles.Play();
        Destroy(ps, 1f);
    }

    private void SpawnHeartEffect()
    {
        GameObject hearts = new GameObject("FeedHearts");
        hearts.transform.position = transform.position + new Vector3(0, 0.5f, 0);
        ParticleSystem ps = hearts.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.2f;
        main.startLifetime = 1f;
        main.startColor = new Color(1f, 0.4f, 0.6f);
        main.startSpeed = 0.8f;
        main.gravityModifier = -0.2f;
        main.maxParticles = 3;
        main.duration = 0.3f;
        main.loop = false;
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 3) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.15f;
        ParticleSystemRenderer psr = hearts.GetComponent<ParticleSystemRenderer>();
        if (psr != null) psr.material = new Material(Shader.Find("Sprites/Default"));
        ps.Play();
        Destroy(hearts, 1.5f);
    }

    private void EnsureProgressBar()
    {
        if (progressBarRenderer != null) return;
        Transform bar = transform.Find("ProgressBar");
        if (bar != null)
        {
            progressBarRenderer = bar.Find("Fill")?.GetComponent<SpriteRenderer>();
            progressBarPivot = bar.Find("Fill")?.transform;
            return;
        }
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
        progressBarRenderer.color = new Color(0.6f, 0.3f, 0.1f);
        progressBarRenderer.sortingOrder = 51;
        root.SetActive(false);
    }

    private void ShowProgressBar()
    {
        Transform bar = transform.Find("ProgressBar");
        if (bar != null) bar.gameObject.SetActive(true);
    }

    private void HideProgressBar()
    {
        Transform bar = transform.Find("ProgressBar");
        if (bar != null) bar.gameObject.SetActive(false);
    }

    private void UpdateProgressBar()
    {
        if (progressBarPivot != null)
            progressBarPivot.localScale = new Vector3(productionProgress, 1, 1);
    }

    private IEnumerator WanderLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(wanderInterval * 0.5f, wanderInterval * 1.5f);
            yield return new WaitForSeconds(waitTime);
            if (!isProducing && !isWandering && zoneController != null)
            {
                Bounds bounds = zoneController.GetZoneBounds();
                bounds.Expand(-1.4f);
                if (bounds.size.x < 1f || bounds.size.y < 1f) bounds = zoneController.GetZoneBounds();
                Vector3 dest = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    transform.position.z);
                yield return StartCoroutine(WanderTo(dest));
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
        Transform visualRoot = StoryVisualBinder.FindAttachedVisualRoot(transform);
        if (visualRoot != null)
            StoryVisualBinder.SetFacing(visualRoot, destination.x < startPos.x);
        while (elapsed < duration)
        {
            if (isProducing) break;
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, destination, elapsed / duration);
            yield return null;
        }
        isWandering = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && CanInteract())
            Interact();
    }
}
