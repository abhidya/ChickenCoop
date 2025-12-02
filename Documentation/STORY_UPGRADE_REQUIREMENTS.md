# Story Upgrade Requirements - Technical Specification

## Overview
This document translates the cinematic story requirements into specific technical requirements and configuration values.

---

## 🎮 Economy Configuration

### Starting Resources (Act 1)
```
Player starts with:
- Coins: 50
- Corn: 0
- Eggs: 0
- Helpers: 0
```

### Base Prices
```
Egg Sell Price: 10 coins
Helper Cost Formula: 100 + (helperCount × 50)
  - First helper: 100 coins
  - Second helper: 150 coins
  - Third helper: 200 coins
  - Fourth helper: 250 coins
  - etc.
```

### Upgrade System (Act 3)
```
All upgrades apply 1.2x multiplier (20% increase)
One-time purchases (not repeatable)

Upgrade 1: Better Seeds
- Cost: 100 coins
- Effect: cornMultiplier *= 1.2
- Description: "+20% Corn per harvest"
- Icon: Corn seed bag

Upgrade 2: Healthier Chickens
- Cost: 200 coins
- Effect: eggMultiplier *= 1.2
- Description: "+20% Egg output"
- Icon: Chicken

Upgrade 3: Premium Eggs
- Cost: 300 coins
- Effect: priceMultiplier *= 1.2
- Description: "+20% Egg sell price"
- Icon: Golden egg

Upgrade 4: Faster Operations
- Cost: 500 coins
- Effect: speedMultiplier *= 1.2
- Description: "+20% Speed for all actions"
- Icon: Clock/Speed icon

Upgrade 5: Bigger Store
- Cost: 750 coins
- Effect: storeEfficiency *= 1.2
- Description: "+20% Store efficiency"
- Icon: Store/Market building
```

---

## ⏱️ Timing Configuration

### Manual Play Loop (Target: 6-8 seconds)
```
Harvest Cooldown: 2.0 seconds
Player Movement Speed: Fast enough for ~0.5s between locations
Feeding Animation: ~0.5 seconds
Chicken Peck Animation: 3 pecks @ ~0.2s each = 0.6s
Egg Laying Delay: ~1.5 seconds after feeding
Egg Collection: Instant (with animation)
Sell Transaction: ~0.5 seconds
Walking Time: ~0.5-1.0 seconds per leg

Total Loop Time: ~6-8 seconds
```

### Helper Loop (Target: 7-8 seconds)
```
Already implemented in HelperAI.cs:
- Move to corn field: ~1.0s
- Harvest + wait: ~0.8s
- Move to chicken: ~1.0s
- Feed + egg lay: ~2.0s
- Collect egg: ~0.6s
- Move to store: ~1.0s
- Sell: ~0.8s
Total: ~7.2s base / SpeedMultiplier

With Faster Operations upgrade:
- 7.2s / 1.2 = 6.0s per loop
```

---

## 🎨 Visual Requirements

### Art Style
```
Style: Soft pastel, cartoony, cute, chibi-style
Vibe: Happy Harvest aesthetic
Lighting: Warm morning/daylight
Animations: Smooth, bouncy, squash-and-stretch
```

### Color Palette
```
Primary:
- Background: Soft green grass (#C8E6C9)
- Sky: Light blue (#B8D4E8)
- UI Panels: Cream (#FFF9C4)

Resources:
- Corn: Golden yellow (#FFF59D)
- Eggs: Cream white (#FFF8E1)
- Coins: Bright gold (#FFD54F)

Characters:
- Chicken: Orange-yellow (#FFE0B2)
- Helpers: Varied pastel hues (auto-generated)
```

### Sprite Requirements
```
Chicken:
- Use: Assets/HappyHarvest/Art/Animals/Chicken/
- Animations: Idle (bobbing), Peck (3 frames), Egg laying

Corn Field:
- Use: Assets/HappyHarvest/Art/Crops/Corn/Sprites/
- Growth stages: Sprite_Corn_01 through Sprite_Corn_05
- Display: Sprite_Corn_04 or Sprite_Corn_05 (fully grown)

Store Counter:
- Use: Assets/HappyHarvest/Art/Environment/Market/
- Style: Simple market stall

Eggs:
- Use: Existing Assets/Sprites/Egg/egg.png or similar
- Style: Round, cream-colored with shine

Player/Helpers:
- Use: Simple character sprite (can be farmer or simplified version)
- Helpers: Same base sprite with HSV color tint applied
```

