# Sprites Directory

This directory contains all visual assets (sprites) for the ChickenCoop game.

## Directory Structure

```
Sprites/
├── Player/           # Player character sprites and animations
├── Chicken/          # Chicken character sprites and animations
├── Egg/              # Egg collectible sprites
├── Environment/      # Corn, platform, and farm elements
├── Background/       # Backgrounds and sky
└── UI/               # User interface icons and buttons
```

## Required Sprites

See [Documentation/SPRITE_SPECIFICATIONS.md](../../Documentation/SPRITE_SPECIFICATIONS.md) for complete specifications.

### Critical (Must Have)
- [ ] `Player/player_idle.png` (64x64)
- [ ] `Chicken/chicken_idle.png` (48x48)
- [ ] `Egg/egg.png` (32x32)
- [ ] `Environment/platform.png` (512x128)
- [ ] `Environment/corn_stalk.png` (32x64)

### Important (Should Have)
- [ ] `Player/player_run_0.png` (64x64)
- [ ] `Player/player_run_1.png` (64x64)
- [ ] `Chicken/chicken_walk_0.png` (48x48)
- [ ] `Chicken/chicken_walk_1.png` (48x48)
- [ ] `Background/lawn.png` (1920x1080)

### Polish (Nice to Have)
- [ ] `Background/sky.png` (1920x1080)
- [ ] `UI/title.png` (512x128)
- [ ] `UI/button_play.png` (256x64)
- [ ] `UI/coin_icon.png` (32x32)
- [ ] `UI/corn_icon.png` (32x32)
- [ ] `UI/egg_icon.png` (32x32)

## Temporary Placeholders

Until proper sprites are created, the game uses colored circles and shapes:

- **Player**: Light green circle
- **Chicken**: Blue circle  
- **Egg**: Yellow/cream circle
- **Corn**: Yellow circle
- **Store**: Brown/red circle
- **Platform**: Large dark green oval

## Import Settings

When importing sprites into Unity:

1. **Select sprite file** in Project window
2. **Open Inspector**
3. **Set Texture Type**: Sprite (2D and UI)
4. **Set Sprite Mode**: Single
5. **Set Pixels Per Unit**: 32 (adjust if sprites appear wrong size)
6. **Set Filter Mode**: 
   - Point (for pixel art - crisp edges)
   - Bilinear (for smooth art - anti-aliased)
7. **Set Compression**: None (for best quality)
8. **Click Apply**

## Style Guidelines

### Color Palette

**Environment**:
- Background Green: `#A5D6A7`
- Platform Green: `#558B2F`
- Sky Blue: `#87CEEB`
- Soil Brown: `#6D4C41`

**Characters**:
- Player Skin: `#FFD4A3`
- Overalls Blue: `#4A90E2`
- Chicken White: `#FFFEF0`
- Beak Orange: `#FF8C42`

**Resources**:
- Corn Yellow: `#FFD54F`
- Egg Off-White: `#FFF8E7`
- Coin Gold: `#FFD700`

### Art Style

- **Chibi/Cute**: Characters have large heads, small bodies
- **Rounded**: Use smooth curves, avoid sharp angles
- **Pastel**: Soft, light colors
- **Simple**: Clear silhouettes, minimal details
- **Expressive**: Large eyes, visible emotions

## File Naming Convention

- Use lowercase with underscores: `player_idle.png`
- Include state/action: `chicken_walk_0.png`
- Number animation frames: `_0`, `_1`, `_2`, etc.
- Use descriptive names: `button_play.png` not `btn1.png`

## See Also

- [SPRITE_SPECIFICATIONS.md](../../Documentation/SPRITE_SPECIFICATIONS.md) - Detailed specs
- [GAME_STORY.md](../../Documentation/GAME_STORY.md) - Visual reference
- [FREE_ASSETS.md](../../Documentation/FREE_ASSETS.md) - Asset resources
- [SCENE_CLEANUP_GUIDE.md](../../Documentation/SCENE_CLEANUP_GUIDE.md) - Scene setup
