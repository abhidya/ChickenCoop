using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// TitleCardManager - Displays cinematic title cards for each act of the game story.
/// Shows "Chicken Coop – Act X: Title" with fade in/out animations.
/// </summary>
public class TitleCardManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public TextMeshProUGUI titleText;
    [SerializeField] public CanvasGroup titleCanvasGroup;
    [SerializeField] public GameObject titleCardPanel;

    [Header("Settings")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float displayDuration = 3.0f;
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private bool showOnStart = true;

    [Header("Story Acts")]
    [SerializeField] private string[] actTitles = new string[]
    {
        "Act 1: Dawn on the Farm",
        "Act 2: Growth & Expansion",
        "Act 3: The Path to Prosperity",
        "Act 4: Mastery"
    };

    private bool isShowing = false;
    private int currentAct = 0;

    private void Start()
    {
        // Ensure components exist
        if (titleCardPanel != null)
        {
            titleCardPanel.SetActive(false);
        }

        if (titleCanvasGroup != null)
        {
            titleCanvasGroup.alpha = 0f;
        }

        // Show Act 1 on start if enabled
        if (showOnStart)
        {
            StartCoroutine(ShowTitleCardDelayed(0, 0.5f));
        }

        // Subscribe to game events for automatic act triggers
        var gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.OnCoinsChanged += OnCoinsChanged;
            gameManager.OnHelperCountChanged += OnHelperCountChanged;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events - check instance still exists
        var gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.OnCoinsChanged -= OnCoinsChanged;
            gameManager.OnHelperCountChanged -= OnHelperCountChanged;
        }
    }

    /// <summary>
    /// Trigger act transitions based on game state
    /// </summary>
    private void OnCoinsChanged(int newCoins)
    {
        // Act 2 starts when player reaches 100 coins (first helper)
        if (currentAct == 0 && newCoins >= 100)
        {
            ShowTitleCard(1);
        }
        // Act 3 starts when player reaches 600 coins (enough for some upgrades)
        else if (currentAct == 1 && newCoins >= 600)
        {
            ShowTitleCard(2);
        }
    }

    private void OnHelperCountChanged(int helperCount)
    {
        // Act 4 starts when player has 3+ helpers and significant progress
        if (currentAct == 2 && helperCount >= 3)
        {
            ShowTitleCard(3);
        }
    }

    /// <summary>
    /// Show a specific act title card
    /// </summary>
    public void ShowTitleCard(int actIndex)
    {
        if (actIndex < 0 || actIndex >= actTitles.Length) return;
        if (actIndex <= currentAct) return; // Don't show previous acts

        currentAct = actIndex;
        StartCoroutine(ShowTitleCardCoroutine(actIndex));
    }

    /// <summary>
    /// Show title card with delay
    /// </summary>
    private IEnumerator ShowTitleCardDelayed(int actIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(ShowTitleCardCoroutine(actIndex));
    }

    /// <summary>
    /// Coroutine to show and hide title card with animations
    /// </summary>
    private IEnumerator ShowTitleCardCoroutine(int actIndex)
    {
        if (isShowing) yield break;
        isShowing = true;

        // Ensure panel is active
        if (titleCardPanel != null)
        {
            titleCardPanel.SetActive(true);
        }

        // Set title text
        if (titleText != null)
        {
            string fullTitle = "Chicken Coop – " + actTitles[actIndex];
            titleText.text = fullTitle;
        }

        // Fade in
        if (titleCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                titleCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            titleCanvasGroup.alpha = 1f;
        }

        // Display
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        if (titleCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                titleCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                yield return null;
            }
            titleCanvasGroup.alpha = 0f;
        }

        // Hide panel
        if (titleCardPanel != null)
        {
            titleCardPanel.SetActive(false);
        }

        isShowing = false;
    }

    /// <summary>
    /// Manually show a title card by act number (0-3)
    /// </summary>
    public void ShowAct(int actNumber)
    {
        ShowTitleCard(actNumber);
    }

    /// <summary>
    /// Show next act in sequence
    /// </summary>
    public void ShowNextAct()
    {
        if (currentAct < actTitles.Length - 1)
        {
            ShowTitleCard(currentAct + 1);
        }
    }

    /// <summary>
    /// Reset to beginning (for new game)
    /// </summary>
    public void ResetActs()
    {
        currentAct = 0;
    }
}
