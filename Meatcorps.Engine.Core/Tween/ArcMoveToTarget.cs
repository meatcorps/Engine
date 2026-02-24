using System.Numerics;

namespace Meatcorps.Engine.Core.Tween;

/// <summary>
/// Extends MoveToTarget with a parabolic arc offset on the Y axis. The arc peaks at the midpoint of the journey with a height of ArcHeight units above the straight-line path.
/// </summary>
public class ArcMoveToTarget : MoveToTarget
{
    /// <summary>Peak height of the parabolic arc above the straight line between start and target, in world units.</summary>
    public readonly float ArcHeight = 1.0f;

    public new void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (!Done)
        {
            // Apply simple vertical arc (parabola) on Y
            var progress = Vector2.Distance(StartPosition, Position) / Vector2.Distance(StartPosition, TargetPosition);
            var heightOffset = ArcHeight * 4 * progress * (1 - progress); // peak at 0.5
            Position = new Vector2(Position.X, StartPosition.Y + (TargetPosition.Y - StartPosition.Y) * progress - heightOffset);
        }
    }
}