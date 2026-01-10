using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.RayLib.Assets.Modules;
using Meatcorps.Engine.RayLib.Modules;
using Meatcorps.Engine.RayLib.PostProcessing.Extensions;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.RayLib.UI.GuiComponent.GuiSettings;
using Meatcorps.Game.GameStarter;
using Meatcorps.Game.GameStarter.Data;
using Meatcorps.Game.GameStarter.GameEnums;
using Meatcorps.Game.GameStarter.Resources;
using Meatcorps.Game.GameStarter.Scenes;
using Raylib_cs;
using GameInput = Meatcorps.Game.GameStarter.Resources.GameInput;

LoggingModule.Load();
CoreModule.Load();

var settings = GameConfig<GameSettings>.Create();

GameInput.Load();

GameSession.Load();
Raylib.SetTraceLogLevel(TraceLogLevel.Warning);


RaylibAssetsModule.Load(new AssetConfig());

GlobalObjectManager.ObjectManager.Register<IGuiSettings>(new DefaultGuiSettings<Meatcorps.Game.GameStarter.GameEnums.GameInput, GameSounds>
{
    BackPressed = Meatcorps.Game.GameStarter.GameEnums.GameInput.Action,
    DownKey = Meatcorps.Game.GameStarter.GameEnums.GameInput.Down,
    UpKey = Meatcorps.Game.GameStarter.GameEnums.GameInput.Up,
    LeftKey = Meatcorps.Game.GameStarter.GameEnums.GameInput.Left,
    RightKey = Meatcorps.Game.GameStarter.GameEnums.GameInput.Right,
    OnSelectionPressed = Meatcorps.Game.GameStarter.GameEnums.GameInput.Start,
    //ErrorSound = GameSounds.Alarm,
    //NavigationSound = GameSounds.Scorechange,
    //SelectionSound = GameSounds.PowerUpScore,
    //NotificationSound = GameSounds.Backgroundplaced,
    PlayerInputId = 1,
    FontScaleSize = 1
});

using var _ = RayLibModule.Setup()
    .SetTitle("Meatcorps Game Starter")
    .SetInitialSize(1920, 1080) // Turn this off when publishing the game!
    //.AutoSize() Turn this on when releasing the game!
    //.StartInFullscreen()
    .SetFixedSizeCamera(640, 360)
    //.SetExitKey()
    .SetupProcessingBloom(0.6f, 0.2f, 0.8f, 2f)
    .SetProcessing(new CrtNewPixiePostProcessor())
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
    .Load(new ExampleScene())
    .Run();
    
    