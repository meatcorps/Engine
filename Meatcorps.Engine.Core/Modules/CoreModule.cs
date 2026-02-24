using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.Interfaces.Trackers;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Settings;
using Meatcorps.Engine.Core.Storage.Module;

namespace Meatcorps.Engine.Core.Modules;

/// <summary>
/// Bootstraps the core engine systems. Must be called once at startup before any other module except the logging module.
/// </summary>
public static class CoreModule
{
    /// <summary>Initializes engine settings, sets the working directory to the entry assembly location, loads storage services, and registers the IBackgroundService, ISceneSwitchTracker, and IDisposable lists in the GlobalObjectManager.</summary>
    public static void Load()
    {
        MeatcorpsEngineLibSettings.Init();
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        StorageModule.Load(GlobalObjectManager.ObjectManager);
        GlobalObjectManager.ObjectManager.RegisterList<IBackgroundService>();
        GlobalObjectManager.ObjectManager.RegisterList<ISceneSwitchTracker>();
        GlobalObjectManager.ObjectManager.RegisterList<IDisposable>();
    }
}