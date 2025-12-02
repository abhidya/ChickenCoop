# Asset Integration Guide
## Chicken Coop - Story Upgrade & Free Asset Integration

This guide provides a comprehensive catalog of all imported free assets and instructions for integrating them into the game to match the 4-act cinematic story.

---

## 📦 Available Asset Packs

### 1. Happy Harvest (Primary Asset Pack)
**Location:** `Assets/HappyHarvest/`

#### Animals
- **Chicken**: `Assets/HappyHarvest/Art/Animals/Chicken/`
  - Sprites available with idle and run animations
  - Animation Controller: `Controller_Prefab_Chicken.controller`
  - Audio files: `Chicken-001.wav`, `Chicken-002.wav`
  - **Use for:** Main chicken character in the game

- **Cow**: `Assets/HappyHarvest/Art/Animals/Cow/`
- **Piggy**: `Assets/HappyHarvest/Art/Animals/Piggy/`

#### Crops
- **Corn**: `Assets/HappyHarvest/Art/Crops/Corn/Sprites/`
  - Multiple growth stages: `Sprite_Corn_01.png` through `Sprite_Corn_05.png`
  - Icon: `Sprite_Corn_icon.png`
  - Seed bag: `Sprite_Corn_seedbag.png`
  - **Use for:** Corn field harvesting mechanic
  
- **Other Crops**: Carrot, Tomato, Wheat, etc. (for future expansion)

#### Environment
- **Market**: `Assets/HappyHarvest/Art/Environment/Market/`
  - **Use for:** Store counter location
- **Bush**: `Assets/HappyHarvest/Art/Environment/Bush/`
- **Flowers**: `Assets/HappyHarvest/Art/Environment/Flowers/`
- **Grass**: `Assets/HappyHarvest/Art/Environment/Grass/`
- **Pinetree**: `Assets/HappyHarvest/Art/Environment/Pinetree/`
- **Rocks**: `Assets/HappyHarvest/Art/Environment/Rocks/`
- **Signs**: `Assets/HappyHarvest/Art/Environment/Signs/`
- **Scarecrow**: `Assets/HappyHarvest/Art/Environment/Scarecrow/`

#### UI Elements
- **Location:** `Assets/HappyHarvest/Art/UI/`
- **Buttons**: 
  - `Sprite_Button_green.png`
  - `Sprite_Button_Blue.png`
- **Icons**:
  - `Sprite_coin_icon.png` - Use for coin counter
  - `Sprite_clock_icon.png` - Use for time/speed upgrades
- **UI Elements**:
  - `Sprite_Circle.png` - Use for various UI elements
  - `Sprite_keyboard_key.png` - Use for control hints

#### VFX (Particle Effects)
- **Harvest Corn**: `Assets/HappyHarvest/VFX/Tools/VFX_Harvest_Corn.vfx`
  - **Use for:** Corn harvest burst particle effect
- **Step Dust**: `Assets/HappyHarvest/VFX/StepDust/P_VFX_Step_Dust.prefab`
  - **Use for:** Helper walking dust trails
- **Fire**: `Assets/HappyHarvest/VFX/Fire/VFX_Fire.prefab`
- **Smoke**: `Assets/HappyHarvest/VFX/Smoke/Smoke.prefab`
- **Ambient Dust**: `Assets/HappyHarvest/VFX/Ambient Dust/`
  - **Use for:** Background atmosphere particles
- **Moths**: `Assets/HappyHarvest/VFX/Moth/P_VFX_Moths.prefab`
- **Leaves**: `Assets/HappyHarvest/VFX/Leaves/`
- **Rain**: `Assets/HappyHarvest/VFX/Rain/`

### 2. Cartoon Farm Crops
**Location:** `Assets/Cartoon_Farm_Crops/`

#### 3D Meshes & Prefabs
- **Standard Prefabs**: `Assets/Cartoon_Farm_Crops/Prefabs/Standard/`
- **Mobile Prefabs**: `Assets/Cartoon_Farm_Crops/Prefabs/Mobile/`
- **Textures**: `Assets/Cartoon_Farm_Crops/Textures/`

