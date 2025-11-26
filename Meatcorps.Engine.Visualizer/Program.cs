using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Abstractions;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.RayLib.Modules;
using Meatcorps.Engine.RayLib.PostProcessing;
using Meatcorps.Engine.RayLib.PostProcessing.Extensions;
using Meatcorps.Engine.RayLib.RemixIcons;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.Visualizer.Enums;
using Meatcorps.Engine.Visualizer.Scenes;

LoggingModule.Load();
CoreModule.Load();

var settings = new BasicConfig();

GlobalObjectManager.ObjectManager.Register<IUniversalConfig>(settings);
using var _ = RayLibModule.Setup()
    .SetTitle("Visualizer")
    .SetInitialSize(1920, 1080)
    .SetFps(120)
    .SetFixedSizeCamera(960, 540)
    .SetupProcessingBloom(0.6f, 0.2f, 0.8f, 4f)
    .SetProcessing(new CrtNewPixiePostProcessor())
    .SetResource(new OneTexture("Assets/CRTSidePanels.png", texture2D => 
        GlobalObjectManager.ObjectManager.Get<CrtNewPixiePostProcessor>()!.SetFrameTexture(texture2D)
    ))
    .SetResource(new TextManager<FontEnum>()
        .AddFont("Assets/Fonts/PressStart2P-Regular.ttf", FontEnum.Default)
        .AddRemixFont(FontEnum.Icons))
    .SetExitKey()
    .Load(new MainScene())
    .Run();