# Chicken Coop Documentation Index 📚

Welcome to the Chicken Coop game documentation! This folder contains comprehensive documentation for developers, designers, and contributors.

---

## Documentation Files

### Core Documentation

| Document | Description |
|----------|-------------|
| [GAME_STORY.md](./GAME_STORY.md) | The game's narrative as a movie script - understand the player journey |
| [FARM_SYSTEMS.md](./FARM_SYSTEMS.md) | Detailed documentation of core farming mechanics |
| [HELPER_CLASSES.md](./HELPER_CLASSES.md) | Documentation for utility and helper classes |
| [UPGRADE_SYSTEM.md](./UPGRADE_SYSTEM.md) | Upgrade mechanics, economy balance, and progression |

### Development Guides

| Document | Description |
|----------|-------------|
| [DEPLOYMENT_FIX.md](./DEPLOYMENT_FIX.md) | **CRITICAL** - Fixes for missing scripts and WebGL deployment issues |
| [SCENE_CLEANUP_GUIDE.md](./SCENE_CLEANUP_GUIDE.md) | **NEW** - Step-by-step guide to fix scene issues in Unity Editor |
| [CODE_SAMPLES.md](./CODE_SAMPLES.md) | **NEW** - Code patterns, singleton usage, and best practices |
| [PREFAB_SPECIFICATIONS.md](./PREFAB_SPECIFICATIONS.md) | **NEW** - Prefab structure and component requirements |
| [SPRITE_SPECIFICATIONS.md](./SPRITE_SPECIFICATIONS.md) | **NEW** - Complete sprite asset specifications |
| [UI_SETUP.md](./UI_SETUP.md) | Guide to creating and configuring the game UI in Unity |
| [MISSING_FEATURES.md](./MISSING_FEATURES.md) | Gap analysis and recommended improvements |
| [FREE_ASSETS.md](./FREE_ASSETS.md) | Free asset recommendations to replace placeholders |
| [GAME_IDEAS.md](./GAME_IDEAS.md) | Future feature ideas and expansion concepts |

---

## Quick Links

### For New Developers

1. **START HERE**: Read [../UNITY_EDITOR_TASKS.md](../UNITY_EDITOR_TASKS.md) - Complete checklist for fixing issues!
2. **Fix Scene Issues**: Follow [SCENE_CLEANUP_GUIDE.md](./SCENE_CLEANUP_GUIDE.md)
3. **Learn Code Patterns**: Review [CODE_SAMPLES.md](./CODE_SAMPLES.md)
4. Set up UI: [UI_SETUP.md](./UI_SETUP.md) - create missing UI elements
5. Understand gameplay: [GAME_STORY.md](./GAME_STORY.md) to understand the player experience
6. Learn mechanics: [FARM_SYSTEMS.md](./FARM_SYSTEMS.md) to understand core mechanics
7. Review utilities: [HELPER_CLASSES.md](./HELPER_CLASSES.md) for utility functions

### For Designers

1. Review [UPGRADE_SYSTEM.md](./UPGRADE_SYSTEM.md) for economy balance
2. Check [MISSING_FEATURES.md](./MISSING_FEATURES.md) for UX gaps
3. Explore [GAME_IDEAS.md](./GAME_IDEAS.md) for expansion possibilities

### For Artists

