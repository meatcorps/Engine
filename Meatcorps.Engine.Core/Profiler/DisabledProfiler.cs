using Meatcorps.Engine.Core.Interfaces.Profiler;
using Meatcorps.Engine.Core.Utilities;

namespace Meatcorps.Engine.Core.Profiler;

public class DisabledProfiler: IProfiler
{
    private static readonly Action<long, long> _noop = (_, __) => { };
    private FrameTimer.ScopedScope _return = new FrameTimer.ScopedScope(_noop, 0, 0);


    public FrameTimer.ScopedScope StartProfile(Type sender, string name, Type? childType = null)
    {
        return _return;
    }

    public IEnumerable<(string, FrameTimer)> GetTimers()
    {
        return Array.Empty<(string, FrameTimer)>();
    }

    public void ClearTimers()
    {
        // Nop
    }
}