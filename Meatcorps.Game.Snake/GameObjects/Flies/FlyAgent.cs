using System.Numerics;
using Meatcorps.Engine.Boids.Interfaces;
using Meatcorps.Engine.Core.Data;
using Meatcorps.Engine.Core.Interfaces.Grid;
using Meatcorps.Engine.Core.Utilities;
using Raylib_cs;

namespace Meatcorps.Game.Snake.GameObjects.Flies;

public class FlyAgent : IBoidAgent, IGridItem
{
    // IBoidAgent
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Radius { get; init; } = 6f;
    public float Mass { get; init; } = 1f;
    public bool IsActive { get; set; } = true;
    public int CurrentAnimationFrame { get; set; } = 0;
    private readonly FixedTimer _animationTimer;

    // IGridItem
    public RectF BoundingBox 
        => new(Position.X - Radius, Position.Y - Radius, Radius * 2, Radius * 2);

    public object Owner { get; }

    public FlyAgent(object parent, Vector2 startPosition)
    {
        Owner = parent;
        Position = startPosition;
        Velocity = Vector2.Zero;
        _animationTimer = new FixedTimer(Raylib.GetRandomValue(80, 100));
    }

    public void Update(float deltaTime)
    {
        _animationTimer.Update(deltaTime);
        CurrentAnimationFrame = (int)(_animationTimer.NormalizedElapsed * 2);
    }
}