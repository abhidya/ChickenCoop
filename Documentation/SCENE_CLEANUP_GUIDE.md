# Scene Cleanup Guide - MainGame.unity

This guide walks through fixing the duplicate GameObjects, missing scripts, and visual issues in the MainGame scene.

## Current Issues (from screenshot)

### Console Errors
- ❌ **8 warnings**: "The referenced script (Unknown) on this Behaviour is missing!"
- ❌ **1 info**: "Default GameObject Tag: Player already registered"
- ❌ **250 errors**: (Type unknown - likely related to missing scripts)

### Hierarchy Issues
- ❌ **Duplicate GameObjects**: Multiple instances of Player, Chicken, Background, CornField
- ❌ **Missing script references**: Some GameObjects have components pointing to deleted/renamed scripts

### Visual Issues
- ❌ Large overlapping white text (placeholder UI text)
- ❌ Colored circles instead of proper sprites
- ❌ Green platform looks unpolished

## Step-by-Step Cleanup Process

### Phase 1: Find and Fix Missing Scripts

1. **Use the Find Missing Scripts Utility**
   ```
   Tools > Find Missing Scripts in Scene
   ```
   - This will scan the entire scene and prefabs
   - Lists all GameObjects with missing script references
   - Note down which objects have issues

2. **Inspect Each GameObject**
   - Select the GameObject in Hierarchy
   - Look in Inspector for components showing "Script: None (Mono Script)"
   - Or components showing "Missing (Mono Script)"

3. **Fix or Remove Missing Scripts**
   
   Option A: **Remove the broken component**
   - Click the gear icon (⚙️) next to the missing script
   - Select "Remove Component"
   
   Option B: **Reassign the correct script**
   - Drag the correct .cs file from Project window
   - Drop it onto the component slot
   
   Option C: **Delete and recreate the GameObject**
   - If too many missing scripts, delete the GameObject
   - Recreate from prefab or manually add components

4. **Save the Scene**
   - File > Save Scene (Ctrl+S)
   - Verify Console shows 0 missing script warnings

### Phase 2: Remove Duplicate GameObjects

**Current Hierarchy** (with duplicates):
```
MainGame
├── Main Camera
├── GameManager
├── Player                    ← FIRST
├── CornField                 ← FIRST
├── Chicken                   ← FIRST
├── StoreCounter
├── Background                ← FIRST
├── EventSystem
├── Player                    ← DUPLICATE (DELETE)
├── Chicken                   ← DUPLICATE (DELETE)
├── Background                ← DUPLICATE (DELETE)
├── CornField                 ← DUPLICATE (DELETE)
├── Store
└── Egg
```

**Clean up steps**:

1. **Identify which duplicates to keep**
   - Keep the FIRST instance of each GameObject
   - Check if it has all required components
   - Check if it has proper references set

2. **Delete duplicate GameObjects**
   - Select the duplicate in Hierarchy
   - Press Delete key
   - Confirm deletion
   - Repeat for: Player (2nd), Chicken (2nd), Background (2nd), CornField (2nd)

3. **Verify no broken references**
   - After deleting, check Console for new errors
   - If references broke, reassign them in Inspector

**Target Hierarchy** (after cleanup):
```
MainGame
├── Main Camera
├── GameManager
├── EventSystem
├── Background
├── Platform (the green oval)
├── CornField
├── Chicken
├── Egg
├── Player
├── StoreCounter
└── UI Canvas (may need to create)
```

### Phase 3: Fix Player Registration Issue

The "Default GameObject Tag: Player already registered" warning suggests one of these issues:

**Diagnosis**:
1. Check how many GameObjects have the "Player" tag:
   ```
   Search in Hierarchy: t:GameObject "Player"
   ```
2. Look for any script trying to programmatically add the "Player" tag

**Fix Option 1: Remove duplicate Player tags**
- Select each Player GameObject
- In Inspector, check the Tag dropdown
- Only ONE GameObject should have tag "Player"
- Set others to "Untagged"

**Fix Option 2: Check for tag registration code**
- Search all scripts for:
  ```csharp
  TagManager.AddTag("Player")
  // or
  Tags.Create("Player")
  ```
- Remove or comment out this code (Player tag already exists by default)

