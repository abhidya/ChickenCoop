# Quick Implementation Guide
## Get Your Story Upgrade Running in Under 30 Minutes

This is the **fastest path** to get the story upgrade working. For complete details, see [STORY_UPGRADE_README.md](./STORY_UPGRADE_README.md).

---

## ⚡ Super Quick Start (10 Minutes)

### 1. Create Configuration Assets (2 minutes)
```
Unity Editor Menu:
1. Tools > Story Upgrade > Setup Game Config
   ✓ Creates GameConfig_Story.asset
   
2. Tools > Story Upgrade > Create All Upgrade Assets
   ✓ Creates 5 upgrade assets
```

### 2. Assign to Scene (3 minutes)
```
1. Open Assets/Scenes/MainGame.unity
2. Select GameManager in Hierarchy
3. Drag GameConfig_Story.asset to "Config" field
4. Select UIManager in Hierarchy
5. Set "Available Upgrades" array size to 5
6. Drag in the 5 upgrade assets:
   - BetterSeeds
   - HealthierChickens  
   - PremiumEggs
   - FasterOperations
   - BiggerStore
```

### 3. Add Title Card (2 minutes)
```
Unity Editor Menu:
Tools > Story Upgrade > Setup Title Card
✓ Auto-creates title card system
✓ Shows "Act 1: Dawn on the Farm" on start
✓ Auto-progresses through acts
```

### 4. Test It! (3 minutes)
```
1. Press Play
2. Harvest corn → Feed chicken → Collect egg → Sell egg
3. Earn 100 coins
4. Hire first helper (button appears)
5. Watch helper automate!
```

**Done! You now have a functional game matching the story requirements.**

---

## 🎨 Visual Upgrade (20 Minutes)

### Replace Key Sprites (15 minutes)

#### Chicken (3 min)
```
Location: Assets/HappyHarvest/Art/Animals/Chicken/
1. Find a chicken sprite (any)
2. Select "Chicken" GameObject
3. Drag sprite to SpriteRenderer > Sprite
4. Scale to fit (try 0.8)
```

#### Corn Field (3 min)
```
Location: Assets/HappyHarvest/Art/Crops/Corn/Sprites/
1. Use: Sprite_Corn_04.png or Sprite_Corn_05.png
2. Select "CornField" GameObject
3. Drag sprite to SpriteRenderer
4. Scale to fit
```

#### Store (3 min)
```
Location: Assets/HappyHarvest/Art/Environment/Market/
1. Find market stall sprite
2. Select "Store" GameObject
3. Drag sprite to SpriteRenderer
4. Scale to fit
```

#### Background (3 min)
```
Location: Assets/HappyHarvest/Art/Environment/Grass/
1. Find grass tile
2. Create new GameObject: "Background"
3. Add SpriteRenderer
4. Drag grass sprite
5. Scale large: (20, 20, 1)
6. Move to back: Z = -2 or Sorting Layer = Background
```

#### Icons for UI (3 min)
```
Corn Icon: HappyHarvest/Art/Crops/Corn/Sprites/Sprite_Corn_icon.png
Coin Icon: HappyHarvest/Art/UI/Sprite_coin_icon.png
Egg Icon: Assets/Sprites/UI/egg_icon.png (if exists)

Use these when creating UI (next section)
```

### Quick UI Setup (5 minutes)

**If Canvas already exists:**
```
1. Select Canvas
2. Right-click > UI > Panel (name it "ResourcePanel")
3. Anchor to top
4. Add 3 Text (TextMeshPro):
   - "Coins: 0"
   - "Corn: 0"
   - "Eggs: 0"
5. Select UIManager
6. Drag texts to corresponding fields
```

**If no Canvas:**
```
See UI_SETUP.md for full instructions
OR
Use existing placeholder game and just test mechanics
```

---

## 🎯 Validation Checklist

After quick setup, verify:

- [ ] Game starts with 50 coins ✓
- [ ] Harvesting gives 1 corn ✓
- [ ] Feeding uses 1 corn, produces 1 egg ✓
- [ ] Selling egg gives 10 coins ✓
- [ ] At 100 coins, can hire helper ✓
- [ ] Helper does full loop automatically ✓
- [ ] Can purchase upgrades at correct costs ✓
- [ ] Title card shows "Act 1" on start ✓

