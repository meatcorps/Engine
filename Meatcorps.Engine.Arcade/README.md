# Meatcorps.Engine.Arcade

Multiplayer session and point management for arcade cabinet games.
Handles player check-in/out, point deduction and submission, and game state broadcasting
via MQTT to a central arcade server.

## Concepts

| Type | Description |
|---|---|
| `ArcadeGame` | Metadata for your game (code, name, price, max players) |
| `ArcadeGameSystem` | Live session manager — tracks checked-in players, manages points over MQTT |
| `FallbackArcadeSystem` | Local dev replacement — simulates players without a real arcade server |
| `IArcadePointsMutator` | Interface for reading and mutating player points |
| `IPlayerCheckin` | Interface for querying player session state |

## Setup

Requires `Meatcorps.Engine.MQTT` to be loaded first. Define your game metadata and load the module:

```csharp
var mqttModule = MQTTModule.Load();

var game = new ArcadeGame
{
    Code = 1,
    Name = "My Game",
    Description = "A short description",
    PricePoints = 100,
    MaxPlayers = 2,
};

GlobalObjectManager.ObjectManager.Register(game);
ArcadeGameSystemModule.Load(game, mqttModule);
```

`ArcadeGameSystemModule.Load` registers `IArcadePointsMutator`, `IPlayerCheckin`, and the
`ArcadeGameSystem` background service into the global `ObjectManager`.

## Resolving in a scene

```csharp
var points   = SceneObjectManager.Get<IArcadePointsMutator>()!;
var checkin  = SceneObjectManager.Get<IPlayerCheckin>()!;
```

## Working with points

```csharp
// Deduct the game's entry price from player 1 (returns false if insufficient points)
if (points.RequestPoints(player: 1))
{
    // player paid — start the round
}

// Deduct a custom amount
if (points.RequestPoints(player: 1, points: 50))
{
    // deducted 50 points
}

// Award points to player 1
points.SubmitPoints(player: 1, points: 200);

// Read current balance
int balance = points.GetPoints(player: 1);
```

## Checking player session state

```csharp
if (checkin.IsPlayerCheckedIn(player: 1, out string name))
{
    Console.WriteLine($"Player 1 is {name}");
}

string displayName = checkin.GetPlayerName(player: 1);
```

## Local development (no arcade server)

Use `FallbackArcadeSystem` instead of `ArcadeGameSystemModule` during local development.
It simulates players with a configurable starting balance, no MQTT required:

```csharp
GlobalObjectManager.ObjectManager.Register(game);

var fallback = new FallbackArcadeSystem(maxPlayers: 2, startingPoints: 3000);
GlobalObjectManager.ObjectManager.Register<IArcadePointsMutator>(fallback);
GlobalObjectManager.ObjectManager.Register<IPlayerCheckin>(fallback);
GlobalObjectManager.ObjectManager.Add<IBackgroundService>(fallback);
```

## Notes

- Player indices are **1-based** (`player: 1`, `player: 2`, etc.).
- `ArcadeGameSystem` automatically kicks excess players if `MaxPlayers` is exceeded on check-in.
- The server URL defaults to `http://localhost:8080/` and can be overridden via config key
  `ArcadeGame > ServerUrl`.

## License

MIT License
See `LICENSE` for details.