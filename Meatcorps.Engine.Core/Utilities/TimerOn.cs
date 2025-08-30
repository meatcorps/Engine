namespace Meatcorps.Engine.Core.Utilities;

/// <summary>
/// TimerOn (TON) - Starts timing when input becomes true.
/// The output becomes true only after the specified delay has elapsed while the input remains true.
/// </summary>
public class TimerOn
{
    private readonly float _delay;
    private float _elapsed;
    public float Elapsed => _elapsed;
    public float TotalTime => _delay;
    public float TimeRemaining => Math.Max(0, _delay - _elapsed);
    public float NormalizedElapsed => Math.Min(1f, _elapsed / _delay);
    private bool _active;

    public bool Output { get; private set; }

    public TimerOn(float delayInMs)
    {
        _delay = delayInMs;
    }

    public void Update(bool input, float deltaTime)
    {
        if (input)
        {
            _elapsed += deltaTime * 1000;
            if (_elapsed >= _delay)
                Output = true;
        }
        else
        {
            _elapsed = 0;
            Output = false;
        }

        _active = input;
    }

    public void Reset()
    {
        _elapsed = 0;
        Output = false;
    }
}