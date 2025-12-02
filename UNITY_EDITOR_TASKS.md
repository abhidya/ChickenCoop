# Unity Editor Tasks - Manual Steps Required

This document lists all tasks that MUST be performed in Unity Editor to complete the fixes described in the problem statement.

## ⚠️ Important Note

This PR provides **tools, documentation, and code** to help fix the issues, but the actual fixes require opening Unity Editor because:

1. **Scene files** are binary/YAML that should not be manually edited
2. **Missing script references** must be removed through Unity's Inspector
3. **Duplicate GameObjects** must be identified and deleted in the Hierarchy
4. **Prefabs** must be created and saved through Unity
5. **Sorting Layers** must be configured in Project Settings

## Prerequisites

- Unity Editor 6000.2.14f1 (or compatible version)
- This repository cloned locally
- All files from this PR committed

## Task Checklist

### Phase 1: Initial Scan (5 minutes)

- [ ] Open Unity Editor and load the project
- [ ] Open scene: `Assets/Scenes/MainGame.unity`
- [ ] Check Console window for initial error count
- [ ] Take screenshot of Console (for comparison later)
- [ ] Take screenshot of Hierarchy (showing duplicates)
- [ ] Take screenshot of Game View (current visual state)

### Phase 2: Fix Missing Scripts (10-15 minutes)

- [ ] **Run utility**: `Tools > Find Missing Scripts in Scene`
- [ ] Review list of GameObjects with missing scripts
- [ ] For each GameObject with missing scripts:
  - [ ] Select in Hierarchy
  - [ ] Locate "Missing (Mono Script)" component in Inspector
  - [ ] Click gear icon (⚙️) > "Remove Component"
  - [ ] Repeat for all missing components
- [ ] **Run utility**: `Tools > Remove All Missing Scripts` (batch operation)
- [ ] Verify Console shows 0 missing script warnings
- [ ] Save scene: `File > Save` (Ctrl+S)

### Phase 3: Remove Duplicate GameObjects (10 minutes)

- [ ] **Run utility**: `Tools > Find Duplicate GameObjects`
- [ ] Review duplicates found (expected: Player, Chicken, Background, CornField)
- [ ] For each duplicate set:
  - [ ] Decide which instance to KEEP (usually the first one)
  - [ ] Select duplicate instance in Hierarchy
  - [ ] Press Delete
  - [ ] Check Console for any new errors
- [ ] Verify only one of each exists:
  - [ ] One "Player" GameObject
  - [ ] One "Chicken" GameObject
  - [ ] One "Background" GameObject
  - [ ] One "CornField" GameObject
- [ ] Save scene

### Phase 4: Fix Player Registration (5 minutes)

- [ ] Check how many GameObjects have "Player" tag:
  - Search Hierarchy: `t:GameObject`
  - Check Tag column or Inspector for each
- [ ] Ensure ONLY ONE GameObject has tag "Player"
- [ ] If multiple found, set extras to "Untagged"
- [ ] Enter Play Mode and check Console
- [ ] Verify "Player already registered" warning does NOT appear
- [ ] Exit Play Mode
- [ ] Save scene

### Phase 5: Configure Sorting Layers (5 minutes)

- [ ] Open: `Edit > Project Settings > Tags and Layers`
- [ ] Scroll to "Sorting Layers" section
- [ ] Add layers if missing (click +):
  - [ ] "Background" (order: -100 or lowest)
  - [ ] "Foreground" (order: 0 or middle)
  - [ ] "Characters" (order: 100)
  - [ ] "UI" (order: 200 or highest)
- [ ] Reorder layers by dragging (back to front order)
- [ ] Close Project Settings

### Phase 6: Assign Sorting Layers to GameObjects (10 minutes)

For each GameObject with a SpriteRenderer:

**Background**:
- [ ] Select "Background" GameObject
- [ ] In SpriteRenderer component, set Sorting Layer: "Background"
- [ ] Set Order in Layer: 0

**Platform/Environment**:
- [ ] Select platform/corn field GameObjects
- [ ] Set Sorting Layer: "Foreground"
- [ ] Set Order in Layer: 0

