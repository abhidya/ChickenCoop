# Art Assets Directory

This directory contains all visual assets for Chicken Coop.

## Current Status

✅ **Visual GameObjects system ready**
- GameObjects can be created with colored sprite placeholders
- Each has a SpriteRenderer component ready for sprites
- Use Editor menu: **Tools > Game Objects > Create All Visuals**

⚠️ **Production art needed**
- Add sprite files to this directory
- Assign sprites to GameObject SpriteRenderers
- See `Licenses/ASSET_CREDITS.md` for free asset sources

## Quick Start

### Creating Visual GameObjects

1. Open Unity Editor
2. Go to **Tools > Game Objects > Create All Visuals**
3. GameObjects will be created with colored placeholders
4. Replace sprites in Inspector when you have proper art

### Replacing Placeholder Sprites

1. Add your sprite PNG files to `Assets/Art/Sprites/`
2. Select GameObject in Hierarchy (e.g., "Player")
3. In Inspector, find **Sprite Renderer** component
4. Drag your sprite into the **Sprite** field
5. Done! The colored circle is now replaced with your art

## Directory Structure

```
Art/
├── Sprites/
│   ├── Characters/     # Player, chicken, helper sprites
│   ├── Objects/        # Eggs, corn, coins
│   ├── UI/            # Icons, buttons, panels
│   └── Environment/    # Backgrounds, grass, store
├── Licenses/
│   └── ASSET_CREDITS.md  # Attribution and license info
└── README.md           # This file
```

## How to Create Visual GameObjects

If game objects are missing:

1. Open Unity Editor
2. Go to menu: **Tools > Game Objects > Create All Visuals**
3. GameObjects will appear in scene with colored placeholders
4. Replace the sprites in Inspector when you have real art

## How to Add Production Art

### Option 1: Kenney Assets (Recommended - CC0)

1. Visit https://kenney.nl/assets
2. Download these packs:
   - Farm Animals Pack
   - Tiny Town
   - Crops
   - Game Icons
3. Extract and copy sprites to appropriate subdirectories
4. Configure import settings:
   - Texture Type: Sprite (2D and UI)
   - Pixels Per Unit: 32-100 (adjust to match scale)
   - Filter Mode: Point (pixel art) or Bilinear (smooth)

### Option 2: OpenGameArt.org

1. Visit https://opengameart.org
2. Search for farm-themed assets
3. Check license (prefer CC0 or CC-BY)
4. Download and import
5. Add attribution to `Licenses/ASSET_CREDITS.md` if CC-BY

## Sprite Requirements

### Characters
- **Farmer**: Main player character (64x64 recommended)
- **Helper**: Automated worker (48x48 recommended)
- **Chicken**: Produces eggs (56x56 recommended)
- **Chicken Eye**: For blink animation (8x8 recommended)

### Objects
- **Egg**: Collected item (32x32 recommended)
- **Corn**: Harvested resource (32x32 recommended)
- **Coin**: Currency indicator (24x24 recommended)

### UI
- **Icons**: Corn, egg, coin for UI (32x32)
- **Buttons**: Action buttons (128x128+)
- **Panels**: Background panels (256x256+)

### Environment
- **Grass**: Background tile (64x64+)
- **Store**: Selling location (96x96+)
- **Corn Field**: Harvesting location (96x96+)

## Import Settings Template

For pixel art:
```
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Pixels Per Unit: 32
Filter Mode: Point (no filter)
Compression: None or LZ4
```

For smooth art:
```
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Pixels Per Unit: 100
Filter Mode: Bilinear
Compression: Normal Quality
```

## Animation Sprites

For animated sprites:
```
Sprite Mode: Multiple
Open Sprite Editor → Slice
Create Animation Clips in Unity
```

---

**Note**: Current placeholders are functional but basic. Production art will significantly improve the game's visual appeal.
