using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Interfaces;
using Meatcorps.Engine.Arcade.RayLib.Services;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.Interfaces.Trackers;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Services;
using Meatcorps.Engine.MQTT.Modules;
using Meatcorps.Engine.RayLib.Abstractions;

namespace Meatcorps.Engine.Arcade.RayLib.Modules;

public class ArcadeEmulatorModule
{
    private readonly ArcadeGame _game;

    public static ArcadeEmulatorModule Load(ArcadeGame game, MQTTModule? mqttModule, string serverUrl = "http://localhost:8080/", bool standaloneMode = false)
    {
        var config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>() ?? new FallbackConfig();
        GlobalObjectManager.ObjectManager.Register(game);
        var fallback = new FallbackArcadeSystem
        {
            RemovePlayersAtIdle = !standaloneMode && config.GetOrDefault("Debug", "RemovePlayersAtIdle", true)
        };

        GlobalObjectManager.ObjectManager.Register(new ArcadeServer
        {
            Url = serverUrl
        });
        GlobalObjectManager.ObjectManager.Register<IArcadePointsMutator>(fallback);
        GlobalObjectManager.ObjectManager.Register<IPlayerCheckin>(fallback);
        GlobalObjectManager.ObjectManager.Add<IBackgroundService>(fallback);
        GlobalObjectManager.ObjectManager.Register(fallback);
        if (config.GetOrDefault("Debug", "ArcadeVisualDebugger", true)  && !standaloneMode) // 
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