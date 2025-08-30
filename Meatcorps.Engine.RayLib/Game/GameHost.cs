using System.Diagnostics;
using System.Numerics;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.Interfaces.Trackers;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Renderer;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Game;

public sealed class GameHost : IDisposable
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public double UpdateTimeInMs { get; private set; }
    public double RenderTimeInMs { get; private set;}
    private readonly string _title;
    private readonly int _targetFps;
    private readonly TimeService _timeService = new();
    private BaseScene? _newSceneToLoad = null;
    private readonly List<IBackgroundService> _backgroundServices = new();
    private bool _gameHasAudio = false;
    public RenderService RenderService { get; }
    
    public GameHost(int width, int height, string title, int targetFps = 60, ICamera? camera = null)
    {
        Width = width;
        Height = height;
        _title = title;
        _targetFps = targetFps;
        
        GlobalObjectManager.ObjectManager.Register<ITimeService>(_timeService);
        GlobalObjectManager.ObjectManager.Register<GameHost>(this);
        
        if (camera != null)
            GlobalObjectManager.ObjectManager.Register<ICamera>(camera);
        
        RenderService = new RenderService(GlobalObjectManager.ObjectManager);
        
        GlobalObjectManager.ObjectManager.Register(RenderService);
    }
    
    public void ToggleFullscreen()
    {
        Raylib.ToggleFullscreen();
        Width = Raylib.GetScreenWidth();
        Height = Raylib.GetScreenHeight();
    }

    public void SetWindowSize(int newWidth, int newHeight)
    {
        Width = newWidth;
        Height = newHeight;
        Raylib.SetWindowSize(newWidth, newHeight);
    }

    public Vector2 GetWindowSize()
    {
        return new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
    }
    
    public void SwitchScene(BaseScene scene)
    {
        _newSceneToLoad = scene;
        foreach (var service in GlobalObjectManager.ObjectManager.GetList<ISceneSwitchTracker>()!)
            service.OnActiveSceneSwitch(_newSceneToLoad);
    }

    public void Run()
    {
        Raylib.InitWindow(Width, Height, _title);
        Raylib.SetTargetFPS(_targetFps);
        var updateLoopTime = new FrameTimer();
        var renderLoopTime = new FrameTimer();

        if (GlobalObjectManager.ObjectManager.GetList<ILoadAfterRayLibInit>()!.Any(x => x is IAudioInitNeeded))
        {
            if (!Raylib.IsAudioDeviceReady())
                Raylib.InitAudioDevice();
            _gameHasAudio = true;
        }

        foreach (var instance in GlobalObjectManager.ObjectManager.GetList<ILoadAfterRayLibInit>()!)
            instance.Load();
        
        _backgroundServices.AddRange(GlobalObjectManager.ObjectManager.GetList<IBackgroundService>() ?? new List<IBackgroundService>());
        
        while (!Raylib.WindowShouldClose())
        {
            if (_newSceneToLoad is not null)
            {
                _newSceneToLoad.SetGameHost(this);
                var currentScene = GlobalObjectManager.ObjectManager.Get<BaseScene>();
                if (currentScene != null)
                {
                    currentScene.Dispose();
                    GlobalObjectManager.ObjectManager.Remove<BaseScene>();
                }

                GlobalObjectManager.ObjectManager.Register(_newSceneToLoad);
        
                _newSceneToLoad.Initialize();

                _newSceneToLoad = null;
            }
            
            var activeScene = GlobalObjectManager.ObjectManager.Get<BaseScene>()!;
            
            _timeService.UpdateFrameTimes(); // update deltaTime etc.
            var totalSteps = 0;
            while (_timeService.TryDequeueStep(out var fixedDeltaTime))
            {
                using (updateLoopTime.Scope())
                {
                    foreach (var backgroundService in _backgroundServices)
                        backgroundService.PreUpdate(fixedDeltaTime * activeScene.UpdateTimeMultiplier);
                    activeScene.PreUpdate(fixedDeltaTime * activeScene.UpdateTimeMultiplier);
                    foreach (var backgroundService in _backgroundServices)
                        backgroundService.Update(fixedDeltaTime * activeScene.UpdateTimeMultiplier);
                    activeScene.Update(fixedDeltaTime * activeScene.UpdateTimeMultiplier);
                    activeScene.AlwaysUpdate(fixedDeltaTime * activeScene.UpdateTimeMultiplier);
                    activeScene.LateUpdate(fixedDeltaTime * activeScene.UpdateTimeMultiplier);
                    foreach (var backgroundService in _backgroundServices)
                        backgroundService.LateUpdate(fixedDeltaTime * activeScene.UpdateTimeMultiplier);

                    RenderService.Update(fixedDeltaTime * activeScene.UpdateTimeMultiplier);
                }

                UpdateTimeInMs = updateLoopTime.AvgMs;
                totalSteps++;
            }

            _timeService.FinalizeFrame();

            var scope = renderLoopTime.Scope();
            activeScene.RegisterForRender();
            activeScene.Draw();
            RenderService.Render(scope);
            
#if DEBUG
            Raylib.SetWindowTitle($"Update time {updateLoopTime:F4}, Render time {renderLoopTime:F4}, FPS {Raylib.GetFPS()}");
#endif
        }

        Raylib.CloseWindow();
        
    }

    public void Dispose()
    {
        GlobalObjectManager.ObjectManager.Dispose();
        if (_gameHasAudio)
            Raylib.CloseAudioDevice();
    }
}