---

## ✨ Particle Effects

### Required Particles
```
1. Corn Harvest Burst
   - Trigger: When corn is harvested
   - Effect: Golden particles burst upward
   - Duration: 0.5s
   - Particles: 10-15
   - Color: Golden yellow (#FFF59D)
   - Source: VFX_Harvest_Corn from Happy Harvest

2. Peck Dust
   - Trigger: Each chicken peck (3 times)
   - Effect: Small dust puff at chicken base
   - Duration: 0.3s
   - Particles: 3-5 per peck
   - Color: Light brown/tan (#D7CCC8)

3. Egg Pop Sparkle
   - Trigger: When egg appears
   - Effect: Sparkle burst around egg
   - Duration: 0.5s
   - Particles: 8-12
   - Color: White/yellow sparkle

4. Coin Burst
   - Trigger: When selling egg
   - Effect: Coins burst upward from store
   - Duration: 1.0s
   - Particles: 5-8 coins
   - Color: Gold (#FFD54F)

5. Walking Dust Trail
   - Trigger: While helper is moving
   - Effect: Small dust puffs trail behind
   - Duration: Continuous while moving
   - Particles: 2-3 per step
   - Color: Light tan (#D7CCC8)
   - Source: P_VFX_Step_Dust from Happy Harvest

6. Ambient Dust Motes
   - Trigger: Always active (background)
   - Effect: Slow-floating particles
   - Particles: 20-30 across scene
   - Color: White/cream, very transparent
   - Source: Ambient Dust VFX from Happy Harvest
```

---

## 🎵 Sound Effects

### Required Sounds
```
1. Harvest Squash (corn harvested)
   - Type: Squash/pop sound
   - Duration: ~0.2s
   - Volume: Medium
   - Pitch: Slightly high

2. Peck Peck Peck (chicken feeding)
   - Type: Quick tap sounds
   - Count: 3 in sequence
   - Duration: 0.2s each
   - Volume: Low-medium
   - Source: Chicken-001.wav or Chicken-002.wav

3. Egg Pop (egg appears)
   - Type: Soft pop with bounce
   - Duration: ~0.3s
   - Volume: Medium
   - Pitch: Medium-high

4. Coin Ding (selling egg)
   - Type: Pleasant chime/ding
   - Duration: ~0.5s
   - Volume: Medium-high
   - Effect: Should feel rewarding

5. UI Click (button press)
   - Type: Subtle click
   - Duration: ~0.1s
   - Volume: Low

6. Upgrade Sparkle (purchasing upgrade)
   - Type: Magical sparkle/chime
   - Duration: ~1.0s
   - Volume: Medium-high
   - Effect: Should feel special

7. Helper Spawn (hiring helper)
   - Type: Magical appearance sound
   - Duration: ~0.8s
   - Volume: Medium
   - Effect: Similar to upgrade sparkle
```

---

## 🗺️ Scene Layout

### Camera Framing
```
View: Top-down or isometric
Zoom: Show all 3 main locations + room for helpers
Aspect Ratio: 16:9 or 16:10
Resolution: 1920x1080 reference

Layout (Horizontal):
┌────────────────────────────────────┐
│     [Trees]         [Clouds]       │
│                                    │
│  Corn    →    Chicken   →   Store │
│  Field        (center)       Counter│
│                                    │
│  [Bushes]         [Flowers]        │
└────────────────────────────────────┘

Z-Order (Layer):
- Background: -2 (sky, distant trees)
- Ground: -1 (grass, paths)
- Objects: 0 (corn, chicken, store)
- Characters: 1 (player, helpers)
- Particles: 2 (dust, sparkles)
- UI: Top layer
```

### Object Positions
```
CornField Position: (-4, 0, 0)
Chicken Position: (0, 0, 0)
Store Position: (4, 0, 0)
Helper Spawn Point: (0, -2, 0)

Distance between locations: ~4 units
Helper movement speed: 3 units/second
Travel time: ~1.3 seconds per leg
```

---

## 📱 UI Layout

