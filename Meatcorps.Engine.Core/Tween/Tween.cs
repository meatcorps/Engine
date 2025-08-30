using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Utilities;

namespace Meatcorps.Engine.Core.Tween;

public static class Tween
{
    public static float Clamp01(float normal)
        => normal < 0 ? 0 : normal > 1 ? 1 : normal;

    public static float Lerp(float from, float to, float normal)
        => from + (to - from) * Clamp01(normal);

    public static Vector2 Lerp(Vector2 from, Vector2 to, float normal)
        => from + (to - from) * Clamp01(normal);

    public static float NormalToUpDown(float normal)
    {
        if (normal < 0.5f)
            return Math.Min(1, normal * 2f);

        return Math.Max(0, 1 - (normal - 0.5f) * 2);
    }

    public static float DurationBasedOnNormal(float normal, float duration)
    {
        return duration * normal;
    }
    
    /// <summary>
    /// Lerp between two angles (degrees) along the shortest arc.
    /// </summary>
    public static float LerpAngle(float from, float to, float normal)
    {
        var delta = ((to - from + 540f) % 360f) - 180f; // shortest signed delta
        return from + delta * normal;
    }

    /// <summary>
    /// Lerp between two angles (radians) along the shortest arc.
    /// </summary>
    public static float LerpAngleRad(float from, float to, float normal)
    {
        var delta = ((to - from + MathF.PI * 3f) % (MathF.PI * 2f)) - MathF.PI;
        return from + delta * normal;
    }

    /// <summary>
    /// Lerp between two positions in polar coordinates (degrees).
    /// </summary>
    public static Vector2 LerpPolar(Vector2 center, float fromRadius, float fromAngleDeg, float toRadius, float toAngleDeg, float normal)
    {
        var radius = Lerp(fromRadius, toRadius, normal);
        var angle  = LerpAngle(fromAngleDeg, toAngleDeg, normal);
        var rad    = MathHelper.ToRadians(angle);
        return new Vector2(center.X + radius * MathF.Cos(rad),
            center.Y + radius * MathF.Sin(rad));
    }

    /// <summary>
    /// Lerp between two positions in polar coordinates (radians).
    /// </summary>
    public static Vector2 LerpPolarRad(Vector2 center, float fromRadius, float fromAngleRad, float toRadius, float toAngleRad, float normal)
    {
        var radius = Lerp(fromRadius, toRadius, normal);
        var angle  = LerpAngleRad(fromAngleRad, toAngleRad, normal);
        return new Vector2(center.X + radius * MathF.Cos(angle),
            center.Y + radius * MathF.Sin(angle));
    }

    public static float ApplyEasing(float t, EaseType ease)
    {
        t = Clamp01(t);

        return ease switch
        {
            EaseType.Linear => t,

            EaseType.EaseIn => t * t,
            EaseType.EaseOut => 1f - (1f - t) * (1f - t),
            EaseType.EaseInOut => t * t * (3f - 2f * t),

            EaseType.EaseInCubic => t * t * t,
            EaseType.EaseOutCubic => 1f - MathF.Pow(1f - t, 3),
            EaseType.EaseInOutCubic => t < 0.5f
                ? 4f * t * t * t
                : 1f - MathF.Pow(-2f * t + 2f, 3) / 2f,

            EaseType.EaseInQuart => t * t * t * t,
            EaseType.EaseOutQuart => 1f - MathF.Pow(1f - t, 4),
            EaseType.EaseInOutQuart => t < 0.5f
                ? 8f * t * t * t * t
                : 1f - MathF.Pow(-2f * t + 2f, 4) / 2f,

            EaseType.EaseInQuint => t * t * t * t * t,
            EaseType.EaseOutQuint => 1f - MathF.Pow(1f - t, 5),
            EaseType.EaseInOutQuint => t < 0.5f
                ? 16f * t * t * t * t * t
                : 1f - MathF.Pow(-2f * t + 2f, 5) / 2f,

            EaseType.EaseInSine => 1f - MathF.Cos((t * MathF.PI) / 2f),
            EaseType.EaseOutSine => MathF.Sin((t * MathF.PI) / 2f),
            EaseType.EaseInOutSine => -(MathF.Cos(MathF.PI * t) - 1f) / 2f,
            EaseType.EaseInBack => EaseInBack(t),
            EaseType.EaseOutBack => EaseOutBack(t),
            EaseType.EaseInOutBack => EaseInOutBack(t),
            
            EaseType.EaseInBounce => EaseInBounce(t),
            EaseType.EaseOutBounce => EaseOutBounce(t),
            EaseType.EaseInOutBounce => EaseInOutBounce(t),

            EaseType.EaseInElastic => EaseInElastic(t),
            EaseType.EaseOutElastic => EaseOutElastic(t),
            EaseType.EaseInOutElastic => EaseInOutElastic(t),
            _ => t
        };
    }

