# Meatcorps.Engine.Session

Typed, serializable session management for games built on the Meatcorps Engine. Handles player tracking, scoped data bags, and serialization — from simple primitives to complex objects.

---

## Concepts

### SessionDataBag

The core container. It holds typed values keyed by an enum and knows how to serialize/deserialize itself to a flat `Dictionary<string, string>`.

There are two scopes of data in a session:

- **Global session data** — values that live for the entire session (current level, seed, score totals). Registered via your session enum.
- **Isolated scope data** — values tied to a transient context like a level or mission. Registered via `IValueType` so you don't need a fixed enum for dynamic data.

### SessionSet

Groups a session-level `SessionDataBag` with per-player `SessionDataBag`s. It enforces a max player count and automatically injects built-in values like `SessionSeed`, `SessionStarted`, `PlayerId`, and `PlayerName`.

### SessionFactory

Builder for constructing the session schema. Defines what data exists in a session and per player, sets the max player count, and registers trackers.

### ISessionTracker

Observer interface. Implement it to react to session lifecycle events — session started/ended, player joined/left.

---

## Getting Started

### 1. Define your data keys

```csharp
public enum MySessionData { CurrentLevel, TotalScore }
public enum MyPlayerData { Score, Lives }
```

### 2. Register the session module

```csharp
SessionModule.Create(
    new SessionFactory<MySessionData, MyPlayerData>()
        .SetMaxPlayers(2)
        .SetSessionDataFactory(() => new SessionDataBag<MySessionData>()
            .RegisterItemByValue(MySessionData.CurrentLevel, 1)
            .RegisterItemByValue(MySessionData.TotalScore, 0)
        )
        .SetPlayerSessionDataFactory(() => new SessionDataBag<MyPlayerData>()
            .RegisterItemByValue(MyPlayerData.Score, 0)
            .RegisterItemByValue(MyPlayerData.Lives, 3)
        )
);
```

### 3. Use the session

```csharp
var session = sessionService.CurrentSession;

// Read
int level = session.SessionData.Get<int>(MySessionData.CurrentLevel);

// Write
session.SessionData.Set(MySessionData.CurrentLevel, 2);

// Convenience helpers
session.PlayerData[0].Inc(MyPlayerData.Score, 100);
session.PlayerData[0].ClampInt(MyPlayerData.Lives, 0, 5);
```

---

## Complex Objects

For class types, use `RegisterComplexItem`. It uses Newtonsoft.Json internally and handles its own serialization — no external serializer registration needed.

```csharp
public class Inventory { public List<string> Items { get; set; } = new(); }

new SessionDataBag<MyPlayerData>()
    .RegisterComplexItem(MyPlayerData.Inventory, new Inventory())
```

Optionally pass a `JsonSerializerSettings` for custom converters or contract resolvers:

```csharp
    .RegisterComplexItem(MyPlayerData.Inventory, new Inventory(), new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore
    })
```

---

## Built-in Types

The following types are supported out of the box without registering a serializer:

| Type | Notes |
|------|-------|
| `int` | InvariantCulture |
| `float` | InvariantCulture |
| `string` | Direct |
| Any `class` | Via `RegisterComplexItem` (Newtonsoft.Json) |

For other primitive types, implement `ISessionDataTypeSerializer` and call `RegisterSerializer`.

---

## Custom Serializers

```csharp
public class BoolSerializer : ISessionDataTypeSerializer
{
    public Type Type => typeof(bool);

    public string Serialize(ISessionDataItem data)
    {
        if (data is not ISessionDataValue<bool> v) throw new Exception("Invalid type");
        return v.Value ? "1" : "0";
    }

    public void Deserialize(string value, ISessionDataItem data)
    {
        if (data is not ISessionDataValue<bool> v) throw new Exception("Invalid type");
        v.Value = value == "1";
    }
}

// Register on the bag:
bag.RegisterSerializer(new BoolSerializer());
bag.RegisterItemByValue(MyPlayerData.IsAlive, true);
```

---

## Serialization

Every `SessionDataBag` can be serialized to and from a flat string dictionary, suitable for saving to disk, sending over a network, or encoding as a token.

```csharp
// Serialize
IReadOnlyDictionary<string, string> data = bag.Serialize();

// Deserialize
bag.Deserialize(data);

// Pack session + all player bags into a Base64 token
string token = SessionUtil.PackToToken(
    session.SessionData.Serialize(),
    session.PlayerData[0].Serialize()
);
```

Keys use the format `Full.Namespace.EnumName:ItemName`, so bags from different enum types never collide.

---

## Lifecycle Events

```csharp
public class MyTracker : ISessionTracker<MySessionData, MyPlayerData>
{
    public void SessionStarted(SessionSet<MySessionData, MyPlayerData> session) { }
    public void SessionEnded(SessionSet<MySessionData, MyPlayerData> session) { }
    public void PlayerJoined(SessionDataBag<MySessionData> session, SessionDataBag<MyPlayerData> player, int totalPlayers) { }
    public void PlayerLeft(SessionDataBag<MySessionData> session, SessionDataBag<MyPlayerData> player) { }
}
```

Register via the factory:

```csharp
factory.RegisterTracker(new MyTracker());
```

A built-in `SessionDebugger` is available that logs all events to the console — useful during development.

---

## License

MIT License — see `LICENSE` for details.