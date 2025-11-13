# Turn System

Manages turn-based gameplay mechanics using observable ScriptableObject pattern for reactive UI updates and game-over detection.

## Structure

- **Abstract/**
  - **TurnSystemBase.cs** - Abstract turn management framework
- **Observable/**
  - **GameTurn.cs** - Observable ScriptableObject for turn count
- **TurnSystem.cs** - Concrete turn tracking implementation

## Architecture
```

TurnSystemBase (abstract)
└── TurnSystem (concrete)
└── Uses: GameTurn (observable ScriptableObject)
```
**Design Philosophy:**
- **Observable pattern** = Reactive turn updates for UI
- **ScriptableObject-based** = Designer-friendly configuration
- **Decrement-only** = Turns decrease until game over

---

## Key Components

### GameTurn
Observable ScriptableObject representing remaining turns.

**Features:**
- Inherits from `ScriptableObservableBase<int>`
- Fires `OnValueChanged` event when turn count updates
- Configurable in Unity Inspector
- Serializable and reusable

**Usage:**
```
csharp
// In Unity: Create → Observables → GameTurn
[SerializeField] private GameTurn playerTurn;

// Subscribe to changes
playerTurn.OnValueChanged += OnTurnChanged;

// Update turns
playerTurn.SetValue(10);
```
**Benefits:**
- **Reactive UI**: Bind turn counter directly to GameTurn
- **Designer control**: Set initial turns without code
- **Runtime debugging**: Inspect/modify in editor
- **Event-driven**: Automatic propagation to listeners

---

### TurnSystemBase
Abstract base for turn-based systems.

**Dependencies:**
- `GameTurn playerTurn` - Observable turn counter

**Key Members:**
- `IsTurnAvailable` - Abstract property indicating turns remain
- `FinishTurn()` - Abstract method to consume a turn
- `HandleTurnChanged(value)` - Abstract callback for turn updates

**Lifecycle:**
```
csharp
Constructor:
Subscribe to playerTurn.OnValueChanged

FinishTurn():
Decrement turn count
→ Triggers OnValueChanged
→ Calls HandleTurnChanged()

Dispose():
Unsubscribe from playerTurn.OnValueChanged
```
---

### TurnSystem
Concrete implementation with availability tracking.

**Features:**
- Tracks if turns remain (game-over detection)
- Decrements turns on `FinishTurn()`
- Logs remaining turns (debug)

**Implementation:**
```
csharp
public override bool IsTurnAvailable => _isTurnAvailable;

public override void FinishTurn()
{
PlayerTurn.SetValue(PlayerTurn.Value - 1);
}

protected override void HandleTurnChanged(int value)
{
Debug.Log($"Remaining turns: {value}");
_isTurnAvailable = value > 0;
}
```
**Turn Availability:**
- `true`: `PlayerTurn.Value > 0` (game continues)
- `false`: `PlayerTurn.Value <= 0` (game over)

---

## Usage

### Setup (Unity Editor)
```

1. Create GameTurn asset:
    - Right-click → Create → Observables → GameTurn
    - Create "PlayerTurn" (initial value set in code)

2. Assign in GameManager:
   [SerializeField] private GameTurn playerTurn;
   [SerializeField] private int initialTurns = 30;
```
### Initialization
```csharp
private TurnSystem _turnSystem;

void Awake()
{
    // Create turn system with initial count
    _turnSystem = new TurnSystem(playerTurn, initialTurns: 30);
}

void Start()
{
    // Enable input if turns available
    if (_turnSystem.IsTurnAvailable)
    {
        _inputHandler.Enable();
    }
}
```
```


### Consuming Turns
```csharp
void HandleLinkComplete(List<ILinkable> linkables)
{
    // Process match
    boardRefillSystem.StartRefill(linkables.Cast<ChipBase>().ToList());
    
    // Consume turn
    _turnSystem.FinishTurn();
    
    // Check game over
    if (!_turnSystem.IsTurnAvailable)
    {
        Debug.Log("Game Over - No turns remaining");
        OnGameOver();
    }
}
```


### UI Binding
```csharp
public class TurnUI : MonoBehaviour
{
    [SerializeField] private GameTurn playerTurn;
    [SerializeField] private TextMeshProUGUI turnText;
    
    void OnEnable()
    {
        playerTurn.OnValueChanged += UpdateUI;
        UpdateUI(playerTurn.Value);
    }
    
    void OnDisable()
    {
        playerTurn.OnValueChanged -= UpdateUI;
    }
    
    void UpdateUI(int turns)
    {
        turnText.text = $"Turns: {turns}";
        
        // Visual feedback
        if (turns <= 5)
            turnText.color = Color.red; // Low turns warning
    }
}
```


### Cleanup
```csharp
void OnDestroy()
{
    _turnSystem?.Dispose();
}
```


---

## Turn Flow

```
Player completes match
  ↓
HandleLinkComplete()
  ↓
_turnSystem.FinishTurn()
  ↓
playerTurn.SetValue(current - 1)
  ↓
playerTurn.OnValueChanged event fires
  ↓
HandleTurnChanged(newValue)
  ↓
Update _isTurnAvailable (newValue > 0)
  ↓
UI updates automatically (subscribed to OnValueChanged)
  ↓
Check IsTurnAvailable:
  true  → Continue gameplay
  false → Trigger game over
```


---

## Important Notes

### Turn Consumption
- **When to call**: After successful match completion
- **Before refill**: Call `FinishTurn()` before board refill
- **Not on invalid actions**: Only consume on valid moves
- **One per action**: Single turn consumed per match

### Game Over Detection
```csharp
// Check after each turn
if (!_turnSystem.IsTurnAvailable)
{
    _inputHandler.Disable();
    ShowGameOverUI();
}
```


### Observable Pattern Benefits
- **Decoupled UI**: Turn counter updates independently
- **Multiple listeners**: UI, audio, VFX can all react
- **Event-driven**: No polling required
- **Reactive**: Automatic propagation

### ScriptableObject Advantages
- **Inspector-friendly**: Designers set initial turns
- **Runtime debugging**: View/modify in editor
- **Persistence**: Can persist across scenes if needed
- **Reusable**: Same GameTurn asset used by multiple systems

---

## Design Patterns

- **Observer Pattern**: GameTurn notifies listeners via events
- **Template Method**: Base defines structure, derived implements logic
- **ScriptableObject Pattern**: Data-driven configuration
- **Dependency Injection**: GameTurn passed via constructor

---

## Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Turns don't update | Missing event subscription | Subscribe in constructor, unsubscribe in Dispose() |
| UI not reactive | Direct value access | Use GameTurn.OnValueChanged event |
| Turns persist between levels | ScriptableObject state | Reset playerTurn.SetValue(initialCount) on level start |
| Memory leak | Missing Dispose() | Call Dispose() in OnDestroy |
| Negative turns | No validation | Add check: `if (value <= 0)` in HandleTurnChanged |

---