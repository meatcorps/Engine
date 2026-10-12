using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Game;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.R3d.Data;
using Meatcorps.Engine.RayLib.R3d.Interfaces;
using Meatcorps.Engine.RayLib.Renderer;
using R3D_cs;

namespace Meatcorps.Engine.RayLib.R3d.GameTasks;

public class R3dGameLoop: IGameLoopTask, IConfigChangeTracker
{
    private GameHost _host = null!;
    private R3dSettings _settings = null!;
    private RenderService _renderService = null!;
    private IUniversalConfig _config = null!;
    
    public int Priority { get; }
    public bool Enabled { get; set; } = true;
    public bool IsInitialized { get; private set; }

    public R3dGameLoop(int priority = 1)
    {
        Priority = priority;
        GlobalObjectManager.ObjectManager.RegisterList<IR3dDispose>();
    }
    
    public void Initialize(GameHost host)
    {
        _host = host;
        IsInitialized = true;
        _settings = GlobalObjectManager.ObjectManager.Get<R3dSettings>()!;
        _renderService = GlobalObjectManager.ObjectManager.Get<RenderService>()!;
        _config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>()!;
        GlobalObjectManager.ObjectManager.Add<IConfigChangeTracker>(this);
    }

    public void Task(GameLoopType type, float deltaTime)
    {
        switch (type)
        {
            case GameLoopType.PreRaylibInit:
                if (Enum.TryParse<AntiAliasingMode>(_config.GetOrDefault("Graphics", "AntiAliasingMode", _settings.AntiAliasingMode.Value.ToString()), out var aaMode))
                    _settings.AntiAliasingMode.Value = aaMode;
                if (Enum.TryParse<AntiAliasingPreset>(_config.GetOrDefault("Graphics", "AntiAliasingPreset", _settings.AntiAliasingPreset.Value.ToString()), out var aaPreset))
                    _settings.AntiAliasingPreset.Value = aaPreset;
                break;
            case GameLoopType.PostRaylibInit:
                UpdateRenderSize();
                _settings.InitializeAll();
                break;
            
            case GameLoopType.BeforeUpdate:
                UpdateRenderSize();
                _settings.RenderBufferSize.Apply();
                break;

            case GameLoopType.PreClosingWindow:
                var disposables =
                    GlobalObjectManager.ObjectManager.GetList<IR3dDispose>();

                if (disposables != null)
                {
                    for (var i = disposables.Count - 1; i >= 0; i--)
                        disposables[i].DisposeR3d();
                }
                R3D.Close();
                break;
        }
    }

    private void UpdateRenderSize()
    {
        switch (_settings.RenderBufferSizeMode.Value)
        {
            case RenderBufferSizeModeEnum.WindowSize:
                if (_host.Width > 10 && _host.Height > 10)
                    _settings.RenderBufferSize.Value = new PointInt(_host.Width, _host.Height);
                break;
            case RenderBufferSizeModeEnum.RenderStrategy:
                var primary = _renderService.PrimaryRenderTargetStrategy;
                if (primary.RenderWidth > 0 && primary.RenderHeight > 0)
                    _settings.RenderBufferSize.Value = new PointInt(primary.RenderWidth, primary.RenderHeight);
                break;
            case RenderBufferSizeModeEnum.Custom:
                if (_settings.RenderBufferSize.Value.X <= 0 || _settings.RenderBufferSize.Value.Y <= 0)
                    throw new ArgumentOutOfRangeException(nameof(_settings.RenderBufferSize), "Render buffer size must be greater than zero both on the X and Y axis when using custom render buffer size");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ConfigChanged(string group, string key, object value)
    {
        if (group != "Graphics")
            return;
        if (key == "AntiAliasingPreset")
        {
            if (Enum.TryParse<AntiAliasingPreset>(
                    _config.GetOrDefault("Graphics", "AntiAliasingPreset",
                        _settings.AntiAliasingPreset.Value.ToString()), out var aaPreset))
            {
                _settings.AntiAliasingPreset.Value = aaPreset;
                _settings.AntiAliasingPreset.Apply();
            }
        }
        else if (key == "AntiAliasingMode")
        {
            if (Enum.TryParse<AntiAliasingMode>(
                    _config.GetOrDefault("Graphics", "AntiAliasingMode", _settings.AntiAliasingMode.Value.ToString()),
                    out var aaMode))
            {
                _settings.AntiAliasingMode.Value = aaMode;
                _settings.AntiAliasingMode.Apply();
            }
        }
    }
}