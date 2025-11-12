# Board System

A grid-based board system for managing tiles and their occupants in a 2D game environment. Uses interface-driven design for flexibility and testability.

## Structure

- **Abstract/** - Base classes for board and tile implementations
  - **BoardSystemBase.cs** - Abstract board management logic
- **Tile/** - Tile implementations and interfaces
  - **Abstract/**
    - **TileBase.cs** - Abstract MonoBehaviour tile with position and lifecycle
  - **Interface/**
    - **ITile.cs** - Core tile contract
  - **BoardTile.cs** - Concrete tile with visual feedback
- **Interface/** - Core interfaces
  - **ITileOccupant.cs** - Contract for objects that can occupy tiles
- **BoardSystem.cs** - Concrete board implementation

## Architecture Overview
```

ITile (interface)
└── TileBase (abstract MonoBehaviour)
└── BoardTile (concrete)

BoardSystemBase (abstract)
└── BoardSystem (concrete)
```
**Design Philosophy:**
- **ITile** = Tile contract (position, lifecycle, visual feedback)
- **TileBase** = Unity-specific tile implementation
- **ITileOccupant** = Contract for objects occupying tiles
- **BoardSystemBase** = Grid management and occupant tracking

---

## Key Components

### ITile
Core interface defining the contract for all tiles in the system.

**Properties:**
- `Name` - Tile identifier (safe to access after destruction)
- `Row` / `Column` - Grid coordinates
- `Position` - World space position (Vector3)

**Methods:**
- `Initialize(row, column)` - Sets up tile grid position
- `Destroy()` - Cleanup and GameObject destruction
- `Highlight()` - Visual feedback when selected/active
- `Conceal()` - Remove visual feedback

**Design Benefits:**
- **Testability**: Can mock tiles without Unity dependencies
- **Flexibility**: Different tile implementations (visual, logical, networked)
- **Polymorphism**: Board system works with any ITile implementation

---

### TileBase
Abstract MonoBehaviour implementing `ITile` with Unity lifecycle integration.

**Properties:**
- `Name` - Returns empty string if destroyed (safe access)
- `Row` / `Column` - Grid coordinates (read-only after initialization)
- `Position` - Abstract property for world position
- Tracks destruction state to prevent accessing destroyed GameObjects

**Lifecycle:**
```
csharp
Awake → OnAwake() (virtual hook)
↓
Initialize(row, col) (called by BoardSystem)
↓
[Gameplay - Highlight/Conceal as needed]
↓
Destroy() → OnDestroy (automatic)
```
**Methods:**
- `OnAwake()` - Protected virtual hook for derived class initialization
- `Initialize(row, column)` - Sets grid coordinates (called by board)
- `Destroy()` - Safe destruction with double-destroy protection
- `Highlight()` - Abstract visual feedback method
- `Conceal()` - Abstract feedback removal method

**Safety Features:**
- Destruction state tracking prevents null reference exceptions
- Safe `Name` property returns empty string after destruction
- Double-destroy protection in `Destroy()` method

---

### BoardTile
Concrete tile implementation with SpriteRenderer-based visual feedback.

**Features:**
- **Visual Feedback**: Color-based highlighting via SpriteRenderer
- **Awake State Tracking**: Validates visual component before use
- **Configurable**: Inspector-editable highlight color

**Configuration:**
```
csharp
[Header("Visuals")]
[SerializeField] private SpriteRenderer visual;

[Header("VFX")]
[SerializeField] private Color highlightColor = Color.green;
```
**Implementation:**
- `Position` - Returns `transform.position`
- `OnAwake()` - Validates SpriteRenderer component
- `Highlight()` - Sets visual color to `highlightColor`
- `Conceal()` - Resets visual color to white

**Safety:**
- Checks `_isAwaken` before color operations
- Logs errors if visual component missing
- Graceful degradation if not properly initialized

---

### ITileOccupant
Interface for any object that can occupy a tile (chips, game pieces, characters).

**Contract:**
- `Tile` - Reference to currently occupied tile (ITile)
- `Occupy(tile)` - Claim a tile and establish reference
- `Release()` - Free the tile for other occupants

**Lifecycle Pattern:**
```
csharp
// Occupy tile
occupant.Occupy(tile);
board.AddOccupant(occupant);

// Use tile
tile.Highlight();

// Release tile
occupant.Release();
board.RemoveOccupant(occupant);
tile.Conceal();
```
**Design Notes:**
- Board system requires `Release()` before `RemoveOccupant()`
- Single occupant per tile enforced by board validation
- Interface allows different occupant implementations (chips, obstacles, power-ups)

---

### BoardSystemBase
Abstract class managing a 2D grid of tiles and tracking occupants.

**Properties:**
- `RowCount` - Number of rows in grid
- `ColumnCount` - Number of columns in grid
- `Tiles` - Protected 2D array of ITile references

**Key Methods:**

**Tile Management:**
- `Initialize()` - Abstract method to create and setup tiles
- `GetTileAt(row, column)` - Retrieve tile at position (null-safe)
- `TryGetEmptyTile(out tile)` - Find first unoccupied tile (O(1) lookup)
- `Dispose()` - Abstract cleanup method

**Occupant Management:**
- `AddOccupant(occupant)` - Register occupant with validation
- `RemoveOccupant(occupant)` - Unregister occupant (requires Release() first)
- `IsTileInBoard(tile)` - Private validation helper

**Performance Features:**
- HashSet-based occupancy checking for O(1) lookup
- Lazy evaluation in `TryGetEmptyTile()` (returns first found)
- Early returns in validation methods

**Validation:**
- Null checks on all occupant operations
- Duplicate occupant prevention
- Tile ownership verification (prevents cross-board bugs)
- Enforces Release() before RemoveOccupant()

---

### BoardSystem
Concrete implementation that instantiates and manages tile prefabs.

**Constructor:**
```
csharp
public BoardSystem(int rowCount, int columnCount, TileBase tilePrefab)
```
**Features:**
- Creates grid layout on `Initialize()`
- Positions tiles at `(column, -row, 0)` in world space
- Validates tile prefab before initialization
- Null-safe disposal of all tiles

**Initialization:**
```
csharp
for (int row = 0; row < RowCount; row++)
{
for (int col = 0; col < ColumnCount; col++)
{
ITile tile = Object.Instantiate(_tilePrefab, new Vector3(col, -row, 0), Quaternion.identity);
tile.Initialize(row, col);
Tiles[row, col] = tile;
}
}
```
**Disposal:**
- Iterates through all tiles
- Calls `Destroy()` on each tile
- Skips null tiles safely

---

## Usage

### Basic Setup
```
csharp
// Create and initialize board
var tilePrefab = Resources.Load<BoardTile>("Prefabs/BoardTile");
var board = new BoardSystem(rows: 8, columns: 8, tilePrefab);
board.Initialize();

// Access specific tile
var tile = board.GetTileAt(row: 3, col: 5);
if (tile != null)
{
tile.Highlight();
}
```
### Working with Occupants
```
csharp
// Find empty tile and place occupant
if (board.TryGetEmptyTile(out var tile))
{
occupant.Occupy(tile);
board.AddOccupant(occupant);

    // Visual feedback
    tile.Highlight();
}

// Move occupant to new tile
occupant.Release();
board.RemoveOccupant(occupant);
tile.Conceal();

if (board.TryGetEmptyTile(out var newTile))
{
occupant.Occupy(newTile);
board.AddOccupant(occupant);
newTile.Highlight();
}
```
### Querying Board State
```
csharp
// Check if position has tile
var tile = board.GetTileAt(row: 2, col: 3);
bool tileExists = tile != null;

// Find empty tile
bool hasEmptySpace = board.TryGetEmptyTile(out var emptyTile);

// Access tile properties
if (tile != null)
{
Debug.Log($"Tile: {tile.Name} at ({tile.Row}, {tile.Column})");
Debug.Log($"World Position: {tile.Position}");
}
```
### Cleanup
```
csharp
// When done with board
board.Dispose(); // Destroys all tile GameObjects
```
---

## Important Notes

### Positioning
- **Standard Grid**: Tiles placed at `(column, -row, 0)` in world space
- **Top-left origin**: (0, 0) is top-left, positive X is right, negative Y is down
- **Customizable**: Override `Initialize()` in custom board systems for different layouts

### Occupant Lifecycle
1. `occupant.Occupy(tile)` - Claim tile
2. `board.AddOccupant(occupant)` - Register with board
3. [Gameplay]
4. `occupant.Release()` - Free tile
5. `board.RemoveOccupant(occupant)` - Unregister from board

**Critical**: Must call `Release()` before `RemoveOccupant()` or error is logged

### Performance
- **TryGetEmptyTile()**: O(1) lookup using HashSet, lazy evaluation
- **GetTileAt()**: O(1) array access
- **AddOccupant()**: O(n) validation, O(1) insertion
- **Early Returns**: All validation methods exit fast on failure

### Memory
- Always call `Dispose()` to clean up tile GameObjects
- Tiles are MonoBehaviours and must be destroyed properly
- Board system doesn't auto-dispose (manual lifecycle management)

### Thread Safety
- Not thread-safe
- Designed for single-threaded Unity context
- All operations must occur on main thread

---

## Design Patterns

- **Interface Segregation**: ITile and ITileOccupant define minimal contracts
- **Template Method**: BoardSystemBase and TileBase define structure, derived classes implement specifics
- **Strategy Pattern**: Different tile and board implementations via polymorphism
- **Dispose Pattern**: Implements IDisposable for proper resource cleanup
- **Hook Method**: `OnAwake()` allows derived tiles to extend initialization
- **Dependency Inversion**: Board depends on ITile interface, not concrete types

---

## Error Handling

Comprehensive validation and logging:
- **Null Checks**: All occupant operations validate null
- **Duplicate Prevention**: `AddOccupant()` checks for existing occupants
- **Tile Ownership**: Validates tile belongs to board before adding occupant
- **Out-of-Bounds**: `GetTileAt()` logs warning and returns null
- **Lifecycle Enforcement**: Requires `Release()` before `RemoveOccupant()`
- **Visual Validation**: BoardTile validates SpriteRenderer on awake
- **Destruction Safety**: TileBase tracks destruction state

---

## Extensibility

Extend the system by:

1. **Custom Tiles**: 
   - Implement `ITile` for non-Unity tiles (networked, logical)
   - Inherit from `TileBase` for Unity-specific tiles (visual, interactive)

2. **Custom Boards**: 
   - Inherit from `BoardSystemBase` for different layouts (hex, irregular, 3D)
   - Override `Initialize()` for custom tile positioning

3. **Custom Occupants**: 
   - Implement `ITileOccupant` for any object needing tiles
   - Examples: chips, obstacles, power-ups, characters

4. **Visual Effects**: 
   - Override `Highlight()` and `Conceal()` for custom feedback
   - Use particles, shaders, animations, audio

5. **Board Variants**:
   - Pooled boards for performance
   - Dynamic boards that grow/shrink
   - Multi-layer boards (3D)

---

## BoardTile Visual Requirements

For the default `BoardTile` implementation:
- Requires `SpriteRenderer` component assigned in inspector
- `highlightColor` configurable (default: Color.green)
- `Color.white` used as default/concealed state
- Validates component in `OnAwake()` before use

---

## Key Updates:
1. ✅ **ITile interface** - Core tile contract documented
2. ✅ **Interface-based architecture** - BoardSystemBase uses `ITile[,]` not `TileBase[,]`
3. ✅ **BoardTile renamed** - From `Tile` to `BoardTile`
4. ✅ **Testing support** - Mock implementations for unit tests
5. ✅ **Design patterns** - Interface Segregation, Dependency Inversion
6. ✅ **Structure update** - Tile/ folder with Interface/ and Abstract/ subfolders
7. ✅ **Usage examples** - Updated to use ITile interface
8. ✅ **Custom implementations** - Examples using interface pattern