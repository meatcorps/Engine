# Meatcorps Engine & Snake Game Development Status

This document contains a detailed overview of the current state of the Meatcorps custom RayLib-powered game engine and the Snake arcade game project, including completed features, architecture details, and planned features.

## 1. Engine Architecture Overview

The engine is built on RayLib for C#, chosen for its simplicity, cross-platform capabilities, and excellent support for ARM and lower-spec devices compared to MonoGame. The architecture is modular and supports dependency injection via an ObjectManager. Key systems include:
- BaseScene management with sub-scene support
- ObjectManager for game object registration and lookup
- RenderService with strict scene and game object layers for deterministic rendering order
- Camera abstraction with world/UI layers
- Post-processing system with pluggable effects
- Input mapping architecture with PlayerInputRouter for flexible player control mapping
- Arduino arcade controller integration with per-button lighting and animations
- Particle system with fluent builder API and mutators
- Tweening utilities for animation, easing, and interpolation

## 2. Completed Features

- Core scene management and game loop
- Strict rendering layer order (scene layers + game object layers)
- PixelPerfectRenderTarget with post-processing pipeline
- Input abstraction via IInputMapper and IInputMapperWithManager
- PlayerInputRouter to assign mappers to different players
- ArduinoControllerModule with fluent setup API, player enabling, and button animations (BlinkAnimation, FlashAnimation)
- Particle system with support for custom mutators and velocity-based oscillations
- Meat spawn logic ensuring placement only on free grid positions
- Multiplayer support with Arduino double-player arcade controller
- Modular architecture to allow Snake game logic to remain separate from engine internals
- PlayerInputRouter has been implemented.
- Wall spawning after a total number of meat slices eaten, placed once no snake tail is in the location
- Warning system before walls spawn
- Meat rotting over time, turning black and reducing score if eaten
- Boid system for flies that target the oldest meat slices


## 3. Planned Snake Game Features

- Power-ups that grant temporary speed boosts, with timed expiration
- Intro and outro screens for arcade mode

## 4. Planned Engine Improvements

- Implement PlayerInputRouter mapping for mixed control schemes (e.g., Player 1 on keyboard, Player 2 on controller)
- Expanded controller support: keyboard, mouse, and standard gamepads
- Optional benchmarking overlay for FPS, frame time, draw calls, and resolution scaling
- Runtime toggling of post-processing effects
- Low-spec optimization path for Raspberry Pi or weaker PCs (dynamic resolution, reduced effects)

## 5. Hardware Testing

- Current development and testing primarily on MacBook Pro M4 Max (high-end, baseline performance)
- Planned tests on Ryzen 7 7840U mini PC with Radeon 780M
- Steam Deck (Zen 2 APU + RDNA 2 GPU) as a mid-tier benchmark environment
- Expected portability to Raspberry Pi 5 for lightweight builds (without heavy post-processing)

## 6. Next Immediate Steps


- Add power-up spawning and logic
- Create intro/outro screens 
- Optional: Build benchmarking overlay for hardware testing




# Extended Project Status – Snake Game

## AsciiScript System

The AsciiScript system is a generic and extensible parser that allows defining level scripting through ASCII-art style maps and command/variable directives. It consists of the following key components:

- AsciiScriptReader: Parses raw script files, identifying blocks, variables, and commands.
- AsciiScriptParser: Registers command handlers, maps parsed data to runtime logic, and controls script execution state.
- IAsciiScriptCommand interface: Defines the contract for script command modules.

The parser supports pausing/resuming execution, allowing commands like DELAY to control the pacing of gameplay events. Scripts are pre-parsed for performance, but can also be streamed live.

## Generic Command Implementations

A set of reusable, parameterized commands have been implemented for the AsciiScript system:

