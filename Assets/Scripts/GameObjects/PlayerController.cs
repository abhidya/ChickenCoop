using UnityEngine;
using System.Collections;

/// <summary>
/// PlayerController - Controls the player's farmer character movement and interactions.
/// Handles tap/click input to move between farm locations and interact with objects.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float tweenDuration = 0.5f;

    [Header("Animation Settings")]
    [SerializeField] private float bobAmount = 0.1f;
    [SerializeField] private float bobSpeed = 8f;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    // Movement state
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float bobTimer = 0f;
    private Vector3 originalScale;

    // Current interaction target
    private IInteractable currentTarget;

    private void Start()
    {
        targetPosition = transform.position;
        originalScale = transform.localScale;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        HandleInput();
        UpdateMovement();
        UpdateAnimation();
    }

    /// <summary>
    /// Handle mouse/touch input for movement
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            // Check for interactable at click position
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null)
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    MoveToAndInteract(hit.collider.transform.position, interactable);
                    return;
                }
            }

            // If no interactable, just move to position
            MoveTo(mousePos);
        }
    }

    /// <summary>
    /// Move to a position and then interact with target
    /// </summary>
    public void MoveToAndInteract(Vector3 position, IInteractable target)
    {
        currentTarget = target;
        MoveTo(position);
    }

    /// <summary>
    /// Move to a world position using smooth tweening
    /// </summary>
    public void MoveTo(Vector3 position)
    {
        targetPosition = position;
        targetPosition.z = transform.position.z;
        isMoving = true;

        // Flip sprite based on movement direction
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = position.x < transform.position.x;
        }

        // Spawn dust puff at start of movement
        SpawnDustPuff();

        // Start tween movement
        StopAllCoroutines();
        StartCoroutine(TweenMove());
    }

    /// <summary>
    /// Smooth tween movement coroutine
    /// </summary>
    private IEnumerator TweenMove()
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        float duration = Vector3.Distance(startPos, targetPosition) / moveSpeed;

        // Set animator to walking if available
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * GameManager.Instance.SpeedMultiplier;
            float t = elapsed / duration;

            // Use smooth step for easing
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPos, targetPosition, t);

            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;

        // Set animator to idle
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }

        // Interact with target if we have one
        if (currentTarget != null)
        {
            currentTarget.Interact();
            currentTarget = null;
        }
    }

    /// <summary>
    /// Update idle bobbing animation
    /// </summary>
    private void UpdateAnimation()
    {
        if (!isMoving)
        {
            // Idle bob animation
            bobTimer += Time.deltaTime * bobSpeed;
            float bob = Mathf.Sin(bobTimer) * bobAmount * 0.2f;
            transform.localScale = originalScale + new Vector3(0, bob, 0);
        }
        else
        {
            // Walk bob animation
            bobTimer += Time.deltaTime * bobSpeed * 2f;
            float bob = Mathf.Abs(Mathf.Sin(bobTimer)) * bobAmount;
            transform.localScale = originalScale + new Vector3(0, bob, 0);
        }
    }

    /// <summary>
    /// Update movement towards target
    /// </summary>
    private void UpdateMovement()
    {
        // Movement is handled by coroutine now
    }

    /// <summary>
    /// Spawn a dust puff particle effect
    /// </summary>
    private void SpawnDustPuff()
    {
        // Create a simple dust effect
        GameObject dustPuff = new GameObject("DustPuff");
        dustPuff.transform.position = transform.position - new Vector3(0, 0.3f, 0);

        ParticleSystem ps = dustPuff.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.2f;
        main.startLifetime = 0.5f;
        main.startColor = new Color(0.8f, 0.7f, 0.6f, 0.5f);
        main.startSpeed = 0.5f;
        main.gravityModifier = -0.1f;
        main.maxParticles = 5;
        main.duration = 0.2f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 5) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;

        ps.Play();
        Destroy(dustPuff, 1f);
    }

    /// <summary>
    /// Play squash and stretch animation
    /// </summary>
    public void PlaySquashStretch()
    {
        StartCoroutine(SquashStretchAnimation());
    }

    private IEnumerator SquashStretchAnimation()
    {
        Vector3 original = originalScale;
        Vector3 squash = new Vector3(original.x * 1.2f, original.y * 0.8f, original.z);
        Vector3 stretch = new Vector3(original.x * 0.9f, original.y * 1.1f, original.z);

        // Squash
        float t = 0;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(original, squash, t / 0.1f);
            yield return null;
        }

        // Stretch
        t = 0;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(squash, stretch, t / 0.1f);
            yield return null;
        }

        // Return to normal
        t = 0;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(stretch, original, t / 0.1f);
            yield return null;
        }

        transform.localScale = original;
    }
}

/// <summary>
/// Interface for interactable objects in the game
/// </summary>
public interface IInteractable
{
    void Interact();
    bool CanInteract();
}
