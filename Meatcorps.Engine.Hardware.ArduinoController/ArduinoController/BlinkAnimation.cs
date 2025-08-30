using Meatcorps.Engine.Core.Interfaces.Input;

namespace Meatcorps.Engine.Hardware.ArduinoController.ArduinoController;

public class BlinkAnimation : IButtonAnimation
{
    private readonly float _speedInMs;
    private DateTimeOffset _lastBlink;

    public BlinkAnimation(long speedInMs)
    {
        _speedInMs = speedInMs;
        _lastBlink = DateTimeOffset.Now;
    }
    
    public bool Update(IInput _)
    {
        if (DateTimeOffset.Now - _lastBlink > TimeSpan.FromMilliseconds(_speedInMs * 2))
            _lastBlink = DateTimeOffset.Now;

        return DateTimeOffset.Now - _lastBlink > TimeSpan.FromMilliseconds(_speedInMs);
    }
}