**Note:** This is a 3D asset pack. For 2D game, can use as reference or for promotional materials.

### 3. Gridness Studios (Lite Farm Pack)
**Location:** `Assets/Gridness Studios/Lite Farm Pack/`

- **Color Pack**: `Textures/GridnessColorPack.png`
- **Use for:** UI color palette reference, additional UI elements

### 4. Low Poly Farm Assets
**Location:** `Assets/LowPolyFarmLite/` and `Assets/Low poly Farm/`

**Note:** These are 3D assets, useful for reference or future 3D expansion.

### 5. Existing Custom Sprites
**Location:** `Assets/Sprites/`

- **Egg**: `Assets/Sprites/Egg/egg.png`
- **UI Icons**: `Assets/Sprites/UI/`
  - `egg_icon.png`
  - Coin, corn icons (if present)

---

## 🎬 Act-by-Act Integration Plan

### ACT 1: Dawn on the Farm
**Story Requirements:**
- Starting resources: 50 coins, 1 corn field, 1 chicken, 1 store counter
- Soft pastel, cartoony, cute art style
- Warm morning lighting
- Ambient particles (dust motes)

**Asset Integration:**
1. **Chicken**:
   - Use: `Assets/HappyHarvest/Art/Animals/Chicken/` sprites
   - Animation: Use provided idle and peck animations
   - Audio: `Chicken-001.wav` for cluck sounds

2. **Corn Field**:
   - Use: `Assets/HappyHarvest/Art/Crops/Corn/Sprites/Sprite_Corn_04.png` (fully grown)
   - Icon: `Sprite_Corn_icon.png` for UI inventory

3. **Store Counter**:
   - Use: `Assets/HappyHarvest/Art/Environment/Market/` sprites
   - Alternative: Simple wooden counter from environment assets

4. **Background**:
   - Use: Grass tiles from `Assets/HappyHarvest/Art/Environment/Grass/`
   - Add: Ambient dust particles from `Assets/HappyHarvest/VFX/Ambient Dust/`

5. **UI**:
   - Coin icon: `Assets/HappyHarvest/Art/UI/Sprite_coin_icon.png`
   - Buttons: `Sprite_Button_green.png` and `Sprite_Button_Blue.png`

### ACT 2: Growth & Expansion
**Story Requirements:**
- Hire Helper button appears at 100 coins
- Helper NPC spawns with unique color
- Helper performs automated loop

**Asset Integration:**
1. **Helper Character**:
   - Use: Modified version of player sprite or simplified chicken sprite
   - Color variation: Apply different HSV tint per helper (already implemented in HelperAI.cs)

2. **Sparkle Effect**:
   - Create custom particle system for helper spawn
   - Colors: Golden/yellow sparkles to match pastel theme

### ACT 3: The Path to Prosperity
**Story Requirements:**
- 5 Specific Upgrades:
  1. Better Seeds (100 coins) - +20% Corn yield
  2. Healthier Chickens (200 coins) - +20% Egg output
  3. Premium Eggs (300 coins) - +20% Sale price
  4. Faster Operations (500 coins) - +20% Speed global
  5. Bigger Store (750 coins) - +20% Store efficiency

**Asset Integration:**
1. **Upgrade Panel**:
   - Background: Use UI panels from Happy Harvest
   - Icons:
     - Corn: `Sprite_Corn_icon.png`
     - Chicken: Miniature chicken sprite
     - Egg: `Assets/Sprites/UI/egg_icon.png`
     - Speed: `Sprite_clock_icon.png`
     - Store: Market building icon

2. **Upgrade VFX**:
   - Use golden particle burst for upgrade confirmation
   - Play sparkle sound effect

### ACT 4: Mastery
**Story Requirements:**
- Multiple helpers working simultaneously
- High-efficiency automated farm
- Strong visual feedback: dust trails, sparkles, coins popping, crop particles

