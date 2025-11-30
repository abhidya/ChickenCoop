using UnityEngine;
using System.Collections;

/// <summary>
/// HelperAI - Automated helper character that performs the game loop:
/// Harvest corn -> Feed chicken -> Collect egg -> Sell at store
/// Uses simple state machine and tweened movement between task points.
/// </summary>
public class HelperAI : MonoBehaviour
{
    // Helper states for the automation loop
    public enum HelperState
    {
        Idle,
        MovingToCorn,
        HarvestingCorn,
        MovingToChicken,
        FeedingChicken,
        CollectingEgg,
        MovingToStore,
        SellingEgg,
        Waiting
    }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float waitTime = 1f;

    [Header("Animation Settings")]
    [SerializeField] private float bobAmount = 0.15f;
    [SerializeField] private float bobSpeed = 10f;
    [SerializeField] private Color helperColor = new Color(0.9f, 0.8f, 0.6f);

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    // State machine
    private HelperState currentState = HelperState.Idle;
    private Vector3 targetPosition;
    private bool isMoving = false;

    // Animation
    private float bobTimer = 0f;
    private Vector3 originalScale;

    // Helper ID for visual distinction
    private int helperId;
    private static int helperCounter = 0;

    private void Start()
    {
        helperId = helperCounter++;
        originalScale = transform.localScale;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Give each helper a slightly different color tint
        if (spriteRenderer != null)
        {
            float hueOffset = (helperId * 0.15f) % 1f;
            Color.RGBToHSV(helperColor, out float h, out float s, out float v);
            spriteRenderer.color = Color.HSVToRGB((h + hueOffset) % 1f, s, v);
        }

        // Start the helper loop after a small delay
        StartCoroutine(StartHelperLoop());
    }

    private void Update()
    {
        UpdateAnimation();
    }

    /// <summary>
    /// Main helper automation loop
    /// </summary>
    private IEnumerator StartHelperLoop()
    {
        // Initial random delay to stagger helpers
        yield return new WaitForSeconds(Random.Range(0.5f, 2f));

        while (true)
        {
            // Step 1: Go to corn field and harvest
            yield return StartCoroutine(GoToAndHarvestCorn());

            // Step 2: Go to chicken and feed
            yield return StartCoroutine(GoToAndFeedChicken());

            // Step 3: Collect the egg (happens at chicken location)
            yield return StartCoroutine(CollectEgg());

            // Step 4: Go to store and sell
            yield return StartCoroutine(GoToAndSellEgg());

            // Short wait before next loop
            yield return new WaitForSeconds(waitTime / GameManager.Instance.SpeedMultiplier);
        }
    }

    /// <summary>
    /// Move to corn field and harvest corn
    /// </summary>
    private IEnumerator GoToAndHarvestCorn()
    {
        currentState = HelperState.MovingToCorn;

        if (GameManager.Instance.CornFieldPosition != null)
        {
            yield return StartCoroutine(MoveTo(GameManager.Instance.CornFieldPosition.position));
        }

        currentState = HelperState.HarvestingCorn;

        // Play harvest animation
        PlaySquashStretch();
        SpawnDustPuff();

        yield return new WaitForSeconds(0.5f / GameManager.Instance.SpeedMultiplier);

        // Add corn to inventory
        GameManager.Instance.AddCorn(1);

        yield return new WaitForSeconds(0.3f / GameManager.Instance.SpeedMultiplier);
    }

    /// <summary>
    /// Move to chicken and feed it
    /// </summary>
    private IEnumerator GoToAndFeedChicken()
    {
        currentState = HelperState.MovingToChicken;

        if (GameManager.Instance.ChickenPosition != null)
        {
            yield return StartCoroutine(MoveTo(GameManager.Instance.ChickenPosition.position));
        }

        currentState = HelperState.FeedingChicken;

        // Use corn to feed chicken
        if (GameManager.Instance.UseCorn(1))
        {
            PlaySquashStretch();
            yield return new WaitForSeconds(0.5f / GameManager.Instance.SpeedMultiplier);

            // Trigger chicken feeding (if chicken script exists)
            Chicken chicken = FindObjectOfType<Chicken>();
            if (chicken != null)
            {
                chicken.Feed();
            }
        }

        yield return new WaitForSeconds(0.3f / GameManager.Instance.SpeedMultiplier);
    }

