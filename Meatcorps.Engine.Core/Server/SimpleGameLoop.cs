using System.Diagnostics;
using Meatcorps.Engine.Core.Interfaces.Services;
using Meatcorps.Engine.Core.ObjectManager;

namespace Meatcorps.Engine.Core.Server;

public class SimpleGameLoop: IDisposable
{
    private Timer _timer;
    private Stopwatch _sw = new();
    private List<IBackgroundService> _services = new();
    private bool _runningUpdate = false;

    private void Update(object? state)
    {
        if (_runningUpdate) 
            return;
        
        _runningUpdate = true;
        var deltaTime = _sw.ElapsedMilliseconds / 1000f;

        if (deltaTime < 0.001f)
        {
            _runningUpdate = false;
            return;
        }

        deltaTime = MathF.Min(deltaTime, 0.1f);

        _sw.Restart();
        try
        {
            foreach (var service in _services)
                service.PreUpdate(deltaTime);

            foreach (var service in _services)
                service.Update(deltaTime);

            foreach (var service in _services)
                service.LateUpdate(deltaTime);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error happend " + e.ToString());
        }
        finally
        {
            _runningUpdate = false;
        }
    }

    public SimpleGameLoop Start()
    {
        _services = GlobalObjectManager.ObjectManager.GetList<IBackgroundService>()!;
        _sw.Start();
        _timer = new Timer(Update, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(1000f / 60f));
        return this;
    }
    
    public void Dispose()
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        _timer.Dispose();
    }
}