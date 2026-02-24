# Meatcorps.Engine.Logging

Configures [Serilog](https://serilog.net/) for the Meatcorps engine and registers an
`ILoggerFactory` in the global `ObjectManager` so any engine module can resolve a logger.

Outputs to both the console and a daily rolling log file.

## Setup

Call `LoggingModule.Load()` always as first, before loading other modules:

```csharp
LoggingModule.Load();

var host = new GameHostBuilder() // Just an example
    // ...
    .Build();
```

That's it. The module configures Serilog internally and registers an `ILoggerFactory` in
`GlobalObjectManager`.

## Log output

**Console** format:
```
12:34:56.789 [Debug] (MyNamespace.MyClass): Message
```

**File** — written to `logs/<AssemblyName>-<date>.log` next to the executable:
- Rolling interval: daily
- Max file size: 10 MB (rolls on size limit)
- Retained files: 7 days

## Resolving a logger in engine code

Any class that has access to `GlobalObjectManager` can resolve a typed logger:

```csharp
var loggerFactory = GlobalObjectManager.ObjectManager.Get<ILoggerFactory>();
var logger = loggerFactory!.CreateLogger<MyClass>();

logger.LogInformation("Game started");
logger.LogWarning("Low memory");
logger.LogError(ex, "Unhandled exception");
```

## Notes

- Minimum log level is `Debug`.
- `LoggingModule.Load()` should only be called once. Calling it multiple times will overwrite
  the previous Serilog configuration.
- The log file path is resolved relative to the entry assembly location via `FileUtilities.GetFullPath`.

## License

MIT License
See `LICENSE` for details.