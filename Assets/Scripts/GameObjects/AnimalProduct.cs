using UnityEngine;
using ChickenCoop.Interfaces;
using ChickenCoop.Managers;

public class AnimalProduct : MonoBehaviour, IInteractable, IFeedable, IZoneMember
{
    [SerializeField] private string productId = "Milk";
    [SerializeField] private float productionTime = 12f;

    [Header("Visuals")]
    [SerializeField] private string pickupVisualResourcePath = "Prop_Bucket_01";
    
    private float productionTimer = 0f;
    private bool isHungry = false;
    private bool isProducing = false;
    private string zoneId;
    private int slotIndex;
    private Vector3 baseScale = Vector3.one;
    
    public bool CanInteract() => !isHungry && !isProducing && productionTimer <= 0f;
    public bool NeedsFeeding() => isHungry;
    public float GetProductionProgress() => productionTimer / productionTime;
    public bool CanAcceptFood(string itemID) => isHungry && string.Equals(itemID, "Wheat", System.StringComparison.OrdinalIgnoreCase);

    public void ApplyVisualState(ProductVisualState state)
    {
        if (state == null)
        {
            return;
        }

        Vector3 appliedBase = baseScale == Vector3.zero ? transform.localScale : baseScale;
        transform.localScale = Vector3.Scale(appliedBase, state.localScale);

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = state.tint;
        }
    }
    
    public void Initialize(string outputId, float prodTime)
    {
        productId = outputId;
        productionTime = prodTime;
        productionTimer = productionTime; // Start ready to produce
        isProducing = true;
    }
    
    public void Initialize(string zone, int index)
    {
        zoneId = zone;
        slotIndex = index;
        baseScale = transform.localScale;
    }
    
    private void Update()
    {
        if (isProducing && !isHungry)
        {
            float speedMultiplier = GameManager.Instance != null ? GameManager.Instance.SpeedMultiplier : 1f;
            productionTimer -= Time.deltaTime * speedMultiplier;
            if (productionTimer <= 0)
            {
                productionTimer = 0;
                isProducing = false;
                SpawnReadySparkle();
            }
        }
    }
    
    public void Interact()
    {
        if (isHungry) return;
        if (productionTimer > 0) return;

        SpawnPickup();
        productionTimer = productionTime;
        isProducing = true;
    }
    
    public void Feed(string itemID)
    {
        if (!CanAcceptFood(itemID)) return;
        isHungry = false;
        isProducing = true;
        productionTimer = productionTime;
        
        // Visual feedback
        SpawnHeartEffect();
    }
    
    private void SpawnHeartEffect()
    {
        GameObject heart = new GameObject("Heart");
        heart.transform.position = transform.position + Vector3.up * 1.5f;
        var sr = heart.AddComponent<SpriteRenderer>();
        sr.color = Color.red;
        heart.transform.localScale = Vector3.one * 0.5f;
        Destroy(heart, 0.5f);
    }

    private void SpawnReadySparkle()
    {
        GameObject sparkle = new GameObject("ProductReadySparkle");
        sparkle.transform.position = transform.position + Vector3.up * 0.8f;
        ParticleSystem ps = sparkle.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.08f;
        main.startLifetime = 0.4f;
        main.startColor = new Color(1f, 0.96f, 0.7f);
        main.startSpeed = 0.7f;
        main.gravityModifier = -0.1f;
        main.maxParticles = 8;
        main.duration = 0.1f;
        main.loop = false;
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 8) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.12f;
        ps.Play();
        Destroy(sparkle, 0.8f);
    }

    private void SpawnPickup()
    {
        Vector3 spawnPos = transform.position + new Vector3(0.25f, 0.1f, 0f);
        GameObject pickup = new GameObject(productId);
        pickup.transform.position = spawnPos;

        SpriteRenderer sr = pickup.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 25;
        sr.color = Color.white;

        GameObject visualPrefab = null;
        if (!string.IsNullOrWhiteSpace(pickupVisualResourcePath))
        {
            visualPrefab = Resources.Load<GameObject>(pickupVisualResourcePath);
        }

        if (visualPrefab != null)
        {
            StoryVisualBinder.AttachVisualPrefab(pickup.transform, visualPrefab, sr, true);
        }
        else
        {
            sr.sprite = CreateFallbackPickupSprite();
            sr.color = new Color(0.96f, 0.95f, 0.95f);
        }

        CircleCollider2D col = pickup.AddComponent<CircleCollider2D>();
        col.radius = 0.35f;
        col.isTrigger = true;

        CollectibleItem collectible = pickup.AddComponent<CollectibleItem>();
        collectible.SetItemId(productId);

        VisualProgressionController.Instance?.ApplyCurrentStyleToCollectible(collectible);
    }

    private static Sprite CreateFallbackPickupSprite()
    {
        Texture2D texture = new Texture2D(28, 20, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(13.5f, 10f);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float dx = (x - center.x) / 10f;
                float dy = (y - center.y) / 8f;
                texture.SetPixel(x, y, dx * dx + dy * dy <= 1f ? Color.white : Color.clear);
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 24f);
    }
}
