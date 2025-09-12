using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Modules;
using Meatcorps.Engine.Arcade.RayLib.Modules;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.ArduinoController.Modules;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.RayLib.Modules;
using Meatcorps.Engine.RayLib.PostProcessing.Extensions;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.Arcade.Leaderboard.Data;
using Meatcorps.Engine.Arcade.Leaderboard.GameEnums;
using Meatcorps.Engine.Arcade.Leaderboard.Input;
using Meatcorps.Engine.Arcade.Leaderboard.Resources;
using Meatcorps.Engine.Arcade.Leaderboard.Scenes;
using Meatcorps.Engine.Arcade.Services;
using Meatcorps.Engine.MQTT.Modules;
using Raylib_cs;

ConsoleLoggingModule.Load();
CoreModule.Load();

GameConfig<GameSettings>.Create();

var mqttModule = MQTTModule.Load();
ArcadeRegisterEndpointModule.Load(mqttModule);
mqttModule.Create();

GlobalObjectManager.ObjectManager.Register(new ArcadeDataService());

Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

using var _ = RayLibModule.Setup()
    .SetTitle("Meatcorps Leaderboard")
    .SetInitialSize(1920, 1080)
    .SetFixedSizeCamera(640, 360)
    .SetupProcessingBloom(0.6f, 0.2f, 0.8f, 4f)
    .SetProcessing(new CrtNewPixiePostProcessor())
    .SetResource(new OneTexture("Assets/CRTSidePanels.png", texture2D =>
        GlobalObjectManager.ObjectManager.Get<CrtNewPixiePostProcessor>()!.SetFrameTexture(texture2D)
    ))
    .SetResource(GameSpriteFactory.Load())
    .SetResource(TextManager.OnlyOneFont("Assets/Fonts/PressStart2P-Regular.ttf"))
    .Load(new MainScene())
    .Run();