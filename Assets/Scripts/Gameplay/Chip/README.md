# Chip System

Manages chip lifecycle, spawning, destruction, and spatial queries for board-based match-3 gameplay. Uses component-based architecture for flexible visual effects and score integration.

## Structure

- **Abstract/** - Base classes for chip management
  - **ChipBase.cs** - Abstract chip entity with component support
  - **ChipManagerBase.cs** - Abstract chip lifecycle manager
- **Components/** - Animation and behavior components
  - **Abstract/**
    - **ChipAnimatorComponentBase.cs** - Abstract animator component
    - **ChipScoreComponentBase.cs** - Abstract score component
  - **ChipAnimatorComponent.cs** - Concrete tween-based animator
  - **ChipScoreComponent.cs** - Concrete score provider
- **BasicChip.cs** - Concrete chip with standard match-3 behavior
- **ChipManager.cs** - Concrete chip manager implementation

## Architecture
```

ILinkable (Core.Link)
└── LinkableBase (Core.Link)
└── ChipBase (abstract - game entity)
├── BasicChip (concrete - standard chip)
├── [Component] ChipAnimatorComponentBase
└── [Component] ChipScoreComponentBase (IScoreAmountProvider)
```
**Design Philosophy:**
- **ChipBase** = Game entity with component management (movement, destruction, score)
- **LinkableBase** = Board interaction (linking, adjacency, tile occupancy)
- **ChipAnimatorComponent** = Swappable visual effects via composition
- **ChipScoreComponent** = Modular score values implementing `IScoreAmountProvider`

---

## Key Components

### ChipBase
Abstract base class for chip entities with component-based architecture.

**Components:**
```
csharp
[SerializeField] private ChipAnimatorComponentBase animator;
[SerializeField] private ChipScoreComponentBase score;
```
**Properties:**
- `Score` - Returns `IScoreAmountProvider` for score integration

**Key Methods:**
- `Destroy()` - Triggers destruction animation via animator
- `MoveTo(position, duration)` - Delegates movement to animator
- `OnLinked()` - Delegates to `animator.PlayLinkEffect()`
- `OnUnlinked()` - Delegates to `animator.PlayUnlinkEffect()`
- `OnAwake()` - Virtual hook for derived class initialization

**Design Benefits:**
- Composition over inheritance
- Swappable components (animator, score)
- Score system integration via `IScoreAmountProvider`
- Testable (can mock components)

---

### ChipScoreComponentBase
Abstract MonoBehaviour component implementing `IScoreAmountProvider`.

**Features:**
```
csharp
[SerializeField] private int scoreAmount;
public int Instance => scoreAmount; // IScoreAmountProvider
```
**Purpose:**
- Provides score value for each chip
- Configurable in Unity Inspector
- Enables per-chip score customization
- Integrates with Score System

**Benefits:**
- **Designer control**: Set chip values without code
- **Flexible scoring**: Different chips can have different values
- **Power-up support**: Special chips can provide bonus scores
- **Component-based**: Swap implementations for complex scoring

---

### ChipScoreComponent
Concrete implementation of score provider.
```
csharp
public override IScoreAmountProvider ScoreAmountProvider => this;
```
**Usage:**
- Attach to chip prefabs
- Set `scoreAmount` in Inspector
- Accessed via `chip.Score.Instance`

**Example:**
```
csharp
// In chip prefab: scoreAmount = 10
var chip = chipManager.FindChipAt(0, 0);
int points = chip.Score.Instance; // Returns 10
```
---

### ChipAnimatorComponentBase
Abstract MonoBehaviour component for chip visual effects.

**Key Methods:**
- `PlayLinkEffect()` - Visual feedback when linked
- `PlayUnlinkEffect()` - Visual feedback when unlinked
- `AnimateDestruction()` - Scale-down animation
- `AnimateMovement(target, duration)` - Position transition

**Design Benefits:**
- Swappable implementations (tween, Animator, physics, DOTween)
- Reusable across different chip types
- Animation state management (coroutine tracking)

---

### ChipAnimatorComponent
Concrete coroutine-based tween animator.

**Features:**
- Link: Scales up 1.2x
- Unlink: Resets to original scale
- Destruction: Scale-down to zero (0.2s)
- Movement: Lerp-based position (0.2s)

**Configuration:**
```
csharp
[SerializeField] private float linkScaleMultiplier = 1.2f;
[SerializeField] private float destroyDuration = 0.2f;
[SerializeField] private float moveDuration = 0.2f;
```
**Safety:**
- Stops previous animation before starting new
- Null-safe operations
- Frame-perfect timing with `Time.deltaTime`

---

### ChipManagerBase
Abstract base class for chip lifecycle management.

**Key Methods:**
- `FillBoard()` - Abstract, populate entire board
- `SpawnRandomChipAt(tile)` - Spawn from prefab pool
- `DestroyChips(chips)` - Batch destruction with automatic cleanup
- `FindChipAt(row, col)` - Position-based query with duplicate detection
- `CleanupDestroyedChips()` - Abstract, remove null references

**Properties:**
- `ActiveChips` - Read-only list of active chips
- `BoardSystem` - Board system reference

**Performance:**
- Manual loop in `FindChipAt()` (avoids LINQ overhead)
- Automatic null cleanup after batch operations
- Duplicate detection for debugging

---

### BasicChip
Concrete chip with standard match-3 behavior.

**Features:**
- Strict type matching (no wildcards)
- 4-directional adjacency (orthogonal only)
- Component-driven visuals and scoring

**Adjacency Rules:**
```

Valid (O = chip, X = adjacent):
X
X O X
X

Invalid (diagonals):
X   X
O
X   X
```
---

### ChipManager
Concrete manager tracking active chips.

**Spawning:**
- `FillBoard()` - Fills all empty tiles
- `SpawnChipAt(tile, prefab)` - Spawns chip above board, animates falling
- Prevents duplicate spawns
- Names chips `Chip_{row}_{column}` for debugging

**Destruction:**
- `DestroyChip(chip)` - Single chip removal
- `DestroyChips(chips)` - Batch destruction + null cleanup
- `DestroyAllChips()` - Clear all (reverse iteration)
- `CleanupDestroyedChips()` - `RemoveAll()` for null references

**Queries:**
- `FindChipAt(row, col)` - O(n) lookup with duplicate detection

**Safety:**
- Duplicate tile occupancy check
- Null cleanup via `RemoveAll()`
- Debug logging for spawn/destroy
- Multi-chip warning at same position

---

## Usage

### Setup
```
csharp
var chipPrefabs = new List<ChipBase> { redChip, blueChip, greenChip };
var chipManager = new ChipManager(boardSystem, chipPrefabs);
chipManager.FillBoard();
```
### Score Integration
```
csharp
void HandleLinkComplete(List<ILinkable> linkables)
{
var chips = linkables.Cast<ChipBase>().ToList();

    // Add score for each chip
    foreach (var chip in chips)
    {
        _scoreSystem.AddScore(chip.Score); // IScoreAmountProvider
    }
    
    chipManager.DestroyChips(chips);
    boardRefillSystem.StartRefill(chips);
}
```
### Custom Chip Scores
```
csharp
// In Unity Inspector:
// BasicChip → ChipScoreComponent → scoreAmount = 10
// BonusChip → BonusScoreComponent → scoreAmount = 50

// Access at runtime:
var chip = chipManager.FindChipAt(0, 0);
Debug.Log($"Chip worth {chip.Score.Instance} points");
```
---

## Important Notes

### Score System Integration
- Chips implement `IScoreAmountProvider` via component
- Score values configurable per-chip in Inspector
- Enables flexible scoring strategies (base, bonus, multipliers)
- Decouples chip logic from score calculation

### Component Architecture
- **ChipAnimatorComponent**: Visual effects (swappable)
- **ChipScoreComponent**: Score values (designer-friendly)
- Components attached to chip prefabs
- Null-safe component access in `ChipBase`

### Performance
- `FindChipAt()` uses manual loop (LINQ avoided)
- Batch operations cache list to prevent enumeration issues
- Null cleanup via `RemoveAll()` after batch destruction
- Component-based architecture separates concerns

### Unity Lifecycle
- `Object.Destroy()` marks for destruction at end-of-frame
- Unity nulls persist until explicit cleanup
- Coroutines stop automatically on GameObject destruction
- Chips spawn above board and animate falling

---

## Design Patterns

- **Template Method**: Base classes define structure, derived implement specifics
- **Strategy Pattern**: Different chip types via `ChipBase` inheritance
- **Component Pattern**: Swappable animator and score implementations
- **Provider Pattern**: `IScoreAmountProvider` for score integration
- **Repository Pattern**: ChipManager as repository for chip entities
- **Dispose Pattern**: `IDisposable` for cleanup

---

## Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Chip count grows | Not removed from list | `CleanupDestroyedChips()` auto-called |
| "Chip already has tile" | Duplicate spawn | `SpawnChipAt()` checks existing chip |
| Multiple chips at position | Sync issue | `FindChipAt()` detects & logs duplicates |
| Score not working | Missing component | Assign ChipScoreComponent to prefab |
| Null score reference | Component not set | Check `OnAwake()` validates components |

---

## Key Changes from Previous Version

1. ✅ **ChipScoreComponent added** - Score system integration
2. ✅ **IScoreAmountProvider implemented** - Chips provide score values
3. ✅ **Component-based scoring** - Designer-friendly score configuration
4. ✅ **Score property exposed** - `chip.Score.Instance` for score access
5. ✅ **Flexible scoring** - Custom score components via inheritance