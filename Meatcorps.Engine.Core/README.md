# Meatcorps.Engine.Core

**Meatcorps.Engine.Core** is the foundational library of the Meatcorps Engine.  
It provides the core building blocks, contracts, and utilities used by higher-level engine modules (such as rendering, input backends, and hardware integration).

This package is **engine-agnostic**: it contains no rendering, windowing, or platform-specific code.

---

## What This Package Is

Meatcorps.Engine.Core contains:

- Core data types and math primitives
- Input abstraction and player routing
- Object and module management
- Timing, tweening, and animation helpers
- Resource, storage, and security contracts
- Grid and spatial utilities
- Low-level utilities used throughout the engine

It is designed to be:
- Deterministic
- Modular
- Testable
- Free of framework or engine dependencies

---

## What This Package Is *Not*

- No Raylib, SDL, or graphics code
- No UI or rendering logic
- No platform-specific APIs
- No opinionated game logic

Those live in separate packages.

---

## High-Level Architecture

The Core package focuses on **contracts and behavior**, not implementation details.

Typical dependency flow:

Game
* Engine Modules (Raylib, SDL, Hardware, etc.)
* Meatcorps.Engine.Core

Game code generally interacts with Core through:
- Interfaces
- Enums
- Routers and managers
- Stateless helpers

---

## Main Namespaces

### `Meatcorps.Engine.Core.GridSystem`
Spatial and grid-based utilities used for:
- Broad-phase spatial queries
- Entity placement
- Grid-driven gameplay systems

Includes:
- `GridAnalyzerYZ`
- `SingleEntityGrid`
- `SpatialEntityGrid`

---

### `Meatcorps.Engine.Core.Data`
Lightweight value types used across the engine:
- `Rect`, `RectF`
- `PointInt`
- `SizeF`
- `MarginF`, `PaddingF`
- `LineF`

These are allocation-free and framework-independent.

---

### `Meatcorps.Engine.Core.Enums`
Shared enums used across systems:
- `ConfigValueType`
- `PlayerInputType`
- `EaseType`

---

### `Meatcorps.Engine.Core.Extensions`
Extension methods for common types:
- `String`, `Integer`, `Float`
- `Enum`
- `Vector2`, `Matrix3x2`
- `PointInt`, `RectangleF`

---

### `Meatcorps.Engine.Core.Input`
A layered, decoupled input abstraction system supporting:
- Multiple hardware backends
- Player-specific routing
- Action mapping via enums
- Axis aggregation
- Input-driven animations and feedback

Key components:
- `IInput`, `BaseInput`
- Generic mappers
- `PlayerInputRouter<T>`
- `InputManager<T>`

This namespace defines **input semantics**, not hardware.

---

### `Meatcorps.Engine.Core.ObjectManager`
Central object and service registry used for:
- Dependency resolution
- Lifetime management
- Global service access

Designed to remain lightweight and explicit.

---

### `Meatcorps.Engine.Core.Modules`
Module contracts and lifecycle hooks used to:
- Register engine subsystems
- Control initialization and teardown order

---

### `Meatcorps.Engine.Core.Resource`
Resource-related contracts and helpers, including:
- Resource ownership rules
- Pooling utilities
- Async-safe patterns

---

### `Meatcorps.Engine.Core.Security`
Security and integrity-related abstractions, such as:
- Encryption contracts
- Secure resource handling

(Implementation is provided by higher-level modules.)

---

### `Meatcorps.Engine.Core.Server`
Minimal server and application loop utilities:
- `ServerApplication`
- `SimpleGameLoop`

Useful for headless or service-based scenarios.

---

### `Meatcorps.Engine.Core.Storage`
Abstractions for configuration and persistence:
- `IKeyValueDatabase`
- `IUniversalConfig`

Allows engine and games to remain storage-agnostic.

---

### `Meatcorps.Engine.Core.Tween`
Tweening and interpolation helpers used for:
- Animation
- Easing
- Smooth value transitions

Includes easing support via `EaseType`.

---

### `Meatcorps.Engine.Core.Utilities`

#### Math helpers
- `MathHelper`
- `PrimitivesHelper`

#### Resources and pools
- `ResourcePool`
- `ThreadSafeList`
- `RandomEnum`
- `ResetValue`

#### Sensors and timers
- `FixedTimer`
- `EdgeDetector`
- `PulseTimer`
- `TimerOn`
- `TimerOff`
- `SmoothValue`
- `FrameTimer`

#### Other helpers
- `Direction`
- `BufferedDirection`
- `UVHelper`
- `FileUtilities`

---

## Documentation

Full reference documentation is available in the GitHub Wiki:

https://github.com/meatcorps/Engine/wiki/Documentation

The wiki is intended as a **reference guide**, not a tutorial.

---

## Design Goals

- Clear separation of concerns
- No hidden global state
- Predictable behavior
- Long-term maintainability
- Suitable for arcade, desktop, and embedded environments

---

## License

MIT License  
See `LICENSE` for details.