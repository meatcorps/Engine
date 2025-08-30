using Meatcorps.Engine.Core.Interfaces.Input;

namespace Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;

public class FlashAnimation : IButtonAnimation
{
    private readonly float _speedInMs;
    private readonly bool _selfTerminate;
    private DateTimeOffset _start;

    public FlashAnimation(long speedInMs, bool selfTerminate = true)
    {
        _speedInMs = speedInMs;
        _selfTerminate = selfTerminate;
        _start = DateTimeOffset.Now;
    }

    public bool Update(IInput input)
    {
        var elapsed = DateTimeOffset.Now - _start;

        if (elapsed < TimeSpan.FromMilliseconds(_speedInMs))
            return true;

        if (_selfTerminate)
            input.Animation = null;

        return false;
    }

    public void Reset()
    {
        _start = DateTimeOffset.Now;
    }
}