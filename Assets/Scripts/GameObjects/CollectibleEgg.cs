using UnityEngine;
using System.Collections;

/// <summary>
/// CollectibleEgg - Egg that can be clicked to collect into inventory
/// </summary>
public class CollectibleEgg : MonoBehaviour, IInteractable
{
    private bool isCollected = false;

    public void Interact()
    {
        if (!isCollected)
        {
            Collect();
        }
    }

    public bool CanInteract()
    {
        return !isCollected;
    }

    private void Collect()
    {
        isCollected = true;
        GameManager.Instance.AddEgg(1);

        // Fly to UI animation
        StartCoroutine(CollectAnimation());
    }

    private IEnumerator CollectAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;

        // Spawn glow effect
        SpawnGlowEffect();

        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float progress = t / 0.3f;

            // Scale down and move up
            transform.localScale = startScale * (1f - progress);
            transform.position = startPos + new Vector3(0, progress * 2f, 0);

            yield return null;
        }

        Destroy(gameObject);
    }

    private void SpawnGlowEffect()
    {
        GameObject glow = new GameObject("GlowEffect");
        glow.transform.position = transform.position;

        ParticleSystem ps = glow.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startSize = 0.15f;
        main.startLifetime = 0.4f;
        main.startColor = new Color(1f, 1f, 0.6f, 0.8f);
        main.startSpeed = 0.8f;
        main.gravityModifier = -0.5f;
        main.maxParticles = 12;
        main.duration = 0.1f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 12) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;

        ps.Play();
        Destroy(glow, 0.8f);
    }

    private void OnMouseDown()
    {
        Interact();
    }
}