1. **Sprite Requirements**: [SPRITE_SPECIFICATIONS.md](./SPRITE_SPECIFICATIONS.md) - Complete sprite specs
2. **Import Guide**: [../Assets/Sprites/README.md](../Assets/Sprites/README.md) - Unity import settings
3. See [FREE_ASSETS.md](./FREE_ASSETS.md) for free asset resources
4. Review visual style in [GAME_STORY.md](./GAME_STORY.md)

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    CHICKEN COOP ARCHITECTURE                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────┐                                           │
│  │   MANAGERS       │                                           │
│  │   (Singletons)   │                                           │
│  ├──────────────────┤                                           │
│  │ • GameManager    │◄─── Central state, events, save/load     │
│  │ • AudioManager   │◄─── Sound effects, music                  │
│  │ • UIManager      │◄─── UI updates, buttons, animations       │
│  │ • TweenHelper    │◄─── Animation utilities                   │
│  └──────────────────┘                                           │
│           │                                                      │
│           │ Events & References                                  │
│           ▼                                                      │
│  ┌──────────────────┐                                           │
│  │   GAME OBJECTS   │                                           │
│  │   (IInteractable)│                                           │
│  ├──────────────────┤                                           │
│  │ • HarvestableField│◄── Corn production                       │
│  │ • Chicken         │◄── Egg production                        │
│  │ • StoreCounter    │◄── Coin conversion                       │
│  │ • CollectibleEgg  │◄── Spawned collectibles                  │
│  │ • PlayerController│◄── Player input/movement                 │
│  └──────────────────┘                                           │
│           │                                                      │
│           │ Automated by                                         │
│           ▼                                                      │
│  ┌──────────────────┐                                           │
│  │   HELPERS        │                                           │
│  ├──────────────────┤                                           │
│  │ • HelperAI       │◄── Automated game loop                    │
│  │ • EnvironmentAnim│◄── Ambient animations                     │
│  └──────────────────┘                                           │
│                                                                  │
│  ┌──────────────────┐                                           │
│  │ SCRIPTABLE OBJ   │                                           │
│  ├──────────────────┤                                           │
│  │ • GameConfig     │◄── Game balance settings                  │
│  │ • UpgradeData    │◄── Individual upgrade configs             │
│  └──────────────────┘                                           │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Game Loop Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    THE CORE GAME LOOP                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│      ┌─────────┐                                                │
│      │  START  │                                                │
│      └────┬────┘                                                │
│           │                                                      │
│           ▼                                                      │
│      ┌─────────┐         ┌─────────────────┐                   │
│      │ HARVEST │ ──────► │ Corn +1         │                   │
│      │  CORN   │         │ Cooldown starts │                   │
│      └────┬────┘         └─────────────────┘                   │
│           │                                                      │
│           ▼                                                      │
│      ┌─────────┐         ┌─────────────────┐                   │
│      │  FEED   │ ──────► │ Corn -1         │                   │
│      │ CHICKEN │         │ Egg production  │                   │
│      └────┬────┘         └─────────────────┘                   │
│           │                                                      │
│           ▼                                                      │
│      ┌─────────┐         ┌─────────────────┐                   │
│      │ COLLECT │ ──────► │ Egg +1          │                   │
│      │   EGG   │         │ Egg destroyed   │                   │
│      └────┬────┘         └─────────────────┘                   │
│           │                                                      │
│           ▼                                                      │
│      ┌─────────┐         ┌─────────────────┐                   │
│      │  SELL   │ ──────► │ Egg -1          │                   │
│      │   EGG   │         │ Coins +10       │                   │
│      └────┬────┘         └─────────────────┘                   │
│           │                                                      │
│           │                                                      │
│           ▼                                                      │
│      ┌─────────┐                                                │
│      │ UPGRADE │◄─── Spend coins on:                           │
│      │   OR    │     • Hire helpers (automation)               │
│      │  HIRE   │     • Buy upgrades (multipliers)              │
│      └────┬────┘                                                │
│           │                                                      │
│           └──────────────────────────────────────────┐          │
│                                                       │          │
│                                                       ▼          │
│                                               [REPEAT LOOP]     │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Key Metrics

### Current Balance

| Metric | Value |
|--------|-------|
| Starting Coins | 50 |
| Egg Sell Price | 10 |
| First Helper Cost | 100 |
| Loop Time (Manual) | ~6-8 seconds |
| Loop Time (Helper) | ~7-8 seconds |
| Upgrade Multiplier | 1.2x |

### Progression Targets

| Milestone | Coins Needed | Approx. Time |
|-----------|--------------|--------------|
| First Helper | 100 | 1-2 minutes |
| Second Helper | 250 (total) | 3-4 minutes |
| All Upgrades | ~1,850 | 10-15 minutes |
| Idle Empire | 5,000+ | 30+ minutes |

---

## Contributing

When adding new features:

1. Document the feature in the appropriate file
2. Update this index if adding new documentation
3. Follow existing code patterns and conventions
4. Add XML documentation comments to C# code
5. Test WebGL build before submitting

---

## File Structure Reference

```
ChickenCoop/
├── Assets/
│   ├── Scripts/
│   │   ├── Managers/          # Singleton managers
│   │   ├── GameObjects/       # Interactable objects
│   │   ├── Helpers/           # Utility scripts
│   │   ├── UI/                # UI management
│   │   └── ScriptableObjects/ # Data containers
│   ├── Prefabs/               # Reusable objects
│   ├── Scenes/                # Game scenes
│   ├── Sprites/               # Visual assets (TODO)
│   └── Audio/                 # Sound assets (TODO)
├── Documentation/             # This folder!
│   ├── INDEX.md               # You are here
│   ├── GAME_STORY.md
│   ├── FARM_SYSTEMS.md
│   ├── HELPER_CLASSES.md
│   ├── UPGRADE_SYSTEM.md
│   ├── MISSING_FEATURES.md
│   ├── FREE_ASSETS.md
│   └── GAME_IDEAS.md
├── docs/                      # WebGL build output
└── README.md                  # Project overview
```
