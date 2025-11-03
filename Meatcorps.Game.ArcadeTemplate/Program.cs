using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Modules;
using Meatcorps.Engine.Arcade.RayLib.Modules;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.ArduinoController.Modules;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.MQTT.Modules;
using Meatcorps.Engine.RayLib.Modules;
using Meatcorps.Engine.RayLib.PostProcessing.Extensions;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.ArcadeTemplate.Data;
using Meatcorps.Game.ArcadeTemplate.GameEnums;
using Meatcorps.Game.ArcadeTemplate.Input;
using Meatcorps.Game.ArcadeTemplate.Resources;
using Meatcorps.Game.ArcadeTemplate.Scenes;
using Raylib_cs;

LoggingModule.Load();
CoreModule.Load();
HighScoreModule.Load();

var settings = GameConfig<GameSettings>.Create();

if (args.Length == 0)
{
    Console.WriteLine("No serial port device specified. Using fallback keyboard input.");
    GameFallbackInput.Load();
}
else
{
    ArduinoControllerModule.Setup()
        .EnableJoystick()
        .EnableButtons(1)
        .EnablePlayer2()
        .Load(args[0])
        .SetupRouter(InputMapper.ArduinoInput());
}

var mqtt = MQTTModule.Load();
var game = new ArcadeGame
{
    MaxPlayers = 1,
    Name = "TEMPLATE!",
    Code = settings.GetOrDefault("ArcadeGame", "Code", 0000),
    PricePoints = settings.GetOrDefault("ArcadeGame", "PricePoints", 1000),
    Description = "The most gore version of the game ever made.",
};

if (settings.GetOrDefault("ArcadeGame", "UseEmulator", false))
    ArcadeEmulatorModule.Load(game, mqtt).SetIntroScene<IntroScene>();
else
    ArcadeGameSystemModule.Load(game, mqtt).SetIntroScene<IntroScene>();

mqtt.Create();

GameSession.Load();
Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

using var _ = RayLibModule.Setup()
    .SetTitle("Meatcorps " + GlobalObjectManager.ObjectManager.Get<ArcadeGame>()!.Name)
    .SetInitialSize(1920, 1080)
    .SetFixedSizeCamera(640, 360)
    .SetupProcessingBloom(0.6f, 0.2f, 0.8f, 4f)
    .SetProcessing(new CrtNewPixiePostProcessor())
    .SetResource(new OneTexture("Assets/CRTSidePanels.png", texture2D => 
        GlobalObjectManager.ObjectManager.Get<CrtNewPixiePostProcessor>()!.SetFrameTexture(texture2D)
        ))
    .SetResource(GameSpriteFactory.Load())
    .SetResource(AudioEnumBinder.BindAllMusic(
            MusicResource<GameMusic>.Create().UsePlaceHoldersForMissingFiles(),"Assets/Music/")) 
    .SetResource(AudioEnumBinder.BindAllSounds(
        SoundFxResource<GameSounds>.Create(6).UsePlaceHoldersForMissingFiles(), "Assets/SoundFX/"))
    .SetResource(TextManager.OnlyOneFont("Assets/Fonts/PressStart2P-Regular.ttf"))
    .Load(new IntroScene())
    .Run();