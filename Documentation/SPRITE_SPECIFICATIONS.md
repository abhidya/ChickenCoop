# Sprite Specifications for ChickenCoop

This document specifies the sprites needed to match the game story and visual requirements.

## Directory Structure

```
Assets/
└── Sprites/
    ├── Player/
    │   ├── player_idle.png          (64x64)
    │   ├── player_run_0.png         (64x64)
    │   └── player_run_1.png         (64x64)
    ├── Chicken/
    │   ├── chicken_idle.png         (48x48)
    │   ├── chicken_walk_0.png       (48x48)
    │   └── chicken_walk_1.png       (48x48)
    ├── Egg/
    │   └── egg.png                  (32x32)
    ├── Environment/
    │   ├── corn_stalk.png           (32x64)
    │   ├── corn_field_tile.png      (64x64)
    │   └── platform.png             (512x128)
    ├── Background/
    │   ├── lawn.png                 (1920x1080)
    │   └── sky.png                  (1920x1080)
    └── UI/
        ├── title.png                (512x128)
        ├── button_play.png          (256x64)
        ├── coin_icon.png            (32x32)
        ├── corn_icon.png            (32x32)
        └── egg_icon.png             (32x32)
```

## Sprite Specifications

### Player Character (Farmer)

**Files**: `player_idle.png`, `player_run_0.png`, `player_run_1.png`

