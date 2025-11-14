# GameManager

Central coordinator for the Wonder Link Clone gameplay.  
It wires together all core systems (board, chips, linking, refills, shuffles, score, turns, input, camera) and connects them to the UI/event layer.

This file lives at the scene level and is designed as an **orchestrator**, not a place for low-level gameplay logic.

---

## 1. Responsibilities

GameManager handles:

- **System lifecycle**
  - Creates all systems in `Awake`.
  - Subscribes to events in `OnEnable`.
  - Initializes gameplay in `Start`.
  - Forwards input updates in `Update`.
  - Unsubscribes and disposes systems in `OnDisable`.

- **Gameplay loop**
  - Initializes the board and chips.
  - Handles “link completed” → scoring → destruction → refill.
  - Checks win/lose conditions after refills.
  - Detects deadlocks and triggers shuffles.
  - Drives replay (starting a new game run without reloading the scene).

- **Integration with UI / events**
  - Raises events for:
    - Level completed
    - No more turns
    - Shuffle permanently failed
    - Next game / restart
  - Listens to events for:
    - Start game (enables player input)
    - Next game (replays the game)

---

## 2. Systems It Manages

GameManager constructs and owns:

- **Camera system**
  - Centers and sizes the camera to frame the board.
  - Uses either injected camera providers or falls back to `Camera.main`.

- **Board system**
  - Creates and manages the tile grid.

- **Chip manager**
  - Spawns initial chips.
  - Destroys and refills chips during gameplay.

- **Link system**
  - Handles drag-based linking of chips.
  - Emits “link completed” when a valid chain is made.

- **Board refill system**
  - Animates chip destruction, gravity, and new chip spawns.

- **Match detection system**
  - Checks if a board has any possible moves left.

- **Shuffle system**
  - Shuffles chip positions when no moves are available.
  - Has a limited number of shuffle attempts before failing.

- **Score system**
  - Tracks player score against a level target using ScriptableObject scores.

- **Turn system**
  - Manages remaining turns using a ScriptableObject turn counter.

- **Input handler**
  - Chooses mouse or touch input by platform.
  - Delegates interactions to the link system.
  - Is enabled/disabled depending on game state (e.g., disabled during refills and shuffles).

---

## 3. Lifecycle & Flow

### Initialization

1. **Awake**
   - `CreateSystems()`:
     - Builds camera, board, link, chip, refill, match detection, shuffle, score, turn, and input systems.
     - Performs configuration validation (prefabs, ScriptableObjects, events).

2. **OnEnable**
   - `SubscribeToEvents()`:
     - Subscribes to:
       - Link completed
       - Refill completed
       - Shuffle completed
       - Start game event
       - Next game event

3. **Start**
   - `CreateGameplay()`:
     - Resets shuffle counter.
     - Initializes board.
     - Fills board with chips.
     - Centers camera.
     - Checks whether an initial shuffle is needed.

### Runtime

- **Update**
  - Forwards per-frame updates to the input handler.

- **On link completed**
  - Disables input.
  - Consumes a turn.
  - Maps linkables to chips and adds their score.
  - Triggers the refill system.

- **On refill completed**
  - If target score reached → raises *level completed* event.
  - Else if no turns left → raises *no more turns* event.
  - Else if no moves available → starts shuffle.
  - Otherwise → re-enables input and continue play.

- **On shuffle completed**
  - Increments shuffle attempt counter.
  - If still no moves and attempts remain → shuffle again.
  - If no moves and no attempts left → raises *shuffle failed* event.
  - Otherwise → re-enables input.

### Teardown & Replay

- **OnDisable**
  - `UnsubscribeFromEvents()`.
  - `DestroySystems()` (disposes all managed systems).

- **Replay flow**
  - On *next game* event:
    - Unsubscribes and destroys current systems.
    - Recreates systems.
    - Re-subscribes to events.
    - Rebuilds gameplay (board, chips, camera).
    - Leaves input disabled until the next *start game* event.

---

## 4. Configuration (Inspector)

GameManager is heavily configured from the Inspector:

- **Board**
  - Row and column count.
  - Board tile prefab.

- **Chips**
  - List of chip prefabs.

- **Cameras**
  - Game camera provider.
  - Link camera provider.
  - Link layer mask.

- **Gameplay**
  - Level target score.
  - Max turns.
  - Max shuffle attempts before failure.

- **ScriptableObjects**
  - Level score asset.
  - Player score asset.
  - Player turn asset.

- **Events**
  - Start game.
  - No more turns.
  - Level completed.
  - Shuffle failed.
  - Next game.

When any required reference is missing, GameManager emits clear error logs and falls back to reasonable defaults only where safe (e.g., creating score/turn ScriptableObjects at runtime).

---

## 5. Extension Guidelines

When modifying or extending GameManager:

- **Add new systems** in `CreateSystems` / `DestroySystems`, and wire their events in `SubscribeToEvents` / `UnsubscribeFromEvents`.
- **Keep GameManager as an orchestrator**:
  - New gameplay rules should live in their own systems or components.
  - GameManager should only coordinate them and translate events to high-level actions or UI events.
- **Keep event handling linear and simple**:
  - Avoid deep nesting in handlers; prefer small methods for new transitions or checks.
- **Maintain replay safety**:
  - Any new system should be created, subscribed, disposed, and unsubscribed consistently so `Replay()` remains reliable.

This keeps GameManager readable, testable (at the system level), and maintainable as the main entry point for the game’s runtime behavior.