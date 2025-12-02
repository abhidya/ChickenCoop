# Prefab Specifications for ChickenCoop

This document specifies the structure and components for all game prefabs.

## Overview

Prefabs are reusable GameObject templates. Each prefab should:
- Have all required components configured
- Have no missing script references
- Use correct sorting layers
- Have proper collision/physics setup
- Be properly tagged (if applicable)

## Required Prefabs

### 1. Player.prefab

**Location**: `Assets/Prefabs/Player.prefab`

**Components**:
```yaml
GameObject: Player
  Tag: Player
  Layer: Default
  
  Transform:
    Position: (0, 0, 0)
    Rotation: (0, 0, 0)
    Scale: (1, 1, 1)
  
  SpriteRenderer:
    Sprite: player_idle
    Color: (1, 1, 1, 1)
    Sorting Layer: Characters
    Order in Layer: 100
    Flip X: false
    Flip Y: false
  
  PlayerController (Script):
    Move Speed: 5
    Bob Amount: 0.1
    Bob Speed: 8
    Sprite Renderer: (reference to SpriteRenderer)
    Animator: (reference to Animator if present)
  
  BoxCollider2D (optional):
    Size: (0.5, 0.5)
    Offset: (0, 0)
    Is Trigger: true
  
  Animator (optional):
    Controller: PlayerAnimatorController
    Avatar: None
    Apply Root Motion: false
```

**Usage**: Instantiate at game start or respawn points

**Notes**:
- Only ONE Player should exist in scene at a time
- Tag "Player" is used by GameManager to find player
- Movement speed can be adjusted per level/upgrade

---

### 2. Chicken.prefab

**Location**: `Assets/Prefabs/Chicken.prefab`

**Components**:
```yaml
GameObject: Chicken
  Tag: Untagged
  Layer: Default
  
  Transform:
    Position: (0, 0, 0)
    Rotation: (0, 0, 0)
    Scale: (1, 1, 1)
  
  SpriteRenderer:
    Sprite: chicken_idle
    Color: (1, 1, 1, 1)
    Sorting Layer: Characters
    Order in Layer: 90
    Flip X: false
    Flip Y: false
  
  Chicken (Script):
    Feed Duration: 2.0
    Lay Duration: 1.5
    Egg Prefab: (reference to Egg.prefab)
    Sprite Renderer: (reference to SpriteRenderer)
    Animator: (reference to Animator if present)
  
  BoxCollider2D:
    Size: (0.6, 0.6)
    Offset: (0, 0)
    Is Trigger: true
  
  Animator (optional):
    Controller: ChickenAnimatorController
    Avatar: None
    Apply Root Motion: false
```

**Usage**: Place in scene or spawn dynamically

**Notes**:
- Can have multiple chickens in scene
- Each chicken operates independently
- Produces eggs when fed corn

---

### 3. Egg.prefab

**Location**: `Assets/Prefabs/Egg.prefab`

**Components**:
```yaml
GameObject: Egg
  Tag: Untagged
  Layer: Default
  
  Transform:
    Position: (0, 0, 0)
    Rotation: (0, 0, 0)
    Scale: (1, 1, 1)
  
  SpriteRenderer:
    Sprite: egg
    Color: (1, 1, 1, 1)
    Sorting Layer: Characters
    Order in Layer: 50
    Flip X: false
    Flip Y: false
  
  CollectibleEgg (Script):
    Collection Delay: 0.5
    Auto Collect Radius: 2.0
    Sprite Renderer: (reference to SpriteRenderer)
  
  CircleCollider2D:
    Radius: 0.3
    Offset: (0, 0)
    Is Trigger: true
```

**Usage**: Spawned by Chicken after feeding

**Notes**:
- Automatically collected by player when nearby
- Can be clicked to collect
- Adds egg to inventory when collected

---

### 4. Helper.prefab

**Location**: `Assets/Prefabs/Helper.prefab` (already exists)

