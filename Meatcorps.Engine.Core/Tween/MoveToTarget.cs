using System.Numerics;

namespace Meatcorps.Engine.Core.Tween;

/// <summary>
/// Moves a position from start to target at a constant speed in units per second. Poll Position and Done each tick after calling Update.
/// </summary>
public class MoveToTarget
{
    public Vector2 StartPosition { get; protected set; }
    public Vector2 TargetPosition { get; protected set; }
    public Vector2 Position { get; protected set; }
    public float Speed { get; set; } // Units per second

    /// <summary>true once Position has reached TargetPosition.</summary>
    public bool Done { get; private set; }

    /// <summary>Remaining distance to the target in world units.</summary>
    public float DistanceRemaining { get; private set; }

    private Vector2 _direction;

    /// <summary>Initializes the movement. Resets Done and begins advancing Position toward to at the given speed.</summary>
    public void Start(Vector2 from, Vector2 to, float speed)
    {
        StartPosition = from;
        TargetPosition = to;
        Speed = speed;
        Position = from;
        Done = false;

        _direction = Vector2.Normalize(to - from);
        DistanceRemaining = Vector2.Distance(from, to);
    }

    public void Update(float deltaTime)
    {
        if (Done || Speed <= 0f) return;

        float moveAmount = Speed * deltaTime;

        if (moveAmount >= DistanceRemaining)
        {
            Position = TargetPosition;
            Done = true;
        }
        else
        {
            Position += _direction * moveAmount;
            DistanceRemaining -= moveAmount;
        }
    }
}