**All checked? You're done with core implementation!**

---

## 📊 What You Now Have

### Economy ✓
- Starting: 50 coins
- Egg value: 10 coins
- Helper costs: 100, 150, 200, 250...
- Upgrade costs: 100, 200, 300, 500, 750

### Upgrades ✓
- ✅ Better Seeds (+20% corn)
- ✅ Healthier Chickens (+20% eggs)
- ✅ Premium Eggs (+20% price)
- ✅ Faster Operations (+20% speed)
- ✅ Bigger Store (+20% efficiency)

### Automation ✓
- Helpers work automatically
- Complete full loop: harvest → feed → collect → sell
- ~7-8 seconds per cycle
- Speed affected by upgrades

### Story Progression ✓
- Act 1: Manual play (0-100 coins)
- Act 2: First helper (100+ coins)
- Act 3: Multiple upgrades (600+ coins)
- Act 4: Mastery (3+ helpers, all upgrades)

---

## 🚀 Next Steps (If You Want More)

### Polish (Optional)
1. **Add more sprites**
   - Trees, bushes, flowers
   - Better button graphics
   - Animated effects

2. **Particle Effects**
   ```
   Tools > Story Upgrade > Show Asset Locations
   → Check VFX folder for particle prefabs
   ```

3. **Sound Effects**
   ```
   Chicken sounds: HappyHarvest/Art/Animals/Chicken/Audio/
   Add to AudioManager
   ```

### Full Visual Upgrade
See [ASSET_INTEGRATION_GUIDE.md](./Documentation/ASSET_INTEGRATION_GUIDE.md) for:
- Complete sprite replacement guide
- Full UI build instructions
- Particle system setup
- Environment decoration
- Sound integration

---

## 🆘 Troubleshooting

### "Helper button doesn't appear"
- Check GameManager has correct startingCoins (50)
- Check helperBaseCost is 100
- Play the game and earn 100 coins

### "Upgrades don't show"
- Verify UIManager has "Available Upgrades" assigned
- Check array size is 5
- Ensure all 5 assets are dragged in

### "No title card"
- Run: Tools > Story Upgrade > Setup Title Card
- Check TitleCardManager exists in scene
- Check titleCardPanel is assigned

### "Game doesn't start with 50 coins"
- Select GameManager
- Check Config is assigned
- Or check "Starting Coins" fallback value

### "Nothing is happening"
- Run: Tools > Story Upgrade > Validate Scene Setup
- Check for errors in Console
- Ensure MainGame scene is open

---

## 📚 Full Documentation

For complete instructions and all features:

- **[STORY_UPGRADE_README.md](./STORY_UPGRADE_README.md)** - Complete guide
- **[ASSET_INTEGRATION_GUIDE.md](./Documentation/ASSET_INTEGRATION_GUIDE.md)** - Asset catalog
- **[STORY_UPGRADE_REQUIREMENTS.md](./Documentation/STORY_UPGRADE_REQUIREMENTS.md)** - Technical specs
- **[UI_SETUP.md](./Documentation/UI_SETUP.md)** - UI creation guide

---

## ✅ Success!

If you followed the Super Quick Start, you now have:
- ✅ Correct game economy (50 coins, 10 per egg, etc.)
- ✅ 5 working upgrades with proper costs
- ✅ Helper automation system
- ✅ Title card system showing acts
- ✅ All 4 acts functional

**Time spent:** ~10 minutes for core, ~30 minutes with visuals

**What remains:** Visual polish (sprites, particles, environment) - optional!

The game is **playable and matches the story requirements**. Visual upgrades are cosmetic enhancements.

---

**Need help?** Check the Troubleshooting section or full documentation.

**Ready for more?** See ASSET_INTEGRATION_GUIDE.md for complete visual integration.

**Want to understand the code?** All systems are documented in their respective .cs files.

---

**Last Updated:** 2025-12-02