- DelayCommand: Pauses execution for a given duration in milliseconds. Supports real-time countdown updates.
- BlockGridCommand: Reads ASCII-art grids into a 2D list of characters, triggering callbacks for game logic.
- IntVariableCommand: Reads integer variables and applies them to logic through callbacks.
- StringVariableCommand: Reads string variables and applies them to logic through callbacks.
- SimpleCommand: Executes a fixed action with no parameters.

## GridAnalyzerYX Utility

The GridAnalyzerYX<T> class is a helper for iterating and analyzing rectangular 2D grids represented as List<List<T>>. It supports searching for specific values, iterating all cells, and navigating with neighbor queries. It is primarily used to interpret LEVELDATA blocks in ASCII scripts.

## LevelScene Integration

The LevelScene now integrates with the AsciiScript system to dynamically load and update game states. Registered script commands control the spawning of players, walls, consumables, timed events, and messages.

OnInitialize sets up the parser with commands like LEVELDATA (for grid loading), RANDOMMEAT (spawn consumables), MESSAGE (display in-game UI), and DELAY/WAITFORPOINTS (flow control).

LoadLevel uses the GridAnalyzerYX to translate characters into game objects:
- '#' → Wall
- 'E' → Respawnable consumable
- 'e' → One-time consumable
- '!' / '1' → Player 1 start and body segments
- '@' / '2' → Player 2 start and body segments



## Example level:
```
//.=NOTHING 
// e=MEATONCE 
// E=MEAT RESPAWN RANDOM 
// !=PLAYERSTART1 
// 1=PLAYER 
// @=PLAYER2 START 
// 2=PLAYER2
// #=WALL
TEMPLATE:
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
ENDTEMPLATE
LEVELNAME=It's starting somewhere 
LEVELDATA:
......................................
......................................
......................................
.......1111!..........................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
......................................
..............................@2222...
......................................
......................................
ENDLEVELDATA
RANDOMMEAT1=5
RANDOMMEAT2=5
DELAY=5000
MESSAGE=WATCH OUT! HERE COMES THE WALLS!
LEVELDATA:
######################################
#....................................#
#....................................#
#....................................#
#....................................#
#....................................#
#....................................#
#....................................#
#....................................#
#....................................#
#....................................#
#....................................#
#....................................#
#....................................#
#....................................#
#....................................#
#....................................#
#....................................#
#....................................#
######################################
ENDLEVELDATA
WAITFORPOINTS=1000
MESSAGE=FINAL PUSH!
LEVELDATA:
######################################
#....................................#
#.##################################.#
#....................................#
#....................................#
#.eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee.#
#....................................#
#....................................#
#....................................#
#.##################################.#
#....................................#
#....................................#
#.eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee.#
#....................................#
#....................................#
#....................................#
#....................................#
#.##################################.#
#....................................#
######################################
ENDLEVELDATA
DELAYCOUNTDOWN=30000
ENDLEVEL
```




## Boid System – Fly Swarm Implementation

A new Boid-based AI system has been implemented to simulate flies swarming around meat in the Snake game. The system is based on a `FlockController` that manages multiple `FlyAgent` instances, each implementing `IBoidAgent` and `IGridItem` for spatial grid integration.

Key features:
- Separation, alignment, cohesion, seek, and wander behaviors via `BoidBehaviors` helper class.
- Configurable via `BoidConfig` with parameters such as `MaxSpeed`, `MaxForce`, `NeighborRadius`, `DesiredSeparation`, and weighting factors.
- Additional randomization through burst acceleration (`BurstStrength`, `BurstIntervalSeconds`, `BurstDurationSeconds`).
- Tangential movement around targets when close to add chaos and realism.
- Fully integrated with the spatial entity grid for efficient neighbor lookups.
- Integrated into gameplay through `FlyFlockGameObject`, which handles rendering, spawning, and updates.

AsciiScript commands have been added:
- `SPAWNFLIES <count>` – Spawns a random number of flies within a given bounding box.
- `REMOVEFLIES` – Removes all active flies.

