# Unity Asset Store Integration Guide

This guide shows how to import and use free Unity Asset Store assets to replace placeholder sprites and make the game prettier.

## 🌟 Recommended Asset: Low Poly Farm Pack Lite

**By:** JustCreate  
**Price:** FREE  
**Link:** https://assetstore.unity.com/packages/3d/environments/industrial/low-poly-farm-pack-lite-188100

This is the **#1 recommended asset** because:
- ✅ FREE and high quality
- ✅ Contains chickens, corn, eggs, farm buildings
- ✅ Used successfully by many Unity developers
- ✅ Low poly = great performance
- ✅ Easy to integrate

## Step-by-Step: Import Low Poly Farm Pack Lite

### Step 1: Download from Asset Store (5 minutes)

1. **Open Unity Editor** with ChickenCoop project
2. **Open Asset Store window**:
   - `Window > Asset Store`
   - OR open in browser: https://assetstore.unity.com/
3. **Search** for "Low Poly Farm Pack Lite"
4. **Click** on the asset
5. **Add to My Assets** (creates account if needed)
6. **Open in Unity** button appears
7. **Click** "Import" or "Download"

### Step 2: Import into Project (2 minutes)

1. **Package Manager** window opens automatically
2. Find "Low Poly Farm Pack Lite" in "My Assets"
3. **Click** "Download" (if not already downloaded)
4. **Click** "Import"
5. **Select All** in import dialog (or customize)
6. **Click** "Import" button
7. Wait for Unity to import (~30 seconds)

### Step 3: Locate Imported Assets (1 minute)

Assets will be in:
```
Assets/
└── JustCreate/
    └── Low Poly Farm Pack Lite/
        ├── Models/
        │   ├── Animals/
        │   │   ├── Chicken.fbx       ← Use this!
        │   │   ├── Chicken_Brown.fbx
        │   │   └── Chicken_White.fbx
        │   ├── Crops/
        │   │   ├── Corn.fbx           ← Use this!
        │   │   └── Corn_Field.fbx
        │   └── Buildings/
        │       ├── Barn.fbx
        │       └── Store.fbx
        ├── Materials/
        ├── Textures/
        └── Prefabs/
```

## Using 3D Assets in a 2D Game

### Option 1: Use 3D Models with Orthographic Camera (Recommended)

**Advantages:**
- Better depth and shadows
- Easy rotation and animation
- Scales well

**Steps:**

1. **Set Camera to Orthographic**:
   - Select "Main Camera" in Hierarchy
   - In Inspector: Projection → Orthographic
   - Size: 5-10 (adjust to taste)

2. **Place 3D Models in Scene**:
   - Drag chicken model from Project to Hierarchy
   - Position at (0, 0, 0)
   - Adjust Z position to layer properly

3. **Adjust Lighting**:
   - Add Directional Light if needed
   - Light angle: (50, -30, 0) for nice shadows

**Camera Settings:**
```yaml
Main Camera:
  Projection: Orthographic
  Size: 8
  Clipping Planes:
    Near: 0.3
    Far: 1000
  Position: (0, 0, -10)
  Rotation: (0, 0, 0)
```

### Option 2: Render 3D Models to Sprites (Advanced)

**Steps:**

1. **Create Render Camera**:
   - Create new camera: "Sprite Renderer Camera"
   - Set to Orthographic
   - Position facing model
   - Set Clear Flags: Solid Color (transparent)

2. **Create Render Texture**:
   - `Assets > Create > Render Texture`
   - Size: 512x512
   - Assign to camera's Target Texture

3. **Render Model**:
   - Position model in front of camera
   - Enter Play Mode to render
   - Export render texture as PNG

4. **Use as Sprite**:
   - Import PNG as sprite
   - Use in SpriteRenderer

### Option 3: Hybrid Approach (Best of Both)

- Use 3D models for animated characters (chicken, player)
- Use rendered sprites for static objects (eggs, corn)
- Use 2D sprites for UI elements

## Replacing Placeholders with Asset Store Models

### Replace Chicken Placeholder

**Old:** Colored blue circle  
**New:** Low Poly Farm Pack chicken model

1. **Locate chicken in scene**:
   - Select "Chicken" GameObject in Hierarchy

2. **Remove old sprite**:
   - In Inspector, delete or disable SpriteRenderer component

3. **Add 3D model**:
   - Drag `JustCreate/...Models/Animals/Chicken.fbx` onto Chicken GameObject
   - Or: Instantiate as child of Chicken GameObject

4. **Adjust scale and position**:
   - Transform Scale: (0.5, 0.5, 0.5) or adjust to match scene
   - Position: Keep Y=0 for ground level
   - Rotation: (0, 180, 0) to face camera

