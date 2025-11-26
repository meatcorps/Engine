using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Modules;
using Meatcorps.Engine.Arcade.RayLib.Modules;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Hardware.ArduinoController.Modules;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.MQTT.Modules;
using Meatcorps.Engine.RayLib.Assets.Modules;
using Meatcorps.Engine.RayLib.Modules;
using Meatcorps.Engine.RayLib.PostProcessing.Extensions;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Game.HorrorJackpot;
using Meatcorps.Game.HorrorJackpot.Data;
using Meatcorps.Game.HorrorJackpot.GameEnums;
using Meatcorps.Game.HorrorJackpot.Input;
using Meatcorps.Game.HorrorJackpot.Resources;
using Meatcorps.Game.HorrorJackpot.Scenes;
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
        .Load(args[0])
        .SetupRouter(InputMapper.ArduinoInput());
}

var mqtt = MQTTModule.Load();
var game = new ArcadeGame
{
    MaxPlayers = 1,
    Name = "HorrorJackpot!",
    Code = settings.GetOrDefault("ArcadeGame", "Code", 4123),
    PricePoints = settings.GetOrDefault("ArcadeGame", "PricePoints", 100),
    Description = "Wheel of horror to throw away your life points!",
};

if (settings.GetOrDefault("ArcadeGame", "UseEmulator", false))
    ArcadeEmulatorModule.Load(game, mqtt).SetIntroScene<IntroScene>();
else
{
    ArcadeGameSystemModule.Load(game, mqtt).SetIntroScene<IntroScene>();
    mqtt.Create();
}


GameSession.Load();
Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

RaylibAssetsModule.Load(new AssetConfig());

using var _ = RayLibModule.Setup()
    .SetTitle("Meatcorps " + GlobalObjectManager.ObjectManager.Get<ArcadeGame>()!.Name)
    .SetInitialSize(720, 1280)
    .SetFixedSizeCamera(360, 640)
       .SetResource(GameSpriteFactory.Load())
    .SetResource(AudioEnumBinder.BindAllMusic(
        MusicResource<GameMusic>.Create().UsePlaceHoldersForMissingFiles().SetMasterVolume(0), "Assets/Music/"))
    .SetupProcessingBloom(0.6f, 0.2f, 0.8f, 4f)
    .SetProcessing(new CrtNewPixiePostProcessor())
    .SetResource(new OneTexture("Assets/CRTSidePanels.png", texture2D => 
        GlobalObjectManager.ObjectManager.Get<CrtNewPixiePostProcessor>()!.SetFrameTexture(texture2D)
    ))
    .SetResource(new ShaderManager<GameShaders>()
        .AddShader(
            Path.Combine("Assets", "Shaders", "Drum", "drum.vs"), 
            Path.Combine("Assets", "Shaders", "Drum", "drum.fx"), 
            GameShaders.Drum))
    .SetResource(new OneTexture(Path.Combine("Assets", "HorrorJackpot.png")), "HorrorJackpot")
    .SetResource(new OneTexture(Path.Combine("Assets", "HorrorJackpot_glow.png")), "HorrorJackpot_glow")
    .SetResource(AudioEnumBinder.BindAllSounds(
        SoundFxResource<GameSounds>.Create(6).UsePlaceHoldersForMissingFiles(), "Assets/SoundFX/"))
    .SetResource(new TextManager<GameFonts>()
        .AddFont("Assets/Fonts/PressStart2P-Regular.ttf", GameFonts.Default)
        .AddFont("Assets/Fonts/alarm clock.ttf", GameFonts.Digit, 64))
    .Load(new IntroScene())
    .Run();