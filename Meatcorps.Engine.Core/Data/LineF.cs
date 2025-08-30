using System.Numerics;
using Meatcorps.Engine.Core.Extensions;

namespace Meatcorps.Engine.Core.Data;

public struct LineF: IEquatable<LineF>
{
    public Vector2 Start { get; set; }
    public Vector2 End { get; set; }

    public LineF()
    {
        Start = Vector2.Zero;
        End = Vector2.Zero;
    }
    
    public LineF(Vector2 start, Vector2 end)
    {
        Start = start;
        End = end;
    }
    
    public LineF(float x1, float y1, float x2, float y2)
    {
        Start = new Vector2(x1, y1);
        End = new Vector2(x2, y2);
    }
    
    public bool Equals(LineF other)
    {
        return Start.Equals(other.Start) && End.Equals(other.End);
    }

    public override bool Equals(object? obj)
    {
        return obj is LineF other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }

    public float Length
        => Vector2.Distance(Start, End);

    public float LengthSquared
        => Vector2.DistanceSquared(Start, End);

    public Vector2 DirectionStartNormalized
        => Vector2.Normalize(End - Start);
    
    public Vector2 DirectionEndNormalized
        => Vector2.Normalize(Start - End);
    
    public Vector2 DirectionStart
        => End - Start;
    
    public Vector2 DirectionEnd
        => Start - End;
    
    public float Dot
        => Vector2.Dot(Start, End);
    
    public Vector2 Lerp(float position)
        => Vector2.Lerp(Start, End, position);

    public float RadiusStart
    {
        get
        {
            var x = DirectionStartNormalized.X;
            var y = DirectionStartNormalized.Y;
            return MathF.Sqrt(x * x + y * y);
        }
    }
    
    public float RadiusEnd
    {
        get
        {
            var x = DirectionEndNormalized.X;
            var y = DirectionEndNormalized.Y;
            return MathF.Sqrt(x * x + y * y);
        }
    }

    public Vector2 ClosestPoint(Vector2 point, bool clamped)
    {
        var normalStartOther = point - Start;
        var normalEndStart = End - Start;

        var magnitude = normalEndStart.LengthSquared();
        var product = Vector2.Dot(normalStartOther, normalEndStart);
        var distance = product / magnitude;

        return clamped 
            ? Lerp(Math.Clamp(distance, 0, 1))
            : Lerp(distance);
    }

    public bool IsIntersecting(LineF other)
    {
        var line1EndStart = End - Start; 
        var line2EndStart = other.End - other.Start;
        var line1StartLine2Start = other.Start - Start;
        
        var lerpValue1 = line1StartLine2Start.Cross(line2EndStart) / line1EndStart.Cross(line2EndStart);
        var lerpValue2 = line1StartLine2Start.Cross(line1EndStart) / line1EndStart.Cross(line2EndStart);
        return lerpValue1.Between01() && lerpValue2.Between01();
    }
    
    public bool IsIntersecting(ref LineF other, out Vector2 intersection)
    {
        return IsIntersecting(ref other, true, out intersection, out var _, out var _, out var _);
    }
    
    public bool IsIntersecting(ref LineF other, bool clamp, out Vector2 intersection1, out Vector2 intersection2, out float lerpValue1, out float lerpValue2)
    {
        var line1EndStart = End - Start; 
        var line2EndStart = other.End - other.Start;
        var line1StartLine2Start = other.Start - Start;
        
        lerpValue1 = line1StartLine2Start.Cross(line2EndStart) / line1EndStart.Cross(line2EndStart);
        lerpValue2 = line1StartLine2Start.Cross(line1EndStart) / line1EndStart.Cross(line2EndStart);
        var isIntersecting = lerpValue1.Between01() && lerpValue2.Between01();;

        if (clamp)
        {
            lerpValue1 = Math.Clamp(lerpValue1, 0, 1);
            lerpValue2 = Math.Clamp(lerpValue2, 0, 1); // prevent infinity
        }
        else
        {
            lerpValue1 = Math.Clamp(lerpValue1, float.MinValue, float.MaxValue);
            lerpValue2 = Math.Clamp(lerpValue2, float.MinValue, float.MaxValue); // prevent infinity
        }

        intersection1 = Lerp(lerpValue1);
        intersection2 = other.Lerp(lerpValue2);
        
        return isIntersecting;
    }

    public static LineF Zero = new ();
}