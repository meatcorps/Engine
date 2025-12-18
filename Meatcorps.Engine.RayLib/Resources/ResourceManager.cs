using System.Collections.Concurrent;

namespace Meatcorps.Engine.RayLib.Resources;

public class ResourceManager
{
    private readonly ConcurrentQueue<Tuple<Action, TaskCompletionSource>> _mainTaskRunner = new();

    public bool AllTaskDone => _mainTaskRunner.Count == 0;

    public async Task AddTaskToMainThread(Action task)
    {
        Console.WriteLine("ADDING TASK: " + _mainTaskRunner.Count);
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _mainTaskRunner.Enqueue(Tuple.Create(task, tcs));
        await tcs.Task;
    }

    public void RunTasks()
    {
        while (_mainTaskRunner.TryDequeue(out var task))
        {
            task.Item1();
            task.Item2.SetResult();
            Console.WriteLine("Task done: " + _mainTaskRunner.Count);
        }
    }
}