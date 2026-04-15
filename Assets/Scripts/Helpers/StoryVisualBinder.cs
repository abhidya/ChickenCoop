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

    public static GameObject AttachVisualPrefab(Transform host, GameObject visualPrefab, SpriteRenderer placeholderRenderer = null, bool preserveRigComponents = false)
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

        GameObject instance = UnityEngine.Object.Instantiate<GameObject>(visualPrefab);
        instance.name = visualPrefab.name + "_Visual";
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        PrepareVisualInstance(instance, host.gameObject, placeholderRenderer, attachFollower: true, preserveRigComponents: preserveRigComponents);

        // NormalizeRootTransform inside PrepareVisualInstance resets localPosition to zero (world origin).
        // Re-position the visual at the host and re-bind the follower so offset is correctly (0,0,0).
        instance.transform.position = host.position;
        StoryVisualFollower follower = instance.GetComponent<StoryVisualFollower>();
        if (follower != null) follower.Bind(host);

        AttachedVisualRoots[host.GetInstanceID()] = instance.transform;
        return instance;
    }

    public static GameObject AttachVisualPrefabAsChild(Transform host, GameObject visualPrefab, SpriteRenderer placeholderRenderer = null, string childName = "Visual", bool preserveRigComponents = false)
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

        GameObject instance = UnityEngine.Object.Instantiate<GameObject>(visualPrefab, host);
        instance.name = string.IsNullOrWhiteSpace(childName) ? "Visual" : childName;
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        PrepareVisualInstance(instance, host.gameObject, placeholderRenderer, attachFollower: false, preserveRigComponents: preserveRigComponents);
        AttachedVisualRoots[hostId] = instance.transform;
        return instance;
    }

    private static GameObject PrepareVisualInstance(GameObject instance, GameObject host, SpriteRenderer placeholderRenderer, bool attachFollower, bool preserveRigComponents)
    {
        NormalizeRootTransform(instance.transform);
        PruneNonVisualChildren(instance.transform);
        StripGameplayComponents(instance, preserveRigComponents);
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

    private static void StripGameplayComponents(GameObject root, bool preserveRigComponents)
    {
        Component[] components = root.GetComponentsInChildren<Component>(true);
        foreach (Component component in components)
        {
            if (component == null)
            {
                continue;
            }

            if (component is Transform || component is SpriteRenderer || component is Animator || ShouldKeepVisualComponent(component, preserveRigComponents))
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
        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            // Only disable renderers that are clearly UI markers or invisible placeholder nodes.
            // Avoid matching broad terms like "target" which appear in animation rig bone names.
            string nameLower = renderer.transform.name.ToLowerInvariant();
            bool looksLikeMarker = nameLower == "ui target" || nameLower.Contains("uimarker")
                || nameLower.StartsWith("logic") || nameLower == "logic"
                || (nameLower.Contains("shadow") && renderer.sprite == null);
            if (looksLikeMarker)
            {
                renderer.enabled = false;
            }
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

            renderer.color = Color.white;
            renderer.maskInteraction = SpriteMaskInteraction.None;
        }
    }

    private static void AlignSortingAndLayers(GameObject host, GameObject visualRoot, SpriteRenderer placeholderRenderer)
    {
        if (host == null || visualRoot == null)
            return;

        int baseLayer = host.layer;
        Transform[] transforms = visualRoot.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in transforms)
        {
            t.gameObject.layer = baseLayer;
        }

        if (placeholderRenderer != null)
        {
            placeholderRenderer.enabled = false;
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

        Component[] components = root.GetComponentsInChildren<Component>(true);
        foreach (Component component in components)
        {
            if (component == null || component.GetType().Name != "SpriteLibrary")
            {
                continue;
            }

            // Load as the exact type the property expects so Unity accepts the assignment.
            PropertyInfo property = component.GetType().GetProperty("spriteLibraryAsset", BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanWrite)
            {
                System.Type assetType = property.PropertyType;
                Object spriteLibraryAsset = Resources.Load(resourcesPath, assetType);
                if (spriteLibraryAsset == null)
                {
                    // Fallback: try loading without type (legacy)
                    spriteLibraryAsset = Resources.Load(resourcesPath);
                }
                if (spriteLibraryAsset != null)
                {
                    property.SetValue(component, spriteLibraryAsset);
                }
                continue;
            }

            FieldInfo field = component.GetType().GetField("m_SpriteLibraryAsset", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                Object spriteLibraryAsset = Resources.Load(resourcesPath, field.FieldType);
                if (spriteLibraryAsset == null)
                {
                    spriteLibraryAsset = Resources.Load(resourcesPath);
                }
                if (spriteLibraryAsset != null)
                {
                    field.SetValue(component, spriteLibraryAsset);
                }
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

    private static bool ShouldKeepVisualComponent(Component component, bool preserveRigComponents)
    {
        string typeName = component.GetType().Name;
        switch (typeName)
        {
            case "SortingGroup":
            case "SpriteResolver":
            case "SpriteLibrary":
            case "Light2D":
                return true;
            case "SpriteSkin":
                return preserveRigComponents;
        }

        string namespaceName = component.GetType().Namespace ?? string.Empty;
        if (namespaceName.StartsWith("UnityEngine.Rendering"))
        {
            return true;
        }

        if (preserveRigComponents && (namespaceName.StartsWith("UnityEngine.U2D") || namespaceName.StartsWith("UnityEngine.U2D.Animation")))
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
    public Vector3 offset;

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
