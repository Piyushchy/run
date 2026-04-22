# Project Guidelines

## Code Style
Use PascalCase for class names, camelCase for fields/methods. Organize Inspector fields with `[Header("...")]` attributes. Follow Unity's standard folder structure: Scripts in `Assets/Scripts/`, Editor tools in `Assets/Editor/`. Reference `Assets/Scripts/PlayerController.cs` for VR input handling patterns and `Assets/Editor/VRRunnerSetup.cs` for Editor scripting conventions.

## Architecture
Endless runner game with VR integration: Player stationary, obstacles move backward. Use singletons for managers (e.g., `GameManager.Instance`). Components loosely coupled via references. XR input with keyboard fallback. Lane-based movement with 3 positions. See `Assets/Scripts/GameManager.cs` for state management and `Assets/Scripts/ObstacleSpawner.cs` for spawning logic.

## Build and Test
Build via Unity Editor menu "Build/Build Android APK" using `Assets/Editor/BuildScript.cs`. Test in Unity play mode. Requires Unity 2022.3.62f3+ with Android module. See README.md for setup and VR configuration.

## Conventions
Singleton pattern for global managers. XR input via `InputDevices` with head calibration. Tags for collision ("Ground", "Obstacle"). Gizmos for debugging (e.g., lane lines). Object pooling for obstacles. Speed ramping for difficulty. Reference `Assets/Scripts/LaneManager.cs` for utility patterns.