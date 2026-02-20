using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Game.GameTasks;

public class BackgroundServicesTask : IGameLoopTask
{
    private readonly List<IBackgroundService> _backgroundServices = new();

    public BackgroundServicesTask(int priority = 2)
    {
        Priority = priority;
    }

    public int Priority { get; }
    public bool Enabled { get; set; } = true;
    public bool IsInitialized { get; private set; }

    public void Initialize(GameHost host)
    {
        IsInitialized = true;
    }

    public void Task(GameLoopType type, float deltaTime)
    {
        if (type == GameLoopType.PostRaylibInit)
            _backgroundServices.AddRange(GlobalObjectManager.ObjectManager.GetList<IBackgroundService>() ??
                                         new List<IBackgroundService>());

        if (type == GameLoopType.PreUpdate)
            foreach (var service in _backgroundServices)
                service.PreUpdate(deltaTime);

        if (type == GameLoopType.Update)
            foreach (var service in _backgroundServices)
                service.Update(deltaTime);

        if (type == GameLoopType.LateUpdate)
            foreach (var service in _backgroundServices)
                service.LateUpdate(deltaTime);
    }
}