### Resource HUD (Top of Screen)
```
Position: Anchor to top, full width
Layout: Horizontal with spacing

[ 🌽 123 ]  [ 🥚 45 ]  [ 💰 678 ]

Elements:
- Corn counter with icon
- Egg counter with icon  
- Coin counter with icon
- Auto-updating text
- Icon punch animation on increase
```

### Action Buttons (Bottom of Screen)
```
Position: Anchor to bottom, centered
Layout: Horizontal with spacing

[ Harvest ] [ Feed ] [ Collect ] [ Sell ]

Style:
- Green button sprites from Happy Harvest
- TextMeshPro labels
- Disable when action unavailable
- Slight scale animation on press
```

### Helper Button (Bottom Right)
```
Position: Bottom-right corner
Size: Larger than action buttons

[ Hire Helper\n100 💰 ]

Features:
- Shows current cost
- Updates cost after each hire
- Glows when affordable
- Disabled/gray when can't afford
```

### Upgrade Panel (Center, Popup)
```
Position: Center screen, initially hidden
Layout: Vertical list of 5 upgrades

╔════════════════════════════╗
║     UPGRADES              ║
╠════════════════════════════╣
║ Better Seeds      100 💰   ║
║ Healthier Chickens 200 💰  ║
║ Premium Eggs      300 💰   ║
║ Faster Operations 500 💰   ║
║ Bigger Store      750 💰   ║
╚════════════════════════════╝

Features:
- Each upgrade shows name, cost, icon
- Purchased upgrades gray out
- Can't repurchase
- Close button
- Bounce-in animation when opened
```

### Title Card (Top Center)
```
Position: Top-center, fade in at start
Text: "Chicken Coop – Act 1: Dawn on the Farm"
Style: Large, stylized text
Animation: Fade in → stay 3s → fade out
```

### Goal Display (Optional)
```
Position: Top-left or bottom-left
Text: Next milestone or goal
Example: "Save 50 more coins to hire a helper!"
Updates: When close to goal
```

---

## 🎯 Milestone Progression

### Act 1: Dawn on the Farm (Start → 100 coins)
```
Starting State:
- 50 coins
- 1 corn field
- 1 chicken
- 1 store
- Manual play only

Goal: Earn first 50 coins (6-8 seconds per cycle)
Cycles needed: 5 cycles
Time to first milestone: ~30-40 seconds

Achievement: Can afford first helper!
```

### Act 2: Growth & Expansion (100 → 600 coins)
```
State:
- 1+ helpers working
- Helpers automate the loop
- Player can still play manually

Goals:
- Hire 1-2 more helpers
- Save for first few upgrades

Helper income (1 helper):
- 10 coins per 7-8 seconds
- ~75-85 coins/minute
- Time to 600 coins: ~7-8 minutes with 1 helper
```

### Act 3: Path to Prosperity (600 → 1,850 coins)
```
State:
- Multiple helpers
- Several upgrades purchased
- Multipliers stacking

Goals:
- Purchase all 5 upgrades
- Total upgrade cost: 100+200+300+500+750 = 1,850 coins

With upgrades:
- Speed multiplier makes loops faster
- Price multiplier increases income per egg
- Production multipliers increase yield

Time to complete all upgrades: ~15-20 minutes
```

### Act 4: Mastery (Infinite)
```
State:
- All upgrades purchased
- Multiple helpers working efficiently
- High income rate

Gameplay:
- Idle gameplay - watch farm run
- Hire more helpers for fun
- Accumulate coins
- Enjoy the automation

Income rate (3 helpers, all upgrades):
- Base: 10 coins/egg
- Price mult: 1.2x = 12 coins/egg
- Speed mult: 1.2x → 6s loops
- Income: 3 helpers × 12 coins × 10 cycles/min = 360 coins/min

Goal: Infinite growth, satisfaction of watching
```

---

## 🔧 Technical Implementation Notes

### GameConfig Asset
```
Create: Assets/ScriptableObjects/GameConfig_Story.asset

Settings:
startingCorn = 0
startingEggs = 0
startingCoins = 50
cornPerHarvest = 1
harvestCooldown = 2.0f
cornToFeed = 1
eggsPerFeed = 1
eggSellPrice = 10
helperBaseCost = 100
helperCostIncrease = 50
helperSpeed = 3.0f
helperWaitTime = 0.5f
tweenDuration = 0.3f
punchScale = 1.3f
bobSpeed = 2.0f
bobAmount = 0.1f
```

