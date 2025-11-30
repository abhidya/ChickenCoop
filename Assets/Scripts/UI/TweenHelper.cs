using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// TweenHelper - Utility class for creating smooth animations and transitions.
/// Provides static methods for common tween operations like move, scale, rotate, and color.
/// </summary>
public class TweenHelper : MonoBehaviour
{
    public static TweenHelper Instance { get; private set; }

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

    /// <summary>
    /// Move a transform to a target position over duration
    /// </summary>
    public static Coroutine MoveTo(Transform target, Vector3 endPosition, float duration, Action onComplete = null)
    {
        if (Instance == null) return null;
        return Instance.StartCoroutine(Instance.MoveToCoroutine(target, endPosition, duration, onComplete));
    }

    private IEnumerator MoveToCoroutine(Transform target, Vector3 endPosition, float duration, Action onComplete)
    {
        Vector3 startPosition = target.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutQuad(elapsed / duration);
            target.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        target.position = endPosition;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Scale a transform to a target scale over duration
    /// </summary>
    public static Coroutine ScaleTo(Transform target, Vector3 endScale, float duration, Action onComplete = null)
    {
        if (Instance == null) return null;
        return Instance.StartCoroutine(Instance.ScaleToCoroutine(target, endScale, duration, onComplete));
    }

    private IEnumerator ScaleToCoroutine(Transform target, Vector3 endScale, float duration, Action onComplete)
    {
        Vector3 startScale = target.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutBack(elapsed / duration);
            target.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        target.localScale = endScale;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Punch scale animation (scale up then back to original)
    /// </summary>
    public static Coroutine PunchScale(Transform target, float punchAmount, float duration)
    {
        if (Instance == null) return null;
        return Instance.StartCoroutine(Instance.PunchScaleCoroutine(target, punchAmount, duration));
    }

    private IEnumerator PunchScaleCoroutine(Transform target, float punchAmount, float duration)
    {
        Vector3 originalScale = target.localScale;
        Vector3 punchScale = originalScale * (1f + punchAmount);

        // Punch up
        float halfDuration = duration * 0.3f;
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutQuad(elapsed / halfDuration);
            target.localScale = Vector3.Lerp(originalScale, punchScale, t);
            yield return null;
        }

        // Return to original with bounce
        elapsed = 0f;
        float returnDuration = duration * 0.7f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutBounce(elapsed / returnDuration);
            target.localScale = Vector3.Lerp(punchScale, originalScale, t);
            yield return null;
        }

        target.localScale = originalScale;
    }

    /// <summary>
    /// Squash and stretch animation
    /// </summary>
    public static Coroutine SquashStretch(Transform target, float squashAmount, float duration)
    {
        if (Instance == null) return null;
        return Instance.StartCoroutine(Instance.SquashStretchCoroutine(target, squashAmount, duration));
    }

    private IEnumerator SquashStretchCoroutine(Transform target, float squashAmount, float duration)
    {
        Vector3 originalScale = target.localScale;
        Vector3 squashScale = new Vector3(
            originalScale.x * (1f + squashAmount),
            originalScale.y * (1f - squashAmount * 0.5f),
            originalScale.z
        );
        Vector3 stretchScale = new Vector3(
            originalScale.x * (1f - squashAmount * 0.3f),
            originalScale.y * (1f + squashAmount * 0.3f),
            originalScale.z
        );

        float third = duration / 3f;

        // Squash
        float elapsed = 0f;
        while (elapsed < third)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / third;
            target.localScale = Vector3.Lerp(originalScale, squashScale, t);
            yield return null;
        }

