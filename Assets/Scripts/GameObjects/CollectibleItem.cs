using UnityEngine;
using System.Collections;
using ChickenCoop.Managers;

/// <summary>
/// Generic collectible item for eggs, milk, truffles, and future products.
/// </summary>
public class CollectibleItem : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemId = "Egg";
    private bool isCollected = false;
    private const float CollectionDistance = 2.0f;
    private SpriteRenderer[] cachedRenderers;
    private Vector3 baseScale = Vector3.one;
    private bool cachedBaseState;
    private static Sprite fallbackDotSprite;

    public string ItemId => itemId;

    private void Awake()
    {
        CacheBaseVisualState();
        VisualProgressionController.Instance?.ApplyCurrentStyleToCollectible(this);
    }

    public void SetItemId(string newItemId)
    {
        if (!string.IsNullOrEmpty(newItemId))
        {
            itemId = newItemId;
        }

        ApplyCurrentVisualState();
        VisualProgressionController.Instance?.ApplyCurrentStyleToCollectible(this);
    }

    public void ApplyVisualState(ProductVisualState state)
    {
        if (state == null)
        {
            return;
        }

        CacheBaseVisualState();
        transform.localScale = Vector3.Scale(baseScale, state.localScale);

        if (cachedRenderers != null && cachedRenderers.Length > 0)
        {
            foreach (SpriteRenderer renderer in cachedRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                renderer.color = state.tint;
            }
        }

        if (!string.IsNullOrWhiteSpace(state.resourcePath))
        {
            AttachMarker(state.markerName, state.resourcePath, state.tint, state.localOffset, state.localScale);
        }
        else if (!string.IsNullOrWhiteSpace(state.markerName))
        {
            AttachMarker(state.markerName, null, state.tint, state.localOffset, state.localScale);
        }
    }

    public void ApplyVisualTint(Color tint, float scaleMultiplier = 1f, string markerName = null, string resourcePath = null, Vector3? localOffset = null)
    {
        CacheBaseVisualState();
        transform.localScale = baseScale * Mathf.Max(scaleMultiplier, 0.01f);

        if (cachedRenderers != null && cachedRenderers.Length > 0)
        {
            foreach (SpriteRenderer renderer in cachedRenderers)
            {
                if (renderer != null)
                {
                    renderer.color = tint;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(markerName))
        {
            AttachMarker(markerName, resourcePath, tint, localOffset ?? Vector3.zero, Vector3.one);
        }
    }

    private void ApplyCurrentVisualState()
    {
        CacheBaseVisualState();
        if (cachedRenderers == null || cachedRenderers.Length == 0)
        {
            return;
        }

        foreach (SpriteRenderer renderer in cachedRenderers)
        {
            if (renderer != null)
            {
                renderer.color = Color.white;
            }
        }
    }

    private void CacheBaseVisualState()
    {
        if (cachedBaseState)
        {
            return;
        }

        cachedBaseState = true;
        baseScale = transform.localScale;
        cachedRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void AttachMarker(string markerName, string resourcePath, Color tint, Vector3 localOffset, Vector3 localScale)
    {
        if (string.IsNullOrWhiteSpace(markerName))
        {
            return;
        }

        Transform marker = transform.Find(markerName);
        if (marker == null)
        {
            GameObject markerObject = new GameObject(markerName);
            markerObject.transform.SetParent(transform, false);
            marker = markerObject.transform;
        }

        marker.localPosition = localOffset;
        marker.localScale = localScale;

        SpriteRenderer renderer = marker.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = marker.gameObject.AddComponent<SpriteRenderer>();
        }

        if (!string.IsNullOrWhiteSpace(resourcePath))
        {
            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
            {
                renderer.sprite = sprite;
            }
        }

        if (renderer.sprite == null)
        {
            renderer.sprite = CreateFallbackDotSprite();
        }

        renderer.color = tint;
        renderer.sortingOrder = 999;
    }

    private void Update()
    {
        if (isCollected || GameManager.Instance == null || GameManager.Instance.Player == null)
        {
            return;
        }

        float dist = Vector3.Distance(transform.position, GameManager.Instance.Player.transform.position);
        if (dist < CollectionDistance)
        {
            Collect();
        }
    }

    public void Interact()
    {
        if (!isCollected)
        {
            Collect();
        }
    }

    public bool CanInteract() => !isCollected;

    protected virtual void Collect()
    {
        isCollected = true;
        GameManager.Instance.AddItem(itemId, 1, transform.position + Vector3.up * 0.4f);
        StartCoroutine(CollectAnimation());
    }

    private IEnumerator CollectAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        SpawnGlowEffect();

        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float progress = t / 0.3f;
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

    private static Sprite CreateFallbackDotSprite()
    {
        if (fallbackDotSprite != null)
        {
            return fallbackDotSprite;
        }

        Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(7.5f, 7.5f);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float dx = (x - center.x) / 6f;
                float dy = (y - center.y) / 6f;
                texture.SetPixel(x, y, dx * dx + dy * dy <= 1f ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        fallbackDotSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 16f);
        return fallbackDotSprite;
    }

    private void OnMouseDown()
    {
        Interact();
    }
}