The system also supports interaction with decaying meat: flies are attracted to the oldest meat slices, and particle effects trigger when they 'bite'. Rotten meat reduces score, with flies acting as a visual indicator of spoilage.



# Collision System Architecture Update

The collision system has been extended with a clean separation between broad-phase, narrow-phase, and resolution, keeping the system lightweight and arcade-focused:

- ICollider.WorldRect is always the axis-aligned bounding box (AABB) of the shape in world space. For RectCollider it is exact, for circles/polygons/capsules/rotated rects it is the enclosing AABB. This ensures spatial grid consistency while providers handle precise narrow-phase checks.

- IBody.BoundingBox is defined as the union of all attached collider AABBs. This is conservative for non-AABB shapes but guarantees correctness.

- Future shapes (circle, polygon, capsule) must follow the same contract: always provide a WorldRect as their AABB, recalculated when parameters change.

- WorldService only handles integration, broad-phase queries (via IWorldEntityResource), and dispatch to providers for narrow-phase. It does not resolve collisions directly.

- Providers encapsulate resolution using MTV calculation and policy-based resolution. They can veto collisions via OnAdditionalResolve (e.g., one-way gates).

- Colliders expose a Solve method that mirrors the WorldService provider resolution. This allows consistency even when using a fallback or alternative world implementation.

- Reflection/dynamic dispatch is avoided. Instead, a registry caches provider lookups for efficient CollideWith/Solve calls.

- Position and RectCollider.LocalRect changes mark bounds dirty only when meaningful (epsilon-based comparison). This reduces unnecessary grid churn from floating-point drift.

- IGridItem is simplified to expose only BoundingBox and Owner. IWorldEntityResource.Update replaces Move, with the spatial grid managing old/new replacements.

- Bodies extend IGridItem so they can live in the spatial grid. Colliders notify their body via MarkBoundsDirty when changes occur.

# Collision Module & Helpers

The CollisionModule provides a fluent, developer-friendly way to bootstrap the collision system. It should be registered with the **scene-level ObjectManager**, not a global/shared manager. This ensures proper scene lifetime, teardown, and avoids cross-scene leaks.

- Use `CollisionModule.Setup(sceneObjectManager)` to create the module builder.
- Call `LoadAllDefaultCollisionProviders()` unless you plan to register custom providers.
- Optionally set a custom resolution policy or spatial resource; otherwise a grid-backed resource is created.
- Finally call `Load()` to register a `WorldService` instance into the **scene** object manager.

## Recommended usage (scene-level ObjectManager)
```
CollisionModule
    .Setup(sceneObjectManager)                // <-- scene-level manager
    .LoadAllDefaultCollisionProviders()
    .SetGridSpatialGridSize(32)
    .Load();

// Retrieve the world from the scene manager
var world = sceneObjectManager.Get<WorldService>();

// Register a dynamic rect
var player = world.RegisterRectFBody(this, new RectF(100, 100, 16, 16));

// Register a static rect (e.g., a wall)
world.RegisterStaticRectFBody(this, new RectF(0, 0, 640, 32));
```
## Notes & gotchas

- **Scene scope**: Avoid registering the collision world in a global manager. Keep it scoped to the active scene's ObjectManager. This prevents systems from referencing stale bodies/colliders after scene transitions.

- **Providers**: The default Rect×Rect provider is loaded automatically unless you register custom providers. Custom providers can be added via `RegisterCollisionProvider<T1,T2>(...)` before `Load()`.

- **Spatial resource**: If you don't set a custom `IWorldEntityResource`, the module creates a `SpatialEntityGridResource` with the configured grid size. Choose a cell size matching your level's tile size (e.g., 32 or 64).

- **Events**: `RegisterRectFBody(owner, rect, registerCollisionEvents)` will auto-register the owner with the world's `ICollisionEvents` sinks when `owner` implements `ICollisionEvents` and the flag is true.