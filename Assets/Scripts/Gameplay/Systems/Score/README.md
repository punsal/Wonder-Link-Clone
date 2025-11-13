# Score System

Manages player score tracking and level goal validation using observable ScriptableObject pattern for reactive UI updates.

## Structure

- **Abstract/**
  - **ScoreSystemBase.cs** - Abstract score management framework
- **Amount/Interface/**
  - **IScoreAmountProvider.cs** - Score value provider contract
- **Observable/**
  - **GameScore.cs** - Observable ScriptableObject for score values
- **ScoreSystem.cs** - Concrete score tracking implementation

## Architecture
```

ScoreSystemBase (abstract)
└── ScoreSystem (concrete)
├── Uses: GameScore (observable ScriptableObject)
└── Uses: IScoreAmountProvider (score amounts)
```
**Design Philosophy:**
- **Observable pattern** = Reactive score updates for UI
- **ScriptableObject-based** = Designer-friendly, serializable configuration
- **Provider pattern** = Flexible score calculation sources

---

## Key Components

### GameScore
Observable ScriptableObject representing a score value.

**Features:**
- Inherits from `ScriptableObservableBase<int>`
- Fires `OnValueChanged` event when score updates
- Configurable in Unity Inspector
- Serializable and reusable across scenes

**Usage:**
```
csharp
// In Unity: Create → Observables → GameScore
[SerializeField] private GameScore playerScore;
[SerializeField] private GameScore levelTargetScore;

// Subscribe to changes
playerScore.OnValueChanged += OnScoreChanged;

// Update score
playerScore.SetValue(100);
```
**Benefits:**
- **Reactive UI**: Bind UI elements directly to GameScore
- **Designer control**: Non-programmers can set goals
- **Persistence**: ScriptableObject survives scene changes
- **Debugging**: Inspect values in editor at runtime

---

### IScoreAmountProvider
Interface for objects that provide score values.

**Contract:**
```
csharp
public interface IScoreAmountProvider : IProvider<int>
{
int Instance { get; }
}
```
**Purpose:**
- Allows different score sources (chips, combos, bonuses)
- Decouples score calculation from score tracking
- Strategy pattern for score amounts

**Example Implementations:**
```
csharp
// Chip provides base score
public class ChipBase : IScoreAmountProvider
{
public int Instance => 10; // Each chip worth 10 points
}

// Combo multiplier
public class ComboScore : IScoreAmountProvider
{
private int _comboMultiplier;
public int Instance => 10 * _comboMultiplier;
}
```
---

### ScoreSystemBase
Abstract base for score management systems.

**Dependencies:**
- `GameScore levelScore` - Target score to reach (goal)
- `GameScore playerScore` - Current player score (tracked)

**Key Members:**
- `IsScoreReached` - Abstract property indicating goal completion
- `AddScore(provider)` - Increments player score by provider amount
- `HandlePlayerScoreChanged(value)` - Abstract callback for score updates

**Lifecycle:**
```
csharp
Constructor:
Subscribe to playerScore.OnValueChanged

AddScore():
playerScore.SetValue(current + provider.Instance)
→ Triggers OnValueChanged
→ Calls HandlePlayerScoreChanged()

Dispose():
Unsubscribe from playerScore.OnValueChanged
```
---

### ScoreSystem
Concrete implementation with goal validation.

**Features:**
- Tracks if player reached level target
- Logs score changes (debug)
- Simple threshold comparison

**Implementation:**
```
csharp
public override bool IsScoreReached => _isScoreReached;

protected override void HandlePlayerScoreChanged(int value)
{
Debug.Log($"Score: {value}/{LevelScore.Value}");
_isScoreReached = value >= LevelScore.Value;
}
```
---

## Usage

### Setup (Unity Editor)
```

1. Create GameScore assets:
    - Right-click → Create → Observables → GameScore
    - Create "PlayerScore" (starts at 0)
    - Create "LevelTarget" (e.g., 1000 points)

2. Assign in GameManager:
   [SerializeField] private GameScore playerScore;
   [SerializeField] private GameScore levelTargetScore;
```
### Initialization
```
csharp
private ScoreSystem _scoreSystem;

void Awake()
{
// Reset scores
playerScore.SetValue(0);
levelTargetScore.SetValue(1000);

    // Create score system
    _scoreSystem = new ScoreSystem(levelTargetScore, playerScore);
}
```
### Adding Score
```
csharp
// After match completion
void HandleLinkComplete(List<ILinkable> linkables)
{
foreach (var linkable in linkables)
{
if (linkable is IScoreAmountProvider provider)
{
_scoreSystem.AddScore(provider);
}
}

    // Check win condition
    if (_scoreSystem.IsScoreReached)
    {
        Debug.Log("Level Complete!");
        // Trigger win sequence
    }
}
```
### UI Binding
```
csharp
public class ScoreUI : MonoBehaviour
{
[SerializeField] private GameScore playerScore;
[SerializeField] private TextMeshProUGUI scoreText;

    void OnEnable()
    {
        playerScore.OnValueChanged += UpdateUI;
        UpdateUI(playerScore.Value);
    }
    
    void OnDisable()
    {
        playerScore.OnValueChanged -= UpdateUI;
    }
    
    void UpdateUI(int score)
    {
        scoreText.text = $"Score: {score}";
    }
}
```
### Cleanup
```
csharp
void OnDestroy()
{
_scoreSystem?.Dispose();
}
```
---

## Score Flow
```

Player makes match
↓
HandleLinkComplete(linkables)
↓
For each linkable (IScoreAmountProvider):
_scoreSystem.AddScore(provider)
↓
playerScore.SetValue(current + amount)
↓
playerScore.OnValueChanged event fires
↓
HandlePlayerScoreChanged(newValue)
↓
Check if newValue >= levelScore
↓
Update _isScoreReached flag
↓
UI updates automatically (subscribed to OnValueChanged)
```
---

## Important Notes

### Observable Pattern Benefits
- **Decoupled UI**: UI components subscribe independently
- **Reactive updates**: Automatic propagation of changes
- **Multiple listeners**: Score, UI, audio, VFX can all react
- **Event-driven**: No polling required

### ScriptableObject Advantages
- **Inspector-friendly**: Designers set goals without code
- **Scene-independent**: Persists across scene transitions
- **Runtime debugging**: View/modify values in editor
- **Reusable**: Same GameScore asset used in multiple systems

### Score Providers
- Any object implementing `IScoreAmountProvider` can contribute score
- Enables flexible scoring (chips, combos, bonuses, time)
- Strategy pattern allows runtime calculation

### Thread Safety
- Not thread-safe (Unity main thread only)
- Event subscriptions should be on main thread
- Use proper subscription/unsubscription lifecycle

---

## Design Patterns

- **Observer Pattern**: GameScore notifies listeners via events
- **Strategy Pattern**: IScoreAmountProvider for different score sources
- **Template Method**: Base defines structure, derived implements validation
- **ScriptableObject Pattern**: Data-driven configuration

---

## Extensibility

### Chip Score Provider
```
csharp
public class BasicChip : ChipBase, IScoreAmountProvider
{
[SerializeField] private int scoreValue = 10;

    public int Instance => scoreValue;
}
```
---

## Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| Score not updating | Missing event subscription | Subscribe in OnEnable, unsubscribe in OnDisable |
| UI not reactive | Direct value access | Use GameScore.OnValueChanged event |
| Score persists between levels | ScriptableObject state | Reset playerScore.SetValue(0) on level start |
| Memory leak | Missing Dispose() | Call Dispose() in OnDestroy/OnDisable |