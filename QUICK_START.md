# Quick Start - Fix ChickenCoop Issues

## 🚀 5-Minute Overview

This PR provides **tools and documentation** to fix the Unity project issues. You must use Unity Editor to complete the fixes.

## 📋 What You Need to Do

### Step 1: Open Unity Editor (2 min)
```bash
# Clone repo if not already done
git clone https://github.com/abhidya/ChickenCoop.git
cd ChickenCoop

# Checkout this branch
git checkout copilot/fix-scripts-remove-duplicates

# Open in Unity Hub or Unity Editor
# Required version: Unity 6000.2.14f1 or compatible
```

### Step 2: Run Utilities (10 min)
In Unity Editor menu:

1. **Remove Missing Scripts**
   - `Tools > Remove All Missing Scripts`
   - Confirm: Yes
   - Result: 0 missing script warnings

2. **Find Duplicates**
   - `Tools > Find Duplicate GameObjects`
   - Note which GameObjects are duplicated
   - Manually delete duplicates in Hierarchy

3. **Fix Player Tag**
   - Find all GameObjects with "Player" tag
   - Ensure only ONE has the tag
   - Set others to "Untagged"

4. **Import Pretty Assets** ⭐ NEW!
   - `Window > Asset Store`
   - Search "Low Poly Farm Pack Lite" by JustCreate
   - Click "Add to My Assets" + "Import"
   - Get professional chickens, corn, farm buildings - FREE!
   - See [UNITY_ASSET_STORE_GUIDE.md](./Documentation/UNITY_ASSET_STORE_GUIDE.md)

### Step 3: Validate (5 min)
- `Tools > Validate Fix Requirements`
- Address any failures
- Re-run until all checks pass

### Step 4: Save & Commit (2 min)
```bash
# In Unity Editor
File > Save
File > Save Project

# In terminal
git add Assets/Scenes/MainGame.unity
git add Assets/Prefabs/*.prefab
git commit -m "Fixed missing scripts, removed duplicates, configured scene"
git push
```

## 🛠️ Tools Provided

| Tool | Menu Location | Purpose |
|------|---------------|---------|
| Find Missing Scripts | `Tools > Find Missing Scripts` | Scan for broken references |
| Remove Missing Scripts | `Tools > Remove All Missing Scripts` | Batch cleanup |
| Find Duplicates | `Tools > Find Duplicate GameObjects` | Identify duplicates |
| Create Placeholders | `Tools > Create Placeholder Sprites` | Generate temp sprites |
| Validate | `Tools > Validate Fix Requirements` | Check acceptance criteria |

## 📚 Documentation

| Document | When to Use |
|----------|-------------|
| **[UNITY_EDITOR_TASKS.md](./UNITY_EDITOR_TASKS.md)** | **Complete step-by-step checklist** |
| [SCENE_CLEANUP_GUIDE.md](./Documentation/SCENE_CLEANUP_GUIDE.md) | Detailed scene fixing instructions |
| [SPRITE_SPECIFICATIONS.md](./Documentation/SPRITE_SPECIFICATIONS.md) | When creating/importing sprites |
| [PR_SUMMARY.md](./PR_SUMMARY.md) | Understanding what this PR does |

## ⚠️ Common Issues

### "Tools menu is missing"
**Fix**: Wait for Unity to finish compiling scripts (check bottom-right of Unity)

### "Still see missing script warnings"
**Fix**: Open prefabs directly and remove missing scripts from them too

### "Deleted wrong duplicate"
**Fix**: Undo (Ctrl+Z) and check which GameObject has the required components

### "Sprites don't appear"
**Fix**: Check sprite import settings (Texture Type: Sprite 2D and UI)

## ✅ Success Criteria

When done, you should have:
- [ ] Console: 0 errors, 0 "missing script" warnings
- [ ] Play Mode: No "Player already registered" warning
- [ ] Hierarchy: No duplicate GameObjects
- [ ] Scene: Improved visuals (sprites or better placeholders)
- [ ] Validation tool: All checks pass

## 🎯 Time Estimate

- **Minimum** (if no issues): 30 minutes
- **Expected** (typical): 60 minutes  
- **Maximum** (complex issues): 90 minutes

## 🆘 Need More Help?

1. **Start with**: [UNITY_EDITOR_TASKS.md](./UNITY_EDITOR_TASKS.md) - Complete checklist
2. **Detailed guide**: [SCENE_CLEANUP_GUIDE.md](./Documentation/SCENE_CLEANUP_GUIDE.md)
3. **Validation**: Run `Tools > Validate Fix Requirements`
4. **Still stuck**: Check Troubleshooting section in UNITY_EDITOR_TASKS.md

## 📞 Questions?

Create GitHub issue with:
- Unity Console screenshot
- Validation tool output
- What you've tried
- Unity Editor version

---

**Ready?** Open Unity Editor → Follow [UNITY_EDITOR_TASKS.md](./UNITY_EDITOR_TASKS.md) → Run `Tools > Validate Fix Requirements` when done! 🎮