**Fix Option 3: Ensure singleton pattern**
- GameManager already uses proper singleton
- Player GameObject should NOT have duplicate instances
- If Player is being spawned at runtime, ensure only one is created

### Phase 4: Consolidate Scene Structure

**Recommended final hierarchy**:

```
MainGame (empty root GameObject)
├── Main Camera
│   └── (camera settings)
├── GameManager
│   ├── GameManager script
│   └── (reference to positions)
├── EventSystem
│   └── (UI event handler)
├── ─── ENVIRONMENT ───
├── Background
│   ├── SpriteRenderer (lawn.png)
│   └── sortingLayer: Background
├── Platform
│   ├── SpriteRenderer (platform.png)
│   ├── sortingLayer: Foreground
│   └── BoxCollider2D (optional)
├── ─── GAME OBJECTS ───
├── CornField
│   ├── SpriteRenderer (corn_field_tile.png)
│   ├── HarvestableField script
│   ├── BoxCollider2D
│   └── sortingLayer: Foreground
├── ChickenGroup (empty parent)
│   └── Chicken
│       ├── SpriteRenderer (chicken_idle.png)
│       ├── Chicken script
│       ├── Animator
│       ├── BoxCollider2D
│       └── sortingLayer: Characters
├── StoreCounter
│   ├── SpriteRenderer (store icon)
│   ├── StoreCounter script
│   ├── BoxCollider2D
│   └── sortingLayer: Foreground
├── ─── PLAYER ───
├── Player (Tag: Player)
│   ├── SpriteRenderer (player_idle.png)
│   ├── PlayerController script
│   ├── Animator
│   ├── Rigidbody2D (optional)
│   ├── BoxCollider2D
│   └── sortingLayer: Characters
├── ─── UI ───
└── Canvas
    ├── Canvas (Screen Space - Overlay)
    ├── Canvas Scaler
    ├── Graphic Raycaster
    ├── Title (TextMeshPro or Image)
    ├── CoinDisplay
    │   ├── Icon (Image)
    │   └── Text (TextMeshPro)
    ├── CornDisplay
    │   ├── Icon (Image)
    │   └── Text (TextMeshPro)
    ├── EggDisplay
    │   ├── Icon (Image)
    │   └── Text (TextMeshPro)
    └── Buttons
        ├── HireHelperButton
        └── UpgradeButton
```

**Steps to organize**:

1. **Create organizational empty GameObjects** (optional but recommended):
   ```
   Right-click in Hierarchy > Create Empty
   Name: "─── ENVIRONMENT ───" (use Unicode for visual separator)
   ```

2. **Group related objects**:
   - Drag CornField, Platform under a "Farm" parent
   - Drag all Chickens under "ChickenGroup"
   - Keep Player at root level (easier to find)

3. **Position objects logically**:
   - Platform at bottom-center: (0, -3, 0)
   - Player on platform: (0, -2, 0)
   - Chicken on platform: (2, -2, 0)
   - CornField on platform: (-2, -2, 0)
   - Store on platform: (4, -2, 0)

### Phase 5: Fix Visual Issues

**Remove overlapping text**:

1. Find the large white text GameObject in Hierarchy
2. It's likely named "Text" or "Label" or "Statistics"
3. Two options:
   - Delete it if it's placeholder/test text
   - Move it to UI Canvas if it's legitimate UI
   - Disable it if it's debug overlay (uncheck in Inspector)

**Replace colored circles with sprites**:

1. **For each GameObject** (Player, Chicken, Egg, etc.):
   - Select the GameObject
   - Find the SpriteRenderer component
   - In the Sprite field, click the circle selector
   - Choose a proper sprite from Assets/Sprites/
   - If sprites don't exist yet, see SPRITE_SPECIFICATIONS.md

2. **Adjust sprite sizes**:
   - Scale Transform: (1, 1, 1) to start
   - If too big/small, adjust Pixels Per Unit in sprite import settings
   - Or adjust Transform scale: (0.5, 0.5, 1) for example

**Fix the platform**:

The green oval is currently a simple colored shape. Options:

1. **Keep it simple**: 
   - Add a better sprite with texture/gradient
   - Or use a Unity Sprite Shape for rounded rectangle

2. **Replace with sprite**:
   - Import platform.png (512x128)
   - Assign to SpriteRenderer
   - Set sortingLayer to "Foreground"

