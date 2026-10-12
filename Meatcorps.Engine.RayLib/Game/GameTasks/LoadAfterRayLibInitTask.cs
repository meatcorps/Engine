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
    private GameHost _host = null!;

    private bool _isLoadingInitialized;

    public LoadAfterRayLibInitTask(int priority = 2)
    {
        Priority = priority;
    }

    public int Priority { get; }
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

            var tasks = new ConcurrentBag<(string, Task)>();

            foreach (var instance in GlobalObjectManager.ObjectManager.GetList<IResourceLoadOnInit>()!)
            {
                tasks.Add((instance.GetType().Name, System.Threading.Tasks.Task.Run(instance.Load)));
            }

            var allTasks = System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.WhenAll(tasks.Select(x => x.Item2));

                foreach (var task in GlobalObjectManager.ObjectManager.GetList<IGameLoopTask>()!)
                    if (task is not LoadAfterRayLibInitTask)
                        task.Enabled = true;

                taskDone = true;
            });

            var total = 0;
            foreach (var loadTask in GlobalObjectManager.ObjectManager.GetList<IResourceLoadOnInit>()!)
                total += loadTask.TotalResources;

            Raylib.SetTargetFPS(0);
            var previousDone = 0;
            var done = 0;
            while (!allTasks.IsCompleted && !Raylib.WindowShouldClose())
            {
                done = 0;
                foreach (var loadTask in GlobalObjectManager.ObjectManager.GetList<IResourceLoadOnInit>()!)
                    done += loadTask.ResourcesLoaded;

                if (done != previousDone)
                {
                    Console.WriteLine($"Loading {done}/{total} TODO:");
                    foreach (var loadTask in GlobalObjectManager.ObjectManager.GetList<IResourceLoadOnInit>()!)
                    {
                        if (loadTask.TotalResources != loadTask.ResourcesLoaded)
                            Console.WriteLine(
                                $"  {loadTask.GetType().Name}: {loadTask.ResourcesLoaded}/{loadTask.TotalResources}");
                    }

                    previousDone = done;
                }
                    
                    
                var normal = (float)done / total;
                GlobalObjectManager.ObjectManager.Get<ResourceManager>()!.RunTasks();
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                Raylib.DrawText($"Loading... [{done}/{total}]", 16, 16, 32, Color.White);
                Raylib.DrawRectangleLinesEx(
                    new Rectangle(new Vector2(16, _host.Height / 2f - 16), new Vector2(_host.Width - 32, 32)), 2,
                    Color.White);
                Raylib.DrawRectangleRec(
                    new Rectangle(new Vector2(20, _host.Height / 2f - 12), new Vector2((_host.Width - 40) * normal, 24)),
                    Color.White);
                Raylib.EndDrawing();
            }

            if (allTasks.IsFaulted)
            {
                foreach (var task in tasks)
                {
                    if (!task.Item2.IsFaulted)
                        continue;

                    Console.WriteLine($"Resource failed: {task.Item2}");
                    Console.WriteLine(task.Item2.Exception);
                }

                allTasks.GetAwaiter().GetResult();
                throw new AggregateException("One or more tasks failed", allTasks.Exception.InnerExceptions);
            }

            _host.SetFps(null);
            
            done = 0;
            foreach (var loadTask in GlobalObjectManager.ObjectManager.GetList<IResourceLoadOnInit>()!)
                done += loadTask.ResourcesLoaded;
            
            
            Console.WriteLine($"Loading done... RESULT: {done}/{total}");
        }
    }
}