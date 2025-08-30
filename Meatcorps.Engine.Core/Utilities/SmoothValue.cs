using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Utilities;

/// <summary>
/// Smoothly transitions the DisplayValue towards the RealValue over time,
/// using a constant speed (units per second).
/// </summary>
public class SmoothValue
{
    /// <summary>
    /// The current value shown to the user. Moves toward RealValue.
    /// </summary>
    public float DisplayValue { get; private set; }

    /// <summary>
    /// The actual value to reach. You can set this at any time.
    /// </summary>
    public float RealValue { get; set; }

    private readonly float _speed;
    private readonly bool _fixedTime;
    private float _startValue;
    private bool _snapToReal;
    private FixedTimer _timer;

    /// <param name="initialValue">Optional starting value</param>
    /// <param name="speed">Speed in units per second</param>
    /// <param name="fixedTime">If true it reach the display value always within the speed in seconds</param>
    public SmoothValue(float initialValue = 0f, float speed = 1f, bool fixedTime = true)
    {
        DisplayValue = RealValue = initialValue;
        _speed = speed;
        _fixedTime = fixedTime;
        _timer = new FixedTimer(speed * 1000);
    }

    public bool IsAtRealValue => DisplayValue.EqualsSafe(RealValue);
    
    /// <summary>
    /// Call this once per frame to move DisplayValue toward RealValue.
    /// </summary>
    public void Update(float deltaTime)
    {
        if (DisplayValue.EqualsSafe(RealValue, 0.01f))
        {
            DisplayValue = RealValue; // snap if close
            _snapToReal = false;
            return;
        }
        
        if (_fixedTime)
        {
            _timer.Update(deltaTime);

            if (_timer.Output)
            {
                DisplayValue = RealValue;
                _startValue = DisplayValue;
                _snapToReal = false;
            }

            DisplayValue = Tween.Tween.Lerp(_startValue, RealValue, _timer.NormalizedElapsed);
            return;
        }

        var delta = _speed * deltaTime;
        if (DisplayValue < RealValue)
            DisplayValue = Math.Min(DisplayValue + delta, RealValue);
        else
            DisplayValue = Math.Max(DisplayValue - delta, RealValue);
    }

    public void SnapToReal()
    {
        DisplayValue = RealValue;
    }
}