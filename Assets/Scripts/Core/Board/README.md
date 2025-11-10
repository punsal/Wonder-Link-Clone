# Board System

A grid-based board system for managing tiles and their occupants in a 2D game environment.

## Structure

- **Abstract/** - Base classes for board and tile implementations
  - **BoardSystemBase.cs** - Abstract board management logic
  - **TileBase.cs** - Abstract tile with position and lifecycle
- **Interface/** - Core interfaces
  - **ITileOccupant.cs** - Contract for objects that can occupy tiles
- **BoardSystem.cs** - Concrete board implementation
- **Tile.cs** - Concrete tile implementation with visual feedback

## Key Components

### BoardSystemBase
Abstract class managing a 2D grid of tiles and tracking occupants.

**Key Methods:**
- `TryGetEmptyTile(out tile)` - Finds first available unoccupied tile (top-left priority, O(1) lookup)
- `AddOccupant(occupant)` - Registers an occupant with validation
- `RemoveOccupant(occupant)` - Unregisters occupant (must call `Release()` first)

**Features:**
- HashSet-based occupancy checking for performance
- Validates tile ownership to prevent cross-board issues
- Defensive programming with comprehensive error logging

### TileBase
Abstract MonoBehaviour representing a single tile in the grid.

**Properties:**
- `Row` / `Column` - Grid coordinates (read-only after initialization)
- `Position` - World space position
- Tracks destruction state to prevent accessing destroyed GameObjects

**Methods:**
- `Initialize(row, column)` - Sets up tile position
- `OnAwake()` - Abstract initialization hook for derived classes
- `Highlight()` - Abstract method for visual feedback when selected/active
- `Conceal()` - Abstract method to remove visual feedback
- `Dispose()` - Safe cleanup with destruction check

### ITileOccupant
Interface for any object that can occupy a tile (e.g., game pieces, chips, characters).

**Contract:**
- `Tile` - Reference to currently occupied tile
- `Occupy(tile)` - Claim a tile and move to its position
- `Release()` - Free the tile for other occupants

### BoardSystem
Concrete implementation that instantiates and manages tile prefabs.
- Creates grid layout on initialization
- Positions tiles at (column, -row, 0)
- Validates tile prefab before initialization
- Handles null-safe disposal

### Tile
Concrete tile implementation with visual feedback using SpriteRenderer.

**Features:**
- Configurable highlight color (default: green)
- SpriteRenderer-based visual feedback
- Safe initialization with awake state tracking
- White color for concealed state

**Configuration:**
- `visual` - SpriteRenderer for tile appearance
- `highlightColor` - Color applied when highlighted

## Usage
```
csharp
// Create and initialize board
var board = new BoardSystem(rows, columns, tilePrefab);
board.Initialize();

// Find empty tile and place occupant
if (board.TryGetEmptyTile(out var tile))
{
occupant.Occupy(tile);
board.AddOccupant(occupant);
}

// Highlight tile
tile.Highlight(); // Visual feedback

// Remove occupant
occupant.Release();
board.RemoveOccupant(occupant);
tile.Conceal(); // Remove visual feedback

// Cleanup
board.Dispose();
```
## Implementing Custom Tiles
```
csharp
public class CustomTile : TileBase
{
protected override void OnAwake()
{
// Initialize custom tile components
}

    public override void Highlight()
    {
        // Custom highlight effect (particles, animation, etc.)
    }
    
    public override void Conceal()
    {
        // Remove custom effects
    }
}
```
## Important Notes

- **Tile Positioning**: Tiles are placed at (column, -row, 0) in world space
- **Occupant Lifecycle**: Must call `Release()` before `RemoveOccupant()`
- **Validation**: `AddOccupant()` validates tile ownership to prevent bugs
- **Performance**: Uses HashSet for O(1) occupancy checks instead of O(n)
- **Memory**: Always call `Dispose()` to clean up tile GameObjects
- **Thread Safety**: Not thread-safe, designed for single-threaded Unity context
- **Initialization Hook**: Override `OnAwake()` in derived tiles for custom setup

## Tile Visual Requirements

For the default `Tile` implementation:
- Requires a `SpriteRenderer` component assigned to `visual` field
- Highlight color configurable in inspector (default: green)
- Uses `Color.white` as default/concealed state

## Design Patterns

- **Template Method**: `BoardSystemBase` and `TileBase` define structure, derived classes implement details
- **Dispose Pattern**: Implements `IDisposable` for proper resource cleanup
- **Strategy Pattern**: `ITileOccupant` allows different occupant behaviors
- **Hook Method**: `OnAwake()` allows derived tiles to extend initialization

## Performance Considerations

- HashSet for O(1) occupancy lookups
- Early returns in validation methods
- Lazy evaluation in `TryGetEmptyTile()` (returns first found)
- Destruction state tracking prevents null reference exceptions

## Extensibility

To extend the system:
1. **Custom Boards**: Inherit from `BoardSystemBase` for different grid layouts or instantiation logic
2. **Custom Tiles**: Inherit from `TileBase` for unique tile types (animated, interactive, destructible, etc.)
3. **Custom Occupants**: Implement `ITileOccupant` for any object that needs to occupy tiles
4. **Visual Effects**: Override `Highlight()` and `Conceal()` for custom feedback (particles, shaders, animations)

## Error Handling

The system includes comprehensive error logging:
- Null occupant checks
- Duplicate occupant prevention
- Tile ownership validation
- Out-of-bounds detection
- Visual component validation
- Awake state verification in Tile