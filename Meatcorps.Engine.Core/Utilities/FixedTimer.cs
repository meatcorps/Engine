namespace Meatcorps.Engine.Core.Utilities;

/// <summary>
/// A repeating interval timer. Output pulses true for one tick each time elapsed time exceeds the duration, then resets automatically. Unlike TimerOn, no boolean input is required — it fires continuously on a fixed cadence.
/// </summary>
public class FixedTimer
{
    private float _pulseDuration;
    private float _elapsed;

    /// <summary>Elapsed time normalized to [0,1] within the current interval.</summary>
    public float NormalizedElapsed => Math.Min(1f, _elapsed / _pulseDuration);

    /// <summary>true for exactly one tick per interval. Use to trigger frame-rate-independent periodic logic.</summary>
    public bool Output { get; private set; }

    /// <summary>The configured interval duration in milliseconds.</summary>
    public float DurationInMs => _pulseDuration;

    /// <summary>The configured interval duration in seconds.</summary>
    public float DurationInS => _pulseDuration / 1000;

    public FixedTimer(float durationInMs)
    {
        _pulseDuration = durationInMs;
    }

    /// <summary>Changes the interval duration without resetting elapsed time.</summary>
    public void ChangeSpeed(float newDurationInMs)
    {
        _pulseDuration = newDurationInMs;
    }

    public void Update(float deltaTime)
    {
        _elapsed += deltaTime * 1000;
        Output = false;
        if (_elapsed >= _pulseDuration)
        {
            Output = true;
            _elapsed = 0;
        }
    }
}