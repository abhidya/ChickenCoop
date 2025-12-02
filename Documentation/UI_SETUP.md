# UI Setup Guide

## Issue: No UI Visible in Game

If you're seeing just a green screen with 5 circles and names but no:
- Counter displays (corn, eggs, coins)
- Progress bars
- Buttons
- Money indicator

This means the UI Canvas needs to be properly set up in the Unity scene.

---

## Understanding the Current State

The game currently shows:
- ✅ **GameObjects with placeholder sprites** (colored circles):
  - Player (blue circle)
  - Chicken (yellow circle)
  - CornField (green circle)
  - Store (brown circle)
  - Background (large green area)
- ❌ **No UI elements visible** - this needs to be fixed!

---

## Why the UI is Missing

The UIManager script expects many UI components that need to be created in the scene:

### Required UI Elements:

1. **Canvas** (with Canvas Scaler and Graphic Raycaster)
2. **EventSystem** (for button clicks)
3. **Resource Displays** (TextMeshPro elements):
   - Corn counter
   - Eggs counter
   - Coins counter
   - Helper counter
   - Income rate display
4. **Action Buttons**:
   - Harvest button
   - Feed button
   - Collect button
   - Sell button
   - Hire Helper button
5. **Progress Bars**:
   - Corn field progress
   - Egg production progress
6. **Panels**:
   - Resource panel (top of screen)
   - Button panel (bottom of screen)
   - Upgrade panel

---

## Quick Fix: Run the Scene in Unity Editor

The easiest way to see if the UI exists:

1. Open the project in Unity Editor
2. Open `Assets/Scenes/MainGame.unity`
3. Look at the Hierarchy panel
4. Expand the "Canvas" GameObject
5. Check if all the UI children exist

### If Canvas exists but UI is invisible:
- Check Canvas Scaler settings
- Verify Camera is set correctly
- Check that TextMeshPro font asset exists

### If Canvas doesn't exist:
- You'll need to create the UI from scratch (see below)

---

## Creating the UI from Scratch

If the Canvas and UI elements are missing, here's how to create them:

### Step 1: Create Canvas

1. Right-click in Hierarchy
2. **UI > Canvas**
3. Select Canvas, in Inspector:
   - **Render Mode**: Screen Space - Overlay
   - Add **Canvas Scaler** component:
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920 x 1080
     - Match: 0.5 (balance between width/height)

### Step 2: Create EventSystem

1. Right-click in Hierarchy
2. **UI > Event System** (usually auto-created with Canvas)

### Step 3: Create Resource Panel (Top)

```
Canvas
├── ResourcePanel (Panel)
│   ├── CornDisplay (HorizontalLayoutGroup)
│   │   ├── CornIcon (Image)
│   │   └── CornText (TextMeshPro)
│   ├── EggsDisplay (HorizontalLayoutGroup)
│   │   ├── EggsIcon (Image)
│   │   └── EggsText (TextMeshPro)
│   └── CoinsDisplay (HorizontalLayoutGroup)
│       ├── CoinsIcon (Image)
│       └── CoinsText (TextMeshPro)
```

**Configuration**:
- ResourcePanel: Anchor to top, full width
- Each Display: Horizontal Layout Group with spacing
- Text: TextMeshPro - Text (UI), Font size 36-48

### Step 4: Create Button Panel (Bottom)

```
Canvas
└── ButtonPanel (Panel)
    ├── HarvestButton (Button)
    ├── FeedButton (Button)
    ├── CollectButton (Button)
    ├── SellButton (Button)
    └── HireHelperButton (Button)
```

**Configuration**:
- ButtonPanel: Anchor to bottom, full width
- Use Horizontal Layout Group for auto-spacing
- Each button: 150x60 pixels minimum

### Step 5: Wire Up UIManager

1. Create empty GameObject named "UIManager"
2. Add **UIManager** script component
3. In Inspector, drag and drop:
   - All TextMeshPro text elements to corresponding fields
   - All buttons to button fields
   - Resource icons to icon fields
   - Progress bars to progress fields

---

## Minimal Working UI (Quick Start)

If you just want to see SOMETHING working, create this minimal UI:

### 1. Canvas + EventSystem (as above)

### 2. Resource Panel

```
- Canvas
  └── ResourcePanel (at top)
      └── ResourceText (TextMeshPro)
          Text: "Corn: 0 | Eggs: 0 | Coins: 0"
```

### 3. Quick Buttons

```
- Canvas
  └── ButtonPanel (at bottom)
      ├── HarvestBtn (Button with "Harvest" text)
      ├── FeedBtn (Button with "Feed" text)
      └── SellBtn (Button with "Sell" text)
```

### 4. Attach to UIManager

Even if you don't fill in ALL fields, attach what you have. The UIManager will handle null fields gracefully.

---

## TextMeshPro Setup

If TextMeshPro isn't installed:

1. **Window > Package Manager**
2. Search for "TextMeshPro"
3. Click **Install**
4. When prompted, click **Import TMP Essentials**

---

## Debugging UI Issues

### Canvas not visible?
- Check Camera settings
- Set Canvas to "Screen Space - Overlay"
- Verify Canvas is enabled

### Buttons not clickable?
- Check EventSystem exists
- Verify Canvas has GraphicRaycaster component
- Make sure buttons are enabled and interactable

### Text not showing?
- TextMeshPro needs font asset
- Use default TMP font or assign one
- Check text color (white on white = invisible!)

### UI too small/large?
- Adjust Canvas Scaler reference resolution
- Change font sizes
- Modify button sizes

---

## Example UIManager Setup

Here's what a properly configured UIManager looks like in Inspector:

```
UIManager Component:
├── Resource Displays
│   ├── Corn Count Text: [CornText]
│   ├── Eggs Count Text: [EggsText]
│   ├── Coins Count Text: [CoinsText]
│   ├── Helper Count Text: [HelperText]
│   └── Income Rate Text: [IncomeText]
├── Resource Icons
│   ├── Corn Icon: [CornIcon Transform]
│   ├── Eggs Icon: [EggsIcon Transform]
│   └── Coins Icon: [CoinsIcon Transform]
├── Action Buttons
│   ├── Harvest Button: [HarvestButton]
│   ├── Feed Button: [FeedButton]
│   ├── Collect Button: [CollectButton]
│   ├── Sell Button: [SellButton]
│   └── Hire Helper Button: [HireHelperButton]
├── Upgrade System
│   └── Upgrade Panel: [UpgradePanel]
├── Goal Display
│   └── Next Goal Text: [GoalText]
└── Progress Indicators
    ├── Corn Progress Bar: [CornBar Image]
    └── Egg Progress Bar: [EggBar Image]
```

---

## Next Steps

1. **Open Unity and check** if Canvas exists
2. **Create missing UI elements** using the guide above
3. **Connect UIManager** to all UI elements in Inspector
4. **Test in Play mode** to verify functionality
5. **Build and test** the WebGL version

---

## Getting Help

If the UI still doesn't appear after following this guide:

1. Check Unity Console for errors
2. Verify UIManager.Instance is not null (check console for errors)
3. Make sure GameManager is in the scene
4. Try the minimal UI setup first, then expand

## Related Documentation

- See [DEPLOYMENT_FIX.md](./DEPLOYMENT_FIX.md) for script reference issues
- See [Assets/Art/README.md](../Assets/Art/README.md) for adding sprites
- See [INDEX.md](./INDEX.md) for architecture overview
