# Board Refill System

Manages board refill sequence after chip matches: destruction → gravity → spawning. Coordinates animations and board state synchronization.

## Structure

- **Abstract/**
  - **BoardRefillSystemBase.cs** - Abstract refill framework
- **BoardRefillSystem.cs** - Concrete 3-phase refill implementation

## Architecture
```

BoardRefillSystemBase (abstract)
└── BoardRefillSystem (concrete)
└── Uses: MatchDetectionSystem (for future shuffle logic)
```
**Design Philosophy:**
- **Coroutine-based** = Sequential animations with proper timing
- **Event-driven** = Notifies completion for input re-enabling
- **Modular phases** = Destroy, gravity, spawn as separate operations

---

## Key Components

### BoardRefillSystemBase
Abstract base class for board refill logic.

**Dependencies:**
- `BoardSystemBase` - Board grid access
- `ChipManagerBase` - Chip lifecycle operations
- `ICoroutineRunner` - Coroutine execution (typically GameManager)
- `MatchDetectionSystem` - Abstract property for match validation

**Key Methods:**
- `StartRefill(chips)` - Initiates refill sequence
- `Refill(chips)` - Abstract 3-phase implementation
- `Dispose()` - Cleanup event subscriptions

**Events:**
- `OnRefillCompleted` - Fired when entire sequence finishes

**Flow:**
```

StartRefill(chips) → WaitForRefillComplete() → Refill() → OnRefillCompleted
```
---

### BoardRefillSystem
Concrete implementation with 3-phase refill sequence.

**Phases:**

**1. Destroy (0.3s)**
- Triggers chip destruction animations
- Waits for visual effects
- Calls `ChipManager.DestroyChips()`

**2. Gravity (0.2s per move)**
- Processes columns left-to-right
- Moves chips bottom-to-top
- Finds lowest empty position below each chip
- Animates chip falling with `chip.MoveTo()`
- Updates board occupancy

**3. Spawn (0.05s per chip)**
- Fills empty tiles bottom-to-top, left-to-right
- Spawns chips via `ChipManager.SpawnRandomChipAt()`
- Small delay for cascade visual effect

**Helper Methods:**
- `FindLowestEmptyRow(row, col)` - Finds target position for falling chip
- `MoveChipToTile(chip, row, col)` - Handles chip movement with board sync

**Timing:**
```
csharp
Destroy:   0.3s wait
Gravity:   0.2s per chip moved
Spawn:     0.05s per chip + 0.1s final wait
```
---

## Usage

### Setup
```
csharp
var refillSystem = new BoardRefillSystem(boardSystem, chipManager, coroutineRunner);
refillSystem.OnRefillCompleted += HandleRefillComplete;
```
### Trigger Refill
```
csharp
void HandleLinkComplete(List<ILinkable> linkables)
{
inputHandler.Disable(); // Disable input during refill

    var chips = linkables.Cast<ChipBase>().ToList();
    refillSystem.StartRefill(chips);
}

void HandleRefillComplete()
{
Debug.Log("Refill complete");
inputHandler.Enable(); // Re-enable input
}
```
### Cleanup
```
csharp
refillSystem.Dispose();
```
---

## Gravity Logic

### Column Processing
- **Order**: Left-to-right (col: 0 → N)
- **Row Processing**: Bottom-to-top (row: N → 0)
- **Movement**: Each chip falls to lowest empty position below it

### Example (4x4 board, 'X' = chip, '.' = empty):
```

Before:        After Gravity:
. . X .        . . . .
X . . .   →    . . . .
. X . X        X . . .
X . X .        X X X X
```
### Algorithm:
```

For each column:
For each row (bottom to top):
If chip exists:
Find lowest empty row below
If found:
Move chip to lowest empty row
Animate movement
```
---

## Important Notes

### Timing
- Animations run sequentially, not in parallel
- Each phase waits for previous to complete
- Total refill time: ~0.5s - 2s depending on board state

### Board Synchronization
- Chips properly released/occupied during moves
- Board occupant list stays in sync
- Tile references updated correctly

### Coroutine Runner
- Requires `ICoroutineRunner` (typically GameManager)
- All coroutines run on runner's MonoBehaviour
- Nested `StartCoroutine()` calls for phase sequencing

### Event Management
- `OnRefillCompleted` fired after all animations
- Call `Dispose()` to prevent memory leaks
- Null-safe event invocation

---

## Design Patterns

- **Template Method**: Base defines structure, derived implements phases
- **Observer Pattern**: Event notification for completion
- **Dependency Injection**: Board, chip manager, coroutine runner via constructor
- **Strategy Pattern**: Different refill algorithms possible via inheritance

---

## Performance

- **Column-by-column**: Processes one column at a time for visual clarity
- **Early exits**: Skips processing if no chip at position
- **Efficient queries**: Uses `ChipManager.FindChipAt()` (O(n) per call)
- **Coroutine overhead**: Minimal, animations are smooth

---

## Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Chips overlap | Gravity doesn't wait for animation | Wait for `MoveChipToTile()` coroutine |
| Input during refill | No input disabling | Disable input on `StartRefill()`, enable on completion event |
| Chips stuck mid-air | Board sync broken | Ensure `Release()`/`Occupy()` + board operations in `MoveChipToTile()` |
| Spawn animation missing | Instant spawn | `ChipManager.SpawnRandomChipAt()` handles spawn animation |

---

## Future Enhancements

- **Cascade Matching**: Detect and chain matches after refill
- **Shuffle Logic**: If no moves possible, shuffle board
- **Combo System**: Track consecutive refills for multipliers
- **Particle Effects**: Add juice to destruction/gravity phases
- **Configurable Timings**: Expose animation durations via constructor/config