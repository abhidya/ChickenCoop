# Deployment Fix Documentation

## Critical Issues Fixed

### Issue 1: Missing Scripts in Unity Scene

**Problem**: When building the game, Unity reported missing scripts on GameObjects:
- Player
- CornField (HarvestableField)
- Chicken
- StoreCounter
- EventSystem

**Root Cause**: The `.gitignore` file was excluding `*.cs.meta` files from version control. Unity uses .meta files to track GUIDs for every asset, including scripts. Without these files:
- Unity generates NEW random GUIDs when someone clones the repository
- Scene files contain hardcoded GUIDs that no longer match the scripts
- Result: "Missing script" errors and broken GameObject references

**Solution**: 
1. Removed `*.cs.meta` from `.gitignore`
2. Generated .meta files for all C# scripts with the correct GUIDs that match the scene file
3. Committed all .meta files to version control

**IMPORTANT**: Never exclude .cs.meta files from git in Unity projects! They are essential for maintaining script references.

---

### Issue 2: Multiple Classes in Single File

**Problem**: 
- `IInteractable` interface was defined at the end of `PlayerController.cs`
- `CollectibleEgg` class was defined at the end of `Chicken.cs`

Unity requires each MonoBehaviour and interface to be in its own file with a matching name.

**Solution**:
1. Extracted `IInteractable` into `IInteractable.cs`
2. Extracted `CollectibleEgg` into `CollectibleEgg.cs`
3. Removed duplicate definitions from original files

---

### Issue 3: WebGL Gzip Compression Headers

**Problem**: GitHub Pages serves gzipped Unity WebGL builds without proper `Content-Encoding: gzip` header, causing this error:

```
Failed to parse binary data file Build/build.data.gz, 
because it is still gzip-compressed. It should have been 
uncompressed by the browser, but it was unable to do so 
since the web server provided the compressed content 
without specifying the HTTP Response Header 
"Content-Encoding: gzip"
```

**Solution**: Created `build/_headers` file to configure GitHub Pages headers:

```
/Build/*.gz
  Content-Encoding: gzip
  Cache-Control: max-age=31536000
```

This tells GitHub Pages to serve gzip files with the correct Content-Encoding header so browsers automatically decompress them.

---

## How to Deploy to GitHub Pages

### Step 1: Build the Game in Unity

1. Open Unity and load the ChickenCoop project
2. Go to **File > Build Settings**
3. Select **WebGL** platform
4. Click **Switch Platform** if not already on WebGL
5. Click **Build** (not "Build and Run")
6. Save the build to the `build/` folder in the repository

### Step 2: Ensure Headers File Exists

Check that `build/_headers` exists with the content:

```
/Build/*.gz
  Content-Encoding: gzip
  Cache-Control: max-age=31536000

/Build/*.data.gz
  Content-Type: application/octet-stream
  Content-Encoding: gzip
  Cache-Control: max-age=31536000

/Build/*.wasm.gz
  Content-Type: application/wasm
  Content-Encoding: gzip
  Cache-Control: max-age=31536000

/Build/*.framework.js.gz
  Content-Type: application/javascript
  Content-Encoding: gzip
  Cache-Control: max-age=31536000
```

### Step 3: Commit and Push

```bash
git add build/
git commit -m "Update WebGL build"
git push
```

### Step 4: Configure GitHub Pages

1. Go to your repository on GitHub
2. Click **Settings** > **Pages**
3. Under "Source", select the branch containing the build (usually `main` or `master`)
4. Set the folder to `/build` or `/` (depending on your setup)
5. Click **Save**

### Step 5: Wait and Test

- GitHub Pages takes 1-2 minutes to deploy
- Visit `https://[username].github.io/ChickenCoop/` to play the game

---

## Troubleshooting

### Still Seeing "Missing Script" Errors?

1. **Delete the Library folder** in your Unity project (Unity will regenerate it)
2. Reopen the project in Unity
3. Check that all .meta files are present in `Assets/Scripts/`
4. Verify the GUIDs match by opening a .meta file and checking the scene file

### Still Seeing Gzip Errors on GitHub Pages?

1. Clear your browser cache
2. Verify `build/_headers` exists and is committed
3. Try using a different browser
4. Wait a few minutes for GitHub Pages cache to clear
5. Check that the build files are actually gzipped: `file build/Build/*.gz`

### Game Shows Green Screen with Circles?

This is normal if:
- Sprites haven't been added yet (the game uses placeholder colored circles)
- To add proper sprites, see `Assets/Art/README.md`

However, if you also see no UI (no counters, buttons, progress bars):
- Check that the Canvas GameObject exists in the scene
- Verify UIManager script is attached to ResourcePanel
- Check that TextMeshPro is installed in the project

---

## Script GUID Mapping Reference

For future reference, these are the correct GUIDs that must be maintained:

| Script File | GUID | Used By |
|------------|------|---------|
| PlayerController.cs | 347b4b538ad3badeab6979ec9b9dd6d3 | Player GameObject |
| Chicken.cs | 707068cab87ceb98b88261b2eb470c4c | Chicken GameObject |
| HarvestableField.cs | e1de2e8561bff172e89d3ddbe0f217a7 | CornField GameObject |
| StoreCounter.cs | 8ff73fda39151091f82355b6d0e86a83 | Store GameObject |
| UIManager.cs | fe87c0e1cc204ed48ad3b37840f39efc | ResourcePanel GameObject |
| TutorialManager.cs | 59f8146938fff824cb5fd77236b75775 | ButtonPanel GameObject |
| GameManager.cs | b2ae56abc646fb995281c90c7d877b85 | GameManager GameObject |
| AudioManager.cs | 9d2de54af5a16d5ed8889255fa3af2aa | AudioManager GameObject |

**Never change these GUIDs manually!** If you need to, let Unity regenerate them and update the scene file accordingly.

---

## Testing Locally

To test the WebGL build locally without deploying:

1. Build the game to the `build/` folder
2. Start a local web server:
   ```bash
   cd build
   python3 -m http.server 8000
   ```
3. Open `http://localhost:8000` in your browser

Note: Some browsers (like Chrome) have security restrictions on local files. Using a local web server is required.

---

## Summary

The main fixes applied:
1. ✅ Separated classes into individual files (IInteractable, CollectibleEgg)
2. ✅ Fixed .gitignore to track .cs.meta files
3. ✅ Generated correct .meta files with scene-matching GUIDs
4. ✅ Added _headers file for GitHub Pages gzip support

The game should now:
- ✅ Build without "missing script" warnings
- ✅ Deploy correctly to GitHub Pages
- ✅ Load and run in the browser without gzip errors
- ✅ Show gameplay (even if using placeholder graphics)
