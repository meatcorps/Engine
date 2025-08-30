using System.Numerics;

namespace Meatcorps.Engine.Core.Tween;

public class ArcMoveToTarget : MoveToTarget
{
    public float ArcHeight = 1.0f;

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