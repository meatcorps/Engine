using System.Diagnostics;
using System.Numerics;
using Meatcorps.Engine.Core.Modules;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Settings;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.Logging.Module;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Audio;
using Meatcorps.Engine.RayLib.Game;
using Meatcorps.Engine.RayLib.Modules;
using Meatcorps.Engine.RayLib.PostProcessing;
using Meatcorps.Engine.RayLib.PostProcessing.Extensions;
using Meatcorps.Engine.RayLib.Resources;
using Meatcorps.Engine.RayLib.Stream;
using Raylib_cs;

LoggingModule.Load();
CoreModule.Load();
BasicConfig.Load();

MeatcorpsEngineLibSettings.IsDebug = true;

using var _ = RayLibModule.Setup()
    .SetTitle("Meatcorps Game Starter")
    .SetInitialSize(1920, 1080) // Turn this off when publishing the game!
    .SetupProcessingBloom(0.2f, 0.2f, 0.8f, 4f)
    .SetProcessing(new CrtNewPixiePostProcessor())
    .SetResource(new OneTexture("Assets/CRTSidePanels.png", texture2D =>
        GlobalObjectManager.ObjectManager.Get<CrtNewPixiePostProcessor>()!.SetFrameTexture(texture2D)
    ))
    .WithStream()
    .SetResource(SoundFxResource<GameSounds>.Create(6).AddSound(GameSounds.Sound, "sound.wav"))
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
    private float _totaltime;
    private FixedTimer _timer = new FixedTimer(2000);
    private SoundFxManager<GameSounds> _sound;
    
    protected override void OnInitialize()
    {
        _sound = GlobalObjectManager.ObjectManager.Get<SoundFxManager<GameSounds>>()!;
    }

    protected override void OnUpdate(float deltaTime)
    {
        _totaltime += deltaTime;
        
        _timer.Update(deltaTime);
        if (_timer.Output)
            _sound.Play(GameSounds.Sound);
    }

    protected override void OnDraw()
    {
        Raylib.ClearBackground(_sound.IsPlaying(GameSounds.Sound) ? Color.DarkGray : Color.Black);
        Raylib.DrawText(DateTime.Now.ToString(), 10, 10, 64, Color.Red);
    
        Raylib.DrawText(DateTime.Now.ToString(), 10, 74, 64, Color.Green);
    
        Raylib.DrawText(DateTime.Now.ToString(), 10, 138, 64, Color.Blue);
    
        Raylib.DrawText($"Total time: {_totaltime:f3}s", 10, 202, 64, Color.White);

        base.OnDraw();
    }

    protected override void OnDispose()
    {
    }
}

public enum GameSounds
{
    Sound
}
