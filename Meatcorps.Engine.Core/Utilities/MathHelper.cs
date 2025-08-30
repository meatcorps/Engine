using System.Numerics;

namespace Meatcorps.Engine.Core.Utilities;

public static class MathHelper
{
    public static float ToDegrees(float radians) => radians * (180f / MathF.PI);
    public static float ToRadians(float degrees) => degrees * (MathF.PI / 180f);

    public static float RandomizedPhase()
    {
        // Full sine wave cycle random offset (0..Tau)
        return Random.Shared.NextSingle() * MathF.Tau;
    }

    /// <summary>Clamp a value between min and max without branching surprises.</summary>
    public static float Clamp(float value, float min, float max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

    /// <summary>Safe acos by clamping input to [-1,1] to avoid NaNs.</summary>
    public static float SafeAcos(float x)
    {
        var clamped = Clamp(x, -1f, 1f);
        return MathF.Acos(clamped);
    }

    /// <summary>Angle between two vectors (radians). Returns 0 if either is ~zero.</summary>
    public static float AngleBetweenRad(Vector2 a, Vector2 b)
    {
        var la = a.X * a.X + a.Y * a.Y;
        var lb = b.X * b.X + b.Y * b.Y;

        if (la < 1e-12f || lb < 1e-12f)
        {
            return 0f;
        }

        var dot = a.X * b.X + a.Y * b.Y;
        var denom = MathF.Sqrt(la * lb);
        return SafeAcos(dot / denom);
    }

    /// <summary>Deterministic random unit vector using provided RNG.</summary>
    public static Vector2 RandomUnitVector(System.Random rng)
    {
        // Uniform angle in [0, 2π)
        var angle = (float)(rng.NextDouble() * MathF.PI * 2f);
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }

    /// <summary>Random point inside unit circle using rejection sampling.</summary>
    public static Vector2 RandomInsideUnitCircle(System.Random rng)
    {
        while (true)
        {
            var x = (float)(rng.NextDouble() * 2.0 - 1.0);
            var y = (float)(rng.NextDouble() * 2.0 - 1.0);
            var v = new Vector2(x, y);

            if (v.X * v.X + v.Y * v.Y <= 1.0f)
            {
                return v;
            }
        }
    }

    /// <summary>Move current toward target by maxDistanceDelta (like Unity’s MoveTowards).</summary>
    public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta)
    {
        var to = target - current;
        var distSq = to.X * to.X + to.Y * to.Y;

        if (distSq <= maxDistanceDelta * maxDistanceDelta || distSq < 1e-12f)
        {
            return target;
        }

        var inv = maxDistanceDelta / MathF.Sqrt(distSq);
        return new Vector2(current.X + to.X * inv, current.Y + to.Y * inv);
    }
}