        // Stretch
        elapsed = 0f;
        while (elapsed < third)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / third;
            target.localScale = Vector3.Lerp(squashScale, stretchScale, t);
            yield return null;
        }

        // Return
        elapsed = 0f;
        while (elapsed < third)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutQuad(elapsed / third);
            target.localScale = Vector3.Lerp(stretchScale, originalScale, t);
            yield return null;
        }

        target.localScale = originalScale;
    }

    /// <summary>
    /// Rotate to a target rotation over duration
    /// </summary>
    public static Coroutine RotateTo(Transform target, Quaternion endRotation, float duration, Action onComplete = null)
    {
        if (Instance == null) return null;
        return Instance.StartCoroutine(Instance.RotateToCoroutine(target, endRotation, duration, onComplete));
    }

    private IEnumerator RotateToCoroutine(Transform target, Quaternion endRotation, float duration, Action onComplete)
    {
        Quaternion startRotation = target.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutQuad(elapsed / duration);
            target.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        target.rotation = endRotation;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Wobble rotation animation
    /// </summary>
    public static Coroutine Wobble(Transform target, float angle, float duration, int oscillations = 3)
    {
        if (Instance == null) return null;
        return Instance.StartCoroutine(Instance.WobbleCoroutine(target, angle, duration, oscillations));
    }

    private IEnumerator WobbleCoroutine(Transform target, float angle, float duration, int oscillations)
    {
        Quaternion originalRotation = target.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float wobble = Mathf.Sin(progress * Mathf.PI * oscillations * 2f) * angle * (1f - progress);
            target.rotation = originalRotation * Quaternion.Euler(0, 0, wobble);
            yield return null;
        }

        target.rotation = originalRotation;
    }

    /// <summary>
    /// Fade a sprite renderer's alpha
    /// </summary>
    public static Coroutine FadeTo(SpriteRenderer target, float endAlpha, float duration, Action onComplete = null)
    {
        if (Instance == null) return null;
        return Instance.StartCoroutine(Instance.FadeToCoroutine(target, endAlpha, duration, onComplete));
    }

    private IEnumerator FadeToCoroutine(SpriteRenderer target, float endAlpha, float duration, Action onComplete)
    {
        Color startColor = target.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, endAlpha);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            target.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        target.color = endColor;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Color tween for sprite renderer
    /// </summary>
    public static Coroutine ColorTo(SpriteRenderer target, Color endColor, float duration, Action onComplete = null)
    {
        if (Instance == null) return null;
        return Instance.StartCoroutine(Instance.ColorToCoroutine(target, endColor, duration, onComplete));
    }

    private IEnumerator ColorToCoroutine(SpriteRenderer target, Color endColor, float duration, Action onComplete)
    {
        Color startColor = target.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            target.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        target.color = endColor;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Bounce animation (jump up and down)
    /// </summary>
    public static Coroutine Bounce(Transform target, float height, float duration)
    {
        if (Instance == null) return null;
        return Instance.StartCoroutine(Instance.BounceCoroutine(target, height, duration));
    }

    private IEnumerator BounceCoroutine(Transform target, float height, float duration)
    {
        Vector3 originalPosition = target.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float bounce = Mathf.Sin(progress * Mathf.PI) * height;
            target.position = originalPosition + new Vector3(0, bounce, 0);
            yield return null;
        }

        target.position = originalPosition;
    }

    /// <summary>
    /// Shake animation
    /// </summary>
    public static Coroutine Shake(Transform target, float intensity, float duration)
    {
        if (Instance == null) return null;
        return Instance.StartCoroutine(Instance.ShakeCoroutine(target, intensity, duration));
    }

    private IEnumerator ShakeCoroutine(Transform target, float intensity, float duration)
    {
        Vector3 originalPosition = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float remaining = 1f - (elapsed / duration);
            float x = UnityEngine.Random.Range(-1f, 1f) * intensity * remaining;
            float y = UnityEngine.Random.Range(-1f, 1f) * intensity * remaining;
            target.localPosition = originalPosition + new Vector3(x, y, 0);
            yield return null;
        }

        target.localPosition = originalPosition;
    }

    /// <summary>
    /// Pop in animation (scale from 0 to target with overshoot)
    /// </summary>
    public static Coroutine PopIn(Transform target, float duration)
    {
        if (Instance == null) return null;
        return Instance.StartCoroutine(Instance.PopInCoroutine(target, duration));
    }

    private IEnumerator PopInCoroutine(Transform target, float duration)
    {
        Vector3 targetScale = target.localScale;
        target.localScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutBack(elapsed / duration);
            target.localScale = targetScale * t;
            yield return null;
        }

        target.localScale = targetScale;
    }

    /// <summary>
    /// Pop out animation (scale to 0)
    /// </summary>
    public static Coroutine PopOut(Transform target, float duration, Action onComplete = null)
    {
        if (Instance == null) return null;
        return Instance.StartCoroutine(Instance.PopOutCoroutine(target, duration, onComplete));
    }

    private IEnumerator PopOutCoroutine(Transform target, float duration, Action onComplete)
    {
        Vector3 originalScale = target.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = EaseInBack(elapsed / duration);
            target.localScale = originalScale * (1f - t);
            yield return null;
        }

        target.localScale = Vector3.zero;
        onComplete?.Invoke();
    }

    // Easing functions
    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    private float EaseInQuad(float t)
    {
        return t * t;
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseInBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }

    private float EaseOutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (t < 1f / d1)
        {
            return n1 * t * t;
        }
        else if (t < 2f / d1)
        {
            t -= 1.5f / d1;
            return n1 * t * t + 0.75f;
        }
        else if (t < 2.5f / d1)
        {
            t -= 2.25f / d1;
            return n1 * t * t + 0.9375f;
        }
        else
        {
            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }
    }

    private float EaseInOutSine(float t)
    {
        return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
    }
}
