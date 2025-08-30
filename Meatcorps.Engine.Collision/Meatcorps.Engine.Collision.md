# Meatcorps.Engine.Collision — Module Readme

## What this module is
A small, fast **collision framework** for arcade games.  
World orchestrates; **providers** do the geometry + resolution; the **spatial resource** (your external grid/quadtree) owns **broad‑phase**. Sensors, layers, kinematic bodies, and deterministic ordering are built in.

---

## Responsibilities (SRP)

- **WorldService** (orchestration only)
  - Integrates bodies (velocity, damping, gravity, movement‑angle snapping).
  - Asks `IWorldEntityResource` for **broad‑phase** candidates.
  - Filters by layer/mask.
  - Runs **providers** for **narrow‑phase** + resolution.
  - Emits `ICollisionEvents` (Enter/Stay/Exit) to 1..N sinks.
  - Tracks contact pairs per frame for phase diffs.

- **Providers** (geometry + resolution)
  - Narrow‑phase test and **manifold** creation.
  - Apply MTV based on an **`IResolutionPolicy`** (pluggable).
  - **May veto** resolution (e.g., one‑way gate) even if overlapping.

- **IWorldEntityResource** (broad‑phase only)
  - `Add/Remove/Move` bodies.
  - `Query(RectF)` / `Query(Vector2)` returns **candidate bodies**.

- **Bodies & Colliders**
  - `IBody` = kinematic data (position, velocity, mass…) + list of colliders.
  - `ICollider` = shape + flags (`IsSensor`, `Layer`, `CollisionMask`, `Enabled`).
  - Rect‑only phase: `RectCollider` with `LocalRect` and computed `WorldRect`.

---

## Directory layout (quick map)

```
Meatcorps.Engine.Collision
├─ Abstractions
│  ├─ BaseCollideProvider.cs
│  └─ BaseRectRectProvider.cs         // default Rect×Rect narrow + resolve, veto hook
├─ Colliders
│  └─ RectCollider.cs
├─ Data
│  ├─ ColliderSet.cs                  // unordered type-pair key
│  ├─ ContactManifold.cs              // Normal, Penetration, OverlapArea
│  ├─ ContactPair.cs
│  └─ ICollisionEvents.cs
├─ Enums
│  ├─ BodyType.cs
│  └─ ContactPhase.cs
├─ Extensions
│  └─ IColliderExtensions.cs          // optional helpers
├─ Interfaces
│  ├─ IBody.cs, ICollider.cs
│  ├─ IColliderProvider.cs
│  ├─ IInternalBody.cs                // internal setter for StableIndex
│  ├─ IResolutionPolicy.cs
│  ├─ IWorldEntityResource.cs
│  └─ IWorldService.cs
├─ Providers
│  ├─ Bodies/Body.cs                  // basic IBody impl
│  ├─ Colliders
│  │  ├─ OneWayGateProvider.cs        // veto example
│  │  └─ RectRectProvider.cs          // concrete Rect×Rect using base logic
│  └─ DefaultResolutionPolicy.cs
├─ Services
│  ├─ ColliderProviderRegistry.cs     // type-pair -> provider
│  └─ WorldService.cs
└─ Utilities
   └─ LayerBits.cs                    // enum -> mask sugar
```

---

## Lifecycle (per frame)

1) **Integrate** dynamic/kinematic bodies  
   - apply gravity (`GravityScale`), limit speed (`MaxSpeed`), damping (`LinearDamping`), snap velocity direction (`MovementConstraintAngle` = 0 free, 45 for 8‑way, 90 for 4‑way).  
   - `IWorldEntityResource.Move(body, body.Position)`

2) **Candidates**  
   - `candidates = worldResource.Query(body.BoundingBox)` (AABB union of colliders).

3) **Filter**  
   - Skip self, disabled, Static–Static.  
   - Layer & mask: `(a.Layer & b.CollisionMask) != 0` and vice versa.

4) **Sensors vs Contacts**  
   - If `a.IsSensor || b.IsSensor` → mark as **trigger pair** (no resolution).  
   - Else → **provider dispatch** via `ColliderSet` (unordered type pair).
     - `provider.CollideWith(a, b, out manifold)`  
     - If hit → `provider.Solve(a, b, manifold, policy)`.  
       - Provider may **veto** (returns false) for one‑way gates, etc.

5) **Events**  
   - World diffs current pairs vs previous:  
     - Enter = new this frame  
     - Stay = in both  
     - Exit = missing this frame  
   - Triggers call `ICollisionEvents.OnTrigger(phase, pair)`  
   - Contacts call `ICollisionEvents.OnContact(phase, pair, manifold?)`

