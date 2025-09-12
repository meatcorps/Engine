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
using Meatcorps.Game.Snake.Data;
using Meatcorps.Game.Snake.Input;
using Meatcorps.Game.Snake.Resources;
using Meatcorps.Game.Snake.Scenes;
using Raylib_cs;

ConsoleLoggingModule.Load();
CoreModule.Load();
HighScoreModule.Load();

var settings = GameConfig<SnakeSettings>.Create();

if (args.Length == 0)
{
    Console.WriteLine("No serial port device specified. Using fallback keyboard input.");
    SnakeFallbackInput.Load();
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
ArcadeGameSystemModule.Load(new ArcadeGame
{
    MaxPlayers = 2,
    Name = "SNAKE!",
    Code = settings.GetOrDefault("ArcadeGame", "Code", 1234),
    PricePoints = settings.GetOrDefault("ArcadeGame", "PricePoints", 1000),
    Description = "The most gore version of the game snake ever made.",
}, mqtt).SetIntroScene<IntroScene>();
mqtt.Create();

SnakeSession.Load();
Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

using var _ = RayLibModule.Setup()
    .SetTitle("Meatcorps Snake!")
    .SetInitialSize(1920, 1080)
    .SetFixedSizeCamera(640, 360)
    .SetupProcessingBloom(0.6f, 0.2f, 0.8f, 4f)
    .SetProcessing(new CrtNewPixiePostProcessor())
    .SetResource(new OneTexture("Assets/CRTSidePanels.png", texture2D => 
        GlobalObjectManager.ObjectManager.Get<CrtNewPixiePostProcessor>()!.SetFrameTexture(texture2D)
        ))
    .SetResource(SnakeSpriteFactory.Load())
    .SetResource(AudioEnumBinder.BindAllMusic(
            MusicResource<SnakeMusic>
                .Create()
                .UsePlaceHoldersForMissingFiles()
                .SetMasterVolume(1),"Assets/Music/")) 
    .SetResource(AudioEnumBinder.BindAllSounds(
            SoundFxResource<SnakeSounds>
                .Create(6)
                .UsePlaceHoldersForMissingFiles()
                .SetMasterVolume(1), "Assets/SoundFX/"))
    .SetResource(TextManager.OnlyOneFont("Assets/Fonts/PressStart2P-Regular.ttf"))
    .Load(new IntroScene())
    .Run();