namespace Meatcorps.Engine.Core.Utilities;

/// <summary>
/// TimerOff (TOF) - When input becomes false, output remains true for the duration of the delay.
/// </summary>
public class TimerOff
{
    private readonly float _delay;
    private float _elapsed;
    public float Elapsed => _elapsed;
    public float TotalTime => _delay;
    public float TimeRemaining => _delay - _elapsed;
    private bool _active;

    public bool Output { get; private set; }

    public TimerOff(float delayInMs)
    {
        _delay = delayInMs;
    }

    public void Update(bool input, float deltaTime)
    {
        if (input)
        {
            Output = true;
            _elapsed = 0;
        }
        else if (Output)
        {
            _elapsed += deltaTime * 1000;
            if (_elapsed >= _delay)
                Output = false;
        }
    }

    public void Reset()
    {
        _elapsed = 0;
        Output = false;
    }
}