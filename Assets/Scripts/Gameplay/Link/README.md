# Link System

A drag-based linking system for match-3+ gameplay with flexible type matching and adjacency validation. Integrates with board and tile systems for spatial awareness.

## Structure

- **Abstract/** - Base classes for link system implementations
  - **LinkSystemBase.cs** - Abstract drag interaction framework
- **Interface/** - Core interfaces and base classes
  - **LinkableBase.cs** - Abstract base for linkable MonoBehaviours
- **Type/** - Type definitions
  - **LinkType.cs** - Enum defining linkable types (Red, Green, Blue, Yellow)
- **LinkSystem.cs** - Concrete link system implementation

## Key Components

### LinkSystemBase
Abstract base class providing foundation for drag-based linking systems.

**Properties:**
- `Camera` - Camera reference for raycasting (via `ICameraProvider`)
- `LayerMask` - Layer filter for linkable detection
- `IsDragging` - Current drag state flag
- `OnInputCompleted` - Event fired when linking completes with valid match

**Abstract Methods:**
- `StartDrag()` - Handle initial touch/click
- `UpdateDrag()` - Process ongoing drag input
- `EndDrag()` - Finalize and validate link chain
- `Dispose()` - Cleanup event subscriptions

### LinkableBase
Abstract MonoBehaviour base class for objects that can be linked together. Implements `ITileOccupant` for board integration.

**Properties:**
- `LinkType` - The type category of this linkable (serialized)
- `Tile` - Reference to occupied tile (from `ITileOccupant`)

**Implemented Methods:**
- `Occupy(tile)` - Claims a tile and moves to its position
- `Release()` - Frees the tile
- `Link()` - Marks as linked, highlights tile, calls `OnLinked()`
- `Unlink()` - Marks as unlinked, conceals tile, calls `OnUnlinked()`

**Abstract Methods:**
- `Move(position)` - Defines how linkable moves to tile position
- `OnLinked()` - Custom visual/audio feedback when linked
- `OnUnlinked()` - Custom visual/audio feedback when unlinked
- `IsTypeMatch(type)` - Determines if this linkable can connect with given type
- `IsAdjacent(other)` - Determines spatial connection rules with another linkable

**Features:**
- Prevents duplicate linking (idempotent `Link()`/`Unlink()`)
- Automatic tile highlighting integration
- Debug logging with tile coordinates

### LinkType
Enum defining available link categories.
- `Red`, `Green`, `Blue`, `Yellow`

### LinkSystem
Concrete implementation handling drag-to-link gameplay.

**Features:**
- **Flexible Type Matching**: Delegates to `LinkableBase.IsTypeMatch()` - implementers define rules
- **Flexible Adjacency**: Delegates to `LinkableBase.IsAdjacent()` - implementers define spatial rules
- **Backtrack Support**: Drag back to previous linkable to deselect current
- **Match-3+ Detection**: Triggers completion event with 3 or more linked items
- **Raycasting**: Uses Physics2D with layer masking for efficient detection
- **Drag State Tracking**: Exposes `IsDragging` property for external systems

**Workflow:**
1. Player starts drag on a linkable → `StartDrag()` stores initial type
2. Player drags to linkables passing type/adjacency checks → `UpdateDrag()`
3. Player releases → `EndDrag()` validates count (≥3) and fires `OnInputCompleted`

## Usage

### Setup LinkSystem
```
csharp
// Create link system
var linkSystem = new LinkSystem(cameraProvider, linkableLayerMask);
linkSystem.OnInputCompleted += HandleLinkComplete;

// Input handling (typically in Update)
if (Input.GetMouseButtonDown(0))
linkSystem.StartDrag();
else if (Input.GetMouseButton(0) && linkSystem.IsDragging)
linkSystem.UpdateDrag();
else if (Input.GetMouseButtonUp(0))
linkSystem.EndDrag();

// Handle completion
void HandleLinkComplete(List<LinkableBase> linkedItems)
{
Debug.Log($"Matched {linkedItems.Count} items!");
// Process matches (destroy, score, spawn effects, etc.)
}

// Cleanup
linkSystem.Dispose();
```
### Implementing LinkableBase
```
csharp
public class Chip : LinkableBase
{
[SerializeField] private float moveSpeed = 10f;

    protected override void Move(Vector3 position)
    {
        // Instant move
        transform.position = position;
        
        // Or animated:
        // StartCoroutine(AnimateMove(position));
    }
    
    protected override void OnLinked()
    {
        // Visual feedback (scale, particles, sound)
        transform.localScale = Vector3.one * 1.2f;
        // Play link sound effect
    }
    
    protected override void OnUnlinked()
    {
        // Remove feedback
        transform.localScale = Vector3.one;
    }
    
    public override bool IsTypeMatch(LinkType type)
    {
        // Strict matching
        return LinkType == type;
        
        // Or advanced rules:
        // - Wildcards: return LinkType == type || LinkType == LinkType.Wild;
        // - Color combos: return IsColorCompatible(LinkType, type);
    }
    
    public override bool IsAdjacent(LinkableBase other)
    {
        if (other == null || Tile == null || other.Tile == null)
            return false;
        
        // 4-directional (orthogonal only)
        int rowDiff = Mathf.Abs(Tile.Row - other.Tile.Row);
        int colDiff = Mathf.Abs(Tile.Column - other.Tile.Column);
        return (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
        
        // Or 8-directional (with diagonals):
        // return rowDiff <= 1 && colDiff <= 1 && (rowDiff + colDiff) > 0;
        
        // Or custom rules based on tile type, power-ups, etc.
    }
}
```
## Board Integration

`LinkableBase` implements `ITileOccupant`, enabling seamless board integration:
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
## Gameplay Rules

- **Minimum Match**: 3 or more linked items required to trigger completion
- **Type Matching**: Determined by `LinkableBase.IsTypeMatch()` implementation
- **Adjacency**: Determined by `LinkableBase.IsAdjacent()` implementation
- **Backtracking**: Drag back to previous item to deselect current
- **No Re-linking**: Cannot link the same item twice in one chain
- **First Type Locks**: Chain type is set by the first linkable selected

## Important Notes

- **Requires Collider2D**: Linkable objects must have colliders for raycasting
- **Layer Mask**: Assign linkables to a specific layer for optimized raycasting
- **Camera Provider**: Uses `ICameraProvider` for decoupled camera access
- **Event Cleanup**: Call `Dispose()` to prevent memory leaks
- **Type Safety**: `LinkType` enum can be extended for additional categories
- **Tile Integration**: Linkables must be placed on tiles via `Occupy()` for adjacency checks
- **Idempotent Operations**: `Link()`/`Unlink()` can be called multiple times safely

## Design Patterns

- **Template Method**: `LinkSystemBase` and `LinkableBase` define structure, derived classes implement specifics
- **Strategy Pattern**: Abstract methods in `LinkableBase` allow different behaviors per linkable type
- **Observer Pattern**: Event-driven completion notification via `OnInputCompleted`
- **Dependency Injection**: Camera provided via `ICameraProvider`
- **Composite Pattern**: Linkables can occupy tiles (board integration)

## Performance Considerations

- **LayerMask Filtering**: Limits raycasting to relevant objects only
- **List-Based Tracking**: O(n) for backtrack checks; consider HashSet for very long chains
- **Per-Frame Raycasting**: Occurs during drag; optimized with layer filtering
- **Delegate Validation**: `IsTypeMatch()` and `IsAdjacent()` called frequently - keep implementations efficient
- **Event Subscriptions**: Single event prevents GC pressure from multiple listeners

## State Management

The system tracks:
- Current drag state (`IsDragging`)
- Linked items list (`_linkables`)
- Last linked item reference (`_lastLinkableBase`)
- Initial link type (`_linkType`)

All state is cleared on `EndDrag()`.

## Extensibility

### Extend LinkableBase
```
csharp
public class PowerUpChip : LinkableBase
{
public override bool IsTypeMatch(LinkType type)
{
// Match any type (wildcard)
return true;
}

    protected override void OnLinked()
    {
        // Special power-up visual
        PlayPowerUpEffect();
    }
}
```
### Custom Link System
```
csharp
public class ComboLinkSystem : LinkSystemBase
{
private int _comboMultiplier = 1;

    public override void EndDrag()
    {
        IsDragging = false;
        
        // Custom combo logic
        if (_linkables.Count >= 5)
            _comboMultiplier++;
            
        if (_linkables.Count >= 3)
            onLinkCompleted?.Invoke(_linkables);
            
        ClearLinkables();
    }
}
```
### Custom LinkTypes
```
csharp
public enum LinkType
{
Red,
Green,
Blue,
Yellow,
Wild,      // Matches any type
Bomb,      // Special match type
Rainbow    // Advanced combo type
}
```
## Debugging

The system includes comprehensive debug logging:
- Drag start/fail messages
- Link/unlink with tile coordinates
- Type mismatch warnings
- Adjacency validation failures
- Duplicate link attempts
- Raycast miss notifications

Enable via Unity Console to track linking behavior during development.

## Testing Support

Abstract base classes enable easy testing:
```
csharp
public class MockLinkable : LinkableBase
{
public bool IsLinked { get; private set; }

    protected override void Move(Vector3 position) { }
    protected override void OnLinked() { IsLinked = true; }
    protected override void OnUnlinked() { IsLinked = false; }
    
    public override bool IsTypeMatch(LinkType type) => LinkType == type;
    public override bool IsAdjacent(LinkableBase other) => true; // Always adjacent for testing
}
```