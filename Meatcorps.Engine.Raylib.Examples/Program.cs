// See https://aka.ms/new-console-template for more information

using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Abstractions;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.Raylib.Examples.Scenes;
using Meatcorps.Engine.RayLib.Modules;
using Meatcorps.Engine.RayLib.Resources;

LoggingModule.Load();
CoreModule.Load();

var settings = new BasicConfig();

GlobalObjectManager.ObjectManager.Register<IUniversalConfig>(settings);

using var _ = RayLibModule.Setup()
    .SetFixedSizeCamera(1920, 1080, false)
    .SetTitle("Controller Test")
    .SetInitialSize(1920, 1080)
    .Load(new MainScene())
    .SetResource(TextManager.OnlyOneFont("PressStart2P-Regular.ttf"))
    .Run();