using System.Numerics;
using Meatcorps.Engine.Collision.Interfaces;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Collision.Utilities;

public static class GridMovement
{
    public static void TryMove(
        IBody body,
        Vector2 desiredVelocity,
        ref Vector2 lastVelocity,
        float deltaTime,
        uint collisionMask,
        float roundingRatio)
    {
        var total = 0;
        var testPosition = body.Position;
               
        if (!lastVelocity.Abs().IsEqualsSafe(desiredVelocity.Abs()))
        {
            testPosition.X = MathF.Round(testPosition.X / roundingRatio) * roundingRatio;
            testPosition.Y = MathF.Round(testPosition.Y / roundingRatio) * roundingRatio;
            testPosition += desiredVelocity * deltaTime;
            foreach (var item in body.WorldService.QueryContacts(body, testPosition, collisionMask))
                total++;
        }

        if (total == 0)
        {
            if (!desiredVelocity.IsEqualsSafe(Vector2.Zero))
            {
                lastVelocity = desiredVelocity;
                body.Position = testPosition;
                body.Velocity = desiredVelocity;
            }
            else
            {
                body.Position = testPosition;
                body.Velocity = lastVelocity;
            }
        }
    }
}