    /// <summary>
    /// Collect egg from chicken
    /// </summary>
    private IEnumerator CollectEgg()
    {
        currentState = HelperState.CollectingEgg;

        PlaySquashStretch();
        yield return new WaitForSeconds(0.3f / GameManager.Instance.SpeedMultiplier);

        // Add egg to inventory
        GameManager.Instance.AddEgg(1);

        SpawnSparkle();

        yield return new WaitForSeconds(0.3f / GameManager.Instance.SpeedMultiplier);
    }

    /// <summary>
    /// Move to store and sell egg
    /// </summary>
    private IEnumerator GoToAndSellEgg()
    {
        currentState = HelperState.MovingToStore;

        if (GameManager.Instance.StorePosition != null)
        {
            yield return StartCoroutine(MoveTo(GameManager.Instance.StorePosition.position));
        }

        currentState = HelperState.SellingEgg;

        PlaySquashStretch();
        yield return new WaitForSeconds(0.3f / GameManager.Instance.SpeedMultiplier);

        // Sell egg at store
        GameManager.Instance.SellEgg();

        yield return new WaitForSeconds(0.5f / GameManager.Instance.SpeedMultiplier);

        currentState = HelperState.Idle;
    }

    /// <summary>
    /// Smooth movement to target position
    /// </summary>
    private IEnumerator MoveTo(Vector3 position)
    {
        targetPosition = position;
        targetPosition.z = transform.position.z;
        isMoving = true;

        // Flip sprite based on direction
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = position.x < transform.position.x;
        }

        SpawnDustPuff();

        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, targetPosition);
        float duration = distance / (moveSpeed * GameManager.Instance.SpeedMultiplier);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smooth step easing
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPos, targetPosition, t);

            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }

    /// <summary>
    /// Update bobbing animation
    /// </summary>
    private void UpdateAnimation()
    {
        bobTimer += Time.deltaTime * bobSpeed;

        if (isMoving)
        {
            // Walking bob - more pronounced
            float bob = Mathf.Abs(Mathf.Sin(bobTimer * 2f)) * bobAmount;
            transform.localScale = originalScale + new Vector3(0, bob, 0);
        }
        else
        {
            // Idle bob - gentle
            float bob = Mathf.Sin(bobTimer) * bobAmount * 0.3f;
            transform.localScale = originalScale + new Vector3(0, bob, 0);
        }
    }

    /// <summary>
    /// Play squash and stretch animation
    /// </summary>
    private void PlaySquashStretch()
    {
        StartCoroutine(SquashStretchAnimation());
    }

    private IEnumerator SquashStretchAnimation()
    {
        Vector3 original = originalScale;
        Vector3 squash = new Vector3(original.x * 1.3f, original.y * 0.7f, original.z);
        Vector3 stretch = new Vector3(original.x * 0.85f, original.y * 1.15f, original.z);

        float t = 0;
        while (t < 0.08f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(original, squash, t / 0.08f);
            yield return null;
        }

        t = 0;
        while (t < 0.08f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(squash, stretch, t / 0.08f);
            yield return null;
        }

        t = 0;
        while (t < 0.08f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(stretch, original, t / 0.08f);
            yield return null;
        }

        transform.localScale = original;
    }

    /// <summary>
    /// Spawn dust puff effect
    /// </summary>
    private void SpawnDustPuff()
    {
        GameObject dustPuff = new GameObject("DustPuff");
        dustPuff.transform.position = transform.position - new Vector3(0, 0.25f, 0);

        ParticleSystem ps = dustPuff.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.15f;
        main.startLifetime = 0.4f;
        main.startColor = new Color(0.85f, 0.75f, 0.65f, 0.4f);
        main.startSpeed = 0.3f;
        main.gravityModifier = -0.05f;
        main.maxParticles = 3;
        main.duration = 0.1f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 3) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.15f;

        ps.Play();
        Destroy(dustPuff, 0.8f);
    }

    /// <summary>
    /// Spawn sparkle effect for collection
    /// </summary>
    private void SpawnSparkle()
    {
        GameObject sparkle = new GameObject("Sparkle");
        sparkle.transform.position = transform.position;

        ParticleSystem ps = sparkle.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.1f;
        main.startLifetime = 0.5f;
        main.startColor = new Color(1f, 1f, 0.6f, 1f);
        main.startSpeed = 1f;
        main.gravityModifier = -0.5f;
        main.maxParticles = 8;
        main.duration = 0.1f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 8) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        ps.Play();
        Destroy(sparkle, 1f);
    }
}
