using Microsoft.Extensions.Logging;

namespace Meatcorps.Engine.Core.ObjectManager;

/// <summary>
/// Convenience helpers for resolving typed loggers from the GlobalObjectManager. Requires an ILoggerFactory to be registered (e.g. via LoggingModule.Load()).
/// </summary>
public static class LoggingService
{
    /// <summary>Resolves a typed ILogger&lt;T&gt; from the global ILoggerFactory.</summary>
    public static ILogger<T> GetLogger<T>()
    {
        var factory = GlobalObjectManager.ObjectManager.Get<ILoggerFactory>();
        return factory!.CreateLogger<T>();
    }

    /// <summary>Resolves an ILogger with the given category name from the global ILoggerFactory.</summary>
    public static ILogger GetLogger(string categoryName)
    {
        var factory = GlobalObjectManager.ObjectManager.Get<ILoggerFactory>();
        return factory!.CreateLogger(categoryName);
    }
}