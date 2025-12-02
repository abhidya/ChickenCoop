# Chicken Coop 🐔

A cute, animated idle-farming game built with Unity 2D. Harvest corn, feed chickens, collect eggs, and grow your farm!

![Game Screenshot](https://img.shields.io/badge/Unity-2022.3%2B-black?logo=unity)
![License](https://img.shields.io/badge/License-MIT-green)
![Platform](https://img.shields.io/badge/Platform-WebGL-blue)

## 🔧 Important: Recent Fixes Applied

**Critical issues have been fixed!** If you previously had problems with:
- "Missing script" errors when building
- WebGL builds not loading (gzip compression errors)
- No UI visible in the game

→ **See [FIXES_APPLIED.md](./FIXES_APPLIED.md)** for what was fixed and next steps.

→ **See [Documentation/UI_SETUP.md](./Documentation/UI_SETUP.md)** to create the game UI.

## 🎮 Play Online

Play the game directly in your browser once built to WebGL!

## 📚 Documentation

Comprehensive documentation is available in the [Documentation](./Documentation/) folder:

| Document | Description |
|----------|-------------|
| [🛠️ FIXES_APPLIED](./FIXES_APPLIED.md) | **START HERE** - Recent critical fixes applied |
| [📖 INDEX](./Documentation/INDEX.md) | Documentation overview and architecture |
| [🚀 DEPLOYMENT_FIX](./Documentation/DEPLOYMENT_FIX.md) | Deployment and script reference fixes |
| [🎨 UI_SETUP](./Documentation/UI_SETUP.md) | How to create the game UI in Unity |
| [🎬 GAME_STORY](./Documentation/GAME_STORY.md) | The game narrative as a movie script |
| [🌾 FARM_SYSTEMS](./Documentation/FARM_SYSTEMS.md) | Core farming mechanics documentation |
| [🔧 HELPER_CLASSES](./Documentation/HELPER_CLASSES.md) | Utility classes and helpers |
| [📈 UPGRADE_SYSTEM](./Documentation/UPGRADE_SYSTEM.md) | Economy balance and progression |
| [🔍 MISSING_FEATURES](./Documentation/MISSING_FEATURES.md) | Gap analysis and improvements |
| [🎨 FREE_ASSETS](./Documentation/FREE_ASSETS.md) | Recommended free assets |
| [💡 GAME_IDEAS](./Documentation/GAME_IDEAS.md) | Future feature ideas |

## 🌟 Features

### Core Gameplay
- **Harvest Corn**: Tap on the corn field to harvest corn with satisfying squash-and-stretch animations
- **Feed Chickens**: Use corn to feed your adorable chickens
- **Collect Eggs**: Watch chickens lay eggs with cute bounce animations
- **Sell at Store**: Sell eggs for coins with rewarding coin burst effects

### Visual Polish
- ✨ Smooth tweened movement between farm locations
- 🎨 Bright, chibi-style visuals with soft pastel colors
- 💫 Particle effects for harvesting, collecting, and purchasing
- 🐤 Animated characters with blinking, bobbing, and wiggling
- 🌾 Environmental animations (swaying leaves, bouncing corn stalks)

### Automation & Progression
- 👷 **Hire Helpers**: Automated helpers perform the complete game loop
- 📈 **Upgrades**: Improve corn fields, chicken production, egg prices, and more
- 💰 **Economy**: Strategic resource management and upgrade paths

## 🎯 Game Loop

1. **Harvest** corn from the corn field
2. **Feed** corn to the chicken
3. **Collect** the egg that the chicken lays
4. **Sell** the egg at the store counter
5. **Hire helpers** and **buy upgrades** to automate and improve!

## 🛠️ Technical Details

### Project Structure

```
ChickenCoop/
├── Assets/
│   ├── Scripts/
│   │   ├── Managers/
│   │   │   ├── GameManager.cs      # Central game state and resource management
│   │   │   └── AudioManager.cs     # Sound effects and music
│   │   ├── GameObjects/
│   │   │   ├── PlayerController.cs # Player movement and interaction
│   │   │   ├── HarvestableField.cs # Corn field mechanics
│   │   │   ├── Chicken.cs          # Chicken behavior and animations
│   │   │   └── StoreCounter.cs     # Store selling mechanics
│   │   ├── Helpers/
│   │   │   ├── HelperAI.cs         # Automated helper behavior
│   │   │   └── EnvironmentAnimator.cs # Ambient animations
│   │   ├── UI/
│   │   │   ├── UIManager.cs        # UI updates and animations
│   │   │   └── TweenHelper.cs      # Animation utility functions
│   │   └── ScriptableObjects/
│   │       ├── UpgradeData.cs      # Upgrade configuration
│   │       └── GameConfig.cs       # Game balance settings
│   ├── Prefabs/                    # Reusable game objects
│   ├── Scenes/
│   │   └── MainGame.unity          # Main game scene
│   ├── Sprites/                    # Visual assets
│   └── Animations/                 # Animation files
├── ProjectSettings/                # Unity project settings
├── Packages/                       # Package dependencies
└── docs/                           # WebGL build for GitHub Pages
```

### Key Scripts

| Script | Description |
|--------|-------------|
| `GameManager` | Singleton managing all game state, resources, and events |
| `PlayerController` | Handles player character movement with smooth tweening |
| `HelperAI` | State machine for automated helper characters |
| `HarvestableField` | Corn harvesting with cooldowns and animations |
| `Chicken` | Feeding, egg laying, and cute animations |
| `StoreCounter` | Egg selling with coin effects |
| `UIManager` | Resource displays, buttons, and UI animations |
| `TweenHelper` | Utility class for smooth animations |

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3 LTS or later
- WebGL build support module

### Running Locally
1. Clone the repository
2. Open the project in Unity
3. Open `Assets/Scenes/MainGame.unity`
4. Press Play to test

### Building for WebGL
1. Go to **File > Build Settings**
2. Select **WebGL** platform
3. Click **Build**
4. Copy build files to `docs/` folder for GitHub Pages

## 📱 Controls

- **Click/Tap** on objects to interact
- **Click/Tap** anywhere to move the farmer
- **UI Buttons** for quick actions

## 🎨 Art Style

The game features a cute, chibi-style art direction with:
- Rounded, expressive characters
- Soft pastel color palette
- Smooth animations and transitions
- Particle effects for visual feedback

## 🔊 Audio

- Procedural sound effects for all interactions
- Satisfying feedback sounds for:
  - Harvesting corn
  - Collecting eggs
  - Selling at store
  - Purchasing upgrades
  - Hiring helpers

## 📄 License

This project is open source and available under the MIT License.

## 🤝 Contributing

Contributions are welcome! Please review the [Documentation](./Documentation/) before contributing:

1. Read [MISSING_FEATURES.md](./Documentation/MISSING_FEATURES.md) for known gaps
2. Check [GAME_IDEAS.md](./Documentation/GAME_IDEAS.md) for planned features
3. Review [FREE_ASSETS.md](./Documentation/FREE_ASSETS.md) for asset guidelines

Ways to contribute:
- Report bugs
- Suggest features
- Submit pull requests
- Improve documentation

## 🙏 Credits

Created with ❤️ using Unity