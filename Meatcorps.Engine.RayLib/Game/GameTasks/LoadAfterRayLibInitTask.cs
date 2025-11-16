using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;

namespace Meatcorps.Engine.RayLib.Game.GameTasks;

public class LoadAfterRayLibInitTask: IGameLoopTask
{
    public int Priority { get; } = 1;
    public bool Enabled { get; set; } = true;
    public bool IsInitialized { get; private set; }

    public LoadAfterRayLibInitTask(int priority = 1)
    {
        Priority = priority;
    }
    
    public void Initialize(GameHost host)
    {
        IsInitialized = true;
    }

    public void Task(GameLoopType type, float deltaTime)
    {
        if (type == GameLoopType.PostRaylibInit)
            foreach (var instance in GlobalObjectManager.ObjectManager.GetList<ILoadAfterRayLibInit>()!)
                instance.Load();

    }
}