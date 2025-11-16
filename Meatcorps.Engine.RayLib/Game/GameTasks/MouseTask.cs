using Meatcorps.Engine.Core.Interfaces.Config;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.Core.Storage.Data;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Game.GameTasks;

public class MouseTask: IGameLoopTask
{
    private IUniversalConfig _config;
    private bool _disableMouseCursor;
    public int Priority { get; }
    public bool Enabled { get; set; } = true;
    public bool IsInitialized { get; private set; }

    public MouseTask(int priority = 0)
    {
        Priority = priority;
    }
    
    public void Initialize(GameHost host)
    {
        IsInitialized = true;
        _config = GlobalObjectManager.ObjectManager.Get<IUniversalConfig>() ?? new BasicConfig();
    }

    public void Task(GameLoopType type, float deltaTime)
    {
        if (type == GameLoopType.PreRaylibInit)
        {
            if (_config.GetOrDefault("General", "HideMouseCursor", false))
                Raylib.HideCursor();

            _disableMouseCursor = _config.GetOrDefault("General", "DisableMouseCursor", true);
        }

        if (type == GameLoopType.PreRender && _disableMouseCursor)
            Raylib.DisableCursor();
    }
}