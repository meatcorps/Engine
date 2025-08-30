using System.Numerics;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Utilities;

public class BufferedDirection
{
    private TimerOn _bufferTimer { get; }
    public Vector2 Direction { get; private set; }
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

    public bool IsDirectionChangedAndIsNotZero(Vector2 velocity, float speed, out Vector2 directionWithSpeed)
    {
        var result = IsDirectionChanged(velocity, speed, out directionWithSpeed);
        return !Direction.IsEqualsSafe(Vector2.Zero) && result;
    }
    
    public bool IsDirectionChanged(Vector2 velocity, float speed, out Vector2 directionWithSpeed)
    {
        directionWithSpeed = Direction * speed;
        return !Direction.NormalizedCopy().IsEqualsSafe(velocity.NormalizedCopy());
    }
}