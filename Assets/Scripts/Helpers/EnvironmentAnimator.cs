using UnityEngine;

/// <summary>
/// EnvironmentAnimator - Adds ambient animations to the farm environment.
/// Handles swaying leaves, bouncing corn stalks, dust puffs, and sparkles.
/// </summary>
public class EnvironmentAnimator : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float swayAmount = 5f;
    [SerializeField] private float swaySpeed = 1f;
    [SerializeField] private float randomOffset = 1f;

    [Header("References")]
    [SerializeField] private Transform[] swayingObjects;
    [SerializeField] private Transform[] bouncingObjects;

    // Animation state
    private float[] swayOffsets;
    private float[] bounceOffsets;
    private Vector3[] originalRotations;
    private Vector3[] originalScales;

    private void Start()
    {
        InitializeAnimations();
    }

    /// <summary>
    /// Initialize random offsets for varied animation
    /// </summary>
    private void InitializeAnimations()
    {
        if (swayingObjects != null && swayingObjects.Length > 0)
        {
            swayOffsets = new float[swayingObjects.Length];
            originalRotations = new Vector3[swayingObjects.Length];

            for (int i = 0; i < swayingObjects.Length; i++)
            {
                swayOffsets[i] = Random.Range(0f, Mathf.PI * 2f);
                if (swayingObjects[i] != null)
                {
                    originalRotations[i] = swayingObjects[i].localEulerAngles;
                }
            }
        }

        if (bouncingObjects != null && bouncingObjects.Length > 0)
        {
            bounceOffsets = new float[bouncingObjects.Length];
            originalScales = new Vector3[bouncingObjects.Length];

            for (int i = 0; i < bouncingObjects.Length; i++)
            {
                bounceOffsets[i] = Random.Range(0f, Mathf.PI * 2f);
                if (bouncingObjects[i] != null)
                {
                    originalScales[i] = bouncingObjects[i].localScale;
                }
            }
        }
    }

    private void Update()
    {
        UpdateSwaying();
        UpdateBouncing();
    }

    /// <summary>
    /// Update swaying animation for leaves and plants
    /// </summary>
    private void UpdateSwaying()
    {
        if (swayingObjects == null) return;

        for (int i = 0; i < swayingObjects.Length; i++)
        {
            if (swayingObjects[i] == null) continue;

            float sway = Mathf.Sin(Time.time * swaySpeed + swayOffsets[i]) * swayAmount;
            swayingObjects[i].localRotation = Quaternion.Euler(
                originalRotations[i].x,
                originalRotations[i].y,
                originalRotations[i].z + sway
            );
        }
    }

    /// <summary>
    /// Update bouncing animation for corn stalks and objects
    /// </summary>
    private void UpdateBouncing()
    {
        if (bouncingObjects == null) return;

        for (int i = 0; i < bouncingObjects.Length; i++)
        {
            if (bouncingObjects[i] == null) continue;

            float bounce = 1f + Mathf.Sin(Time.time * swaySpeed * 0.5f + bounceOffsets[i]) * 0.03f;
            bouncingObjects[i].localScale = originalScales[i] * bounce;
        }
    }

    /// <summary>
    /// Spawn a dust puff at a position
    /// </summary>
    public void SpawnDustPuff(Vector3 position)
    {
        GameObject dustPuff = new GameObject("DustPuff");
        dustPuff.transform.position = position;

        ParticleSystem ps = dustPuff.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.15f;
        main.startLifetime = 0.4f;
        main.startColor = new Color(0.85f, 0.75f, 0.65f, 0.4f);
        main.startSpeed = 0.3f;
        main.gravityModifier = -0.05f;
        main.maxParticles = 4;
        main.duration = 0.1f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 4) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.15f;

        ps.Play();
        Destroy(dustPuff, 0.8f);
    }

    /// <summary>
    /// Spawn sparkles at a position (for upgrades/special events)
    /// </summary>
    public void SpawnSparkles(Vector3 position)
    {
        GameObject sparkles = new GameObject("Sparkles");
        sparkles.transform.position = position;

        ParticleSystem ps = sparkles.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.1f;
        main.startLifetime = 0.6f;
        main.startColor = new Color(1f, 1f, 0.5f, 1f);
        main.startSpeed = 1f;
        main.gravityModifier = -0.3f;
        main.maxParticles = 15;
        main.duration = 0.2f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 15) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.4f;

        // Add color over lifetime for sparkle effect
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.yellow, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;

        ps.Play();
        Destroy(sparkles, 1f);
    }

    /// <summary>
    /// Spawn ambient background particles (floating motes, etc.)
    /// </summary>
    public void CreateAmbientParticles()
    {
        GameObject ambient = new GameObject("AmbientParticles");
        ambient.transform.position = Vector3.zero;

        ParticleSystem ps = ambient.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.05f;
        main.startLifetime = 5f;
        main.startColor = new Color(1f, 1f, 0.8f, 0.3f);
        main.startSpeed = 0.2f;
        main.gravityModifier = -0.02f;
        main.maxParticles = 30;
        main.duration = 0f;
        main.loop = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 5f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(10f, 6f, 1f);

        ps.Play();
    }
}