    private static float EaseInBack(float t)
    {
        var c1 = 1.70158f;
        return c1 * t * t * t - c1 * t * t;
    }

    private static float EaseOutBack(float t)
    {
        var c1 = 1.70158f;
        var c3 = c1 + 1f;
        return 1f + c3 * MathF.Pow(t - 1f, 3) + c1 * MathF.Pow(t - 1f, 2);
    }

    private static float EaseInOutBack(float t)
    {
        var c1 = 1.70158f;
        var c2 = c1 * 1.525f;
        return t < 0.5f
            ? (MathF.Pow(2f * t, 2) * ((c2 + 1f) * 2f * t - c2)) / 2f
            : (MathF.Pow(2f * t - 2f, 2) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
    }
    
    private static float EaseOutBounce(float t)
    {
        if (t < 1f / 2.75f)
            return 7.5625f * t * t;
        if (t < 2f / 2.75f)
        {
            t -= 1.5f / 2.75f;
            return 7.5625f * t * t + 0.75f;
        }
        if (t < 2.5f / 2.75f)
        {
            t -= 2.25f / 2.75f;
            return 7.5625f * t * t + 0.9375f;
        }

        t -= 2.625f / 2.75f;
        return 7.5625f * t * t + 0.984375f;
    }

    private static float EaseInBounce(float t)
    {
        return 1f - EaseOutBounce(1f - t);
    }

    private static float EaseInOutBounce(float t)
    {
        return t < 0.5f
            ? (1f - EaseOutBounce(1f - 2f * t)) / 2f
            : (1f + EaseOutBounce(2f * t - 1f)) / 2f;
    }

    private static float EaseInElastic(float t)
    {
        if (t == 0f || t == 1f) return t;
        return -MathF.Pow(2f, 10f * (t - 1f)) * MathF.Sin((t - 1.075f) * (2f * MathF.PI) / 0.3f);
    }

    private static float EaseOutElastic(float t)
    {
        if (t == 0f || t == 1f) return t;
        return MathF.Pow(2f, -10f * t) * MathF.Sin((t - 0.075f) * (2f * MathF.PI) / 0.3f) + 1f;
    }

    private static float EaseInOutElastic(float t)
    {
        if (t == 0f || t == 1f) return t;

        t *= 2f;
        if (t < 1f)
            return -0.5f * MathF.Pow(2f, 10f * (t - 1f)) * MathF.Sin((t - 1.1125f) * (2f * MathF.PI) / 0.45f);

        return MathF.Pow(2f, -10f * (t - 1f)) * MathF.Sin((t - 1.1125f) * (2f * MathF.PI) / 0.45f) * 0.5f + 1f;
    }

    public static float StepTo(float current, float target, float speed, float deltaTime)
    {
        var step = speed * deltaTime;
        if (Math.Abs(target - current) <= step)
            return target;

        return current + Math.Sign(target - current) * step;
    }

    public static Vector2 StepTo(Vector2 current, Vector2 target, float speed, float deltaTime)
    {
        return new Vector2(StepTo(current.X, target.X, speed, deltaTime),
            StepTo(current.Y, target.Y, speed, deltaTime));
    }
    
    public static float FloatBasedOnVelocity(
        float currentPosition, float deltaTime, 
        float period, float currentTime, float centerPosition, 
        float amplitude = 30f, float phase = 0f, 
        float correctionGain = 0.1f)
    {
        if (deltaTime <= 0f) 
            return 0f;

        var omega = MathF.Tau / period;
        var angle = omega * currentTime + phase;

        var vx = amplitude * omega * MathF.Cos(angle);
        var desiredX = centerPosition + amplitude * MathF.Sin(angle);
        var correction = (desiredX - currentPosition) / deltaTime;

        return vx + correctionGain * correction;
    }
}