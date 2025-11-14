# UI Management

This folder contains the UI layer for the Wonder Link Clone project.  
It is focused on **state-driven views**, **event-based transitions**, and **simple, testable bindings** to game data (score, turns).

---

## 1. Overview

The UI is organized around a small state machine:

- **States** describe high-level UI modes (start, gameplay, win/lose, shuffle failed).
- **Views** are self-contained canvases that represent one state.
- **UIManager** listens to gameplay events and switches between views.
- **Observables** (score, turns) drive the in-game HUD reactively.

This keeps gameplay logic and UI presentation loosely coupled.

---

## 2. Structure

- `State/`
  - `UIState`: enum defining all high-level UI states.
- `View/`
  - `Abstract/UIViewBase`: base class for all views (lifecycle + show/hide).
  - `Template/SingleButtonEventViewBase`: template for “one button → one event” views.
  - `StartView`: start screen with “Play” button.
  - `GameplayView`: in-game HUD (score + remaining turns).
  - `LevelCompletedView`: shown when level is completed.
  - `NoMoreTurnsView`: shown when the player runs out of turns.
  - `ShuffleFailedView`: shown when shuffles cannot recover the board.
- `Extension/`
  - `UIComponentExtensions`: `CanvasGroup.Show/Hide()` helpers.
- `UIManager.cs`
  - Central controller wiring game events to UI states and views.

---

## 3. UI Flow

High-level flow:

1. **Startup**
   - `UIManager` validates configuration and subscribes to game events.
   - Calls `SetState(initialState)` (typically `StartGame`).

2. **State Transitions (examples)**
   - Start button → raises “start game” event → `UIManager` switches to `Gameplay`.
   - When gameplay reports:
     - **Level completed** → `LevelCompletedView`.
     - **No turns left** → `NoMoreTurnsView`.
     - **Shuffle failed permanently** → `ShuffleFailedView`.
   - “Next game” flows (handled in gameplay) eventually drive UI back to `StartGame`.

3. **Gameplay HUD**
   - `GameplayView` listens to score/turn observables and updates TMP labels.
   - On `Start`, it also pulls the latest values so UI is correct even before changes.

---

## 4. Design & Good Practices

- **State-based views**
  - Each view declares its `UIState` and `UIViewBase` handles lifecycles.
  - `UIManager` never needs to know internal details about a view.

- **Composition over inheritance (with small, focused bases)**
  - `UIViewBase`:
    - Validates `CanvasGroup`.
    - Exposes `Show/Hide` and `Enable/Disable` hooks.
    - Guards lifecycle via `OnAwake()` returning a `bool`.
  - `SingleButtonEventViewBase`:
    - Encapsulates the common “single button raises a `GameEvent`” pattern.

- **Event- and data-driven UI**
  - Game events drive **which view** is visible.
  - Observable ScriptableObjects drive **what each view displays** (score, turns).

- **Defensive configuration**
  - All critical references (buttons, events, texts, observables, views list) are validated in `Awake` / `OnAwake`.
  - If configuration is invalid, the view/UI manager marks itself as not “awaken” and skips subscriptions, logging clear errors.

- **Inspector-friendly**
  - All references are serialized fields.
  - Adding/changing UI behavior mostly involves wiring prefabs and ScriptableObjects, not changing code.

---

## 5. Extending the UI

When adding new UI behavior, prefer these patterns:

1. **New state screen (e.g., “Pause”)**
   - Add a value to `UIState`.
   - Create a new view:
     - Inherit from `UIViewBase` for custom logic, or
     - Inherit from `SingleButtonEventViewBase` if it is just a “single button + event” screen.
   - Add an instance of the new view to `UIManager.views` in the scene.
   - Hook up a `GameEvent` from gameplay that triggers `UIManager.SetState(newState)`.

2. **New HUD element in `GameplayView`**
   - Add TMP text (or other UI component) to the view.
   - Add a serialized field and validation in `OnAwake`.
   - Subscribe to the relevant observable or game event in `Enable`, unsubscribe in `Disable`.
   - Initialize the display in `Start` (or in `Enable`, if you want it to update on re-enable).

3. **New UI interaction pattern**
   - If the pattern looks reusable (e.g., “two buttons with confirm/cancel events”), consider adding a new small template base class in `View/Template` to keep concrete views minimal and consistent.

---

## 6. Things for Reviewers to Check

When reviewing UI changes:

- **Lifecycle correctness**
  - Are subscriptions done in `Enable` and unsubscribed in `Disable`?
  - Does `OnAwake()` validate all required references and return `false` when something is missing?

- **State wiring**
  - Is the new view added to `UIManager.views`?
  - Is there a clear event that triggers the transition into and out of the new state?

- **Coupling**
  - Does UI only depend on:
    - Scriptable observables,
    - Game events,
    - Or `UIState` transitions?
  - Avoid direct, hard coupling from UI into core gameplay logic.

Keeping these points in mind will help maintain a clean, predictable UI layer that matches the rest of the project’s architecture.