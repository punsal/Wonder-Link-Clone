# Chip System

Manages chip lifecycle, spawning, destruction, and queries for board-based gameplay. Provides abstraction for different chip types with component-based animation and centralized chip management.

## Structure

- **Abstract/** - Base classes for chip management
  - **ChipBase.cs** - Abstract chip entity with component support
  - **ChipManagerBase.cs** - Abstract chip lifecycle manager
- **Components/** - Animation and behavior components
  - **Abstract/**
    - **ChipAnimatorComponentBase.cs** - Abstract animator component
  - **ChipAnimatorComponent.cs** - Concrete tween-based animator
- **BasicChip.cs** - Concrete chip implementation with standard behavior
- **ChipManager.cs** - Concrete chip manager implementation

## Architecture Overview
```

LinkableBase (Core.Link)
└── ChipBase (abstract - game entity)
└── BasicChip (concrete - standard chip)
└── ChipAnimatorComponent (component - visual effects)
```
**Design Philosophy:**
- **ChipBase** = Game entity abstraction (movement, destruction, components)
- **LinkableBase** = Board interaction contract (linking, adjacency, tile occupancy)
- **ChipAnimatorComponent** = Swappable visual effects system

---

## Key Components

### ChipBase
Abstract base class representing a game chip entity with component support.

**Responsibilities:**
- Component management (animator, movement, effects)
- Visual feedback for linking/unlinking
- Destruction animation coordination
- Movement delegation to animator

**Key Methods:**
- `Destroy()` - Triggers destruction animation
- `MoveTo(position, duration)` - Animates chip movement
- `OnLinked()` - Delegates to animator for link effect
- `OnUnlinked()` - Delegates to animator for unlink effect
- `OnAwake()` - Virtual hook for initialization

**Component Integration:**
```
csharp
[Header("Components")]
[SerializeField] private ChipAnimatorComponentBase animator;
```
**Design Notes:**
- Uses **composition over inheritance** for animations
- Animator components are swappable at runtime
- Protected `OnAwake()` allows derived classes to extend initialization
- Delegates visual effects to specialized components

---

### ChipAnimatorComponentBase
Abstract MonoBehaviour component for chip visual effects.

**Responsibilities:**
- Link/unlink visual feedback
- Destruction animations
- Movement animations
- Animation state management

**Key Methods:**
- `PlayLinkEffect()` - Visual feedback when chip is linked
- `PlayUnlinkEffect()` - Visual feedback when chip is unlinked
- `AnimateDestruction()` - Destruction animation (scale down)
- `AnimateMovement(target, duration)` - Smooth position transition

**Design Benefits:**
- **Swappable implementations**: Easily switch between tween, Animator, or physics-based
- **Testable**: Can mock animations in unit tests
- **Reusable**: Same animator can be used on different chip types

---

### ChipAnimatorComponent
Concrete implementation using coroutine-based tweening.

**Features:**
- **Link Effect**: Scales chip up by 1.2x
- **Unlink Effect**: Resets scale to original
- **Destruction**: Smooth scale-down to zero over 0.2s
- **Movement**: Lerp-based position animation

**Configuration:**
```
csharp
[Header("Settings")]
[SerializeField] private float linkScaleMultiplier = 1.2f;
[SerializeField] private float destroyDuration = 0.2f;
[SerializeField] private float moveDuration = 0.2f;
```
**Animation Safety:**
- Stops previous animation before starting new one
- Handles destroyed GameObject gracefully
- Frame-perfect timing with `Time.deltaTime`

---

### ChipManagerBase
Abstract base class for managing chip lifecycle and operations.

**Responsibilities:**
- Chip spawning with random or specific prefabs
- Chip destruction (individual or batch)
- Chip queries by board position
- Board integration via `BoardSystemBase`
- Null reference cleanup

**Key Methods:**
- `FillBoard()` - Abstract method to populate entire board
- `SpawnRandomChipAt(tile)` - Spawns random chip from prefab pool
- `SpawnChipAt(tile, prefab)` - Abstract method for specific chip spawning
- `DestroyChips(chips)` - Batch destruction with automatic cleanup
- `FindChipAt(row, col)` - Query chip by grid coordinates (with duplicate detection)
- `CleanupDestroyedChips()` - Abstract method for removing null references

**Properties:**
- `ActiveChips` - Read-only list of currently active chips
- `BoardSystem` - Reference to board system for tile/occupant management

**Design Notes:**
- Performance-optimized: Avoids LINQ in `FindChipAt()` for frequent queries
- Implements `IDisposable` for proper cleanup
- Automatic null cleanup after batch destruction
- Duplicate detection warns of chip count mismatches

---

### BasicChip
Concrete implementation of `ChipBase` with standard match-3 behavior.

**Features:**
- **Type Matching**: Strict equality check (no wildcards)
- **4-Directional Adjacency**: Horizontal and vertical only (no diagonals)
- **Component-Driven**: All visuals handled by `ChipAnimatorComponent`

**Adjacency Rules:**
```

Valid adjacent positions (O = current chip, X = adjacent):
X
X O X
X

Invalid (diagonals):
X   X
O
X   X
```
**Methods:**
- `IsTypeMatch(type)` - Strict equality check
- `IsAdjacent(other)` - 4-directional validation with null safety

---

### ChipManager
Concrete implementation managing active chip collection.

**Features:**
- Tracks all active chips in `_activeChips` list
- Integrates with board system for tile occupancy
- Handles GameObject instantiation and destruction
- Automatic null cleanup after batch operations
- Duplicate spawn detection

**Key Operations:**

**Spawning:**
- `FillBoard()` - Fills all empty tiles with random chips
- `SpawnChipAt()` - Creates chip above board, animates falling down
  - Spawn position: `(tile.x, tile.y + boardHeight, 0)`
  - Prevents duplicate spawns on same tile
  - Names chips as `Chip_{row}_{column}` for debugging

**Destruction:**
- `DestroyChip()` - Removes single chip safely
- `DestroyChips()` - Batch destruction + automatic null cleanup
- `DestroyAllChips()` - Clears all chips (reverse iteration for safety)
- `CleanupDestroyedChips()` - Removes null references from list

**Queries:**
- `FindChipAt(row, col)` - Fast position-based lookup with duplicate detection

**Safety Features:**
- Duplicate tile occupancy detection
- Null reference cleanup via `RemoveAll()`
- Debug logging for spawn/destroy operations
- Warning if multiple chips found at same position

---

## Usage

### Setup
```
csharp
// Create chip manager
var chipPrefabs = new List<ChipBase> { redChipPrefab, blueChipPrefab, greenChipPrefab };
var chipManager = new ChipManager(boardSystem, chipPrefabs);

// Fill board initially
chipManager.FillBoard();
```
### Spawning Chips
```
csharp
// Spawn random chip at specific tile
if (boardSystem.TryGetEmptyTile(out var tile))
{
chipManager.SpawnRandomChipAt(tile);
}

// Fill all empty tiles (handles animation automatically)
chipManager.FillBoard();
```
### Destroying Chips
```
csharp
// Destroy matched chips (with automatic cleanup)
var matchedChips = new List<ChipBase> { chip1, chip2, chip3 };
chipManager.DestroyChips(matchedChips); // Cleanup happens automatically

// Destroy single chip
var chipToRemove = chipManager.FindChipAt(2, 3);
if (chipToRemove != null)
{
chipManager.DestroyChip(chipToRemove);
}

// Clean up all chips
chipManager.Dispose(); // Or DestroyAllChips()
```
### Querying Chips
```
csharp
// Find chip at grid position
var chip = chipManager.FindChipAt(row: 3, col: 5);

if (chip != null)
{
Debug.Log($"Found {chip.LinkType} chip at ({chip.Tile.Row}, {chip.Tile.Column})");
}

// Check if position is empty
bool isEmpty = chipManager.FindChipAt(2, 3) == null;
```
### Integration with Refill System
```
csharp
// After link completion
private void HandleLinkComplete(List<ILinkable> linkables)
{
// Cast to ChipBase for processing
var chips = linkables.Cast<ChipBase>().ToList();

    // Destroy matched chips
    chipManager.DestroyChips(chips); // Automatic cleanup included
    
    // BoardRefillSystem handles gravity and spawning
    boardRefillSystem.StartRefill(chips);
}
```
---

## Important Notes

### Performance
- **Query Optimization**: `FindChipAt()` uses manual loop instead of LINQ (performance-critical)
- **Batch Operations**: `DestroyChips()` caches list to avoid enumeration issues
- **Automatic Cleanup**: Null references removed via `RemoveAll()` after batch destruction
- **Component-Based**: Animation logic separated from game logic for better performance

### Unity Lifecycle
- **Destruction Timing**: `Object.Destroy()` marks for destruction at end of frame
- **Null References**: Unity nulls persist in lists until explicit cleanup
- **Animation Coroutines**: Stopped automatically when chip is destroyed
- **Spawn Animation**: Chips spawn above board and animate falling down

### Safety
- **Duplicate Detection**: Warns if multiple chips found at same position
- **Tile Validation**: Checks tile occupancy before spawning
- **Null Safety**: All public methods check for null chips
- **Cleanup Guarantee**: `CleanupDestroyedChips()` called after batch operations

### Board Integration
- Always uses `Occupy()`/`Release()` and `AddOccupant()`/`RemoveOccupant()` pattern
- Maintains sync between ChipManager list and BoardSystem occupants
- Validates tile references before operations

---

## Design Patterns

- **Template Method**: `ChipManagerBase` defines structure, derived classes implement specifics
- **Strategy Pattern**: Different chip types via `ChipBase` inheritance
- **Component Pattern**: `ChipAnimatorComponentBase` for swappable animation systems
- **Repository Pattern**: ChipManager acts as repository for chip entities
- **Object Pool**: Can be implemented via custom manager (see example above)
- **Dispose Pattern**: Implements `IDisposable` for cleanup

---

## Debugging Support

### Built-in Logging
- Spawn operations log position and total count
- Destroy operations log before/after counts
- Duplicate detection logs position conflicts
- Null cleanup logs removed count

## Common Issues & Solutions

### Issue: Chip count grows over time
**Cause**: Chips not removed from list after destruction  
**Solution**: `CleanupDestroyedChips()` now called automatically in `DestroyChips()`

### Issue: "Chip already has a tile" error
**Cause**: Duplicate spawn on occupied tile  
**Solution**: `SpawnChipAt()` now checks for existing chip before spawning

### Issue: Multiple chips at same position
**Cause**: Synchronization issue between list and board  
**Solution**: `FindChipAt()` detects and logs duplicate positions

### Issue: Animation doesn't stop when chip destroyed
**Cause**: Coroutine continues after GameObject destroyed  
**Solution**: `ChipAnimatorComponent` checks for null before animation operations

---

## Extensibility

Extend the system by:
1. **Custom Chips**: Inherit from `ChipBase` for unique behaviors (power-ups, obstacles, wildcards)
2. **Custom Animators**: Inherit from `ChipAnimatorComponentBase` for different animation systems (DOTween, Animator, Physics)
3. **Custom Managers**: Inherit from `ChipManagerBase` for pooling, spawning strategies, or special rules
4. **Adjacency Rules**: Override `IsAdjacent()` in chip classes for custom spatial logic (8-directional, range-based)
5. **Type Matching**: Override `IsTypeMatch()` for wildcards, combos, or special interactions

## Key Updates:
1. ✅ **Component-based architecture** - ChipAnimatorComponent separation
2. ✅ **Spawn animation** - Chips fall from above
3. ✅ **Automatic null cleanup** - CleanupDestroyedChips() pattern
4. ✅ **Duplicate detection** - FindChipAt() warns of conflicts
5. ✅ **Debugging support** - Logging and context menu examples
6. ✅ **Common issues section** - Solutions to bugs we fixed
7. ✅ **Updated examples** - Reflect ChipBase instead of LinkableBase