using Meatcorps.Engine.Core.Extensions;
using Meatcorps.Engine.Core.Utilities;
using Raylib_cs;

namespace Meatcorps.Engine.RayLib.Utilities;

/// <summary>
/// Smoothly transitions the DisplayValue towards the RealValue over time,
/// using a constant speed (units per second).
/// </summary>
public class SmoothColor
{
    private float _speed;

    /// <summary>
    /// The current value shown to the user. Moves toward RealValue.
    /// </summary>
    public Color DisplayValue
    {
        get
        {
            
            return new Color((byte)_redD, (byte)_greenD, (byte)_blueD, (byte)_alphaD);
        }
    }

    /// <summary>
    /// The actual value to reach. You can set this at any time.
    /// </summary>
    public Color RealValue
    {
        get
        {
            
            return new Color((byte)_red, (byte)_green, (byte)_blue, (byte)_alpha);
        }
        set
        {
            _red = (float)value.R;
            _green = (float)value.G;
            _blue = (float)value.B;
            _alpha = (float)value.A;
        }
    }

    private float _red;
    private float _green;
    private float _blue;
    private float _alpha;
    
    private float _redD;
    private float _greenD;
    private float _blueD;
    private float _alphaD;
    
    /// <param name="initialValue">Optional starting value</param>
    /// <param name="speed">Speed in units per second</param>
    /// <param name="fixedTime">If true it reach the display value always within the speed in seconds</param>
    public SmoothColor(Color initialValue, float speed = 1f)
    {
        _speed = speed;
        _red = initialValue.R;
        _green = initialValue.G;
        _blue = initialValue.B;
        _alpha = initialValue.A;
        RealValue = initialValue;
    }

    public bool IsAtRealValue
    {
        get
        {
            var realColor = RealValue;
            var displayColor = DisplayValue;
            return realColor.R == displayColor.R && realColor.G == displayColor.G && realColor.B == displayColor.B && realColor.A == displayColor.A;
        }
    }
    
    /// <summary>
    /// Call this once per frame to move DisplayValue toward RealValue.
    /// </summary>
    public void Update(float deltaTime)
    {
        _redD = Math.Clamp(CalculateWalkTo(_redD, _red, _speed, deltaTime), 0, 256);
        _greenD = Math.Clamp(CalculateWalkTo(_greenD, _green, _speed, deltaTime), 0, 256);
        _blueD = Math.Clamp(CalculateWalkTo(_blueD, _blue, _speed, deltaTime), 0, 256);
        _alphaD = Math.Clamp(CalculateWalkTo(_alphaD, _alpha, _speed, deltaTime), 0, 256);
    }

    private float CalculateWalkTo(float value, float target, float speed, float deltaTime)
    {
        var delta = speed * deltaTime;
        
        if (value < target)
            value = Math.Min(value + delta, target);
        else
            value = Math.Max(value - delta, target);
        
        return value;
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }
}