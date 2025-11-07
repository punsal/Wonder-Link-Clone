# Board System

A grid-based board system for managing tiles and their occupants.

## Structure

- **Abstract/** - Base classes for board and tile implementations
- **Interface/** - Core interfaces
- **GameBoard.cs** - Concrete board implementation
- **Tile.cs** - Concrete tile implementation

## Key Components

### BoardBase
Abstract class managing a 2D grid of tiles and their occupants.
- `TryGetEmptyTile()` - Finds first available unoccupied tile (top-left priority)
- `AddOccupant()` - Adds an occupant and validates tile ownership
- `RemoveOccupant()` - Removes occupant (must call `Release()` first)

### TileBase
Abstract MonoBehaviour representing a single tile with row/column coordinates.
- Tracks destruction state to prevent accessing destroyed objects
- Provides `Position` property for world coordinates

### ITileOccupant
Interface for any object that can occupy a tile (e.g., game pieces, chips).
- `Tile` - Reference to currently occupied tile
- `Occupy()` - Claim a tile
- `Release()` - Free the tile

### GameBoard
Concrete implementation that instantiates and manages tile prefabs in a grid layout.

## Usage
```
csharp
// Create board
var board = new GameBoard(rows, columns, tilePrefab);
board.Initialize();

// Get empty tile
if (board.TryGetEmptyTile(out var tile))
{
// Use occupant pattern
occupant.Occupy(tile);
board.AddOccupant(occupant);
}

// Remove occupant
occupant.Release();
board.RemoveOccupant(occupant);

// Cleanup
board.Dispose();
```
## Important Notes

- Tiles are positioned at (column, -row, 0) in world space
- Always call `Dispose()` to clean up tile GameObjects
- Occupants must call `Release()` before being removed from board
- `AddOccupant()` validates that tiles belong to the board
- `TryGetEmptyTile()` uses HashSet for optimized O(1) lookups

## Implementation Details

- Uses HashSet for efficient occupancy checking
- Null-safe disposal of tiles
- Defensive validation to prevent common errors