3. **Use UI Image** (not recommended for game world objects):
   - Convert to Canvas > UI Image

### Phase 6: Configure Sorting Layers

1. **Open Tags and Layers**:
   ```
   Edit > Project Settings > Tags and Layers
   ```

2. **Add Sorting Layers** (if not already present):
   - Click "+" to add new layer
   - Name: "Background"
   - Name: "Foreground"
   - Name: "Characters"
   - Name: "UI"

3. **Reorder layers** (drag to correct position):
   - Background (back)
   - Foreground
   - Characters
   - UI (front)

4. **Assign layers to GameObjects**:
   - Select GameObject
   - In SpriteRenderer component
   - Set Sorting Layer dropdown
   - Set Order in Layer (number)

### Phase 7: Update Prefabs

If you modified GameObjects that are prefab instances:

1. **Check if GameObject is a prefab instance**:
   - Look for blue cube icon in Hierarchy
   - Or "Prefab" label at top of Inspector

2. **Apply changes to prefab**:
   - In Inspector, click "Overrides" dropdown
   - Click "Apply All"
   - Or right-click GameObject > Prefab > Apply All

3. **Or break prefab connection**:
   - If you don't want to update prefab
   - Right-click > Prefab > Unpack Completely

### Phase 8: Create Missing Prefabs

**Create Player prefab**:

1. **Set up Player GameObject** (if not already good):
   - Add SpriteRenderer with player sprite
   - Add PlayerController script
   - Add Animator (optional)
   - Add BoxCollider2D (for interactions)
   - Set Tag to "Player"
   - Set sortingLayer to "Characters"

2. **Save as prefab**:
   - Drag Player GameObject from Hierarchy
   - Drop into Assets/Prefabs/ folder
   - Name: "Player.prefab"

**Repeat for**:
- Chicken.prefab
- Egg.prefab
- CornField.prefab
- StoreCounter.prefab

### Phase 9: Test and Validate

1. **Clear Console**: 
   - Click "Clear" button in Console
   - Or Ctrl+Shift+C

2. **Enter Play Mode**:
   - Click Play button (Ctrl+P)
   - Watch Console for new errors/warnings

3. **Check that issues are resolved**:
   - [ ] Zero "missing script" warnings
   - [ ] No "Player already registered" warning
   - [ ] No duplicate GameObjects visible
   - [ ] Sprites display correctly
   - [ ] Sorting layers show objects in correct order
   - [ ] Game is playable (can click and interact)

4. **Test at different resolutions**:
   - Game tab > Resolution dropdown
   - Try 1920x1080 (Full HD)
   - Try 1366x768 (common laptop)
   - Verify UI and sprites look good at both

5. **Check prefabs**:
   - Verify prefabs have no missing scripts
   - Test instantiating prefabs at runtime (if applicable)

### Phase 10: Final Save and Commit

1. **Save all changes**:
   - File > Save Scene (Ctrl+S)
   - File > Save Project (Ctrl+Shift+S)

2. **Check what changed**:
   - In Unity: Version Control > Show Incoming Changes
   - Or in Git: `git status`

3. **Test one more time**:
   - Close and reopen Unity
   - Open MainGame.unity
   - Enter Play Mode
   - Verify everything still works

4. **Commit changes**:
   ```bash
   git add Assets/Scenes/MainGame.unity
   git add Assets/Prefabs/*.prefab
   git commit -m "Fix missing scripts, remove duplicates, clean up scene"
   ```

## Troubleshooting

### "I still see missing script warnings"

**Check**:
- Did you save the scene after removing components?
- Are there missing scripts on prefabs (not just scene objects)?
- Run Tools > Find Missing Scripts to locate them all

**Fix**:
- Open each prefab directly (double-click in Project)
- Remove missing scripts from prefab
- Save prefab
- Reopen scene

### "Deleting duplicates broke references"

**Symptoms**:
- GameManager shows "None" for CornFieldPosition
- Scripts can't find Player GameObject

**Fix**:
- Select GameManager
- In Inspector, find empty reference fields
- Drag the correct GameObject from Hierarchy into the field
- Or click circle selector and choose from list

### "Player registration warning still appears"

**Check**:
- How many GameObjects have Tag "Player"?
  ```
  Search: t:GameObject Player
  ```
- Is there code calling TagManager.AddTag("Player")?

