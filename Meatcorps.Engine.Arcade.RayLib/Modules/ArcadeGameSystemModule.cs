using Meatcorps.Engine.Arcade.Constants;
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

public class ArcadeGameSystemModule
{
    private readonly ArcadeGame _game;

    public static ArcadeGameSystemModule Load(ArcadeGame game, MQTTModule mqttModule)
    {
        var config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>() ?? new FallbackConfig();
        GlobalObjectManager.ObjectManager.Register<ArcadeGame>(game);

        mqttModule.RegisterComplexObject(ArcadeEndpointTopics.CHANGE_POINTS, false, true, new ArcadePointChange());
        mqttModule.RegisterComplexObject(ArcadeEndpointTopics.REGISTER_GAME, false, true, game);
        mqttModule.RegisterComplexObject(ArcadeEndpointTopics.GAMESESSION_SIGNIN_AND_UPDATE, true, false, new ArcadePlayer());
        mqttModule.RegisterComplexObject(ArcadeEndpointTopics.GAMESESSION_SIGNOUT, false, true, new ArcadePlayer());
        
        var arcadeGameSystem = new ArcadeGameSystem();
        GlobalObjectManager.ObjectManager.Register<ArcadeServer>(new ArcadeServer
        {
            Url = config.GetOrDefault("ArcadeGame", "ServerUrl", "http://localhost:8080/")
        });
        GlobalObjectManager.ObjectManager.Register<IArcadePointsMutator>(arcadeGameSystem);
        GlobalObjectManager.ObjectManager.Register<IPlayerCheckin>(arcadeGameSystem);
        GlobalObjectManager.ObjectManager.Add<IBackgroundService>(arcadeGameSystem);
        /*if (config.GetOrDefault("Debug", "ArcadeVisualDebugger", false))
        {
            GlobalObjectManager.ObjectManager.Add<IBackgroundService>(new ArcadeVisualDebugger());
        }*/
        return new ArcadeGameSystemModule(game);
    }

    private ArcadeGameSystemModule(ArcadeGame game)
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