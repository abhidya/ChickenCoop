using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// StoryVisualBinder - Attaches Happy Harvest art prefabs onto simple gameplay objects
/// while stripping runtime logic components from the visual clone.
/// </summary>
public static class StoryVisualBinder
{
    private static readonly Dictionary<int, Transform> AttachedVisualRoots = new Dictionary<int, Transform>();

    public static GameObject AttachVisualPrefab(Transform host, GameObject visualPrefab, SpriteRenderer placeholderRenderer = null)
    {
        if (host == null || visualPrefab == null)
        {
            return null;
        }

        Transform existingRoot = FindExistingVisualRoot(host);
        if (existingRoot != null)
        {
            AlignSortingAndLayers(host.gameObject, existingRoot.gameObject, placeholderRenderer);
            if (placeholderRenderer != null)
            {
                placeholderRenderer.enabled = false;
            }

            AttachedVisualRoots[host.GetInstanceID()] = existingRoot;
            return existingRoot.gameObject;
        }

        GameObject instance = Object.Instantiate(visualPrefab);
        instance.name = visualPrefab.name + "_Visual";
        instance.transform.position = host.position;
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        PrepareVisualInstance(instance, host.gameObject, placeholderRenderer, attachFollower: true);
        AttachedVisualRoots[host.GetInstanceID()] = instance.transform;
        return instance;
    }

    public static GameObject AttachVisualPrefabAsChild(Transform host, GameObject visualPrefab, SpriteRenderer placeholderRenderer = null, string childName = "Visual")
    {
        if (host == null || visualPrefab == null)
        {
            return null;
        }

        int hostId = host.GetInstanceID();
        Transform existingRoot = FindExistingVisualRoot(host);
        if (existingRoot != null)
        {
            if (existingRoot.parent == host)
            {
                DestroyObject(existingRoot.gameObject);
            }
            AttachedVisualRoots.Remove(hostId);
        }

        GameObject instance = Object.Instantiate(visualPrefab, host);
        instance.name = string.IsNullOrWhiteSpace(childName) ? "Visual" : childName;
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        PrepareVisualInstance(instance, host.gameObject, placeholderRenderer, attachFollower: false);
        AttachedVisualRoots[hostId] = instance.transform;
        return instance;
    }

