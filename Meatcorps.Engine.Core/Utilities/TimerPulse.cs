namespace Meatcorps.Engine.Core.Utilities;

/// <summary>
/// PulseTimer (TP) - When input becomes true, the output becomes true for a fixed time (pulse),
/// regardless of the input afterward. Input must return to false before it can retrigger.
/// </summary>
public class PulseTimer
{
    private readonly float _pulseDuration;
    private float _elapsed;
    private bool _triggered;

    public bool Output { get; private set; }

    public PulseTimer(float durationInMs)
    {
        _pulseDuration = durationInMs / 1000;
    }

    public void Update(bool input, float deltaTime)
    {
        if (!_triggered && input)
        {
            _elapsed = 0;
            Output = true;
            _triggered = true;
        }

        if (_triggered)
        {
            _elapsed += deltaTime;
            if (_elapsed >= _pulseDuration)
            {
                Output = false;
                _triggered = false;
            }
        }
    }

    public void Reset()
    {
        _elapsed = 0;
        Output = false;
        _triggered = false;
    }
}