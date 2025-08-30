namespace Meatcorps.Engine.Core.Utilities;

public class FixedTimer
{
    private float _pulseDuration;
    private float _elapsed;
    public float NormalizedElapsed => Math.Min(1f, _elapsed / _pulseDuration);
    
    public bool Output { get; private set; }
    public float DurationInMs => _pulseDuration;
    public float DurationInS => _pulseDuration / 1000;

    public FixedTimer(float durationInMs)
    {
        _pulseDuration = durationInMs;
    }

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