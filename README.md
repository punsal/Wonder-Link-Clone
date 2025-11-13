# Wonder Link Clone

A minimalist, open-source take on a link-match puzzle game built with Unity. Connect adjacent chips of the same type to score points before you run out of turns. The project is structured as a clean, modular example featuring camera, board, linking, refill, shuffle, score, and turn systems.

## Highlights

- Link-to-match gameplay loop (drag to connect adjacent, same-type chips)
- Configurable board size (rows/columns) and chip set
- Automatic board refill and deadlock-aware shuffling
- Score and turn management with easy Inspector tuning
- Clean separation of gameplay systems for learning and extension

## Requirements

- Unity 2022.3 LTS (tested with 2022.3.62f2)
- .NET Framework profile used by Unity (C# 9.0 via Unity’s compiler)

## Getting Started

1. Clone or download this repository.
2. Open the project folder in Unity Hub and select Unity 2022.3.62f2 (or 2022.3 LTS).
3. Open the main scene (e.g., a sample scene included in the project) or create your own.
4. Press Play.

If you don’t see a preconfigured scene, add the `GameManager` to an empty scene and follow the configuration below.

## How to Play

- Click/tap and drag across adjacent chips of the same type to create a link.
- Release to clear the linked chips and earn points.
- New chips will drop in to refill the board automatically.
- Use strategy to reach the level score before your turns run out.

Controls may vary based on your input settings. By default, mouse or touch drag is expected for linking.

## Configuring the Game (GameManager)

The central entry point is `Assets/Scripts/GameManager.cs`. Add it to a scene and configure via the Inspector.

Camera
- Game Camera Provider: Assign a `UnityCameraProviderBase` that your camera system uses for gameplay rendering.

Board
- Row Count: 4–12 (default 8)
- Column Count: 4–12 (default 8)
- Board Tile Prefab: Assign a `TileBase` prefab used for the grid tiles.

Chip
- Chip Prefabs: List of `ChipBase` prefabs representing distinct chip types available on the board.

Linking
- Link Camera Provider: Typically your UI/input camera provider implementing `UnityCameraProviderBase`.
- Link Layer Mask: The physics layer(s) used for detecting linkable chips or interaction.

Gameplay
- Max Level Score: Total points required to “beat” the level (default 100).
- Max Player Turn: Number of turns the player has (default 10).

System Configuration
- Shuffle Count Before Failure: How many times the board may attempt to shuffle when no moves remain (default 2).
- Level Score: Reference to a `GameScore` ScriptableObject or component tracking the target score.
- Player Score: Reference to a `GameScore` that tracks the player’s running score.
- Player Turn: Reference to a `GameTurn` that tracks remaining turns.

Under the hood, `GameManager` wires up and orchestrates:
- CameraSystemBase
- BoardSystemBase
- LinkSystemBase
- ChipManagerBase
- BoardRefillSystemBase
- MatchDetectionSystemBase
- ShuffleSystemBase
- ScoreSystemBase
- TurnSystemBase
- InputHandlerBase

These systems are designed behind abstract/base types to keep responsibilities separated and enable swaps or extensions.

## Project Structure (high level)

- Assets/
  - Scripts/
    - Core/ … core systems (board, camera, link, etc.)
    - Gameplay/ … concrete gameplay systems (input, refill, match detection, shuffle, score, turn)
    - GameManager.cs … scene entry point that composes systems
- LICENSE … project license

Note: Namespaces in code (e.g., `Core.Board`, `Gameplay.Systems.MatchDetection`) mirror this modular layout.

## Build and Deployment

1. Open File → Build Settings…
2. Add your active scene(s) to the build.
3. Choose your target platform.
4. Click Build (or Build and Run).

Mobile: The link-drag mechanic works well on touch screens. You may need to fine-tune input handling and layers for your device.

## Troubleshooting

- No input or linking not detected
  - Ensure the Link Camera Provider is set and the Link Layer Mask includes your chip layers.
  - Verify chip prefabs have the required colliders and components.

- Board doesn’t refill
  - Confirm `BoardRefillSystemBase` has concrete implementations in scene or via setup.
  - Make sure chip prefabs are assigned and valid.

- Immediate “no moves” or frequent shuffles
  - Increase chip variety or adjust board size.
  - Raise “Shuffle Count Before Failure” to allow more recovery attempts.

## Contributing

Contributions, bug reports, and feature ideas are welcome. Please open an issue describing your proposal before submitting a PR.

## License

This project is distributed under the terms of the LICENSE file included in the repository.
