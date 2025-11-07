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
- `TryGetEmptyTile()` - Finds available tiles not occupied
- `AddOccupant()` / `RemoveOccupant()` - Manages tile occupancy

### TileBase
Abstract MonoBehaviour representing a single tile with row/column coordinates.

### ITileOccupant
Interface for any object that can occupy a tile (e.g., game pieces, chips).

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
// Place occupant on tile
}

// Cleanup
board.Dispose();
```
## Notes

- Tiles are positioned at (column, -row, 0) in world space
- Always call `Dispose()` to clean up tile GameObjects
- Occupants must implement `ITileOccupant` interface