using System.Runtime.Loader;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.Core.Server;

public class ServerApplication
{
    public bool Running;
    private readonly TaskCompletionSource _tcs = new();
    
    public ServerApplication()
    {
        Running = true;
        GlobalObjectManager.ObjectManager.Register(this);
        
        Console.CancelKeyPress += (_, _) =>
        {
            _tcs.TrySetResult();
        };

        AssemblyLoadContext.Default.Unloading += _ =>
        {
            _tcs.TrySetResult();
        };

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            _tcs.TrySetResult();
        };
    }

    public async Task Run()
    {
        await _tcs.Task;
        Running = false;
        GlobalObjectManager.ObjectManager.Dispose();
    }
}