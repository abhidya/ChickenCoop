# Art Assets Directory

This directory contains all visual assets for Chicken Coop.

## Current Status

✅ **Placeholder sprites have been generated**
- Simple colored geometric shapes
- Functional for development and testing
- Located in `Sprites/` subdirectories

⚠️ **Production art needed**
- Replace placeholders with Kenney assets (recommended)
- See `Licenses/ASSET_CREDITS.md` for sources

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

## How to Generate Placeholder Sprites

If sprites are missing, use the Unity Editor tool:

1. Open Unity Editor
2. Go to menu: **Tools > Create Placeholder Sprites**
3. Sprites will be generated in `Assets/Art/Sprites/`

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
