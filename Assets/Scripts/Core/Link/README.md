# Link System

A drag-based linking system for match-3+ gameplay with interface-driven design for flexible type matching and adjacency validation.

## Structure

- **Abstract/** - Base classes for link system implementations
  - **LinkSystemBase.cs** - Abstract drag interaction framework
  - **LinkableBase.cs** - Abstract MonoBehaviour base for linkable objects
- **Interface/** - Core interfaces
  - **ILinkable.cs** - Linkable object contract
- **Type/** - Type definitions
  - **LinkType.cs** - Enum defining linkable types (Red, Green, Blue, Yellow)
- **LinkSystem.cs** - Concrete link system implementation

## Architecture
```

ILinkable (interface)
└── LinkableBase (abstract MonoBehaviour)
└── ChipBase (game-specific, in Gameplay/)
└── BasicChip (concrete)

LinkSystemBase (abstract)
└── LinkSystem (concrete)
```
**Design Philosophy:**
- **ILinkable** = Contract for linkable behavior (type matching, adjacency, linking)
- **LinkableBase** = MonoBehaviour implementation with tile integration
- **LinkSystemBase** = Drag interaction framework
- **LinkSystem** = Match-3+ validation and event dispatch

---

## Key Components

### ILinkable
Interface defining the contract for objects participating in the linking system.

**Properties:**
- `LinkType` - Type category for matching
- `Tile` - Currently occupied tile (from `ITileOccupant`)

**Methods:**
- `Link()` - Mark as linked and apply visual feedback
- `Unlink()` - Remove link state and feedback
- `IsTypeMatch(type)` - Validate if type is compatible for linking
- `IsAdjacent(other)` - Validate spatial connection rules
- `Occupy(tile)` / `Release()` - Inherited from `ITileOccupant`

---

### LinkableBase
Abstract MonoBehaviour implementing `ILinkable` with board integration.

**Properties:**
- `LinkType` - Serialized type category
- `Tile` - Reference to occupied tile (ITile)

**Implemented Methods:**
- `Occupy(tile)` - Claims tile and calls virtual `OnOccupied()`
- `Release()` - Frees tile and calls virtual `PreRelease()`
- `Link()` - Sets linked state, highlights tile, calls `OnLinked()`
- `Unlink()` - Clears linked state, conceals tile, calls `OnUnlinked()`

**Abstract Methods:**
- `OnLinked()` - Custom visual/audio feedback when linked
- `OnUnlinked()` - Custom visual/audio feedback when unlinked
- `IsTypeMatch(type)` - Determines type compatibility rules
- `IsAdjacent(other)` - Determines spatial connection rules

**Virtual Hooks:**
- `OnOccupied(tile)` - Called after tile assignment
- `PreRelease(tile)` - Called before tile release

**Features:**
- Idempotent linking (prevents duplicate link/unlink)
- Automatic tile highlighting integration
- Debug logging with coordinates
- Null-safe tile operations

---

### LinkSystemBase
Abstract base class for drag-based linking systems.

**Properties:**
- `Camera` - Camera for raycasting (via `ICameraProvider`)
- `LayerMask` - Layer filter for linkable detection
- `IsDragging` - Current drag state
- `OnLinkCompleted` - Event fired with valid match (3+)

**Abstract Methods:**
- `StartDrag()` - Handle initial input
- `UpdateDrag()` - Process ongoing drag
- `EndDrag()` - Finalize and validate chain
- `Dispose()` - Cleanup event subscriptions

**Constructor:**
```
csharp
protected LinkSystemBase(ICameraProvider cameraProvider, LayerMask layerMask)
```
---

### LinkSystem
Concrete implementation handling match-3+ drag-to-link gameplay.

**Features:**
- **Interface-Based**: Works with `ILinkable`, not concrete types
- **Type Matching**: Delegates to `IsTypeMatch()` - implementers define rules
- **Adjacency**: Delegates to `IsAdjacent()` - implementers define spatial logic
- **Backtracking**: Drag back to previous linkable to undo selection
- **Match-3+ Detection**: Validates ≥3 items before firing completion event
- **Physics2D Raycasting**: Efficient detection with layer masking

**Workflow:**
1. `StartDrag()` → Stores initial `LinkType`, begins tracking
2. `UpdateDrag()` → Validates type/adjacency, handles backtracking
3. `EndDrag()` → Validates count (≥3), fires `OnLinkCompleted`, clears state

**State Tracking:**
- `_linkables` - List of currently linked items
- `_lastLinkable` - Most recent linkable for adjacency checks
- `_linkType` - Type locked from first linkable
- All state cleared on `EndDrag()`

---

## Usage

### Setup
```
csharp
// Create link system
var linkSystem = new LinkSystem(cameraProvider, linkableLayerMask);
linkSystem.OnLinkCompleted += HandleLinkComplete;

// Input handling (e.g., in Update)
if (Input.GetMouseButtonDown(0))
linkSystem.StartDrag();
else if (Input.GetMouseButton(0) && linkSystem.IsDragging)
linkSystem.UpdateDrag();
else if (Input.GetMouseButtonUp(0))
linkSystem.EndDrag();

// Handle completion
void HandleLinkComplete(List<ILinkable> linkedItems)
{
Debug.Log($"Matched {linkedItems.Count} items!");
// Process matches
}

// Cleanup
linkSystem.Dispose();
```
### Implementing ILinkable
```
csharp
public class Chip : LinkableBase
{
protected override void OnLinked()
{
transform.localScale = Vector3.one * 1.2f;
}

    protected override void OnUnlinked()
    {
        transform.localScale = Vector3.one;
    }
    
    public override bool IsTypeMatch(LinkType type)
    {
        return LinkType == type; // Strict matching
        // Or: return true; // Wildcard
    }
    
    public override bool IsAdjacent(ILinkable other)
    {
        if (other?.Tile == null || Tile == null) return false;
        
        // 4-directional (orthogonal only)
        int rowDiff = Mathf.Abs(Tile.Row - other.Tile.Row);
        int colDiff = Mathf.Abs(Tile.Column - other.Tile.Column);
        return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
        
        // Or 8-directional: return rowDiff <= 1 && colDiff <= 1 && (rowDiff + colDiff) > 0;
    }
}
```
---

## Board Integration

`ILinkable` extends `ITileOccupant` for seamless board integration:
```
csharp
// Place linkable on board
if (board.TryGetEmptyTile(out var tile))
{
linkable.Occupy(tile);
board.AddOccupant(linkable);
}

// Remove from board
linkable.Release();
board.RemoveOccupant(linkable);
```
---

## Gameplay Rules

- **Minimum Match**: 3+ linked items required
- **Type Locking**: First selected linkable determines chain type
- **Backtracking**: Drag to previous item to deselect current
- **No Duplicates**: Cannot link same item twice per chain
- **Type Matching**: Defined by `IsTypeMatch()` implementation
- **Adjacency**: Defined by `IsAdjacent()` implementation

---

## Requirements

- **Collider2D**: Required on linkable objects for raycasting
- **LayerMask**: Assign linkables to specific layer for optimization
- **Camera Provider**: Uses `ICameraProvider` for decoupled camera access
- **Tile System**: Linkables must occupy tiles for adjacency checks
- **Event Cleanup**: Call `Dispose()` to prevent memory leaks

---

## Design Patterns

- **Interface Segregation**: `ILinkable` defines minimal contract
- **Template Method**: Base classes define structure, derived classes implement specifics
- **Strategy Pattern**: Abstract methods allow different validation behaviors
- **Observer Pattern**: Event-driven completion via `OnLinkCompleted`
- **Dependency Injection**: Camera via `ICameraProvider`

---

## Performance

- **LayerMask Filtering**: Limits raycasts to relevant objects
- **List-Based Tracking**: O(n) backtrack checks
- **Per-Frame Raycasting**: Only during active drag
- **Delegate Validation**: Keep `IsTypeMatch()` and `IsAdjacent()` efficient

---

## Important Notes

- `Link()`/`Unlink()` are idempotent (safe to call multiple times)
- Tile highlighting/concealing handled automatically
- Type and adjacency rules fully customizable per linkable
- Debug logging included (can be disabled in production)
- Interface-based design allows testing without Unity dependencies

## Key Changes:
1. ✅ **Shortened significantly** - Removed redundant examples and verbose explanations
2. ✅ **ILinkable interface documented** - Core contract highlighted
3. ✅ **Architecture diagram** - Shows interface-based design
4. ✅ **Virtual hooks documented** - `OnOccupied()` and `PreRelease()`
5. ✅ **Removed testing section** - Moved to testing docs if needed
6. ✅ **Consolidated extensibility** - Brief examples only
7. ✅ **Clean structure** - Matches board system README format