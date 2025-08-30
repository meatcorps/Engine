using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Arcade.RayLib.Services;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.Interfaces.Trackers;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Services;
using Meatcorps.Engine.RayLib.Abstractions;

namespace Meatcorps.Engine.Arcade.RayLib.Modules;

public class ArcadeEmulatorModule
{
    private readonly ArcadeGame _game;

    public static ArcadeEmulatorModule Load(ArcadeGame game, string serverUrl = "http://localhost:8080/")
    {
        var config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>() ?? new FallbackConfig();
        GlobalObjectManager.ObjectManager.Register<ArcadeGame>(game);
        var fallback = new FallbackArcadeSystem();
        fallback.RemovePlayersAtIdle = config.GetOrDefault("Debug", "RemovePlayersAtIdle", true);
        GlobalObjectManager.ObjectManager.Register<ArcadeServer>(new ArcadeServer
        {
            Url = serverUrl
        });
        GlobalObjectManager.ObjectManager.Register<IArcadePointsMutator>(fallback);
        GlobalObjectManager.ObjectManager.Register<IPlayerCheckin>(fallback);
        GlobalObjectManager.ObjectManager.Add<IBackgroundService>(fallback);
        if (config.GetOrDefault("Debug", "ArcadeVisualDebugger", true))
        {
            GlobalObjectManager.ObjectManager.Add<IBackgroundService>(new ArcadeVisualDebugger());
        }
        return new ArcadeEmulatorModule(game);
    }

    private ArcadeEmulatorModule(ArcadeGame game)
    {
        _game = game;
    }

    public void SetIntroScene<T>() where T : BaseScene
    {
        var tracker = new ArcadeGameStateTracker(_game);
        tracker.SetIntroScene<T>();
        GlobalObjectManager.ObjectManager.Add<ISceneSwitchTracker>(tracker);
    }
    
}