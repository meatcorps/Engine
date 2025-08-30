using Microsoft.Extensions.Logging;

namespace Meatcorps.Engine.Core.ObjectManager;

public static class LoggingService
{
    public static ILogger<T> GetLogger<T>()
    {
        var factory = GlobalObjectManager.ObjectManager.Get<ILoggerFactory>();
        return factory!.CreateLogger<T>();
    }

    public static ILogger GetLogger(string categoryName)
    {
        var factory = GlobalObjectManager.ObjectManager.Get<ILoggerFactory>();
        return factory!.CreateLogger(categoryName);
    }
}