# Code Samples for ChickenCoop

This document contains code snippets and examples for common patterns used in the ChickenCoop project.

## GameManager Singleton Pattern

The GameManager already implements a proper singleton pattern that prevents duplicate instances:

```csharp
private void Awake()
{
    // Singleton setup with persistence
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeGame();
    }
    else
    {
        // Destroy duplicate instances
        Destroy(gameObject);
    }
}
```

**Key Points**:
- Only one GameManager instance can exist
- Duplicate GameObjects with GameManager will be destroyed automatically
- The first instance persists across scene loads

**To prevent "Player already registered" warnings**:
The issue is likely that there are multiple Player GameObjects in the scene, not multiple GameManager instances. Check the scene hierarchy for duplicate Player objects.

## Preventing Duplicate Player Registration

If you need to add idempotent player registration logic, here's the pattern:

```csharp
public class GameManager : MonoBehaviour
{
    private GameObject registeredPlayer;
    
    public void RegisterPlayer(GameObject player)
    {
        if (registeredPlayer != null)
        {
            Debug.LogWarning("Player already registered; skipping duplicate registration.");
            return;
        }
        
        registeredPlayer = player;
        Debug.Log($"Player registered: {player.name}");
    }
    
    public void UnregisterPlayer()
    {
        registeredPlayer = null;
    }
}
```

## Proper Tag Usage

Unity's default tags (like "Player") are registered automatically. The warning "Default GameObject Tag: Player already registered" suggests:

1. Code is trying to add a tag that already exists
2. Multiple objects have the "Player" tag when only one should

**Solution**:
- Don't try to programmatically add built-in tags
- Use `GameObject.FindWithTag("Player")` carefully - it only returns the first match
- Ensure only ONE GameObject in the scene has the "Player" tag

```csharp
// CORRECT: Check if tag exists before finding
GameObject player = GameObject.FindWithTag("Player");
if (player != null)
{
    // Use player
}

// AVOID: Don't try to add default tags
// TagManager.AddTag("Player"); // This causes the warning!
```

## Finding and Removing Duplicate GameObjects

To programmatically find and remove duplicate GameObjects in the scene:

```csharp
[MenuItem("Tools/Remove Duplicate GameObjects")]
static void RemoveDuplicates()
{
    // Find all GameObjects with a specific name
    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
    
    Dictionary<string, GameObject> uniqueObjects = new Dictionary<string, GameObject>();
    List<GameObject> duplicates = new List<GameObject>();
    
    foreach (GameObject obj in allObjects)
    {
        if (obj.transform.parent == null) // Only check root objects
        {
            if (uniqueObjects.ContainsKey(obj.name))
            {
                duplicates.Add(obj);
            }
            else
            {
                uniqueObjects[obj.name] = obj;
            }
        }
    }
    
    foreach (GameObject duplicate in duplicates)
    {
        Debug.Log($"Removing duplicate: {duplicate.name}");
        DestroyImmediate(duplicate);
    }
    
    Debug.Log($"Removed {duplicates.Count} duplicate GameObjects");
}
```

## Sorting Layers Configuration

Unity Sorting Layers should be configured in this order (from back to front):

1. **Background** (Order: -100) - Lawn, sky, distant objects
2. **Foreground** (Order: 0) - Platform, corn field
3. **Characters** (Order: 100) - Player, chickens, eggs
4. **UI** (Order: 200) - Text, buttons, overlays

**Setting in code**:
```csharp
SpriteRenderer sr = GetComponent<SpriteRenderer>();
sr.sortingLayerName = "Characters";
sr.sortingOrder = 100;
```

**Setting in prefab** (YAML format):
```yaml
SpriteRenderer:
  m_SortingLayerID: 123456789  # Get from Unity Editor
  m_SortingLayer: 2
  m_SortingOrder: 100
```

## Sprite Import Settings

For pixel-perfect 2D sprites:

```csharp
[MenuItem("Tools/Fix Sprite Import Settings")]
static void FixSpriteSettings()
{
    string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" });
    
    foreach (string guid in guids)
    {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32; // Adjust based on your art
            importer.filterMode = FilterMode.Point; // For pixel art
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            
            AssetDatabase.ImportAsset(path);
        }
    }
    
    Debug.Log($"Fixed import settings for {guids.Length} sprites");
}
```

## Creating Prefabs Programmatically

To create a prefab from a GameObject in the scene:

```csharp
[MenuItem("Tools/Create Prefab From Selection")]
static void CreatePrefabFromSelection()
{
    GameObject selected = Selection.activeGameObject;
    if (selected == null)
    {
        Debug.LogError("No GameObject selected!");
        return;
    }
    
    string path = $"Assets/Prefabs/{selected.name}.prefab";
    
    // Create the prefab
    PrefabUtility.SaveAsPrefabAsset(selected, path);
    
    Debug.Log($"Created prefab at {path}");
}
```

## Safe Resource Access Pattern

Always check if singleton instances exist before using them:

```csharp
// CORRECT: Safe access with null check
if (GameManager.Instance != null)
{
    GameManager.Instance.AddCoins(10);
}

// CORRECT: Using null-conditional operator
AudioManager.Instance?.PlaySound("collect");

// AVOID: Direct access without check (can cause NullReferenceException)
// GameManager.Instance.AddCoins(10);
```

## Event Subscription Pattern

Subscribe to GameManager events safely:

```csharp
private void OnEnable()
{
    if (GameManager.Instance != null)
    {
        GameManager.Instance.OnCoinsChanged += HandleCoinsChanged;
    }
}

private void OnDisable()
{
    if (GameManager.Instance != null)
    {
        GameManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
    }
}

private void HandleCoinsChanged(int newCoinCount)
{
    Debug.Log($"Coins changed to: {newCoinCount}");
}
```

## Animation State Machine Example

For character animations:

```csharp
public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    public void PlayIdle()
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsIdle", true);
        }
    }
    
    public void PlayWalk()
    {
        if (animator != null)
        {
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsWalking", true);
        }
    }
    
    public void PlayAction(string actionName)
    {
        if (animator != null)
        {
            animator.SetTrigger(actionName);
        }
    }
}
```

## Particle Effect Spawning

Spawn temporary particle effects:

```csharp
public void SpawnEffect(GameObject effectPrefab, Vector3 position)
{
    if (effectPrefab == null) return;
    
    GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
    
    // Auto-destroy after particle system finishes
    ParticleSystem ps = effect.GetComponent<ParticleSystem>();
    if (ps != null)
    {
        Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
    }
    else
    {
        Destroy(effect, 2f); // Fallback timeout
    }
}
```

## See Also

- [GameManager.cs](../Assets/Scripts/Managers/GameManager.cs) - Full implementation
- [PlayerController.cs](../Assets/Scripts/GameObjects/PlayerController.cs) - Movement patterns
- [HELPER_CLASSES.md](./HELPER_CLASSES.md) - Utility classes
- [FARM_SYSTEMS.md](./FARM_SYSTEMS.md) - Game mechanics