### UpgradeData Assets
```
Create 5 assets in: Assets/ScriptableObjects/Upgrades/

1. BetterSeeds.asset
   upgradeName = "Better Seeds"
   description = "+20% Corn yield"
   baseCost = 100
   upgradeType = CornField
   effectMultiplier = 1.2
   maxLevel = 1

2. HealthierChickens.asset
   upgradeName = "Healthier Chickens"
   description = "+20% Egg output"
   baseCost = 200
   upgradeType = ChickenProduction
   effectMultiplier = 1.2
   maxLevel = 1

3. PremiumEggs.asset
   upgradeName = "Premium Eggs"
   description = "+20% Sale price"
   baseCost = 300
   upgradeType = EggPrice
   effectMultiplier = 1.2
   maxLevel = 1

4. FasterOperations.asset
   upgradeName = "Faster Operations"
   description = "+20% Speed global"
   baseCost = 500
   upgradeType = Speed
   effectMultiplier = 1.2
   maxLevel = 1

5. BiggerStore.asset
   upgradeName = "Bigger Store"
   description = "+20% Store efficiency"
   baseCost = 750
   upgradeType = StoreCapacity
   effectMultiplier = 1.2
   maxLevel = 1
```

### Helper System
```
Already implemented in HelperAI.cs

Features to ensure:
✓ Unique color per helper (HSV tint)
✓ Full loop: harvest → feed → collect → sell
✓ Smooth movement with tweening
✓ Bobbing animation while idle/walking
✓ Dust puffs while moving
✓ Squash-stretch on actions
✓ Speed affected by speedMultiplier

Timing: 7-8 seconds per loop (base)
```

---

## ✅ Implementation Checklist

### Code Changes (Minimal)
- [ ] Verify GameManager economy values match spec
- [ ] Ensure helper cost formula: 100 + (count × 50)
- [ ] Confirm upgrade multipliers are 1.2x
- [ ] Check loop timing matches 6-8s target

### Configuration
- [ ] Create/Update GameConfig asset with story values
- [ ] Create 5 UpgradeData assets
- [ ] Assign assets to GameManager and UIManager

### Visual Assets
- [ ] Use chicken sprites from Happy Harvest
- [ ] Use corn sprites from Happy Harvest  
- [ ] Use market sprites for store
- [ ] Use UI buttons and icons from Happy Harvest
- [ ] Add background elements (grass, trees, bushes)

### Particles
- [ ] Harvest burst effect
- [ ] Peck dust effect
- [ ] Egg pop sparkle
- [ ] Coin burst effect
- [ ] Walking dust trails
- [ ] Ambient dust motes

### UI
- [ ] Create Canvas with resource HUD
- [ ] Create action button panel
- [ ] Create hire helper button
- [ ] Create upgrade panel
- [ ] Add title card
- [ ] Wire all to UIManager

### Sound
- [ ] Add harvest sound
- [ ] Add peck sounds (use Chicken audio)
- [ ] Add egg pop sound
- [ ] Add coin ding sound
- [ ] Add UI click sound
- [ ] Add upgrade sparkle sound

### Testing
- [ ] Test Act 1: Manual play to 100 coins
- [ ] Test Act 2: Hire first helper
- [ ] Test Act 3: Purchase all upgrades
- [ ] Test Act 4: Multiple helpers + all upgrades
- [ ] Verify loop timing
- [ ] Verify economy balance
- [ ] Check visual cohesion

---

## 📊 Success Metrics

The implementation is successful if:

1. **Gameplay Loop**: 6-8 seconds for manual, 7-8 seconds for helpers
2. **Economy Balance**: First helper at 100 coins is achievable in ~1 minute
3. **All Upgrades**: Purchasable within 15-20 minutes
4. **Visual Quality**: Pastel, cute, cohesive with Happy Harvest aesthetic
5. **Feel**: Satisfying, juicy animations and particle effects
6. **Automation**: Helpers work reliably without player input
7. **Acts Clear**: Player progresses through all 4 acts naturally

---

**Specification Version:** 1.0
**Target Unity Version:** 2022.3+ or 6000.x
**Last Updated:** 2025-12-02