**Asset Integration:**
1. **Particle Effects**:
   - Harvest burst: `VFX_Harvest_Corn.vfx`
   - Walking dust: `P_VFX_Step_Dust.prefab`
   - Ambient: Moths, dust motes from VFX folder

2. **Environment Decoration**:
   - Trees: `Assets/HappyHarvest/Art/Environment/Pinetree/`
   - Bushes: `Assets/HappyHarvest/Art/Environment/Bush/`
   - Flowers: `Assets/HappyHarvest/Art/Environment/Flowers/`
   - Scarecrow: For decoration

---

## 🛠️ Technical Integration Steps

### Step 1: Replace Placeholder Sprites

#### Chicken
```
GameObject: Chicken
Current: Placeholder yellow circle
Replace with: Assets/HappyHarvest/Art/Animals/Chicken/[sprite_name]
Steps:
1. Select Chicken GameObject in Hierarchy
2. In Inspector, find SpriteRenderer component
3. Drag chicken sprite into "Sprite" field
4. Adjust scale if needed (typically 0.5 - 1.0)
5. Add Animator component
6. Assign Controller_Prefab_Chicken.controller
```

#### Corn Field
```
GameObject: CornField
Current: Placeholder green circle
Replace with: Assets/HappyHarvest/Art/Crops/Corn/Sprites/Sprite_Corn_04.png
Steps:
1. Select CornField GameObject
2. Replace sprite in SpriteRenderer
3. Scale appropriately
4. Consider adding animation for swaying effect
```

#### Store
```
GameObject: Store
Current: Placeholder brown circle
Replace with: Assets/HappyHarvest/Art/Environment/Market/[market_sprite]
Steps:
1. Select Store GameObject
2. Replace sprite
3. May need to create composite (stall + roof + counter)
```

#### Egg
```
Prefab: Egg
Current: Assets/Sprites/Egg/egg.png (might be placeholder)
Replace with: Better egg sprite if available or keep existing
Icon: Assets/Sprites/UI/egg_icon.png for UI
```

### Step 2: Create UI Canvas

**Complete UI Structure:**
```
Canvas (Screen Space - Overlay)
├── ResourcePanel (Anchor: Top)
│   ├── CoinDisplay
│   │   ├── CoinIcon (Sprite: Sprite_coin_icon.png)
│   │   └── CoinText (TextMeshPro)
│   ├── CornDisplay
│   │   ├── CornIcon (Sprite: Sprite_Corn_icon.png)
│   │   └── CornText (TextMeshPro)
│   └── EggDisplay
│       ├── EggIcon (Sprite: egg_icon.png)
│       └── EggText (TextMeshPro)
├── TitleCard (Anchor: Center-Top)
│   └── TitleText ("Act 1: Dawn on the Farm")
├── ActionPanel (Anchor: Bottom)
│   ├── HarvestButton (Sprite: Sprite_Button_green.png)
│   ├── FeedButton (Sprite: Sprite_Button_green.png)
│   ├── CollectButton (Sprite: Sprite_Button_Blue.png)
│   └── SellButton (Sprite: Sprite_Button_Blue.png)
├── HelperPanel (Anchor: Bottom-Right)
│   └── HireHelperButton (Sprite: Sprite_Button_green.png)
└── UpgradePanel (Initially hidden)
    ├── Panel Background
    ├── UpgradeButton_1 (Better Seeds)
    ├── UpgradeButton_2 (Healthier Chickens)
    ├── UpgradeButton_3 (Premium Eggs)
    ├── UpgradeButton_4 (Faster Operations)
    └── UpgradeButton_5 (Bigger Store)
```

### Step 3: Add Particle Systems

#### Corn Harvest Burst
```csharp
// In HarvestableField.cs, when harvest is complete:
if (harvestParticlePrefab != null)
{
    GameObject particles = Instantiate(harvestParticlePrefab, transform.position, Quaternion.identity);
    Destroy(particles, 2f);
}

// Alternative: Use VFX_Harvest_Corn from Happy Harvest
// Drag prefab into HarvestableField inspector
```

