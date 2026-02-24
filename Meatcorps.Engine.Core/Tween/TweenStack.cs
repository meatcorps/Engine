using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Tween;

/// <summary>
/// A multi-segment tween that interpolates a float value through a sequence of eased sub-segments, each covering a portion of a global [0,1] normal. Build the stack with Register/Assign/FromKeyframes, then call Lerp(globalNormal) to evaluate.
/// </summary>
public class TweenStack
{
    private readonly List<TweenStackEntry> _stack = new();
    private readonly List<float> _values = new();

    /// <summary>Adds a pre-built entry to the stack.</summary>
    public TweenStack Register(TweenStackEntry entry)
    {
        _stack.Add(entry);
        return this;
    }

    /// <summary>Adds a segment using standard Lerp with the given easing, starting at normalOffset and spanning normalDuration.</summary>
    public TweenStack Register(EaseType easeType, float normalOffset, float normalDuration)
    {
        _stack.Add(
            new TweenStackEntry((normal, from, to) =>
                Tween.Lerp(
                    from, to, Tween.ApplyEasing(normal, easeType)),
                normalOffset,
                normalDuration));
        return this;
    }

    /// <summary>Adds a segment using absolute durations (converted to normalized internally).</summary>
    public TweenStack Register(EaseType easeType, float duration, float totalDuration, float durationOffset)
    {
        _stack.Add(
            new TweenStackEntry((normal, from, to) =>
                    Tween.Lerp(
                        from, to, Tween.ApplyEasing(normal, easeType)),
                duration,
                totalDuration,
                durationOffset));
        return this;
    }

    /// <summary>Sets the starting value for the first segment. Must be called before AssignToValue.</summary>
    public TweenStack AssignFromValue(float value)
    {
        _values.Clear();
        _values.Add(value);
        return this;
    }

    /// <summary>Shorthand for AssignFromValue + AssignToValue.</summary>
    public TweenStack Assign(float from, float to)
    {
        AssignFromValue(from);
        AssignToValue(to);
        return this;
    }

    /// <summary>Appends the end value for the next segment.</summary>
    public TweenStack AssignToValue(float value)
    {
        if (_values.Count == 0)
            throw new InvalidOperationException("AssignFromValue must be called first.");

        _values.Add(value);
        return this;
    }

    /// <summary>Builds the stack from an array of (at, value, ease) keyframes sorted by at in [0,1]. Clears any existing stack.</summary>
    public TweenStack FromKeyframes(params (float at, float value, EaseType ease)[] keys)
    {
        if (keys.Length < 2) throw new ArgumentException("Need at least two keyframes");
        _stack.Clear(); _values.Clear();

        // assume keys sorted by 'at' in 0..1
        for (var i = 0; i < keys.Length - 1; i++)
        {
            var from = keys[i];
            var to   = keys[i + 1];
            Register(new TweenStackEntry(
                (t, a, b) => Tween.Lerp(a, b, Tween.ApplyEasing(t, to.ease)),
                normalOffset: from.at,
                normalDuration: MathF.Max(1e-6f, to.at - from.at)));
            if (i == 0) AssignFromValue(from.value);
            AssignToValue(to.value);
        }
        return this;
    }

    /// <summary>Builds the stack from absolute millisecond durations. Total must equal totalDuration.</summary>
    public TweenStack FromDurationInMilliseconds(float startValue, float totalDuration, params (float duration, float value, EaseType ease)[] keys)
    {
        var currentOffset = 0f;
        var normalized = new (float at, float value, EaseType ease)[keys.Length + 1];

        normalized[0].at = 0;
        normalized[0].value = startValue;
        normalized[0].ease = EaseType.Linear;

        for (var i = 0; i < keys.Length; i++)
        {
            if (keys[i].duration < 0)
                throw new ArgumentException("Duration must be positive");

            var duration = keys[i].duration / totalDuration;
            normalized[i + 1].at = duration + currentOffset;
            normalized[i + 1].value = keys[i].value;
            normalized[i + 1].ease = keys[i].ease;
            currentOffset += duration;
        }

        if (!currentOffset.EqualsSafe(1))
            throw new ArgumentException("Total duration and keyframes duration mismatch.");

        return FromKeyframes(normalized);
    }

    /// <summary>Evaluates the stack at the given global normal in [0,1]. Returns the interpolated float value.</summary>
    public float Lerp(float globalNormal)
    {
        if (_values.Count - 1 != _stack.Count)
            throw new InvalidOperationException($"Stack and assigned values length mismatch. Stack: {_stack.Count}, values: {_values.Count}");

        // Before first segment
        if (globalNormal < _stack[0].NormalOffset)
            return _values[0];

        // Find segment
        for (var i = 0; i < _stack.Count; i++)
        {
            if (_stack[i].InBounds(globalNormal) || (i == _stack.Count - 1 && globalNormal >= _stack[i].NormalOffset))
            {
                _stack[i].Lerp(globalNormal, _values[i], _values[i + 1], out var v);
                return v;
            }
        }

        // After last segment
        return _values[^1];
    }
}

/// <summary>
/// A single segment in a TweenStack, defined by a normalized offset and duration within the global [0,1] range and a mutator function that computes the interpolated value.
/// </summary>
public struct TweenStackEntry
{
    /// <summary>Start position within the global [0,1] range.</summary>
    public readonly float NormalOffset = 0;

    /// <summary>Length of this segment within the global [0,1] range.</summary>
    public readonly float NormalDuration = 1;
    public delegate float MutatorEvent(float normal, float from, float to);
    public MutatorEvent Mutator = Tween.Lerp;

    public TweenStackEntry(MutatorEvent mutator, float normalOffset, float normalDuration)
    {
        Mutator = mutator;
        NormalOffset = normalOffset;
        NormalDuration = normalDuration;
    }

    public TweenStackEntry(MutatorEvent mutator, float duration, float totalDuration, float durationOffset)
    {
        Mutator = mutator;
        NormalDuration = duration / totalDuration;
        NormalOffset = durationOffset / totalDuration;
    }

    /// <summary>Returns true if globalNormal falls within this segment's range.</summary>
    public bool InBounds(float globalNormal)
    {
        return globalNormal.Between(NormalOffset, NormalOffset + NormalDuration);
    }

    /// <summary>Evaluates this segment. Returns false if globalNormal is outside the segment.</summary>
    public bool Lerp(float globalNormal, float from, float to, out float value)
    {
        if (!InBounds(globalNormal))
        {
            value = from;
            return false;
        }

        var normal = Tween.Clamp01((globalNormal - NormalOffset) / NormalDuration);
        value = Mutator(normal, from, to);
        //Console.WriteLine($"globalNormal: {globalNormal} NormalOffset: {NormalOffset} NormalDuration: {NormalDuration} Normal: {normal}: {from}->{to}={value}");
        return true;
    }
}