5. **Update collider** (if needed):
   - Replace BoxCollider2D with BoxCollider or CapsuleCollider
   - Adjust size to match model

6. **Keep Chicken script**:
   - The Chicken.cs script should still work
   - Update sprite references if needed

### Replace Player (Farmer) Placeholder

**Old:** Light green circle  
**New:** Use chicken model with different color, or find farmer model

**Option A: Reuse Chicken Model**
```csharp
// Change material color in script
MeshRenderer renderer = GetComponent<MeshRenderer>();
if (renderer != null)
{
    renderer.material.color = new Color(1f, 0.9f, 0.7f); // Beige/farmer color
}
```

**Option B: Find Farmer Asset**
- Search Asset Store for "Low Poly Human" or "Farmer"
- Free options: "FREE Low Poly Human - RPG Character" by Blink

### Replace Corn Field

1. **Locate** "CornField" in Hierarchy
2. **Drag** Corn.fbx model
3. **Scale**: (0.3, 0.3, 0.3)
4. **Duplicate** for multiple corn stalks
5. **Or** use Corn_Field.fbx for full field

### Replace Egg

1. **Create egg prefab**:
   - Create primitive: GameObject > 3D Object > Sphere
   - Scale: (0.3, 0.4, 0.3) for egg shape
   - Material: White with slight yellow tint

2. **Or find egg model** in Food pack assets

### Replace Platform/Ground

1. **Use Ground plane** from farm pack
2. **Or** create plane: GameObject > 3D Object > Plane
3. **Apply grass texture** from asset pack materials
4. **Scale**: (5, 1, 5) or larger

### Replace Store Counter

1. **Use Barn or Store model** from asset pack
2. **Position** at store location
3. **Scale down** if needed: (0.5, 0.5, 0.5)

## Adjusting Visual Style

### Make it Prettier - Lighting