#### Peck Dust (Chicken Feeding)
```csharp
// In Chicken.cs, during peck animation:
private void SpawnPeckDust()
{
    GameObject dust = new GameObject("PeckDust");
    dust.transform.position = transform.position + Vector3.down * 0.3f;
    // Configure particle system...
}
```

#### Egg Pop Sparkle
```csharp
// When egg is laid:
// Use golden sparkle particles, small burst
```

#### Coin Burst
```csharp
// In StoreCounter.cs, when selling:
// Spawn coin particles upward
// Already partially implemented in GameManager
```

#### Walking Dust
```csharp
// In HelperAI.cs, already implemented with SpawnDustPuff()
// Can replace with: Assets/HappyHarvest/VFX/StepDust/P_VFX_Step_Dust.prefab
```

### Step 4: Environment Decoration

```
Scene Layout (Top-Down View):

Background Layer:
- Large grass sprite or tiled grass

Environment Objects:
- Trees in background corners
- Bushes scattered around
- Flowers near paths
- Optional: Scarecrow, signs, rocks

Game Objects (Main Layer):
- CornField (Left)
- Chicken (Center)
- Store (Right)
- Helpers (moving between locations)

Foreground:
- Dust particles
- Coin bursts
- Sparkles
```

### Step 5: Configure GameConfig ScriptableObject

Update or create GameConfig asset with story values:

```
Starting Resources:
- startingCoins: 50
- startingCorn: 0
- startingEggs: 0

Economy:
- eggSellPrice: 10
- helperBaseCost: 100
- helperCostIncrease: 50

Upgrades:
- Create 5 UpgradeData assets matching story requirements
```

---

## 🎨 Color Palette (Pastel Theme)

Based on "soft pastel, cartoony, cute, Happy Harvest vibe":

```
Primary Colors:
- Sky Blue: #B8D4E8
- Grass Green: #C8E6C9
- Corn Yellow: #FFF59D
- Warm Brown: #D7CCC8
- Soft White: #FAFAFA

Accent Colors:
- Chicken Orange: #FFE0B2
- Coin Gold: #FFD54F
- Egg Cream: #FFF8E1
- Button Green: #A5D6A7
- Button Blue: #90CAF9

UI Colors:
- Text: #3E2723 (dark brown)
- Background: #FFF9C4 (light cream)
- Panel: #F5F5F5 with soft shadow
```

---

## 🔊 Sound Effects Integration

### Happy Harvest Audio
**Location:** `Assets/HappyHarvest/Common/Audio/`

**Available Sounds:**
- Chicken clucks: `Chicken-001.wav`, `Chicken-002.wav`
- Use Happy Harvest sound manager if available

### Required SFX (Create or Source):
1. **Harvest Squash**: Short, satisfying squash/pop sound
2. **Peck Peck Peck**: Three quick tap sounds
3. **Egg Pop**: Cute pop with slight echo
4. **Coin Ding**: Pleasant ding/chime (short)
5. **UI Click**: Subtle click
6. **Upgrade Sparkle**: Magical sparkle/chime

**Implementation:**
```csharp
// In AudioManager.cs
// Map sound names to AudioClips
soundEffects["harvest"] = harvestSound;
soundEffects["peck"] = peckSound;
soundEffects["egg"] = eggPopSound;
soundEffects["sell"] = coinDingSound;
soundEffects["click"] = clickSound;
soundEffects["upgrade"] = upgradeSparkleSound;
```

---

## 📝 Animation Timeline

### Manual Play Loop (6-8 seconds)
```
0.0s: Click harvest button
0.5s: Squash animation + harvest particle
1.0s: Walk to chicken
1.5s: Feed corn
1.6s: Peck 1
1.8s: Peck 2
2.0s: Peck 3
2.5s: Egg appear animation (pop + bounce)
3.0s: Collect egg (float up + sparkle)
3.5s: Walk to store
4.0s: Sell animation
4.5s: Coin burst
5.0s: Loop complete
```