- **Size**: 64x64 pixels
- **Style**: Chibi/cute character with large head, small body
- **Colors**: 
  - Skin: Peachy (#FFD4A3)
  - Overalls: Blue denim (#4A90E2)
  - Hat: Straw yellow (#F5DEB3)
- **Features**:
  - Large expressive eyes
  - Simple smile
  - Straw hat
  - Overalls and boots
- **Animation Frames**:
  - `idle`: Standing, subtle bob
  - `run_0`: Left leg forward
  - `run_1`: Right leg forward
- **Export**: Transparent PNG, no padding

**Import Settings**:
```
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Pixels Per Unit: 32
Filter Mode: Point (for pixel art) or Bilinear (for smooth art)
Compression: None
Max Size: 256
```

### Chicken

**Files**: `chicken_idle.png`, `chicken_walk_0.png`, `chicken_walk_1.png`

- **Size**: 48x48 pixels
- **Style**: Round, adorable chicken
- **Colors**:
  - Body: White with slight cream (#FFFEF0)
  - Beak: Orange (#FF8C42)
  - Comb: Red (#FF5252)
- **Features**:
  - Round fluffy body
  - Small wings
  - Tiny feet
  - Blinking eyes (optional animation)
- **Animation Frames**:
  - `idle`: Standing, gentle bob
  - `walk_0`: Pecking down
  - `walk_1`: Head up
- **Export**: Transparent PNG

**Import Settings**: Same as Player

### Egg

**File**: `egg.png`

- **Size**: 32x32 pixels
- **Style**: Simple, smooth egg
- **Colors**:
  - Main: Off-white (#FFF8E7)
  - Highlight: Slight shine on top
  - Shadow: Soft gray at bottom
- **Features**:
  - Oval shape
  - Subtle highlight for 3D feel
  - Small shadow underneath
- **Export**: Transparent PNG

### Corn/Environment

**Files**: `corn_stalk.png`, `corn_field_tile.png`, `platform.png`

**Corn Stalk** (32x64):
- Tall green stalk with golden corn at top
- Colors: Green stem (#7CB342), yellow corn (#FFD54F)
- Should have slight animation sway

**Corn Field Tile** (64x64):
- Repeatable ground tile
- Brown soil with small plants
- Colors: Dark brown (#6D4C41), green sprouts (#8BC34A)

**Platform** (512x128):
- Large green oval/rounded rectangle
- Dark green color (#558B2F)
- Slight gradient for depth
- Smooth edges
- This is where characters stand

### Background

**Files**: `lawn.png`, `sky.png`

**Lawn** (1920x1080):
- Full-screen background
- Soft pastel colors
- Light green grass (#A5D6A7)
- Gentle rolling hills in distance
- Can include small details: flowers, mushrooms

**Sky** (1920x1080):
- Gradient from light blue at top to lighter at horizon
- Top: #87CEEB (Sky Blue)
- Bottom: #E0F7FA (Light Cyan)
- Optional: fluffy white clouds
- Optional: sun in corner

### UI Elements

**Files**: `title.png`, `button_play.png`, `coin_icon.png`, `corn_icon.png`, `egg_icon.png`

**Title** (512x128):
- Text: "CHICKEN COOP" or game title
- Font: Rounded, friendly font
- Colors: Bright, eye-catching
- Drop shadow for depth

**Button Play** (256x64):
- Rounded rectangle button
- Color: Bright green (#4CAF50)
- Text: "PLAY" or "START"
- Slight 3D effect

**Icons** (32x32 each):
- **Coin**: Golden circle with "$" or shine
- **Corn**: Single corn kernel, yellow
- **Egg**: Small egg icon matching collectible

## Unity Sorting Layers

Configure in **Edit > Project Settings > Tags and Layers > Sorting Layers**:

| Layer Name | Order | Usage |
|------------|-------|-------|
| Background | -100  | Sky, distant hills |
| Foreground | 0     | Platform, corn field |
| Characters | 100   | Player, chickens, eggs |
| UI         | 200   | Text, buttons, overlays |

## Sprite Renderer Settings

### Player Prefab
```yaml
SpriteRenderer:
  sprite: {fileID: player_idle}
  sortingLayerName: Characters
  sortingOrder: 100
  color: {r: 1, g: 1, b: 1, a: 1}
  flipX: 0
  flipY: 0
```

### Chicken Prefab
```yaml
SpriteRenderer:
  sprite: {fileID: chicken_idle}
  sortingLayerName: Characters
  sortingOrder: 90
  color: {r: 1, g: 1, b: 1, a: 1}
```

### Platform
```yaml
SpriteRenderer:
  sprite: {fileID: platform}
  sortingLayerName: Foreground
  sortingOrder: 0
  color: {r: 1, g: 1, b: 1, a: 1}
```

## Animation Configuration

### Player Animation Controller

**States**:
- Idle (default)
- Walking
- Harvesting (trigger)
- Celebrating (trigger)

**Transitions**:
- Idle → Walking: When IsWalking = true
- Walking → Idle: When IsWalking = false
- Any → Harvesting: On Harvest trigger
- Any → Celebrating: On Celebrate trigger

**Parameters**:
- `IsWalking` (Bool)
- `Harvest` (Trigger)
- `Celebrate` (Trigger)

### Chicken Animation Controller

**States**:
- Idle (default)
- Pecking
- Laying Egg (trigger)

**Transitions**:
- Idle → Pecking: When IsPecking = true
- Pecking → Idle: When IsPecking = false
- Any → LayingEgg: On LayEgg trigger

**Parameters**:
- `IsPecking` (Bool)
- `LayEgg` (Trigger)

## Color Palette

### Primary Colors
- **Background Green**: #A5D6A7 (Light Green)
- **Platform Green**: #558B2F (Dark Green)
- **Sky Blue**: #87CEEB
- **Soil Brown**: #6D4C41

### Character Colors
- **Player Skin**: #FFD4A3 (Peach)
- **Overalls Blue**: #4A90E2
- **Chicken White**: #FFFEF0 (Cream White)
- **Beak Orange**: #FF8C42

### Resource Colors
- **Corn Yellow**: #FFD54F
- **Egg Off-White**: #FFF8E7
- **Coin Gold**: #FFD700

### UI Colors
- **Button Green**: #4CAF50
- **Text Dark**: #2E7D32
- **Highlight**: #FFEB3B (Yellow)

## Placeholder Creation

If actual art is not available, create simple placeholder sprites:

### Circle Placeholder (any size)
```csharp
// Unity Editor script to create colored circles
public static Texture2D CreateCircle(int size, Color color)
{
    Texture2D tex = new Texture2D(size, size);
    int center = size / 2;
    float radius = size / 2f;
    
    for (int x = 0; x < size; x++)
    {
        for (int y = 0; y < size; y++)
        {
            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
            if (dist < radius)
            {
                tex.SetPixel(x, y, color);
            }
            else
            {
                tex.SetPixel(x, y, Color.clear);
            }
        }
    }
    
    tex.Apply();
    return tex;
}
```

### Current Scene State (from screenshot)
The scene currently shows:
- Large green oval (platform) - **KEEP THIS**
- Colored circles representing entities:
  - Light green circle (Player?) 
  - Blue circle (Chicken?)
  - Yellow/cream circles (Corn/Eggs?)
  - Brown/red circles (Store?)

**Action**: Replace these colored circles with proper sprites but maintain their positions and functionality.

## Asset Creation Tools (Free Options)

1. **Piskel** (https://www.piskelapp.com/) - Free pixel art editor
2. **Krita** (https://krita.org/) - Free drawing software
3. **GIMP** (https://www.gimp.org/) - Free image editor
4. **Aseprite** ($19.99) - Professional pixel art tool
5. **OpenGameArt.org** - Free game assets

## Asset License Requirements

All sprites must be:
- Original work, OR
- Licensed for commercial use (CC0, CC-BY, MIT, etc.)
- Properly attributed in `Assets/Art/Licenses/ASSET_CREDITS.md`

## Testing Checklist

After importing sprites:
- [ ] All sprites import without errors
- [ ] Sprites appear in Scene view at correct sizes
- [ ] Sorting layers display objects in correct order
- [ ] Animations play smoothly
- [ ] No transparent edges or artifacts
- [ ] Sprites scale properly at different resolutions (1920x1080, 1366x768)
- [ ] Colors match the palette
- [ ] UI text is readable over backgrounds

## Implementation Priority

1. **HIGH PRIORITY** (Minimal viable visuals):
   - Platform sprite (replace green oval with textured version)
   - Player idle sprite (replace colored circle)
   - Chicken idle sprite (replace colored circle)
   - Egg sprite (replace colored circle)
   - Corn sprite (replace colored circle)

2. **MEDIUM PRIORITY** (Polish):
   - Player walk animation frames
   - Chicken animation frames
   - Background lawn/sky
   - UI icons

3. **LOW PRIORITY** (Nice to have):
   - Particle effects
   - Additional decorations
   - Alternative character skins

## Next Steps

1. Create or source the sprites listed above
2. Import into `Assets/Sprites/` with correct settings
3. Update prefabs to use new sprites instead of colored circles
4. Configure sorting layers
5. Set up animation controllers
6. Test in Play mode
7. Adjust sprite scales and positions as needed

## See Also

- [GAME_STORY.md](./GAME_STORY.md) - Visual narrative reference
- [FREE_ASSETS.md](./FREE_ASSETS.md) - Free asset resources
- [UI_SETUP.md](./UI_SETUP.md) - UI implementation guide
