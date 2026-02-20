using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.Settings;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Modules;
using Raylib_cs;

LoggingModule.Load();
CoreModule.Load();
BasicConfig.Load();

MeatcorpsEngineLibSettings.IsDebug = true;

using var _ = RayLibModule.Setup()
    .SetTitle("Meatcorps Game Starter")
    .SetInitialSize(1920, 1080) // Turn this off when publishing the game!
    .Load(new ExampleScene())
    .Run();


public class ExampleScene : BaseScene
{
    protected override void OnInitialize()
    {
        AddGameObject(new ExampleGameObject());
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnDispose()
    {
    }
}

public class ExampleGameObject : BaseGameObject
{
    protected override void OnInitialize()
    {
    }

    protected override void OnUpdate(float deltaTime)
    {
    }

    protected override void OnDraw()
    {
        Raylib.DrawText("HELLO WORLD!", 32, 32, 16, Color.White);
        base.OnDraw();
    }

    protected override void OnDispose()
    {
    }
}