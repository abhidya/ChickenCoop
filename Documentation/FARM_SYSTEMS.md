# Farm Systems Documentation 🌾

This document details the core farming systems in Chicken Coop, including harvestable fields, chicken production, store mechanics, and the player controller.

---

## Table of Contents

1. [HarvestableField](#harvestablefield)
2. [Chicken](#chicken)
3. [StoreCounter](#storecounter)
4. [PlayerController](#playercontroller)

---

## HarvestableField

**Location:** `Assets/Scripts/GameObjects/HarvestableField.cs`

### Purpose
Represents the corn field that players interact with to harvest corn. Features cooldown-based regrowth and visual feedback animations.

### Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `cornPerHarvest` | int | 1 | Base corn yield |
| `harvestCooldown` | float | 2f | Seconds between harvests |
| `bounceAmount` | float | 0.1f | Animation bounce scale |
| `bounceSpeed` | float | 2f | Animation frequency |
| `readyColor` | Color | (1, 0.9, 0.3) | Golden yellow when ready |
| `cooldownColor` | Color | (0.6, 0.7, 0.4) | Muted green during cooldown |

### State Machine

```
┌────────────────────────────────────────────────────────────┐
│              HARVESTABLE FIELD STATE MACHINE               │
├────────────────────────────────────────────────────────────┤
│                                                            │
│   ┌─────────────────┐                                      │
│   │  READY STATE    │ ◄───────────────────────┐           │
│   │  canHarvest=true│                          │           │
│   │  • Golden color │                          │           │
│   │  • Pulsing bounce│                         │           │
│   │  • Swaying corn │                          │           │
│   └────────┬────────┘                          │           │
│            │                                   │           │
│            │ Player interacts                  │           │
│            ▼                                   │           │
│   ┌─────────────────┐                          │           │
│   │ HARVESTING      │                          │           │
│   │ (Animation)     │                          │           │
│   │ • Squash down   │                          │           │
│   │ • Stretch up    │                          │           │
│   │ • Particles burst│                         │           │
│   └────────┬────────┘                          │           │
│            │                                   │           │
│            ▼                                   │           │
│   ┌─────────────────┐    cooldownTimer <= 0   │           │
│   │ COOLDOWN STATE  │ ─────────────────────────┘           │
│   │ canHarvest=false│                                      │
│   │ • Muted color   │                                      │
│   │ • Smaller scale │                                      │
│   │ • Growing back  │                                      │
│   └─────────────────┘                                      │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

### Key Methods

#### `Harvest()`
Main harvest action triggered by player interaction.

```csharp
public void Harvest()
{
    if (!canHarvest) return;
    
    canHarvest = false;
    cooldownTimer = harvestCooldown;
    
    GameManager.Instance.AddCorn(cornPerHarvest);
    StartCoroutine(HarvestAnimation());
    spriteRenderer.color = cooldownColor;
    SpawnHarvestParticles();
    AudioManager.Instance?.PlaySound("harvest");
}
```

#### Animation Sequence

```
HarvestAnimation():
1. Squash: Scale to (1.4x, 0.6y) over 0.1s
2. Stretch: Scale to (0.8x, 1.2y) over 0.1s  
3. Shrink: Scale to 0.8 over 0.15s
4. GrowBack: Gradually return to 1.0 over cooldown duration
```

### Particle Effects

**Harvest Particles:**
```csharp
main.startSize = 0.2f;
main.startLifetime = 0.6f;
main.startColor = golden yellow;
main.startSpeed = 3f;
main.gravityModifier = 0.5f;
emission.burst = 10 particles;
shape = Cone (30° angle);
```

**Ready Sparkle:**
```csharp
main.startSize = 0.15f;
main.startLifetime = 0.4f;
main.startColor = light yellow;
main.gravityModifier = -0.3f; // floats up
emission.burst = 5 particles;
```

### Upgrade Integration

```csharp
public void UpgradeField(int additionalCorn)
{
    cornPerHarvest += additionalCorn;
    StartCoroutine(UpgradeAnimation()); // Scale pop effect
}
```

---

## Chicken

**Location:** `Assets/Scripts/GameObjects/Chicken.cs`

### Purpose
The chicken is fed corn and produces eggs. Features cute animations including blinking, wiggling, pecking, and egg-laying.

### Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `cornRequired` | int | 1 | Corn needed to feed |
| `eggLayDelay` | float | 1.5f | Time to produce egg |
| `blinkInterval` | float | 3f | Seconds between blinks |
| `blinkDuration` | float | 0.1f | How long eyes stay closed |
| `wiggleAmount` | float | 5f | Body wiggle degrees |
| `wiggleSpeed` | float | 3f | Wiggle frequency |
| `bobAmount` | float | 0.05f | Idle bob magnitude |

### Visual References

```
┌────────────────────────────────────────┐
│           CHICKEN ANATOMY              │
├────────────────────────────────────────┤
│                                        │
│            bodySprite                  │
│         ┌─────────────┐               │
│         │   eyeSprite │               │
│         │     ○ ○     │◄── Blinks!   │
│         │    ╰───╯    │               │
│         │   (beak)    │◄── Pecks!    │
│         │   ┌─────┐   │               │
│         │   │body │   │◄── Wiggles!  │
│         └───┴─────┴───┘               │
│               ▼                        │
│         eggSpawnPoint                  │
│                                        │
└────────────────────────────────────────┘
```

### State Machine

```
┌──────────────────────────────────────────────────────────────┐
│                 CHICKEN STATE MACHINE                         │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────────┐                                          │
│  │  IDLE STATE    │ ◄──────────────────────┐                │
│  │  • Blinking    │                         │                │
│  │  • Wiggling    │                         │                │
│  │  • Bobbing     │                         │                │
│  └───────┬────────┘                         │                │
│          │                                  │                │
│          │ Feed() called                    │                │
│          │ (requires corn)                  │                │
│          ▼                                  │                │
│  ┌────────────────┐                         │                │
│  │ EATING STATE   │                         │                │
│  │ isLayingEgg=true                         │                │
│  │ • Peck x3      │                         │                │
│  │ • "eat" sound  │                         │                │
│  └───────┬────────┘                         │                │
│          │                                  │                │
│          ▼                                  │                │
│  ┌────────────────┐                         │                │
│  │ LAYING STATE   │                         │                │
│  │ • Squash wide  │                         │                │
│  │ • Pop! stretch │                         │                │
│  │ • "egg" sound  │                         │                │
│  └───────┬────────┘                         │                │
│          │                                  │                │
│          ▼                                  │                │
│  ┌────────────────┐                         │                │
│  │ EGG SPAWNED    │ ────────────────────────┘                │
│  │ • Bounce anim  │                                          │
│  │ • Particles    │                                          │
│  │ isLayingEgg=false                                         │
│  └────────────────┘                                          │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

### Animation Sequences

#### Blinking
```csharp
private IEnumerator Blink()
{
    eyeSprite.transform.localScale = new Vector3(x, 0.1f, z); // Squish eyes
    yield return new WaitForSeconds(0.1f);
    eyeSprite.transform.localScale = originalEyeScale; // Open eyes
}
```

#### Eating (Pecking)
```csharp
for (int i = 0; i < 3; i++) // 3 pecks
{
    // Rotate head down 15°
    // Return to original
    // Wait 0.1s
}
```

#### Egg Laying
```csharp
// Build up - squash wider
Scale: (1.3x, 0.7y) over 0.3s

// Pop! - stretch tall  
Scale: (0.8x, 1.2y) over 0.1s

// Settle with bounce
Scale: return to (1x, 1y) with sine bounce over 0.2s
```

### Egg Spawning

```csharp
private void SpawnEgg()
{
    Vector3 spawnPos = eggSpawnPoint ?? transform.position - new Vector3(0, 0.5f, 0);
    
    if (eggPrefab != null)
        Instantiate(eggPrefab, spawnPos, Quaternion.identity);
    else
        CreateSimpleEgg(spawnPos); // Programmatic fallback
    
    SpawnEggParticles(spawnPos);
}
```

**Simple Egg Creation (Fallback):**
- Creates GameObject with SpriteRenderer
- Off-white color (1, 0.98, 0.9)
- CircleCollider2D (radius 0.3, trigger)
- Adds CollectibleEgg component

---

## StoreCounter

**Location:** `Assets/Scripts/GameObjects/StoreCounter.cs`

### Purpose
Location where eggs are sold for coins. Features coin burst particle effects and sell animation.

### Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `sellCooldown` | float | 0.5f | Minimum time between sales |
| `bounceAmount` | float | 0.1f | Idle animation scale |
| `activeColor` | Color | (0.9, 0.8, 0.5) | When eggs available |
| `inactiveColor` | Color | (0.7, 0.6, 0.4) | When no eggs |

### State Machine

```
┌────────────────────────────────────────────────────────────┐
│              STORE COUNTER STATE MACHINE                   │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  ┌─────────────────┐                                       │
│  │ ACTIVE STATE    │ ◄─────────────────────────┐          │
│  │ eggs > 0        │                            │          │
│  │ canSell = true  │                            │          │
│  │ • Warm color    │                            │          │
│  │ • Gentle bounce │                            │          │
│  └────────┬────────┘                            │          │
│           │                                     │          │
│           │ SellEgg()                           │          │
│           ▼                                     │          │
│  ┌─────────────────┐                            │          │
│  │ SELLING         │                            │          │
│  │ • Pop scale     │                            │          │
│  │ • Coin burst    │                            │          │
│  │ • "sell" sound  │                            │          │
│  │ • "coin" sound  │                            │          │
│  └────────┬────────┘                            │          │
│           │                                     │          │
│           ▼                                     │          │
│  ┌─────────────────┐    cooldown expires       │          │
│  │ COOLDOWN        │ ──────────────────────────┘          │
│  │ canSell = false │                                       │
│  └─────────────────┘                                       │
│                                                            │
│  ┌─────────────────┐                                       │
│  │ INACTIVE STATE  │                                       │
│  │ eggs == 0       │                                       │
│  │ • Muted color   │                                       │
│  └─────────────────┘                                       │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

### Sell Process

```csharp
public void SellEgg()
{
    if (!canSell || GameManager.Instance.Eggs <= 0) return;
    
    canSell = false;
    
    if (GameManager.Instance.SellEgg())
    {
        StartCoroutine(SaleAnimation());
        SpawnCoinBurst();
        AudioManager.Instance?.PlaySound("sell");
    }
    
    StartCoroutine(SellCooldown());
}
```

### Coin Burst Effect

```csharp
// Creates golden coin particles
main.startSize = 0.2f;
main.startLifetime = 0.8f;
main.startColor = gold (1, 0.85, 0.2);
main.startSpeed = 3f;
main.gravityModifier = 1f; // Falls down
emission.burst = 15 particles;
shape = Cone (45° angle);

// Size fades over lifetime
sizeOverLifetime: 1.0 → 0.0
```

---

## PlayerController

**Location:** `Assets/Scripts/GameObjects/PlayerController.cs`

### Purpose
Controls the player's farmer character - movement and interaction with game objects.

### Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `moveSpeed` | float | 5f | Movement units/second |
| `tweenDuration` | float | 0.5f | Base tween time |
| `bobAmount` | float | 0.1f | Walk/idle bob |
| `bobSpeed` | float | 8f | Bob frequency |

### Input Handling

```csharp
private void HandleInput()
{
    if (Input.GetMouseButtonDown(0))
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // Check for interactable
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider != null)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                MoveToAndInteract(hit.transform.position, interactable);
                return;
            }
        }
        
        // Otherwise just move
        MoveTo(mousePos);
    }
}
```

### Movement System

```
┌────────────────────────────────────────────────────────────┐
│              PLAYER MOVEMENT SYSTEM                        │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  Click/Tap                                                 │
│     │                                                      │
│     ▼                                                      │
│  ┌──────────────┐  Yes  ┌─────────────────┐               │
│  │ Interactable?│──────►│ MoveToAndInteract│              │
│  └──────┬───────┘       │ • Store target   │              │
│         │ No            │ • Move to position│              │
│         ▼               │ • Call Interact() │              │
│  ┌──────────────┐       └─────────────────┘               │
│  │   MoveTo()   │                                          │
│  │ • Flip sprite│                                          │
│  │ • Dust puff  │                                          │
│  │ • Start tween│                                          │
│  └──────────────┘                                          │
│         │                                                  │
│         ▼                                                  │
│  ┌──────────────┐                                          │
│  │ TweenMove()  │                                          │
│  │ • Smooth step│                                          │
│  │ • Bob anim   │                                          │
│  │ • Walk anim  │                                          │
│  └──────────────┘                                          │
│         │                                                  │
│         ▼                                                  │
│  ┌──────────────┐                                          │
│  │ On Arrival   │                                          │
│  │ • Stop walk  │                                          │
│  │ • Interact() │                                          │
│  └──────────────┘                                          │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