1. **Add Directional Light**:
   - GameObject > Light > Directional Light
   - Rotation: (50, -30, 0)
   - Color: Slight warm yellow (#FFFACD)
   - Intensity: 1.2

2. **Add Point Lights** (optional):
   - Add small point lights near chickens for highlight
   - Warm yellow color
   - Range: 3-5
   - Intensity: 0.5

### Make it Prettier - Post Processing (Optional)

1. **Install Post Processing**:
   - `Window > Package Manager`
   - Search "Post Processing"
   - Install

2. **Add Post Process Volume**:
   - GameObject > Volume > Global Volume
   - Add effects: Bloom, Color Grading, Vignette

3. **Suggested Settings**:
   - Bloom: Intensity 0.2, Threshold 0.9
   - Color Grading: Slight saturation boost
   - Ambient Occlusion: For depth

### Make it Prettier - Shadows

1. **Quality Settings**:
   - `Edit > Project Settings > Quality`
   - Shadows: Hard or Soft
   - Shadow Distance: 50
   - Shadow Cascades: Two Cascades

2. **Per-Light Settings**:
   - Directional Light: Shadow Type = Soft Shadows
   - Strength: 0.7-1.0

## Additional Free Asset Store Packs

### Low Poly Food Lite (JustCreate)

**Link:** https://assetstore.unity.com/packages/3d/props/food/low-poly-food-lite-258693

**Contains:**
- Corn models
- Egg models
- Vegetables

**Use For:**
- Collectible items
- Store inventory display
- Better quality food models

### Cartoon Farm Crops (False Wisp Studios)

**Search:** "Cartoon Farm Crops" on Asset Store

**Contains:**
- Stylized crop models
- Growth stages
- Animated vegetation

**Use For:**
- Growing corn animation
- Multiple crop types
- Visual variety

### Fantasy Wooden GUI : Free (BLACK HAMMER)

**Search:** "Fantasy Wooden GUI Free" on Asset Store

**Contains:**
- Wooden UI panels
- Buttons with nice styling
- Frames and borders

**Use For:**
- Store UI
- Upgrade panels
- Inventory menus

### Simple UI Pack (Various)

**Search:** "Simple UI Free" on Asset Store

**Contains:**
- Clean, modern UI elements
- Icons and buttons
- Progress bars

**Use For:**
- Resource counters
- Button styling
- Clean UI overlay

## Lighting for Low Poly Look

Low poly assets look best with specific lighting:

### Recommended Lighting Setup

```yaml
Directional Light "Sun":
  Type: Directional
  Color: (255, 250, 205) # Warm white
  Intensity: 1.2
  Rotation: (50, -30, 0)
  Shadows: Soft Shadows
  Shadow Strength: 0.8

Ambient Lighting:
  Window > Rendering > Lighting
  Environment:
    Skybox Material: (Default or custom)
    Sun Source: Directional Light
  Ambient Source: Color
  Ambient Color: (180, 200, 220) # Light blue-gray
  Ambient Intensity: 0.7
```

## Material and Texture Tips

### Making Assets Match Your Style

If imported assets don't match your color scheme:

1. **Create Material Variants**:
   - Duplicate material
   - Adjust color/hue in material inspector
   - Apply to model

2. **Adjust Colors in Script**:
   ```csharp
   MeshRenderer renderer = GetComponent<MeshRenderer>();
   renderer.material.color = Color.Lerp(renderer.material.color, targetColor, 0.5f);
   ```

3. **Use Shader Tint**:
   - Select material
   - If shader has tint/color property, adjust there

### Performance Optimization

For mobile or web:

1. **Reduce polygon count**:
   - Use LOD (Level of Detail) if available
   - Remove unnecessary details

2. **Compress textures**:
   - Select texture in Project
   - Inspector: Compression = Normal Quality
   - Max Size: 1024 or 512

3. **Batch materials**:
   - Use same material for multiple objects
   - Enable GPU Instancing on materials

## Checklist: After Importing Assets

- [ ] Chicken model imported and placed
- [ ] Corn/crop models imported and placed
- [ ] Egg model created or imported
- [ ] Platform/ground configured
- [ ] Store building placed
- [ ] Camera set to Orthographic (if using 3D models)
- [ ] Lighting configured (Directional Light added)
- [ ] Shadows enabled and looking good
- [ ] All colliders updated for 3D models
- [ ] Scripts still work with new models
- [ ] Performance is acceptable (test Play Mode)
- [ ] Visual style is consistent
- [ ] Assets credited in ASSET_CREDITS.md

## Asset Credits File

After importing Asset Store content, update credits:

**File:** `Assets/Art/Licenses/ASSET_CREDITS.md`

Add:
```markdown
## Unity Asset Store Assets

### Low Poly Farm Pack Lite
- **Creator:** JustCreate
- **Source:** https://assetstore.unity.com/packages/3d/environments/industrial/low-poly-farm-pack-lite-188100
- **License:** Unity Asset Store EULA
- **Date:** 2024-12-02
- **Used For:** Chicken models, corn crops, farm buildings

### [Other Asset Name]
- **Creator:** [Creator Name]
- **Source:** [Asset Store Link]
- **License:** [License Type]
- **Date:** [Date Imported]
- **Used For:** [What you used it for]
```

## Troubleshooting

### "Imported models are too big/small"

**Fix:** Adjust Transform scale
- Select GameObject
- Transform Scale: Try (0.1, 0.1, 0.1) to (2, 2, 2)

### "Models are pink/no texture"

**Fix:** Materials not imported
- Reimport asset package
- Check "Import Materials" is enabled
- Or manually apply materials from asset pack

### "Models don't appear in Game View"

**Fix:** Camera or layer issue
- Check camera Position (should be negative Z if objects at Z=0)
- Check objects are not behind camera
- Check Layer visibility

### "Shadows look bad"

**Fix:** Adjust shadow settings
- `Edit > Project Settings > Quality`
- Shadow Distance: 50-100
- Shadow Cascades: Two or Four Cascades
- Per-light: Shadow Strength 0.7-1.0

### "Performance is slow after importing 3D models"

**Fix:** Optimize
- Reduce Shadow Distance
- Lower texture quality
- Remove unnecessary details
- Use occlusion culling

## Reference: wkoziel/2D-Farming-Game

The GitHub repo "wkoziel/2D-Farming-Game" is a good reference for:
- How to structure a farming game
- Asset integration patterns
- 2D implementation examples

**Link:** https://github.com/wkoziel/2D-Farming-Game

Study their implementation for:
- Sprite usage
- Animation setup
- UI layout
- Game loop structure

## Next Steps

After importing assets:

1. **Replace all placeholders** with new models
2. **Adjust lighting** for best visual quality
3. **Test performance** in Play Mode
4. **Update ASSET_CREDITS.md** with proper attribution
5. **Take screenshots** for PR documentation
6. **Commit changes** to repository

## See Also

- [SPRITE_SPECIFICATIONS.md](./SPRITE_SPECIFICATIONS.md) - Original sprite requirements
- [SCENE_CLEANUP_GUIDE.md](./SCENE_CLEANUP_GUIDE.md) - Scene organization
- [FREE_ASSETS.md](./FREE_ASSETS.md) - All free asset options
- [Assets/Art/Licenses/ASSET_CREDITS.md](../Assets/Art/Licenses/ASSET_CREDITS.md) - Credits file
