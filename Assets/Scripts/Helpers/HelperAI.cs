using UnityEngine;
using ChickenCoop.Managers;
using ChickenCoop.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// HelperAI - Automated helper character that performs a generic game loop:
/// - Find closest ready IHarvestable -> Harvest
/// - Find closest hungry IFeedable -> Feed (if inventory permits)
/// - Collect ground items -> Sell at Market
/// </summary>
public class HelperAI : MonoBehaviour
{
    public enum HelperState { Idle, Moving, Harvesting, Feeding, Selling, Waiting }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float waitTime = 1f;

    [Header("Animation Settings")]
    [SerializeField] private float bobAmount = 0.15f;
    [SerializeField] private float bobSpeed = 10f;
    [SerializeField] private Color helperColor = new Color(0.9f, 0.8f, 0.6f);

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private HelperState currentState = HelperState.Idle;
    private bool isMoving = false;
    private float bobTimer = 0f;
    private Vector3 originalScale;
    private Transform happyHarvestVisualRoot;

    private int helperId;
    private static int helperCounter = 0;

    private void Start()
    {
        helperId = helperCounter++;
        originalScale = transform.localScale;

        GameConfig config = GameManager.Instance != null ? GameManager.Instance.Config : null;
        if (config != null)
        {
            moveSpeed = config.helperSpeed;
            waitTime = config.helperWaitTime;
        }

        spriteRenderer = spriteRenderer ?? GetComponent<SpriteRenderer>();
        happyHarvestVisualRoot = StoryVisualBinder.FindAttachedVisualRoot(transform);

        if (spriteRenderer != null)
        {
            float hueOffset = (helperId * 0.15f) % 1f;
            Color.RGBToHSV(helperColor, out float h, out float s, out float v);
            spriteRenderer.color = Color.HSVToRGB((h + hueOffset) % 1f, s, v);
        }

        StartCoroutine(StartHelperLoop());
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private IEnumerator StartHelperLoop()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 2f));

        while (true)
        {
            yield return StartCoroutine(PerformNextTask());
            float wait = waitTime / Mathf.Max(GameManager.Instance.SpeedMultiplier, 0.01f);
            yield return new WaitForSeconds(wait);
        }
    }

    private IEnumerator PerformNextTask()
    {
        // 1. Collect Ground Items (Highest Priority)
        var collectible = FindClosestInteractable<CollectibleEgg>(); // Still specialized for now
        if (collectible != null)
        {
            yield return StartCoroutine(MoveAndAction(collectible.transform.position, "ITEM", () => collectible.Interact()));
            yield break;
        }

        // 2. Harvest Ready Crops
        var harvestable = FindClosestHarvestable();
        if (harvestable != null)
        {
            yield return StartCoroutine(MoveAndAction(harvestable.transform.position, "WORK", () => harvestable.Harvest()));
            yield break;
        }

        // 3. Feed Hungry Animals
        var feedable = FindClosestFeedable();
        if (feedable != null)
        {
            yield return StartCoroutine(MoveAndAction(feedable.transform.position, "FEED", () => feedable.Feed("Generic")));
            yield break;
        }

        // 4. Sell Inventory
        if (GameManager.Instance.Eggs > 0 || GameManager.Instance.Corn > 10) // Legacy counts for now
        {
            if (GameManager.Instance.StorePosition != null)
            {
                yield return StartCoroutine(MoveAndAction(GameManager.Instance.StorePosition.position, "GOLD", () => {
                    StoreCounter store = FindObjectOfType<StoreCounter>();
                    if (store != null) store.SellEgg();
                    else GameManager.Instance.SellEgg(transform.position);
                }));
                yield break;
            }
        }

        currentState = HelperState.Idle;
        yield return new WaitForSeconds(1.0f);
    }

    private IEnumerator MoveAndAction(Vector3 pos, string bubbleText, System.Action action)
    {
        SpawnTaskBubble(bubbleText);
        yield return StartCoroutine(MoveTo(pos));
        PlaySquashStretch();
        yield return new WaitForSeconds(0.4f / GameManager.Instance.SpeedMultiplier);
        action?.Invoke();
        yield return new WaitForSeconds(0.2f / GameManager.Instance.SpeedMultiplier);
    }

    private IHarvestable FindClosestHarvestable()
    {
        return FindObjectsOfType<MonoBehaviour>().OfType<IHarvestable>()
            .Where(h => h.IsReadyToHarvest())
            .OrderBy(h => Vector3.Distance(transform.position, ((MonoBehaviour)h).transform.position))
            .FirstOrDefault();
    }

    private IFeedable FindClosestFeedable()
    {
        return FindObjectsOfType<MonoBehaviour>().OfType<IFeedable>()
            .Where(f => f.CanInteract())
            .OrderBy(f => Vector3.Distance(transform.position, ((MonoBehaviour)f).transform.position))
            .FirstOrDefault();
    }

    private T FindClosestInteractable<T>() where T : MonoBehaviour, IInteractable
    {
        return FindObjectsOfType<T>()
            .Where(i => i.CanInteract())
            .OrderBy(i => Vector3.Distance(transform.position, i.transform.position))
            .FirstOrDefault();
    }

    private IEnumerator MoveTo(Vector3 position)
    {
        isMoving = true;
        currentState = HelperState.Moving;
        Vector3 startPos = transform.position;
        position.z = startPos.z;

        if (spriteRenderer != null)
        {
            bool faceLeft = position.x < startPos.x;
            spriteRenderer.flipX = faceLeft;
            StoryVisualBinder.SetFacing(happyHarvestVisualRoot, faceLeft);
        }

        float distance = Vector3.Distance(startPos, position);
        float duration = distance / Mathf.Max(moveSpeed * GameManager.Instance.SpeedMultiplier, 0.1f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, position, elapsed / duration);
            yield return null;
        }

        transform.position = position;
        isMoving = false;
    }

    private void UpdateAnimation()
    {
        bobTimer += Time.deltaTime * bobSpeed;
        float bob = (isMoving ? Mathf.Abs(Mathf.Sin(bobTimer * 2f)) : Mathf.Sin(bobTimer) * 0.3f) * bobAmount;
        transform.localScale = originalScale + new Vector3(0, bob, 0);
    }

    private void PlaySquashStretch() => StartCoroutine(SquashStretchAnimation());

    private IEnumerator SquashStretchAnimation()
    {
        Vector3 original = originalScale;
        Vector3 squash = new Vector3(original.x * 1.3f, original.y * 0.7f, original.z);
        Vector3 stretch = new Vector3(original.x * 0.85f, original.y * 1.15f, original.z);

        float t = 0;
        while (t < 0.1f) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(original, squash, t / 0.1f); yield return null; }
        t = 0;
        while (t < 0.1f) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(squash, stretch, t / 0.1f); yield return null; }
        t = 0;
        while (t < 0.1f) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(stretch, original, t / 0.1f); yield return null; }
        transform.localScale = original;
    }

    private void SpawnTaskBubble(string icon)
    {
        GameObject bubble = new GameObject("TaskBubble");
        bubble.transform.position = transform.position + Vector3.up * 1.5f;
        var tm = bubble.AddComponent<TextMesh>();
        tm.text = icon;
        tm.fontSize = 32;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.characterSize = 0.1f;
        Destroy(bubble, 1.0f);
    }
}
