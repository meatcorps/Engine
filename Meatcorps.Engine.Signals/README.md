# Meatcorps.Engine.Signals

Typed signal values that automatically broadcast changes to all other instances sharing the same
group and topic. Works in-process out of the box, and can be extended with remote backends
(e.g. MQTT) by registering a custom tracker.

## Backends

| Backend | Package | Description |
|---|---|---|
| In-memory | `Meatcorps.Engine.Signals` (this package) | Syncs within the same process. No external dependencies. |
| MQTT | `Meatcorps.Engine.MQTT` | Syncs across processes/machines over MQTT. |

## In-memory setup

Call `SignalModule.Load()` once at startup. This registers an `InternalSignalValueEvent` tracker
for the built-in `SignalDefault.Internal` group:

```csharp
SignalModule.Load();
```

Then create signal values using `SignalDefault.Internal` as the group:

```csharp
var score = new SignalValue<int, SignalDefault>(SignalDefault.Internal, "game/score", initialValue: 0);
```

All `SignalValue` instances with the same group and topic will stay in sync within the process.

## Custom group (for multiple trackers)

Define your own group enum when you need to separate concerns or use a different backend per group:

```csharp
public enum MyGroup { Local, Remote }
```

Register a tracker for each group you use:

```csharp
var tracker = new InternalSignalValueEvent<MyGroup>(MyGroup.Local);
GlobalObjectManager.ObjectManager.RegisterSet<ISignalValueEvent<MyGroup>>();
GlobalObjectManager.ObjectManager.Add<ISignalValueEvent<MyGroup>>(tracker);
```

## Basic usage

```csharp
// Read / write
score.Value = 100;         // broadcasts to all matching signal values
int current = score.Value;

// Subscribe to changes (local sets and incoming remote updates)
score.ValueChanged += value => Console.WriteLine($"Score: {value}");

// Subscribe only to values arriving from a remote tracker
score.IncomingValue += value => Console.WriteLine($"Incoming: {value}");

// Push the current value without changing it (useful for initial sync)
score.Push();
```

## Custom tracker backend

You can implement your own backend (e.g. WebSocket, Redis, file) by extending
`BaseSignalValueEvent<TGroup>`. The only requirement is implementing `GetGroup()`:

```csharp
public class MyCustomTracker : BaseSignalValueEvent<MyGroup>
{
    private readonly MyGroup _group;

    public MyCustomTracker(MyGroup group)
    {
        _group = group;
    }

    public override MyGroup GetGroup() => _group;

    // Optional: override OnDispose(bool disposing) to clean up custom resources
}
```

`BaseSignalValueEvent<TGroup>` handles registration, deduplication, Rx subject routing,
and broadcasting to all matching signal values. Your implementation only needs to add
transport-specific logic (e.g. publishing to a remote broker) by calling into
`GetSubject<TValueType>(topic)` or `SetValue<TValueType>(topic, value)`.

Register it the same way as the built-in tracker:

```csharp
var tracker = new MyCustomTracker(MyGroup.Remote);
GlobalObjectManager.ObjectManager.RegisterSet<ISignalValueEvent<MyGroup>>();
GlobalObjectManager.ObjectManager.Add<ISignalValueEvent<MyGroup>>(tracker);
```

## Disposal

`SignalValue` implements `IDisposable`. Always dispose when the owning object is destroyed
to unregister from the tracker and stop receiving updates:

```csharp
score.Dispose();
```

## Notes

- If no tracker is registered for the given group and no `initialValue` is provided, the constructor
  throws `ArgumentNullException`. This is a fail-fast guard — a tracker must exist before signals
  of that group can be created.
- Value changes are skipped if the new value equals the current value (`DistinctUntilChanged`).
- Signal identity is based on `(Group, Topic)` — two `SignalValue` instances with the same group
  and topic share state through the tracker.

## License

MIT License
See `LICENSE` for details.
