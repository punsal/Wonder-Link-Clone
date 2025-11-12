# Match Detection System

Detects potential 3+ matches on the board using flood-fill algorithm. Used to validate if any valid moves exist before shuffling.

## Structure

- **Abstract/**
  - **MatchDetectionSystemBase.cs** - Abstract match detection framework
- **MatchDetectionSystem.cs** - Concrete flood-fill implementation

## Architecture
```

MatchDetectionSystemBase (abstract)
└── MatchDetectionSystem (concrete - flood-fill)
```
**Design Philosophy:**
- **Non-destructive** = Read-only board analysis
- **Flood-fill based** = Finds all connected chips of same type
- **Future-proof** = Designed for shuffle integration

---

## Key Components

### MatchDetectionSystemBase
Abstract base class for match detection systems.

**Dependencies:**
- `BoardSystemBase` - Board dimensions and structure
- `ChipManagerBase` - Chip queries

**Key Methods:**
- `HasPossibleMoves()` - Scans entire board for valid 3+ matches
- `CanFormMatch(chip)` - Abstract method to check if chip can form match

**Algorithm:**
```

For each chip on board:
If chip can form 3+ match:
Return true
Return false (no moves possible)
```
---

### MatchDetectionSystem
Concrete implementation using recursive flood-fill.

**Features:**
- **Flood-fill algorithm** - Finds all connected chips of same type
- **4-directional** - Checks orthogonal adjacency (up, down, left, right)
- **HashSet tracking** - Prevents revisiting same chip (O(1) lookup)
- **Type-based matching** - Uses `chip.IsTypeMatch()` for validation

**Match Criteria:**
- Minimum 3 connected chips
- Same `LinkType`
- Orthogonally adjacent (horizontal/vertical only)

**Algorithm:**
```

CanFormMatch(startChip):
visited = new HashSet
connected = new List

FindConnectedChips(startChip):
If visited or null: return
If type mismatch: return

    Add to visited and connected
    
    For each adjacent position (up/down/left/right):
      Recursively check adjacent chip

Return connected.Count >= 3
```
---

## Usage

### Setup
```
csharp
var matchDetection = new MatchDetectionSystem(boardSystem, chipManager);
```
### Check for Valid Moves
```
csharp
// After board refill
if (!matchDetection.HasPossibleMoves())
{
Debug.Log("No moves available - shuffle required");
shuffleSystem.Shuffle();
}
```
### Integration with Refill
```
csharp
protected override IEnumerator Refill(List<ChipBase> chips)
{
yield return DestroyChips(chips);
yield return ApplyGravity();
yield return SpawnNewChips();

    // Check if shuffle needed
    if (!MatchDetectionSystem.HasPossibleMoves())
    {
        Debug.Log("No possible moves. Shuffling board...");
        // Trigger shuffle
    }
}
```
---

## Match Detection Examples

### Valid Matches (3+ connected):
```

Horizontal:
R R R .    (3 red chips)

Vertical:
B
B
B
.          (3 blue chips)

L-Shape:
G G .
G . .      (3 green chips)

Complex:
Y Y Y
Y . .      (4 yellow chips)
```
### Invalid (< 3 or not connected):
```

Diagonal (not adjacent):
R . .
. R .      (Not valid - diagonals don't count)
. . R

Separated:
R . R      (Not valid - not connected)

Only 2:
B B .      (Not valid - needs 3+)
```
---

## Algorithm Details

### Flood-Fill (Recursive)
```
csharp
FindConnectedChips(current, targetType, visited, connected):
// Base cases
If current is null: return
If current already visited: return
If current.LinkType != targetType: return

// Mark as visited
visited.Add(current)
connected.Add(current)

// Recursive calls for all 4 directions
For each direction (up, down, left, right):
adjacentChip = FindChipAt(row + dir[0], col + dir[1])
FindConnectedChips(adjacentChip, targetType, visited, connected)
```
### Directions
```
csharp
[-1, 0]  // Up
[1, 0]   // Down
[0, -1]  // Left
[0, 1]   // Right
```
---

## Performance

### Complexity
- **Best Case**: O(1) - First chip checked forms match
- **Worst Case**: O(n²) - Full board scan with flood-fill per chip
- **Typical Case**: O(n) - Match found in first few chips

### Optimization Techniques
- **Early exit**: Returns `true` on first valid match found
- **HashSet tracking**: O(1) visited checks during flood-fill
- **Lazy evaluation**: Doesn't find all matches, just validates one exists

### Performance Characteristics
For 12x12 board (144 chips):
- **Worst case**: ~144 flood-fill operations
- **Each flood-fill**: Up to 144 chips checked
- **Total**: ~20,736 operations (worst case)
- **Real-world**: Much faster due to early exits

### When to Call
```
csharp
❌ Every frame (too expensive)
❌ During active gameplay (unnecessary)
✅ After board refill completes (infrequent)
✅ When no matches detected for long time (rare)
```
---

## Important Notes

### Read-Only Operation
- Does not modify board state
- Does not modify chip state
- Safe to call multiple times
- No side effects

### Match Validation vs Detection
- **This system**: Validates moves exist (yes/no)
- **Link system**: Actually performs matching (player-driven)
- **Refill system**: Triggers this for shuffle decision

### Adjacency Rules
- Uses same adjacency as `chip.IsAdjacent()`
- Typically 4-directional (orthogonal)
- Respects chip-specific adjacency rules
- Diagonals not counted (unless chip overrides)

---

## Design Patterns

- **Template Method**: Base defines structure, derived implements algorithm
- **Strategy Pattern**: Different match detection algorithms possible
- **Visitor Pattern**: Flood-fill visits each connected chip
- **Dependency Injection**: Board and chip manager via constructor

---

## Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| False positives | Diagonal chips counted | Ensure 4-directional check only |
| Performance slow | Called every frame | Only call after refill completion |
| Stack overflow | Infinite recursion | HashSet prevents revisiting chips |
| Wrong count | Visited set not cleared | New HashSet per `CanFormMatch()` call |

---

## Future Enhancements

- **Match caching**: Store last detection result
- **Partial board scan**: Only check changed regions
- **Parallel processing**: Multi-threaded flood-fill
- **Pattern detection**: Identify specific match shapes
- **Hint system**: Return actual matches for UI display
- **Difficulty scaling**: Adjust minimum match count

---

## Integration Points

Used by:
- **BoardRefillSystem** - Checks for shuffle after refill
- **ShuffleSystem** - (Future) Validates shuffle created valid board