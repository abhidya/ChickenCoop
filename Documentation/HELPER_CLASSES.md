# Helper Classes Documentation 🔧

This document provides comprehensive documentation for all helper and utility classes in Chicken Coop.

---

## Table of Contents

1. [HelperAI](#helperai)
2. [EnvironmentAnimator](#environmentanimator)
3. [TweenHelper](#tweenhelper)
4. [IInteractable Interface](#iinteractable-interface)
5. [CollectibleEgg](#collectibleegg)

---

## HelperAI

**Location:** `Assets/Scripts/Helpers/HelperAI.cs`

### Purpose
Automated helper character that continuously performs the complete game loop: harvest corn → feed chicken → collect egg → sell at store.

### State Machine

```
┌─────────────────────────────────────────────────────────────────┐
│                      HELPER STATE MACHINE                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────┐                                                        │
│  │ Idle │ ──────────────────────────────────────────────┐       │
│  └──┬───┘                                                │       │
│     │ Start                                              │       │
│     ▼                                                    │       │
│  ┌────────────────┐      ┌────────────────┐             │       │
│  │ MovingToCorn   │ ───► │ HarvestingCorn │             │       │
│  └────────────────┘      └───────┬────────┘             │       │
│                                  │                       │       │
│                                  ▼                       │       │
│  ┌────────────────┐      ┌────────────────┐             │       │
│  │ MovingToChicken│ ◄─── │                │             │       │
│  └───────┬────────┘      └────────────────┘             │       │
│          │                                               │       │
│          ▼                                               │       │
│  ┌────────────────┐      ┌────────────────┐             │       │
│  │ FeedingChicken │ ───► │ CollectingEgg  │             │       │
│  └────────────────┘      └───────┬────────┘             │       │
│                                  │                       │       │
│                                  ▼                       │       │
│  ┌────────────────┐      ┌────────────────┐             │       │
│  │ MovingToStore  │ ◄─── │                │             │       │
│  └───────┬────────┘      └────────────────┘             │       │
│          │                                               │       │
│          ▼                                               │       │
│  ┌────────────────┐                                      │       │
│  │ SellingEgg     │ ─────────────────────────────────────┘       │
│  └────────────────┘                                              │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Public Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `moveSpeed` | float | 3f | Movement speed in units/second |
| `waitTime` | float | 1f | Pause duration between loops |
| `bobAmount` | float | 0.15f | Vertical bob amplitude |
| `bobSpeed` | float | 10f | Bob animation frequency |
| `helperColor` | Color | (0.9, 0.8, 0.6) | Base tint color |

### Key Methods

#### `StartHelperLoop()`
Main coroutine that runs indefinitely, executing the game loop.

```csharp
private IEnumerator StartHelperLoop()
{
    yield return new WaitForSeconds(Random.Range(0.5f, 2f)); // Stagger start
    while (true)
    {
        yield return GoToAndHarvestCorn();
        yield return GoToAndFeedChicken();
        yield return CollectEgg();
        yield return GoToAndSellEgg();
        yield return new WaitForSeconds(waitTime / GameManager.Instance.SpeedMultiplier);
    }
}
```

#### `MoveTo(Vector3 position)`
Smooth movement with easing to target position.

#### Visual Effects
- `SpawnDustPuff()` - Creates dust particle when moving
- `SpawnSparkle()` - Creates sparkle effect when collecting
- `PlaySquashStretch()` - Cartoon-style action feedback

### Usage Example

```csharp
// Spawning a new helper
GameObject helper = Instantiate(helperPrefab, spawnPosition, Quaternion.identity);
// The helper automatically begins its loop on Start()
```

---

## EnvironmentAnimator

**Location:** `Assets/Scripts/Helpers/EnvironmentAnimator.cs`

### Purpose
Adds ambient animations to the farm environment for visual polish. Handles swaying leaves, bouncing objects, and ambient particle effects.

### Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `swayAmount` | float | 5f | Maximum sway angle in degrees |
| `swaySpeed` | float | 1f | Sway animation speed |
| `randomOffset` | float | 1f | Variation between objects |
| `swayingObjects` | Transform[] | - | Objects to apply sway animation |
| `bouncingObjects` | Transform[] | - | Objects to apply bounce animation |

### Key Methods

#### `UpdateSwaying()`
Applies sinusoidal rotation to all registered swaying objects.

```csharp
float sway = Mathf.Sin(Time.time * swaySpeed + swayOffsets[i]) * swayAmount;
swayingObjects[i].localRotation = Quaternion.Euler(0, 0, originalRotations[i].z + sway);
```

#### `UpdateBouncing()`
Applies subtle scale pulsing to bouncing objects.

```csharp
float bounce = 1f + Mathf.Sin(Time.time * swaySpeed * 0.5f + bounceOffsets[i]) * 0.03f;
bouncingObjects[i].localScale = originalScales[i] * bounce;
```

#### `SpawnDustPuff(Vector3 position)`
Creates a dust particle effect at the specified position.

**Particle Settings:**
- Size: 0.15
- Lifetime: 0.4s
- Color: Tan (0.85, 0.75, 0.65, 0.4)
- Count: 4 particles

#### `SpawnSparkles(Vector3 position)`
Creates a sparkle effect for upgrades/special events.

**Particle Settings:**
- Size: 0.1
- Lifetime: 0.6s
- Color: Yellow → White gradient
- Count: 15 particles

#### `CreateAmbientParticles()`
Creates persistent floating motes for atmosphere.

**Particle Settings:**
- Size: 0.05
- Lifetime: 5s
- Continuous emission: 5/second
- Coverage: 10x6 unit box

### Usage Example

```csharp
// Add to scene and assign objects
EnvironmentAnimator animator = gameObject.AddComponent<EnvironmentAnimator>();

// Trigger effects programmatically
animator.SpawnSparkles(transform.position);
animator.SpawnDustPuff(transform.position);
```

---

## TweenHelper

**Location:** `Assets/Scripts/UI/TweenHelper.cs`

### Purpose
Utility singleton providing smooth animation methods for transforms and sprites. Central animation system for the entire game.

### Available Tweens

| Method | Parameters | Description |
|--------|------------|-------------|
| `MoveTo` | target, endPosition, duration, onComplete | Smooth position tween |
| `ScaleTo` | target, endScale, duration, onComplete | Smooth scale tween |
| `PunchScale` | target, punchAmount, duration | Scale up then back |
| `SquashStretch` | target, squashAmount, duration | Cartoon deformation |
| `RotateTo` | target, endRotation, duration, onComplete | Smooth rotation |
| `Wobble` | target, angle, duration, oscillations | Wiggle animation |
| `FadeTo` | spriteRenderer, endAlpha, duration, onComplete | Alpha fade |
| `ColorTo` | spriteRenderer, endColor, duration, onComplete | Color transition |
| `Bounce` | target, height, duration | Jump up and down |
| `Shake` | target, intensity, duration | Screen shake effect |
| `PopIn` | target, duration | Scale from 0 with overshoot |
| `PopOut` | target, duration, onComplete | Scale to 0 |

### Easing Functions

```csharp
// Quadratic ease out - smooth deceleration
float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

// Quadratic ease in - smooth acceleration  
float EaseInQuad(float t) => t * t;

// Back ease - overshoot effect
float EaseOutBack(float t) // Overshoots then settles

// Bounce ease - bouncing settle
float EaseOutBounce(float t) // Multiple bounces
```

### Usage Examples

```csharp
// Move an object smoothly
TweenHelper.MoveTo(transform, targetPosition, 0.5f, () => {
    Debug.Log("Movement complete!");
});

// Punch scale for feedback
TweenHelper.PunchScale(button.transform, 0.3f, 0.2f);

// Squash and stretch for cartoon effect
TweenHelper.SquashStretch(character.transform, 0.3f, 0.3f);

// Fade out a sprite
TweenHelper.FadeTo(spriteRenderer, 0f, 0.5f, () => {
    Destroy(gameObject);
});

// Pop in animation
TweenHelper.PopIn(newObject.transform, 0.3f);
```

### Implementation Notes

- **Singleton Pattern:** Access via `TweenHelper.Instance`
- **Coroutine-Based:** All tweens run as coroutines
- **Callback Support:** Optional `onComplete` for chaining
- **Time.deltaTime:** Framerate independent

---

## IInteractable Interface

**Location:** `Assets/Scripts/GameObjects/PlayerController.cs`

### Purpose
Common interface for all interactive objects in the game world.

### Definition

```csharp
public interface IInteractable
{
    void Interact();
    bool CanInteract();
}
```

### Implementing Classes

| Class | Interact() Behavior | CanInteract() Logic |
|-------|---------------------|---------------------|
| `HarvestableField` | Harvest corn | `canHarvest == true` |
| `Chicken` | Feed chicken | `!isLayingEgg && Corn >= required` |
| `StoreCounter` | Sell egg | `canSell && Eggs > 0` |
| `CollectibleEgg` | Collect egg | `!isCollected` |

### Usage in PlayerController

```csharp
private void HandleInput()
{
    if (Input.GetMouseButtonDown(0))
    {
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider != null)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                MoveToAndInteract(hit.collider.transform.position, interactable);
                return;
            }
        }
    }
}
```

---

## CollectibleEgg

**Location:** `Assets/Scripts/GameObjects/Chicken.cs`

### Purpose
Spawned egg object that can be clicked to add to inventory.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `isCollected` | bool | Prevents double-collection |

### Methods

#### `Interact()`
Collects the egg if not already collected.

#### `Collect()`
```csharp
private void Collect()
{
    isCollected = true;
    GameManager.Instance.AddEgg(1);
    StartCoroutine(CollectAnimation());
}
```

#### `CollectAnimation()`
- Spawns glow effect
- Scales down to zero
- Moves upward
- Destroys gameObject

### Visual Effects

**Glow Effect Particles:**
- Size: 0.15
- Lifetime: 0.4s
- Color: Light yellow (1, 1, 0.6, 0.8)
- Count: 12 particles
- Gravity: -0.5 (floats upward)

---

## Class Relationships Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    HELPER CLASSES RELATIONSHIPS                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────┐                    ┌──────────────────┐       │
│  │ GameManager  │◄───────────────────│    HelperAI      │       │
│  │  (Singleton) │    Gets positions  │ (State Machine)  │       │
│  └──────┬───────┘    and resources   └────────┬─────────┘       │
│         │                                      │                 │
│         │ Provides                             │ Uses            │
│         ▼                                      ▼                 │
│  ┌──────────────┐                    ┌──────────────────┐       │
│  │    Chicken   │                    │  TweenHelper     │       │
│  │ StoreCounter │ ◄──────────────────│  (Animation)     │       │
│  │ HarvestField │   IInteractable    └──────────────────┘       │
│  └──────────────┘                              ▲                 │
│         ▲                                      │                 │
│         │                                      │ Uses            │
│         │ Spawns                               │                 │
│         │                            ┌─────────┴────────┐       │
│  ┌──────┴───────┐                    │ Environment      │       │
│  │CollectibleEgg│                    │ Animator         │       │
│  │(IInteractable)│                   │ (Ambient FX)     │       │
│  └──────────────┘                    └──────────────────┘       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```
