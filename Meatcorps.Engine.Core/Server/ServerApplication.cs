using System.Runtime.Loader;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.Core.Server;

public class ServerApplication
{
    public bool Running;
    public CancellationToken Active;
    
    private CancellationTokenSource _cts = new();
    private TaskCompletionSource _tcs = new();
    
    public ServerApplication()
    {
        GlobalObjectManager.ObjectManager.Register(this);
        
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            _tcs.TrySetResult();
        };

        AssemblyLoadContext.Default.Unloading += context =>
        {
            _tcs.TrySetResult();
        };

        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            _tcs.TrySetResult();
        };
    }

    public async Task Run()
    {
        Running = true;
        await _tcs.Task;
        Running = false;
        GlobalObjectManager.ObjectManager.Dispose();
    }
}