**Characters (Player, Chicken, Egg)**:
- [ ] Select "Player" GameObject
- [ ] Set Sorting Layer: "Characters"
- [ ] Set Order in Layer: 100
- [ ] Repeat for Chicken (order: 90) and Egg (order: 50)

**UI Elements**:
- [ ] If UI Canvas exists, ensure it's Screen Space - Overlay
- [ ] Set UI sorting order to highest

- [ ] Save scene

### Phase 7: Create/Fix Prefabs (15 minutes)

**Player Prefab**:
- [ ] Select Player GameObject in Hierarchy
- [ ] Verify it has:
  - [ ] SpriteRenderer component
  - [ ] PlayerController script
  - [ ] Tag: "Player"
  - [ ] Sorting Layer: "Characters"
- [ ] Drag Player from Hierarchy to `Assets/Prefabs/` folder
- [ ] Name: "Player.prefab"
- [ ] Verify prefab was created (blue icon)

**Chicken Prefab**:
- [ ] Select Chicken GameObject
- [ ] Verify components (SpriteRenderer, Chicken script)
- [ ] Drag to `Assets/Prefabs/`
- [ ] Name: "Chicken.prefab"

**Egg Prefab**:
- [ ] Select Egg GameObject (or create if missing)
- [ ] Add SpriteRenderer if missing
- [ ] Add CollectibleEgg script if missing
- [ ] Drag to `Assets/Prefabs/`
- [ ] Name: "Egg.prefab"

**Check existing prefabs**:
- [ ] Open `Assets/Prefabs/Helper.prefab`
- [ ] Check for missing scripts
- [ ] Remove any missing components
- [ ] Save prefab

### Phase 8: Create Placeholder Sprites (OPTIONAL) (10 minutes)

If you want temporary placeholder sprites:

- [ ] **Run utility**: `Tools > Create Placeholder Sprites`
- [ ] Click "Create All Placeholder Sprites"
- [ ] Wait for completion
- [ ] Check `Assets/Sprites/` for created PNG files
- [ ] For each sprite:
  - [ ] Select in Project window
  - [ ] In Inspector, set Texture Type: "Sprite (2D and UI)"
  - [ ] Set Pixels Per Unit: 32
  - [ ] Click Apply

**Update GameObjects with sprites**:
- [ ] Select Player GameObject
- [ ] In SpriteRenderer, assign: `Sprites/Player/player_idle`
- [ ] Repeat for other GameObjects

### Phase 9: Fix UI Overlapping Text (5 minutes)

- [ ] Find large white overlapping text in Hierarchy
- [ ] Possible names: "Text", "Label", "Statistics", "Debug Text"
- [ ] Options:
  - **Delete** if it's placeholder/test text
  - **Disable** if it's debug overlay (uncheck GameObject)
  - **Move to UI Canvas** if it's legitimate UI
  - **Resize/reposition** if it's needed but wrongly placed

### Phase 10: Organize Scene Hierarchy (5 minutes)

- [ ] Create empty GameObjects for organization (optional):
  - [ ] "─── ENVIRONMENT ───"
  - [ ] "─── CHARACTERS ───"
  - [ ] "─── UI ───"
- [ ] Group related objects under parents
- [ ] Recommended final structure:
  ```
  MainGame
  ├── Main Camera
  ├── GameManager
  ├── EventSystem
  ├── Background
  ├── Platform
  ├── CornField
  ├── Chicken
  ├── Egg
  ├── Player
  ├── StoreCounter
  └── Canvas (UI)
  ```
- [ ] Save scene

### Phase 11: Test and Validate (10 minutes)

- [ ] Clear Console: Click "Clear" or Ctrl+Shift+C
- [ ] Enter Play Mode (Ctrl+P)
- [ ] Check Console - should see:
  - [ ] ✅ 0 errors
  - [ ] ✅ 0 missing script warnings
  - [ ] ✅ No "Player already registered" warning
- [ ] Check Game View:
  - [ ] Sprites display correctly
  - [ ] No overlapping text
  - [ ] Objects in correct visual order
- [ ] Test interactions:
  - [ ] Click on objects
  - [ ] Verify player movement works
  - [ ] Verify game mechanics work
