# Input Handling

Abstracts input handling for drag-based gameplay interactions with support for multiple input methods (mouse, touch).

## Structure

- **Abstract/** - Base classes for input handling
  - **InputHandlerBase.cs** - Abstract input handler with enable/disable lifecycle
- **MouseInputHandler.cs** - Mouse/desktop input implementation
- **TouchInputHandler.cs** - Touch/mobile input implementation

## Key Components

### InputHandlerBase
Abstract base providing input handling lifecycle and `LinkSystemBase` integration.

**Lifecycle:**
- `Enable()` / `Disable()` - Controls input processing state
- `Update()` - Processes input when enabled (call from MonoBehaviour)
- `OnEnabled()` / `OnDisabled()` - Virtual hooks for derived classes

**Abstract:**
- `ProcessInput()` - Platform-specific input detection

**Design:**
- Guards processing with enabled state check
- Delegates drag operations to `LinkSystemBase`
- Provides extension points via virtual hooks

### MouseInputHandler
Desktop input using mouse button 0 (left click).

**Mapping:**
- Mouse down → `StartDrag()`
- Mouse held + dragging → `UpdateDrag()`
- Mouse up + dragging → `EndDrag()`

**Safety:** Forces drag end on disable to prevent stuck states.

### TouchInputHandler
Mobile input using single-finger touch.

**Mapping:**
- `TouchPhase.Began` → `StartDrag()`
- `TouchPhase.Moved/Stationary` → `UpdateDrag()` (if dragging)
- `TouchPhase.Ended/Canceled` → `EndDrag()` (if dragging)

**Features:**
- Single-touch only (uses `GetTouch(0)`)
- Handles touch cancellation
- Forces drag end on disable

## Usage
```
csharp
// Setup
private InputHandlerBase _inputHandler;

private void Awake()
{
var linkSystem = new LinkSystem(cameraProvider, layerMask);

    // Platform-specific selection
#if UNITY_EDITOR || UNITY_STANDALONE
_inputHandler = new MouseInputHandler(linkSystem);
#else
_inputHandler = new TouchInputHandler(linkSystem);
#endif
}

private void OnEnable() => _inputHandler.Enable();
private void Update() => _inputHandler.Update();
private void OnDisable() => _inputHandler.Disable();
```
## Extending
```
csharp
public class KeyboardInputHandler : InputHandlerBase
{
protected override void ProcessInput()
{
// Custom input logic
if (Input.GetKeyDown(KeyCode.Space))
LinkSystem.StartDrag();
}

    protected override void OnDisabled()
    {
        base.OnDisabled();
        if (LinkSystem.IsDragging)
            LinkSystem.EndDrag();
    }
}
```
## Design Patterns

- **Template Method**: Base defines structure, derived classes implement `ProcessInput()`
- **Strategy Pattern**: Input handlers are interchangeable at runtime
- **State Pattern**: Tracks enabled/disabled state

## Important Notes

- Must call `Update()` from MonoBehaviour Update loop
- Both handlers force-end drag on disable for safety
- State-aware: Checks `IsDragging` before update/end operations
- No input buffering - processes immediately
- Touch handler throws exception for unsupported touch phases

## Architecture Benefits

- **Platform Agnostic**: Easy to add new input methods (gamepad, VR, etc.)
- **Decoupled**: Input logic separated from link/gameplay systems
- **Testable**: Can mock input handlers for testing
- **Flexible**: Runtime switching between input methods supported