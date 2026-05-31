using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Arcade.Modules;
using Meatcorps.Engine.Arcade.RayLib.Modules;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Settings;
using Meatcorps.Engine.Hardware.ArduinoController.Modules;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.MQTT.Modules;
using Meatcorps.Engine.RayLib.Assets.Modules;
using Meatcorps.Engine.RayLib.Modules;
using Meatcorps.Engine.RayLib.PostProcessing;
using Meatcorps.Engine.RayLib.PostProcessing.Extensions;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.RayLib.UI.GuiComponent.GuiSettings;
using Meatcorps.Game.CyberMaze;
using Meatcorps.Game.CyberMaze.Data;
using Meatcorps.Game.CyberMaze.GameEnums;
using Meatcorps.Game.CyberMaze.Input;
using Meatcorps.Game.CyberMaze.Resources;
using Meatcorps.Game.CyberMaze.Scenes;
using Raylib_cs;

LoggingModule.Load();
CoreModule.Load();

#if DEBUG
MeatcorpsEngineLibSettings.IsDebug = true;
#endif

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
    MaxPlayers = 2,
    Name = "CYBERMAZE!",
    Code = settings.GetOrDefault("ArcadeGame", "Code", 8271, false),
    PricePoints = settings.GetOrDefault("ArcadeGame", "PricePoints", 1000, false),
    Description = "The most gore version of the cyberPlayer game ever made.",
};

if (settings.GetOrDefault("ArcadeGame", "UseEmulator", true, false))
{
    ArcadeEmulatorModule.Load(game, mqtt, standaloneMode: true).SetIntroScene<IntroScene>();
}
else
{
    ArcadeGameSystemModule.Load(game, mqtt).SetIntroScene<IntroScene>();
    mqtt.Create();
}

GameSession.Load();
Raylib.SetTraceLogLevel(TraceLogLevel.Warning);

GlobalObjectManager.ObjectManager.Register(GameTileRules.Create());

RaylibAssetsModule.Load(new AssetConfig());

GlobalObjectManager.ObjectManager.Register<IGuiSettings>(new DefaultGuiSettings<GameInput, GameSounds>
{
    BackPressed = GameInput.Action,
    DownKey = GameInput.Down,
    UpKey = GameInput.Up,
    LeftKey = GameInput.Left,
    RightKey = GameInput.Right,
    OnSelectionPressed = GameInput.Start,
    ErrorSound = GameSounds.Alarm,
    NavigationSound = GameSounds.Scorechange,
    SelectionSound = GameSounds.PowerUpScore,
    NotificationSound = GameSounds.Backgroundplaced,
    PlayerInputId = 1,
    FontScaleSize = 1
});

using var _ = RayLibModule.Setup()
    .SetTitle("Meatcorps " + GlobalObjectManager.ObjectManager.Get<ArcadeGame>()!.Name)
    .SetInitialSize(1920, 1080)
    .AutoSize()
    .StartInFullscreen()
    .SetFixedSizeCamera(640, 360)
    .SetExitKey()
    .SetupProcessingBloom(0.6f, 0.2f, 0.8f, 2f)
    .SetProcessing(new CrtNewPixiePostProcessor())
    //.SetupProcessingPaletteRed()
    //.SetProcessing(new HeatwaveBasePostProcessing())
    .SetResource(new OneTexture("Assets/CRTSidePanels.png", texture2D =>
        GlobalObjectManager.ObjectManager.Get<CrtNewPixiePostProcessor>()!.SetFrameTexture(texture2D)
    ))
    .SetResource(GameSpriteFactory.Load())
    .SetResource(AudioEnumBinder.BindAllMusic(
        MusicResource<GameMusic>.Create(), "Assets/Music/"))
    .SetResource(AudioEnumBinder.BindAllSounds(
        SoundFxResource<GameSounds>.Create(10), "Assets/SoundFX/"))
    .SetResource(TextManager.OnlyOneFont("Assets/Fonts/PressStart2P-Regular.ttf"))
    .SetResource(new UniversalResourceLoader(() =>
    {
        
    }))
    .Load(new IntroScene())
    .Run();
    
    