- [ ] Exit Play Mode
- [ ] Test at different resolutions:
  - [ ] Game View > Resolution dropdown
  - [ ] Try 1920x1080
  - [ ] Try 1366x768
  - [ ] Verify UI and sprites look correct
- [ ] Take screenshots:
  - [ ] Console (showing 0 errors)
  - [ ] Hierarchy (showing clean structure)
  - [ ] Game View (showing improved visuals)

### Phase 12: Save and Commit (5 minutes)

- [ ] Save scene: `File > Save` (Ctrl+S)
- [ ] Save project: `File > Save Project` (Ctrl+Shift+S)
- [ ] In Unity: Check for unsaved changes (asterisk on tabs)
- [ ] Close Unity Editor
- [ ] In Git:
  ```bash
  git status
  git add Assets/Scenes/MainGame.unity
  git add Assets/Prefabs/*.prefab
  git add Assets/Sprites/  # if created sprites
  git commit -m "Unity Editor: Fixed missing scripts, removed duplicates, configured scene"
  git push
  ```

## Estimated Total Time

- **Minimum (if no issues)**: 60 minutes
- **Expected (typical issues)**: 90 minutes
- **Maximum (major issues)**: 120 minutes

## Troubleshooting

### "I can't find the Tools menu items"

**Problem**: Editor scripts not compiled yet

**Solution**:
1. Wait for Unity to finish compiling scripts
2. Check Console for compilation errors
3. Fix any script errors
4. Wait for "Compiling..." to finish

### "Utility scripts have errors"

**Problem**: Missing Unity namespaces

**Solution**:
1. Ensure scripts are in `Assets/Editor/` folder
2. Verify `using UnityEditor;` is present
3. Reimport scripts: Right-click > Reimport

### "Deleted duplicate but references broke"

**Problem**: Other objects were referencing the deleted instance

**Solution**:
1. Check Console for NullReferenceException
2. Find objects with empty reference fields
3. Drag correct GameObject into empty field
4. Or use script to find and reassign references

### "Missing scripts keep coming back"

**Problem**: Prefab has missing scripts

**Solution**:
1. Open prefab directly (double-click in Project)
2. Remove missing scripts from prefab
3. Save prefab
4. Reopen scene

### "Sprites don't appear"

**Problem**: Incorrect import settings or sorting layer

**Solution**:
1. Select sprite in Project
2. Check Texture Type is "Sprite (2D and UI)"
3. Check Pixels Per Unit (try 32)
4. Click Apply
5. Check SpriteRenderer is enabled
6. Check Sorting Layer is visible

## Additional Resources

- [SCENE_CLEANUP_GUIDE.md](Documentation/SCENE_CLEANUP_GUIDE.md) - Detailed cleanup steps
- [SPRITE_SPECIFICATIONS.md](Documentation/SPRITE_SPECIFICATIONS.md) - Sprite requirements
- [CODE_SAMPLES.md](Documentation/CODE_SAMPLES.md) - Code patterns
- [Assets/Sprites/README.md](Assets/Sprites/README.md) - Sprite import guide

## After Completion

When all tasks are done:

1. Verify acceptance criteria:
   - [ ] Console: zero "missing script" warnings
   - [ ] Play mode: no "Player already registered" warning
   - [ ] Single Player instance registered
   - [ ] Prefabs updated and saved
   - [ ] Visuals improved (no colored circles, proper sprites)
   - [ ] Sorting layers configured correctly
   - [ ] No duplicate GameObjects

2. Create before/after comparison:
   - [ ] Attach screenshots to PR
   - [ ] Document changes made
   - [ ] List any remaining issues

3. Update documentation:
   - [ ] Mark tasks as complete in this file
   - [ ] Add any lessons learned
   - [ ] Note any deviations from plan

## Questions or Issues?

If you encounter problems not covered here:

1. Check Unity Console for specific error messages
2. Search Unity Documentation: https://docs.unity3d.com/
3. Review the detailed guides in `Documentation/` folder
4. Create GitHub issue with:
   - Screenshot of error
   - Steps to reproduce
   - Unity version
   - What you've tried
