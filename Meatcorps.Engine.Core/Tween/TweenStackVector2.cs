using System.Numerics;
using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Tween;

public class TweenStackVector2
{
    private readonly TweenStack _stackX = new();
    private readonly TweenStack _stackY = new();
    
    public TweenStackVector2 Register(EaseType easeType, float normalOffset, float normalDuration)
    {
        _stackX.Register(easeType, normalOffset, normalDuration);
        _stackY.Register(easeType, normalOffset, normalDuration);
        return this;   
    }
    
    public TweenStackVector2 Register(EaseType easeType, float duration, float totalDuration, float durationOffset)
    {
        _stackX.Register(easeType, duration, totalDuration, durationOffset);
        _stackY.Register(easeType, duration, totalDuration, durationOffset);
        return this;   
    }
    
    public TweenStackVector2 Register(
        EaseType easeX, EaseType easeY,
        float normalOffset, float normalDuration)
    {
        _stackX.Register(easeX, normalOffset, normalDuration);
        _stackY.Register(easeY, normalOffset, normalDuration);
        return this;
    }
    
    public TweenStackVector2 Assign(Vector2 from, Vector2 to)
    {
        _stackX.Assign(from.X, to.X);
        _stackY.Assign(from.Y, to.Y);
        return this;  
    }
    
    public TweenStackVector2 AssignFromValue(Vector2 value)
    {
        _stackX.AssignFromValue(value.X);
        _stackY.AssignFromValue(value.Y);
        return this;  
    }
    
    public TweenStackVector2 AssignToValue(Vector2 value)
    {
        _stackX.AssignToValue(value.X);
        _stackY.AssignToValue(value.Y);
        return this;
    }
    
    public TweenStackVector2 FromKeyframes(params (float at, Vector2 value, EaseType ease)[] keys)
    {
        if (keys.Length < 2) throw new ArgumentException("Need at least two keyframes");
        _stackX.AssignFromValue(keys[0].value.X);
        _stackY.AssignFromValue(keys[0].value.Y);

        for (var i = 0; i < keys.Length - 1; i++)
        {
            var from = keys[i];
            var to   = keys[i + 1];
            var duration = MathF.Max(1e-6f, to.at - from.at);
            Register(to.ease, from.at, duration);
            AssignToValue(to.value);
        }
        return this;
    }

    public TweenStackVector2 FromDurationInMilliseconds(
        Vector2 startValue,
        float totalDurationInMilliseconds,
        params (float durationInMilliseconds, Vector2 value, EaseType ease)[] keys)
    {
        if (totalDurationInMilliseconds <= 0f)
            throw new ArgumentOutOfRangeException(nameof(totalDurationInMilliseconds), "Total duration must be > 0.");

        var normalized = new (float at, Vector2 value, EaseType ease)[keys.Length + 1];

        normalized[0].at = 0f;
        normalized[0].value = startValue;
        normalized[0].ease = EaseType.Linear;

        var currentOffset = 0f;

        for (var i = 0; i < keys.Length; i++)
        {
            var segment = keys[i];
            if (segment.durationInMilliseconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(keys), "Duration cannot be negative.");

            var normalizedDuration = segment.durationInMilliseconds / totalDurationInMilliseconds;
            currentOffset += normalizedDuration;

            normalized[i + 1].at = currentOffset;
            normalized[i + 1].value = segment.value;
            normalized[i + 1].ease = segment.ease;
        }

        if (!currentOffset.EqualsSafe(1f))
            throw new ArgumentException("Total duration and keyframes duration mismatch.");

        return FromKeyframes(normalized);
    }

    public Vector2 Lerp(float normal)
    {
        return new Vector2(_stackX.Lerp(normal), _stackY.Lerp(normal));
    } 
}
