using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;
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
    
    /// <summary>
    /// Generates a repeating 0→1→0 triangle wave from free-running time.
    /// That means it goes forward, then back, forever.
    /// </summary>
    public static float PingPong(float time, float period = 1f, float phase = 0f)
    {
        if (period <= 0f) throw new ArgumentOutOfRangeException(nameof(period));
    
        // Map time into [0,2)
        var p = ((time + phase) / period).Wrap(2f);
    
        // Triangle wave: 0..1..0
        return p <= 1f ? p : 2f - p;
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
    
    public static float ExpDecay(float value, float dt, float halfLife)
    {
        if (halfLife <= 0f) return 0f;
        return value * MathF.Pow(0.5f, dt / halfLife);
    }

    public static float DecayToZero(float value, float dt, float halfLife, float min = 1e-3f)
    {
        float v = ExpDecay(value, dt, halfLife);
        return MathF.Abs(v) < min ? 0f : v;
    }
    
    public static float Decelerate(float speed, float decelPerSec, float dt)
    {
        if (decelPerSec <= 0f) return speed;
        float s = MathF.Abs(speed);
        s = MathF.Max(0f, s - decelPerSec * dt);
        return speed >= 0f ? s : -s;
    }
    
    public static float AdvanceWrap01(float rotation01, float speedWrapsPerSec, float dt)
    {
        return (rotation01 + speedWrapsPerSec * dt).Wrap(1f);
    }

// Snap normalized rotation to nearest cell center for N cells
    public static float SnapToCellCenter01(float rotation01, int cellCount)
    {
        if (cellCount <= 0) return rotation01.Wrap(1f);
        float cell = 1f / cellCount;
        return (cell * MathF.Round(rotation01 / cell)).Wrap(1f);
    }
    
    // Exponential reel with friction
    public static void ReelUpdateExp(ref float rotation01, ref float speed, float halfLife, float minSpeed, float dt)
    {
        rotation01 = AdvanceWrap01(rotation01, speed, dt);
        speed = DecayToZero(speed, dt, halfLife, minSpeed);
    }

    // Linear drag version (constant decel), no snapping
    public static void ReelUpdateLinear(ref float rotation01, ref float speed, float decelPerSec, float dt, float minSpeed = 1e-3f)
    {
        rotation01 = AdvanceWrap01(rotation01, speed, dt);
        speed = Decelerate(speed, decelPerSec, dt);
        if (MathF.Abs(speed) < minSpeed) speed = 0f;
    }

    // Exponential reel that snaps to symbol center
    public static void ReelUpdateExpSnap(ref float rotation01, ref float speed, float halfLife, float minSpeed, float dt, int cellCount)
    {
        rotation01 = AdvanceWrap01(rotation01, speed, dt);
        speed = DecayToZero(speed, dt, halfLife, minSpeed);
        if (speed == 0f)
            rotation01 = SnapToCellCenter01(rotation01, cellCount);
    }
}