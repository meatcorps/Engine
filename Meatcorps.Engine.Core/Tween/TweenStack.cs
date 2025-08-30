using Meatcorps.Engine.Core.Enums;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Tween;

public class TweenStack
{
    private readonly List<TweenStackEntry> _stack = new();
    private readonly List<float> _values = new();
    
    public TweenStack Register(TweenStackEntry entry)
    {
        _stack.Add(entry);
        return this;   
    }
    
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
    
    public TweenStack AssignFromValue(float value)
    {
        _values.Clear();
        _values.Add(value);
        return this;  
    }
    
    public TweenStack Assign(float from, float to)
    {
        AssignFromValue(from);
        AssignToValue(to);
        return this;  
    }

    
    public TweenStack AssignToValue(float value)
    {
        if (_values.Count == 0)
            throw new InvalidOperationException("AssignFromValue must be called first.");

        _values.Add(value);
        return this;
    }
    
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

public struct TweenStackEntry
{
    public float NormalOffset = 0;
    public float NormalDuration = 1;
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

    public bool InBounds(float globalNormal)
    {
        return globalNormal.Between(NormalOffset, NormalOffset + NormalDuration);
    }

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