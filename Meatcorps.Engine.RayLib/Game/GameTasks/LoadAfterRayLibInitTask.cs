using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Resources;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Game.GameTasks;

public class LoadAfterRayLibInitTask: IGameLoopTask
{
    public int Priority { get; } = 1;
    public bool Enabled { get; set; } = true;
    public bool IsInitialized { get; private set; }

    private bool _isLoadingInitialized;

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
        if (type == GameLoopType.PostRaylibInit && !_isLoadingInitialized)
        {
            _isLoadingInitialized = true;
            
            //foreach (var instance in GlobalObjectManager.ObjectManager.GetList<IResourceLoadOnInit>()!)
            //    instance.Load();

            foreach (var task in GlobalObjectManager.ObjectManager.GetList<IGameLoopTask>()!)
            {
                if (task is not LoadAfterRayLibInitTask)
                    task.Enabled = false;
            }
            
            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                foreach (var instance in GlobalObjectManager.ObjectManager.GetList<IResourceLoadOnInit>()!)
                    await instance.Load();

                foreach (var task in GlobalObjectManager.ObjectManager.GetList<IGameLoopTask>()!)
                {
                    if (task is not LoadAfterRayLibInitTask)
                        task.Enabled = true;
                }
            });
            
        }

        if (type == GameLoopType.Update)
        {
            GlobalObjectManager.ObjectManager.Get<ResourceManager>()!.RunTasks();
        }

        if (type == GameLoopType.Render)
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            Raylib.DrawText("Loading...", 16, 16, 32, Color.White);
            Raylib.EndDrawing();
        }
    }
}