using UnityEngine;

/// <summary>
/// StoryVisualBinder - Attaches Happy Harvest art prefabs onto simple gameplay objects
/// while stripping runtime logic components from the visual clone.
/// </summary>
public static class StoryVisualBinder
{
    public static GameObject AttachVisualPrefab(Transform host, GameObject visualPrefab, SpriteRenderer placeholderRenderer = null)
    {
        if (host == null || visualPrefab == null)
        {
            return null;
        }

        Transform existing = host.Find(visualPrefab.name + "_Visual");
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject instance = Object.Instantiate(visualPrefab, host);
        instance.name = visualPrefab.name + "_Visual";
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        PruneNonVisualChildren(instance.transform);
        StripGameplayComponents(instance);
        DisableMarkerRenderers(instance.transform);

        if (placeholderRenderer != null)
        {
            placeholderRenderer.enabled = false;
        }

        return instance;
    }

    public static Sprite ExtractRepresentativeSprite(GameObject visualPrefab)
    {
        if (visualPrefab == null)
        {
            return null;
        }

        GameObject temp = Object.Instantiate(visualPrefab);
        temp.hideFlags = HideFlags.HideAndDontSave;

        Transform visualRoot = FindPreferredVisualRoot(temp.transform);
        Sprite sprite = FindBestSprite(visualRoot != null ? visualRoot : temp.transform);

        Object.Destroy(temp);
        return sprite;
    }

    private static void StripGameplayComponents(GameObject root)
    {
        Component[] components = root.GetComponentsInChildren<Component>(true);
        foreach (Component component in components)
        {
            if (component == null)
            {
                continue;
            }

            if (component is Transform || component is SpriteRenderer || component is Animator || component.GetType().Name == "SortingGroup")
            {
                continue;
            }

            if (component is Renderer || component is ParticleSystem)
            {
                continue;
            }

            Object.Destroy(component);
        }
    }

    private static void PruneNonVisualChildren(Transform root)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child == root)
            {
                continue;
            }

            string lowered = child.name.ToLowerInvariant();
            if (lowered.Contains("ui target") || lowered == "logic" || lowered.Contains("collider") || lowered.Contains("pathfind"))
            {
                Object.Destroy(child.gameObject);
            }
        }
    }

    private static void DisableMarkerRenderers(Transform root)
    {
        Transform preferredVisualRoot = FindPreferredVisualRoot(root);
        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            string path = BuildPath(renderer.transform).ToLowerInvariant();
            bool looksLikeMarker = path.Contains("ui target") || path.Contains("target") || path.Contains("marker") || path.Contains("logic");
            bool outsideVisualRoot = preferredVisualRoot != null && !renderer.transform.IsChildOf(preferredVisualRoot);
            renderer.enabled = !looksLikeMarker && !outsideVisualRoot;
        }
    }

    private static Transform FindPreferredVisualRoot(Transform root)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            string lowered = child.name.ToLowerInvariant();
            if (lowered == "visual" || lowered.Contains("visual"))
            {
                return child;
            }
        }

        return null;
    }

    private static Sprite FindBestSprite(Transform root)
    {
        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null || renderer.sprite == null)
            {
                continue;
            }

            string path = BuildPath(renderer.transform).ToLowerInvariant();
            if (path.Contains("ui target") || path.Contains("target") || path.Contains("marker") || path.Contains("logic"))
            {
                continue;
            }

            return renderer.sprite;
        }

        return null;
    }

    private static string BuildPath(Transform current)
    {
        string path = current.name;
        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }
}
