using Meatcorps.Engine.Core.Enums;
using System.Numerics;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Tween;

public sealed class TweenStackPolar
{
    private readonly TweenStack _radius = new();
    private readonly TweenStack _angleDeg = new();
    private Vector2 _center;

    public TweenStackPolar WithCenter(Vector2 center) { _center = center; return this; }

    public TweenStackPolar AssignRadius(float from, float to)
    {
        _radius.Assign(from, to);
        return this;
    }

    public TweenStackPolar AssignAngleDeg(float fromDeg, float toDeg)
    {
        _angleDeg.Assign(fromDeg, toDeg);
        return this;
    }

    public TweenStackPolar RegisterRadius(EaseType ease, float normalOffset, float normalDuration)
    {
        _radius.Register(ease, normalOffset, normalDuration);
        return this;
    }

    public TweenStackPolar RegisterAngle(EaseType ease, float normalOffset, float normalDuration)
    {
        _angleDeg.Register(new TweenStackEntry(
            (local, from, to) => Tween.LerpAngle(from, to, Tween.ApplyEasing(local, ease)),
            normalOffset, normalDuration));
        return this;
    }
    
    public TweenStackPolar FromKeyframes(params (float at, float radius, float angleDeg, EaseType ease)[] keys)
    {
        if (keys.Length < 2) throw new ArgumentException("Need at least two keyframes");
        _radius.AssignFromValue(keys[0].radius);
        _angleDeg.AssignFromValue(keys[0].angleDeg);

        for (var i = 0; i < keys.Length - 1; i++)
        {
            var from = keys[i];
            var to   = keys[i + 1];
            var duration = MathF.Max(1e-6f, to.at - from.at);
            RegisterRadius(to.ease, from.at, duration);
            RegisterAngle(to.ease, from.at, duration);
            AssignRadius(from.radius, to.radius);
            AssignAngleDeg(from.angleDeg, to.angleDeg);
        }
        return this;
    }

    public TweenStackPolar FromDurationInMilliseconds(
        float totalDurationInMilliseconds,
        params (float durationInMilliseconds, float radius, float angleDeg, EaseType ease)[] keys)
    {
        if (totalDurationInMilliseconds <= 0f)
            throw new ArgumentOutOfRangeException(nameof(totalDurationInMilliseconds), "Total duration must be > 0.");
        if (keys == null || keys.Length == 0)
            throw new ArgumentException("At least one key is required.", nameof(keys));

        var normalized = new (float at, float radius, float angleDeg, EaseType ease)[keys.Length];

        var currentOffset = 0f;
        for (var i = 0; i < keys.Length; i++)
        {
            var k = keys[i];
            if (k.durationInMilliseconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(keys), "Duration cannot be negative.");

            var segmentFrac = k.durationInMilliseconds / totalDurationInMilliseconds;
            currentOffset += segmentFrac;

            normalized[i].at = currentOffset; // cumulative normalized time
            normalized[i].radius = k.radius;
            normalized[i].angleDeg = k.angleDeg;
            normalized[i].ease = k.ease;
        }

        if (!currentOffset.EqualsSafe(1f))
            throw new ArgumentException("Total duration and keyframe durations mismatch.");

        return FromKeyframes(normalized);
    }

    public Vector2 Lerp(float normal)
    {
        var r = _radius.Lerp(normal);
        var deg = _angleDeg.Lerp(normal);
        var rad = MathF.PI / 180f * deg;
        return _center + new Vector2(r * MathF.Cos(rad), r * MathF.Sin(rad));
    }
}