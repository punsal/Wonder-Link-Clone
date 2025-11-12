# Chip System

Manages chip lifecycle, spawning, destruction, and spatial queries for board-based match-3 gameplay. Uses component-based architecture for flexible visual effects and centralized chip management.

## Structure

- **Abstract/** - Base classes for chip management
  - **ChipBase.cs** - Abstract chip entity with component support
  - **ChipManagerBase.cs** - Abstract chip lifecycle manager
- **Components/** - Animation and behavior components
  - **Abstract/**
    - **ChipAnimatorComponentBase.cs** - Abstract animator component
  - **ChipAnimatorComponent.cs** - Concrete tween-based animator
- **BasicChip.cs** - Concrete chip with standard match-3 behavior
- **ChipManager.cs** - Concrete chip manager implementation

## Architecture
```

ILinkable (Core.Link)
└── LinkableBase (Core.Link)
└── ChipBase (abstract - game entity)
├── BasicChip (concrete - standard chip)
└── [Component] ChipAnimatorComponentBase
```
**Design Philosophy:**
- **ChipBase** = Game entity with component management (movement, destruction, effects)
- **LinkableBase** = Board interaction (linking, adjacency, tile occupancy)
- **ChipAnimatorComponent** = Swappable visual effects via composition

---

## Key Components

### ChipBase
Abstract base class for chip entities with component-based animation.

**Responsibilities:**
- Component management (animator reference)
- Visual feedback delegation (link/unlink)
- Destruction animation coordination
- Movement delegation to animator

**Key Methods:**
- `Destroy()` - Triggers destruction animation via animator
- `MoveTo(position, duration)` - Delegates movement to animator
- `OnLinked()` - Delegates to `animator.PlayLinkEffect()`
- `OnUnlinked()` - Delegates to `animator.PlayUnlinkEffect()`
- `OnAwake()` - Virtual hook for derived class initialization

**Component Integration:**
```
csharp
[SerializeField] private ChipAnimatorComponentBase animator;
```
**Design Benefits:**
- Composition over inheritance for animations
- Swappable animator components
- Testable (can mock animators)

---

### ChipAnimatorComponentBase
Abstract MonoBehaviour component for chip visual effects.

**Responsibilities:**
- Link/unlink visual feedback
- Destruction animations
- Movement animations
- Animation state management (current coroutine tracking)

**Key Methods:**
- `PlayLinkEffect()` - Visual feedback when linked
- `PlayUnlinkEffect()` - Visual feedback when unlinked
- `AnimateDestruction()` - Scale-down animation
- `AnimateMovement(target, duration)` - Position transition

**Design Benefits:**
- Swappable implementations (tween, Animator, physics, DOTween)
- Reusable across different chip types
- Unit test friendly

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

**Responsibilities:**
- Chip spawning (random or specific)
- Chip destruction (individual or batch)
- Spatial queries by grid position
- Board system integration
- Null reference cleanup

**Key Methods:**
- `FillBoard()` - Abstract, populate entire board
- `SpawnRandomChipAt(tile)` - Spawn from prefab pool
- `SpawnChipAt(tile, prefab)` - Abstract, specific chip spawning
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
- Component-driven visuals

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
**Methods:**
- `IsTypeMatch(type)` - Returns `LinkType == type`
- `IsAdjacent(other)` - 4-directional validation with null safety

---

### ChipManager
Concrete manager tracking active chips.

**Features:**
- Active chip list management
- Board system integration
- GameObject lifecycle (Instantiate/Destroy)
- Automatic null cleanup
- Duplicate spawn prevention

**Spawning:**
- `FillBoard()` - Fills all empty tiles
- `SpawnChipAt(tile, prefab)` - Spawns chip above board (row + boardHeight), animates falling
- Prevents duplicate spawns on occupied tiles
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
### Spawning
```
csharp
// Random chip at tile
if (boardSystem.TryGetEmptyTile(out var tile))
chipManager.SpawnRandomChipAt(tile);

// Fill all empty tiles
chipManager.FillBoard();
```
### Destruction
```
csharp
// Batch destroy (with auto-cleanup)
chipManager.DestroyChips(matchedChips);

// Single chip
var chip = chipManager.FindChipAt(2, 3);
if (chip != null)
chipManager.DestroyChip(chip);

// Clear all
chipManager.Dispose();
```
### Queries
```
csharp
var chip = chipManager.FindChipAt(row: 3, col: 5);
bool isEmpty = chipManager.FindChipAt(2, 3) == null;
```
### Integration with Refill
```
csharp
void HandleLinkComplete(List<ILinkable> linkables)
{
var chips = linkables.Cast<ChipBase>().ToList();
chipManager.DestroyChips(chips);
boardRefillSystem.StartRefill(chips);
}
```
---

## Important Notes

### Performance
- `FindChipAt()` uses manual loop (LINQ avoided)
- Batch operations cache list to prevent enumeration issues
- Null cleanup via `RemoveAll()` after batch destruction
- Component-based animation separates concerns

### Unity Lifecycle
- `Object.Destroy()` marks for destruction at end-of-frame
- Unity nulls persist until explicit cleanup
- Coroutines stop automatically on GameObject destruction
- Chips spawn above board and animate falling

### Safety
- Duplicate position detection
- Tile occupancy validation before spawn
- Null-safe public methods
- Guaranteed cleanup after batch operations

### Board Integration
- Uses `Occupy()`/`Release()` + `AddOccupant()`/`RemoveOccupant()` pattern
- Maintains sync between ChipManager list and BoardSystem
- Validates tile references before operations

---

## Design Patterns

- **Template Method**: Base classes define structure, derived implement specifics
- **Strategy Pattern**: Different chip types via `ChipBase` inheritance
- **Component Pattern**: Swappable `ChipAnimatorComponentBase` implementations
- **Repository Pattern**: ChipManager as repository for chip entities
- **Dispose Pattern**: `IDisposable` for cleanup

---

## Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Chip count grows | Not removed from list | `CleanupDestroyedChips()` auto-called |
| "Chip already has tile" | Duplicate spawn | `SpawnChipAt()` checks existing chip |
| Multiple chips at position | Sync issue | `FindChipAt()` detects & logs duplicates |
| Animation doesn't stop | Coroutine continues | Animator checks null before operations |

---

## Debugging

Built-in logging:
- Spawn: Position & total count
- Destroy: Before/after counts
- Duplicate: Position conflicts
- Cleanup: Null count removed

Disable logs by removing `Debug.Log()` calls in production builds.

---

## Key Changes:
1. ✅ **Shortened by ~40%** - Removed redundant sections
2. ✅ **Table format** for common issues - Easier to scan
3. ✅ **Removed "Key Updates"** section - Not relevant for maintainers
4. ✅ **Consolidated extensibility** - Brief, focused examples
5. ✅ **Cleaner structure** - Matches board/link README format
6. ✅ **Quick reference** - Important notes as bullet points
7. ✅ **Removed verbose explanations** - Kept only essential info