**Components**:
```yaml
GameObject: Helper
  Tag: Untagged
  Layer: Default
  
  Transform:
    Position: (0, 0, 0)
    Rotation: (0, 0, 0)
    Scale: (0.8, 0.8, 0.8)  # Slightly smaller than player
  
  SpriteRenderer:
    Sprite: helper_idle (or reuse player_idle with different color)
    Color: (0.8, 0.9, 1, 1)  # Slight blue tint
    Sorting Layer: Characters
    Order in Layer: 95
    Flip X: false
    Flip Y: false
  
  HelperAI (Script):
    Move Speed: 4.0
    Work Speed Multiplier: 1.0
    Sprite Renderer: (reference to SpriteRenderer)
  
  BoxCollider2D (optional):
    Size: (0.4, 0.4)
    Offset: (0, 0)
    Is Trigger: true
```

**Usage**: Spawned when player hires helpers

**Notes**:
- Automatically performs harvest-feed-collect-sell loop
- Multiple helpers can exist simultaneously
- Can be differentiated by hue/color variation

---

### 5. CornField.prefab (optional - may be scene-only)

**Location**: `Assets/Prefabs/CornField.prefab`

**Components**:
```yaml
GameObject: CornField
  Tag: Untagged
  Layer: Default
  
  Transform:
    Position: (0, 0, 0)
    Rotation: (0, 0, 0)
    Scale: (1, 1, 1)
  
  SpriteRenderer:
    Sprite: corn_field_tile
    Color: (1, 1, 1, 1)
    Sorting Layer: Foreground
    Order in Layer: 0
    Flip X: false
    Flip Y: false
  
  HarvestableField (Script):
    Corn Per Harvest: 1
    Harvest Cooldown: 2.0
    Visual Animator: (optional reference)
    Sprite Renderer: (reference to SpriteRenderer)
  
  BoxCollider2D:
    Size: (1, 1)
    Offset: (0, 0)
    Is Trigger: true
```

**Usage**: Place in scene (usually not instantiated at runtime)

**Notes**:
- Can have multiple fields for larger farms
- Cooldown prevents spam-clicking

---

### 6. StoreCounter.prefab (optional - may be scene-only)

**Location**: `Assets/Prefabs/StoreCounter.prefab`

**Components**:
```yaml
GameObject: StoreCounter
  Tag: Untagged
  Layer: Default
  
  Transform:
    Position: (0, 0, 0)
    Rotation: (0, 0, 0)
    Scale: (1, 1, 1)
  
  SpriteRenderer:
    Sprite: store_counter
    Color: (1, 1, 1, 1)
    Sorting Layer: Foreground
    Order in Layer: 10
    Flip X: false
    Flip Y: false
  
  StoreCounter (Script):
    Coin Particle Prefab: (optional particle effect)
    Sprite Renderer: (reference to SpriteRenderer)
  
  BoxCollider2D:
    Size: (1.2, 1)
    Offset: (0, 0)
    Is Trigger: true
```

**Usage**: Place in scene (usually static)

**Notes**:
- Players and helpers interact to sell eggs
- Triggers coin particle effects on sale

---

### 7. Platform.prefab (optional)

**Location**: `Assets/Prefabs/Platform.prefab`

**Components**:
```yaml
GameObject: Platform
  Tag: Untagged
  Layer: Default
  
  Transform:
    Position: (0, -3, 0)
    Rotation: (0, 0, 0)
    Scale: (1, 1, 1)
  
  SpriteRenderer:
    Sprite: platform
    Color: (1, 1, 1, 1)
    Sorting Layer: Foreground
    Order in Layer: -10
    Flip X: false
    Flip Y: false
```

**Usage**: Background element, usually one per scene

**Notes**:
- Visual element only (no collision needed)
- Characters stand "on" platform visually

---

## Prefab Creation Checklist

When creating a new prefab:

- [ ] All required components added
- [ ] No missing script references
- [ ] Correct Tag assigned (if needed)
- [ ] Correct Layer assigned
- [ ] Sorting Layer configured
- [ ] Order in Layer set appropriately
- [ ] Collider size matches sprite visually
- [ ] Script references assigned (SpriteRenderer, etc.)
- [ ] Default values set for all script parameters
- [ ] Prefab saved in `Assets/Prefabs/` folder
- [ ] Prefab tested in Play Mode