Determinism: bodies processed by `StableIndex` (assigned on registration). `ContactPair` stores ordered `(A,B)` by stable ordering.

---

## How to register (fluent, factory‑style)

```csharp
// Build providers
var policy = new DefaultResolutionPolicy();
var providers = new ColliderProviderRegistry()
    .Register(new RectRectProvider(policy))
    .Register(new OneWayGateProvider(policy, allowedNormal: new Vector2(1,0), degreesTolerance: 60));

// Build world
var world = new WorldService(spatial, providers)
    .SetGravity(Vector2.Zero)
    .SetResolutionPolicy(policy)
    .AddCollisionEvents(globalSink1)
    .AddCollisionEvents(globalSink2);

// Create a body + collider
var body = new Body(world)
    .SetType(BodyType.Dynamic)
    .SetMass(1)
    .SetMaxSpeed(180)
    .SetMovementConstraintAngle(90); // 4-way

var col = new RectCollider(body, new RectF(-8,-8,16,16))
    .SetLayer(LayerBits.Bit(CollisionLayer.Player))
    .SetMask(LayerBits.MaskOf(CollisionLayer.Walls, CollisionLayer.Pellets, CollisionLayer.Ghost));

body.AddCollider(col);
world.RegisterBody(body);
```

---

## Providers (default + extensibility)

**BaseRectRectProvider** (default) centralizes Rect×Rect logic:
- Uses `RectF.Intersects(ref,ref)` and `RectF.Intersection(ref,ref)`.
- Builds MTV on shallow axis.
- Applies MTV according to `IResolutionPolicy` (`DefaultResolutionPolicy` rules: push dynamic vs static; split by mass for dynamic vs dynamic).
- **Hook** `OnAdditionalResolve(a,b,manifold)` → return `false` to **veto** (one‑way gate, portals, etc.).
- **Hook** `OnResolved(...)` for side effects.

**OneWayGateProvider** example:
- Overrides `OnAdditionalResolve` to allow contact only if the mover’s velocity direction matches an allowed normal within a tolerance.

---

## Layers & masks (enum‑friendly)

- Each collider has `Layer : uint`, `CollisionMask : uint`.  
- Use `LayerBits` to set them from enums:

```csharp
public enum CollisionLayer { Walls=0, Pellets=1, Player=2, Ghost=3, Gate=4, Sensor=5 }

var playerLayer = LayerBits.Bit(CollisionLayer.Player);
var playerMask  = LayerBits.MaskOf(CollisionLayer.Walls, CollisionLayer.Pellets, CollisionLayer.Ghost, CollisionLayer.Gate);

collider.SetLayer(playerLayer).SetMask(playerMask);
```

---

## Sensors

- `ICollider.IsSensor == true` → **no resolution**, only **triggers** (Enter/Stay/Exit).
  - Pellets, energizers, tunnel triggers, doors as sensors, etc.

---

## Performance & determinism

- **Broad‑phase** is external (`IWorldEntityResource`) → use a uniform grid for tile games; swap later if needed without touching World.  
- **No allocations** in the hot path: reuse lists/sets where possible.  
- **StableIndex** + ordered `ContactPair` ensures deterministic iteration and hashing.

---

## Extension points

- New shapes: add new collider type + provider (world does not change).  
- New policies: implement `IResolutionPolicy` (e.g., different mass rules).  
- Custom events: add another `ICollisionEvents` sink.  
- Conditional behaviors: override provider’s `OnAdditionalResolve`.

---

## Testing tips

- Unit test **providers** in isolation: feed rectangles, assert MTV and veto rules.  
- Unit test **world** with a simple fake `IWorldEntityResource` that returns controlled candidates.  
- Snapshot tests for **Enter/Stay/Exit** sequences on scripted scenarios.

---

## Future roadmap (when needed)

- Per‑collider local rotation (OBB) → SAT in providers; world unchanged.  
- Raycasts/segment casts as optional queries in `IWorldService`.  
- Sleep heuristics (optional) for large scenes.

---

## Gotchas to avoid

- Don’t put resolution back into `WorldService` — keep SRP.  
- Don’t include `IsSensor` in `ColliderSet` keys — provider math doesn’t change; only behavior does.  
- Ensure type‑ids in `ColliderProviderRegistry` are **stable** during the run (use a registry counter, not `GetHashCode()`).

---

That’s the snapshot. If you want, I can generate a tiny **usage sample** folder (Program + FakeSpatial + ConsoleEvents) that runs two bodies hitting a wall, a pellet trigger, and a one‑way gate, so you’ve got a runnable smoke test for the module.
