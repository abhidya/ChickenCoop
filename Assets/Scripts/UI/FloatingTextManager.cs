using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// FloatingTextManager - Spawns and animates floating text for resource feedback.
/// Shows "+X 🌽" style text that floats up and fades out.
/// </summary>
public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [Header("Floating Text Settings")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatDuration = 1.5f;
    [SerializeField] private float startScale = 0.5f;
    [SerializeField] private float endScale = 1.2f;
    [SerializeField] private float fontSize = 24f;

    [Header("References")]
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private TMP_FontAsset floatingTextFont;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Create world space canvas if not assigned
        if (worldCanvas == null)
        {
            CreateWorldCanvas();
        }

        // Subscribe to game events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnResourceGained += SpawnFloatingText;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnResourceGained -= SpawnFloatingText;
        }
    }

    /// <summary>
    /// Create a world space canvas for floating text
    /// </summary>
    private void CreateWorldCanvas()
    {
        GameObject canvasObj = new GameObject("FloatingTextCanvas");
        canvasObj.transform.SetParent(transform);

        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingOrder = 100;

        // Scale canvas for world units
        RectTransform rt = canvasObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(10f, 10f);
        rt.localScale = Vector3.one * 0.01f;
    }

    /// <summary>
    /// Spawn floating text at a world position
    /// </summary>
    public void SpawnFloatingText(string text, Vector3 worldPosition, Color color)
    {
        StartCoroutine(FloatingTextCoroutine(text, worldPosition, color));
    }

    /// <summary>
    /// Spawn floating text with default color
    /// </summary>
    public void SpawnFloatingText(string text, Vector3 worldPosition)
    {
        SpawnFloatingText(text, worldPosition, Color.white);
    }

    /// <summary>
    /// Coroutine to animate floating text
    /// </summary>
    private IEnumerator FloatingTextCoroutine(string text, Vector3 startPosition, Color color)
    {
        // Create text object
        GameObject textObj = new GameObject("FloatingText");
        textObj.transform.position = startPosition;

        // Add TextMeshPro component
        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 100;

        // Use assigned font or default
        if (floatingTextFont != null)
        {
            tmp.font = floatingTextFont;
        }

        // Animate
        float elapsed = 0f;
        Vector3 endPosition = startPosition + Vector3.up * floatSpeed * floatDuration;

        while (elapsed < floatDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / floatDuration;

            // Position - float upward
            textObj.transform.position = Vector3.Lerp(startPosition, endPosition, progress);

            // Scale - pop in then shrink slightly
            float scaleProgress = progress < 0.3f 
                ? Mathf.Lerp(startScale, endScale, progress / 0.3f)
                : Mathf.Lerp(endScale, 1f, (progress - 0.3f) / 0.7f);
            textObj.transform.localScale = Vector3.one * scaleProgress;

            // Alpha - fade out in last half
            if (progress > 0.5f)
            {
                float fadeProgress = (progress - 0.5f) / 0.5f;
                tmp.alpha = 1f - fadeProgress;
            }

            yield return null;
        }

        Destroy(textObj);
    }

    /// <summary>
    /// Spawn error text (red, shakes)
    /// </summary>
    public void SpawnErrorText(string text, Vector3 worldPosition)
    {
        StartCoroutine(ErrorTextCoroutine(text, worldPosition));
    }

    /// <summary>
    /// Coroutine for error text with shake effect
    /// </summary>
    private IEnumerator ErrorTextCoroutine(string text, Vector3 startPosition)
    {
        // Create text object
        GameObject textObj = new GameObject("ErrorText");
        textObj.transform.position = startPosition;

        // Add TextMeshPro component
        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.red;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 100;

        if (floatingTextFont != null)
        {
            tmp.font = floatingTextFont;
        }

        // Shake animation
        float elapsed = 0f;
        float shakeDuration = 0.5f;
        float shakeIntensity = 0.1f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;

            // Shake with decreasing intensity
            float shake = shakeIntensity * (1f - progress);
            Vector3 offset = new Vector3(
                Random.Range(-shake, shake),
                Random.Range(-shake, shake),
                0
            );
            textObj.transform.position = startPosition + offset;

            // Fade out
            tmp.alpha = 1f - progress;

            yield return null;
        }

        Destroy(textObj);
    }

    /// <summary>
    /// Spawn bonus text (gold, larger, bounces)
    /// </summary>
    public void SpawnBonusText(string text, Vector3 worldPosition)
    {
        StartCoroutine(BonusTextCoroutine(text, worldPosition));
    }

    /// <summary>
    /// Coroutine for bonus text with bounce effect
    /// </summary>
    private IEnumerator BonusTextCoroutine(string text, Vector3 startPosition)
    {
        // Create text object
        GameObject textObj = new GameObject("BonusText");
        textObj.transform.position = startPosition;

        // Add TextMeshPro component
        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize * 1.5f;
        tmp.color = new Color(1f, 0.85f, 0.2f); // Gold
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 100;

        if (floatingTextFont != null)
        {
            tmp.font = floatingTextFont;
        }

        // Bounce animation
        float elapsed = 0f;
        float duration = 2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Position - float up with bounce
            float yOffset = Mathf.Sin(progress * Mathf.PI) * 1.5f;
            textObj.transform.position = startPosition + new Vector3(0, yOffset, 0);

            // Scale - pulse
            float scale = 1f + Mathf.Sin(progress * Mathf.PI * 4f) * 0.2f;
            textObj.transform.localScale = Vector3.one * scale;

            // Fade out in last third
            if (progress > 0.66f)
            {
                float fadeProgress = (progress - 0.66f) / 0.34f;
                tmp.alpha = 1f - fadeProgress;
            }

            yield return null;
        }

        Destroy(textObj);
    }
}
