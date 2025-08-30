using System.Numerics;
using Meatcorps.Engine.Boids.Data;
using Meatcorps.Engine.Boids.Interfaces;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Boids.Utilities;

public static class BoidBehaviors
{
    public static Vector2 Seek(Vector2 position, Vector2 velocity, Vector2 target, float maxSpeed)
    {
        var desired = (target - position).NormalizedSafe();
        return desired * maxSpeed - velocity;
    }

    public static Vector2 Arrive(Vector2 position, Vector2 velocity, Vector2 target, float slowRadius, float maxSpeed)
    {
        var toTarget = target - position;
        var distSq = toTarget.LengthSquared();
        if (distSq < 1e-8f)
        {
            return Vector2.Zero;
        }

        var dist = MathF.Sqrt(distSq);
        var ramped = maxSpeed * (dist / MathF.Max(slowRadius, 1e-6f));
        var clipped = MathF.Min(ramped, maxSpeed);
        var desired = toTarget * (clipped / dist);
        return desired - velocity;
    }

    public static Vector2 Separation(Vector2 selfPosition, IEnumerable<IBoidAgent> neighbors, float desiredSeparation, float maxSpeed)
    {
        var steer = Vector2.Zero;
        var count = 0;

        var desiredSq = desiredSeparation * desiredSeparation;
        foreach (var n in neighbors)
        {
            var dSq = selfPosition.DistanceSquared(n.Position);
            if (dSq > 0 && dSq < desiredSq)
            {
                // push away inversely proportional to distance
                var away = (selfPosition - n.Position);
                var inv = 1.0f / MathF.Sqrt(dSq);
                steer += away * inv;
                count++;
            }
        }

        if (count > 0)
        {
            steer /= count;
        }

        if (steer.LengthSquared() > 0)
        {
            steer = steer.NormalizedSafe() * maxSpeed;
        }

        return steer;
    }

    public static Vector2 Alignment(Vector2 selfPosition, IEnumerable<IBoidAgent> neighbors, float neighborRadius, float maxSpeed)
    {
        var sumVel = Vector2.Zero;
        var count = 0;
        var rSq = neighborRadius * neighborRadius;

        foreach (var n in neighbors)
        {
            var dSq = selfPosition.DistanceSquared(n.Position);
            if (dSq <= rSq)
            {
                sumVel += n.Velocity;
                count++;
            }
        }

        if (count == 0)
        {
            return Vector2.Zero;
        }

        var avg = sumVel / count;
        if (avg.LengthSquared() > 0)
        {
            avg = avg.NormalizedSafe() * maxSpeed;
        }

        return avg;
    }

    public static Vector2 Cohesion(Vector2 selfPosition, IEnumerable<IBoidAgent> neighbors, float neighborRadius, float maxSpeed)
    {
        var center = Vector2.Zero;
        var count = 0;
        var rSq = neighborRadius * neighborRadius;

        foreach (var n in neighbors)
        {
            var dSq = selfPosition.DistanceSquared(n.Position);
            if (dSq <= rSq)
            {
                center += n.Position;
                count++;
            }
        }

        if (count == 0)
        {
            return Vector2.Zero;
        }

        center /= count;
        return Seek(selfPosition, Vector2.Zero, center, maxSpeed); // desired velocity towards center
    }

    /// <summary>
    /// Small random perturbation that nudges the velocity direction.
    /// Use timeSeconds or a seeded RNG outside if you want strict determinism.
    /// </summary>
    public static Vector2 Wander(Vector2 velocity, float jitterAmount, float timeSeconds)
    {
        // simple, fast hash from time -> angle delta
        var t = (timeSeconds * 123.4567f);
        var s = MathF.Sin(t) + MathF.Sin(t * 0.7f + 1.234f) * 0.5f;
        var c = MathF.Cos(t * 1.3f + 0.918f);

        var jitterDir = new Vector2(c, s).NormalizedSafe(Vector2.UnitX);
        // scale relative to current speed (keeps very slow agents from flipping around abruptly)
        var baseDir = velocity.LengthSquared() > 0 ? velocity.NormalizedSafe() : Vector2.UnitX;

        // blend base direction with a small random offset
        var blended = (baseDir * 0.85f) + (jitterDir * 0.15f * jitterAmount);
        return blended.NormalizedSafe();
    }

    /// <summary>
    /// Weight and clamp the combined steering to MaxForce. Returns an acceleration vector.
    /// </summary>
    public static Vector2 BlendAndClamp(
        in Vector2 seek,
        in Vector2 separation,
        in Vector2 alignment,
        in Vector2 cohesion,
        in Vector2 wander,
        BoidConfig cfg)
    {
        var accel =
            seek        * cfg.WeightSeek +
            separation  * cfg.WeightSeparation +
            alignment   * cfg.WeightAlignment +
            cohesion    * cfg.WeightCohesion +
            wander      * cfg.WeightWander;

        return accel.LimitMagnitude(cfg.MaxForce);
    }
}