using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.Interfaces.Trackers;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Settings;
using Meatcorps.Engine.Core.Storage.Module;

namespace Meatcorps.Engine.Core.Modules;

public static class CoreModule
{
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