using Meatcorps.Engine.Core.ObjectManager;
using Microsoft.Extensions.Logging;

namespace Meatcorps.Engine.Logging.Module;

public static class ConsoleLoggingModule
{
    public static void Load()
    {
        GlobalObjectManager.ObjectManager.Register<ILoggerFactory>(LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddConsole(); // Or custom provider
        }));
    }
}