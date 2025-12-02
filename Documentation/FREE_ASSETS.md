# Free Asset Recommendations 🎨

This document provides recommendations for free assets to replace placeholder graphics in Chicken Coop. All assets listed are available under licenses compatible with open-source projects.

---

## Table of Contents

1. [Unity Asset Store (Recommended)](#unity-asset-store-recommended)
2. [2D Sprite Assets](#2d-sprite-assets)
3. [Audio Assets](#audio-assets)
4. [Font Assets](#font-assets)
5. [Particle Assets](#particle-assets)
6. [Asset Integration Guide](#asset-integration-guide)

---

## Unity Asset Store (Recommended)

### 🌟 TOP PICKS - Unity Asset Store Free Farm Assets

These assets are **highly recommended** as they're proven, tested, and directly compatible with Unity projects:

#### 1. **Low Poly Farm Pack Lite** by JustCreate ⭐ BEST CHOICE
**Store Link:** https://assetstore.unity.com/packages/3d/environments/industrial/low-poly-farm-pack-lite-188100

**What's Included:**
- Farm buildings (barn, silo, chicken coop)
- Crops (corn, wheat, vegetables)
- Farm animals (chickens, cows, pigs)
- Farm equipment (tractor, tools)
- Fences, gates, hay bales
- Trees, rocks, grass

**Why This Asset:**
- ✅ **FREE and HIGH QUALITY**
- ✅ Used by many Unity developers (proven in production)
- ✅ Low poly style (performs well)
- ✅ Includes chickens and farm elements we need
- ✅ MIT or Unity Store Standard License (check license)
- ✅ Easy to import and use

**Perfect For:**
- Chicken models
- Corn field / crops
- Farm platform/ground
- Background elements
- Store counter building

**Note:** This is a **3D asset pack**. For a 2D game, you can either:
1. Use the 3D models with orthographic camera
2. Extract sprites by rendering the 3D models
3. Use as reference for creating 2D sprites

---

#### 2. **Low Poly Food Lite** by JustCreate
**Store Link:** https://assetstore.unity.com/packages/3d/props/food/low-poly-food-lite-258693

**What's Included:**
- Corn, eggs, vegetables
- Food items and ingredients
- Simple, clean style

**Perfect For:**
- Egg models
- Corn item models
- Store inventory visuals

---

#### 3. **Simple Foods** by BUBBA
**What's Included:**
- Stylized food icons
- Clean, readable designs
- Good for UI

**Perfect For:**
- UI icons (corn, egg, coin)
- Inventory display

---

#### 4. **Free Pixel Food** by HENRY SOFTWARE
**What's Included:**
- Pixel art food sprites
- Corn, eggs, vegetables

**Perfect For:**
- Retro pixel art style option
- 2D sprites ready to use

---

#### 5. **Fantasy Wooden GUI : Free** by BLACK HAMMER
**Store Link:** Search Unity Asset Store

**What's Included:**
- Wooden UI panels
- Buttons, frames, borders
- Fantasy farm aesthetic

**Perfect For:**
- UI panels and buttons
- Store interface
- Upgrade menu

---

### Reference: Proven Games Using These Assets

From GitHub search, these games successfully use similar assets:
- **2D-Farming-Game** (wkoziel) - Example farming game implementation
- **CulinaryChaos** (TheLodge) - Uses multiple farm packs
- **3D-Adventure-Game** (gkyriopoulos) - Uses JustCreate farm assets

---

## 2D Sprite Assets

### Recommended 2D Asset Packs

#### 1. Kenney Assets (CC0 - Public Domain)
**Website:** https://kenney.nl/assets

| Asset Pack | Use For | Link |
|------------|---------|------|
| **Farm Animals Pack** | Chicken sprites | kenney.nl/assets/farm-animals |
| **Tiny Town** | Farm buildings, store | kenney.nl/assets/tiny-town |
| **Crops** | Corn field, vegetation | kenney.nl/assets/crops |
| **RPG Urban Pack** | Character sprites | kenney.nl/assets/rpg-urban-pack |
| **Particle Pack** | Effects | kenney.nl/assets/particle-pack |
| **Game Icons** | UI icons | kenney.nl/assets/game-icons |

**Why Kenney:** 
- Completely free (CC0 license)
- High quality, consistent style
- Designed for games
- No attribution required

#### 2. OpenGameArt.org
**Website:** https://opengameart.org

| Asset | License | Link |
|-------|---------|------|
| 16x16 Farm Tileset | CC0 | opengameart.org/content/lpc-farming-tilesets |
| Chicken Sprite | CC-BY | opengameart.org/content/chicken-sprite |
| Egg Sprites | CC0 | opengameart.org/content/eggs |
| Coin Animation | CC0 | opengameart.org/content/coin-animation |

#### 3. Itch.io Free Assets
**Website:** https://itch.io/game-assets/free

| Asset Pack | Style | Link |
|------------|-------|------|
| Pixel Farm Pack | Pixel art | Various creators |
| Cute Farm Animals | Chibi style | Various creators |
| Simple UI Pack | Clean UI | Various creators |

---

## Specific Asset Mapping

### Character Sprites Needed

```
┌────────────────────────────────────────────────────────────┐
│                    SPRITE REQUIREMENTS                      │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  FARMER (Player Character)                                 │
│  ├── Idle animation (4-8 frames)                          │
│  ├── Walk animation (4-8 frames)                          │
│  └── Action pose (optional)                               │
│                                                            │
│  CHICKEN                                                   │
│  ├── Idle animation (4 frames)                            │
│  ├── Pecking animation (4 frames)                         │
│  ├── Laying animation (4 frames)                          │
│  └── Happy wiggle (optional)                              │
│                                                            │
│  HELPER                                                    │
│  ├── Same as farmer OR                                    │
│  └── Simplified/smaller variant                           │
│                                                            │
│  OBJECTS                                                   │
│  ├── Corn stalk (growth stages optional)                  │
│  ├── Corn item/icon                                       │
│  ├── Egg (single sprite)                                  │
│  ├── Coin (animation optional)                            │
│  └── Store counter/building                               │
│                                                            │
│  BACKGROUND                                                │
│  ├── Grass/ground tiles                                   │
│  ├── Farm fence (optional)                                │
│  └── Decorative elements (flowers, etc.)                  │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

### Recommended Kenney Assets Breakdown

**From "Farm Animals Pack":**
- Chicken sprite (multiple frames)
- Pig (for future expansion)
- Cow (for future expansion)

**From "Tiny Town":**
- Small house (store building)
- Character sprites (farmer, helper)
- Fence pieces
- Ground tiles

**From "Crops":**
- Corn stalk stages
- Wheat/other crops (expansion)
- Harvested items

**From "Game Icons":**
- Corn icon (for UI)
- Egg icon (for UI)
- Coin icon (for UI)
- Heart (future health system)
- Star (achievements)

---

## Audio Assets

### Sound Effects

#### Freesound.org (CC0/CC-BY)
**Website:** https://freesound.org

| Sound | Search Terms | License |
|-------|--------------|---------|
| Harvest | "pop", "pluck", "pick" | CC0 |
| Chicken cluck | "chicken", "cluck", "hen" | CC0 |
| Egg pop | "pop", "spawn", "appear" | CC0 |
| Coin collect | "coin", "cha-ching", "money" | CC0 |
| Purchase | "cash register", "buy" | CC0 |
| Error | "error", "wrong", "buzz" | CC0 |
| Button click | "click", "tap", "button" | CC0 |

#### Recommended Packs

| Pack | Contents | Source |
|------|----------|--------|
| Kenney Audio | UI sounds, impacts | kenney.nl/assets/interface-sounds |
| 512 Sound Effects | Various | opengameart.org |
| Casual Game Sounds | Complete set | Various |

### Background Music

**Recommendations:**

| Track Style | Source | License |
|-------------|--------|---------|
| Cheerful farm | Kevin MacLeod (incompetech.com) | CC-BY |
| Relaxing acoustic | FreeMusicArchive.org | Various |
| Casual game loop | opengameart.org | CC0/CC-BY |

---

## Font Assets

### Recommended Free Fonts

| Font Name | Style | Use For | Source |
|-----------|-------|---------|--------|
| **Press Start 2P** | Pixel | Retro style | Google Fonts |
| **Fredoka One** | Rounded | Friendly UI | Google Fonts |
| **Nunito** | Clean | General text | Google Fonts |
| **Bangers** | Bold | Headlines | Google Fonts |
| **Quicksand** | Modern | Clean UI | Google Fonts |

### Unity Implementation

```csharp
// Fonts should be imported as TMP Font Assets
// Create via: Window → TextMeshPro → Font Asset Creator
```

---

## Particle Assets

### Built-in Alternatives

The current implementation uses programmatic particles, which work well. However, for more polished effects:

#### Kenney Particle Pack
- Spark sprites
- Smoke puffs
- Star shapes
- Circle/soft particles

#### Custom Particle Textures Needed

```
PARTICLE SPRITES:
├── Soft circle (for dust)
├── Star shape (for sparkles)
├── Coin shape (for money burst)
├── Corn kernel (for harvest)
└── Heart (for happy effects)
```

---

## Asset Integration Guide

### Step 1: Download Assets

```bash
# Create asset folders
mkdir -p Assets/Sprites/Characters
mkdir -p Assets/Sprites/Objects
mkdir -p Assets/Sprites/UI
mkdir -p Assets/Sprites/Environment
mkdir -p Assets/Audio/SFX
mkdir -p Assets/Audio/Music
mkdir -p Assets/Fonts
```

### Step 2: Import to Unity

```
1. Drag PNG files into Sprites folder
2. Set Texture Type: "Sprite (2D and UI)"
3. Set Pixels Per Unit: 100 (adjust for your art)
4. Set Filter Mode: "Point (no filter)" for pixel art
5. Apply changes
```

### Step 3: Configure Sprite Sheets

```
For animated sprites:
1. Set Sprite Mode: "Multiple"
2. Open Sprite Editor
3. Slice by cell size or automatic
4. Create animation clips in Animation window
```

### Step 4: Update Prefabs

```csharp
// Update prefab sprite references
[SerializeField] private Sprite farmerSprite;
[SerializeField] private Sprite chickenSprite;
[SerializeField] private Sprite cornSprite;
[SerializeField] private Sprite eggSprite;
[SerializeField] private Sprite storeSprite;
```

### Step 5: Create Animations

```
Animation Clips to Create:
├── Farmer_Idle.anim
├── Farmer_Walk.anim
├── Chicken_Idle.anim
├── Chicken_Peck.anim
├── Chicken_LayEgg.anim
├── Corn_Sway.anim (optional)
└── Coin_Spin.anim (optional)
```

---

## Quick Start: Minimal Asset Set

For the quickest visual improvement, download just these:

### Kenney Tiny Town
```
Characters:
- character_farmer.png → Farmer
- character_worker.png → Helper

Buildings:
- shop_small.png → Store Counter
```

### Kenney Crops
```
Items:
- crop_corn_grown.png → Corn Field
- corn.png → Corn Item
```

### Kenney Farm Animals
```
Animals:
- chicken.png → Chicken
```

### Kenney Game Icons
```
Icons:
- egg.png → Egg (both item and UI)
- coin_gold.png → Coin UI
- corn.png → Corn UI
```

---

## License Summary

| Source | License | Attribution Required |
|--------|---------|---------------------|
| Kenney.nl | CC0 | No |
| OpenGameArt (CC0) | CC0 | No |
| OpenGameArt (CC-BY) | CC-BY | Yes |
| Freesound (CC0) | CC0 | No |
| Freesound (CC-BY) | CC-BY | Yes |
| Google Fonts | OFL | No |

### Attribution Template (if needed)

```markdown
## Asset Credits

- Sprites by Kenney (kenney.nl) - CC0
- Sound effects from Freesound.org:
  - "coin_sound.wav" by [username] - CC-BY 3.0
- Music by Kevin MacLeod (incompetech.com) - CC-BY 3.0
- Fonts from Google Fonts - Open Font License
```

---

## Implementation Priority

```
Phase 1: Basic Visuals
□ Farmer sprite
□ Chicken sprite  
□ Corn sprite
□ Egg sprite
□ Store sprite
□ Background/ground

Phase 2: UI Polish
□ Resource icons
□ Button backgrounds
□ Panel backgrounds
□ Font integration

Phase 3: Effects
□ Particle sprites
□ Coin animation
□ Sparkle effects

Phase 4: Animation
□ Character walk cycles
□ Chicken animations
□ Idle animations

Phase 5: Audio
□ Core sound effects
□ Background music
```