    private static GameObject PrepareVisualInstance(GameObject instance, GameObject host, SpriteRenderer placeholderRenderer, bool attachFollower)
    {
        NormalizeRootTransform(instance.transform);
        NormalizeNestedPrefabChildren(instance.transform);
        PruneNonVisualChildren(instance.transform);
        StripGameplayComponents(instance);
        DisableMarkerRenderers(instance.transform);
        NormalizeSpriteRenderers(instance.transform);
        AlignSortingAndLayers(host, instance, placeholderRenderer);

        if (attachFollower)
        {
            StoryVisualFollower follower = instance.GetComponent<StoryVisualFollower>();
            if (follower == null)
            {
                follower = instance.AddComponent<StoryVisualFollower>();
            }
            follower.Bind(host.transform);
        }
        else
        {
            StoryVisualFollower follower = instance.GetComponent<StoryVisualFollower>();
            if (follower != null)
            {
                DestroyObject(follower);
            }
        }

        if (placeholderRenderer != null)
        {
            placeholderRenderer.enabled = false;
        }

        return instance;
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

            if (component is Transform || component is SpriteRenderer || component is Animator || ShouldKeepVisualComponent(component))
            {
                continue;
            }

            if (component is Renderer || component is ParticleSystem)
            {
                continue;
            }

            DestroyObject(component);
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
                DestroyObject(child.gameObject);
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
            bool looksLikeMarker = path.Contains("ui target") || path.Contains("target") || path.Contains("marker") || path.Contains("logic") || path.Contains("shadow");
            bool outsideVisualRoot = preferredVisualRoot != null && !renderer.transform.IsChildOf(preferredVisualRoot);
            renderer.enabled = !looksLikeMarker && !outsideVisualRoot;
        }
    }

    private static void NormalizeSpriteRenderers(Transform root)
    {
        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }

            renderer.sharedMaterial = null;
            renderer.color = Color.white;
            renderer.maskInteraction = SpriteMaskInteraction.None;
        }
    }

    private static void AlignSortingAndLayers(GameObject host, GameObject visualRoot, SpriteRenderer placeholderRenderer)
    {
        if (host == null || visualRoot == null)
        {
            return;
        }

        int baseLayer = host.layer;
        int sortingLayerId = placeholderRenderer != null ? placeholderRenderer.sortingLayerID : 0;
        int sortingOrder = placeholderRenderer != null ? placeholderRenderer.sortingOrder + 1 : 10;

        Transform[] transforms = visualRoot.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in transforms)
        {
            t.gameObject.layer = baseLayer;
        }

        Component[] components = visualRoot.GetComponentsInChildren<Component>(true);
        foreach (Component component in components)
        {
            if (component == null)
            {
                continue;
            }

            if (component is SpriteRenderer renderer)
            {
                renderer.sortingLayerID = sortingLayerId;
                renderer.sortingOrder = sortingOrder;
            }
            else if (component.GetType().Name == "SortingGroup")
            {
                var type = component.GetType();
                type.GetProperty("sortingLayerID")?.SetValue(component, sortingLayerId);
                type.GetProperty("sortingOrder")?.SetValue(component, sortingOrder);
            }
        }
    }

    private static void NormalizeRootTransform(Transform root)
    {
        if (root == null)
        {
            return;
        }

        root.localPosition = Vector3.zero;
        root.localRotation = Quaternion.identity;
        root.localScale = Vector3.one;
    }

    private static void NormalizeNestedPrefabChildren(Transform root)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (!child.name.StartsWith("Prefab_"))
            {
                continue;
            }

            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
        }
    }

    public static void SetFacing(Transform visualRoot, bool faceLeft)
    {
        if (visualRoot == null)
        {
            return;
        }

        Vector3 localScale = visualRoot.localScale;
        float magnitude = Mathf.Abs(localScale.x);
        localScale.x = faceLeft ? -magnitude : magnitude;
        visualRoot.localScale = localScale;
    }

    public static Transform FindAttachedVisualRoot(Transform host)
    {
        if (host == null)
        {
            return null;
        }

        int hostId = host.GetInstanceID();
        if (AttachedVisualRoots.TryGetValue(hostId, out Transform attachedRoot) && attachedRoot != null)
        {
            return attachedRoot;
        }

        return null;
    }

    public static void ApplySpriteLibrary(GameObject root, string resourcesPath)
    {
        if (root == null || string.IsNullOrWhiteSpace(resourcesPath))
        {
            return;
        }

        Object spriteLibraryAsset = Resources.Load(resourcesPath);
        if (spriteLibraryAsset == null)
        {
            return;
        }

        Component[] components = root.GetComponentsInChildren<Component>(true);
        foreach (Component component in components)
        {
            if (component == null || component.GetType().Name != "SpriteLibrary")
            {
                continue;
            }

            PropertyInfo property = component.GetType().GetProperty("spriteLibraryAsset", BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanWrite)
            {
                property.SetValue(component, spriteLibraryAsset);
                continue;
            }

            FieldInfo field = component.GetType().GetField("m_SpriteLibraryAsset", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(component, spriteLibraryAsset);
            }
        }
    }

    private static Transform FindExistingVisualRoot(Transform host)
    {
        if (host == null)
        {
            return null;
        }

        Transform attached = FindAttachedVisualRoot(host);
        if (attached != null)
        {
            return attached;
        }

        for (int i = 0; i < host.childCount; i++)
        {
            Transform child = host.GetChild(i);
            string lowered = child.name.ToLowerInvariant();
            if (lowered == "visual" || lowered.EndsWith("_visual") || lowered.Contains("visual"))
            {
                return child;
            }
        }

        return null;
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

    private static bool ShouldKeepVisualComponent(Component component)
    {
        string typeName = component.GetType().Name;
        switch (typeName)
        {
            case "SortingGroup":
            case "SpriteResolver":
            case "SpriteLibrary":
            case "SpriteSkin":
            case "Light2D":
                return true;
        }

        string namespaceName = component.GetType().Namespace ?? string.Empty;
        if (namespaceName.StartsWith("UnityEngine.U2D") || namespaceName.StartsWith("UnityEngine.Rendering"))
        {
            return true;
        }

        return false;
    }

    private static void DestroyObject(Object target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(target);
        }
        else
        {
            Object.DestroyImmediate(target);
        }
    }

    private static string BuildPath(Transform current)
    {
        if (current == null)
        {
            return string.Empty;
        }

        string path = current.name;
        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }
}

public sealed class StoryVisualFollower : MonoBehaviour
{
    private Transform target;
    private Vector3 offset;

    public void Bind(Transform newTarget)
    {
        target = newTarget;
        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = target.position + offset;
    }
}
