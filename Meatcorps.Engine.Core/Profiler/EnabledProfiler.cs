using System.Text;
using Meatcorps.Engine.Core.Interfaces.Profiler;
using Meatcorps.Engine.Core.Utilities;

namespace Meatcorps.Engine.Core.Profiler;

public class EnabledProfiler : IProfiler
{
    private Dictionary<string, FrameTimer> _timers = new Dictionary<string, FrameTimer>();
    private StringBuilder _sb = new StringBuilder();
    private const string DOT = ".";
    
    public FrameTimer.ScopedScope StartProfile(Type sender, string name, Type? childType = null)
    {
        _sb.Clear();
        _sb.Append(sender.Name);
        _sb.Append(DOT);
        if (childType != null)
        {
            _sb.Append(childType.Name);
            _sb.Append(DOT);
        }

        _sb.Append(name);
        
        return GetTimer(_sb.ToString()).Scope();
    }

    private FrameTimer GetTimer(string name)
    {
        if (!_timers.ContainsKey(name))
            _timers.Add(name, new FrameTimer());

        return _timers[name];
    }

    public IEnumerable<(string, FrameTimer)> GetTimers()
    {
        foreach (var timer in _timers)
            yield return (timer.Key, timer.Value);
    }

    public void ClearTimers()
    {
        _timers.Clear();
    }
}