using UnityEngine;

/// <summary>
/// DayNightCycle - Creates a visual day/night cycle by smoothly transitioning
/// the camera background color through a gradient over time.
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    [Header("Cycle Settings")]
    [Tooltip("Duration of a full day/night cycle in seconds")]
    [SerializeField] private float cycleDuration = 120f;

    [Tooltip("Starting time of day (0-1, where 0.5 is noon)")]
    [SerializeField] private float startTime = 0.25f;

    [Header("Sky Colors")]
    [SerializeField] private Gradient skyGradient;

    [Header("References")]
    [SerializeField] private Camera mainCamera;

    [Header("Ambient Light (Optional)")]
    [SerializeField] private bool adjustAmbientLight = true;
    [SerializeField] private Gradient ambientGradient;

    // Current time of day (0-1)
    private float timeOfDay;

    // Is the cycle running
    private bool isRunning = true;

    private void Awake()
    {
        // Initialize with default gradients if not set
        if (skyGradient == null || skyGradient.colorKeys.Length == 0)
        {
            CreateDefaultSkyGradient();
        }

        if (ambientGradient == null || ambientGradient.colorKeys.Length == 0)
        {
            CreateDefaultAmbientGradient();
        }
    }

    private void Start()
    {
        // Get main camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Set initial time
        timeOfDay = startTime;

        // Apply initial colors
        UpdateColors();
    }

    private void Update()
    {
        if (!isRunning || mainCamera == null) return;

        // Advance time
        timeOfDay += Time.deltaTime / cycleDuration;
        timeOfDay %= 1f; // Loop between 0 and 1

        UpdateColors();
    }

    /// <summary>
    /// Update camera and ambient colors based on current time
    /// </summary>
    private void UpdateColors()
    {
        if (mainCamera != null && skyGradient != null)
        {
            mainCamera.backgroundColor = skyGradient.Evaluate(timeOfDay);
        }

        if (adjustAmbientLight && ambientGradient != null)
        {
            RenderSettings.ambientLight = ambientGradient.Evaluate(timeOfDay);
        }
    }

    /// <summary>
    /// Create default sky gradient (dawn -> day -> dusk -> night)
    /// </summary>
    private void CreateDefaultSkyGradient()
    {
        skyGradient = new Gradient();

        // Color keys for sky throughout the day
        GradientColorKey[] colorKeys = new GradientColorKey[6];
        
        // Midnight (dark blue)
        colorKeys[0] = new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 0f);
        
        // Dawn (orange-pink)
        colorKeys[1] = new GradientColorKey(new Color(0.98f, 0.6f, 0.4f), 0.2f);
        
        // Morning (light blue)
        colorKeys[2] = new GradientColorKey(new Color(0.5f, 0.7f, 0.9f), 0.3f);
        
        // Noon (bright blue)
        colorKeys[3] = new GradientColorKey(new Color(0.53f, 0.81f, 0.92f), 0.5f);
        
        // Dusk (orange-red)
        colorKeys[4] = new GradientColorKey(new Color(0.95f, 0.5f, 0.3f), 0.75f);
        
        // Night (dark blue)
        colorKeys[5] = new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 1f);

        // Alpha keys (always fully opaque)
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(1f, 1f);

        skyGradient.SetKeys(colorKeys, alphaKeys);
    }

    /// <summary>
    /// Create default ambient light gradient
    /// </summary>
    private void CreateDefaultAmbientGradient()
    {
        ambientGradient = new Gradient();

        GradientColorKey[] colorKeys = new GradientColorKey[4];
        
        // Night (dim)
        colorKeys[0] = new GradientColorKey(new Color(0.3f, 0.3f, 0.4f), 0f);
        
        // Day (bright)
        colorKeys[1] = new GradientColorKey(new Color(1f, 1f, 0.95f), 0.3f);
        colorKeys[2] = new GradientColorKey(new Color(1f, 1f, 0.95f), 0.7f);
        
        // Night (dim)
        colorKeys[3] = new GradientColorKey(new Color(0.3f, 0.3f, 0.4f), 1f);

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(1f, 1f);

        ambientGradient.SetKeys(colorKeys, alphaKeys);
    }

    /// <summary>
    /// Get current time of day (0-1)
    /// </summary>
    public float GetTimeOfDay()
    {
        return timeOfDay;
    }

    /// <summary>
    /// Set time of day directly (0-1)
    /// </summary>
    public void SetTimeOfDay(float time)
    {
        timeOfDay = Mathf.Clamp01(time);
        UpdateColors();
    }

    /// <summary>
    /// Get time of day as a human-readable string
    /// </summary>
    public string GetTimeString()
    {
        int hours = Mathf.FloorToInt(timeOfDay * 24f);
        int minutes = Mathf.FloorToInt((timeOfDay * 24f - hours) * 60f);
        return $"{hours:D2}:{minutes:D2}";
    }

    /// <summary>
    /// Check if it's currently daytime
    /// </summary>
    public bool IsDaytime()
    {
        return timeOfDay >= 0.25f && timeOfDay <= 0.75f;
    }

    /// <summary>
    /// Pause the day/night cycle
    /// </summary>
    public void Pause()
    {
        isRunning = false;
    }

    /// <summary>
    /// Resume the day/night cycle
    /// </summary>
    public void Resume()
    {
        isRunning = true;
    }

    /// <summary>
    /// Toggle the day/night cycle
    /// </summary>
    public void Toggle()
    {
        isRunning = !isRunning;
    }

    /// <summary>
    /// Set cycle duration
    /// </summary>
    public void SetCycleDuration(float duration)
    {
        cycleDuration = Mathf.Max(10f, duration);
    }
}
