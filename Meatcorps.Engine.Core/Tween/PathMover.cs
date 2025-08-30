using System.Numerics;

namespace Meatcorps.Engine.Core.Tween;

public class PathMover
{
    public Vector2 Position { get; private set; }
    public float Speed { get; set; } // Units per second
    public bool Loop { get; set; }
    public bool Done { get; private set; }

    private readonly Queue<Vector2> _waypoints = new();
    private Vector2 _currentTarget;
    private Vector2 _direction;
    public float DistanceRemaining { get; private set; }

    public void Start(Vector2 start, IEnumerable<Vector2> path, float speed, bool loop = false)
    {
        _waypoints.Clear();
        foreach (var point in path)
            _waypoints.Enqueue(point);

        Position = start;
        Speed = speed;
        Loop = loop;
        Done = false;

        MoveToNext();
    }

    public void Update(float deltaTime)
    {
        if (Done || Speed <= 0f || DistanceRemaining <= 0f)
            return;

        var moveAmount = Speed * deltaTime;

        if (moveAmount >= DistanceRemaining)
        {
            Position = _currentTarget;
            MoveToNext();
        }
        else
        {
            Position += _direction * moveAmount;
            DistanceRemaining -= moveAmount;
        }
    }

    private void MoveToNext()
    {
        if (_waypoints.Count == 0)
        {
            Done = true;
            return;
        }

        var next = _waypoints.Dequeue();

        if (Loop)
            _waypoints.Enqueue(next);

        _currentTarget = next;
        _direction = Vector2.Normalize(_currentTarget - Position);
        DistanceRemaining = Vector2.Distance(Position, _currentTarget);
    }
}