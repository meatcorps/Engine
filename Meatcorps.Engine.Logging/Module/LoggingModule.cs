using System.Reflection;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Serilog;

namespace Meatcorps.Engine.Logging.Module;

public static class LoggingModule
{
    public static void Load()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.WithProperty("SourceContext", "")
            .MinimumLevel.Debug()
            .WriteTo.Console(
                outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level}] ({SourceContext}): {Message}{NewLine}{Exception}")
            .WriteTo.File(
                path: FileUtilities.GetFullPath(Path.Combine("logs", Assembly.GetEntryAssembly()!.GetName().Name + "-.log")),
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 10 * 1024 * 1024,
                retainedFileCountLimit: 7,
                rollOnFileSizeLimit: true,
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] ({SourceContext}): {Message}{NewLine}{Exception}")
            .CreateLogger();
        
        GlobalObjectManager.ObjectManager.Register<ILoggerFactory>(LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Debug)
                .AddSerilog(Log.Logger, dispose: true)
                .AddConsole();
        }));
    }
}