# Shuffle System

Randomizes chip positions on the board when no valid moves exist. Maintains board integrity while providing visual feedback through animated transitions.

## Structure

- **Abstract/**
  - **ShuffleSystemBase.cs** - Abstract shuffle framework
- **ShuffleSystem.cs** - Concrete random shuffle implementation

## Key Components

### ShuffleSystemBase
Abstract base for shuffle systems.

**Dependencies:**
- `BoardSystemBase` - Board dimensions and tile access
- `ChipManagerBase` - Active chip queries
- `ICoroutineRunner` - Coroutine execution (typically GameManager)

**Key Methods:**
- `StartShuffle()` - Initiates shuffle sequence
- `Shuffle()` - Abstract shuffle implementation

**Events:**
- `OnShuffleCompleted` - Fired when shuffle finishes

**Properties:**
- `ShuffleCount` - Tracks total shuffles performed (starts at 10 for debugging)

**Flow:**
```

StartShuffle() → WaitForShuffle() → Shuffle() → OnShuffleCompleted
```
---

### ShuffleSystem
Concrete implementation using Fisher-Yates shuffle algorithm.

**Algorithm:**
```

1. Collect all active chips with valid tiles
2. Store original tile positions
3. Release all chips from tiles (board sync)
4. Shuffle tile list randomly (OrderBy Random.value)
5. Reassign chips to shuffled tiles
6. Animate chips to new positions (0.3s each)
7. Wait for animations to complete (0.35s)
```
**Safety Features:**
- **Chip count validation**: Warns if count != board size
- **Duplicate detection**: Logs if multiple chips at same position
- **Null filtering**: Skips null or tile-less chips
- **Board sync**: Proper Release/Occupy + RemoveOccupant/AddOccupant

**Timing:**
- Per-chip animation: 0.3s
- Final wait: 0.35s total
- Chips animate simultaneously (yield between assignments for visual effect)

---

## Usage

### Setup
```
csharp
var shuffleSystem = new ShuffleSystem(boardSystem, chipManager, coroutineRunner);
shuffleSystem.OnShuffleCompleted += HandleShuffleComplete;
```
### Trigger Shuffle
```
csharp
// After detecting no valid moves
if (!matchDetection.HasPossibleMoves())
{
Debug.Log("No moves - shuffling board");
shuffleSystem.StartShuffle();
}

void HandleShuffleComplete()
{
Debug.Log($"Shuffle #{shuffleSystem.ShuffleCount} complete");
// Re-enable input or continue game flow
}
```
### Integration with Refill
```csharp
private IEnumerator WaitForRefillComplete(List<ChipBase> chips)
{
    yield return Refill(chips);
    
    if (!MatchDetectionSystem.HasPossibleMoves())
    {
        Debug.Log("No moves detected. Shuffling...");
        ShuffleSystem.StartShuffle();
        yield return new WaitForSeconds(0.5f); // Wait for shuffle
    }
    
    onRefillCompleted?.Invoke();
}
```
### Cleanup
```csharp
shuffleSystem.Dispose();
```

---

## Shuffle Example

### Before:
```
Board (4x4):
R B G Y
G Y R B
B R Y G
Y G B R
```


### After (Random):
```
Y R B G
B G R Y
R Y G B
G B Y R
```


**Process:**
1. Collect 16 chips and their tiles
2. Release all chips from tiles
3. Shuffle tile list: `[Tile(0,0), Tile(0,1), ...] → [Tile(2,3), Tile(1,0), ...]`
4. Reassign: Chip[0] → ShuffledTile[0], Chip[1] → ShuffledTile[1], ...
5. Animate all chips to new positions

---

## Important Notes

### Validation
- Warns if chip count doesn't match board size
- Detects and logs duplicate chip positions
- Filters out null chips and chips without tiles

### Board Synchronization
```csharp
// Release phase
chip.Release();                    // Clear chip's tile reference
BoardSystem.RemoveOccupant(chip);  // Remove from board tracking

// Reassign phase
chip.Occupy(newTile);              // Set new tile reference
BoardSystem.AddOccupant(chip);     // Re-register with board
```


### Animation
- Chips animate simultaneously (not sequential)
- `yield return null` between assignments for visual pacing
- Final 0.35s wait ensures all animations complete
- Uses `chip.MoveTo()` which delegates to animator component

### Performance
- O(n log n) due to LINQ `OrderBy`
- Full board operation (not performance-critical)
- Only called when no moves exist (rare)

---

## Design Patterns

- **Template Method**: Base defines structure, derived implements algorithm
- **Strategy Pattern**: Different shuffle algorithms via inheritance
- **Observer Pattern**: Event notification for completion
- **Dependency Injection**: Board, chip manager, coroutine runner via constructor

---

## Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Chip count mismatch | Null chips in list | Filter nulls: `Where(c => c != null && c.Tile != null)` |
| Duplicate positions | Board sync broken | Ensure Release/Occupy + board operations |
| Animation glitches | Coroutine timing | Wait for animations: `yield return new WaitForSeconds(0.35f)` |
| Board state inconsistent | Missing Release() | Always Release before RemoveOccupant |

---

## Integration Points

**Used by:**
- **BoardRefillSystem** - Triggers when no moves exist after refill
- **GameManager** - Coordinates input disabling during shuffle

**Depends on:**
- **BoardSystem** - Tile access and occupant management
- **ChipManager** - Active chip queries and board sync
- **MatchDetectionSystem** - Validates shuffle was needed

---

## Future Enhancements

- **Guarantee valid moves**: Shuffle until valid match exists
- **Shuffle animation types**: Fade, spiral, cascade effects
- **Shuffle history**: Track tile permutations for undo
- **Partial shuffle**: Shuffle only problematic regions
- **Shuffle count limit**: Prevent infinite shuffle loops
