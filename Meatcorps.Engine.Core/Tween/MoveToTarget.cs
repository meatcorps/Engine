using System.Numerics;

namespace Meatcorps.Engine.Core.Tween;

public class MoveToTarget
{
    public Vector2 StartPosition { get; protected set; }
    public Vector2 TargetPosition { get; protected set; }
    public Vector2 Position { get; protected set; }
    public float Speed { get; set; } // Units per second
    public bool Done { get; private set; }
    public float DistanceRemaining { get; private set; }

    private Vector2 _direction;

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