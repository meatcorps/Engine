using System.Collections.Concurrent;
using System.Numerics;
using Meatcorps.Engine.Core.ObjectManager;
using Meatcorps.Engine.RayLib.Enums;
using Meatcorps.Engine.RayLib.Interfaces;
using Meatcorps.Engine.RayLib.Resources;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Game.GameTasks;

public class LoadAfterRayLibInitTask : IGameLoopTask
{
    private GameHost _host;

    private bool _isLoadingInitialized;

    public LoadAfterRayLibInitTask(int priority = 1)
    {
        Priority = priority;
    }

    public int Priority { get; } = 1;
    public bool Enabled { get; set; } = true;
    public bool IsInitialized { get; private set; }

    public void Initialize(GameHost host)
    {
        _host = host;
        IsInitialized = true;
    }

    public void Task(GameLoopType type, float deltaTime)
    {
        if (type == GameLoopType.PostRaylibInit && !_isLoadingInitialized)
        {
            _isLoadingInitialized = true;

            foreach (var task in GlobalObjectManager.ObjectManager.GetList<IGameLoopTask>()!)
                if (task is not LoadAfterRayLibInitTask)
                    task.Enabled = false;

            var taskDone = false;

            var tasks = new ConcurrentBag<Task>();

            foreach (var instance in GlobalObjectManager.ObjectManager.GetList<IResourceLoadOnInit>()!)
            {
                tasks.Add(System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        instance.Load();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Something really bad happend while loading more info:\n" + e);
                    }
                }));
            }

            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.WhenAll(tasks);

                foreach (var task in GlobalObjectManager.ObjectManager.GetList<IGameLoopTask>()!)
                    if (task is not LoadAfterRayLibInitTask)
                        task.Enabled = true;

                taskDone = true;
            });

            var total = 0;
            foreach (var loadTask in GlobalObjectManager.ObjectManager.GetList<IResourceLoadOnInit>()!)
                total += loadTask.TotalResources;

            Raylib.SetTargetFPS(0);
            while (!taskDone && !Raylib.WindowShouldClose())
            {
                var done = 0;
                foreach (var loadTask in GlobalObjectManager.ObjectManager.GetList<IResourceLoadOnInit>()!)
                    done += loadTask.ResourcesLoaded;

                var normal = (float)done / total;
                GlobalObjectManager.ObjectManager.Get<ResourceManager>()!.RunTasks();
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                Raylib.DrawText($"Loading... [{done}/{total}]", 16, 16, 32, Color.White);
                Raylib.DrawRectangleLinesEx(
                    new Rectangle(new Vector2(16, _host.Height / 2 - 16), new Vector2(_host.Width - 32, 32)), 2,
                    Color.White);
                Raylib.DrawRectangleRec(
                    new Rectangle(new Vector2(20, _host.Height / 2 - 12), new Vector2((_host.Width - 40) * normal, 24)),
                    Color.White);
                Raylib.EndDrawing();
            }

            _host.SetFps(null);
        }
    }
}