## Updating Existing Prefabs

If modifying an existing prefab:

1. **Open prefab** in Prefab Mode (double-click in Project)
2. **Make changes** in Inspector
3. **Test changes** in Prefab Mode if possible
4. **Save prefab** (Ctrl+S)
5. **Verify instances update** in scene
6. **Test in Play Mode**

## Prefab Variants (Advanced)

For variations of base prefabs (e.g., different chicken colors):

1. Right-click base prefab > Create > Prefab Variant
2. Name variant: `Chicken_Brown.prefab`
3. Override specific properties (sprite, color)
4. Keep all other components from base

## Nested Prefabs

For complex objects with child GameObjects:

```yaml
ParentPrefab
├── VisualRoot
│   └── SpriteRenderer
├── CollisionRoot
│   └── Collider2D
└── ScriptRoot
    └── BehaviorScript
```

**Benefits**:
- Easier to swap visuals without changing logic
- Better organization
- Cleaner Inspector

## Prefab Import Settings

Ensure .prefab files and their .meta files are tracked in Git:

```bash
git add Assets/Prefabs/*.prefab
git add Assets/Prefabs/*.prefab.meta
```

**DO NOT**:
- Manually edit .prefab files (use Unity Editor)
- Delete .meta files (causes broken references)
- Copy prefabs without their .meta files

## Testing Prefabs

### Unit Test Pattern

```csharp
[Test]
public void Prefab_HasRequiredComponents()
{
    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
    Assert.IsNotNull(prefab);
    Assert.IsNotNull(prefab.GetComponent<SpriteRenderer>());
    Assert.IsNotNull(prefab.GetComponent<PlayerController>());
}
```

### Runtime Test Pattern

```csharp
void TestPrefabInstantiation()
{
    GameObject instance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
    Assert.IsNotNull(instance);
    
    PlayerController controller = instance.GetComponent<PlayerController>();
    Assert.IsNotNull(controller);
    
    Destroy(instance);
}
```

## Common Issues

### Missing Script Reference

**Symptom**: Prefab shows "Script: None (Mono Script)" in Inspector

**Causes**:
- Script was deleted/renamed
- Script moved to different folder
- .meta file is missing

**Solution**:
1. Find correct script file
2. Drag script to empty slot in prefab
3. Or remove component and re-add

### Broken Prefab Connection

**Symptom**: GameObject no longer updates when prefab changes

**Cause**: Prefab connection was broken (Unpack Prefab was used)

**Solution**:
1. Delete GameObject instance
2. Drag prefab from Project to Hierarchy
3. Reconfigure any scene-specific overrides

### Prefab Overrides Not Applying

**Symptom**: Changes to prefab instance don't appear

**Solution**:
1. Select instance in Hierarchy
2. Click "Overrides" dropdown at top of Inspector
3. Click "Apply All" to push to prefab
4. Or "Revert All" to discard changes

## Prefab Best Practices

1. **Keep prefabs simple**: One responsibility per prefab
2. **Use variants**: Don't duplicate entire prefabs for small changes
3. **Document parameters**: Add tooltips to script fields
4. **Test thoroughly**: Instantiate and destroy in Play Mode
5. **Version control**: Always commit .prefab and .meta together
6. **Naming convention**: Use clear, descriptive names
7. **Organization**: Group related prefabs in subfolders

## See Also

- [SCENE_CLEANUP_GUIDE.md](./SCENE_CLEANUP_GUIDE.md) - Scene organization
- [CODE_SAMPLES.md](./CODE_SAMPLES.md) - Script patterns
- [SPRITE_SPECIFICATIONS.md](./SPRITE_SPECIFICATIONS.md) - Visual assets
- [Unity Prefabs Manual](https://docs.unity3d.com/Manual/Prefabs.html)
