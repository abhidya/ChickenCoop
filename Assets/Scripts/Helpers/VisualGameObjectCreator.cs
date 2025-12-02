using UnityEngine;

/// <summary>
/// Creates visual GameObjects with colored sprites/quads as placeholders.
/// These GameObjects have proper structure and can easily have their sprites replaced.
/// 
/// Usage: Attach to any GameObject or call from editor/runtime to create visual representations.
/// Each created object has a SpriteRenderer that can be assigned a proper sprite later.
/// </summary>
public class VisualGameObjectCreator : MonoBehaviour
{
    [Header("Auto-Create Visuals on Start")]
    [SerializeField] private bool createOnStart = false;
    [SerializeField] private bool createPlayer = true;
    [SerializeField] private bool createChicken = true;
    [SerializeField] private bool createEnvironment = true;
    
    private void Start()
    {
        if (createOnStart)
        {
            CreateAllVisuals();
        }
    }
    
    /// <summary>
    /// Create all game visuals with colored quads
    /// </summary>
    public void CreateAllVisuals()
    {
        if (createPlayer) CreatePlayerVisual();
        if (createChicken) CreateChickenVisual();
        if (createEnvironment) CreateEnvironmentVisuals();
        
        Debug.Log("✅ Visual GameObjects created! Replace SpriteRenderer sprites with proper art when available.");
    }
    
    /// <summary>
    /// Create a player character GameObject with a colored sprite
    /// </summary>
    public GameObject CreatePlayerVisual()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        
        // Add SpriteRenderer with colored sprite
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = CreateColoredSprite(new Color(0.4f, 0.6f, 0.9f, 1f), 64); // Blue
        sr.sortingOrder = 10;
        
        // Add PlayerController if it exists
        var controller = player.AddComponent<PlayerController>();
        
        // Position
        player.transform.position = new Vector3(-2f, 0f, 0f);
        
        // Add a simple label
        CreateLabel(player, "PLAYER", new Vector3(0, 1.2f, 0));
        
        Debug.Log("Created Player GameObject - replace sprite in Inspector");
        return player;
    }
    
    /// <summary>
    /// Create a chicken GameObject with a colored sprite
    /// </summary>
    public GameObject CreateChickenVisual()
    {
        GameObject chicken = new GameObject("Chicken");
        chicken.tag = "Chicken";
        
        // Main body
        SpriteRenderer sr = chicken.AddComponent<SpriteRenderer>();
        sr.sprite = CreateColoredSprite(new Color(1f, 0.9f, 0.3f, 1f), 56); // Yellow
        sr.sortingOrder = 10;
        
        // Add eye child object
        GameObject eye = new GameObject("Eye");
        eye.transform.SetParent(chicken.transform);
        eye.transform.localPosition = new Vector3(0, 0.3f, 0);
        SpriteRenderer eyeSr = eye.AddComponent<SpriteRenderer>();
        eyeSr.sprite = CreateColoredSprite(new Color(0.1f, 0.1f, 0.1f, 1f), 8); // Black eye
        eyeSr.sortingOrder = 11;
        
        // Add Chicken script if it exists
        var chickenScript = chicken.AddComponent<Chicken>();
        
        // Position
        chicken.transform.position = new Vector3(2f, 0f, 0f);
        
        // Add label
        CreateLabel(chicken, "CHICKEN", new Vector3(0, 1.2f, 0));
        
        Debug.Log("Created Chicken GameObject - replace sprite in Inspector");
        return chicken;
    }
    
    /// <summary>
    /// Create environment GameObjects (store, cornfield, etc.)
    /// </summary>
    public void CreateEnvironmentVisuals()
    {
        // Background
        GameObject background = new GameObject("Background");
        SpriteRenderer bgSr = background.AddComponent<SpriteRenderer>();
        bgSr.sprite = CreateColoredSprite(new Color(0.4f, 0.7f, 0.3f, 1f), 256); // Green grass
        bgSr.sortingOrder = -10;
        background.transform.position = new Vector3(0, -2f, 0);
        background.transform.localScale = new Vector3(10f, 5f, 1f);
        
        // Corn Field
        GameObject cornField = new GameObject("CornField");
        cornField.tag = "CornField";
        SpriteRenderer cfSr = cornField.AddComponent<SpriteRenderer>();
        cfSr.sprite = CreateColoredSprite(new Color(0.5f, 0.8f, 0.2f, 1f), 96); // Light green
        cfSr.sortingOrder = 5;
        cornField.transform.position = new Vector3(-4f, 0f, 0f);
        CreateLabel(cornField, "CORN FIELD", new Vector3(0, 1.5f, 0));
        
        // Add HarvestableField script if it exists
        cornField.AddComponent<HarvestableField>();
        
        // Store
        GameObject store = new GameObject("Store");
        store.tag = "Store";
        SpriteRenderer storeSr = store.AddComponent<SpriteRenderer>();
        storeSr.sprite = CreateColoredSprite(new Color(0.6f, 0.4f, 0.3f, 1f), 96); // Brown
        storeSr.sortingOrder = 5;
        store.transform.position = new Vector3(4f, 0f, 0f);
        CreateLabel(store, "STORE", new Vector3(0, 1.5f, 0));
        
        // Add StoreCounter script if it exists
        store.AddComponent<StoreCounter>();
        
        Debug.Log("Created Environment GameObjects - replace sprites in Inspector");
    }
    
    /// <summary>
    /// Create an egg GameObject that can be instantiated
    /// </summary>
    public GameObject CreateEggPrefab()
    {
        GameObject egg = new GameObject("Egg");
        egg.tag = "Egg";
        
        SpriteRenderer sr = egg.AddComponent<SpriteRenderer>();
        sr.sprite = CreateColoredSprite(new Color(0.95f, 0.9f, 0.85f, 1f), 32); // Cream white
        sr.sortingOrder = 8;
        
        // Add collider for interaction
        CircleCollider2D collider = egg.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        
        Debug.Log("Created Egg prefab - replace sprite in Inspector");
        return egg;
    }
    
    /// <summary>
    /// Create a simple text label for a GameObject
    /// </summary>
    private GameObject CreateLabel(GameObject parent, string text, Vector3 offset)
    {
        GameObject label = new GameObject("Label");
        label.transform.SetParent(parent.transform);
        label.transform.localPosition = offset;
        
        // Add TextMesh for simple 3D text
        TextMesh tm = label.AddComponent<TextMesh>();
        tm.text = text;
        tm.fontSize = 20;
        tm.color = Color.white;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        
        // Add a black outline material for readability
        tm.GetComponent<MeshRenderer>().sortingOrder = 100;
        
        return label;
    }
    
    /// <summary>
    /// Create a simple colored sprite from a texture
    /// This creates an actual Sprite object that can be replaced in the Inspector
    /// </summary>
    private Sprite CreateColoredSprite(Color color, int size)
    {
        // Create a simple texture
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        // Fill with solid color in a circle shape for better visual
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    pixels[y * size + x] = color;
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
