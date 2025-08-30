namespace Meatcorps.Engine.Core.Extensions;

public static class FloatExtensions
{
    public static bool EqualsSafe(this float first, float second, float epsilon = 0.0001f)
    {
        return Math.Abs(first - second) < epsilon;
    }
    
    public static bool Between(this float value, float min, float max)
    {
        return value >= min && value <= max;
    }
    
    public static bool Between01(this float value)
    {
        return Between(value, 0, 1);
    }
    
    public static bool IsZero(this float value)
    {
        return Between(value, -float.Epsilon, float.Epsilon);
    }
    
    /// <summary>Clamp to [0,1]. (Alias; Tween.Clamp01 exists, but this helps in pipelines.)</summary>
    public static float Saturate(this float value)
    {
        if (value < 0f)
        {
            return 0f;
        }

        if (value > 1f)
        {
            return 1f;
        }

        return value;
    }

    /// <summary>Approximate equality with epsilon.</summary>
    public static bool Approximately(this float a, float b, float epsilon = 1e-5f)
    {
        return MathF.Abs(a - b) <= epsilon;
    }
    
    public static float Wrap(this float value, float size)
    {
        if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
        return ((value % size) + size) % size; 
    }
}