### Helper Loop (7-8 seconds per helper)
```
Already implemented in HelperAI.cs
- Move to corn (1.0s)
- Harvest (0.8s)
- Move to chicken (1.0s)
- Feed + egg lay (2.0s)
- Collect (0.6s)
- Move to store (1.0s)
- Sell (0.8s)
Total: ~7.2s base time / SpeedMultiplier
```

---

## ✅ Integration Checklist

### Phase 1: Core Visuals
- [ ] Replace Chicken sprite with Happy Harvest chicken
- [ ] Replace Corn field with Happy Harvest corn sprites
- [ ] Replace Store with Market building sprites
- [ ] Add grass background
- [ ] Set proper sprite scales and layers

### Phase 2: UI Creation
- [ ] Create Canvas with proper scaling
- [ ] Add ResourcePanel with coin, corn, egg displays
- [ ] Add UI icons from Happy Harvest
- [ ] Create ActionPanel with styled buttons
- [ ] Create UpgradePanel with 5 upgrade buttons
- [ ] Add TitleCard for act display
- [ ] Wire all UI elements to UIManager

### Phase 3: Particle Effects
- [ ] Add corn harvest burst particles
- [ ] Add peck dust particles
- [ ] Add egg pop sparkles
- [ ] Add coin burst particles
- [ ] Add helper walking dust trails
- [ ] Add ambient dust motes for atmosphere

### Phase 4: Environment
- [ ] Add background trees
- [ ] Add decorative bushes
- [ ] Add flowers
- [ ] Add optional scarecrow/signs
- [ ] Set up proper layering (background/main/foreground)

### Phase 5: Configuration
- [ ] Update GameConfig with story values
- [ ] Create 5 UpgradeData assets
- [ ] Configure upgrade costs and effects
- [ ] Set starting resources to match story

### Phase 6: Audio
- [ ] Add chicken cluck sounds
- [ ] Add harvest sound
- [ ] Add peck sounds
- [ ] Add egg pop sound
- [ ] Add coin ding sound
- [ ] Add UI click sound
- [ ] Add upgrade sparkle sound

### Phase 7: Testing
- [ ] Test manual play loop timing
- [ ] Test helper automation
- [ ] Test all 5 upgrades
- [ ] Verify economy balance
- [ ] Check visual cohesion
- [ ] Verify all 4 acts work as described

---

## 🎯 Quick Start: Minimal Integration

If time is limited, focus on these high-impact changes:

1. **Replace main sprites** (Chicken, Corn, Store)
2. **Add coin icon** to UI
3. **Create basic Canvas** with resource displays
4. **Add one particle effect** (harvest burst)
5. **Update GameConfig** with story values

This will give you ~70% of the visual improvement with ~30% of the work.

---

## 📚 Related Documentation

- [GAME_STORY.md](./GAME_STORY.md) - Full cinematic story description
- [UI_SETUP.md](./UI_SETUP.md) - Detailed UI creation guide
- [UPGRADE_SYSTEM.md](./UPGRADE_SYSTEM.md) - Upgrade system documentation
- [FREE_ASSETS.md](./FREE_ASSETS.md) - Additional asset recommendations

---

## 💡 Tips

1. **Start with static assets**: Replace sprites before adding animations
2. **Test frequently**: Make one change, test in play mode
3. **Use prefabs**: Create prefabs for repeated elements (helpers, particles)
4. **Layer properly**: Background (-1), Main (0), Foreground (1), UI (top)
5. **Match the aesthetic**: Keep everything soft, pastel, and cute
6. **Performance**: Don't spawn too many particles at once
7. **Scale consistency**: Keep similar objects at similar scales

---

## 🚀 Next Steps

After completing asset integration:
1. Polish animations and transitions
2. Add juice and game feel improvements
3. Implement sound effects
4. Add background music
5. Create title screen
6. Add settings/pause menu
7. Implement save/load system
8. Build and deploy to WebGL

---

**Asset Integration Status:** 📋 Ready for implementation
**Last Updated:** 2025-12-02
