using UnityEngine;
using System.Collections;

/// <summary>
/// Incubator - Uses a Basket visual to hatch new chickens.
/// Spending eggs and time will result in a new chicken being spawned.
/// </summary>
public class Incubator : MonoBehaviour, IInteractable
{
    private const string RuntimeBasketVisualResourcePath = "Prefab_Basket_Visual";

    [Header("Incubation Settings")]
    [SerializeField] private float incubationTime = 10f;
    [SerializeField] private int eggCost = 5;

    [Header("Animation")]
    [SerializeField] private float wiggleAmount = 10f;
    
    // State
    private bool isIncubating = false;
    private float progress = 0f;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        
        // Load the Basket visual
        GameObject prefab = Resources.Load<GameObject>(RuntimeBasketVisualResourcePath);
        if (prefab != null)
        {
            StoryVisualBinder.AttachVisualPrefab(transform, prefab, spriteRenderer);
        }
    }

    public void Interact()
    {
        if (CanInteract())
        {
            StartCoroutine(IncubateSequence());
        }
    }

    public bool CanInteract()
    {
        return !isIncubating && GameManager.Instance.Eggs >= eggCost;
    }

    public float GetProgress() => progress;

    private IEnumerator IncubateSequence()
    {
        if (!GameManager.Instance.UseEggs(eggCost)) yield break;

        isIncubating = true;
        progress = 0.01f;
        float elapsed = 0f;
        
        Vector3 originalRotation = transform.localEulerAngles;

        while (elapsed < incubationTime)
        {
            elapsed += Time.deltaTime * GameManager.Instance.SpeedMultiplier;
            progress = elapsed / incubationTime;

            // Wiggle animation increases over time
            float w = Mathf.Sin(Time.time * 10f) * wiggleAmount * progress;
            transform.localEulerAngles = new Vector3(0, 0, w);

            yield return null;
        }

        transform.localEulerAngles = originalRotation;
        
        // Complete!
        GameManager.Instance.AddChicken();
        
        isIncubating = false;
        progress = 0f;
        
        // Pop effect
        StartCoroutine(PopEffect());
    }

    private IEnumerator PopEffect()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.5f;
        yield return new WaitForSeconds(0.2f);
        transform.localScale = originalScale;
    }
}
