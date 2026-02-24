using System.Numerics;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Settings;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.Core.Utilities;
using Meatcorps.Engine.RayLib.Abstractions;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Game.GameTasks;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Renderer;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Game;

public sealed class GameHost : IDisposable, IConfigChangeTracker
{
    private readonly bool _gameHasAudio = false;
    private readonly TimeService _timeService = new();
    private readonly string _title;
    private int _borderLessPosX;
    private int _borderLessPosY;
    private IUniversalConfig _config = null!;
    private KeyboardKey _exitKey = KeyboardKey.Escape;
    private List<IGameLoopTask> _gameLoopTasksBackward = new();
    private List<IGameLoopTask> _gameLoopTasksForward = new();
    private bool _isBorderless;
    private int _targetFps;
    public FrameTimer RenderLoopTime { get; private set; } = null!;
    public FrameTimer UpdateLoopTime { get; private set; } = null!;

    public GameHost(int width, int height, string title, int targetFps = 60, ICamera? camera = null)
    {
        Width = width;
        Height = height;
        _title = title;
        _targetFps = targetFps;

        GlobalObjectManager.ObjectManager.Register<ITimeService>(_timeService);
        GlobalObjectManager.ObjectManager.Register(this);

        if (camera != null)
            GlobalObjectManager.ObjectManager.Register(camera);

        RenderService = new RenderService(GlobalObjectManager.ObjectManager);

        GlobalObjectManager.ObjectManager.Add<IConfigChangeTracker>(this);
        GlobalObjectManager.ObjectManager.Register(RenderService);
        LoadGameLoopTasks();
    }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public RenderService RenderService { get; }

    public void ConfigChanged(string group, string key, object value)
    {
        var width = _config.GetOrDefault("Graphics", "WindowWidth", Width);
        if (width > 0)
            Width = width;
        
        var height = _config.GetOrDefault("Graphics", "WindowHeight", Height);
        if (height > 0)
            Height = height;
        
        SetWindowSize(Width, Height);
        _borderLessPosX = _config.GetOrDefault("Graphics", "BorderlessWindowPositionX", -1);
        _borderLessPosY = _config.GetOrDefault("Graphics", "BorderlessWindowPositionY", -1);
        SetBorderlessWindow(_config.GetOrDefault("Graphics", "Borderless", false));
        SetFullscreen(_config.GetOrDefault("Graphics", "FullScreen", false));
    }

    public void Dispose()
    {
        GlobalObjectManager.ObjectManager.Dispose();
        if (_gameHasAudio)
            Raylib.CloseAudioDevice();
    }

    public void SetFps(int? fps)
    {
        if (fps is not null)
            _targetFps = fps.Value;

        Raylib.SetTargetFPS(_targetFps);
    }


    public void SetMultiplier(float multiplier)
    {
        _timeService.DeltaMultiplier = multiplier;
    }

    public void SetFullscreen(bool fullscreen = true)
    {
        if (Raylib.IsWindowFullscreen() == fullscreen)
            return;

        _isBorderless = false;

        var monitor = _config.GetOrDefault("Graphics", "Monitor", -1);
        if (monitor > -1)
            Raylib.SetWindowMonitor(monitor);
        Raylib.ToggleFullscreen();
        Width = Raylib.GetScreenWidth();
        Height = Raylib.GetScreenHeight();
    }

    public void SetBorderlessWindow(bool borderless = true)
    {
        if (borderless == _isBorderless)
            return;

        _isBorderless = !_isBorderless;

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
            if (tasks is SceneTask sceneTask)
                sceneTask.SwitchScene(scene);
    }

    public void LoadGameLoopTasks()
    {
        _gameLoopTasksForward = GlobalObjectManager.ObjectManager.GetList<IGameLoopTask>() ?? new List<IGameLoopTask>();
        _gameLoopTasksBackward =
            GlobalObjectManager.ObjectManager.GetList<IGameLoopTask>() ?? new List<IGameLoopTask>();
        _gameLoopTasksForward.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        _gameLoopTasksBackward.Sort((a, b) => a.Priority.CompareTo(b.Priority));

        foreach (var task in _gameLoopTasksForward)
            if (!task.IsInitialized)
                task.Initialize(this);
    }

    public void Run()
    {
        UpdateLoopTime = new FrameTimer();
        RenderLoopTime = new FrameTimer();
        RunGameLoopTask(GameLoopType.PreRaylibInit, true);

        _config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>() ?? new BasicConfig();
        _config.GetOrDefault("Graphics", "Monitor", -1, false);
        _borderLessPosX = _config.GetOrDefault("Graphics", "BorderlessWindowPositionX", -1, false);
        _borderLessPosY = _config.GetOrDefault("Graphics", "BorderlessWindowPositionY", -1, false);

        Raylib.InitWindow(Width, Height, _title);
        Raylib.SetExitKey(_exitKey);
        Raylib.SetTargetFPS(_targetFps);

        if (_config.GetOrDefault("Graphics", "Borderless", false, false))
            SetBorderlessWindow();

        if (_config.GetOrDefault("Graphics", "FullScreen", false, false))
            SetFullscreen();

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
            if (MeatcorpsEngineLibSettings.IsDebug) 
                Raylib.SetWindowTitle(
                    $"Steps: {totalSteps}, Update time {UpdateLoopTime}, Render time {RenderLoopTime}, FPS {Raylib.GetFPS()}");
        }

        Raylib.CloseWindow();
        RunGameLoopTask(GameLoopType.AfterClosingWindow, true);
    }

    private void RunGameLoopTask(GameLoopType type, bool forward, float deltaTime = 0f)
    {
        foreach (var task in forward ? _gameLoopTasksForward : _gameLoopTasksBackward)
            if (task.Enabled)
                task.Task(type, deltaTime);
    }
}