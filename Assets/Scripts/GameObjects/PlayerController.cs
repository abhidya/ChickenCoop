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

    [Header("Animation Settings")]
    [SerializeField] private float bobAmount = 0.1f;
    [SerializeField] private float bobSpeed = 8f;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject storyVisualPrefab;
    [SerializeField] private string happyHarvestFarmerLibraryResourcePath = "HappyHarvestFarmer";

    // Movement state
    private Vector3 targetPosition;
    private bool isMoving = false;
    private float bobTimer = 0f;
    private Vector3 originalScale;
    private Transform happyHarvestVisualRoot;
    private static readonly Vector3 HappyHarvestVisualOffset = new Vector3(0f, -0.35f, 0f);
    private const float HappyHarvestVisualScale = 0.45f;
    private Vector2 lastMoveDirection = Vector2.down;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DirXHash = Animator.StringToHash("DirX");
    private static readonly int DirYHash = Animator.StringToHash("DirY");

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

        ApplyHappyHarvestVisual();
        UpdateAnimatorParameters(Vector2.zero, false);
    }

    private void Update()
    {
        HandleInput();
        UpdateMovement();
        UpdateAnimation();
    }

    private void LateUpdate()
    {
        SyncHappyHarvestVisualTransform();
    }

    /// <summary>
    /// Safely check if a pointer is strictly over a UI Button to bypass broken WebGL background canvases
    /// </summary>
    private bool IsPointerOverUIButton()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;
        
        UnityEngine.EventSystems.PointerEventData eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        eventData.position = Input.touchCount > 0 ? Input.touches[0].position : Input.mousePosition;

        var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            if (result.gameObject.GetComponentInParent<UnityEngine.UI.Button>() != null)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Handle mouse/touch input for movement
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Block world clicks if we're clicking on a UI element (like the Tutorial panel or Next buttons)
            if (IsPointerOverUIButton())
            {
                return;
            }

            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Debug.Log($"[Player] Moving to world pos {mousePos}");

            // Check for interactable at click position
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null)
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    Debug.Log($"[Player] Interacting with {hit.collider.gameObject.name}");
                    MoveToAndInteract(hit.collider.transform.position, interactable);
                    return;
                }
            }

            // Move to clicked world position
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
        Vector2 plannedMove = targetPosition - transform.position;
        if (plannedMove.sqrMagnitude > 0.0001f)
        {
            lastMoveDirection = plannedMove.normalized;
        }

        // Flip sprite based on movement direction
        if (spriteRenderer != null)
        {
            bool faceLeft = position.x < transform.position.x;
            spriteRenderer.flipX = faceLeft;
            StoryVisualBinder.SetFacing(happyHarvestVisualRoot, faceLeft);
        }

        // Spawn dust puff at start of movement
        SpawnDustPuff();

        // Start tween movement
        StopAllCoroutines();
        UpdateAnimatorParameters(plannedMove, true);
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

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * GameManager.Instance.SpeedMultiplier;
            float t = elapsed / duration;

            // Use smooth step for easing
            t = t * t * (3f - 2f * t);

            Vector3 nextPosition = Vector3.Lerp(startPos, targetPosition, t);
            Vector2 moveDelta = nextPosition - transform.position;
            transform.position = nextPosition;
            UpdateAnimatorParameters(moveDelta, true);

            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
        UpdateAnimatorParameters(Vector2.zero, false);

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

    private void ApplyHappyHarvestVisual()
    {
        // Resolve SpriteRenderer if not set via Inspector
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            }
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.enabled = false;
                spriteRenderer.sortingOrder = 10;
            }
        }

        GameObject visualInstance = FindExistingHappyHarvestVisual();

        if (visualInstance == null)
        {
            // Resolve visual prefab — serialized field first, then Resources
            GameObject visualPrefab = storyVisualPrefab;
            if (visualPrefab == null)
            {
                visualPrefab = Resources.Load<GameObject>("Character");
            }

            if (visualPrefab == null)
            {
                Debug.LogWarning("[PlayerController] Could not find Character prefab via Inspector or Resources.");
                return;
            }

            // Use AttachVisualPrefabAsChild with preserveRigComponents=true so SpriteSkin/U2D
            // bone deformation components are preserved — required for eyes/face to render correctly.
            visualInstance = StoryVisualBinder.AttachVisualPrefabAsChild(
                transform, visualPrefab, spriteRenderer, "CharacterVisual", true);
        }

        if (visualInstance != null)
        {
            // The cleanest way to ensure dynamically instantiated characters draw over all static
            // structures (which may contain their own SortingGroups) is applying a master SortingGroup.
            UnityEngine.Rendering.SortingGroup sGroup = gameObject.GetComponent<UnityEngine.Rendering.SortingGroup>();
            if (sGroup == null) sGroup = gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
            sGroup.sortingOrder = 5000;
        }

        if (visualInstance == null)
        {
            return;
        }

        happyHarvestVisualRoot = visualInstance.transform;
        if (happyHarvestVisualRoot.parent != transform)
        {
            happyHarvestVisualRoot.SetParent(transform, false);
        }

        StoryVisualBinder.ApplySpriteLibrary(visualInstance, happyHarvestFarmerLibraryResourcePath);
        happyHarvestVisualRoot.localScale = Vector3.one * HappyHarvestVisualScale;
        happyHarvestVisualRoot.localPosition = HappyHarvestVisualOffset;

        if (animator == null)
        {
            animator = visualInstance.GetComponent<Animator>();
            if (animator == null)
            {
                animator = visualInstance.GetComponentInChildren<Animator>(true);
            }
        }

        if (spriteRenderer != null)
        {
            StoryVisualBinder.SetFacing(happyHarvestVisualRoot, spriteRenderer.flipX);
        }
    }

    private GameObject FindExistingHappyHarvestVisual()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            string lowered = child.name.ToLowerInvariant();
            if (lowered == "visual" || lowered == "charactervisual" || lowered.Contains("visual"))
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private void SyncHappyHarvestVisualTransform()
    {
        if (happyHarvestVisualRoot == null)
        {
            return;
        }

        if (happyHarvestVisualRoot.parent != transform)
        {
            happyHarvestVisualRoot.SetParent(transform, false);
        }

        happyHarvestVisualRoot.localPosition = HappyHarvestVisualOffset;

        Vector3 scale = happyHarvestVisualRoot.localScale;
        float facing = scale.x < 0f ? -1f : 1f;
        happyHarvestVisualRoot.localScale = new Vector3(
            HappyHarvestVisualScale * facing,
            HappyHarvestVisualScale,
            HappyHarvestVisualScale);
    }

    private void UpdateAnimatorParameters(Vector2 moveDelta, bool movingNow)
    {
        if (animator == null)
        {
            return;
        }

        if (moveDelta.sqrMagnitude > 0.0001f)
        {
            lastMoveDirection = moveDelta.normalized;
        }

        if (HasAnimatorParameter(animator, IsWalkingHash))
        {
            animator.SetBool(IsWalkingHash, movingNow);
        }

        if (HasAnimatorParameter(animator, SpeedHash))
        {
            animator.SetFloat(SpeedHash, movingNow ? 1f : 0f);
        }

        if (HasAnimatorParameter(animator, DirXHash))
        {
            animator.SetFloat(DirXHash, lastMoveDirection.x);
        }

        if (HasAnimatorParameter(animator, DirYHash))
        {
            animator.SetFloat(DirYHash, lastMoveDirection.y);
        }
    }

    private static bool HasAnimatorParameter(Animator targetAnimator, int hash)
    {
        if (targetAnimator == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in targetAnimator.parameters)
        {
            if (parameter.nameHash == hash)
            {
                return true;
            }
        }

        return false;
    }
}