### Animation States

#### Idle Animation
```csharp
bobTimer += Time.deltaTime * bobSpeed;
float bob = Mathf.Sin(bobTimer) * bobAmount * 0.2f;
transform.localScale = originalScale + new Vector3(0, bob, 0);
```

#### Walking Animation
```csharp
bobTimer += Time.deltaTime * bobSpeed * 2f; // Faster bob
float bob = Mathf.Abs(Mathf.Sin(bobTimer)) * bobAmount;
transform.localScale = originalScale + new Vector3(0, bob, 0);
```

### Dust Puff Effect

```csharp
private void SpawnDustPuff()
{
    // Programmatic particle system
    main.startSize = 0.2f;
    main.startLifetime = 0.5f;
    main.startColor = tan (0.8, 0.7, 0.6, 0.5);
    main.startSpeed = 0.5f;
    main.gravityModifier = -0.1f; // Floats slightly
    emission.burst = 5 particles;
    shape = Circle (0.2 radius);
}
```

### Squash & Stretch

```csharp
public void PlaySquashStretch()
{
    // Squash: (1.2x, 0.8y) over 0.1s
    // Stretch: (0.9x, 1.1y) over 0.1s
    // Return: (1x, 1y) over 0.1s
}
```

---

## System Integration Diagram

```
┌───────────────────────────────────────────────────────────────────┐
│                    FARM SYSTEMS INTEGRATION                        │
├───────────────────────────────────────────────────────────────────┤
│                                                                    │
│                        ┌──────────────┐                           │
│                        │ GameManager  │                           │
│                        │  (Central)   │                           │
│                        └──────┬───────┘                           │
│                               │                                    │
│         ┌─────────────┬───────┴───────┬─────────────┐             │
│         │             │               │             │             │
│         ▼             ▼               ▼             ▼             │
│  ┌────────────┐ ┌──────────┐ ┌────────────┐ ┌────────────┐       │
│  │ Harvestable│ │ Chicken  │ │   Store    │ │  Player    │       │
│  │   Field    │ │          │ │  Counter   │ │ Controller │       │
│  └─────┬──────┘ └────┬─────┘ └─────┬──────┘ └─────┬──────┘       │
│        │             │             │              │               │
│        │ +Corn       │ -Corn       │ -Egg         │               │
│        │             │ +Egg        │ +Coins       │               │
│        │             │             │              │               │
│        └─────────────┴──────┬──────┴──────────────┘               │
│                             │                                      │
│                             ▼                                      │
│                    ┌────────────────┐                              │
│                    │   HelperAI     │                              │
│                    │ (Automates all)│                              │
│                    └────────────────┘                              │
│                                                                    │
│  Resource Flow:                                                    │
│  [Field] ──Corn──► [Chicken] ──Egg──► [Store] ──Coins──► [Upgrades]│
│                                                                    │
└───────────────────────────────────────────────────────────────────┘
```
