using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.Raylib.Examples.Enums;
using Meatcorps.Engine.Raylib.Examples.Resources;
using Meatcorps.Engine.Raylib.Examples.Scenes;
using Meatcorps.Engine.RayLib.Modules;
using Meatcorps.Engine.RayLib.PostProcessing;
using Meatcorps.Engine.RayLib.PostProcessing.Extensions;
using Meatcorps.Engine.RayLib.Resources;

LoggingModule.Load();
CoreModule.Load();

var settings = new BasicConfig();

GameFallbackInput.Load();

GlobalObjectManager.ObjectManager.Register<IUniversalConfig>(settings);
using var _ = RayLibModule.Setup()
    .SetTitle("Controller Test")
    .SetFps(120)
    .SetInitialSize(1920, 1080)
    .SetFixedSizeCamera(640, 360)
    .SetupProcessingBloom(0.2f, 0.2f, 0.8f, 4f)
    .SetExitKey()
    .SetProcessing(new CrtNewPixiePostProcessor())
    .SetResource(new OneTexture("Assets/CRTSidePanels.png", texture2D =>
        GlobalObjectManager.ObjectManager.Get<CrtNewPixiePostProcessor>()!.SetFrameTexture(texture2D)
    ))
    .SetResource(new OneTexture("Assets/test2.png"), "BGPIC")
    .SetResource(TextManager.OnlyOneFont("PressStart2P-Regular.ttf"))
    //.SetResource(TextManager.OnlyOneFont("Assets/gnf.regular.ttf", 16))
    .SetResource(AudioEnumBinder.BindAllSounds(
        SoundFxResource<GameSounds>
            .Create(6)
            .UsePlaceHoldersForMissingFiles(), "Assets/SoundFX/"))
    .Load(new MainScene())
    .Run();