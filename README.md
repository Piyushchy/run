# VR Runner Game
## Download Here https://github.com/Piyushchy/run/blob/main/run.apk
A VR endless runner game built with Unity 2022.3.62f3.

## Features

- VR head tracking for jumping and ducking
- Lane-based movement (left, center, right)
- Obstacle spawning system
- Score based on distance traveled
- Game over on collision

## Setup

1. Open the project in Unity 2022.3.62f3 or later.
2. In Unity Editor, go to VR Runner > Setup Scene to automatically set up the scene.
3. Ensure XR settings are configured for your VR headset.
4. Build and run for your target platform.

## Scripts

- **GameManager.cs**: Manages game state, score, and game over.
- **PlayerController.cs**: Handles player movement, VR input, and physics.
- **LaneManager.cs**: Manages lane positions and transitions.
- **ObstacleSpawner.cs**: Spawns obstacles at random intervals.

## Controls

- Move head up to jump
- Move head down to duck/roll
- Lane changes are automatic or based on input (customize as needed)

## Development

Edit scripts in `Assets/Scripts/`.
Use the Editor script `VRRunnerSetup.cs` to quickly set up scenes.
