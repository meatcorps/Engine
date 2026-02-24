using System.Numerics;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Utilities;

/// <summary>
/// Holds the last non-zero direction for a configurable window after input goes to zero. Useful for forgiving directional input — prevents direction loss on brief input gaps (e.g. releasing a key a few frames before a grid snap).
/// </summary>
public class BufferedDirection
{
    private TimerOn _bufferTimer { get; }

    /// <summary>The current effective direction, held for the buffer window after input returns to zero.</summary>
    public Vector2 Direction { get; private set; }

    /// <summary>true if Direction is non-zero.</summary>
    public bool IsGoingTowards => !Direction.IsEqualsSafe(Vector2.Zero);
    private Vector2 _wantDirection = Vector2.Zero;

    public BufferedDirection(float bufferTime)
    {
        _bufferTimer = new TimerOn(bufferTime);
    }

    public void Update(Vector2 raw, float deltaTime)
    {
        if (!raw.IsEqualsSafe(Vector2.Zero))
            _wantDirection = raw;
        else
            raw = _wantDirection;

        _bufferTimer.Update(!_wantDirection.IsEqualsSafe(Vector2.Zero), deltaTime);

        if (_bufferTimer.Output)
            _wantDirection = Vector2.Zero;

        Direction = raw;
    }

    /// <summary>Returns true if the direction changed relative to velocity AND is non-zero. Outputs the direction scaled by speed.</summary>
    public bool IsDirectionChangedAndIsNotZero(Vector2 velocity, float speed, out Vector2 directionWithSpeed)
    {
        var result = IsDirectionChanged(velocity, speed, out directionWithSpeed);
        return !Direction.IsEqualsSafe(Vector2.Zero) && result;
    }

    /// <summary>Returns true if the direction changed relative to velocity. Outputs the direction scaled by speed.</summary>
    public bool IsDirectionChanged(Vector2 velocity, float speed, out Vector2 directionWithSpeed)
    {
        directionWithSpeed = Direction * speed;
        return !Direction.NormalizedCopy().IsEqualsSafe(velocity.NormalizedCopy());
    }
}