**Fix**:
- Ensure ONLY ONE GameObject has "Player" tag
- Remove any code trying to programmatically add "Player" tag
- Player tag is built-in, doesn't need to be registered

### "Sprites are too big/small"

**Adjust**:
1. Select sprite asset in Project
2. In Inspector, change "Pixels Per Unit"
   - Lower number = bigger sprite
   - Higher number = smaller sprite
3. Click Apply
4. Alternatively, adjust GameObject Transform scale

### "Can't see my sprites"

**Check**:
1. Is SpriteRenderer enabled? (checkbox in Inspector)
2. Is sprite assigned in SpriteRenderer?
3. Is sorting layer correct?
4. Is Z position 0? (should be for 2D)
5. Is camera positioned to see the sprites?

**Camera settings**:
- Position: (0, 0, -10)
- Size (Orthographic): 5-10
- Projection: Orthographic
- Clear Flags: Solid Color

## Editor Utility Scripts

### Remove All Missing Scripts (Batch)

Create `Assets/Editor/RemoveMissingScripts.cs`:

```csharp
using UnityEngine;
using UnityEditor;

public class RemoveMissingScripts : EditorWindow
{
    [MenuItem("Tools/Remove All Missing Scripts")]
    static void RemoveAll()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int removedCount = 0;
        
        foreach (GameObject go in allObjects)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            removedCount += removed;
        }
        
        Debug.Log($"Removed {removedCount} missing scripts from scene");
        EditorUtility.DisplayDialog("Complete", $"Removed {removedCount} missing scripts", "OK");
    }
}
```

### Find Duplicate GameObjects

Create `Assets/Editor/FindDuplicates.cs`:

```csharp
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FindDuplicates : EditorWindow
{
    [MenuItem("Tools/Find Duplicate GameObjects")]
    static void Find()
    {
        Dictionary<string, List<GameObject>> objectsByName = new Dictionary<string, List<GameObject>>();
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.transform.parent == null) // Root objects only
            {
                if (!objectsByName.ContainsKey(obj.name))
                {
                    objectsByName[obj.name] = new List<GameObject>();
                }
                objectsByName[obj.name].Add(obj);
            }
        }
        
        bool foundDuplicates = false;
        foreach (var kvp in objectsByName)
        {
            if (kvp.Value.Count > 1)
            {
                Debug.LogWarning($"Found {kvp.Value.Count} objects named '{kvp.Key}'");
                foreach (GameObject obj in kvp.Value)
                {
                    Debug.Log($"  - {obj.name} at {obj.transform.position}", obj);
                }
                foundDuplicates = true;
            }
        }
        
        if (!foundDuplicates)
        {
            Debug.Log("No duplicate root GameObjects found!");
        }
    }
}
```

## Checklist

Use this checklist to track cleanup progress:

- [ ] Ran "Find Missing Scripts" utility
- [ ] Removed/fixed all missing script references
- [ ] Deleted duplicate Player GameObject
- [ ] Deleted duplicate Chicken GameObject
- [ ] Deleted duplicate Background GameObject
- [ ] Deleted duplicate CornField GameObject
- [ ] Verified only ONE GameObject has "Player" tag
- [ ] Fixed "Player already registered" warning
- [ ] Removed or moved overlapping white text
- [ ] Configured sorting layers (Background, Foreground, Characters, UI)
- [ ] Replaced colored circles with proper sprites (or documented for artist)
- [ ] Created/updated Player prefab
- [ ] Created/updated Chicken prefab
- [ ] Created/updated Egg prefab
- [ ] Organized hierarchy with logical grouping
- [ ] Tested in Play Mode - no Console errors
- [ ] Tested at 1920x1080 resolution
- [ ] Tested at 1366x768 resolution
- [ ] Saved scene
- [ ] Saved project
- [ ] Committed changes to git

## See Also

- [FindMissingScripts.cs](../Assets/Editor/FindMissingScripts.cs) - Utility to find missing scripts
- [SPRITE_SPECIFICATIONS.md](./SPRITE_SPECIFICATIONS.md) - Sprite asset details
- [CODE_SAMPLES.md](./CODE_SAMPLES.md) - Code patterns and examples
- [UI_SETUP.md](./UI_SETUP.md) - UI creation guide
- [GAME_STORY.md](./GAME_STORY.md) - Visual reference for how game should look
