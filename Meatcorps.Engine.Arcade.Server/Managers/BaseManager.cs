using Meatcorps.Engine.Arcade.Data;
using Meatcorps.Engine.Signals.Data;
using Meatcorps.Engine.Signals.Enums;

namespace Meatcorps.Engine.Arcade.Server.Managers;

public abstract class BaseManager: IDisposable
{
    private bool _disposed;
    private SignalValue<ArcadeCentralData, SignalDefault> _dataSignal = new SignalValue<ArcadeCentralData, SignalDefault>(SignalDefault.Internal, nameof(ArcadeCentralData));
    
    protected ArcadeCentralData Data => _dataSignal.Value;

    public void Push()
    {
        _dataSignal.Push();
    }

    protected virtual void OnDispose()
    {
        
    }

    public void Dispose()
    {
        if (_disposed) 
            return;

        OnDispose();
        
        _dataSignal.Dispose();
        _disposed = true;
    }
}