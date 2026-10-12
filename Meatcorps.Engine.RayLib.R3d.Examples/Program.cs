using System.Numerics;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Modules;
using Meatcorps.Engine.RayLib.PostProcessing;
using Meatcorps.Engine.RayLib.PostProcessing.Extensions;
using Meatcorps.Engine.RayLib.R3d.Camera;
using Meatcorps.Engine.RayLib.R3d.Examples.Resources;
using Meatcorps.Engine.RayLib.R3d.Examples.Scenes;
using Meatcorps.Engine.RayLib.R3d.Modules;
using Meatcorps.Engine.RayLib.R3d.Resources;
using Meatcorps.Engine.RayLib.Resources;

LoggingModule.Load();
CoreModule.Load();

var settings = new BasicConfig();

GameFallbackInput.Load();

GlobalObjectManager.ObjectManager.Register<IUniversalConfig>(settings);

var camera = new Camera3D(false);
camera.Position3D = new Vector3(10, 1, 10);
camera.Target3D = new Vector3(0, 1, 0);

using var _ = RayLibModule.Setup()
    .RegisterCustomBuildAction(_ =>
    {
        R3dRegistrationHelper.AutoLoadAssets();
    })
    .SetTitle("R3D Test")
    .Enable3D()
    .Set3DCamera(camera)
    .SetFps(120)
    .SetInitialSize(1920, 1080)
    .Set3DFixedCamera(1920, 1080)
    .SetupProcessingBloom(0.2f, 0.2f, 0.8f, 4f)
    .SetProcessing(new CrtNewPixiePostProcessor())
    .SetResource(new OneTexture("Assets/CRTSidePanels.png", texture2D =>
        GlobalObjectManager.ObjectManager.Get<CrtNewPixiePostProcessor>()!.SetFrameTexture(texture2D)
    ))
    .SetResource(TextManager.OnlyOneFont("Assets/PressStart2P-Regular.ttf"))
    .Load(new MainScene())
    .Run();