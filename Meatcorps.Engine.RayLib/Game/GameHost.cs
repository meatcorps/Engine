using System.Diagnostics;
using System.Numerics;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.Interfaces.Trackers;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Game.GameTasks;
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
    private IUniversalConfig _config;
    private int _borderLessPosX;
    private int _borderLessPosY;
    private bool _disableMouseCursor = false;
    private KeyboardKey _exitKey = KeyboardKey.Escape;
    private List<IGameLoopTask> _gameLoopTasksForward = new();
    private List<IGameLoopTask> _gameLoopTasksBackward = new();
    public FrameTimer UpdateLoopTime;
    public FrameTimer RenderLoopTime;
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
        LoadGameLoopTasks();
    }
    
    public void SetMultiplier(float multiplier)
    {
        _timeService.DeltaMultiplier = multiplier;
    }
    
    public void ToggleFullscreen()
    {
        var monitor = _config.GetOrDefault("Graphics", "Monitor", -1);
        if (monitor > -1)
            Raylib.SetWindowMonitor(monitor);
        Raylib.ToggleFullscreen();
        Width = Raylib.GetScreenWidth();
        Height = Raylib.GetScreenHeight();
    }

    public void ToggleBorderlessWindow()
    {
        if (_borderLessPosX != -1 && _borderLessPosY != -1)
            Raylib.SetWindowPosition(_borderLessPosX, _borderLessPosY);

        Raylib.ToggleBorderlessWindowed();
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

    public void SetExistKey(KeyboardKey key = KeyboardKey.Null)
    {
        _exitKey = key;
    }

    public void SwitchScene(BaseScene scene)
    {
        foreach (var tasks in _gameLoopTasksForward) 
        {
            if (tasks is SceneTask sceneTask)
                sceneTask.SwitchScene(scene);
        }
    }

    public void LoadGameLoopTasks()
    {
        _gameLoopTasksForward = GlobalObjectManager.ObjectManager.GetList<IGameLoopTask>() ?? new List<IGameLoopTask>();
        _gameLoopTasksBackward = GlobalObjectManager.ObjectManager.GetList<IGameLoopTask>() ?? new List<IGameLoopTask>();
        _gameLoopTasksForward.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        _gameLoopTasksBackward.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        foreach (var task in _gameLoopTasksForward)
        {
            if (!task.IsInitialized)
                task.Initialize(this);
        }
        
    }
    
    public void Run()
    {
        UpdateLoopTime = new FrameTimer();
        RenderLoopTime = new FrameTimer();
        RunGameLoopTask(GameLoopType.PreRaylibInit, true);
        
        _config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>() ?? new BasicConfig();
        _borderLessPosX = _config.GetOrDefault("Graphics", "BorderlessWindowPositionX", -1);
        _borderLessPosY = _config.GetOrDefault("Graphics", "BorderlessWindowPositionY", -1);
        
        Raylib.InitWindow(Width, Height, _title);
        Raylib.SetExitKey(_exitKey);
        Raylib.SetTargetFPS(_targetFps);
        
        if (_config.GetOrDefault("Graphics", "Borderless", false))
            ToggleBorderlessWindow();
        
        if (_config.GetOrDefault("Graphics", "FullScreen", false))
            ToggleFullscreen();
        
        RunGameLoopTask(GameLoopType.PostRaylibInit, true);
        
        while (!Raylib.WindowShouldClose())
        {
            RunGameLoopTask(GameLoopType.BeforeUpdate, true);
            var totalSteps = 0;
            
            _timeService.UpdateFrameTimes();
            while (_timeService.TryDequeueStep(out var fixedDeltaTime))
            {
                using (UpdateLoopTime.Scope())
                {
                    RunGameLoopTask(GameLoopType.PreUpdate, true, fixedDeltaTime);
                    RunGameLoopTask(GameLoopType.Update, true, fixedDeltaTime);
                    RunGameLoopTask(GameLoopType.LateUpdate, false, fixedDeltaTime);
                }
                
                UpdateTimeInMs = UpdateLoopTime.AvgMs;
                totalSteps++;
            }
            _timeService.FinalizeFrame();
            
            RunGameLoopTask(GameLoopType.AfterUpdate, true);
            RunGameLoopTask(GameLoopType.PreRender, true);
            
            using (RenderLoopTime.Scope())
            {
                RunGameLoopTask(GameLoopType.Render, true);
            }
            
            RunGameLoopTask(GameLoopType.PostRender, false);
#if DEBUG
            Raylib.SetWindowTitle($"Steps: {totalSteps}, Update time {UpdateLoopTime:F4}, Render time {RenderLoopTime:F4}, FPS {Raylib.GetFPS()}");
#endif
        }
        
        Raylib.CloseWindow();
        RunGameLoopTask(GameLoopType.AfterClosingWindow, true);
    }

    private void RunGameLoopTask(GameLoopType type, bool forward, float deltaTime = 0f)
    {
        foreach (var task in forward ? _gameLoopTasksForward : _gameLoopTasksBackward)
        {
            if (task.Enabled)
                task.Task(type, deltaTime);
        }
    }

    public void Dispose()
    {
        GlobalObjectManager.ObjectManager.Dispose();
        if (_gameHasAudio)
            Raylib.CloseAudioDevice();
    }
}